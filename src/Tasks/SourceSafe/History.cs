#region GNU General Public License 
//
// NAntContrib
// Copyright (C) 2001-2004 Gerry Shaw
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
// Jay Vilalta (jvilalta@users.sourceforge.net)
//
#endregion

using System;
using System.Globalization;
using System.IO;
using System.Xml;

using SourceSafeTypeLib;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks.SourceSafe {
    /// <summary>
    /// Generates an XML file showing all changes made to a Visual SourceSafe
    /// project/file between specified labels or dates (by a given user).
    /// </summary>
    /// <example>
    ///   <para>
    ///   Write all changes between "Release1" and "Release2" to XML file 
    ///   "changelog.xml".
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <vsshistory
    ///     dbpath="C:\VSS\srcsafe.ini"
    ///     path="$/My Project"
    ///     fromlabel="Release1"
    ///     tolabel="Release2"
    ///     output="changelog.xml" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Write all changes between January 1st 2004 and March 31st 2004 to XML 
    ///   file "history.xml".
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <vsshistory
    ///     dbpath="C:\VSS\srcsafe.ini"
    ///     path="$/My Project"
    ///     fromdate="01/01/2004"
    ///     todate="03/31/2004"
    ///     output="history.xml"
    ///     />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("vsshistory")]
    public class History : BaseTask {
        #region Private Instance Methods

        private string _toLabel;
        private string _fromLabel;
        private DateTime _toDate;
        private DateTime _fromDate;
        private bool _recursive = true;
        private string _user;
        private FileInfo _output;
        private XmlDocument _outputDoc;

        #endregion Private Instance Methods

        #region Public Instance Properties

        /// <summary>
        /// The value of the label to start comparing to. If it is not included, 
        /// the compare will start with the very first history item.
        /// </summary>
        [TaskAttribute("fromlabel", Required=false)]
        public string FromLabel {
            get { return _fromLabel; }
            set { _fromLabel = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The value of the label to compare up to. If it is not included,
        /// the compare will end with the last history item.
        /// </summary>
        [TaskAttribute("tolabel", Required=false)]
        public string ToLabel {
            get { return _toLabel; }
            set { _toLabel = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Start date for comparison.
        /// </summary>
        [TaskAttribute("fromdate", Required=false)]
        public DateTime FromDate {
            get { return _fromDate; }
            set { _fromDate = value; }
        }

        /// <summary>
        /// End date for comparison.
        /// </summary>
        [TaskAttribute("todate", Required=false)]
        public DateTime ToDate {
            get { return _toDate; }
            set { _toDate = value; }
        }

        /// <summary>
        /// Output file to save history to (as XML).
        /// </summary>
        [TaskAttribute("output", Required=true)]
        public FileInfo Output {
            get { return _output; }
            set { _output = value; }
        }

        /// <summary>
        /// Determines whether to perform the comparison recursively.
        /// The default is <see langword="true" />.
        /// </summary>
        [TaskAttribute("recursive", Required=false)]
        public bool Recursive {
            get { return _recursive; }
            set { _recursive = value; }
        }

        /// <summary>
        /// Name of the user whose changes you want to see.
        /// </summary>
        [TaskAttribute("user", Required=false)]
        public string User {
            get { return _user; }
            set { _user = StringUtils.ConvertEmptyToNull(value); }
        }

        #endregion Public Instance Properties

        #region Private Instance Properties

        /// <summary>
        /// Gets the flags that should be used to retrieve the history of
        /// <see cref="Path" />.
        /// </summary>
        private int VersionFlags {
            get {
                int versionFlags = Recursive ? (int) VSSFlags.VSSFLAG_RECURSYES
                    : (int) VSSFlags.VSSFLAG_RECURSNO;
                return versionFlags;
            }
        }

        #endregion Private Instance Properties

        #region Override implementation of BaseTask

        /// <summary>
        /// Override to avoid exposing the corresponding attribute to build 
        /// authors.
        /// </summary>
        [Obsolete("Use \"username\" attribute instead.", false)]
        public override string Login { 
            get { return base.Login; }
            set { base.Login = value; }
        }

        /// <summary>
        /// Override to avoid exposing the corresponding attribute to build 
        /// authors.
        /// </summary>
        public override string Version {
            get { return base.Version; }
            set { base.Version = value; }
        }

        #endregion Override implementation of BaseTask

        #region Override implementation of Task

        protected override void ExecuteTask() {
            Open();

            try {
                Log(Level.Info, "Examining \"{0}\"...", this.Path);

                //Setup the XmlOutput File
                _outputDoc = new XmlDocument();
                XmlElement root = _outputDoc.CreateElement("VssHistory");
                XmlAttribute attrib = _outputDoc.CreateAttribute("FromLabel");
                attrib.Value = _fromLabel;
                root.Attributes.Append(attrib);

                attrib = _outputDoc.CreateAttribute("ToLabel");
                attrib.Value = _toLabel;
                root.Attributes.Append(attrib);

                attrib = _outputDoc.CreateAttribute("FromDate");
                if (FromDate != DateTime.MinValue) {
                    attrib.Value = XmlConvert.ToString(FromDate);
                }
                root.Attributes.Append(attrib);

                attrib = _outputDoc.CreateAttribute("ToDate");
                if (ToDate != DateTime.MinValue) {
                    attrib.Value = XmlConvert.ToString(ToDate);
                }
                root.Attributes.Append(attrib);

                attrib = _outputDoc.CreateAttribute("Path");
                attrib.Value = this.Path;
                root.Attributes.Append(attrib);

                attrib = _outputDoc.CreateAttribute("Recursive");
                attrib.Value = XmlConvert.ToString(this.Recursive);
                root.Attributes.Append(attrib);

                attrib = _outputDoc.CreateAttribute("User");
                attrib.Value = this.User;
                root.Attributes.Append(attrib);

                attrib = _outputDoc.CreateAttribute("Generated");
                attrib.Value = XmlConvert.ToString(DateTime.Now);
                root.Attributes.Append(attrib);

                _outputDoc.AppendChild(root);

                ItemDiff(Item);

                _outputDoc.Save(Output.FullName);
            } catch (Exception ex) {
                throw new BuildException("diff failed", Location, ex);
            }

            Log(Level.Info, "Diff File generated: " + Output.FullName);
        }

        #endregion Override implementation of Task

        #region Private Instance Methods

        private void ItemDiff(IVSSItem ssItem) {
            Log(Level.Verbose, "History of \"{0}\"...", ssItem.Name);

            if (FromLabel != null || ToLabel != null) {
                DiffByLabel(ssItem);
            } else {
                DiffByDate(ssItem);
            }
        }

        private void DiffByDate(IVSSItem ssItem) {
            bool startLogging = false;
            bool stopLogging = false;

            string user = User != null ? User.ToLower(CultureInfo.InvariantCulture)
                : null;

            foreach (IVSSVersion version in ssItem.get_Versions(VersionFlags)) {
                // VSS returns the versions in descending order, meaning the
                // most recent versions appear first.
                if (ToDate == DateTime.MinValue || version.Date <= ToDate) {
                    startLogging = true;
                }
                if (FromDate != DateTime.MinValue && FromDate > version.Date) {
                    stopLogging = true;
                }
                if (startLogging && !stopLogging) {
                    // if user was specified, then skip changes that were not 
                    // performed by that user
                    if (user != null && version.Username.ToLower(CultureInfo.InvariantCulture) != user) {
                        continue;
                    }

                    LogChange(version);
                }
            }
        }

        private void DiffByLabel(IVSSItem ssItem){
            bool startLogging = false;
            bool stopLogging = false;

            string user = User != null ? User.ToLower(CultureInfo.InvariantCulture)
                : null;

            foreach (IVSSVersion version in ssItem.get_Versions(VersionFlags)) {
                // VSS returns the versions in descending order, meaning the
                // most recent versions appear first.
                if (ToLabel == null || version.Action.StartsWith(string.Format("Labeled '{0}'", ToLabel))){
                    startLogging = true;
                }
                if (FromLabel != null && version.Action.StartsWith(string.Format("Labeled '{0}'", FromLabel))) {
                    stopLogging = true;
                }
                if (startLogging && !stopLogging) {
                    // if user was specified, then skip changes that were not 
                    // performed by that user
                    if (user != null && version.Username.ToLower(CultureInfo.InvariantCulture) != user) {
                        continue;
                    }

                    LogChange(version);
                }
            }
        }

        private void LogChange(IVSSVersion version) {
            const int FILE_OR_PROJECT_DOES_NOT_EXIST = -2147166577;

            XmlElement node;
            XmlAttribute attrib;

            try {
                node = _outputDoc.CreateElement("Entry");
                attrib = _outputDoc.CreateAttribute("Name");
                attrib.Value = version.VSSItem.Name;
                node.Attributes.Append(attrib);

                attrib = _outputDoc.CreateAttribute("Path");
                attrib.Value = version.VSSItem.Spec;
                node.Attributes.Append(attrib);
            } catch (System.Runtime.InteropServices.COMException ex) {
                if (ex.ErrorCode != FILE_OR_PROJECT_DOES_NOT_EXIST) {
                    throw ex;
                }

                return;
            }

            attrib = _outputDoc.CreateAttribute("Action");
            attrib.Value = version.Action;
            node.Attributes.Append(attrib);

            attrib = _outputDoc.CreateAttribute("Date");
            attrib.Value = XmlConvert.ToString(version.Date);
            node.Attributes.Append(attrib);

            attrib = _outputDoc.CreateAttribute("Version");
            attrib.Value = XmlConvert.ToString(version.VersionNumber);
            node.Attributes.Append(attrib);    

            attrib = _outputDoc.CreateAttribute("User");
            attrib.Value = version.Username;
            node.Attributes.Append(attrib);

            attrib = _outputDoc.CreateAttribute("Comment");
            attrib.Value = version.Comment;
            node.Attributes.Append(attrib);

            attrib = _outputDoc.CreateAttribute("Label");
            attrib.Value = version.Label;
            node.Attributes.Append(attrib);

            attrib = _outputDoc.CreateAttribute("LabelComment");
            attrib.Value = version.LabelComment;
            node.Attributes.Append(attrib);

            _outputDoc.ChildNodes.Item(0).AppendChild(node);
        }

        #endregion Private Instance Methods
    }
}
