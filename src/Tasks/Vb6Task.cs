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

using System;
using System.Collections.Specialized; 
using System.Globalization;
using System.IO;
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
    ///     <para>Uses the VB6.EXE executable included with the Visual Basic 6 environment.</para>
    ///     <para>The compiler uses the settings and source files specified in the project or group file.</para>
    /// </remarks>
    /// <example>
    ///   <para>Build the project <c>HelloWorld.vbp</c> in the <c>build</c> directory.</para>
    ///   <code>
    ///     <![CDATA[
    /// <vb6 project="HelloWorld.vbp" outdir="build" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("vb6")]
    public class Vb6Task : ExternalProgramBase {
        #region Private Instance Fields

        private string _projectFile = null;
        private string _outdir = null;
        private string _programArguments = null;
        private string _errorFile = null;
        private bool _checkReferences = true;

        #endregion Private Instance Fields

        #region Public Instance Properties
        
        /// <summary>
        /// Output directory for the compilation target.  
        /// </summary>
        [TaskAttribute("outdir")]
        public string OutDir {
            get { return Project.GetFullPath(_outdir); }
            set { _outdir = StringUtils.ConvertEmptyToNull(value); }
        } 

        /// <summary>
        /// Visual Basic project or group file.
        /// </summary>
        [TaskAttribute("project", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string ProjectFile {
            get { return (_projectFile != null) ? Project.GetFullPath(_projectFile) : null; }
            set { _projectFile = StringUtils.ConvertEmptyToNull(value); }
        } 

        /// <summary>
        /// Determines whether project references are checked when deciding 
        /// whether the project needs to be recompiled.  The default is 
        /// <see langword="true" />.
        /// </summary>
        [BooleanValidator()]
        [TaskAttribute("checkreferences")]
        public bool CheckReferences {
            get { return _checkReferences; }
            set { _checkReferences = value; }
        }

        /// <summary>
        /// The file to which the Visual Basic compiler should log errors.
        /// </summary>
        [TaskAttribute("errorfile")]
        public string ErrorFile {
            get { return (_errorFile != null) ? Project.GetFullPath(_errorFile) : null; }
            set { _errorFile = StringUtils.ConvertEmptyToNull(value); }
        } 

        #endregion Public Instance Properties

        #region Override implementation of ExternalProgramBase

        /// <summary>
        /// Gets the filename of the external program to start.
        /// </summary>
        /// <value>
        /// The filename of the external program.
        /// </value>
        public override string ProgramFileName  {
            get { return Name; }
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
            Log(Level.Info, LogPrefix + "Building project {0}.", ProjectFile);
            if (NeedsCompiling()) {
                //Using a stringbuilder vs. StreamWriter since this program will 
                // not accept response files.
                StringBuilder writer = new StringBuilder();

                writer.AppendFormat(" /make \"{0}\"", ProjectFile);

                // make sure the output directory exists
                if (!Directory.Exists(OutDir)) {
                    Directory.CreateDirectory(OutDir);
                }

                writer.AppendFormat(" /outdir \"{0}\"", OutDir);

                if (ErrorFile != null) {
                    writer.AppendFormat(" /out \"{0}\"", ErrorFile);
                }

                _programArguments = writer.ToString();

                // call base class to do the work
                base.ExecuteTask();
            }
        }

        #endregion Override implementation of ExternalProgramBase

        #region Protected Instance Methods

        protected virtual bool NeedsCompiling() {
            // return true as soon as we know we need to compile

            if (string.Compare(Path.GetExtension(ProjectFile), ".VBG", true, CultureInfo.InvariantCulture) == 0) {
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
                return ProjectNeedsCompiling(ProjectFile);
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
        protected StringCollection ParseGroupFile(string groupFile) {
            StringCollection projectFiles = new StringCollection();

            if (!File.Exists(Project.GetFullPath(groupFile))) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture, 
                    "Visual Basic group file '{0}' does not exist.", groupFile),
                    Location);
            }

            string fileLine = null;
        
            // Regexp that extracts INI-style "key=value" entries used in the VBP
            Regex keyValueRegEx = new Regex(@"(?<key>\w+)\s*=\s*(?<value>.*)\s*$");

            string key = String.Empty;
            string keyValue = String.Empty;
            
            Match match = null;
            using (StreamReader reader = new StreamReader(Project.GetFullPath(groupFile), Encoding.ASCII)) {
                while ((fileLine = reader.ReadLine()) != null) {
                    match = keyValueRegEx.Match(fileLine);
                    if (match.Success) {
                        key = match.Groups["key"].Value;
                        keyValue = match.Groups["value"].Value;

                        if (key == "StartupProject" || key == "Project") {
                            // This is a project file - get the file name and 
                            // add it to the project list
                            projectFiles.Add(keyValue);
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
        /// <returns>true if the project should be compiled, false otherwise</returns>
        protected bool ProjectNeedsCompiling(string projectFile) {
            // return true as soon as we know we need to compile
        
            FileSet sources = new FileSet();
            sources.BaseDirectory = BaseDirectory;
            
            FileSet references = new FileSet();
            references.BaseDirectory = BaseDirectory;

            string outputFile = ParseProjectFile(projectFile, sources, references);

            FileInfo outputFileInfo = new FileInfo(OutDir != null ? Path.Combine(OutDir, outputFile) : outputFile) ;
            if (!outputFileInfo.Exists) {
                Log(Level.Info, LogPrefix + "Output file {0} does not exist, recompiling.", outputFileInfo.FullName);
                return true;
            }

            string fileName = FileSet.FindMoreRecentLastWriteTime(outputFileInfo.FullName, outputFileInfo.LastWriteTime);
            if (fileName != null) {
                Log(Level.Info, LogPrefix + "{0} is out of date, recompiling.", fileName);
                return true;
            }

            fileName = FileSet.FindMoreRecentLastWriteTime(sources.FileNames, outputFileInfo.LastWriteTime);
            if (fileName != null) {
                Log(Level.Info, LogPrefix + "{0} is out of date, recompiling.", fileName);
                return true;
            }

            if (CheckReferences) {
                fileName = FileSet.FindMoreRecentLastWriteTime(references.FileNames, outputFileInfo.LastWriteTime);
                if (fileName != null) {
                    Log(Level.Info, LogPrefix + "{0} is out of date, recompiling.", fileName);
                    return true;
                }
            }

            return false;
        }

        #endregion Protected Instance Methods

        #region Private Instance Methods

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
            Regex referenceRegEx = new Regex(@"Reference\s*=\s*\*\\G{(?<tlbguid>[0-9\-A-Fa-f]*($^\.)*)}\#(?<majorver>[0-9\.0-9($^\.)*]*)\#(?<minorver>[0-9]($^\.)*)\#(?<tlbname>.*)\#");

            string key = String.Empty;
            string keyValue = String.Empty;
            
            Match match = null;
            using (StreamReader reader = new StreamReader(Project.GetFullPath(projectFile), Encoding.ASCII)) {
                while ((fileLine = reader.ReadLine()) != null) {
                    match = keyValueRegEx.Match(fileLine);
                    if (match.Success) {
                        key = match.Groups["key"].Value;
                        keyValue = match.Groups["value"].Value;

                        if (key == "Class" || key == "Module") {
                            // This is a class or module source file - extract the file name and add it to the sources fileset
                            // The entry is of the form "Class=ClassName;ClassFile.cls"
                            match = codeRegEx.Match(fileLine);
                            if (match.Success) {
                                sources.Includes.Add(match.Groups["filename"].Value);
                            }
                        }
                        else if (key == "Form" || key == "UserControl" || key == "PropertyPage") {
                            // This is a form, control, or property page source file - add the file name to the sources fileset
                            // The entry is of the form "Form=Form1.frm"
                            sources.Includes.Add(keyValue);
                        }
                        else if (key == "Reference") {
                            // This is a source file - extract the reference name and add it to the references fileset
                            match = referenceRegEx.Match(fileLine);
                            if (match.Success) {
                                string tlbName = match.Groups["tlbname"].Value;
                                if (File.Exists(tlbName)) {
                                    references.Includes.Add(tlbName);
                                }
                                else {
                                    //the tlb filename embedded in the VBP file is just
                                    //a hint about where to look for it. If the file isn't
                                    //at that location, the typelib ID is used to lookup
                                    //the file name
                                            
                                    // # Added to properly cast the parts of the version #
                                    // Ensure that we use the correct cast option
                                    string temp = match.Groups["majorver"].Value;
                                    ushort majorVer = (ushort) double.Parse(temp, CultureInfo.InvariantCulture);
                                    
                                    temp = match.Groups["minorver"].Value;
                                    ushort minorVer = (ushort) double.Parse(temp, CultureInfo.InvariantCulture);
                                    temp = match.Groups["lcid"].Value;
                                    uint lcid = 0;
                                    
                                    if (0 < temp.Length) {
                                        lcid = (uint) double.Parse(temp, CultureInfo.InvariantCulture);
                                    }
                                    
                                    string tlbGuid = match.Groups["tlbguid"].Value;
                                    Guid guid = new Guid(tlbGuid);
                                    try {
                                        QueryPathOfRegTypeLib(ref guid, majorVer, minorVer, lcid, out tlbName);
                                        if (File.Exists(tlbName)) {
                                            references.Includes.Add(tlbName);
                                        }
                                    } catch (COMException) {
                                        //Typelib wasn't found - vb6 will barf
                                        //when the compile happens, but we won't worry about it.
                                    }
                                }
                            }
                        } else if (key == "ExeName32") {
                            // Store away the built file name so that we can check against it later
                            // If the project was never built in the IDE, or the project file wasn't saved
                            // after the build occurred, this setting won't exist. In that case, VB uses the
                            // ProjectName as the DLL/EXE name
                            outputFile = keyValue.Trim('"');
                        } else if (key == "Type") {
                            // Store away the project type - we may need it to construct the built
                            // file name if ExeName32 doesn't exist
                            projectType = keyValue;
                        } else if (key == "Name") {
                            // Store away the project name - we may need it to construct the built
                            // file name if ExeName32 doesn't exist
                            projectName = keyValue.Trim('"');
                        }
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

        [DllImport("oleaut32.dll", PreserveSig=false)]
        private static extern void QueryPathOfRegTypeLib(
            ref Guid guid, 
            ushort majorVer, 
            ushort minorVer, 
            uint lcid, 
            [MarshalAs(UnmanagedType.BStr)] out string path
            );

        #endregion Private Static Methods
    }
}
