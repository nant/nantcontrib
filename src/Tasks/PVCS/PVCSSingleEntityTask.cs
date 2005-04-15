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
using NAnt.Core.Util;

using NAnt.Contrib.Types.PVCS;

namespace NAnt.Contrib.Tasks.PVCS {
    /// <summary>
    /// Base class for all PVCS project database tasks that operate against a single entity.
    /// </summary>
    public abstract class PVCSSingleEntityTask : PVCSProjectDatabaseTask {
        #region Fields

        /// <see cref="Entity"/>
        private string _entity;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the entity involved in the operation.
        /// </summary>
        [TaskAttribute("entity", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public virtual string Entity {
            get {
                return _entity;
            }
            set {
                _entity = value;
            }
        }

        #endregion

        #region Methods

        /// <see cref="PVCSTask.AddCommandLineArguments"/>
        protected override void AddCommandLineArguments(PVCSCommandArgumentCollection arguments) {
            base.AddCommandLineArguments(arguments);
            arguments.Add(Entity, null, PVCSCommandArgumentPosition.End);
        }

        #endregion
    }
}
