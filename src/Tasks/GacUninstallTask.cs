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
using NAnt.Contrib.Types;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Uninstalls assemblies from the Global Assembly Cache (GAC) by using the 
    /// <c>gacutil</c> SDK tool.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Assemblies are specified via an <see cref="AssemblySet"/>. Individual 
    /// assemblies are specified by their identity information. Only a name is 
    /// required but, optionally, the assembly version, culture and public key 
    /// token may be specified.
    /// </para>
    /// <para>
    /// Assemblies can be uninstalled from the GAC with or without reference 
    /// counting. The full details of reference counting can be found in the 
    /// SDK documentation.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>Uninstalls <c>Shared</c> assembly from the GAC.</para>
    ///   <code>
    ///     <![CDATA[
    /// <gac-uninstall>
    ///        <assemblies>
    ///            <assembly name="Shared" />
    ///        </assemblies>
    /// </gac-uninstall>
    ///     ]]>
    /// </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Uninstalls <c>Shared</c> and <c>MyWeb</c> from the GAC.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <gac-uninstall>
    ///        <assemblies>
    ///            <assembly name="Shared" />
    ///            <assembly name="MyWeb" />
    ///        </assemblies>
    /// </gac-uninstall>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Decrements references to <c>Shared</c> in the GAC and uninstalls if 
    ///   the reference count reaches zero.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <gac-uninstall>
    ///     <reference scheme-type="Opaque" scheme-id="MyID" scheme-description="My description" />
    ///        <assemblies>
    ///            <assembly name="Shared" />
    ///        </assemblies>
    /// </gac-uninstall>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Uninstalls version <c>2.1.7.9201</c> of <c>Shared</c> plus the 
    ///   Australian-cultured <c>MyWeb</c> from the GAC.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <gac-uninstall>
    ///        <assemblies>
    ///            <assembly name="Shared" version="2.1.7.9201" />
    ///            <assembly name="MyWeb" culture="en-AU" />
    ///        </assemblies>
    /// </gac-uninstall>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Uninstalls the neutrally-cultured, version <c>1.0.5000.0</c> of 
    ///   <c>System.Xml</c> from the native image cache. The assembly must
    ///   also have a public key token of <c>b77a5c561934e08a</c> to be 
    ///   uninstalled.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <gac-uninstall native="true">
    ///        <assemblies>
    ///            <assembly name="System.Xml" version="1.0.5000.0" public-key-token="b77a5c561934e08a" culture="Neutral" />
    ///        </assemblies>
    /// </gac-uninstall>
    ///     ]]>
    ///   </code>
    /// </example>
    [ProgramLocation(LocationType.FrameworkSdkDir)]
    [TaskName("gac-uninstall")]
    public sealed class GacUninstallTask : GacTaskBase {
        #region Fields

        /// <summary>
        /// See <see cref="Native" />.
        /// </summary>
        private bool _native;

        /// <summary>
        /// See <see cref="Assemblies" />.
        /// </summary>
        private AssemblySet _assemblies;

        #endregion

        #region Properties

        /// <summary>
        /// If <see langword="true" />, specifies that the assemblies should be 
        /// uninstalled from the native image cache. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("native", Required = false)]
        [BooleanValidator]
        public bool Native {
            get { return _native; }
            set { _native = value; }
        }

        /// <summary>
        /// Specifies the assemblies to uninstall.
        /// </summary>
        [BuildElement("assemblies", Required=true)]
        public AssemblySet Assemblies {
            get { return _assemblies; }
            set { _assemblies = value; }
        }

        /// <summary>
        /// Gets the assembly list to uninstall.
        /// </summary>
        protected override ICollection AssemblyList {
            get { return _assemblies.AssemblyCollection; }
        }

        /// <summary>
        /// If <see langword="true" />, the specified assemblies will be forcibly 
        /// removed from the GAC. All references to the specified assemblies will
        /// be removed from the GAC prior to removing the assemblies themselves. 
        /// The default is <see langword="false" />.
        /// </summary>
        /// <remarks>
        /// You cannot use this option to remove an assembly that was installed using Microsoft Windows Installer.
        /// </remarks>
        [TaskAttribute("force", Required=false)]
        [BooleanValidator]
        public override bool Force {
            get { return base.Force; }
            set { base.Force = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructs an instance of the <c>GacUninstallTask</c>.
        /// </summary>
        public GacUninstallTask() {
            _assemblies = new AssemblySet();
        }

        /// <summary>
        /// Appends any install-specific arguments.
        /// </summary>
        /// <param name="sb"></param>
        protected override void AppendProgramArguments(StringBuilder sb) {
            sb.Append(" /u");

            if (Native) {
                sb.Append("ngen");
            }
        }

        /// <summary>
        /// Outputs log information.
        /// </summary>
        protected override void BeforeExecuteTask() {
            Log(Level.Info, string.Format("Uninstalling '{0}' . . .", CurrentAssembly));
        }

        #endregion
    }
}
