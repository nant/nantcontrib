//
// NAntContrib
// Copyright (C) 2001-2004 Gerry Shaw
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
using System.Globalization;
using System.IO;
using System.Xml;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;
using NAnt.Core.Types;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Manipulates the contents of the global assembly cache.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   This tasks provides some of the same functionality as the gacutil tool 
    ///   provided in the .NET Framework SDK.
    ///   </para>
    ///   <para>
    ///   Specifically, the <see cref="GacTask" /> allows you to install assemblies 
    ///   into the cache and remove them from the cache.
    ///   </para>
    ///   <para>
    ///   Refer to the <see href="ms-help://MS.NETFrameworkSDK/cptools/html/cpgrfglobalassemblycacheutilitygacutilexe.htm">
    ///   Global Assembly Cache Tool (Gacutil.exe)</see> for more information.
    ///   </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Inserts assembly <c>mydll.dll</c> into the global assembly cache.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <gac assembly="mydll.dll" action="install" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Removes the assembly <c>hello</c> from the global assembly cache and 
    ///   the native image cache.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <gac assembly="hello" action="uninstall" />
    ///     ]]>
    ///   </code>
    ///   <para>
    ///   Note that the previous command might remove more than one assembly 
    ///   from the assembly cache because the assembly name is not fully 
    ///   specified. For example, if both version 1.0.0.0 and 3.2.2.1 of 
    ///   <c>hello</c> are installed in the cache, both of the assemblies will 
    ///   be removed from the global assembly cache.
    ///   </para>
    /// </example>
    /// <example>
    ///   <para>
    ///   Use the following example to avoid removing more than one assembly. 
    ///   This command removes only the hello assembly that matches the fully 
    ///   specified version number, culture, and public key.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <gac assembly="hello,Version=1.0.0.1,Culture=de,PublicKeyToken=45e343aae32233ca" action="uninstall" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("gac")]
    [ProgramLocation(LocationType.FrameworkSdkDir)]
    [Obsolete("Use either <gac-install> or <gac-uninstall> tasks instead.", false)]
    public class GacTask : ExternalProgramBase {
        /// <summary>
        /// Defines the actions that can be performed on an assembly using the
        /// <see cref="GacTask" />.
        /// </summary>
        public enum ActionTypes {
            /// <summary>
            /// Installs an assembly into the global assembly cache.
            /// </summary>
            install,

            /// <summary>
            /// Installs an assembly into the global assembly cache. If an assembly 
            /// with the same name already exists in the global assembly cache, it is 
            /// overwritten.
            /// </summary>
            overwrite,

            /// <summary>
            /// Uninstalls an assembly from the global assembly cache.
            /// </summary>
            uninstall
        };

        #region Private Instance Fields

        private ActionTypes _action = ActionTypes.install;
        private string _assemblyName;
        private FileSet _assemblies = new FileSet();

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The name of a file that contains an assembly manifest.
        /// </summary>
        [TaskAttribute("assembly")]
        public string AssemblyName {
            get { return _assemblyName; }
            set { _assemblyName = value; }
        }

        /// <summary>
        /// Defines the action to take with the assembly. The default is 
        /// <see cref="ActionTypes.install" />.
        /// </summary>
        [TaskAttribute("action")]
        public ActionTypes ActionType {
            get { return _action; }
            set { _action = value; }
        }

        /// <summary>
        /// Fileset are used to define multiple assemblies.
        /// </summary>
        [BuildElement("assemblies")]
        public FileSet AssemblyFileSet {
            get { return _assemblies; }
            set { _assemblies = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of ExternalProgramBase

        public override string ExeName {
            get { return "gacutil"; }
        }

        /// <summary>
        /// Gets a value indiciating whether the external program is a managed
        /// application which should be executed using a runtime engine, if 
        /// configured. 
        /// </summary>
        /// <value>
        /// <see langword="ManagedExecutionMode.Auto" />.
        /// </value>
        /// <remarks>
        /// Modifying this property has no effect.
        /// </remarks>
        public override ManagedExecution Managed {
            get { return ManagedExecution.Auto; }
            set { }
        }

        public override string ProgramArguments {
            get {
                // suppresses display of the logo banner
                string prefix = "/nologo";
                if (!Verbose) {
                    prefix += " /silent";
                }
                switch (ActionType) {
                    case ActionTypes.install:
                        return string.Format(CultureInfo.InvariantCulture, "{0} /i \"{1}\"", prefix, AssemblyName);
                    case ActionTypes.overwrite:
                        return string.Format(CultureInfo.InvariantCulture, "{0} /if \"{1}\"", prefix, AssemblyName);
                    case ActionTypes.uninstall:
                        // uninstalling does not work with a filename, but with an assemblyname
                        string assembly = AssemblyName;
                        FileInfo fi = new FileInfo(AssemblyName);
                        if (fi.Exists) {
                            // strip of the path and extension
                            assembly = fi.Name.Substring(0, (fi.Name.Length - fi.Extension.Length));
                        }
                        return string.Format(CultureInfo.InvariantCulture, "{0} /u \"{1}\"", prefix, assembly);
                    default:
                        throw new BuildException(string.Format(CultureInfo.InvariantCulture, 
                            "Unknown action '{0}'.", ActionType.ToString(CultureInfo.InvariantCulture)),
                            Location);
                }
            }
        }

        protected override void ExecuteTask() {
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
            if (AssemblyFileSet.FileNames.Count != 0) {
                Log(Level.Info, "{0} {1} assemblies.", msg, AssemblyFileSet.FileNames.Count);
                foreach (string assemblyName in AssemblyFileSet.FileNames) {
                    AssemblyName = assemblyName;
                    base.ExecuteTask();
                }
            } else {
                Log(Level.Info, "{0} assembly '{1}'.", msg, AssemblyName);
                base.ExecuteTask();
            }
        }

        #endregion Override implementation of ExternalProgramBase

        #region Override implementation of Element

        protected override void Initialize() {
            base.Initialize ();

            if (AssemblyName != null && AssemblyFileSet.FileNames.Count != 0) {
                throw new BuildException("Cannot use both the \"assembly\"" +
                    " attribute and a \"assemblies\" fileset.", Location);
            } else if (AssemblyName == null && AssemblyFileSet.FileNames.Count == 0) {
                throw new BuildException("Specify either an \"assembly\" attribute"
                    + " or a \"assemblies\" fileset.", Location);
            }
        }

        #endregion Override implementation of Element
    }
}
