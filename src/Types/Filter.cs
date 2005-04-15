#region GNU General Public License

// NAntContrib
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

#endregion

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Types {
    /// <summary>
    /// Individual filter component of <see cref="FilterSet" />.
    /// </summary>
    public class Filter : Element {
        #region Private Instance Fields

        /// <summary>
        /// Holds the token which will be replaced in the filter operation.
        /// </summary>
        private string _token;
        
        /// <summary>
        /// Holsd the value which will replace the token in the filtering operation.
        /// </summary>
        private string _value;

        #endregion Private Instance Fields
        
        #region Public Instance Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Filter" /> class.
        /// </summary>
        public Filter() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Filter" /> class with
        /// the given token and value.
        /// </summary>
        /// <param name="token">The token which will be replaced when filtering.</param>
        /// <param name="value">The value which will replace the token when filtering.</param>
        public Filter(string token, string value) {
            _token = token;
            _value = value;
        }

        #endregion Public Instance Constructors

        /// <summary>
        /// The token which will be replaced when filtering.
        /// </summary>
        [TaskAttribute("token", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Token {
            get { return _token; }
            set { _token = value; }
        }
        
        /// <summary>
        /// The value which will replace the token when filtering.
        /// </summary>
        [TaskAttribute("value", Required=true)]
        public string Value {
            get { return _value; }
            set { _value = value; }
        }
    }
}
