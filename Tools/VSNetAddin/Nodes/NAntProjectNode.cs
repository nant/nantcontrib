//
// NAntContrib - NAntAddin
// Copyright (C) 2002 Jayme C. Edwards (jedwards@wi.rr.com)
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

using EnvDTE;
using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing.Design;
using NAnt.Contrib.NAntAddin.Controls;

namespace NAnt.Contrib.NAntAddin.Nodes {
    /// <summary>
    /// Delegate that defines Reload so it can be called 
    /// when the Project File changes outside the IDE.
    /// </summary>
    /// <remarks>None.</remarks>
    internal delegate void ReloadDelegate();

    /// <summary>
    /// Tree Node that represents an NAnt "project" element.
    /// </summary>
    /// <remarks>None.</remarks>
    [Category("NAnt Project")]
    public class NAntProjectNode : NAntBaseNode {
        private bool available = true;
        private bool reloading = false;
        private bool readOnly = false;
        private bool saving = false;
        private bool closing = false;

        internal Addin addin;
        internal Project project;
        private ProjectItem projectItem;
        private FileSystemWatcher projectFileWatcher;
        private XmlDocument projectDocument = new XmlDocument();

        internal object changing = new object();

        /// <summary>
        /// Creates a new <see cref="NAntProjectNode"/>.
        /// </summary>
        /// <param name="Addin">The NAnt Addin.</param>
        /// <param name="NAntScriptProject">A VS.NET Project containing 
        /// a Project Item that is an NAnt build script</param>
        /// <param name="NAntScriptProjectItem">A VS.NET Project Item 
        /// representing a file that is an NAnt build script</param>
        /// <remarks>None.</remarks>
        public NAntProjectNode(Addin Addin, Project NAntScriptProject, ProjectItem NAntScriptProjectItem) : base(null) {
            addin = Addin;
            project = NAntScriptProject;
            projectItem = NAntScriptProjectItem;

            Uri projectFileUri = new Uri(FileName, true);
            string[] projectFileSegments = projectFileUri.Segments;

            StringBuilder basePath = new StringBuilder();
            for (int i = 0; i < projectFileSegments.Length-1; i++) {
                basePath.Append(projectFileSegments[i]);
            }

            projectFileWatcher = new FileSystemWatcher(basePath.ToString(), 
                projectFileSegments[projectFileSegments.Length-1]);

            projectFileWatcher.Changed += new FileSystemEventHandler(ProjectFile_Changed);

            projectFileWatcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Reloads the Name when a new Target has been added/pasted or removed.
        /// </summary>
        internal void ReloadName() {
            if (Available) {
                XmlNodeList targetElems = projectDocument.SelectNodes("/project/target");
                int targetCount = targetElems.Count;

                StringBuilder projectText = new StringBuilder();
                projectText.Append("Project '");
                projectText.Append((Name == null || Name == "") ? projectItem.Name : Name);
                projectText.Append("' (");
                projectText.Append(targetCount);
                projectText.Append(" targets");
                if (ReadOnly) {
                    projectText.Append(", read only");
                }
                projectText.Append(")");

                Text = projectText.ToString();
            }
        }

        /// <summary>
        /// Reloads the NAnt Project when an external change has ocurred.
        /// </summary>
        /// <remarks>None.</remarks>
        public void Reload() {
            Monitor.Enter(changing);

            reloading = true;

            Nodes.Clear();

            //28
            try {
                FileAttributes projectFileAttr = File.GetAttributes(FileName);
                if ((projectFileAttr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                    ImageIndex = 5;
                    SelectedImageIndex = 5;
                    readOnly = true;
                } else {
                    readOnly = false;
                }
            } catch (Exception) {
            }

            try {
                projectDocument.Load(FileName);

                ReloadName();

                TreeNode dummyNode = new TreeNode(ScriptExplorerControl.DUMMY_NODE);
                Nodes.Add(dummyNode);

                available = true;

                if (!readOnly) {
                    ImageIndex = 0;
                    SelectedImageIndex = 0;
                }
            } catch (Exception e) {
                Text = projectItem.Name + " - (" + e.Message + ")";

                available = false;

                ImageIndex = 4;
                SelectedImageIndex = 4;
            }

            reloading = false;
            closing = false;

            if (TreeView != null) {
                if (TreeView.SelectedNode == this) {
                    Object[] refObjs = new Object[] { this };
                    if (ReadOnly || !Available) {
                        //refObjs = new Object[] { new NAntReadOnlyNode(this) };
                    }
                    addin.scriptExplorerWindow.SetSelectionContainer(ref refObjs);
                    addin.scriptExplorer.scriptsTree_AfterSelect(
                        this, new TreeViewEventArgs(this));
                }
            }

            Monitor.Exit(changing);
        }

        /// <summary>
        /// Gets whether the Project's ProjectItem is being closed.
        /// </summary>
        /// <value>Whether the Project's ProjectItem is being closed.</value>
        /// <remarks>None.</remarks>
        [Browsable(false)]
        public bool Closing {
            get {
                return closing;
            }
            set {
                closing = value;
            }
        }

        /// <summary>
        /// Gets whether the Project loaded.
        /// </summary>
        /// <value>Whether the Project loaded.</value>
        /// <remarks>None.</remarks>
        [Browsable(false)]
        public bool Available {
            get {
                return available;
            }
        }

        /// <summary>
        /// Gets whether the Project is read only.
        /// </summary>
        /// <value>Whether the Project is read only.</value>
        /// <remarks>None.</remarks>
        [Browsable(false)]
        public bool ReadOnly {
            get {
                return readOnly;
            }
        }

        /// <summary>
        /// Gets the VS.NET Project Item representing 
        /// a file that is an NAnt build script.
        /// </summary>
        /// <value>The VS.NET Project Item representing 
        /// a file that is an NAnt build script.</value>
        /// <remarks>None.</remarks>
        [Browsable(false)]
        public XmlDocument ProjectDocument {
            get {
                return projectDocument;
            }
        }

        /// <summary>
        /// Gets the VS.NET Project containing 
        /// a Project Item that is an NAnt build script.
        /// </summary>
        /// <value>The VS.NET Project containing 
        /// a Project Item that is an NAnt build script.</value>
        /// <remarks>None.</remarks>
        [Browsable(false)]
        public Project Project {
            get {
                return project;
            }
        }

        /// <summary>
        /// Gets the VS.NET ProjectItem 
        /// that is an NAnt build script.
        /// </summary>
        /// <value>The VS.NET ProjectItem 
        /// that is an NAnt build script.</value>
        /// <remarks>None.</remarks>
        [Browsable(false)]
        public ProjectItem ProjectItem {
            get {
                return projectItem;
            }
        }

        /// <summary>
        /// Gets or sets the name of the Project.
        /// </summary>
        /// <value>The name of the Project.</value>
        /// <remarks>None.</remarks>
        [Description("The name of the Project."),Category("Appearance")]
        public string Name {
            get {
                return ProjectDocument.DocumentElement.GetAttribute("name");
            }
            set {
                if (value == "") {
                    ProjectDocument.DocumentElement.RemoveAttribute("name");
                } else {
                    ProjectDocument.DocumentElement.SetAttribute("name", value);
                }

                Save();

                ReloadName();
            }
        }

        /// <summary>
        /// Gets or sets a description of the Project.
        /// </summary>
        /// <value>A description of the Project.</value>
        /// <remarks>None.</remarks>
        [Description("A description of the Project"),Category("Appearance")]
        public string Description {
            get {
                XmlElement descriptionElem = (XmlElement)ProjectDocument.DocumentElement.SelectSingleNode("description");
                if (descriptionElem != null) {
                    return descriptionElem.InnerText;
                }
                return null;
            }

            set {
                XmlElement descriptionElem = (XmlElement)ProjectDocument.DocumentElement.SelectSingleNode("description");
                if (descriptionElem != null) {
                    if (value == null || value == "") {
                        ProjectDocument.DocumentElement.RemoveChild(descriptionElem);
                        Save();
                    } else {
                        descriptionElem.InnerText = value;
                        Save();
                    }
                } else {
                    if (value != null && value != "") {
                        descriptionElem = ProjectDocument.CreateElement("description");
                        descriptionElem.InnerText = value;

                        XmlNodeList childNodes = ProjectDocument.DocumentElement.ChildNodes;
                        XmlElement firstChild = null;
                        for (int i = 0; i < childNodes.Count; i++) {
                            if (childNodes[i].NodeType == XmlNodeType.Element) {
                                firstChild = (XmlElement)childNodes[i];
                                break;
                            }
                        }

                        if (firstChild != null) {
                            ProjectDocument.DocumentElement.InsertBefore(descriptionElem, firstChild);
                            Save();
                        } else {
                            ProjectDocument.DocumentElement.AppendChild(descriptionElem);
                            Save();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the target executed at build time if no other is specified.
        /// </summary>
        /// <value>The target executed at build time if no other is specified.</value>
        /// <remarks>None.</remarks>
        [Description("The Target executed at build time if no other is specified."),Category("Behavior")]
        public string DefaultTarget {
            get {
                return ProjectDocument.DocumentElement.GetAttribute("default");
            }
            set {
                if (value == "") {
                    ProjectDocument.DocumentElement.RemoveAttribute("default");
                } else {
                    ProjectDocument.DocumentElement.SetAttribute("default", value);
                }
                Save();
            }
        }

        /// <summary>
        /// Gets or sets the base filesystem path from which to resolve resources.
        /// </summary>
        /// <value>The base filesystem path from which to resolve resources.</value>
        /// <remarks>None.</remarks>
        [Description("The base filesystem path from which to resolve resources."),Category("Data")]
        public string BaseDir {
            get {
                return ProjectDocument.DocumentElement.GetAttribute("basedir");
            }
            set {
                if (value == "") {
                    ProjectDocument.DocumentElement.RemoveAttribute("basedir");
                } else {
                    ProjectDocument.DocumentElement.SetAttribute("basedir", value);
                }
                Save();
            }
        }

        /// <summary>
        /// Gets or sets whether detailed messages should be logged to the output window during builds.
        /// </summary>
        /// <value>Whether detailed messages should be logged to the output window during builds.</value>
        /// <remarks>None.</remarks>
        [Description("If detailed messages should be logged to the output window during builds."),Category("Behavior")]
        public bool Verbose {
            get {
                string verbose = ProjectDocument.DocumentElement.GetAttribute("verbose");
                return verbose == "true";
            }
            set {
                ProjectDocument.DocumentElement.SetAttribute(
                    "verbose", value == true ? "true" : "false");
                Save();
            }
        }

        /// <summary>
        /// Gets or sets the filesystem path of the NAnt build file.
        /// </summary>
        /// <value>The filesystem path of the NAnt build file.</value>
        /// <remarks>None.</remarks>
        [Description("The filesystem path of the NAnt build file.")]
        public string FileName {
            get {
                return (string)projectItem.Properties.Item("FullPath").Value;
            }
        }

        /// <summary>
        /// Occurs when the project file is changed outside the IDE.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Arguments passed to the event</param>
        private void ProjectFile_Changed(object sender, FileSystemEventArgs e) {
            if (e.ChangeType == WatcherChangeTypes.Changed) {
                if (!reloading && !saving && !closing) {
                    if (TreeView.IsHandleCreated) {
                        //TreeView.Invoke(new ReloadDelegate(Reload), new object[0]);
                    }
                }
            }
        }

        /// <summary>
        /// Saves the NAnt Project.
        /// </summary>
        /// <remarks>None.</remarks>
        public void Save() {
            Monitor.Enter(changing);

            saving = true;

            try {
                projectDocument.Save(FileName);
            } catch (Exception e) {
                MessageBox.Show("Error Saving Changes to NAnt Script\n\n" + 
                    FileName + e.Message, "Error Saving NAnt Script", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            saving = false;

            Monitor.Exit(changing);
        }
    }
}
