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

using System.Text.RegularExpressions;

namespace NAnt.Contrib.Tasks.Grep {
    /// <summary>
    /// This purpose of this class is to get the line-numbers within 
    /// a string for a specific position of a character 
    /// (an index, as returned by the <see cref="Regex" /> class).
    /// </summary>
    public class LineCounter {
        #region Private Instance Fields

        /// <summary>
        /// The string to count in
        /// </summary>
        private string _string;

        /// <summary>
        /// The current position within <see cref="_string" />.
        /// </summary>
        private int _currentPos;

        /// <summary>
        /// The number of line feeds upto (but exluding) <see cref="_currentPos" />.
        /// </summary>
        private int _currentLine;

        #endregion Private Instance Fields

        #region Public Instance Constructors

        /// <summary>
        /// Constructs a line-counter for a <see cref="string" />.
        /// </summary>
        /// <param name="str"><see cref="string" /> for which lines are counted.</param>
        public LineCounter(string str) {
            _string         = str;
            _currentLine    = 1;
            _currentPos     = 0;
        }

        #endregion Public Instance Constructors

        #region Public Instance Methods

        /// <summary>
        /// Counts the line-numbers until the position <paramref name="pos" />
        /// is reached.
        /// </summary>
        /// <param name="pos">Index into the string given during construction </param>
        /// <returns>
        /// The number of lines.
        /// </returns>
        public int CountTo(int pos) {
            if (_currentPos <= pos) {
                _currentLine += Count(_string, '\n', _currentPos, pos);
            } else {
                _currentLine -= Count(_string, '\n', pos, _currentPos);
            }
            _currentPos = pos;
            return _currentLine;
        }

        #endregion Public Instance Methods

        #region Private Instance Methods

        /// <summary>
        /// Counts the number of occurences of <paramref name="c" /> in the 
        /// range from <paramref name="start" /> to <paramref name="end" /> in 
        /// string <paramref name="str" />.
        /// </summary>
        /// <param name="str"><see cref="string" /> to count in.</param>
        /// <param name="c">Character to count.</param>
        /// <param name="start">Start of range.</param>
        /// <param name="end">End of range.</param>
        /// <returns>
        /// The number of occurences of <paramref name="c" /> in the range from 
        /// <paramref name="start" /> to <paramref name="end" /> in string 
        /// <paramref name="str" />.
        /// </returns>
        private int Count(string str, char c, int start, int end) {
            int lines = 0;
            for( int i = start ; i < end ; i++ ) {
                if( str[i] == c ) {
                    lines++;
                }
            }
            return lines;
        }

        #endregion Private Instance Methods
    }
}
