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
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks.Grep {
    /// <summary>
    /// Searches files for a regular-expression and produces an XML report of 
    /// the matches.
    /// </summary>
    /// <example>
    ///     <para>
    ///         Extract all <i>TODO:</i>, <i>UNDONE:</i> or <i>HACK:</i>-
    ///         comment-tags from C# source files and write them to a file
    ///         <i>out.xml</i>. (A xslt-stylesheet could then transform it to
    ///         a nice html-page for your project-homepage, but that is beyond
    ///         the scope of this example.) 
    ///     </para>
    ///     <para>
    ///         <i>Path</i>, <i>File</i> and <i>LineNumber</i> are automatically
    ///         generated elements.
    ///     </para>
    ///     <code>
    ///         <![CDATA[
    /// <grep output="out.xml" pattern="// (?'Type'TODO|UNDONE|HACK): (\[(?'Author'\w*),(?'Date'.*)\])? (?'Text'[^\n\r]*)">
    ///     <fileset>
    ///         <include name="*.cs" />
    ///     </fileset>
    /// </grep>
    ///         ]]>
    ///     </code>
    ///     <para>
    ///         The resulting XML file for a comment-tag  
    ///         'TODO: [md, 14-02-2004] comment this method'
    ///         will look like
    ///     </para>
    ///     <code>
    ///         <![CDATA[
    /// <?xml version="1.0" encoding="utf-8" ?> 
    /// <Matches>
    ///     <Match>
    ///         <Type>TODO</Type> 
    ///         <Text>comment this method</Text> 
    ///         <Path>C:\MyProjects\MyPath</Path>
    ///         <File>MyFile.cs</Filename> 
    ///         <LineNumber>146</LineNumber> 
    ///         <Author>md</Author>
    ///         <Date>14-02-2004</Date>
    ///     </Match>
    ///     ...
    /// </Matches>
    ///         ]]>
    ///     </code> 
    /// </example>
    [TaskName("grep")]
    public class GrepTask : Task {
        #region Private Instance Fields

        private FileInfo _outputFile;
        private string _pattern;
        private FileSet _inputFiles;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// Specifies the name of the output file.
        /// </summary>
        [TaskAttribute("output", Required=true)]
        public FileInfo OutputFile {
            get { return _outputFile; }
            set { _outputFile = value; }
        }

        /// <summary>
        /// Specifies the regular-expression to search for.
        /// </summary>
        [TaskAttribute("pattern", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Pattern {
            get { return _pattern; }
            set { _pattern = value; }
        }

        /// <summary>
        /// The set of files in which the expression is searched.
        /// </summary>
        [BuildElement("fileset")]
        public FileSet InputFiles {
            get { return _inputFiles; }
            set { _inputFiles = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        /// <summary>
        /// Performs the regex-search.
        /// </summary>
        protected override void ExecuteTask() {
            MatchCollection matches = new MatchCollection();
            Pattern pattern = new Pattern(Pattern);

            Log(Level.Info, "Writing matches to '{0}'.", OutputFile.FullName);

            foreach (string filename in InputFiles.FileNames ) {
                using (StreamReader reader = new StreamReader(filename)) {
                    string fileContent = reader.ReadToEnd();
                    matches.Add(pattern.Extract(filename, fileContent));
                }
            }

            WriteMatches(matches);
        }

        #endregion Override implementation of Task

        #region Private Instance Methods

        private delegate void WriteMethod(MatchCollection matches, StreamWriter writer);

        /// <summary>
        /// Writes the collection of matches to the specified <see cref="StreamWriter" />
        /// in XML format.
        /// </summary>
        /// <param name="matches">The matches to write.</param>
        /// <param name="writer"><see cref="StreamWriter" /> to write the matches to.</param>
        private void WriteXml(MatchCollection matches, StreamWriter writer) {
            XmlTextWriter xmlWriter = new XmlTextWriter(writer);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Matches");
            foreach (Match match in matches) {
                match.WriteXml(xmlWriter);
            }
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
        }

        /// <summary>
        /// Writes the specified matches to <see cref="OutputFile" />.
        /// </summary>
        /// <param name="matches">The collection of matches to write.</param>
        private void WriteMatches(MatchCollection matches) {
            WriteMethod writeMethod = new WriteMethod(this.WriteXml);
            using (StreamWriter writer = new StreamWriter(OutputFile.FullName)) {
                writeMethod(matches, writer);
            }
        }

        #endregion Private Instance Methods
    }
}
