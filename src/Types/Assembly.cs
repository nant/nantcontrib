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
using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Util;

namespace NAnt.Contrib.Types {
    /// <summary>
    /// Represents a single assembly in an <see cref="AssemblySet"/>.
    /// </summary>
    public sealed class Assembly : Element {
        #region Fields

        /// <summary>
        /// See <see cref="Culture"/>.
        /// </summary>
        private string _culture;

        /// <summary>
        /// See <see cref="If"/>.
        /// </summary>
        private bool _if;

        /// <summary>
        /// See <see cref="Name"/>.
        /// </summary>
        private string _name;

        /// <summary>
        /// See <see cref="PublicKeyToken"/>.
        /// </summary>
        private string _publicKeyToken;

        /// <summary>
        /// See <see cref="Unless"/>.
        /// </summary>
        private bool _unless;

        /// <summary>
        /// See <see cref="Version"/>.
        /// </summary>
        private string _version;

        #endregion

        #region Properties

        /// <summary>
        /// The culture for the assembly.
        /// </summary>
        [TaskAttribute("culture", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string Culture {
            get {
                return _culture;
            }
            set {
                _culture = value;
            }
        }

        /// <summary>
        /// If <c>true</c> then the assembly will be included. The default is <c>true</c>.
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
        /// The name of the assembly.
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
        /// The public key token of the assembly.
        /// </summary>
        [TaskAttribute("public-key-token", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string PublicKeyToken {
            get {
                return _publicKeyToken;
            }
            set {
                _publicKeyToken = value;
            }
        }

        /// <summary>
        /// Opposite of <see cref="If"/>. If <c>false</c> then the assembly will be included. The default is
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

        /// <summary>
        /// The version of the assembly.
        /// </summary>
        [TaskAttribute("version", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string Version {
            get {
                return _version;
            }
            set {
                _version = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructs and initializes an instance of <c>Assembly</c>.
        /// </summary>
        public Assembly() {
            If = true;
        }

        /// <summary>
        /// Converts this <c>Assembly</c> object into it's <c>string</c> representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            StringBuilder retVal = new StringBuilder();

            retVal.Append(Name);

            if (Version != null) {
                retVal.Append(",Version=").Append(Version);
            }

            if (Culture != null) {
                retVal.Append(",Culture=").Append(Culture);
            }

            if (PublicKeyToken != null) {
                retVal.Append(",PublicKeyToken=").Append(PublicKeyToken);
            }

            return retVal.ToString();
        }

        #endregion
    }
}
