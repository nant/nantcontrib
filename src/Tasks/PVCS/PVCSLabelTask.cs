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
using System.Text;
using NAnt.Core.Attributes;
using NAnt.Core.Types;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks.PVCS {
    /// <summary>
    /// Assigns a version label to a revision of the specified versioned files.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>label</c> PCLI command to label the items.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Labels all files in the project database specified by the <c>project-database</c> property. The label
    ///   applied is <c>Beta</c>.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcslabel projectdatabase="${project-database}" versionlabel="Beta" includesubprojects="true">
    ///     <entities>
    ///         <entity name="/"/>
    ///     </entities>
    /// </pvcslabel>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Labels revision <c>1.8</c> of <c>App.ico</c> as <c>Dodgy</c> in the project database specified by the
    ///   <c>project-database</c> property.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcslabel projectdatabase="${project-database}" versionlabel="Dodgy" revision="1.8">
    ///     <entities>
    ///         <entity name="App.ico"/>
    ///     </entities>
    /// </pvcslabel>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("pvcslabel")]
    public sealed class PVCSLabelTask : PVCSMultipleEntityTask {
        #region Fields

        /// <see cref="Floating"/>
        private bool _floating;

        /// <see cref="Revision"/>
        private double _revision;

        /// <see cref="VersionLabel"/>
        private string _versionLabel;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the label should "float" to the newest revision.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-f</c> parameter to the <c>pcli label</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("floating", Required = false)]
        [BooleanValidator]
        public bool Floating {
            get {
                return _floating;
            }
            set {
                _floating = value;
            }
        }

        /// <summary>
        /// Gets or sets the revision to label.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-r</c> parameter to the <c>pcli label</c> command.
        /// </para>
        /// <para>
        /// If this property has not yet been set, it will return <c>Double.MaxValue</c>.
        /// </para>
        /// </remarks>
        [TaskAttribute("revision", Required = false)]
        public double Revision {
            get {
                return _revision;
            }
            set {
                _revision = value;
            }
        }

        /// <summary>
        /// Gets or sets the version label to assign.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-v</c> parameter to the <c>pcli label</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("versionlabel", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string VersionLabel {
            get {
                return _versionLabel;
            }
            set {
                _versionLabel = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructs and initializes an instance of <c>PVCSLabel</c>.
        /// </summary>
        public PVCSLabelTask() {
            _revision = Double.MaxValue;
        }

        /// <see cref="PVCSTask.AddCommandLineArguments"/>
        protected override void AddCommandLineArguments(PVCSCommandArgumentCollection arguments) {
            base.AddCommandLineArguments(arguments);

            if (Floating) {
                arguments.Add("-f");
            }

            if (Revision != Double.MaxValue) {
                arguments.Add("-r", Revision);
            }

            if (VersionLabel != null) {
                arguments.Add("-v", VersionLabel);
            }
        }

        #endregion
    }
}
