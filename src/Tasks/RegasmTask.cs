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

// Ian MacLean (ian@maclean.ms)

using System;
using System.IO;
using System.Collections.Specialized;

using NAnt.Core;
using NAnt.Core.Types;
using NAnt.Core.Tasks;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks {
    /// <summary>Register an assembly for  use from COM clients.</summary>
    /// <remarks>
    ///   <para>Refer to the <a href="ms-help://MS.VSCC/MS.MSDNVS/cptools/html/cpgrfassemblyregistrationtoolregasmexe.htm">Regasm</a> for  more information on the regasm tool</para>
    /// </remarks>
    /// <example>
    ///   <para>Register a single assembly.</para>
    ///   <code><![CDATA[<regasm file="myAssembly.dll"/>]]></code>
    ///   <para>Register an assembly while exporting a typelibrary </para>
    ///   <code><![CDATA[<regasm file="myAssembly.dll" typelib="myAssembly.tlb"/>]]></code>
    ///   <para>Register a set of assemblies at once.</para>
    ///   <code>
    /// <![CDATA[
    /// <regasm unregister="false" codebase="true" >
    ///     <fileset>
    ///         <includes name="**/*.dll"/>
      ///         <excludes name="notanassembly.dll"/>
    ///     </fileset>
    /// </regasm>
    /// ]]>
    ///   </code>
    /// </example>
    [TaskName("regasm")]
    public class RegasmTask : ExternalProgramBase {

        string _assemblyName = null;
        string _regfile = null;
        string _typelib = null;
        bool _codebase = false;
        bool _silent = false;
        bool _exporttypelib = false;
        bool _unregister = false;
        bool _registered = false;        
        FileSet _fileset = new FileSet();  

       /// <summary>The name of the file to register. This is provided as an alternate to using the task's fileset.</summary>
        [TaskAttribute("assembly")]
        public string AssemblyName {
            get { return _assemblyName; }
            set { _assemblyName = value; } 
        }      

        /// <summary>Registry file to export to instead of entering the types directly into the registry. If a fileset is used then the entries are all collated into this file.</summary>
        [TaskAttribute("regfile")]        
        public string RegistryFile {
            get { return _regfile; }
            set { _regfile = value; }
        }

        /// <summary>Set the code base registry setting. </summary>
        [TaskAttribute("codebase")]       
        [BooleanValidator()]
        public bool CodeBase {
            get { return _codebase; }
            set { _codebase = value; }
        }

        /// <summary>Silent mode. Prevents displaying of success messages. Default is "false".</summary>
        [TaskAttribute("silent")]
        public bool Silent {
            get { return _silent; }
            set { _silent = value; }
        }

        /// <summary>Export a typelib and register it. The typelib will have the same name as the source assembly unless the "typelib" attribute is used.</summary>
        [TaskAttribute("exporttypelib")]       
        [BooleanValidator()]
        public bool ExportTypelib {
            get { return _exporttypelib; }
            set { _exporttypelib = value; }
        }

        /// <summary>Only refer to already registered type libraries. </summary>
        [TaskAttribute("registered")]       
        [BooleanValidator()]
        public bool Registered {
            get { return _registered; }
            set { _registered = value; }
        }
        
        /// <summary>Export the assembly to the specified type library and register it.( ignored when a fileset is specified )</summary>
        [TaskAttribute("typelib")]       
        public string TypeLib {
            get { return _typelib; }
            set { _typelib = value; }
        }
        
        /// <summary>Unregistering this time. ( /u paramater )Default is "false".</summary>
        [TaskAttribute("unregister")]
        [BooleanValidator()]
        public bool Unregister {
            get { return _unregister; }
            set { _unregister = value; }
        }
               
        /// <summary>the set of files to register..</summary>
        [FileSet("fileset")]
        public FileSet RegasmFileSet { 
            get { return _fileset; } 
            set { _fileset = value; }
        }
        
        public override string ProgramFileName {
            get {return Name;}
        }

        public override string ProgramArguments {
            get {
                string args = "";
                string assemblyName = AssemblyName;
                if (Unregister) {                   
                    args += " /unregister ";
                }                
                if ( ExportTypelib && TypeLib != null ) {
                    args += " /tlb:" + TypeLib;
                }
                if ( CodeBase ) {
                    args += " /codebase";
                }
                if ( RegistryFile != null ) {
                    args += " /regfile:" + RegistryFile;
                }
                if ( Registered ){
                     args += " /registered";
                }
                if ( Verbose ){
                     args += " /verbose";
                }
                if ( Silent ){
                     args += " /silent";
                }
                args += " /nologo";

                args += " \"" + assemblyName +"\"";
                return args;
            }
        }

        protected override void ExecuteTask() {
            // If the user wants to see the actual command the -verbose flag
            // will cause ExternalProgramBase to display the actual call.
            if ( AssemblyName != null ) {
                Log(Level.Info, LogPrefix + "{0} {1} for COM Interop", Unregister ? "UnRegistering" : "Registering", AssemblyName);
                if ( ExportTypelib && TypeLib == null ) {
                    TypeLib = AssemblyName.Replace(Path.GetExtension(AssemblyName), ".tlb");                        
                }
                base.ExecuteTask();
            }
            // Lop thru fileset 
            else {               
                                                              
                // gather the information needed to perform the operation
                StringCollection fileNames = RegasmFileSet.FileNames;

                // display build log message
                Log(Level.Info, LogPrefix + "Registering {0} files", fileNames.Count);
                
                string registryFile = RegistryFile;

                // perform operation
                foreach (string path in fileNames) {
                    AssemblyName = path;
                    if ( RegistryFile != null ) {
                        string regFile = path.Replace( Path.GetExtension(path), ".reg");
                        RegistryFile = regFile;
                    }
                    if ( ExportTypelib ) {
                        string typelibFile = path.Replace(Path.GetExtension(path), ".tlb");
                        TypeLib = typelibFile;
                    }
                    // Ignore certain flags if in multifile mode..                  
                    Log(Level.Info, LogPrefix + "{0} {1} for COM Interop", Unregister ? "UnRegistering" : "Registering", Path.GetFileName(path));
                    base.ExecuteTask();
                }

                // Collate registry files
                if ( registryFile != null ) {
                    // get full path relative to ..
                    string fullRegfilePath = Project.GetFullPath( registryFile );
                    System.IO.StreamWriter writer = new StreamWriter( registryFile );
                    try {                        
                        foreach (string path in fileNames) {
                            string regFile = path.Replace( Path.GetExtension(path), ".reg");
                            StreamReader reader = new StreamReader( regFile );
                            string data = reader.ReadToEnd();
                            writer.Write(data);

                            reader.Close();
                            File.Delete(regFile); // clean up the registry file once its read
                        }
                        writer.Close();
                    }
                    catch ( Exception e ){
                        throw new BuildException("Error collating .reg files " + e.Message, Location );
                    }
                    finally{
                        if ( writer != null ) {
                            writer.Close();    
                        }
                    }
                }
            }
        }
    }
}
