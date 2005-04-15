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
using NAnt.Contrib.Tasks;

namespace NAnt.Contrib.Types {
    /// <summary>
    /// Used to specify reference information when working with the GAC.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The full details of GAC references can be found in the SDK documentation.
    /// </para>
    /// </remarks>
    public sealed class GacReference : Element {
        #region Fields

        /// <summary>
        /// See <see cref="If"/>.
        /// </summary>
        private bool _if;

        /// <summary>
        /// See <see cref="Unless"/>.
        /// </summary>
        private bool _unless;

        /// <summary>
        /// See <see cref="SchemeType"/>.
        /// </summary>
        private SchemeType _schemeType;

        /// <summary>
        /// See <see cref="SchemeId"/>.
        /// </summary>
        private string _schemeId;

        /// <summary>
        /// See <see cref="SchemeDescription"/>.
        /// </summary>
        private string _schemeDescription;

        #endregion

        #region Properties

        /// <summary>
        /// If <c>true</c> then the entity will be included. The default is <c>true</c>.
        /// </summary>
        [TaskAttribute("if")]
        [BooleanValidator]
        public bool If {
            get { return _if; }
            set { _if = value; }
        }

        /// <summary>
        /// The scheme type to use when working with GAC references. The default 
        /// is <see cref="F:SchemeType.None" />, which means that references will 
        /// not be used by the GAC task.
        /// </summary>
        [TaskAttribute("scheme-type", Required=false)]
        public SchemeType SchemeType {
            get { return _schemeType; }
            set { _schemeType = value; }
        }

        /// <summary>
        /// The scheme ID to use when working with GAC references. This is only 
        /// relevant if a scheme type other than <see cref="F:SchemeType.None" />
        /// is specified.
        /// </summary>
        [TaskAttribute("scheme-id", Required=false)]
        [StringValidator(AllowEmpty = false)]
        public string SchemeId {
            get { return _schemeId; }
            set { _schemeId = value; }
        }

        /// <summary>
        /// The scheme description to use when working with GAC references. This 
        /// is only relevant if a scheme type other than <see cref="F:SchemeType.None" />
        /// is specified.
        /// </summary>
        [TaskAttribute("scheme-description", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string SchemeDescription {
            get { return _schemeDescription; }
            set { _schemeDescription = value; }
        }

        /// <summary>
        /// Opposite of <see cref="If"/>. If <c>false</c> then the entity will be included. The default is
        /// <c>false</c>.
        /// </summary>
        [TaskAttribute("unless")]
        [BooleanValidator]
        public bool Unless {
            get { return _unless; }
            set { _unless = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructs and initializes an instance of <c>GacReference</c>.
        /// </summary>
        public GacReference() {
            If = true;
        }

        #endregion
    }
}
