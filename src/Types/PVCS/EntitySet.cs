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
using System.Collections.Specialized;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Types.PVCS {
    /// <summary>
    /// Represents a set of entities to include in a PVCS project database task.
    /// </summary>
    /// <seealso cref="NAnt.Contrib.Tasks.PVCS.PVCSMultipleEntityTask"/>
    [Serializable]
    [ElementName("entities")]
    public sealed class EntitySet : DataTypeBase {
        #region Fields

        /// <see cref="EntityPaths"/>
        private StringCollection _entityPaths;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the collection of entity paths assigned to this entity set.
        /// </summary>
        internal StringCollection EntityPaths {
            get {
                return _entityPaths;
            }
        }

        /// <summary>
        /// The entities to include in the project task.
        /// </summary>
        [BuildElementArray("entity", Required = true)]
        public Entity[] Entities {
            set {
                foreach (Entity entity in value) {
                    if (entity.If && !entity.Unless) {
                        EntityPaths.Add(entity.Name);
                    }
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructs and initializes an instance of <c>EntitySet</c>.
        /// </summary>
        public EntitySet() {
            _entityPaths = new StringCollection();
        }

        #endregion
    }
}
