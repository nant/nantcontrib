// NAntContrib
// Copyright (C) 2004 Gert Driesen (drieseng@users.sourceforge.net)
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

using System;
using System.IO;
using System.Text;

using NAnt.Contrib.Types;

namespace NAnt.Contrib.Util {
    /// <summary>
    /// Groups a set of useful file manipulation methods.
    /// </summary>
    public sealed class FileUtils {
        #region Private Instance Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileUtils" /> class.
        /// </summary>
        /// <remarks>
        /// Prevents instantiation of the <see cref="FileUtils" /> class.
        /// </remarks>
        private FileUtils() {
        }

        #endregion Private Instance Constructors

        #region Public Static Methods

        /// <summary>
        /// Copies a file while replacing the tokens identified by the given
        /// <see cref="FilterSetCollection" />.
        /// </summary>
        /// <param name="sourceFileName">The file to copy.</param>
        /// <param name="destinationFileName">The name of the destination file.</param>
        /// <param name="encoding">The <see cref="Encoding" /> used when filter-copying the file.</param>
        /// <param name="filtersets">The collection of filtersets that should be applied to the file.</param>
        public static void CopyFile(string sourceFileName, string destinationFileName, Encoding encoding, FilterSetCollection filtersets) {
            if (filtersets.HasFilters()) {
                StreamReader reader = null;
                StreamWriter writer = null;

                try {
                    if (encoding == null) {
                        reader = new StreamReader(new BufferedStream(File.OpenRead(sourceFileName)));
                        writer = new StreamWriter(new BufferedStream(File.Create(destinationFileName)));
                    } else {
                        reader = new StreamReader(new BufferedStream(File.OpenRead(sourceFileName)), encoding);
                        writer = new StreamWriter(new BufferedStream(File.Create(destinationFileName)), encoding);
                    }

                    string line = reader.ReadLine();
                    while (line != null) {
                        if (line.Length == 0) {
                            writer.WriteLine();
                        } else {
                            writer.WriteLine(filtersets.ReplaceTokens(line));
                        }
                        line = reader.ReadLine();
                    }
                } finally {
                    if (writer != null) {
                        writer.Close();
                    }
                    if (reader != null) {
                        reader.Close();
                    }
                }
            } else {
                // copy the source file to the destination file 
                File.Copy(sourceFileName, destinationFileName, true);
            }
        }

        /// <summary>
        /// Moves a file while replacing the tokens identified by the given
        /// <see cref="FilterSetCollection" />.
        /// </summary>
        /// <param name="sourceFileName">The file to move.</param>
        /// <param name="destinationFileName">The name of the destination file.</param>
        /// <param name="encoding">The <see cref="Encoding" /> used when filter-copying the file.</param>
        /// <param name="filtersets">The collection of filtersets that should be applied to the file.</param>
        public static void MoveFile(string sourceFileName, string destinationFileName, Encoding encoding, FilterSetCollection filtersets) {
            if (filtersets.HasFilters()) {
                // copy the source file to the destination file and replace tokens
                FileUtils.CopyFile(sourceFileName, destinationFileName, encoding, filtersets); 

                // remove the source file
                File.Delete(sourceFileName);
            } else {
                // move the source file to destination file
                File.Move(sourceFileName, destinationFileName);
            }
        }

        #endregion Public Static Methods
    }
}
