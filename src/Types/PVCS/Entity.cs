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
using NAnt.Core.Util;

namespace NAnt.Contrib.Types.PVCS {
    /// <summary>
    /// Represents an entity in an <see cref="EntitySet"/>.
    /// </summary>
    public sealed class Entity : Element {
        #region Fields

        /// <see cref="Name"/>
        private string _name;

        /// <see cref="If"/>
        private bool _if;

        /// <see cref="Unless"/>
        private bool _unless;

        #endregion

        #region Properties

        /// <summary>
        /// The path for the entity.
        /// </summary>
        [TaskAttribute("name", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public new string Name {
            get {
                return _name;
            }
            set {
                _name = value;
            }
        }

        /// <summary>
        /// If <c>true</c> then the entity will be included. The default is <c>true</c>.
        /// </summary>
        [TaskAttribute("if")]
        [BooleanValidator]
        public bool If {
            get {
                return _if;
            }
            set {
                _if = value;
            }
        }

        /// <summary>
        /// Opposite of <see cref="If"/>. If <c>false</c> then the entity will be included. The default is
        /// <c>false</c>.
        /// </summary>
        [TaskAttribute("unless")]
        [BooleanValidator]
        public bool Unless {
            get {
                return _unless;
            }
            set {
                _unless = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructs and initializes an instance of <c>Entity</c>.
        /// </summary>
        public Entity() {
            If = true;
        }

        #endregion
    }
}
