// NAnt - A .NET build tool
// Copyright (C) 2001-2008 Gerry Shaw
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
// Gert Driesen (drieseng@users.sourceforge.net)

using System;
using System.IO;

using NAnt.Core;
using NAnt.Core.Types;
using NAnt.Core.Attributes;
using NAnt.Contrib.Util;

namespace NAnt.Contrib.Functions {
    /// <summary>
    /// Groups a set of functions for dealing with files.
    /// </summary>
    [FunctionSet("file", "File")]
    public class FileFunctions : FunctionSetBase {
        #region Public Instance Constructors

        public FileFunctions(Project project, PropertyDictionary properties) : base(project, properties) {
        }

        #endregion Public Instance Constructors

        #region Public Instance Methods

        /// <summary>
        /// Calculates a checksum for a given file and returns it in a hex
        /// string.
        /// </summary>
        /// <param name="path">The file for which to calculate a checksum.</param>
        /// <param name="algorithm">The name of hash algorithm to use.</param>
        /// <returns>
        /// The hex-formatted checksum of the specified file.
        /// </returns>
        /// <exception cref="IOException">The specified file does not exist.</exception>
        /// <exception cref="ArgumentException">
        /// <para>
        /// <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters.
        /// </para>
        /// <para>-or-</para>
        /// <para>
        /// <paramref name="algorithm" /> is not a valid name of a hash algorithm.
        /// </para>
        /// </exception>
        /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
        /// <exception cref="NotSupportedException">The <paramref name="path" /> parameter is in an invalid format.</exception>
        /// <example>
        ///   <para>
        ///   Displays the MD5 checksum of a file.
        ///   </para>
        ///   <code>
        ///     <![CDATA[
        /// <echo>Checksum=${file::get-checksum('Cegeka.exe', 'MD5')}"></echo>
        ///     ]]>
        ///   </code>
        /// </example>
        [Function("get-checksum")]
        public String GetChecksum(string path, string algorithm) {
            string fullPath = Project.GetFullPath(path);
            ChecksumHelper helper = new ChecksumHelper (algorithm);
            return helper.CalculateChecksum (fullPath);
        }

        #endregion Public Instance Methods
    }
}
