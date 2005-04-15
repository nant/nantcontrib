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
    /// Renames a label in a PVCS repository.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>renamelabel</c> PCLI command to rename the label.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Renames the label on <c>App.ico</c> from <c>Beater</c> to <c>Beta</c> in the project database specified by
    ///   the <c>project-database</c> property.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcsrenamelabel projectdatabase="${project-database}" from="Beater" to="Beta">
    ///     <entities>
    ///         <entity name="App.ico"/>
    ///     </entities>
    /// </pvcsrenamelabel>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Renames the label on all files from <c>Alfa</c> to <c>Alpha</c> in the project database specified by the
    ///   <c>project-database</c> property.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcsrenamelabel projectdatabase="${project-database}" from="Alfa" to="Alpha" includesubprojects="true">
    ///     <entities>
    ///         <entity name="/"/>
    ///     </entities>
    /// </pvcsrenamelabel>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("pvcsrenamelabel")]
    public sealed class PVCSRenameLabelTask : PVCSMultipleEntityTask {
        #region Fields

        /// <see cref="From"/>
        private string _from;

        /// <see cref="To"/>
        private string _to;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the existing label.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-vf</c> parameter to the <c>pcli renamelabel</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("from", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string From {
            get {
                return _from;
            }
            set {
                _from = value;
            }
        }

        /// <summary>
        /// Gets or sets the new label.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-vt</c> parameter to the <c>pcli renamelabel</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("to", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string To {
            get {
                return _to;
            }
            set {
                _to = value;
            }
        }

        #endregion

        #region Methods

        /// <see cref="PVCSTask.AddCommandLineArguments"/>
        protected override void AddCommandLineArguments(PVCSCommandArgumentCollection arguments) {
            base.AddCommandLineArguments(arguments);
            arguments.Add("-vf", From);
            arguments.Add("-vt", To);
        }

        #endregion
    }
}
