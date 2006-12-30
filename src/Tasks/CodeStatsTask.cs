// NAntContrib
// Copyright (C) 2003 Brant Carter
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
// Brant Carter (brantcarter@hotmail.com)

using System;
using System.Collections;
using System.IO;
using System.Xml;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

using NAnt.Contrib.Types;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Generates statistics from source code.
    /// </summary>
    /// <remarks>
    /// Scans files in a fileset counting lines.
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Generate statistics for a set of C# and VB.NET sources, applying 
    ///   different labels for both.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <codestats output="test.xml" append="true" buildname="MyBuildName">
    ///     <counts>
    ///         <count label="C#">
    ///             <fileset>
    ///                 <include name="**/*.cs" />
    ///             </fileset>
    ///         </count>
    ///         <count label="VB">
    ///             <fileset>
    ///                 <include name="**\*.vb" />
    ///             </fileset>
    ///         </count>
    ///     </counts>
    /// </codestats>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Generate statistics for all C# sources and only output a summary to 
    ///   the log.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <codestats output="test.xml" verbose="true" summarize="true">
    ///     <counts>
    ///         <count label="C#">
    ///             <fileset>
    ///                 <include name="**\*.cs" />
    ///             </fileset>
    ///         </count>
    ///     </counts>
    /// </codestats>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("codestats")]
    public class CodeStatsTask : Task {
        #region Private Instance Fields

        private int _lineCount;
        private int _commentLineCount;
        private int _emptyLinesCount;
        private ArrayList _fileNames = new ArrayList();
        private CodeStatsCountCollection _codeStats = new CodeStatsCountCollection();
        private FileInfo _outputFile;
        private bool _appendFile;
        private bool _summarize;
        private string _buildName;

        #endregion Private Instance Fields

        #region Private Static Fields

        private const string _commentDblSlash ="//";
        private const string _commentSingleQuote ="'";
        private const string _commentSlashStar ="/*";
        private const string _commentStarSlash ="*/";

        #endregion Private Static Fields

        #region Public Instance Properties

        /// <summary>
        /// Set of line counters to enable.  
        /// </summary>
        [BuildElementCollection("counts", "count")]
        public CodeStatsCountCollection CodeStats {
            get { return _codeStats; }
        }

        /// <summary>
        /// An identifier to be able to track which build last updated the 
        /// code stats file.
        /// </summary>
        [TaskAttribute("buildname")]
        public string  BuildName {
            get { return _buildName; }
            set { _buildName = value; }
        }

        /// <summary>
        /// Specifies whether the results should be appended to the output file.
        /// The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("append")]
        public bool AppendFile {
            get { return _appendFile; }
            set { _appendFile = value; }
        }

        /// <summary>
        /// If you only want to show summary stats for the whole fileset
        /// </summary>
        [TaskAttribute("summarize", Required=false)]
        public bool Summarize {
            get { return _summarize; }
            set { _summarize = value; }
        }

        /// <summary>
        /// The name of the file to save the output to (in XML).
        /// </summary>
        [TaskAttribute("output")]
        public FileInfo OutputFile {
            get { return _outputFile; }
            set { _outputFile = value; }
        }

        #endregion Public Instance Properties

        #region Override implemenation of Task

        protected override void ExecuteTask() {
            XmlDocument doc = new XmlDocument();
            XmlNode codeSummaries = null;

            // if the output file is specified then write to it
            if (OutputFile != null) {
                // if append file is true and the file already exist.
                // assume it is in the right format to be able to append the new
                // xml nodes to it
                if (AppendFile && OutputFile.Exists) {
                    // load the existing document
                    doc.Load(OutputFile.FullName);

                    // select the root node so that we can append to it
                    codeSummaries = doc.SelectSingleNode("//code-summaries");
                } else {
                    // if not appending to the document and creating new
                    // create the Xml declaration and append it to the document
                    XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                    doc.AppendChild(docNode);

                    // add the code-summaries root element
                    codeSummaries = doc.CreateElement("code-summaries");
                    doc.AppendChild(codeSummaries);
                }

                // create code summary note to write all of the information
                // for this run into
                XmlNode summaryNode = doc.CreateElement("code-summary");

                // add the date
                XmlAttribute date = doc.CreateAttribute("date");
                date.Value = XmlConvert.ToString(DateTime.Now);
                summaryNode.Attributes.Append(date);

                // add the buildname
                XmlAttribute buildName = doc.CreateAttribute("buildname");
                buildName.Value = BuildName;
                summaryNode.Attributes.Append(buildName);

                // loop thru each of the CodeStatsTypeCollection
                foreach (CodeStatsCount codeStat in CodeStats) {
                    // count the lines for the fileset for this CodeStatsType
                    CountLines(codeStat.FileSet, codeStat.Label);

                    // create the linecount node
                    XmlNode lineCountNode = doc.CreateElement("linecount");

                    // add the label
                    XmlAttribute label = doc.CreateAttribute("label");
                    label.Value = codeStat.Label;
                    lineCountNode.Attributes.Append(label);

                    // add the total line count
                    XmlAttribute totalLineCount = doc.CreateAttribute("totalLineCount");
                    totalLineCount.Value = _lineCount.ToString();
                    lineCountNode.Attributes.Append(totalLineCount);

                    // add the empty line count
                    XmlAttribute emptyLineCount = doc.CreateAttribute("emptyLineCount");
                    emptyLineCount.Value = _emptyLinesCount.ToString();
                    lineCountNode.Attributes.Append(emptyLineCount);

                    // add the comment line count
                    XmlAttribute commentLineCount = doc.CreateAttribute("commentLineCount");
                    commentLineCount.Value = _commentLineCount.ToString();
                    lineCountNode.Attributes.Append(commentLineCount);

                    // if not showing only summary.  display the line count information
                    // for each of the files within the fileset
                    if (!Summarize) {
                        // create file summaries node
                        XmlNode fileSummaries = doc.CreateElement("file-summaries");

                        ICollection collection = this._fileNames;

                        // loop thru each file in the _fileNames ArrayList 
                        foreach (FileCodeCountInfo file in collection) {
                            // create a file summary node
                            XmlElement fileNode = doc.CreateElement("file-summary");

                            // add the file name
                            fileNode.SetAttribute("name", file.FileName);

                            // add the total line count
                            fileNode.SetAttribute("totalLineCount", file.LineCount.ToString());

                            // add the empty line count
                            fileNode.SetAttribute("emptyLineCount", file.EmptyLineCount.ToString());

                            // add the comment line count
                            fileNode.SetAttribute("commentLineCount", file.CommentLineCount.ToString());

                            // append the file summary node to the file summaries node
                            fileSummaries.AppendChild(fileNode);
                        }

                        // add the file summaries node to the line count node
                        lineCountNode.AppendChild(fileSummaries);
                    }

                    //add the line count node to the code summary node
                    summaryNode.AppendChild(lineCountNode);
                }

                // add the code summary node to the code summaries node
                codeSummaries.AppendChild(summaryNode);

                // save the xml document
                doc.Save(OutputFile.FullName);
            }
        }

        #endregion Override implemenation of Task

        #region Private Instance Methods

        private void CountLines(FileSet TargetFileSet, string label) {
            _lineCount = 0;
            _commentLineCount = 0;
            _emptyLinesCount = 0;
            _fileNames = new ArrayList();
            _fileNames.Capacity = TargetFileSet.FileNames.Count;

            FileCodeCountInfo fileInfo;

            foreach(string file in TargetFileSet.FileNames) {
                fileInfo = CountFile(file);
                _lineCount += fileInfo.LineCount;
                _emptyLinesCount += fileInfo.EmptyLineCount;
                _commentLineCount += fileInfo.CommentLineCount;
                _fileNames.Add(fileInfo);
            }

            _fileNames.TrimToSize();
            Log(Level.Info, "Totals:\t[{0}] \t[T] {1}\t[C] {2}\t[E] {3}", 
                label, _lineCount, _commentLineCount, _emptyLinesCount);
        }

        private FileCodeCountInfo CountFile(string fileName) {
            int fileLineCount = 0;
            int fileCommentLineCount =0;
            int fileEmptyLineCount = 0;
            bool inComment = false;

            using (StreamReader sr = File.OpenText(fileName)) {
                while (sr.Peek() != -1) {
                    string line = sr.ReadLine();
                    if (line != null) {
                        line = line.Trim();
                        if (line == "") {
                            fileEmptyLineCount++;
                        } else if (line.StartsWith(_commentSlashStar)) {
                            fileCommentLineCount++;
                            // we're only in comment block if it was not closed
                            // on same line
                            if (line.IndexOf(_commentStarSlash) == -1) {
                                inComment = true;
                            }
                        } else if (inComment) {
                            fileCommentLineCount++;
                            // check if comment block is closed in line
                            if (line.IndexOf(_commentStarSlash) != -1) {
                                inComment = false;
                            }
                        } else if (line.StartsWith(_commentDblSlash)) {
                            fileCommentLineCount++;
                        } else if (line.StartsWith(_commentSingleQuote)) {
                            fileCommentLineCount++;
                        }
                        fileLineCount++;
                    }
                }
                sr.Close();
            }

            if (!Summarize) {
                Log(Level.Info,  "{0} Totals:\t[T] {1}\t[C] {2}\t[E] {3}", 
                    fileName, fileLineCount, fileCommentLineCount, 
                    fileEmptyLineCount);
            }

            return new FileCodeCountInfo(fileName, fileLineCount, fileCommentLineCount,
                fileEmptyLineCount);
        }

        #endregion Private Instance Methods

        private struct FileCodeCountInfo {
            public int LineCount;
            public int CommentLineCount;
            public int EmptyLineCount;
            public string FileName;

            public FileCodeCountInfo(string fileName, int lineCount, int commentLineCount, int emptyLineCount) {
                FileName = fileName;
                LineCount = lineCount;
                CommentLineCount = commentLineCount;
                EmptyLineCount = emptyLineCount;
            }
        }
    }
}
