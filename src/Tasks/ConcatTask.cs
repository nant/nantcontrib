//
// NAntContrib
// Copyright (C) 2002 Tomas Restrepo (tomasr@mvps.org)
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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// A task that concatenates a set of files.
    /// Loosely based on Ant's Concat task.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task takes a set of input files in a fileset
    /// and concatenates them into a single file. You can 
    /// either replace the output file, or append to it 
    /// by using the append attribute.
    /// </para>
    /// <para>
    /// The order the files are concatenated in is not
    /// especified.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <code>
    ///     <![CDATA[
    /// <concat destfile="${outputdir}\Full.txt" append="true">
    ///     <fileset>
    ///         <include name="${outputdir}\Test-*.txt" />
    ///     </fileset>
    /// </concat>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("concat")]
    public class ConcatTask : Task {
        #region Private Instance Fields

        private FileInfo _destinationFile;
        private bool _append;
        private FileSet _fileset = new FileSet();

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// Name of the destination file.
        /// </summary>
        [TaskAttribute("destfile", Required=true)]
        public FileInfo DestinationFile {
            get { return _destinationFile; }
            set { _destinationFile = value; }
        }
        
        /// <summary>
        /// Specifies whether to append to the destination file.
        /// The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("append")]
        [BooleanValidator()]
        public bool Append { 
            get { return _append; } 
            set { _append = value; } 
        }

        /// <summary>
        /// Set of files to use as input.
        /// </summary>
        [BuildElement("fileset")]
        public FileSet FileSet {
            get { return _fileset; }
            set { _fileset = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Element

        /// <summary>
        /// Initializes task and ensures the supplied attributes are valid.
        /// </summary>
        protected override void Initialize() {
            if (FileSet.FileNames.Count == 0) {
                throw new BuildException("Concat fileset cannot be empty!", 
                    Location);
            }
        }

        #endregion Override implementation of Element

        #region Override implementation of Task

        /// <summary>
        /// This is where the work is done
        /// </summary>
        protected override void ExecuteTask() {
            FileStream output = OpenDestinationFile();

            try {
                AppendFiles(output);
            } finally {
                output.Close();
            }
        }

        #endregion Override implementation of Task

        #region Private Instance Methods

        /// <summary>
        /// Opens the destination file according
        /// to the specified flags
        /// </summary>
        /// <returns></returns>
        private FileStream OpenDestinationFile() {
            FileMode mode;

            if (Append) {
                mode = FileMode.Append | FileMode.OpenOrCreate; 
            } else {
                mode = FileMode.Create;
            }

            try {
                return File.Open(DestinationFile.FullName, mode);
            } catch (IOException ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "File \"{0}\" could not be opened.", DestinationFile.FullName),
                    Location, ex);
            }
        }

        /// <summary>
        /// Appends all specified files
        /// </summary>
        /// <param name="output">File to write to</param>
        private void AppendFiles(FileStream output) {
            const int size = 64*1024;
            byte[] buffer = new byte[size];

            foreach (string file in FileSet.FileNames) {
                int bytesRead = 0;
                FileStream input = null;

                try {
                    input = File.OpenRead(file);
                } catch (IOException ex) {
                    Log(Level.Info, "File \"{0}\" could not be read: {1}", 
                        file, ex.Message);
                    continue;
                }
               
                try {
                    while ((bytesRead = input.Read(buffer, 0, size)) != 0) {
                        output.Write(buffer, 0, bytesRead);
                    }
                } catch (IOException ex) {
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                        "Could not read or write from file \"{0}\".", file), 
                        Location, ex);
                } finally {
                    input.Close();
                }
            }
        }

        #endregion Private Instance Methods
    }
}
