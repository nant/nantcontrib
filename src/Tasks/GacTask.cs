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

// Bill Baker (bill.baker@epigraph.com)
// Gerry Shaw (gerry_shaw@yahoo.com)

using System;
using System.IO;

using SourceForge.NAnt;
using SourceForge.NAnt.Tasks;
using SourceForge.NAnt.Attributes;

namespace NAnt.Contrib.Tasks {


    /// <summary>Manipulates the contents of the global assembly cache.</summary>
    /// <remarks>
    ///   <para>This tasks provides some of the same functionality as the gacutil tool provided in the .NET SDK.</para>
    ///   <para>Specifically, the gac task allows you to install assemblies into the cache and remove them from the cache.</para>
    ///   <para>Refer to the <a href="ms-help://MS.NETFrameworkSDK/cptools/html/cpgrfglobalassemblycacheutilitygacutilexe.htm">Global Assembly Cache Tool (Gacutil.exe)</a> for more information.</para>
    /// </remarks>
    /// <example>
    ///   <para>Inserts the file mydll.dll into the global assembly cache.</para>
    ///   <code>&lt;gac assembly=mydll.dll"/&gt;</code>
    ///   <para>Removes the assembly hello from the global assembly cache and the native image cache.</para>
    ///   <code>&lt;gac assembly="hello" uninstall="true"/&gt;</code>
    ///   <para>Note that the previous command might remove more than one assembly from the assembly cache because the assembly name is not fully specified. For example, if both version 1.0.0.0 and 3.2.2.1 of hello are installed in the cache, the command gacutil /u hello removes both of the assemblies.</para>
    ///   <para>Use the following example to avoid removing more than one assembly. This command removes only the hello assembly that matches the fully specified version number, culture, and public key.</para>
    ///   <code>&lt;gac assembly='hello,Version=1.0.0.1,Culture="de",PublicKeyToken=45e343aae32233ca' uninstall="true"/&gt;</code>
    /// </example>
    [TaskName("gac")]
    public class GacTask : ExternalProgramBase {
        public enum ActionTypes {
            install,
            overwrite,
            uninstall };

        ActionTypes _action = ActionTypes.install;
        string _assemblyName = null;
        int _timeout = Int32.MaxValue;
        FileSet _assemblies = new FileSet();
        bool _silent = false;

        /// <summary>The name of a file that contains an assembly manifest.</summary>
        [TaskAttribute("assembly")]
        public string AssemblyName {
            get { return _assemblyName; }
            set { _assemblyName = value; }
        }

        /// <summary>Defines the action to take with the assembly. Supported actions are: <c>install</c>, <c>overwrite</c>, and <c>uninstall</c>.</summary>
        [TaskAttribute("action")]
        public ActionTypes ActionType { get { return _action; } set { _action = value; }}

        /// <summary>Fileset are used to define multiple assemblies.</summary>
        [FileSet("assemblies")]
        public FileSet CopyFileSet      { get { return _assemblies; } }

        /// <summary>Quiet mode.</summary>
        [TaskAttribute("silent")]
        [BooleanValidator()]
        public bool Silent {
            get { return _silent; }
            set { _silent = value; }
        }

        /// <summary>Stop the build if the command does not finish within the specified time.  Specified in milliseconds.  Default is no time out.</summary>
        [TaskAttribute("timeout")]
        [Int32Validator()]
        public override int TimeOut {
            get { return _timeout; }
            set { _timeout = value; }
        }

        public override string ProgramFileName {
            get { return "gacutil"; }
        }

        public override string ProgramArguments {
            get {
                string prefix = Silent ? "/silent" : "/nologo";
                switch (ActionType) {
                    case ActionTypes.install:
                        return String.Format("{0} /i {1}", prefix, AssemblyName);
                    case ActionTypes.overwrite:
                        return String.Format("{0} /if {1}", prefix, AssemblyName);
                    case ActionTypes.uninstall:
                        // uninstalling does not work with a filename, but with an assemblyname
                        string assembly = AssemblyName;
                        FileInfo fi = new FileInfo(AssemblyName);
                        if (fi.Exists) {
                            // strip of the path and extension
                            assembly = fi.Name.Substring(0, (fi.Name.Length - fi.Extension.Length));
                        }
                        return String.Format("{0} /u {1}", prefix, assembly);
                    default:
                        return "";
                }
            }
        }

        protected override void ExecuteTask() {
            if ( (_assemblyName != null) && (_assemblies.FileNames.Count != 0)) {
                throw new BuildException("Cannot use both the \"assembly\" attribute and a \"assemblies\" fileset");
            } else if ( (_assemblyName == null) && (_assemblies.FileNames.Count == 0)) {
                throw new BuildException("Specify either an \"assembly\" attribute or a \"assemblies\" fileset");
            }
            string msg = "";
            switch (ActionType) {
                case ActionTypes.install:
                    msg = "Installing";
                    break;
                case ActionTypes.overwrite:
                    msg = "Overwriting";
                    break;
                case ActionTypes.uninstall:
                    msg = "Uninstalling";
                    break;
            }
            if (_assemblies.FileNames.Count != 0) {
                //multiple assemblies
                Log.WriteLine(LogPrefix + "{0} {1} assemblies", msg, _assemblies.FileNames.Count);
                foreach (string assemblyName in _assemblies.FileNames) {
                    AssemblyName = assemblyName;
                    base.ExecuteTask();
                }
            } else {

                // If the user wants to see the actual command the -verbose flag
                // will cause ExternalProgramBase to display the actual call.
                Log.WriteLine(LogPrefix + "{0} {1}", msg, AssemblyName);
                base.ExecuteTask();
            }
        }
    }
}
