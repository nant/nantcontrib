//
// NAntContrib
// Copyright (C) 2001-2002 Gerry Shaw
//
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307 USA
//

// Aaron A. Anderson (aaron@skypoint.com | aaron.anderson@farmcreditbank.com)
// Kevin Dente (kevin_d@mindspring.com)
// Hani Atassi (haniatassi@users.sourceforge.com)

using Microsoft.Win32;
using System;
using System.Collections.Specialized; 
using System.Globalization;
using System.IO;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;
using NAnt.Core.Types;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Compiles Microsoft Visual Basic 6 programs.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   Uses the VB6.EXE executable included with the Visual Basic 6
    ///   environment.
    ///   </para>
    ///   <para>
    ///   The compiler uses the settings and source files specified in the 
    ///   project or group file.
    ///   </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Build the project <c>HelloWorld.vbp</c> in the <c>build</c> directory.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <vb6 project="HelloWorld.vbp" outdir="build" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Check compiled property "vb6.compiled"
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <echo message="Compiled sucessfully" if="${vb6.compiled}" />
    /// <echo message="Compilation not needed" if="${not vb6.compiled}" />
    ///     ]]>
    ///   </code>    
    /// </example>
    [TaskName("vb6")]
    public class Vb6Task : ExternalProgramBase {
        #region Private Instance Fields

        private FileInfo _projectFile;
        private DirectoryInfo _outDir;
        private string _programArguments = null;
        private FileInfo _errorFile;
        private bool _checkReferences = true;
        private string _conditionals = null;
        private string _compiledProperty = "vb6.compiled";

        #endregion Private Instance Fields

        #region Public Instance Properties
        
        /// <summary>
        /// Output directory for the compilation target.
        /// </summary>
        [TaskAttribute("outdir")]
        public DirectoryInfo OutDir {
            get {
                if (_outDir == null) {
                    return new DirectoryInfo(Project.BaseDirectory);
                }
                return _outDir;
            }
            set { _outDir = value; }
        }

        /// <summary>
        /// Visual Basic project or group file.
        /// </summary>
        [TaskAttribute("project", Required=true)]
        public FileInfo ProjectFile {
            get { return _projectFile; }
            set { _projectFile = value; }
        }

        /// <summary>
        /// Determines whether project references are checked when deciding 
        /// whether the project needs to be recompiled. The default is 
        /// <see langword="true" />.
        /// </summary>
        [TaskAttribute("checkreferences")]
        [BooleanValidator()]
        public bool CheckReferences {
            get { return _checkReferences; }
            set { _checkReferences = value; }
        }

        /// <summary>
        /// The file to which the Visual Basic compiler should log errors.
        /// </summary>
        [TaskAttribute("errorfile")]
        public FileInfo ErrorFile {
            get { return _errorFile; }
            set { _errorFile = value; }
        }

        /// <summary>
        /// Tells Visual Basic which values to use for conditional compilation
        /// constants.
        /// </summary>
        [TaskAttribute("conditionals")]
        public string Conditionals {
            get { return _conditionals; }
            set { _conditionals = value; }
        }

        /// <summary>
        /// The name of a property in which will be set to <see langword="true" /> 
        /// if compilation was needed and done without errors (default: "vb6.compiled")
        /// This can be used for touching the compilation files if
        /// vb6 autoincrement is set to true to avoid recompilation without any 
        /// other changes.
        /// </summary>
        [TaskAttribute("compiledproperty")]
        [StringValidator(AllowEmpty=false)]
        public string CompiledProperty {
            get { return _compiledProperty; }
            set { _compiledProperty = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of ExternalProgramBase

        /// <summary>
        /// Gets the filename of the external program to start.
        /// </summary>
        /// <value>
        /// The filename of the external program.
        /// </value>
        public override string ProgramFileName {
            get {
                // first check to see if VB6.EXE is available in one of the 
                // directories in the PATH environment variable
                PathScanner scanner = new PathScanner();
                // check for VB6.EXE
                scanner.Add("VB6.EXE");
                // get results
                StringCollection results = scanner.Scan();
                // check if we found a match
                if (results.Count > 0) {
                    return results[0];
                } else {
                    try {
                        // check registry for VB6 install dir
                        const string x86Vb6RegistryPath = @"SOFTWARE\Microsoft\VisualStudio\6.0\Setup\Microsoft Visual Basic";
                        const string x64Vb6RegistryPath = @"SOFTWARE\Wow6432Node\Microsoft\VisualStudio\6.0\Setup\Microsoft Visual Basic";
                        RegistryKey vbKey = Registry.LocalMachine.OpenSubKey(x86Vb6RegistryPath) ?? Registry.LocalMachine.OpenSubKey(x64Vb6RegistryPath);
                        if (vbKey != null) {
                            string productDir = vbKey.GetValue("ProductDir") as string;
                            if (productDir != null) {
                                string vb6Exe = Path.Combine(productDir, "VB6.EXE");
                                if (File.Exists(vb6Exe)) {
                                    return vb6Exe;
                                }
                            }
                        }
                    } catch (SecurityException) {
                    }
                }
                
                // if VB6.exe is not available on PATH, and registry could not
                // be found or access then just have ExternalProgramBase use
                // "vb6" as program file name and deal with error
                return Name; 
            }
        }

        /// <summary>
        /// Gets the command-line arguments for the external program.
        /// </summary>
        /// <value>
        /// The command-line arguments for the external program.
        /// </value>
        public override string ProgramArguments {
            get { return _programArguments; }
        }

        /// <summary>
        /// Compiles the Visual Basic project or project group.
        /// </summary>
        protected override void ExecuteTask() { 
            Log(Level.Info, "Building project '{0}'.", ProjectFile.FullName);
            if (CompiledProperty != null) {
                Properties[CompiledProperty] = "false";
            }

            if (NeedsCompiling()) {
                //Using a stringbuilder vs. StreamWriter since this program will 
                // not accept response files.
                StringBuilder writer = new StringBuilder();

                writer.AppendFormat(" /make \"{0}\"", ProjectFile.FullName);

                // make sure the output directory exists
                if (!OutDir.Exists) {
                    OutDir.Create();
                }

                if (Conditionals != null) {
                    writer.AppendFormat(" /d {0}", _conditionals);
                }

                writer.AppendFormat(" /outdir \"{0}\"", OutDir.FullName);

                if (ErrorFile != null) {
                    writer.AppendFormat(" /out \"{0}\"", ErrorFile.FullName);
                }

                _programArguments = writer.ToString();

                // call base class to do the work
                base.ExecuteTask();
                if (CompiledProperty != null) {
                    Properties[CompiledProperty] = "true";
                }
            }
        }

        #endregion Override implementation of ExternalProgramBase

        #region Protected Instance Methods

        protected virtual bool NeedsCompiling() {
            // return true as soon as we know we need to compile

            if (string.Compare(ProjectFile.Extension, ".VBG", true, CultureInfo.InvariantCulture) == 0) {
                // The project file is a Visual Basic group file (VBG).
                // We need to check each subproject in the group
                StringCollection projectFiles = ParseGroupFile(ProjectFile);
                foreach (string projectFile in projectFiles) {
                    if (ProjectNeedsCompiling(projectFile)) {
                        return true;
                    }
                }
            } else {
                // The project file is a Visual Basic project file (VBP)
                return ProjectNeedsCompiling(ProjectFile.FullName);
            }

            return false;
        }

        /// <summary>
        /// Parses a VB group file and extract the file names of the sub-projects 
        /// in the group.
        /// </summary>
        /// <param name="groupFile">The file name of the group file.</param>
        /// <returns>
        /// A string collection containing the list of sub-projects in the group.
        /// </returns>
        protected StringCollection ParseGroupFile(FileInfo groupFile) {
            StringCollection projectFiles = new StringCollection();

            if (!groupFile.Exists) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture, 
                    "Visual Basic group file '{0}' does not exist.", 
                    groupFile.FullName), Location);
            }

            string fileLine = null;
        
            // Regexp that extracts INI-style "key=value" entries used in the VBP
            Regex keyValueRegEx = new Regex(@"(?<key>\w+)\s*=\s*(?<value>.*)\s*$");

            string key = string.Empty;
            string keyValue = string.Empty;
            
            Match match = null;
            using (StreamReader reader = new StreamReader(groupFile.FullName, Encoding.ASCII)) {
                while ((fileLine = reader.ReadLine()) != null) {
                    match = keyValueRegEx.Match(fileLine);
                    if (match.Success) {
                        key = match.Groups["key"].Value;
                        keyValue = match.Groups["value"].Value;

                        if (key == "StartupProject" || key == "Project") {
                            // This is a project file - get the file name and 
                            // add it to the project list
                            projectFiles.Add(Path.Combine(groupFile.DirectoryName, keyValue));
                        }
                    }
                }

                // close the reader
                reader.Close();
            }
            
            return projectFiles;
        }

        /// <summary>
        /// Determines if a VB project needs to be recompiled by comparing the timestamp of 
        /// the project's files and references to the timestamp of the last built version.
        /// </summary>
        /// <param name="projectFile">The file name of the project file.</param>
        /// <returns>
        /// <see langword="true" /> if the project should be compiled; otherwise,
        /// <see langword="false" />.
        /// </returns>
        protected bool ProjectNeedsCompiling(string projectFile) {
            // return true as soon as we know we need to compile
            FileSet sources = new FileSet();
            FileSet references = new FileSet();
            
            string basedir = Path.GetDirectoryName(projectFile);
            if (basedir != "" ) {
                sources.BaseDirectory = new DirectoryInfo(Path.GetDirectoryName( projectFile ) );
                references.BaseDirectory = sources.BaseDirectory;
            }
                     
            string outputFile = ParseProjectFile(projectFile, sources, references);

            FileInfo outputFileInfo = new FileInfo(OutDir != null ? Path.Combine(OutDir.FullName, outputFile) : outputFile);
            if (!outputFileInfo.Exists) {
                Log(Level.Info, "Output file '{0}' does not exist, recompiling.", 
                    outputFileInfo.FullName);
                return true;
            }
            // look for a changed project file.
            string fileName = FileSet.FindMoreRecentLastWriteTime( projectFile, outputFileInfo.LastWriteTime);
            if (fileName != null) {
                Log(Level.Info, "{0} is out of date, recompiling.", fileName);
                return true;
            }
            // check for a changed source file
            fileName = FileSet.FindMoreRecentLastWriteTime(sources.FileNames, outputFileInfo.LastWriteTime);
            if (fileName != null) {
                Log(Level.Info, "{0} is out of date, recompiling.", fileName);
                return true;
            }
            // check for a changed reference 
            if (CheckReferences) {
                fileName = FileSet.FindMoreRecentLastWriteTime(references.FileNames, outputFileInfo.LastWriteTime);
                if (fileName != null) {
                    Log(Level.Info, "{0} is out of date, recompiling.", fileName);
                    return true;
                }
            }
            return false;
        }

        #endregion Protected Instance Methods

        #region Private Instance Methods

        /// <summary>
        /// VB6 uses a special algorithm to search for the typelib file. It doesn't 
        /// rely on the API function QueryPathOfRegTypeLib, because VB could use a newer
        /// version of the TLB.
        /// 
        /// The algorithm used by VB is not perfect and has some flaws, which you could
        /// get a newer version even if your requested version is installed. This is because
        /// the algorithm iterates the registry beneath the Guid - entry by entry - from the 
        /// beginning and returns the first TLB version that is higher or equal to the 
        /// requested version.
        /// 
        /// pseudo code:
        /// 1. open the key HKEY_CLASSES_ROOT\TypeLib\{Guid}
        /// 2. If the key exists:
        ///     3. Foreach version under the key that has the requested culture entry:
        ///         4. If the version higher or equal to the requested version:
        ///             5. Get the TLB filename and returns it
        /// </summary>
        /// <param name="guid">The guid of the tlb to look for</param>
        /// <param name="major">The major version number of the tlb</param>
        /// <param name="minor16">The minor version number of the tlb. If you parse minor from a string, treat the string as hex value.</param>
        /// <param name="lcid">The culture id</param>
        /// <returns>null if couldn't find a match, otherwise it returns the file.</returns>
        private string VB6GetTypeLibFile(Guid guid, ushort major, ushort minor16, uint lcid) {
            string tlbFile = null;

            Microsoft.Win32.RegistryKey regKey;
            regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(string.Format("TypeLib\\{{{0}}}", guid));
            if (regKey != null) {
                foreach (string ver in regKey.GetSubKeyNames()) {
                    Microsoft.Win32.RegistryKey regKeyCulture = regKey.OpenSubKey(string.Format("{0}\\{1}", ver, lcid));
                    if (regKeyCulture == null)
                        continue;

                    ushort tmpMajor = 0;
                    ushort tmpMinor16 = 0;
                    string [] parts = ver.Split('.');
                    if (parts.Length > 0) {
                        tmpMajor = (ushort) Convert.ToUInt16(parts[0], 16);
                        if (parts.Length > 1) {
                            tmpMinor16 = Convert.ToUInt16(parts[1], 16);  // Treat minor as hex
                        }
                    }       

                    if (major < tmpMajor  || (major == tmpMajor && minor16 <= tmpMinor16)) {
                        // Found it..
                        Microsoft.Win32.RegistryKey regKeyWin32 = regKeyCulture.OpenSubKey("win32");
                        if (regKeyWin32 != null) {
                            tlbFile = (string)regKeyWin32.GetValue("");
                            regKeyWin32.Close();
                            break;
                        }
                    }
                }
                regKey.Close();
            }
            
            if (tlbFile != null ) {
                int lastBackslash = tlbFile.LastIndexOf(@"\");
                int lastDot = tlbFile.LastIndexOf('.');
                if (lastBackslash > lastDot) {
                    return tlbFile.Substring(0, lastBackslash);
                }
            }

            return tlbFile;
        }

        /// <summary>
        /// Parses a VB project file and extracts the source files, reference files, and 
        /// the name of the compiled file for the project.
        /// </summary>
        /// <param name="projectFile">The filename of the project file.</param>
        /// <param name="sources">
        /// A fileset representing the source files of the project, which will
        /// populated by the method.
        /// </param>
        /// <param name="references">
        /// A fileset representing the references of the project, which will
        /// populated by the method.
        /// </param>
        /// <returns>A string containing the output file name for the project.</returns>
        private string ParseProjectFile(string projectFile, FileSet sources, FileSet references) {
            if (!File.Exists(Project.GetFullPath(projectFile))) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture, 
                    "Visual Basic project file '{0}' does not exist.", projectFile),
                    Location);
            }

            string outputFile = null;
            string fileLine = null;
            string projectName = null;
            string projectType = null;

            //# Modified each regular expressioni to properly parse the project references in the vbp file #
            // Regexp that extracts INI-style "key=value" entries used in the VBP
            Regex keyValueRegEx = new Regex(@"(?<key>\w+)\s*=\s*(?<value>.*($^\.)*)\s*$");

            // Regexp that extracts source file entries from the VBP (Class=,Module=,Form=,UserControl=)
            Regex codeRegEx = new Regex(@"(Class|Module)\s*=\s*\w*;\s*(?<filename>.*($^\.)*)\s*$");

            // Regexp that extracts reference entries from the VBP (Reference=)
            Regex referenceRegEx = new Regex(@"(Object|Reference)\s*=\s*({|\*\\G{)(?<tlbguid>[0-9\-A-Fa-f]*($^\.)*)}\#(?<majorver>[0-9a-fA-F($^\.)]*)\.(?<minorver>[0-9a-fA-F($^\.)]*)\#(?<lcid>[0-9]($^\.)*)(;|\#)(?<tlbname>[^\#\n\r]*)");
            
            string key = String.Empty;
            string keyValue = String.Empty;
            
            Match match = null;
            using (StreamReader reader = new StreamReader(Project.GetFullPath(projectFile), Encoding.ASCII)) {
                while ((fileLine = reader.ReadLine()) != null) {
                    match = keyValueRegEx.Match(fileLine);
                    if (!match.Success) {
                        continue;
                    }

                    key = match.Groups["key"].Value;
                    keyValue = match.Groups["value"].Value;

                    switch (key) {
                        case "Class":
                        case "Module":
                            // This is a class or module source file - extract the file name and add it to the sources fileset
                            // The entry is of the form "Class=ClassName;ClassFile.cls"
                            match = codeRegEx.Match(fileLine);
                            if (match.Success) {
                                sources.Includes.Add(match.Groups["filename"].Value);
                            }
                            break;
                        case "Designer":
                        case "Form":
                        case "UserControl":
                        case "PropertyPage":
                        case "ResFile32":
                            // This is a form, control, or property page source file - add the file name to the sources fileset
                            // The entry is of the form "Form=Form1.frm"
                            sources.Includes.Add(keyValue.Trim('"'));
                            break;
                        case "Object":
                        case "Reference":
                            // This is a source file - extract the reference name and add it to the references fileset
                            match = referenceRegEx.Match(fileLine);
                            if (!match.Success) {
                                break;
                            }

                            string tlbName = match.Groups["tlbname"].Value;
                            if (File.Exists(tlbName)) {
                                references.Includes.Add(tlbName);
                            } else {
                                // the tlb filename embedded in the VBP file is just
                                // a hint about where to look for it. If the file isn't
                                // at that location, the typelib ID is used to lookup
                                // the file name

                                string temp = match.Groups["majorver"].Value;
                                ushort majorVer = 0;
                                if (key == "Object") {
                                    // for OCX's major is a decimal value
                                    majorVer = ushort.Parse(temp, CultureInfo.InvariantCulture);
                                } else {
                                    // for dll's major is a hex value
                                    majorVer = (ushort) Convert.ToUInt16(temp, 16);
                                }
                            
                                // minor is considered a hex value
                                temp = match.Groups["minorver"].Value;
                                ushort minorVer16 = Convert.ToUInt16(temp, 16);

                                temp = match.Groups["lcid"].Value;
                                uint lcid = 0;
                            
                                if (temp.Length != 0) {
                                    lcid = (uint) double.Parse(temp, CultureInfo.InvariantCulture);
                                }
                            
                                string tlbGuid = match.Groups["tlbguid"].Value;
                                Guid guid = new Guid(tlbGuid);

                                // find the type library file 
                                tlbName = VB6GetTypeLibFile(guid, majorVer, minorVer16, lcid);
                                if (tlbName == null) {
                                    Log(Level.Warning, "Type library '{0}' version {1}.{2:x} could not be found.", 
                                        guid, match.Groups["majorver"].Value, match.Groups["minorver"].Value);
                                } else {
                                    if (File.Exists(tlbName)) {
                                        references.Includes.Add(tlbName);
                                    } else {
                                        Log(Level.Warning, "Type library file '{0}' does not exist.", tlbName);
                                    }
                                }
                            }
                            break;
                        case "ExeName32":
                            // Store away the built file name so that we can check against it later
                            // If the project was never built in the IDE, or the project file wasn't saved
                            // after the build occurred, this setting won't exist. In that case, VB uses the
                            // ProjectName as the DLL/EXE name
                            outputFile = keyValue.Trim('"');
                            break;
                        case "Type":
                            // Store away the project type - we may need it to construct the built
                            // file name if ExeName32 doesn't exist
                            projectType = keyValue;
                            break;
                        case "Name":
                            // Store away the project name - we may need it to construct the built
                            // file name if ExeName32 doesn't exist
                            projectName = keyValue.Trim('"');
                            break;
                    }
                }
                reader.Close();
            }

            if (outputFile == null) {
                // The output file name wasn't specified in the project file, so
                // We need to figure out the output file name from the project name and type
                if (projectType == "Exe" || projectType == "OleExe") {
                    outputFile = Path.ChangeExtension(projectName, ".exe");
                } else if (projectType == "OleDll") {
                    outputFile = Path.ChangeExtension(projectName, ".dll");
                } else if (projectType == "Control") {
                    outputFile = Path.ChangeExtension(projectName, ".ocx");
                }
            }

            return outputFile;
        }

        #endregion Protected Instance Methods

        #region Private Static Methods

        #endregion Private Static Methods
    }
}
