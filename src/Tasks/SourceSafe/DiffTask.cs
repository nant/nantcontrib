#region GNU General Public License
//
// NAntContrib
// Copyright (C) 2003 Brant Carter
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
// Brant Carter (brantcarter@hotmail.com)
#endregion

using System;
using System.IO;
using System.Xml;

using SourceSafeTypeLib;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.SourceSafe {
    /// <summary>
    /// Used to generate differences in a vss database. It will show all changes to a project
    /// after the specified label.
    /// </summary>
    /// <remarks>
    /// This only shows differences between the current version and the version specified.
    /// </remarks>
    /// <example>
    ///   <code><![CDATA[
    ///            <vssdiff
    ///                dbpath='ss.ini'
    ///                path='$/My Project'
    ///                label='My Label'
    ///                user='ssuser'
    ///                outputfile='diff.xml'
    ///            />
    ///   ]]></code>
    /// </example>
    [TaskName("vssdiff")]
    public class DiffTask : BaseTask {
        string _label = string.Empty;
        FileInfo _outputFile;
        XmlDocument _outputDoc;

        /// <summary>
        /// The value of the label to compare to. Required.
        /// </summary>
        [TaskAttribute("label", Required=true)]
        public string Label {
            get { return _label; }
            set { _label = value; }
        }

        /// <summary>
        /// The output file to generate (xml)
        /// </summary>
        [TaskAttribute("outputfile", Required=true)]
        public FileInfo OutputFile {
            get { return _outputFile; }
            set { _outputFile = value; }
        }

        protected override void ExecuteTask() {
            Open();
            try {
                Log(Level.Info, LogPrefix + "Examining: " + this.Path);

                //Setup the XmlOutput File
                _outputDoc = new XmlDocument();
                XmlElement root = _outputDoc.CreateElement("vssdiff");
                XmlAttribute attrib = _outputDoc.CreateAttribute ("label");
                attrib.Value = Label;
                root.Attributes.Append (attrib);
                attrib = _outputDoc.CreateAttribute ("generated");
                attrib.Value = System.DateTime.Now.ToString();
                root.Attributes.Append (attrib);
                attrib = _outputDoc.CreateAttribute ("project");
                attrib.Value = this.Path;
                root.Attributes.Append (attrib);

                _outputDoc.AppendChild (root);

                //Start the recursive search
                ProjectDiff(this.Path);
                _outputDoc.Save(OutputFile.FullName);
            } catch (Exception ex) {
                throw new BuildException("diff failed", Location, ex);
            }

            Log(Level.Info, LogPrefix + "Diff File generated: " + _outputFile);
        }

        protected void ItemDiff(IVSSItem ssItem) {
            if (this.Verbose) {
                Log(Level.Info, LogPrefix + "Processing item " + ssItem.Name );
            }
            bool addVersion = true;
            foreach (IVSSVersion version in ssItem.get_Versions(0)) {
                // VSS returns the versions in descending order, meaning the
                // most recent versions appear first.
                string action = version.Action;

                // We found our version so stop adding versions to our list
                if (action.StartsWith ("Labeled '" + _label + "'")) {
                    addVersion = false;
                    //This is a bit annoying, it would be more efficient to break
                    //out of the loop here but VSS throws an exception !%?!
                    //http://tinyurl.com/nmct
                    //break;
                }
                if (addVersion) {
                    // Only add versions that have been added, created or checked in.  Ignore label actions.
                    if( (action.StartsWith("Add")) || (action.StartsWith("Create")) || (action.StartsWith("Check") )) {
                        if (this.Verbose) {
                            Log(Level.Info, LogPrefix + "Adding: " + version.VSSItem.Name);
                        }

                        // Build our XML Element with hopefully useful information.
                        XmlElement node = _outputDoc.CreateElement ("item");
                        XmlAttribute attrib = _outputDoc.CreateAttribute ("name");
                        attrib.Value= version.VSSItem.Name;
                        node.Attributes.Append (attrib);

                        attrib = _outputDoc.CreateAttribute ("path");
                        attrib.Value= version.VSSItem.Spec;
                        node.Attributes.Append (attrib);

                        attrib = _outputDoc.CreateAttribute ("action");
                        attrib.Value= action;
                        node.Attributes.Append (attrib);

                        attrib = _outputDoc.CreateAttribute ("date");
                        attrib.Value= version.Date.ToString();
                        node.Attributes.Append (attrib);

                        attrib = _outputDoc.CreateAttribute ("version");
                        attrib.Value= version.VersionNumber.ToString() ;
                        node.Attributes.Append (attrib);

                        attrib = _outputDoc.CreateAttribute ("user");
                        attrib.Value= version.Username  ;
                        node.Attributes.Append (attrib);

                        attrib = _outputDoc.CreateAttribute ("comment");
                        attrib.Value= version.Comment  ;
                        node.Attributes.Append (attrib);

                        _outputDoc.ChildNodes.Item(0).AppendChild (node);
                    }
                }
            }
        }
        protected void ProjectDiff(string Project) {
            // Recursively loop through our vss projects
            if (this.Verbose) {
                Log(Level.Info, LogPrefix + "Processing project " + Project);
            }
            IVSSItem ssProj = this.Database.get_VSSItem(Project,false);
            IVSSItems ssSubItems = ssProj.get_Items(false);
            foreach (IVSSItem subItem in ssSubItems) {
                if (subItem.Type == 0) {
                    //Type=0 is a Project
                    ProjectDiff(Project + "/" + subItem.Name);
                } else {
                    ItemDiff(subItem);
                }
            }
        }
    }
}
