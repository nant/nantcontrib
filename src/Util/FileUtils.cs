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

        /// <summary>
        /// Given an absolute directory and an absolute file name, returns a 
        /// relative file name.
        /// </summary>
        /// <param name="basePath">An absolute directory.</param>
        /// <param name="absolutePath">An absolute file name.</param>
        /// <returns>
        /// A relative file name for the given absolute file name.
        /// </returns>
        public static string GetRelativePath(string basePath, string absolutePath) {
            string fullBasePath = Path.GetFullPath(basePath);
            string fullAbsolutePath = Path.GetFullPath(absolutePath);

            bool caseInsensitive = false;

            // check if we're not on unix
            if ((int) Environment.OSVersion.Platform != 128) {
                // for simplicity, we'll consider all filesystems on windows
                // to be case-insensitive
                caseInsensitive = true;

                // on windows, paths with different roots are located on different
                // drives, so only absolute names will do
                if (string.Compare(Path.GetPathRoot(fullBasePath), Path.GetPathRoot(fullAbsolutePath), caseInsensitive) != 0) {
                    return fullAbsolutePath;
                }
            }

            int baseLen = fullBasePath.Length;
            int absoluteLen = fullAbsolutePath.Length;

            // they are on the same "volume", find out how much of the base path
            // is in the absolute path
            int i = 0;
            while (i < absoluteLen && i < baseLen && string.Compare(fullBasePath[i].ToString(), fullAbsolutePath[i].ToString(), caseInsensitive) == 0) {
                i++;
            }
            
            if (i == baseLen && (fullAbsolutePath[i] == Path.DirectorySeparatorChar || fullAbsolutePath[i-1] == Path.DirectorySeparatorChar)) {
                // the whole current directory name is in the file name,
                // so we just trim off the current directory name to get the
                // current file name.
                if (fullAbsolutePath[i] == Path.DirectorySeparatorChar) {
                    // a directory name might have a trailing slash but a relative
                    // file name should not have a leading one...
                    i++;
                }

                return fullAbsolutePath.Substring(i);
            }

            // The file is not in a child directory of the current directory, so we
            // need to step back the appropriate number of parent directories by
            // using ".."s.  First find out how many levels deeper we are than the
            // common directory

            string commonPath = fullBasePath.Substring(0, i);

            int levels = 0;
            string parentPath = fullBasePath;

            // remove trailing directory separator character
            if (parentPath[parentPath.Length - 1] == Path.DirectorySeparatorChar) {
                parentPath = parentPath.Substring(0, parentPath.Length - 1);
            }

            while (string.Compare(parentPath,commonPath, caseInsensitive) != 0) {
                levels++;
                DirectoryInfo parentDir = Directory.GetParent(parentPath);
                if (parentDir != null) {
                    parentPath = parentDir.FullName;
                } else {
                    parentPath = null;
                }
            }
                
            string relativePath = "";
            
            for (i = 0; i < levels; i++) {
                relativePath += ".." + Path.DirectorySeparatorChar;
            }

            relativePath += fullAbsolutePath.Substring(commonPath.Length);
            return relativePath;
        }

        #endregion Public Static Methods
    }
}
