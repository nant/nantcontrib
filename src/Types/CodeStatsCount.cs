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

using System;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Util;
using NAnt.Core.Types;

namespace NAnt.Contrib.Types {
    public sealed class CodeStatsCount : Element {
        #region Private Instance Fields

        private string _label = "";
        private FileSet _fileset = new FileSet();

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The set of files to work on.
        /// </summary>
        [BuildElement("fileset", Required=true)]
        public FileSet FileSet {
            get { return _fileset; }
            set { _fileset = value; }
        }

        /// <summary>
        /// The label to apply to the results.
        /// </summary>
        [TaskAttribute("label", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Label {
            get { return _label; }
            set { _label= value; }
        }

        #endregion Public Instance Properties
    }
}
