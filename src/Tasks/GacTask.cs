// NAnt - A .NET build tool
// Copyright (C) 2001-2002 Gerry Shaw
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

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

        string _assemblyName = null;
        bool _uninstall = false;
        int _timeout = Int32.MaxValue;

        /// <summary>The name of a file that contains an assembly manifest.</summary>
        [TaskAttribute("assembly", Required=true)]
        public string AssemblyName {
            get { return _assemblyName; }
            set { _assemblyName = value; }
        }

        /// <summary>Removes the assembly from the global assembly cache and the native image cache.  Default is "false".</summary>
        [TaskAttribute("uninstall")]
        [BooleanValidator()]
        public bool Uninstall {
            get { return _uninstall; }
            set { _uninstall = value; }
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
                string assemblyName = AssemblyName;
                if (Uninstall) {
                    int dllExtension = assemblyName.ToUpper().IndexOf(".DLL");
                    if (dllExtension > -1) {
                        assemblyName = assemblyName.Substring(0, dllExtension);
                    }
                    return "/u " + assemblyName;
                } else {
                    return "/i " + assemblyName;
                }
            }
        }

        protected override void ExecuteTask() {
            // If the user wants to see the actual command the -verbose flag
            // will cause ExternalProgramBase to display the actual call.
            Log.WriteLine(LogPrefix + "{0} {1}", Uninstall ? "Uninstalling" : "Installing", AssemblyName);
            base.ExecuteTask();
        }
    }
}
