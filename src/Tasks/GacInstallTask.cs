#region GNU General Public License

// NAntContrib
// Copyright (C) 2004 Kent Boogaart
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
// Kent Boogaart (kentcb@internode.on.net)

#endregion

using System;
using System.Collections;
using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.DotNet.Types;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Installs assemblies into the Global Assembly Cache (GAC) by using the 
    /// <c>gacutil</c> SDK tool.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Assemblies can be installed to the GAC with or without reference counting. 
    /// The full details of reference counting can be found in the SDK 
    /// documentation.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>Installs <c>Shared.dll</c> into the GAC.</para>
    ///   <code>
    ///     <![CDATA[
    /// <gac-install>
    ///     <assemblies>
    ///         <include name="Shared.dll" />
    ///     </assemblies>
    /// </gac-install>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Installs <c>Shared.dll</c> and <c>MyWeb.dll</c> into the GAC.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <gac-install>
    ///     <assemblies>
    ///         <include name="Shared.dll" />
    ///         <include name="MyWeb.dll" />
    ///     </assemblies>
    /// </gac-install>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Installs <c>Shared.dll</c> and <c>MyWeb.dll</c> into the GAC and 
    ///   specifies reference information.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <gac-install>
    ///     <reference scheme-type="Opaque" scheme-id="MyID" scheme-description="My description" />
    ///     <assemblies>
    ///         <include name="Shared.dll" />
    ///         <include name="MyWeb.dll" />
    ///     </assemblies>
    /// </gacinstall>
    ///     ]]>
    ///   </code>
    /// </example>
    [ProgramLocation(LocationType.FrameworkSdkDir)]
    [TaskName("gac-install")]
    public sealed class GacInstallTask : GacTaskBase {
        #region Fields

        /// <summary>
        /// See <see cref="Assemblies" />.
        /// </summary>
        private AssemblyFileSet _assemblies;

        #endregion

        #region Properties

        /// <summary>
        /// Specifies the assemblies to install.
        /// </summary>
        [BuildElement("assemblies", Required=true)]
        public AssemblyFileSet Assemblies {
            get { return _assemblies; }
            set { _assemblies = value; }
        }

        /// <summary>
        /// Gets the assembly list to install.
        /// </summary>
        protected override ICollection AssemblyList {
            get { return _assemblies.FileNames; }
        }

        /// <summary>
        /// If <see langword="true" />, the specified assemblies will be forcibly 
        /// installed. Any existing duplicate assemblies in the GAC will be 
        /// overwritten. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("force", Required=false)]
        [BooleanValidator]
        public override bool Force {
            get { return base.Force; }
            set { base.Force = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructs and initialises an instance of the <c>GacInstallTask</c>.
        /// </summary>
        public GacInstallTask() {
            _assemblies = new AssemblyFileSet();
        }

        /// <summary>
        /// Appends any install-specific arguments.
        /// </summary>
        /// <param name="sb">The <c>StringBuilder</c> to append arguments to.</param>
        protected override void AppendProgramArguments(StringBuilder sb) {
            sb.Append(" /i");
        }

        /// <summary>
        /// Outputs log information.
        /// </summary>
        protected override void BeforeExecuteTask() {
            Log(Level.Info, string.Format("Installing '{0}' . . .", CurrentAssembly));
        }

        #endregion
    }
}
