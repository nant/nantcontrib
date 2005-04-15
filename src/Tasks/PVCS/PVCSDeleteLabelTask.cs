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
    /// Removes a label from specified versioned files or projects.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>deletelabel</c> PCLI command to remove the version label from the versioned files.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Removes the label called <c>My Label</c> from the versioned file called <c>App.ico</c> from the project
    ///   database specified by the <c>project-database</c> property.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcsdeletelabel projectdatabase="${project-database}" versionlabel="My Label">
    ///     <entities>
    ///         <entity name="/App.ico"/>
    ///     </entities>
    /// </pvcsdeletelabel>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Removes the label called <c>My Label</c> from all files at and below both <c>folder1</c> and <c>folder2</c>
    ///   in the project database specified by the <c>project-database</c> property.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcsdeletelabel projectdatabase="${project-database}" versionlabel="My Label" includesubprojects="true">
    ///     <entities>
    ///         <entity name="/folder1"/>
    ///         <entity name="/folder2"/>
    ///     </entities>
    /// </pvcsdeletelabel>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("pvcsdeletelabel")]
    public sealed class PVCSDeleteLabelTask : PVCSMultipleEntityTask {
        #region Fields

        /// <see cref="VersionLabel"/>
        private string _versionLabel;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the version label to remove.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-v</c> parameter to the <c>pcli deletelabel</c> command.
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

        /// <see cref="PVCSTask.AddCommandLineArguments"/>
        protected override void AddCommandLineArguments(PVCSCommandArgumentCollection arguments) {
            base.AddCommandLineArguments(arguments);
            arguments.Add("-v", VersionLabel);
        }

        #endregion
    }
}
