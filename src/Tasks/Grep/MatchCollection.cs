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

using System.Collections;

namespace NAnt.Contrib.Tasks.Grep {
    /// <summary>
    ///  A strongly-typed collection of <see cref="Match"/> instances.
    /// </summary>
    public class MatchCollection : CollectionBase {
        #region Public Instance Properties

        /// <summary>
        /// Gets the <paramref name="idx" />th match stored in this collection.
        /// </summary>
        public Match this[int idx] {
            get { return (Match)List[idx]; }
        }

        #endregion Public Instance Properties

        #region Public Instance Methods

        /// <summary>
        /// Adds a <see cref="Match" /> to this collection.
        /// </summary>
        /// <param name="match"><see cref="Match" /> to add.</param>
        public void Add(Match match) {
            List.Add(match);
        }

        /// <summary>
        /// Adds all <see cref="Match"/> instances <paramref name="matches" />
        /// to this collection.
        /// </summary>
        /// <param name="matches">Collection of <see cref="Match" /> instances to add.</param>
        public void Add(MatchCollection matches) {
            foreach (Match match in matches) {
                this.Add(match);
            }
        }

        #endregion Public Instance Methods
    }
}
