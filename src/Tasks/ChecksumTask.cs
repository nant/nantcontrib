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
using System.IO;
using System.Reflection;
using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

using NAnt.Contrib.Util;

namespace NAnt.Contrib.Tasks { 
    /// <summary>
    /// Calculates checksums for a set of files.
    /// Loosely based on Ant's Checksum task.
    /// </summary>
    /// <remarks>
    /// This task takes a set of input files in a fileset
    /// and calculates a checksum for each one of them. 
    /// You can specify the algorithm to use when calculating
    /// the checksum value (MD5 or SHA1, for example).
    /// The calculated value is saved to a file with the same
    /// name as the input file and an added extension either
    /// based on the algorithm name (e.g. .MD5), or whatever 
    /// is specified through the fileext attribute.
    /// </remarks>
    /// <example>
    ///   <code>
    ///     <![CDATA[
    /// <checksum algorithm="MD5" fileext="MD5">
    ///     <fileset>
    ///         <include name="${outputdir}\*.dll"/>
    ///     </fileset>
    /// </checksum>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("checksum")]
    public class ChecksumTask : Task {
        private string _algorithm = "MD5";
        private string _fileext = null;
        private FileSet _fileset = new FileSet();

        /// <summary>
        /// Name of Algorithm to use when calculating
        /// the checksum. Can be MD5 or SHA1.
        /// </summary>
        [TaskAttribute("algorithm", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Algorithm { 
            get { return _algorithm; } 
            set { _algorithm = value; } 
        }
        
        /// <summary>
        /// The generated checksum file's name will be the 
        /// original filename with "." and fileext 
        /// added to it. Defaults to the 
        /// algorithm name being used
        /// </summary>
        [TaskAttribute("fileext")]
        public string FileExtension {
            get { return _fileext; }
            set { _fileext = value; }
        }

        /// <summary>
        /// Set of files to use as input
        /// </summary>
        [BuildElement("fileset")]
        public FileSet FileSet {
            get { return _fileset; }
            set { _fileset = value; }
        }


        /// <summary>
        /// Initializes task and ensures the supplied attributes are valid.
        /// </summary>
        protected override void Initialize() {
            if (FileSet.FileNames.Count == 0) {
                throw new BuildException("Checksum fileset cannot be empty!", Location);
            }

            if (FileExtension == null) {
                FileExtension = Algorithm;
            }
        }

        /// <summary>
        /// This is where the work is done
        /// </summary>
        protected override void ExecuteTask() {
            ChecksumHelper helper = new ChecksumHelper(Algorithm);

            foreach ( String file in FileSet.FileNames ) {
                string checksum = helper.CalculateChecksum(file);
                string outfile = file + "." + FileExtension;
            
                WriteChecksum(outfile, checksum);
            }
        }


        /// <summary>
        /// Writes a checksum to a destination file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="checksum"></param>
        private void WriteChecksum(string filename, string checksum) {
            StreamWriter writer = null;
            try {
                writer = new StreamWriter(filename);
                writer.Write(checksum);
            } catch ( Exception e ) {
                Log(Level.Error, "Checksum: Failed to write to {0}: {1}", filename, e.Message);
            } finally {
                if ( writer != null )
                    writer.Close();
            }
        }
    }
}
