//
// NAntContrib
// Copyright (C) 2004 Manfred Doetter (mdoetter@users.sourceforge.net)
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

using System;
using System.Collections;
//using System.IO;
using System.Xml;

namespace NAnt.Contrib.Tasks.Grep {
    /// <summary>
    /// Encapsulation of a match of a regular-expression with the
    /// associated named capture-groups.
    /// </summary>
    public class Match {
        #region Private Instance Fields

        /// <summary>
        /// <see cref="Hashtable" /> containing the mapping from group names 
        /// to values.
        /// </summary>
        private Hashtable values = new Hashtable();

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// Gets or sets the value for a capture group.
        /// </summary>
        public string this[string name] {
            get { return (string)values[name]; }
            set { values[name] = value; }
        }

        #endregion Public Instance Properties

        #region Public Instance Methods

        /// <summary>
        /// Writes this match to an <see cref="XmlTextWriter" />.
        /// </summary>
        /// <param name="xmlWriter">The <see cref="XmlTextWriter" /> to write to.</param>
        public void WriteXml(XmlTextWriter xmlWriter) {
            xmlWriter.WriteStartElement("Match");
            foreach (string groupName in values.Keys) {
                xmlWriter.WriteElementString(groupName,this[groupName]);
            }
            xmlWriter.WriteEndElement();
        }

        #endregion Public Instance Methods
    }
}
