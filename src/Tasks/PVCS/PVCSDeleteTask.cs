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
using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks.PVCS {
    /// <summary>
    /// Deletes folder, projects, versioned items and workspaces in a PVCS repository.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>delete</c> PCLI command to delete the items.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    ///   Deletes the versioned file called <c>App.ico</c> from the project database specified by the
    ///   <c>project-database</c> property.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcsdelete projectdatabase="${project-database}">
    ///     <entities>
    ///         <entity name="/App.ico"/>
    ///     </entities>
    /// </pvcsdelete>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Deletes the files called <c>file1.txt</c> and <c>file2.txt</c> from the project called <c>folder</c> in the
    ///   project database specified by the <c>project-database</c> property.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcsdelete projectdatabase="${project-database}" projectpath="/folder">
    ///     <entities>
    ///         <entity name="file1.txt"/>
    ///         <entity name="file2.txt"/>
    ///     </entities>
    /// </pvcsdelete>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("pvcsdelete")]
    public sealed class PVCSDeleteTask : PVCSMultipleEntityTask {
        #region Fields

        #endregion

        #region Properties

        /// <see cref="PVCSProjectDatabaseTask.SupportsIncludeSubprojects"/>
        public override bool SupportsIncludeSubprojects {
            get {
                return false;
            }
        }

        #endregion

        #region Methods

        #endregion
    }
}
