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
using SourceForge.NAnt.Attributes;
using SourceForge.NAnt;
using System.Xml;

struct FileCodeCountInfo {
    public int LineCount;
    public int CommentLineCount;
    public int EmptyLineCount;
    public string FileName;
    public string Directory;
    public FileCodeCountInfo(string fileName, string directory, int lineCount, 
        int commentLineCount, int emptyLineCount) {
        this.Directory = directory;
        this.FileName = fileName;
        this.LineCount = lineCount;
        this.CommentLineCount = commentLineCount;
        this.EmptyLineCount = emptyLineCount;
    }
}

namespace Nant.Contrib.Tasks.CodeStatsTask {
    /// <summary>
    /// Generates Statistics from Source Code
    /// </summary>
    /// <remarks>
    /// <para>Scans files in a fileset counting lines</para>
    /// </remarks>
    /// <example>
    /// <para>Count all .cs files for a project</para>
    /// <para>Verbose mode is suported and will print a summary to the Log</para>
    /// <para>Output can be saved to an xml file</para>
    /// <code>
    /// <![CDATA[
    ///		<codestats outputFile="test.xml" verbose="true">
    ///			<fileset>
    ///				<includes name="**.cs" />
    ///			</fileset>
    ///		</codestats>
    /// ]]>
    /// </code>
    /// </example>
    [TaskName("codestats")]
    public class CodeStatsTask : Task {
        protected int _lineCount;
        protected int _commentLineCount;
        protected int _emptyLinesCount;
        protected ArrayList _fileNames = new ArrayList();

        const string _commentDblSlash ="//";
        const string _commentSingleQuote ="'";
        const string _commentSlashStar ="/*";
        const string _commentStarSlash ="*/";
        int _StillInComment = 0;

        string _outputFile = "";
        FileSet _fileset = new FileSet();

        /// <summary>
        /// The file set to work on
        /// </summary>
        [FileSet("fileset")]
        public FileSet TargetFileSet {
            get { return _fileset; }
        }

        /// <summary>
        /// If you want to save the output (in xml) specify the file name here
        /// </summary>
        [TaskAttribute("outputFile", Required=false)]
        public string OutputFile {
            get { return _outputFile;}
            set { _outputFile = value;}
        }


        protected override void ExecuteTask() {
            countLines();
            if (_outputFile != "") {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("code-summary");
                root.SetAttribute("date", DateTime.Now.ToString("M/d/yyyy"));
                root.SetAttribute("time", DateTime.Now.ToString("hh:mm tt"));
                root.SetAttribute("totalLineCount",this._lineCount.ToString());
                root.SetAttribute("emptyLineCount", this._emptyLinesCount.ToString());
                root.SetAttribute("commentLineCount", 
                    this._commentLineCount.ToString());
                doc.AppendChild(root);

                ICollection collection = this._fileNames;
                int filesCounted = collection.Count;
                foreach(FileCodeCountInfo file in collection) {
                    XmlElement fileNode = doc.CreateElement("file-summary");
                    fileNode.SetAttribute("name", file.Directory + "\\" + file.FileName);
                    fileNode.SetAttribute("totalLineCount", file.LineCount.ToString());
                    fileNode.SetAttribute("emptyLineCount", 
                        file.EmptyLineCount.ToString());
                    fileNode.SetAttribute("commentLineCount", 
                        file.CommentLineCount.ToString());
                    root.AppendChild (fileNode);
                }
                doc.Save(_outputFile);
            }
        }

        private void countLines() {
            _lineCount = 0;
            _commentLineCount = 0;
            _emptyLinesCount = 0;
            _fileNames.Capacity = TargetFileSet.FileNames.Count;

            FileCodeCountInfo fileInfo;
            foreach(string file in TargetFileSet.FileNames) {
                fileInfo = countFile(file);
                _lineCount += fileInfo.LineCount;
                _emptyLinesCount += fileInfo.EmptyLineCount;
                _commentLineCount += fileInfo.CommentLineCount;
                _fileNames.Add(fileInfo);
            }
            this._fileNames.TrimToSize();
            Log.WriteLineIf(Verbose, "Totals:\t[T] {0}\t[C] {1}\t[E] {2}", _lineCount, _commentLineCount, _emptyLinesCount);
        }
        private FileCodeCountInfo countFile(string FileName) {
            int fileLineCount = 0;
            int fileCommentLineCount =0;
            int fileEmptyLineCount = 0;

            // open files for streamreader
            StreamReader sr = File.OpenText(FileName);

            //loop until the end
            bool keepReading = true;
            while (keepReading) {
                string line = sr.ReadLine();

                if(line != null) {
                    line = line.Trim();
                    if(line == "") {
                        fileEmptyLineCount++;
                    }
                    else if(line.StartsWith(_commentSlashStar)) {
                        fileCommentLineCount++;
                        if(line.IndexOf(_commentStarSlash)== 0) {
                            _StillInComment = 1;
                        }
                    }
                    else if(_StillInComment == 1) {
                        fileCommentLineCount++;
                        if(line.IndexOf(_commentStarSlash)>0) {
                            _StillInComment = 0;
                        }
                    }
                    else if(line.StartsWith(_commentDblSlash)) {
                        fileCommentLineCount++;
                    }
                    else if(line.StartsWith(_commentSingleQuote)) {
                        fileCommentLineCount++;
                    }

                    fileLineCount++;
                }
                else {
                    keepReading = false;
                }
            }

            sr.Close();
            Log.WriteLineIf(Verbose, "{0} Totals:\t[T] {1}\t[C] {2}\t[E] {3}", FileName, fileLineCount, fileCommentLineCount, fileEmptyLineCount);
            return new FileCodeCountInfo(FileName, 
                FileName,fileLineCount,fileCommentLineCount,fileEmptyLineCount);
        }
    }
}