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

using System;
using System.Xml;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using NAnt.Contrib.NAntAddin.Nodes;
using NAnt.Contrib.NAntAddin.Dialogs;
using NAnt.Contrib.NAntAddin.Controls;

namespace NAnt.Contrib.NAntAddin.Controls {
    /// <summary>
    /// Control that provides the Script Explorer Window.
    /// </summary>
    /// <remarks>None.</remarks>
    public class ScriptExplorerControl : UserControl {
        private Addin addin;

        private VSNetToolbarDesigner toolbarMain;
        private ToolBarButton buttonRefresh;
        private ToolBarButton menSepRefresh;
        private ToolBarButton menSep1;
        private ToolBarButton buttonAddProperty;
        private ToolBarButton buttonAddTarget;
        private ToolBarButton buttonAddTask;
        private ToolBarButton menSep2;
        private ToolBarButton buttonAbout;
        private ToolBarButton buttonBuild;
        private ToolBarButton buttonViewCode;
        private ToolBarButton menSep3;
        private ToolBarButton buttonMoveUp;
        private ToolBarButton buttonMoveDown;
        private AboutDialog aboutDialog = new AboutDialog();
        private Panel panel1;
        private Panel panel2;

        private const string HELP_PREFIX = "ms-help://MS.VSCC/NAntAddin/NAntAddin/html/";

        internal TreeNode editedNode;
        internal TreeView scriptsTree;
        internal ImageList treeImages;
        internal ImageList buttonImages;
        internal TaskNodesTable tasksTable;
        internal VSNetToolbarControl vsNetToolbar;
        internal ClipboardManager clipboardManager;

        internal const string DUMMY_NODE = "<dummy-nant-addin-tree-node>";

        /// <summary>
        /// Creates a new <see cref="ScriptExplorerControl"/>.
        /// </summary>
        /// <remarks>None.</remarks>
        public ScriptExplorerControl() {
            InitializeComponent();

            tasksTable = new TaskNodesTable(treeImages);
            clipboardManager = new ClipboardManager(tasksTable);
        }

        /// <summary>
        /// Sets or returns the <see cref="NAnt.Contrib.NAntAddin.Addin"/> hosting this Control.
        /// </summary>
        /// <value>The <see cref="NAnt.Contrib.NAntAddin.Addin"/> hosting this Control</value>
        /// <remarks>None.</remarks>
        public Addin Addin {
            get {
                return addin;
            }

            set {
                addin = value;
            }
        }

        /// <summary>
        /// Occurs before a TreeNode is Expanded.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Arguments passed to the event</param>
        internal void scriptsTree_BeforeExpand(object sender, TreeViewCancelEventArgs e) {
            if (e.Node is NAntProjectNode) {
                NAntProjectNode projectNode = (NAntProjectNode)e.Node;

                if (projectNode.ProjectItem.Document != null) {
                    DialogResult result = MessageBox.Show("The NAnt script:\n\n" + 
                        projectNode.Text + 
                        "\n\nIs open in Visual Studio.NET. To use the NAnt Addin " + 
                        "to edit this script\n" +
                        "you must first close the file.\n\nDo you want to close " + 
                        "the file:\n\n " + projectNode.FileName + "?", 
                        "Close NAnt Script Source Code",
                        MessageBoxButtons.YesNo, 
                        MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                    if (result == DialogResult.Yes) {
                        projectNode.Closing = true;

                        projectNode.ProjectItem.Document.Close(
                            EnvDTE.vsSaveChanges.vsSaveChangesPrompt);
                        if (projectNode.ProjectItem.Document != null) {
                            e.Cancel = true;
                            projectNode.Closing = false;
                            return;
                        }
                        else {
                            projectNode.Reload();
                        }
                    }
                    else {
                        e.Cancel = true;
                        return;
                    }
                }
            }

            bool expand = false;

            for (int x = 0; x < e.Node.Nodes.Count; x++) {
                if (e.Node.Nodes[x].Text.Equals(DUMMY_NODE)) {
                    expand = true;
                    break;
                }
            }

            if (expand) {
                e.Node.Nodes.Clear();

                if (e.Node is NAntProjectNode) { // "project" element
                    NAntProjectNode projectNode = (NAntProjectNode)e.Node;

                    //
                    // "property" nodes
                    //
                    XmlNodeList propertyNodes = projectNode.ProjectDocument.DocumentElement.SelectNodes("property");
                    for (int i = 0; i < propertyNodes.Count; i++) {
                        XmlElement propertyElem = (XmlElement)propertyNodes[i];
                        NAntPropertyNode propNode = new NAntPropertyNode(propertyElem, 
                            (XmlElement)propertyElem.ParentNode);
                        propNode.NodeFont = new System.Drawing.Font(
                            scriptsTree.Font, System.Drawing.FontStyle.Regular);
                        projectNode.Nodes.Add(propNode);
                    }

                    //
                    // "target" nodes
                    //
                    XmlNodeList targetNodes = projectNode.ProjectDocument.DocumentElement.SelectNodes("target");
                    for (int i = 0; i < targetNodes.Count; i++) {
                        XmlElement targetElem = (XmlElement)targetNodes[i];
                        NAntTargetNode targetNode = new NAntTargetNode(targetElem, 
                            (XmlElement)targetElem.ParentNode);

                        if (targetNode.Name != projectNode.DefaultTarget) {
                            targetNode.NodeFont = new System.Drawing.Font(
                                scriptsTree.Font, System.Drawing.FontStyle.Regular);
                        }

                        projectNode.Nodes.Add(targetNode);
                    }
                }
                else if (e.Node is NAntTargetNode) { // "target" element
                    NAntTargetNode targetNode = (NAntTargetNode)e.Node;

                    //
                    // task nodes
                    //
                    XmlNodeList taskNodes = targetNode.TargetElement.ChildNodes;
                    for (int i = 0; i < taskNodes.Count; i++) {
                        if (taskNodes[i].NodeType == XmlNodeType.Element) {
                            XmlElement taskElem = (XmlElement)taskNodes[i];
                            NAntTaskNode taskNode = tasksTable.CreateNodeForTask(taskElem, 
                                (XmlElement)taskElem.ParentNode);
                            taskNode.NodeFont = new System.Drawing.Font(
                                scriptsTree.Font, System.Drawing.FontStyle.Regular);

                            object[] taskAttributes = taskNode.GetType().GetCustomAttributes(typeof(NAntTaskAttribute), false);
                            if (taskAttributes.Length > 0) {
                                NAntTaskAttribute taskAttribute = (NAntTaskAttribute)taskAttributes[0];

                                taskNode.ImageIndex = tasksTable.GetTaskNodeImageIndex(taskAttribute.Name);
                                taskNode.SelectedImageIndex = tasksTable.GetTaskNodeImageIndex(taskAttribute.Name);
                            }
                            targetNode.Nodes.Add(taskNode);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Occurs after a TreeNode is Clicked.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Arguments passed to the event</param>
        private void scriptsTree_MouseDown(object sender, MouseEventArgs e) {
            TreeNode clickedNode = scriptsTree.GetNodeAt(e.X, e.Y);

            if (clickedNode != null) {
                scriptsTree.SelectedNode = clickedNode;
            }
        }

        /// <summary>
        /// Occurs after a TreeNode is Clicked.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Arguments passed to the event</param>
        private void scriptsTree_MouseUp(object sender, MouseEventArgs e) {
            TreeNode selectedNode = scriptsTree.SelectedNode;

            if (selectedNode != null) {
                if (e.Button == MouseButtons.Right) {
                    Point clickLoc = new Point(e.X, e.Y);
                    Point screenClickLoc = scriptsTree.PointToScreen(clickLoc);

                    if (selectedNode is NAntProjectNode) {
                        addin.cmdAddinPopup.ShowPopup(screenClickLoc.X, screenClickLoc.Y);
                    }
                    else if (selectedNode is NAntPropertyNode) {
                        addin.cmdPropertyPopup.ShowPopup(screenClickLoc.X, screenClickLoc.Y);
                        return;
                    }
                    else if (selectedNode is NAntTargetNode) {
                        addin.cmdTargetPopup.ShowPopup(screenClickLoc.X, screenClickLoc.Y);
                    }
                    else if (selectedNode is NAntTaskNode) {
                        addin.cmdTaskPopup.ShowPopup(screenClickLoc.X, screenClickLoc.Y);
                    }
                }
            }
        }

        /// <summary>
        /// Occurs after a TreeNode is Selected.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Arguments passed to the event</param>
        internal void scriptsTree_AfterSelect(object sender, TreeViewEventArgs e) {
            if (editedNode != null) {
                if (scriptsTree.LabelEdit == true) {
                    scriptsTree.LabelEdit = false;
                }
            }

            if (e.Node != null) {
                if (e.Node is NAntProjectNode) { // "project" element
                    NAntProjectNode projectNode = (NAntProjectNode)e.Node;

                    object[] refObjs = new object[0];

                    if (projectNode.ReadOnly || (!projectNode.Available)) {
                        object readOnlyProxy = NAntReadOnlyNodeBuilder.GetReadOnlyNode(projectNode);
                        ((NAntProjectNode)readOnlyProxy).Reload();
                        refObjs = new Object[] { readOnlyProxy };
                    }
                    else {
                        refObjs = new Object[] { projectNode };
                    }

                    addin.scriptExplorerWindow.SetSelectionContainer(ref refObjs);

                    buttonRefresh.Visible = true;
                    menSepRefresh.Visible = true;

                    bool docOpen = false;
                    if (projectNode.ProjectItem.Document != null) {
                        docOpen = true;
                    }

                    buttonAddProperty.Visible = projectNode.Available;
                    buttonAddProperty.Enabled = !projectNode.ReadOnly && !docOpen;
                    buttonAddTarget.Visible = projectNode.Available;
                    buttonAddTarget.Enabled = !projectNode.ReadOnly && !docOpen;
                    buttonAddTask.Visible = false;
                    buttonBuild.Visible = projectNode.Available;
                    buttonBuild.ToolTipText = "Build Project";
                    buttonViewCode.Visible = true;
                    menSep1.Visible = true;
                    menSep2.Visible = projectNode.Available;
                    menSep3.Visible = false;
                    buttonMoveUp.Visible = false;
                    buttonMoveDown.Visible = false;
                }
                else if (e.Node is NAntPropertyNode) { // "property" element
                    NAntPropertyNode propertyNode = (NAntPropertyNode)e.Node;

                    object[] refObjs = new object[0];

                    if (propertyNode.ProjectNode.ReadOnly || (!propertyNode.ProjectNode.Available)) {
                        object readOnlyProxy = NAntReadOnlyNodeBuilder.GetReadOnlyNode(propertyNode);
                        refObjs = new Object[] { readOnlyProxy };
                    }
                    else {
                        refObjs = new Object[] { propertyNode };
                    }

                    addin.scriptExplorerWindow.SetSelectionContainer(ref refObjs);

                    if (e.Node.Parent is NAntProjectNode) {
                        bool movePropUp=false, movePropDown=false;
                        CanPropertyReorder(e.Node, ref movePropUp, ref movePropDown);

                        buttonMoveUp.Enabled = movePropUp && !propertyNode.ProjectNode.ReadOnly;
                        buttonMoveDown.Enabled = movePropDown && !propertyNode.ProjectNode.ReadOnly;
                    }
                    else {
                        bool moveTaskUp=false, moveTaskDown=false;
                        CanTaskReorder(e.Node, ref moveTaskUp, ref moveTaskDown);

                        buttonMoveUp.Enabled = moveTaskUp && !propertyNode.ProjectNode.ReadOnly;
                        buttonMoveDown.Enabled = moveTaskDown && !propertyNode.ProjectNode.ReadOnly;
                    }

                    buttonRefresh.Visible = false;
                    menSepRefresh.Visible = false;
                    buttonAddProperty.Visible = false;
                    buttonAddTarget.Visible = false;
                    buttonAddTask.Visible = false;
                    buttonBuild.Visible = false;
                    buttonViewCode.Visible = false;
                    menSep1.Visible = false;
                    menSep2.Visible = false;
                    menSep3.Visible = true;
                    buttonMoveUp.Visible = true;
                    buttonMoveDown.Visible = true;
                }
                else if (e.Node is NAntTargetNode) { // "target" element
                    NAntTargetNode targetNode = (NAntTargetNode)e.Node;

                    object[] refObjs = new object[0];

                    if (targetNode.ProjectNode.ReadOnly) {
                        object readOnlyProxy = NAntReadOnlyNodeBuilder.GetReadOnlyNode(targetNode);
                        refObjs = new Object[] { readOnlyProxy };
                    }
                    else {
                        refObjs = new Object[] { targetNode };
                    }

                    addin.scriptExplorerWindow.SetSelectionContainer(ref refObjs);

                    bool moveTargetUp=false, moveTargetDown=false;
                    CanTargetReorder(e.Node, ref moveTargetUp, ref moveTargetDown);

                    buttonMoveUp.Enabled = moveTargetUp && !targetNode.ProjectNode.ReadOnly;
                    buttonMoveDown.Enabled = moveTargetDown && !targetNode.ProjectNode.ReadOnly;

                    buttonRefresh.Visible = false;
                    menSepRefresh.Visible = false;
                    buttonAddProperty.Visible = true;
                    buttonAddProperty.Enabled =  !targetNode.ProjectNode.ReadOnly;
                    buttonAddTarget.Visible = false;
                    buttonAddTask.Visible = true;
                    buttonAddTask.Enabled = !targetNode.ProjectNode.ReadOnly;
                    buttonBuild.Visible = true;
                    buttonBuild.ToolTipText = "Build Target";
                    buttonViewCode.Visible = false;
                    menSep1.Visible = true;
                    menSep2.Visible = true;
                    menSep3.Visible = true;
                    buttonMoveUp.Visible = true;
                    buttonMoveDown.Visible = true;
                }
                else if (e.Node is NAntTaskNode) { // "task" element
                    NAntTaskNode taskNode = (NAntTaskNode)e.Node;

                    object[] refObjs = new object[0];

                    if (taskNode.ProjectNode.ReadOnly) {
                        object readOnlyProxy = NAntReadOnlyNodeBuilder.GetReadOnlyNode(taskNode);
                        refObjs = new Object[] { readOnlyProxy };
                    }
                    else {
                        refObjs = new Object[] { taskNode };
                    }

                    addin.scriptExplorerWindow.SetSelectionContainer(ref refObjs);

                    bool moveTaskUp=false, moveTaskDown=false;
                    CanTaskReorder(e.Node, ref moveTaskUp, ref moveTaskDown);

                    buttonMoveUp.Enabled = moveTaskUp && !taskNode.ProjectNode.ReadOnly;
                    buttonMoveDown.Enabled = moveTaskDown && !taskNode.ProjectNode.ReadOnly;

                    buttonRefresh.Visible = false;
                    menSepRefresh.Visible = false;
                    buttonAddProperty.Visible = false;
                    buttonAddTarget.Visible = false;
                    buttonAddTask.Visible = false;
                    buttonBuild.Visible = false;
                    buttonViewCode.Visible = false;
                    menSep1.Visible = false;
                    menSep2.Visible = false;
                    menSep3.Visible = true;
                    buttonMoveUp.Visible = true;
                    buttonMoveDown.Visible = true;
                }
            }

            vsNetToolbar.Repaint();
            addin.scriptExplorerWindow.Activate();
            ActiveControl = scriptsTree;
        }

        /// <summary>
        /// Occurs after the Text of a TreeNode has been Edited.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Arguments passed to the event</param>
        private void scriptsTree_AfterLabelEdit(object sender, NodeLabelEditEventArgs e) {
            if (e.Label != null) {
                if (e.Label.Length == 0) {
                    MessageBox.Show(
                        "Invalid tree node label. Property name can not be blank.",
                        "Invalid Property Name", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Stop);
                    e.CancelEdit = true;
                    scriptsTree.LabelEdit = false;
                    return;
                }

                if (e.Label.IndexOfAny(new char[]{'@', ',', '!', ' '}) != -1) {
                    MessageBox.Show(
                        "Invalid tree node label. Property name contained an invalid character.",
                        "Invalid Property Name", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Stop);
                    e.CancelEdit = true;
                    scriptsTree.LabelEdit = false;
                    return;
                }

                if (e.Node is NAntPropertyNode) {
                    NAntPropertyNode propertyNode = (NAntPropertyNode)e.Node;

                    if (e.Label != e.Node.Text) {
                        bool duplicateProperty = false;

                        for (int i = 0; i < propertyNode.Parent.Nodes.Count; i++) {
                            if (propertyNode.Parent.Nodes[i] is NAntPropertyNode) {
                                NAntPropertyNode siblingProperty = (NAntPropertyNode)propertyNode.Parent.Nodes[i];
                                if (siblingProperty.Name == e.Label) {
                                    duplicateProperty = true;
                                    break;
                                }
                            }
                        }

                        if (duplicateProperty) {
                            MessageBox.Show(
                                "Property \"" + e.Label + 
                                "\" already exists.", "Property Exists", 
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            e.CancelEdit = true;
                        }
                        else {
                            propertyNode.Name = e.Label;
                            e.Node.EndEdit(false);
                        }
                    }
                }
                else if (e.Node is NAntTargetNode) {
                    if (e.Label != e.Node.Text) {
                        NAntTargetNode targetNode = (NAntTargetNode)e.Node;

                        bool duplicateTarget = false;

                        for (int i = 0; i < targetNode.Parent.Nodes.Count; i++) {
                            if (targetNode.Parent.Nodes[i] is NAntTargetNode) {
                                NAntTargetNode siblingTarget = (NAntTargetNode)targetNode.Parent.Nodes[i];
                                if (siblingTarget.Name == e.Label) {
                                    duplicateTarget = true;
                                    break;
                                }
                            }
                        }

                        if (duplicateTarget) {
                            MessageBox.Show(
                                "Target \"" + e.Label + 
                                "\" already exists.", "Target Exists", 
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            e.CancelEdit = true;
                        }
                        else {
                            targetNode.Name = e.Label;
                            e.Node.EndEdit(false);
                        }
                    }
                }

                scriptsTree.LabelEdit = false;
            }
        }

        /// <summary>
        /// Occurs when the "Build Project" menu item is clicked.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Arguments passed to the event</param>
        internal void BuildProject_Click(object sender, EventArgs e) {
            if (scriptsTree.SelectedNode is NAntProjectNode) {
                addin.ExecuteBuild((NAntProjectNode)scriptsTree.SelectedNode, null);
            }
        }

        /// <summary>
        /// Occurs when the "Build Target" menu item is clicked.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Arguments passed to the event</param>
        internal void BuildTarget_Click(object sender, EventArgs e) {
            if (scriptsTree.SelectedNode is NAntTargetNode) {
                NAntTargetNode targetNode = (NAntTargetNode)scriptsTree.SelectedNode;
                addin.ExecuteBuild((NAntProjectNode)((NAntTargetNode)scriptsTree.SelectedNode).Parent, targetNode.Name);
            }
        }

        /// <summary>
        /// Occurs when the "View Code" menu item is clicked.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Arguments passed to the event</param>
        internal void ViewCode_Click(object sender, EventArgs e) {
            if (scriptsTree.SelectedNode is NAntProjectNode) {
                NAntProjectNode projectNode = (NAntProjectNode)scriptsTree.SelectedNode;
                if (projectNode.ProjectItem.Document == null) {
                    projectNode.ProjectItem.Open(
                        EnvDTE.Constants.vsViewKindTextView);
                }

                projectNode.Collapse();
                scriptsTree_AfterSelect(scriptsTree, new TreeViewEventArgs(scriptsTree.SelectedNode));
                projectNode.ProjectItem.Document.Activate();
            }
        }

        /// <summary>
        /// Occurs when the "Add Property" menu item is clicked.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Arguments passed to the event</param>
        internal void AddProperty_Click(object sender, EventArgs e) {
            if (scriptsTree.SelectedNode is NAntProjectNode) {
                NAntProjectNode projectNode = (NAntProjectNode)scriptsTree.SelectedNode;
                new AddPropertyDialog(projectNode, clipboardManager).ShowDialog();
            }
            else if (scriptsTree.SelectedNode is NAntTargetNode) {
                NAntTargetNode targetNode = (NAntTargetNode)scriptsTree.SelectedNode;
                new AddPropertyDialog(targetNode, clipboardManager).ShowDialog();
            }
        }

        /// <summary>
        /// Occurs when the "Add Target" menu item is clicked.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Arguments passed to the event</param>
        internal void AddTarget_Click(object sender, EventArgs e) {
            if (scriptsTree.SelectedNode is NAntProjectNode) {
                NAntProjectNode projectNode = (NAntProjectNode)scriptsTree.SelectedNode;
                new AddTargetDialog(projectNode, clipboardManager).ShowDialog();
            }
        }

        /// <summary>
        /// Occurs when the "Add Task" menu item is clicked.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Arguments passed to the event</param>
        internal void AddTask_Click(object sender, EventArgs e) {
            if (scriptsTree.SelectedNode is NAntTargetNode) {
                NAntTargetNode targetNode = (NAntTargetNode)scriptsTree.SelectedNode;
                new AddTaskDialog(tasksTable, targetNode, treeImages, clipboardManager).ShowDialog();
            }
        }

        /// <summary>
        /// Occurs when the "Cut" menu item is clicked.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Arguments passed to the event</param>
        internal void Cut_Click(object sender, EventArgs e) {
            if (scriptsTree.SelectedNode is NAntPropertyNode) {
                NAntPropertyNode propertyNode = (NAntPropertyNode)scriptsTree.SelectedNode;
                clipboardManager.Cut(propertyNode);
            }
            else if (scriptsTree.SelectedNode is NAntTargetNode) {
                NAntTargetNode targetNode = (NAntTargetNode)scriptsTree.SelectedNode;
                clipboardManager.Cut(targetNode);
            }
            else if (scriptsTree.SelectedNode is NAntTaskNode) {
                NAntTaskNode taskNode = (NAntTaskNode)scriptsTree.SelectedNode;
                clipboardManager.Cut(taskNode);
            }
        }

        /// <summary>
        /// Occurs when the "Copy" menu item is clicked.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Arguments passed to the event</param>
        internal void Copy_Click(object sender, EventArgs e) {
            if (scriptsTree.SelectedNode is NAntPropertyNode) {
                NAntPropertyNode propertyNode = (NAntPropertyNode)scriptsTree.SelectedNode;
                clipboardManager.Copy(propertyNode);
            }
            else if (scriptsTree.SelectedNode is NAntTargetNode) {
                NAntTargetNode targetNode = (NAntTargetNode)scriptsTree.SelectedNode;
                clipboardManager.Copy(targetNode);
            }
            else if (scriptsTree.SelectedNode is NAntTaskNode) {
                NAntTaskNode taskNode = (NAntTaskNode)scriptsTree.SelectedNode;
                clipboardManager.Copy(taskNode);
            }
        }

        /// <summary>
        /// Occurs when the "Paste" menu item is clicked.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Arguments passed to the event</param>
        internal void Paste_Click(object sender, EventArgs e) {
            if (scriptsTree.SelectedNode is NAntProjectNode) {
                NAntProjectNode projectNode = (NAntProjectNode)scriptsTree.SelectedNode;
                clipboardManager.Paste(projectNode, scriptsTree);
            }
            else if (scriptsTree.SelectedNode is NAntTargetNode) {
                NAntTargetNode targetNode = (NAntTargetNode)scriptsTree.SelectedNode;
                clipboardManager.Paste(targetNode, scriptsTree);
            }
        }

        /// <summary>
        /// Occurs when the "Delete" menu item is clicked.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Arguments passed to the event</param>
        internal void Delete_Click(object sender, EventArgs e) {
            NAntProjectNode projectNode = ((NAntBaseNode)scriptsTree.SelectedNode).ProjectNode;

            if (scriptsTree.SelectedNode is NAntPropertyNode) {
                NAntPropertyNode propertyNode = (NAntPropertyNode)scriptsTree.SelectedNode;

                DialogResult result = MessageBox.Show(
                    "Delete NAnt Property \"" + propertyNode.Text + "\"?", 
                    "Confirm Deletion of NAnt Property", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes) {
                    propertyNode.PropertyElement.ParentNode.RemoveChild(propertyNode.PropertyElement);
                    projectNode.Save();
                    propertyNode.TreeView.SelectedNode = propertyNode.Parent;
                    propertyNode.Remove();
                }
                else {
                    scriptsTree.SelectedNode = propertyNode;
                }
            }
            else if (scriptsTree.SelectedNode is NAntTargetNode) {
                NAntTargetNode targetNode = (NAntTargetNode)scriptsTree.SelectedNode;

                DialogResult result = MessageBox.Show(
                    "Delete NAnt Target \"" + targetNode.Text + "\"?", 
                    "Confirm Deletion of NAnt Target", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes) {
                    targetNode.TargetElement.ParentNode.RemoveChild(targetNode.TargetElement);
                    projectNode.Save();
                    targetNode.TreeView.SelectedNode = targetNode.Parent;
                    targetNode.Remove();

                    projectNode.ReloadName();
                }
                else {
                    scriptsTree.SelectedNode = targetNode;
                }
            }
            else if (scriptsTree.SelectedNode is NAntTaskNode) {
                NAntTaskNode taskNode = (NAntTaskNode)scriptsTree.SelectedNode;

                DialogResult result = MessageBox.Show(
                    "Delete Selected NAnt \"" + taskNode.Text + "\" Task?", 
                    "Confirm Deletion of NAnt Task", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes) {
                    taskNode.ParentElement.RemoveChild(taskNode.TaskElement);
                    projectNode.Save();
                    taskNode.TreeView.SelectedNode = taskNode.Parent;
                    taskNode.Remove();
                }
                else {
                    scriptsTree.SelectedNode = taskNode;
                }
            }
        }

        /// <summary>
        /// Occurs when the "Rename" menu item is clicked.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Arguments passed to the event</param>
        internal void Rename_Click(object sender, EventArgs e) {
            if (scriptsTree.SelectedNode is NAntPropertyNode) {
                NAntPropertyNode propertyNode = (NAntPropertyNode)scriptsTree.SelectedNode;
                scriptsTree.LabelEdit = true;
                editedNode = propertyNode;
                propertyNode.BeginEdit();
            }
            else if (scriptsTree.SelectedNode is NAntTargetNode) {
                NAntTargetNode targetNode = (NAntTargetNode)scriptsTree.SelectedNode;
                scriptsTree.LabelEdit = true;
                editedNode = targetNode;
                targetNode.BeginEdit();
            }
        }

        /// <summary>
        /// Occurs when the "Set as Startup Target" menu item is clicked.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Arguments passed to the event</param>
        internal void SetAsStartupTarget_Click(object sender, EventArgs e) {
            if (scriptsTree.SelectedNode is NAntTargetNode) {
                NAntTargetNode targetNode = (NAntTargetNode)scriptsTree.SelectedNode;
                NAntProjectNode projectNode = (NAntProjectNode)targetNode.Parent;

                string oldDefaultTarget = projectNode.DefaultTarget;
                if (oldDefaultTarget != null) {
                    foreach (TreeNode projectChild in projectNode.Nodes) {
                        if (projectChild is NAntTargetNode) {
                            NAntTargetNode targetChild = (NAntTargetNode)projectChild;
                            if (targetChild.Name == oldDefaultTarget) {
                                targetChild.NodeFont = new Font(
                                    targetChild.TreeView.Font, FontStyle.Regular);
                            }
                        }
                    }
                }

                targetNode.NodeFont = new Font(
                    targetNode.TreeView.Font, FontStyle.Bold);

                projectNode.DefaultTarget = targetNode.Name;
            }
        }

        /// <summary>
        /// Occurs when the "Move Up" menu item is clicked.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Arguments passed to the event</param>
        internal void MoveUp_Click(object sender, EventArgs e) {
            if (scriptsTree.SelectedNode is NAntTargetNode) {
                NAntTargetNode targetToMoveUp = (NAntTargetNode)scriptsTree.SelectedNode;
                NAntTargetNode previousTarget = null;

                int targetIndex = 0;
                int refIndex = 0;

                for (int i = 0; i < targetToMoveUp.Parent.Nodes.Count; i++) {
                    if (targetToMoveUp.Parent.Nodes[i] is NAntTargetNode) {
                        NAntTargetNode target = (NAntTargetNode)targetToMoveUp.Parent.Nodes[i];
                        if (target == targetToMoveUp) {
                            targetIndex = i;
                            break;
                        }
                        previousTarget = target;
                        refIndex = i;
                    }
                }

                targetToMoveUp.Parent.Nodes.Remove(targetToMoveUp);
                previousTarget.Parent.Nodes.Insert(refIndex, targetToMoveUp);
                scriptsTree.SelectedNode = targetToMoveUp;

                XmlElement targetElement = targetToMoveUp.TargetElement;
                XmlNode targetNode = targetElement.ParentNode.RemoveChild(targetElement);
                previousTarget.TargetElement.ParentNode.InsertBefore(targetNode, previousTarget.TargetElement);

                previousTarget.ProjectNode.Save();

                return;
            }

            if (scriptsTree.SelectedNode is NAntPropertyNode) {
                if (scriptsTree.SelectedNode.Parent is NAntProjectNode) {
                    NAntPropertyNode propToMoveUp = (NAntPropertyNode)scriptsTree.SelectedNode;
                    NAntPropertyNode previousProp = null;

                    int propIndex = 0;
                    int refIndex = 0;

                    for (int i = 0; i < propToMoveUp.Parent.Nodes.Count; i++) {
                        if (propToMoveUp.Parent.Nodes[i] is NAntPropertyNode) {
                            NAntPropertyNode property = (NAntPropertyNode)propToMoveUp.Parent.Nodes[i];
                            if (property == propToMoveUp) {
                                propIndex = i;
                                break;
                            }
                            previousProp = property;
                            refIndex = i;
                        }
                    }

                    propToMoveUp.Parent.Nodes.Remove(propToMoveUp);
                    previousProp.Parent.Nodes.Insert(refIndex, propToMoveUp);
                    scriptsTree.SelectedNode = propToMoveUp;

                    XmlElement propElement = propToMoveUp.PropertyElement;
                    XmlNode propNode = propElement.ParentNode.RemoveChild(propElement);
                    previousProp.PropertyElement.ParentNode.InsertBefore(propNode, previousProp.PropertyElement);

                    previousProp.ProjectNode.Save();

                    return;
                }
            }

            NAntTaskNode taskToMoveUp = (NAntTaskNode)scriptsTree.SelectedNode;
            NAntTaskNode previousTask = null;

            int taskIndex = 0;
            int refIndexT = 0;

            for (int i = 0; i < taskToMoveUp.Parent.Nodes.Count; i++) {
                NAntTaskNode task = (NAntTaskNode)taskToMoveUp.Parent.Nodes[i];
                if (task == taskToMoveUp) {
                    taskIndex = i;
                    break;
                }
                previousTask = task;
                refIndexT = i;
            }

            taskToMoveUp.Parent.Nodes.Remove(taskToMoveUp);
            previousTask.Parent.Nodes.Insert(refIndexT, taskToMoveUp);
            scriptsTree.SelectedNode = taskToMoveUp;

            XmlElement taskElement = taskToMoveUp.TaskElement;
            XmlNode taskNode = taskElement.ParentNode.RemoveChild(taskElement);
            previousTask.TaskElement.ParentNode.InsertBefore(taskNode, previousTask.TaskElement);

            previousTask.ProjectNode.Save();
        }

        /// <summary>
        /// Occurs when the "Move Down" menu item is clicked.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Arguments passed to the event</param>
        internal void MoveDown_Click(object sender, EventArgs e) {
            if (scriptsTree.SelectedNode is NAntTargetNode) {
                NAntTargetNode targetToMoveDown = (NAntTargetNode)scriptsTree.SelectedNode;
                NAntTargetNode nextTarget = null;

                int targetIndex = 0;
                int refIndex = 0;

                for (int i = targetToMoveDown.Parent.Nodes.Count-1; i > -1; i--) {
                    if (targetToMoveDown.Parent.Nodes[i] is NAntTargetNode) {
                        NAntTargetNode target = (NAntTargetNode)targetToMoveDown.Parent.Nodes[i];
                        if (target == targetToMoveDown) {
                            targetIndex = i;
                            break;
                        }
                        nextTarget = target;
                        refIndex = i;
                    }
                }

                targetToMoveDown.Parent.Nodes.Remove(targetToMoveDown);
                nextTarget.Parent.Nodes.Insert(refIndex, targetToMoveDown);
                scriptsTree.SelectedNode = targetToMoveDown;

                XmlElement targetElement = targetToMoveDown.TargetElement;
                XmlNode targetNode = targetElement.ParentNode.RemoveChild(targetElement);
                nextTarget.TargetElement.ParentNode.InsertAfter(targetNode, nextTarget.TargetElement);

                nextTarget.ProjectNode.Save();

                return;
            }

            if (scriptsTree.SelectedNode is NAntPropertyNode) {
                if (scriptsTree.SelectedNode.Parent is NAntProjectNode) {
                    NAntPropertyNode propToMoveDown = (NAntPropertyNode)scriptsTree.SelectedNode;
                    NAntPropertyNode nextProp = null;

                    int propIndex = 0;
                    int refIndex = 0;

                    for (int i = propToMoveDown.Parent.Nodes.Count-1; i > -1; i--) {
                        if (propToMoveDown.Parent.Nodes[i] is NAntPropertyNode) {
                            NAntPropertyNode property = (NAntPropertyNode)propToMoveDown.Parent.Nodes[i];
                            if (property == propToMoveDown) {
                                propIndex = i;
                                break;
                            }
                            nextProp = property;
                            refIndex = i;
                        }
                    }

                    propToMoveDown.Parent.Nodes.Remove(propToMoveDown);
                    nextProp.Parent.Nodes.Insert(refIndex, propToMoveDown);
                    scriptsTree.SelectedNode = propToMoveDown;

                    XmlElement propElement = propToMoveDown.PropertyElement;
                    XmlNode propNode = propElement.ParentNode.RemoveChild(propElement);
                    nextProp.PropertyElement.ParentNode.InsertAfter(propNode, nextProp.PropertyElement);

                    nextProp.ProjectNode.Save();

                    return;
                }
            }

            NAntTaskNode taskToMoveDown = (NAntTaskNode)scriptsTree.SelectedNode;
            NAntTaskNode nextTask = null;

            int taskIndex = 0;
            int refIndexT = 0;

            for (int i = taskToMoveDown.Parent.Nodes.Count-1; i > -1; i--) {
                NAntTaskNode task = (NAntTaskNode)taskToMoveDown.Parent.Nodes[i];
                if (task == taskToMoveDown) {
                    taskIndex = i;
                    break;
                }
                nextTask = task;
                refIndexT = i;
            }

            taskToMoveDown.Parent.Nodes.Remove(taskToMoveDown);
            nextTask.Parent.Nodes.Insert(refIndexT, taskToMoveDown);
            scriptsTree.SelectedNode = taskToMoveDown;

            XmlElement taskElement = taskToMoveDown.TaskElement;
            XmlNode taskNode = taskElement.ParentNode.RemoveChild(taskElement);
            nextTask.TaskElement.ParentNode.InsertAfter(taskNode, nextTask.TaskElement);

            nextTask.ProjectNode.Save();
        }

        /// <summary>
        /// Occurs when a Toolbar Button is Clicked.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Arguments passed to the event</param>
        private void toolbarMain_ButtonClick(object sender, ToolBarButtonClickEventArgs e) {
            if (e.Button.Equals(buttonAbout)) {
                if (DialogResult.No == aboutDialog.ShowDialog()) {
                    addin.application.ItemOperations.Navigate(
                        "http://nant.sourceforge.net", 
                        EnvDTE.vsNavigateOptions.vsNavigateOptionsDefault);
                }
            }
            else if (e.Button.Equals(buttonRefresh)) {
                if (scriptsTree.SelectedNode != null) {
                    NAntProjectNode projectNode = (NAntProjectNode)scriptsTree.SelectedNode;

                    bool expanded = false;

                    if (projectNode.IsExpanded) {
                        projectNode.Collapse();
                        expanded = true;
                    }

                    projectNode.Reload();

                    if (expanded) {
                        projectNode.Expand();
                    }
                }
            }
            else if (e.Button.Equals(buttonAddProperty)) {
                AddProperty_Click(e.Button, new EventArgs());
            }
            else if (e.Button.Equals(buttonAddTarget)) {
                AddTarget_Click(e.Button, new EventArgs());
            }
            else if (e.Button.Equals(buttonAddTask)) {
                AddTask_Click(e.Button, new EventArgs());
            }
            else if (e.Button.Equals(buttonBuild)) {
                if (scriptsTree.SelectedNode is NAntProjectNode) {
                    BuildProject_Click(e.Button, new EventArgs());
                }
                else {
                    BuildTarget_Click(e.Button, new EventArgs());
                }
            }
            else if (e.Button.Equals(buttonViewCode)) {
                ViewCode_Click(e.Button, new EventArgs());
            }
            else if (e.Button.Equals(buttonMoveUp)) {
                MoveUp_Click(e.Button, new EventArgs());
            }
            else if (e.Button.Equals(buttonMoveDown)) {
                MoveDown_Click(e.Button, new EventArgs());
            }
        }

        /// <summary>
        /// Occurs when help is requested for the script explorer tree.
        /// </summary>
        /// <param name="sender">The object that sent the event</param>
        /// <param name="e">Arguments passed to the event</param>
        private void scriptsTree_HelpRequested(object sender, HelpEventArgs e) {
            if (scriptsTree.SelectedNode == null) {
                addin.application.ItemOperations.Navigate(
                    HELP_PREFIX + "scriptexplorer.htm", 
                    EnvDTE.vsNavigateOptions.vsNavigateOptionsDefault);
            }
            else if (scriptsTree.SelectedNode is NAntProjectNode) {
                addin.application.ItemOperations.Navigate(
                    HELP_PREFIX + "projectnode.htm", 
                    EnvDTE.vsNavigateOptions.vsNavigateOptionsDefault);
            }
            else if (scriptsTree.SelectedNode is NAntPropertyNode) {
                addin.application.ItemOperations.Navigate(
                    HELP_PREFIX + "propertynode.htm", 
                    EnvDTE.vsNavigateOptions.vsNavigateOptionsDefault);
            }
            else if (scriptsTree.SelectedNode is NAntTargetNode) {
                addin.application.ItemOperations.Navigate(
                    HELP_PREFIX + "targetnode.htm", 
                    EnvDTE.vsNavigateOptions.vsNavigateOptionsDefault);
            }
            else if (scriptsTree.SelectedNode is NAntTaskNode) {
                addin.application.ItemOperations.Navigate(
                    HELP_PREFIX + "tasknodes.htm", 
                    EnvDTE.vsNavigateOptions.vsNavigateOptionsDefault);
            }
        }

        /// <summary>
        /// Occurs when this Control becomes the active control of its container.
        /// </summary>
        /// <param name="sender">The object that sent the event</param>
        /// <param name="e">Arguments passed to the event</param>
        private void ScriptExplorerControl_Enter(object sender, System.EventArgs e) {
            ActiveControl = scriptsTree;
        }

        /// <summary>
        /// Determines if a Property can be moved up and down.
        /// </summary>
        /// <param name="PropertyNode">The TreeNode for the Property</param>
        /// <param name="MoveUp">Returns true if it can move up</param>
        /// <param name="MoveDown">Returns true if it can move down</param>
        internal void CanPropertyReorder(TreeNode PropertyNode, ref bool MoveUp, ref bool MoveDown) {
            // See if the property can be moved up or down
            int propindex=0, firstprop=-1, lastprop=0;
            for (int i = 0; i < PropertyNode.Parent.Nodes.Count; i++) {
                if (PropertyNode.Parent.Nodes[i] is NAntPropertyNode) {
                    if (firstprop == -1) {
                        firstprop = i;
                    }

                    if (PropertyNode.Parent.Nodes[i] == PropertyNode) {
                        propindex = i;
                    }

                    lastprop = i;
                }
            }

            MoveUp = (propindex != firstprop);
            MoveDown = (propindex != lastprop);
        }

        /// <summary>
        /// Determines if a Task can be moved up and down.
        /// </summary>
        /// <param name="TaskNode">The TreeNode for the Task</param>
        /// <param name="MoveUp">Returns true if it can move up</param>
        /// <param name="MoveDown">Returns true if it can move down</param>
        internal void CanTaskReorder(TreeNode TaskNode, ref bool MoveUp, ref bool MoveDown) {
            // See if the task can be moved up or down
            int nodeindex=0, firstindex=-1, lastindex=0;
            for (int i = 0; i < TaskNode.Parent.Nodes.Count; i++) {
                if (firstindex == -1) {
                    firstindex = i;
                }

                if (TaskNode.Parent.Nodes[i] == TaskNode) {
                    nodeindex = i;
                }

                lastindex = i;
            }

            MoveUp = (nodeindex != firstindex);
            MoveDown = (nodeindex != lastindex);
        }

        /// <summary>
        /// Determines if a Target can be moved up and down.
        /// </summary>
        /// <param name="TargetNode">The TreeNode for the Target</param>
        /// <param name="MoveUp">Returns true if it can move up</param>
        /// <param name="MoveDown">Returns true if it can move down</param>
        internal void CanTargetReorder(TreeNode TargetNode, ref bool MoveUp, ref bool MoveDown) {
            // See if the target can be moved up or down
            int nodeindex=0, firstindex=-1, lastindex=0;
            for (int i = 0; i < TargetNode.Parent.Nodes.Count; i++) {
                if (TargetNode.Parent.Nodes[i] is NAntTargetNode) {
                    if (firstindex == -1) {
                        firstindex = i;
                    }

                    if (TargetNode.Parent.Nodes[i] == TargetNode) {
                        nodeindex = i;
                    }

                    lastindex = i;
                }
            }

            MoveUp = (nodeindex != firstindex);
            MoveDown = (nodeindex != lastindex);
        }

        #region Visual Studio Designer Code

        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ScriptExplorerControl));
            this.toolbarMain = new VSNetToolbarDesigner();
            this.buttonRefresh = new System.Windows.Forms.ToolBarButton();
            this.menSepRefresh = new System.Windows.Forms.ToolBarButton();
            this.buttonBuild = new System.Windows.Forms.ToolBarButton();
            this.buttonViewCode = new System.Windows.Forms.ToolBarButton();
            this.menSep1 = new System.Windows.Forms.ToolBarButton();
            this.buttonAddProperty = new System.Windows.Forms.ToolBarButton();
            this.buttonAddTarget = new System.Windows.Forms.ToolBarButton();
            this.buttonAddTask = new System.Windows.Forms.ToolBarButton();
            this.menSep2 = new System.Windows.Forms.ToolBarButton();
            this.buttonMoveUp = new System.Windows.Forms.ToolBarButton();
            this.buttonMoveDown = new System.Windows.Forms.ToolBarButton();
            this.menSep3 = new System.Windows.Forms.ToolBarButton();
            this.buttonAbout = new System.Windows.Forms.ToolBarButton();
            this.buttonImages = new System.Windows.Forms.ImageList(this.components);
            this.treeImages = new System.Windows.Forms.ImageList(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.vsNetToolbar = new NAnt.Contrib.NAntAddin.Controls.VSNetToolbarControl();
            this.scriptsTree = new System.Windows.Forms.TreeView();
            this.panel2 = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // toolbarMain
            // 
            this.toolbarMain.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
            this.toolbarMain.AutoSize = false;
            this.toolbarMain.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
                                                                                           this.buttonRefresh,
                                                                                           this.menSepRefresh,
                                                                                           this.buttonBuild,
                                                                                           this.buttonViewCode,
                                                                                           this.menSep1,
                                                                                           this.buttonAddProperty,
                                                                                           this.buttonAddTarget,
                                                                                           this.buttonAddTask,
                                                                                           this.menSep2,
                                                                                           this.buttonMoveUp,
                                                                                           this.buttonMoveDown,
                                                                                           this.menSep3,
                                                                                           this.buttonAbout});
            this.toolbarMain.ButtonSize = new System.Drawing.Size(24, 24);
            this.toolbarMain.Divider = false;
            this.toolbarMain.DropDownArrows = true;
            this.toolbarMain.ImageList = this.buttonImages;
            this.toolbarMain.Name = "toolbarMain";
            this.toolbarMain.ShowToolTips = true;
            this.toolbarMain.Size = new System.Drawing.Size(248, 24);
            this.toolbarMain.TabIndex = 0;
            this.toolbarMain.Visible = false;
            this.toolbarMain.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolbarMain_ButtonClick);
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.ImageIndex = 0;
            this.buttonRefresh.ToolTipText = "Reload Project";
            this.buttonRefresh.Visible = false;
            // 
            // menSepRefresh
            // 
            this.menSepRefresh.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            this.menSepRefresh.Visible = false;
            // 
            // buttonBuild
            // 
            this.buttonBuild.ImageIndex = 5;
            this.buttonBuild.ToolTipText = "Build Project/Target";
            this.buttonBuild.Visible = false;
            // 
            // buttonViewCode
            // 
            this.buttonViewCode.ImageIndex = 6;
            this.buttonViewCode.ToolTipText = "View Code";
            this.buttonViewCode.Visible = false;
            // 
            // menSep1
            // 
            this.menSep1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            this.menSep1.Visible = false;
            // 
            // buttonAddProperty
            // 
            this.buttonAddProperty.ImageIndex = 1;
            this.buttonAddProperty.ToolTipText = "Add Property";
            this.buttonAddProperty.Visible = false;
            // 
            // buttonAddTarget
            // 
            this.buttonAddTarget.ImageIndex = 2;
            this.buttonAddTarget.ToolTipText = "Add Target";
            this.buttonAddTarget.Visible = false;
            // 
            // buttonAddTask
            // 
            this.buttonAddTask.ImageIndex = 3;
            this.buttonAddTask.ToolTipText = "Add Task";
            this.buttonAddTask.Visible = false;
            // 
            // menSep2
            // 
            this.menSep2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            this.menSep2.Visible = false;
            // 
            // buttonMoveUp
            // 
            this.buttonMoveUp.ImageIndex = 7;
            this.buttonMoveUp.ToolTipText = "Move Up";
            this.buttonMoveUp.Visible = false;
            // 
            // buttonMoveDown
            // 
            this.buttonMoveDown.ImageIndex = 8;
            this.buttonMoveDown.ToolTipText = "Move Down";
            this.buttonMoveDown.Visible = false;
            // 
            // menSep3
            // 
            this.menSep3.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            this.menSep3.Visible = false;
            // 
            // buttonAbout
            // 
            this.buttonAbout.ImageIndex = 4;
            this.buttonAbout.ToolTipText = "About NAnt Addin";
            // 
            // buttonImages
            // 
            this.buttonImages.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.buttonImages.ImageSize = new System.Drawing.Size(16, 16);
            this.buttonImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("buttonImages.ImageStream")));
            this.buttonImages.TransparentColor = System.Drawing.Color.Magenta;
            // 
            // treeImages
            // 
            this.treeImages.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            this.treeImages.ImageSize = new System.Drawing.Size(16, 16);
            this.treeImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("treeImages.ImageStream")));
            this.treeImages.TransparentColor = System.Drawing.Color.Magenta;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 535);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(248, 1);
            this.panel1.TabIndex = 3;
            // 
            // vsNetToolbar
            // 
            this.vsNetToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.vsNetToolbar.DockPadding.Bottom = 27;
            this.vsNetToolbar.Location = new System.Drawing.Point(0, 24);
            this.vsNetToolbar.Name = "vsNetToolbar";
            this.vsNetToolbar.Size = new System.Drawing.Size(248, 27);
            this.vsNetToolbar.TabIndex = 9;
            this.vsNetToolbar.ToolBar = this.toolbarMain;
            // 
            // scriptsTree
            // 
            this.scriptsTree.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.scriptsTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scriptsTree.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.scriptsTree.ImageList = this.treeImages;
            this.scriptsTree.Indent = 20;
            this.scriptsTree.Location = new System.Drawing.Point(0, 51);
            this.scriptsTree.Name = "scriptsTree";
            this.scriptsTree.Size = new System.Drawing.Size(248, 484);
            this.scriptsTree.TabIndex = 10;
            this.scriptsTree.AfterLabelEdit += new NodeLabelEditEventHandler(this.scriptsTree_AfterLabelEdit);
            this.scriptsTree.AfterSelect += new TreeViewEventHandler(this.scriptsTree_AfterSelect);
            this.scriptsTree.BeforeExpand += new TreeViewCancelEventHandler(this.scriptsTree_BeforeExpand);
            this.scriptsTree.HelpRequested += new HelpEventHandler(this.scriptsTree_HelpRequested);
            this.scriptsTree.MouseUp += new MouseEventHandler(this.scriptsTree_MouseUp);
            this.scriptsTree.MouseDown += new MouseEventHandler(this.scriptsTree_MouseDown);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 51);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1, 484);
            this.panel2.TabIndex = 11;
            // 
            // ScriptExplorerControl
            // 
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                          this.panel2,
                                                                          this.scriptsTree,
                                                                          this.vsNetToolbar,
                                                                          this.panel1,
                                                                          this.toolbarMain});
            this.Name = "ScriptExplorerControl";
            this.Size = new System.Drawing.Size(248, 536);
            this.Enter += new System.EventHandler(this.ScriptExplorerControl_Enter);
            this.ResumeLayout(false);

        }

        /// <summary>
        /// Occurs when the control should release resources.
        /// </summary>
        /// <param name="disposing">Whether the control is being disposed.</param>
        /// <remarks>None.</remarks>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private System.ComponentModel.IContainer components;

        #endregion
    }
}