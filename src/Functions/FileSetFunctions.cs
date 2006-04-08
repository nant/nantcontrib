// NAnt - A .NET build tool
// Copyright (C) 2001-2004 Gerry Shaw
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
// Bill Martin

using System;
using System.Text;

using NAnt.Core;
using NAnt.Core.Types;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Functions {
    /// <summary>
    /// Provides methods for interrogating Filesets.
    /// </summary>
    [FunctionSet("fileset", "FileSet")]
    public class FileSetFunctions : FunctionSetBase {
        #region Public Instance Constructors

        public FileSetFunctions(Project project, PropertyDictionary properties) : base(project, properties) { }

        #endregion Public Instance Constructors

        #region Public Instance Methods

        /// <summary>
        /// Determines the number of files within a <see cref="FileSet"/>.
        /// </summary>
        /// <param name="fileset">The id of the FileSet to scan.</param>
        /// <returns>The number of files included in the FileSet</returns>
        /// <exception cref="ArgumentException"><paramref name="fileset" /> is not a valid refid to a defined fileset.</exception>
        /// <example>
        ///   <para>
        ///   Define a fileset and check the number of files in it.
        ///   </para>
        ///   <code>
        ///     <![CDATA[
        /// <fileset id="test.fileset">
        ///     <include name="**/*.cs">
        /// </fileset>
        /// <echo message="FileSet contains ${fileset::get-file-count('test.fileset')} files." />
        ///     ]]>
        ///   </code>
        /// </example>
        [Function("get-file-count")]
        public int GetFileCount(string fileset) {
            //Try to retrieve the specified fileset from the Data References on the project
            FileSet fs = base.Project.DataTypeReferences[fileset] as FileSet;
            //If the value was missing or the referenced type was not a fileset, then throw an exception
            if (fs == null) {
                throw new ArgumentException(string.Format("'{0}' is not a valid fileset reference", fileset));
            }
            //Otherwise return the number of files in the fileset.
            return fs.FileNames.Count;
        }

        /// <summary>
        /// Determines whether <see cref="FileSet"/> contains any files.
        /// </summary>
        /// <param name="fileset">The id of the fileset to check.</param>
        /// <returns><see langword="true" /> if the FileSet contains one or more files, otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="fileset" /> is not a valid refid to a defined fileset.</exception>
        /// <example>
        ///   <para>
        ///   Perform conditional processing on a fileset if it contains files.
        ///   </para>
        ///   <code>
        ///     <![CDATA[
        /// <fileset id="test.fileset">
        ///     <include name="**/*.cs">
        /// </fileset>
        /// <if test="${fileset::has-files('test.fileset')}">
        ///     <dostuff... />
        /// </if>
        ///     ]]>
        ///   </code>
        /// </example>
        [Function("has-files")]
        public bool HasFiles(string fileset) {
            //Try to retrieve the specified fileset from the Data References on the project
            FileSet fs = base.Project.DataTypeReferences[fileset] as FileSet;
            //If the value was missing or the referenced type was not a fileset, then throw an exception
            if (fs == null) {
                throw new ArgumentException(string.Format("'{0}' is not a valid fileset reference", fileset));
            }
            //Otherwise return whether or not the fileset contains files.
            return (fs.FileNames.Count > 0);
        }

        /// <summary>
        /// Returns a delimited string of all the filenames within a <see cref="FileSet"/> with each filename
        /// separated by the specified delimiter string.
        /// </summary>
        /// <param name="fileset">The id of the fileset to check.</param>
        /// <param name="delimiter">String to separate filenames with.</param>
        /// <returns>A delimited string of the filenames within the specified FileSet.</returns>
        /// <exception cref="ArgumentException"><paramref name="fileset" /> is not a valid refid to a defined fileset.</exception>
        /// <example>
        ///   <para>
        ///   Displays a space-pipe-space separated string fo the files within a defined FileSet.
        ///   </para>
        ///   <code>
        ///     <![CDATA[
        /// <fileset id="test.fileset">
        ///     <include name="**/*.cs">
        /// </fileset>
        /// <echo message="${fileset::to-string('test.fileset', ' | ')}">
        ///     ]]>
        ///   </code>
        /// </example>
        [Function("to-string")]
        public string ToString(string fileset, string delimiter) {
            //Try to retrieve the specified fileset from the Data References on the project
            FileSet fs = base.Project.DataTypeReferences[fileset] as FileSet;
            
            //If the value was missing or the referenced type was not a fileset, then throw an exception
            if (fs == null) {
                throw new ArgumentException(string.Format("'{0}' is not a valid fileset reference", fileset));
            }

            //We've got a valid fileset.  Use a stringbuilder to build the delimiter separated list.
            StringBuilder sb = new StringBuilder();

            //This is a loop as we need to omit the delimiter on the last element, and it's easier 
            //to check the last element with a for loop than in a foreach loop.
            for (int i = 0; i < fs.FileNames.Count; i++) {
                string file = fs.FileNames[i];
                sb.Append(file);
                //if this is not the last element, then append the delimiter
                if (i < fs.FileNames.Count - 1) {
                    sb.Append(delimiter);
                }
            }
            //Now return the string representation of the FileSet.
            return sb.ToString();
        }

        #endregion Public Instance Methods
    }
}
