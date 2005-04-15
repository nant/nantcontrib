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
    /// Deletes the specified users from the PVCS access control database.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>deleteuser</c> PCLI command to delete the users.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Deletes the users called <c>kb</c>, <c>kv</c> and <c>tb</c> from the project database specified by the
    ///   <c>project-database</c> property.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcsdeleteusers projectdatabase="${project-database}">
    ///     <entities>
    ///         <entity name="kb"/>
    ///         <entity name="kv"/>
    ///         <entity name="tb"/>
    ///     </entities>
    /// </pvcsdeleteusers>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("pvcsdeleteusers")]
    public sealed class PVCSDeleteUsersTask : PVCSMultipleEntityTask {
        #region Fields

        #endregion

        #region Properties

        /// <see cref="PVCSProjectDatabaseTask.SupportsIncludeSubprojects"/>
        public override bool SupportsIncludeSubprojects {
            get {
                return false;
            }
        }

        /// <see cref="PVCSTask.PCLICommandName"/>
        protected override string PCLICommandName {
            get {
                return "deleteuser";
            }
        }

        #endregion

        #region Methods

        #endregion
    }
}
