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
    /// Base class for all PVCS project database tasks that operate against one or more entities.
    /// </summary>
    public abstract class PVCSMultipleEntityTask : PVCSProjectDatabaseTask {
        #region Fields

        /// <see cref="Entities"/>
        private EntitySet _entities;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the entities involved in the operation.
        /// </summary>
        [BuildElement("entities", Required = true)]
        public EntitySet Entities {
            get {
                return _entities;
            }
            set {
                _entities = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructs and initializes an instance of <c>PVCSMultipleEntityTask</c>.
        /// </summary>
        public PVCSMultipleEntityTask() {
            _entities = new EntitySet();
        }

        /// <see cref="PVCSTask.AddCommandLineArguments"/>
        protected override void AddCommandLineArguments(PVCSCommandArgumentCollection arguments) {
            base.AddCommandLineArguments(arguments);

            foreach (string entityPath in Entities.EntityPaths) {
                arguments.Add(entityPath, null, PVCSCommandArgumentPosition.End);
            }
        }

        #endregion
    }
}