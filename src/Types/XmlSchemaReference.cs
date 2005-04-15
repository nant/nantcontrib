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
    /// Represents the schema collection element.
    /// </summary>
    public class XmlSchemaReference : Element {
        #region Private Instance Fields

        private string _namespace;
        private string _source;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// Namespace URI associated with this schema. 
        /// If not present, it is assumed that the 
        /// schema's targetNamespace value is to be used.
        /// </summary>
        [TaskAttribute("namespace")]
        public string Namespace {
            get { return _namespace; }
            set { _namespace = value; }
        }

        /// <summary>
        /// Location of this schema. Could be a 
        /// local file path or an HTTP URL.
        /// </summary>
        [TaskAttribute("source", Required=true)]
        public string Source {
            get { return _source; }
            set { _source = value; }
        }

        #endregion Public Instance Properties
    }
}
