//
// NAntContrib
// Copyright (C) 2001-2003 Gerry Shaw
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
// Ian MacLean (ian@maclean.ms)

using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;

using NAnt.Core;
using NAnt.Core.Types;
using NAnt.Core.Tasks;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Registers an assembly for use from COM clients.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   Refer to the <see href="ms-help://MS.VSCC/MS.MSDNVS/cptools/html/cpgrfassemblyregistrationtoolregasmexe.htm">Regasm</see> 
    ///   documentation for more information on the regasm tool.
    ///   </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Register a single assembly.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <regasm assembly="myAssembly.dll" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Register an assembly while exporting a typelibrary.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <regasm assembly="myAssembly.dll" typelib="myAssembly.tlb" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Register a set of assemblies at once.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <regasm unregister="false" codebase="true">
    ///     <fileset>
    ///         <include name="**/*.dll" />
      ///       <exclude name="notanassembly.dll" />
    ///     </fileset>
    /// </regasm>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("regasm")]
    [ProgramLocation(LocationType.FrameworkDir)]
    public class RegasmTask : ExternalProgramBase {
        #region Private Instance Fields

        private FileInfo _assemblyFile;
        private FileInfo _regfile;
        private FileInfo _typelib;
        private bool _codebase;
        private bool _exporttypelib;
        private bool _unregister;
        private bool _registered;
        private FileSet _fileset = new FileSet();

        #endregion Private Instance Fields

        #region Public Instance Properties

       /// <summary>
       /// The name of the file to register. This is provided as an alternate 
       /// to using the task's fileset.
       /// </summary>
        [TaskAttribute("assembly")]
        public FileInfo AssemblyFile {
            get { return _assemblyFile; }
            set { _assemblyFile = value; }
        }

        /// <summary>
        /// Registry file to export to instead of entering the types directly 
        /// into the registry. If a fileset is used then the entries are all 
        /// collated into this file.
        /// </summary>
        [TaskAttribute("regfile")]
        public FileInfo RegistryFile {
            get { return _regfile; }
            set { _regfile = value; }
        }

        /// <summary>
        /// Set the code base registry setting.
        /// </summary>
        [TaskAttribute("codebase")]
        [BooleanValidator()]
        public bool CodeBase {
            get { return _codebase; }
            set { _codebase = value; }
        }

        /// <summary>
        /// Export a typelib and register it. The typelib will have the same 
        /// name as the source assembly unless the <see cref="TypeLib" /> 
        /// attribute is used.
        /// </summary>
        [TaskAttribute("exporttypelib")]
        [BooleanValidator()]
        public bool ExportTypelib {
            get { return _exporttypelib; }
            set { _exporttypelib = value; }
        }

        /// <summary>
        /// Only refer to already registered type libraries.
        /// </summary>
        [TaskAttribute("registered")]
        [BooleanValidator()]
        public bool Registered {
            get { return _registered; }
            set { _registered = value; }
        }
        
        /// <summary>
        /// Export the assembly to the specified type library and register it.
        /// This attribute is ignored when a fileset is specified.
        /// </summary>
        [TaskAttribute("typelib")]
        public FileInfo TypeLib {
            get { return _typelib; }
            set { _typelib = value; }
        }
        
        /// <summary>
        /// Unregister the assembly. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("unregister")]
        [BooleanValidator()]
        public bool Unregister {
            get { return _unregister; }
            set { _unregister = value; }
        }
               
        /// <summary>
        /// The set of files to register.
        /// </summary>
        [BuildElement("fileset")]
        public FileSet RegasmFileSet {
            get { return _fileset; }
            set { _fileset = value; }
        }

        #endregion Public Instance Properties

        public override string ProgramArguments {
            get {
                string args = "";

                if (Unregister) {
                    args += " /unregister ";
                }
                if (ExportTypelib && TypeLib != null) {
                    args += string.Format(CultureInfo.InvariantCulture,
                        " /tlb:\"{0}\"", TypeLib.FullName);
                }
                if (CodeBase) {
                    args += " /codebase";
                }
                if (RegistryFile != null) {
                    args += string.Format(CultureInfo.InvariantCulture,
                        " /regfile:\"{0}\"", RegistryFile.FullName);
                }
                if (Registered) {
                     args += " /registered";
                }
                if (Verbose) {
                     args += " /verbose";
                } else {
                     args += " /silent";
                }
                args += " /nologo";

                args += " \"" + AssemblyFile.FullName + "\"";
                return args;
            }
        }

        protected override void ExecuteTask() {
            if (AssemblyFile != null) {
                Log(Level.Info, "{0} '{1}' for COM Interop", 
                    Unregister ? "UnRegistering" : "Registering", 
                    AssemblyFile.FullName);

                if (ExportTypelib && TypeLib == null) {
                    TypeLib = new FileInfo(AssemblyFile.FullName.Replace(
                        Path.GetExtension(AssemblyFile.FullName), ".tlb"));
                }
                base.ExecuteTask();
            } else { // Loop thru fileset 
                // gather the information needed to perform the operation
                StringCollection fileNames = RegasmFileSet.FileNames;

                // display build log message
                Log(Level.Info, "{0} {1} files for COM interop", 
                    Unregister ? "UnRegistering" : "Registering", 
                    fileNames.Count);
                
                FileInfo registryFile = RegistryFile;

                // perform operation
                foreach (string path in fileNames) {
                    AssemblyFile = new FileInfo(path);
                    if (RegistryFile != null) {
                        RegistryFile = new FileInfo(path.Replace(
                            Path.GetExtension(path), ".reg"));
                    }
                    if (ExportTypelib) {
                        TypeLib = new FileInfo(path.Replace(
                            Path.GetExtension(path), ".tlb"));
                    }
                    Log(Level.Verbose, "{0} '{1}' for COM Interop", 
                        Unregister ? "UnRegistering" : "Registering", 
                        path);
                    base.ExecuteTask();
                }

                // Collate registry files
                if (registryFile != null) {
                    try {
                        using (StreamWriter writer = new StreamWriter(registryFile.FullName)) {
                            foreach (string path in fileNames) {
                                string regFile = path.Replace(Path.GetExtension(path), ".reg");
                                StreamReader reader = new StreamReader(regFile);
                                string data = reader.ReadToEnd();
                                writer.Write(data);
                                // close reader
                                reader.Close();
                                // clean up the registry file once its read
                                File.Delete(regFile); 
                            }
                        }
                    } catch (Exception ex) {
                        throw new BuildException("Error collating .reg files.", 
                            Location, ex);
                    }
                }
            }
        }
    }
}
