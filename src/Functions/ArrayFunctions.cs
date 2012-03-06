// NAnt - A .NET build tool
// Copyright (C) 2001-2012 Gerry Shaw
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// Ryan Boggs (rmboggs@users.sourceforge.net)

using System;

using NAnt.Core;
using NAnt.Core.Types;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Functions
{
    /// <summary>
    /// Provides a set of functions that deals with Arrays.
    /// </summary>
    [FunctionSet("array", "Array")]
    public class ArrayFunctions : FunctionSetBase
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="NAnt.Contrib.Functions.ArrayFunctions"/> class.
        /// </summary>
        /// <param name='project'>
        /// Project.
        /// </param>
        /// <param name='properties'>
        /// Properties.
        /// </param>
        public ArrayFunctions(Project project, PropertyDictionary properties)
            : base(project, properties) {}

        #endregion Public Constructors

        #region Public Instance Methods

        /// <summary>
        /// Sorts the items in an array.
        /// </summary>
        /// <param name='array'>
        /// The <see cref="System.String"/> containing all the array items to sort.
        /// </param>
        /// <param name='separator'>
        /// The <see cref="System.String"/> used to separate the items in
        /// <paramref name="array"/>
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="separator"/> is invalid because it
        /// is empty or <see langword="null" />.
        /// </exception>
        [Function("sort")]
        public string Sort(string array, string separator)
        {
            string[] tempArray = StringToArray(array, separator);
            Array.Sort(tempArray);
            return String.Join(separator, tempArray);
        }

        /// <summary>
        /// Reverse the items in an array.
        /// </summary>
        /// <param name='array'>
        /// The <see cref="System.String"/> containing all the array items to reverse.
        /// </param>
        /// <param name='separator'>
        /// The <see cref="System.String"/> used to separate the items in
        /// <paramref name="array"/>
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="separator"/> is invalid because it
        /// is empty or <see langword="null" />.
        /// </exception>
        [Function("reverse")]
        public string Reverse(string array, string separator)
        {
            string[] tempArray = StringToArray(array, separator);
            Array.Reverse(tempArray);
            return String.Join(separator, tempArray);
        }

        #endregion Public Instance Methods

        #region Private Instance Methods

        /// <summary>
        /// Converts a <see cref="System.String"/> variable to
        /// an <see cref="System.Array"/> based on a value separator.
        /// </summary>
        /// <returns>
        /// The value of <paramref name="array"/> in an <see cref="System.Array"/>
        /// format.
        /// </returns>
        /// <param name='array'>
        /// The <see cref="System.String"/> to parse to an <see cref="System.Array"/>.
        /// </param>
        /// <param name='separator'>
        /// The <see cref="System.String"/> used to separate the items in
        /// <paramref name="array"/>
        /// </param>
        /// <exception cref='ArgumentNullException'>
        /// The <paramref name="separator"/> is invalid because it
        /// is empty or <see langword="null" />.
        /// </exception>
        private string[] StringToArray(string array, string separator)
        {
            if (String.IsNullOrEmpty(separator))
            {
                throw new ArgumentNullException("separator");
            }
            return array.Split(new string[] { separator }, StringSplitOptions.None);

        }

        #endregion Private Instance Methods
    }
}

