#region GNU General Public License
//
// NAntContrib
// Copyright (C) 2001-2003 Gerry Shaw
//
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
// Paul Francis, Edenbrook. (paul.francis@edenbrook.co.uk)

#endregion

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Xml;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.Mks {
    /// <summary>
    /// Generates an XML file containing the differences between the sandbox and
    /// the project in the MKS database.
    /// </summary>
    /// <example>
    ///   <para>Get changes to a project in an MKS database.</para>
    ///   <code><![CDATA[
    ///     <mkschanges
    ///       username="myusername"
    ///       password="mypassword"
    ///       host="servername"
    ///       port="123"
    ///       sandbox="mysandbox.pj"
    ///       output="mychanges.xml"
    ///     />
    ///   ]]></code>
    /// </example>
    [TaskName("mkschanges")]
    public sealed class ChangesTask : BaseTask {
        #region Private Instance Fields

        private string _sandbox;
        private FileInfo _outputFile;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The project to retrieve the changes for.
        /// </summary>
        [TaskAttribute("sandbox", Required=true)]
        public string Sandbox {
            get { return _sandbox; }
            set { _sandbox = value; }
        }
    
        /// <summary>
        /// The file where the output will be stored in XML format.
        /// </summary>
        [TaskAttribute("output", Required=true)]
        public FileInfo OutputFile {
            get { return _outputFile; }
            set { _outputFile = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        protected override void ExecuteTask() {
            ArrayList memberChanges = GetChanges();

            //now we have the revison history serialise it to XML

            try {
                XmlDocument doc = new XmlDocument();
                XmlNode root = doc.CreateElement("changes");
                doc.AppendChild(root);

                if (memberChanges!=null) {
                    foreach (ChangeHistory item in memberChanges) {
                        XmlElement member = doc.CreateElement("member");

                        XmlElement name = doc.CreateElement("name");
                        name.InnerText=item.MemberName;

                        XmlElement version = doc.CreateElement("version");
                        version.InnerText=item.MemberRevision;

                        member.AppendChild(name);
                        member.AppendChild(version);
                        root.AppendChild(member);

                        foreach (HistoryItem hi in item.RevisionHistory) {
                            XmlElement revision = doc.CreateElement("revision");

                            XmlElement author = doc.CreateElement("author");
                            author.InnerText=hi.Author;
                            revision.AppendChild(author);

                            XmlElement description = doc.CreateElement("description");
                            description.InnerText=hi.Description;
                            revision.AppendChild(description);

                            XmlElement revisionNumber = doc.CreateElement("version");
                            revisionNumber.InnerText=hi.RevisionNumber;
                            revision.AppendChild(revisionNumber);

                            XmlElement revisionDate = doc.CreateElement("date");
                            revisionDate.InnerText=hi.RevisionDate.ToString("s"); //use iso8601 
                            revision.AppendChild(revisionDate);

                            member.AppendChild(revision);
                        }
                    }
                }
                doc.Save(OutputFile.FullName);
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Could not create history file \"{0}\".", OutputFile.FullName),
                    Location, ex);
            }
        }

        #endregion Override implementation of Task

        #region Public Instance Methods

        public ArrayList GetChanges() {
            Open();

            try {
                string cmd = "rlog  --filter=changed --headerFormat='¬' --trailerformat='' --format=¦au^{author}~dt^{date}~de^{description}~mn^{membername}~mr^{memberrev}~rn^{revision} -S '" + _sandbox + "'";

                string output = MKSExecute(cmd);
                if (output != "") {
                    ArrayList memberFiles = new ArrayList();
                    HistoryItem hi = new HistoryItem();
                    ChangeHistory memberHistory=new ChangeHistory();

                    string[] memberArray = output.Split("¬".ToCharArray());

                    for (int x=1; x<memberArray.Length; x++) {
                        string memberLine = memberArray[x];

                        memberHistory = new ChangeHistory();
                        memberFiles.Add(memberHistory);

                        string[] revisionArray = memberLine.Split("¦".ToCharArray());
                        for(int y=1; y < revisionArray.Length; y++) {
                            hi = new HistoryItem();
                            memberHistory.RevisionHistory.Add(hi);

                            string revisionLine = revisionArray[y];

                            string[] buff = revisionLine.Split("~".ToCharArray());

                            for (int i=0;i<buff.Length;i++) {
                                string[] temp = buff[i].Split("^".ToCharArray());

                                if (temp.Length > 1) {
                                    switch (temp[0]) {
                                        case "au":
                                            hi.Author = temp[1];
                                            break;
                                        case "dt":
                                            hi.RevisionDate = Convert.ToDateTime(temp[1]);
                                            break;
                                        case "de":
                                            hi.Description = temp[1].Replace("\n","");
                                            break;
                                        case "rn":
                                            hi.RevisionNumber=temp[1];
                                            break;
                                        case "mn":
                                            memberHistory.MemberName=temp[1];
                                            break;
                                        case "mr":
                                            memberHistory.MemberRevision=temp[1];
                                            break;
                                    }
                                }
                            }
                        }
                    }

                    return memberFiles;
                } else {
                    return null;
                }
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Could not get history of project \"{0}\".", Sandbox), 
                    Location, ex);
            }
        }

        #endregion Public Instance Methods

        public class HistoryItemCollection: CollectionBase {
            public Int32 ItemCount() {
                return List.Count;
            }

            public HistoryItem Item(int Index) {
                return (HistoryItem)List[Index];
            }

            public int Add(HistoryItem item) {
                return List.Add(item);
            }
        }

        public class ChangeHistory {
            #region Private Instance Fields

            private string _memberName = string.Empty;
            private string _memberRevision = string.Empty;
            private HistoryItemCollection _revisionHistory = new HistoryItemCollection();

            #endregion Private Instance Fields

            #region Public Instance Constructors

            public ChangeHistory() {
            }

            #endregion Public Instance Constructors

            #region Public Instance Properties

            public string MemberName {
                get { return _memberName; }
                set { _memberName = value; }
            }

            public string MemberRevision {
                get { return _memberRevision; }
                set { _memberRevision = value; }
            }

            public HistoryItemCollection RevisionHistory {
                get { return _revisionHistory; }
                set { _revisionHistory = value; }
            }

            #endregion Public Instance Properties
        }

        public class HistoryItem {
            #region Private Instance Fields

            private string _author = string.Empty;
            private DateTime _revisionDate;
            private string _description = string.Empty;
            private string _revisionNumber = string.Empty;

            #endregion Private Instance Fields

            #region Public Instance Constructors

            public HistoryItem() {
            }

            #endregion Public Instance Constructors

            #region Public Instance Properties

            public string Author {
                get { return _author; }
                set { _author = value; }
            }

            public string Description {
                get { return _description; }
                set { _description = value; }
            }

            public DateTime RevisionDate {
                get { return _revisionDate; }
                set { _revisionDate = value; }
            }

            public string RevisionNumber {
                get { return _revisionNumber; }
                set { _revisionNumber = value; }
            }

            #endregion Public Instance Properties
        }
    }
}
