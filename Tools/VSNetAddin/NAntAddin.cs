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
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

using Microsoft.Office.Core;

using EnvDTE;
using Extensibility;
using VSUserControlHostLib;
using stdole;

using NAnt.Core;

using NAnt.Contrib.NAntAddin.Controls;
using NAnt.Contrib.NAntAddin.Nodes;

namespace NAnt.Contrib.NAntAddin {
    /// <summary>
    /// Visual Studio.NET Addin for using the NAnt build system.
    /// </summary>
    [GuidAttribute("51E3777F-BC62-4C53-BEA4-95C6DC2B36F6"), ProgId("NAnt.Addin")]
    public class Addin : IDTExtensibility2, IDTCommandTarget {
        private AddIn addin;
        private OutputWindow outputWindow;
        private IntPtr hBmp = new IntPtr(0);
        private SolutionEvents solutionEvents;
        private ProjectItemsEvents projectItemEvents;
        private DocumentEvents documentEvents;
        private IVSUserControlHostCtl userControlHost;

        internal CommandBar cmdAddinPopup, cmdPropertyPopup,
            cmdTargetPopup, cmdTaskPopup;
        internal Command cmdBuildProject, cmdBuildTarget,
            cmdViewCode, cmdAddProjectProperty, cmdAddTargetProperty,
            cmdAddTarget, cmdAddTask, cmdCutProperty, cmdCutTarget,
            cmdCutTask, cmdCopyProperty, cmdCopyTarget, cmdCopyTask,
            cmdPasteProject, cmdPasteTarget, cmdDeleteProperty,
            cmdDeleteTarget, cmdDeleteTask, cmdRenameProperty,
            cmdRenameTarget, cmdSetAsStartupTarget, cmdMovePropertyUp,
            cmdMoveTargetUp, cmdMoveTaskUp, cmdMovePropertyDown,
            cmdMoveTargetDown, cmdMoveTaskDown;

        internal _DTE application;
        internal bool building = false;
        internal bool loaded = false;
        internal Window scriptExplorerWindow;
        internal OutputWindowPane buildOutputWindow;
        internal ScriptExplorerControl scriptExplorer;
        internal object buildingMonitor = new object();

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(int handle);

        /// <summary>
        ///	Initializes a new instance of the <see cref="Addin"/> class.
        /// </summary>
        public Addin() {
        }

        /// <summary>
        /// Gets a value indicating whether a build is in progress.
        /// </summary>
        /// <value>
        /// <see langword="true" /> is a build is in progress; otherwise,
        /// <see langword="false" />.
        /// </value>
        public bool Building {
            get { return building; }
        }

        /// <summary>
        /// Occurs when the addin is being loaded by the IDE.
        /// </summary>
        /// <param name="Application">Root object of the IDE extensibility model.</param>
        /// <param name="ConnectMode">Describes the mode in which Addin is being loaded.</param>
        /// <param name="AddinInstance">Object representing this instance of the Addin.</param>
        /// <param name="CustomArguments">Array of arguments passed to the Addin.</param>
        public void OnConnection(
            object Application, ext_ConnectMode ConnectMode,
            object AddinInstance, ref Array CustomArguments) {
            application = (_DTE)Application;
            addin = (AddIn)AddinInstance;

            CreateContextMenu();

            if (ConnectMode == ext_ConnectMode.ext_cm_AfterStartup) {
                CreateBuildOutputWindow();
                CreateScriptExplorerWindow();
                CreateToolbar();
                Solution_Opened();

                loaded = true;
            }

            // Hook Solution Events
            solutionEvents = application.Events.SolutionEvents;
            solutionEvents.Opened +=
                new _dispSolutionEvents_OpenedEventHandler(Solution_Opened);
            solutionEvents.ProjectAdded +=
                new _dispSolutionEvents_ProjectAddedEventHandler(Project_Added);
            solutionEvents.ProjectRemoved +=
                new _dispSolutionEvents_ProjectRemovedEventHandler(Project_Removed);
            solutionEvents.BeforeClosing +=
                new _dispSolutionEvents_BeforeClosingEventHandler(Solution_BeforeClosing);

            // Hook ProjectItem Events
            projectItemEvents = (EnvDTE.ProjectItemsEvents)application.Events.GetObject(
                "CSharpProjectItemsEvents");
            projectItemEvents.ItemRenamed +=
                new _dispProjectItemsEvents_ItemRenamedEventHandler(Item_Renamed);
            projectItemEvents.ItemAdded +=
                new _dispProjectItemsEvents_ItemAddedEventHandler(Item_Added);
            projectItemEvents.ItemRemoved +=
                new _dispProjectItemsEvents_ItemRemovedEventHandler(Item_Removed);

            // Hook Document Events
            documentEvents = application.Events.get_DocumentEvents(null);
            documentEvents.DocumentClosing +=
                new _dispDocumentEvents_DocumentClosingEventHandler(Document_Closing);
            documentEvents.DocumentOpened +=
                new _dispDocumentEvents_DocumentOpenedEventHandler(Document_Opened);
        }

        /// <summary>
        /// Occurs when the addin is being unloaded by the IDE.
        /// </summary>
        /// <param name="DisconnectMode">Describes the mode in which the Addin is being unloaded.</param>
        /// <param name="CustomArguments">Array of arguments passed to the Addin.</param>
        public void OnDisconnection(ext_DisconnectMode DisconnectMode, ref Array CustomArguments) {
            // Close the Script Explorer Window
            if (scriptExplorerWindow != null) {
                scriptExplorerWindow.Visible = false;
            }

            // Clean up the Tab Bitamp
            if (hBmp.ToInt32() != 0) {
                CloseHandle(hBmp.ToInt32());
            }

            // Clean up the Context Menu
            CleanupContextMenu();

            // Unhook Solution Events
            solutionEvents.Opened -=
                new _dispSolutionEvents_OpenedEventHandler(Solution_Opened);
            solutionEvents.ProjectAdded -=
                new _dispSolutionEvents_ProjectAddedEventHandler(Project_Added);
            solutionEvents.ProjectRemoved -=
                new _dispSolutionEvents_ProjectRemovedEventHandler(Project_Removed);
            solutionEvents.BeforeClosing -=
                new _dispSolutionEvents_BeforeClosingEventHandler(Solution_BeforeClosing);

            // Unhook ProjectItem Events *
            projectItemEvents.ItemRenamed -=
                new _dispProjectItemsEvents_ItemRenamedEventHandler(Item_Renamed);
            projectItemEvents.ItemAdded -=
                new _dispProjectItemsEvents_ItemAddedEventHandler(Item_Added);
            projectItemEvents.ItemRemoved -=
                new _dispProjectItemsEvents_ItemRemovedEventHandler(Item_Removed);

            // Unhook Document Events
            documentEvents.DocumentClosing -=
                new _dispDocumentEvents_DocumentClosingEventHandler(Document_Closing);
            documentEvents.DocumentOpened -=
                new _dispDocumentEvents_DocumentOpenedEventHandler(Document_Opened);

            loaded = false;
        }

        /// <summary>
        /// Executes a build on an NAnt Project.
        /// </summary>
        /// <param name="Project">The <see cref="NAnt.Contrib.NAntAddin.Nodes.NAntProjectNode"/> that represents an NAnt Project.</param>
        /// <param name="Target">The name of the Target to execute or null to use the default.</param>
        public void ExecuteBuild(NAntProjectNode Project, string Target) {
            BuildExecutor executor = new BuildExecutor(this, Project, Target, outputWindow, buildOutputWindow);

            ThreadStart start = new ThreadStart(executor.Run);
            System.Threading.Thread thread = new System.Threading.Thread(start);

            thread.Start();
        }

        /// <summary>
        /// Occurs when the collection of Addins has changed.
        /// </summary>
        /// <param name="CustomArguments">Array of arguments passed to the Addin.</param>
        public void OnAddInsUpdate(ref Array CustomArguments) {

        }

        /// <summary>
        /// Occurs when the IDE has completed loading.
        /// </summary>
        /// <param name="CustomArguments">Array of arguments passed to the Addin.</param>
        public void OnStartupComplete(ref Array CustomArguments) {
            if (!loaded) {
                CreateBuildOutputWindow();
                CreateScriptExplorerWindow();
                CreateToolbar();
                Solution_Opened();

                loaded = true;
            }
        }

        /// <summary>
        /// Occurs when the IDE is being unloaded.
        /// </summary>
        /// <param name="CustomArguments">Array of arguments passed to the Addin.</param>
        public void OnBeginShutdown(ref Array CustomArguments) {

        }

        /// <summary>
        /// Occurs when a named Command is executed in the IDE.
        /// </summary>
        /// <param name="CommandName">The name of the Command.</param>
        /// <param name="ExecOption">Execution options for the Command.</param>
        /// <param name="VariantIn">Input parameters to the Command.</param>
        /// <param name="VariantOut">Output parameters of the Command.</param>
        /// <param name="Handled">Whether the Command was handled successfully.</param>
        public void Exec(string CommandName, vsCommandExecOption ExecOption,
            ref object VariantIn, ref object VariantOut, ref bool Handled) {
            EventArgs e = new EventArgs();

            switch (CommandName) {
                case NAntAddinCommands.FULL_BUILD_PROJECT: {
                    scriptExplorer.BuildProject_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_BUILD_TARGET: {
                    scriptExplorer.BuildTarget_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_VIEW_CODE: {
                    scriptExplorer.ViewCode_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_ADD_PROJECT_PROPERTY: {
                    scriptExplorer.AddProperty_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_ADD_TARGET_PROPERTY: {
                    scriptExplorer.AddProperty_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_ADD_TARGET: {
                    scriptExplorer.AddTarget_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_ADD_TASK: {
                    scriptExplorer.AddTask_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_CUT_PROPERTY: {
                    scriptExplorer.Cut_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_CUT_TARGET: {
                    scriptExplorer.Cut_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_CUT_TASK: {
                    scriptExplorer.Cut_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_COPY_PROPERTY: {
                    scriptExplorer.Copy_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_COPY_TARGET: {
                    scriptExplorer.Copy_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_COPY_TASK: {
                    scriptExplorer.Copy_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_PASTE_PROJECT: {
                    scriptExplorer.Paste_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_PASTE_TARGET: {
                    scriptExplorer.Paste_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_DELETE_PROPERTY: {
                    scriptExplorer.Delete_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_DELETE_TARGET: {
                    scriptExplorer.Delete_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_DELETE_TASK: {
                    scriptExplorer.Delete_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_RENAME_PROPERTY: {
                    scriptExplorer.Rename_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_RENAME_TARGET: {
                    scriptExplorer.Rename_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_STARTUP_TARGET: {
                    scriptExplorer.SetAsStartupTarget_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_MOVEUP_PROPERTY: {
                    scriptExplorer.MoveUp_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_MOVEUP_TARGET: {
                    scriptExplorer.MoveUp_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_MOVEUP_TASK: {
                    scriptExplorer.MoveUp_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_MOVEDOWN_PROPERTY: {
                    scriptExplorer.MoveDown_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_MOVEDOWN_TARGET: {
                    scriptExplorer.MoveDown_Click(this, e);
                    Handled = true;
                    break;
                }
                case NAntAddinCommands.FULL_MOVEDOWN_TASK: {
                    scriptExplorer.MoveDown_Click(this, e);
                    Handled = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Occurs before a named Command is displayed in the context menu
        /// for a TreeNode in the <see cref="NAnt.Contrib.NAntAddin.Controls.ScriptExplorerControl"/>.
        /// </summary>
        /// <param name="CommandName">The name of the Command.</param>
        /// <param name="NeededText">Text needed by the Command.</param>
        /// <param name="Status">Return the status of the Command.</param>
        /// <param name="CommandText">Return the text of the Command.</param>
        public void QueryStatus(string CommandName, vsCommandStatusTextWanted NeededText,
            ref vsCommandStatus Status, ref object CommandText) {
            NAntBaseNode selectedNode = (NAntBaseNode)scriptExplorer.scriptsTree.SelectedNode;

            if (CommandName.StartsWith(NAntAddinCommands.ADDIN_PROGID)) {
                if (selectedNode != null) {
                    NAntProjectNode projectNode = selectedNode.ProjectNode;
                    if (projectNode.ReadOnly) {
                        switch (CommandName) {
                            case NAntAddinCommands.FULL_BUILD_PROJECT: {
                                Status = vsCommandStatus.vsCommandStatusSupported |
                                    vsCommandStatus.vsCommandStatusEnabled;
                                break;
                            }
                            case NAntAddinCommands.FULL_BUILD_TARGET: {
                                Status = vsCommandStatus.vsCommandStatusSupported |
                                    vsCommandStatus.vsCommandStatusEnabled;
                                break;
                            }
                            case NAntAddinCommands.FULL_VIEW_CODE: {
                                Status = vsCommandStatus.vsCommandStatusSupported |
                                    vsCommandStatus.vsCommandStatusEnabled;
                                break;
                            }
                            case NAntAddinCommands.FULL_COPY_PROPERTY: {
                                Status = vsCommandStatus.vsCommandStatusSupported |
                                    vsCommandStatus.vsCommandStatusEnabled;
                                break;
                            }
                            case NAntAddinCommands.FULL_COPY_TARGET: {
                                Status = vsCommandStatus.vsCommandStatusSupported |
                                    vsCommandStatus.vsCommandStatusEnabled;
                                break;
                            }
                            case NAntAddinCommands.FULL_COPY_TASK: {
                                Status = vsCommandStatus.vsCommandStatusSupported |
                                    vsCommandStatus.vsCommandStatusEnabled;
                                break;
                            }
                        }
                        return;
                    }

                    if (!projectNode.Available) {
                        switch (CommandName) {
                            case NAntAddinCommands.FULL_BUILD_PROJECT: {
                                return;
                            }
                            case NAntAddinCommands.FULL_ADD_PROJECT_PROPERTY: {
                                return;
                            }
                            case NAntAddinCommands.FULL_ADD_TARGET: {
                                return;
                            }
                        }
                    }
                }

                switch (CommandName) {
                    case NAntAddinCommands.FULL_PASTE_PROJECT: {
                        if (scriptExplorer.clipboardManager.Contents == ClipboardContents.PROPERTY ||
                            scriptExplorer.clipboardManager.Contents == ClipboardContents.TARGET) {
                            Status = vsCommandStatus.vsCommandStatusSupported |
                                vsCommandStatus.vsCommandStatusEnabled;
                        }
                        break;
                    }
                    case NAntAddinCommands.FULL_PASTE_TARGET: {
                        if (scriptExplorer.clipboardManager.Contents == ClipboardContents.PROPERTY ||
                            scriptExplorer.clipboardManager.Contents == ClipboardContents.TASK) {
                            Status = vsCommandStatus.vsCommandStatusSupported |
                                vsCommandStatus.vsCommandStatusEnabled;
                        }
                        break;
                    }
                    case NAntAddinCommands.FULL_MOVEUP_PROPERTY: {
                        if (selectedNode.Parent is NAntProjectNode) {
                            bool moveUp=false, moveDown=false;
                            scriptExplorer.CanPropertyReorder(selectedNode, ref moveUp, ref moveDown);
                            if (moveUp) {
                                Status = vsCommandStatus.vsCommandStatusSupported |
                                    vsCommandStatus.vsCommandStatusEnabled;
                            }
                        }
                        else {
                            bool moveUp=false, moveDown=false;
                            scriptExplorer.CanTaskReorder(selectedNode, ref moveUp, ref moveDown);
                            if (moveUp) {
                                Status = vsCommandStatus.vsCommandStatusSupported |
                                    vsCommandStatus.vsCommandStatusEnabled;
                            }
                        }
                        break;
                    }
                    case NAntAddinCommands.FULL_MOVEUP_TARGET: {
                        bool moveUp=false, moveDown=false;
                        scriptExplorer.CanTargetReorder(selectedNode, ref moveUp, ref moveDown);
                        if (moveUp) {
                            Status = vsCommandStatus.vsCommandStatusSupported |
                                vsCommandStatus.vsCommandStatusEnabled;
                        }
                        break;
                    }
                    case NAntAddinCommands.FULL_MOVEUP_TASK: {
                        bool moveUp=false, moveDown=false;
                        scriptExplorer.CanTaskReorder(selectedNode, ref moveUp, ref moveDown);
                        if (moveUp) {
                            Status = vsCommandStatus.vsCommandStatusSupported |
                                vsCommandStatus.vsCommandStatusEnabled;
                        }
                        break;
                    }
                    case NAntAddinCommands.FULL_MOVEDOWN_PROPERTY: {
                        if (selectedNode.Parent is NAntProjectNode) {
                            bool moveUp=false, moveDown=false;
                            scriptExplorer.CanPropertyReorder(selectedNode, ref moveUp, ref moveDown);
                            if (moveDown) {
                                Status = vsCommandStatus.vsCommandStatusSupported |
                                    vsCommandStatus.vsCommandStatusEnabled;
                            }
                        }
                        else {
                            bool moveUp=false, moveDown=false;
                            scriptExplorer.CanTaskReorder(selectedNode, ref moveUp, ref moveDown);
                            if (moveDown) {
                                Status = vsCommandStatus.vsCommandStatusSupported |
                                    vsCommandStatus.vsCommandStatusEnabled;
                            }
                        }
                        break;
                    }
                    case NAntAddinCommands.FULL_MOVEDOWN_TARGET: {
                        bool moveUp=false, moveDown=false;
                        scriptExplorer.CanTargetReorder(selectedNode, ref moveUp, ref moveDown);
                        if (moveDown) {
                            Status = vsCommandStatus.vsCommandStatusSupported |
                                vsCommandStatus.vsCommandStatusEnabled;
                        }
                        break;
                    }
                    case NAntAddinCommands.FULL_MOVEDOWN_TASK: {
                        bool moveUp=false, moveDown=false;
                        scriptExplorer.CanTaskReorder(selectedNode, ref moveUp, ref moveDown);
                        if (moveDown) {
                            Status = vsCommandStatus.vsCommandStatusSupported |
                                vsCommandStatus.vsCommandStatusEnabled;
                        }
                        break;
                    }
                    default: {
                        Status = vsCommandStatus.vsCommandStatusSupported |
                            vsCommandStatus.vsCommandStatusEnabled;

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Occurs when a Solution or Project is Opened.
        /// </summary>
        private void Solution_Opened() {
            // For each Project
            for (int i = 1; i < (application.Solution.Projects.Count+1); i++) {
                EnvDTE.Project project = application.Solution.Projects.Item(i);

                // For each Project Item
                for (int j = 1; j < (project.ProjectItems.Count+1); j++) {
                    Item_Added(project.ProjectItems.Item(j));
                }
            }
        }

        /// <summary>
        /// Occurs when a Project is added to the Solution.
        /// </summary>
        /// <param name="Project">The VS.NET Project Object</param>
        private void Project_Added(EnvDTE.Project Project) {
            // For each Project Item
            for (int j = 1; j < (Project.ProjectItems.Count+1); j++) {
                Item_Added(Project.ProjectItems.Item(j));
            }
        }

        /// <summary>
        /// Occurs when an item is added to the project.
        /// </summary>
        /// <param name="projectItem">The <see cref="ProjectItem" /> being added.</param>
        private void Item_Added(ProjectItem projectItem) {
            // If the Project Item's Filename ends with ".build"
            if (projectItem.Kind.Equals(Constants.vsProjectItemKindPhysicalFile)) {
                if (projectItem.Name.EndsWith(".build")) {
                    NAntProjectNode projectNode = new NAntProjectNode(
                        this, projectItem.ContainingProject, projectItem);

                    // Add a NAnt Project Node to the Script Explorer Tree
                    scriptExplorer.scriptsTree.Nodes.Add(projectNode);

                    projectNode.NodeFont = new System.Drawing.Font(
                        scriptExplorer.scriptsTree.Font, FontStyle.Regular);

                    projectNode.Reload();

                    object[] refObjs = null;

                    if (projectNode.ReadOnly) {
                        object readOnlyProxy = NAntReadOnlyNodeBuilder.GetReadOnlyNode(projectNode);
                        ((NAntProjectNode)readOnlyProxy).Reload();
                        refObjs = new Object[] { readOnlyProxy };
                    }
                    else {
                        refObjs = new Object[] { projectNode };
                    }

                    scriptExplorerWindow.SetSelectionContainer(ref refObjs);
                }
            }
        }

        /// <summary>
        /// Occurs when an item is renamed in the project.
        /// </summary>
        /// <param name="projectItem">The <see cref="ProjectItem" /> object being renamed.</param>
        /// <param name="OldName">The name of the <see cref="ProjectItem" /> before it was renamed.</param>
        private void Item_Renamed(ProjectItem projectItem, string OldName) {
            if (OldName.EndsWith(".build") && !projectItem.Name.EndsWith(".build")) {
                Item_Removed(projectItem);
            } else if (!OldName.EndsWith(".build") && projectItem.Name.EndsWith(".build")) {
                Item_Added(projectItem);
            }
        }

        /// <summary>
        /// Occurs prior to a solution closing.
        /// </summary>
        private void Solution_BeforeClosing() {
            // For each Project
            for (int i = 1; i < (application.Solution.Projects.Count+1); i++) {
                Project_Removed(application.Solution.Projects.Item(i));
            }
        }

        /// <summary>
        /// Occurs when a project is removed from the solution.
        /// </summary>
        /// <param name="Project">The VS.NET Project Object that is removed.</param>
        private void Project_Removed(EnvDTE.Project Project) {
            // For each Script Explorer Tree Node
            for (int j = 0; j < scriptExplorer.scriptsTree.Nodes.Count; j++) {
                NAntProjectNode projectNode = (NAntProjectNode)scriptExplorer.scriptsTree.Nodes[j];

                // If the NAnt Project Node's Project is the one being closed
                if (projectNode.Project == Project) {
                    // Remove the NANt Project Node
                    scriptExplorer.scriptsTree.Nodes.RemoveAt(j);
                }
            }
        }

        /// <summary>
        /// Occurs when an item is removed from the project.
        /// </summary>
        /// <param name="projectItem">The <see cref="ProjectItem" /> being removed</param>
        private void Item_Removed(ProjectItem projectItem) {
            // For each Script Explorer Tree Node
            for (int j = 0; j < scriptExplorer.scriptsTree.Nodes.Count; j++) {
                NAntProjectNode projectNode = (NAntProjectNode)scriptExplorer.scriptsTree.Nodes[j];

                // If the NAnt Project Node's Project is the one being closed
                if (projectNode.ProjectItem == projectItem) {
                    // Remove the NANt Project Node
                    scriptExplorer.scriptsTree.Nodes.RemoveAt(j);
                }
            }
        }

        /// <summary>
        /// Occurs when an ProjectItem's Document is Closed.
        /// </summary>
        /// <param name="Document">The Document of the ProjecItem being Edited</param>
        private void Document_Closing(Document Document) {
            // For each Script Explorer Tree Node
            for (int j = 0; j < scriptExplorer.scriptsTree.Nodes.Count; j++) {
                NAntProjectNode projectNode = (NAntProjectNode)scriptExplorer.scriptsTree.Nodes[j];

                if (Document.FullName == projectNode.FileName) {
                    projectNode.Reload();
                }
            }
        }

        /// <summary>
        /// Occurs when a ProjectItem's Document is Opened.
        /// </summary>
        /// <param name="Document">The Document of the ProjecItem being Edited</param>
        private void Document_Opened(Document Document) {
            // For each Script Explorer Tree Node
            for (int j = 0; j < scriptExplorer.scriptsTree.Nodes.Count; j++) {
                NAntProjectNode projectNode = (NAntProjectNode)scriptExplorer.scriptsTree.Nodes[j];

                if (projectNode.ProjectItem.Document != null) {
                    if (projectNode.ProjectItem.Document == Document) {
                        projectNode.Collapse();
                    }
                }
            }
        }

        /// <summary>
        /// Creates the Output Window Panel to Display NAnt Builds Progress.
        /// </summary>
        private void CreateBuildOutputWindow() {
            outputWindow = (OutputWindow)application.Windows.Item(Constants.vsWindowKindOutput).Object;
            buildOutputWindow = outputWindow.OutputWindowPanes.Add("NAnt Build");
        }

        /// <summary>
        /// Creates the Script Explorer Window and Docks it to the IDE.
        /// </summary>
        private void CreateScriptExplorerWindow() {
            object controlReference = null;

            // Create the Script Explorer Tool Window
            scriptExplorerWindow = application.Windows.CreateToolWindow(
                addin, "VSUserControlHost.VSUserControlHostCtl",
                "NAnt Scripts", "{753877F3-05AD-4e01-BF7B-86EB4A9F3B3F}",
                ref controlReference);

            // NOTE: Must set to true here to initialize managed window
            // _START_INIT_WINDOW
            scriptExplorerWindow.Visible = true;

            userControlHost = (IVSUserControlHostCtl)controlReference;

            // Get the Script Explorer Control
            scriptExplorer = (ScriptExplorerControl)userControlHost.HostUserControl(
                Assembly.GetExecutingAssembly().Location,
                "NAnt.Contrib.NAntAddin.Controls.ScriptExplorerControl");

            // _END_INIT_WINDOW
            scriptExplorerWindow.Visible = false;

            // Set the Tab Image
            Bitmap tabImage = (Bitmap)Bitmap.FromFile(TaskNodesTable.BaseDir + "nanttabicon.bmp", false);
            object iPictureDisp = TabImageHost.GetIPictureDispFromPicture2(tabImage);
            scriptExplorerWindow.SetTabPicture(iPictureDisp);

            scriptExplorer.Addin = this;
            scriptExplorerWindow.Visible = true;
        }

        /// <summary>
        /// Creates the Project Node Popup.
        /// </summary>
        private void CreateProjectPopup() {
            //
            // Remove Command Bar
            //
            try {
                _CommandBars cmdBars = application.CommandBars;
                CommandBar delBar = (CommandBar)cmdBars[NAntAddinCommandBars.PROJECT];
                application.Commands.RemoveCommandBar(delBar);
            }
            catch (Exception) {}

            //
            // Re-Add Command Bar
            //
            cmdAddinPopup = (CommandBar)application.Commands.AddCommandBar(
                NAntAddinCommandBars.PROJECT,
                EnvDTE.vsCommandBarType.vsCommandBarTypePopup,
                null, 0);

            object[] contextUIGuids = new Object[] {};
            int status = (int)vsCommandStatus.vsCommandStatusSupported +
                (int)vsCommandStatus.vsCommandStatusEnabled;

            //
            // Re-Add Commands
            //

            cmdBuildProject = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.BUILD_PROJECT,
                "Build Project",
                "Builds the Project",
                false, 101, ref contextUIGuids, status);
            cmdBuildProject.AddControl(cmdAddinPopup, 1);

            cmdViewCode = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.VIEW_CODE,
                "View Code",
                "Opens the Script XML Source",
                false, 102, ref contextUIGuids, status);
            cmdViewCode.AddControl(cmdAddinPopup, 2);

            cmdAddProjectProperty = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.ADD_PROJECT_PROPERTY,
                "Add Property...",
                "Adds an NAnt Property",
                false, 103, ref contextUIGuids, status);
            CommandBarControl ctrlAddProjectProperty = cmdAddProjectProperty.AddControl(cmdAddinPopup, 3);
            ctrlAddProjectProperty.BeginGroup = true;

            cmdAddTarget = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.ADD_TARGET,
                "Add Target...",
                "Adds an NAnt Target",
                false, 104, ref contextUIGuids, status);
            cmdAddTarget.AddControl(cmdAddinPopup, 4);

            cmdPasteProject = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.PASTE_PROJECT,
                "Paste",
                "Paste the NAnt Property or Target on the Clipboard onto the the NAnt Project",
                false, 110, ref contextUIGuids, status);
            CommandBarControl ctrlPaste = cmdPasteProject.AddControl(cmdAddinPopup, 5);
            ctrlPaste.BeginGroup = true;
        }

        /// <summary>
        /// Creates the Property Node Popup.
        /// </summary>
        private void CreatePropertyPopup() {
            //
            // Remove Command Bar
            //
            try {
                _CommandBars cmdBars = application.CommandBars;
                CommandBar delBar = (CommandBar)cmdBars[NAntAddinCommandBars.PROPERTY];
                application.Commands.RemoveCommandBar(delBar);
            }
            catch (Exception) {}

            //
            // Re-Add Command Bar
            //
            cmdPropertyPopup = (CommandBar)application.Commands.AddCommandBar(
                NAntAddinCommandBars.PROPERTY,
                EnvDTE.vsCommandBarType.vsCommandBarTypePopup,
                null, 0);

            object[] contextUIGuids = new Object[] {};
            int status = (int)vsCommandStatus.vsCommandStatusSupported +
                (int)vsCommandStatus.vsCommandStatusEnabled;

            //
            // Re-Add Commands
            //

            cmdCutProperty = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.CUT_PROPERTY,
                "Cut",
                "Cut the NAnt Property to the Clipboard",
                false, 108, ref contextUIGuids, status);
            cmdCutProperty.AddControl(cmdPropertyPopup, 1);

            cmdCopyProperty = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.COPY_PROPERTY,
                "Copy",
                "Copy the NAnt Property to the Clipboard",
                false, 109, ref contextUIGuids, status);
            cmdCopyProperty.AddControl(cmdPropertyPopup, 2);

            cmdDeleteProperty = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.DELETE_PROPERTY,
                "Delete",
                "Delete the NAnt Property",
                false, 111, ref contextUIGuids, status);
            cmdDeleteProperty.AddControl(cmdPropertyPopup, 3);

            cmdRenameProperty = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.RENAME_PROPERTY,
                "Rename",
                "Rename the NAnt Property",
                false, 0, ref contextUIGuids, status);
            cmdRenameProperty.AddControl(cmdPropertyPopup, 4);

            cmdMovePropertyUp = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.MOVEUP_PROPERTY,
                "Move Up",
                "Moves the Selected NAnt Property Up",
                false, 106, ref contextUIGuids, status);
            CommandBarControl ctrlMoveUp = cmdMovePropertyUp.AddControl(cmdPropertyPopup, 5);
            ctrlMoveUp.BeginGroup = true;

            cmdMovePropertyDown = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.MOVEDOWN_PROPERTY,
                "Move Down",
                "Moves the Selected NAnt Property Down",
                false, 107, ref contextUIGuids, status);
            cmdMovePropertyDown.AddControl(cmdPropertyPopup, 6);
        }

        /// <summary>
        /// Creates the Target Node Popup.
        /// </summary>
        private void CreateTargetPopup() {
            //
            // Remove Command Bar
            //
            try {
                _CommandBars cmdBars = application.CommandBars;
                CommandBar delBar = (CommandBar)cmdBars[NAntAddinCommandBars.TARGET];
                application.Commands.RemoveCommandBar(delBar);
            }
            catch (Exception) {}

            //
            // Re-Add Command Bar
            //
            cmdTargetPopup = (CommandBar)application.Commands.AddCommandBar(
                NAntAddinCommandBars.TARGET,
                EnvDTE.vsCommandBarType.vsCommandBarTypePopup,
                null, 0);

            object[] contextUIGuids = new Object[] {};
            int status = (int)vsCommandStatus.vsCommandStatusSupported +
                (int)vsCommandStatus.vsCommandStatusEnabled;

            //
            // Re-Add Commands
            //

            cmdBuildTarget = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.BUILD_TARGET,
                "Build Target",
                "Builds the Target",
                false, 101, ref contextUIGuids, status);
            cmdBuildTarget.AddControl(cmdTargetPopup, 1);

            cmdAddTargetProperty = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.ADD_TARGET_PROPERTY,
                "Add Property...",
                "Adds an NAnt Property",
                false, 103, ref contextUIGuids, status);
            CommandBarControl ctrlAddTargetProperty = cmdAddTargetProperty.AddControl(cmdTargetPopup, 2);
            ctrlAddTargetProperty.BeginGroup = true;

            cmdAddTask = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.ADD_TASK,
                "Add Task...",
                "Adds an NAnt Task",
                false, 105, ref contextUIGuids, status);
            cmdAddTask.AddControl(cmdTargetPopup, 3);

            cmdCutTarget = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.CUT_TARGET,
                "Cut",
                "Cut the NAnt Target to the Clipboard",
                false, 108, ref contextUIGuids, status);
            CommandBarControl ctrlCut = cmdCutTarget.AddControl(cmdTargetPopup, 4);
            ctrlCut.BeginGroup = true;

            cmdCopyTarget = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.COPY_TARGET,
                "Copy",
                "Copy the NAnt Target to the Clipboard",
                false, 109, ref contextUIGuids, status);
            cmdCopyTarget.AddControl(cmdTargetPopup, 5);

            cmdPasteTarget = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.PASTE_TARGET,
                "Paste",
                "Paste the NAnt Property or Task on the Clipboard onto the the NAnt Target",
                false, 110, ref contextUIGuids, status);
            cmdPasteTarget.AddControl(cmdTargetPopup, 6);

            cmdDeleteTarget = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.DELETE_TARGET,
                "Delete",
                "Delete the NAnt Target",
                false, 111, ref contextUIGuids, status);
            cmdDeleteTarget.AddControl(cmdTargetPopup, 7);

            cmdRenameTarget = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.RENAME_TARGET,
                "Rename",
                "Rename the NAnt Target",
                false, 0, ref contextUIGuids, status);
            cmdRenameTarget.AddControl(cmdTargetPopup, 8);

            cmdSetAsStartupTarget = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.STARTUP_TARGET,
                "Set as Startup Target",
                "Sets the Selected NAnt Target as its Project's Startup Target",
                false, 0, ref contextUIGuids, status);
            CommandBarControl ctrlSetAsStartupTarget = cmdSetAsStartupTarget.AddControl(cmdTargetPopup, 9);
            ctrlSetAsStartupTarget.BeginGroup = true;

            cmdMoveTargetUp = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.MOVEUP_TARGET,
                "Move Up",
                "Moves the Selected NAnt Target Up",
                false, 106, ref contextUIGuids, status);
            CommandBarControl ctrlMoveUp = cmdMoveTargetUp.AddControl(cmdTargetPopup, 10);
            ctrlMoveUp.BeginGroup = true;

            cmdMoveTargetDown = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.MOVEDOWN_TARGET,
                "Move Down",
                "Moves the Selected NAnt Target Down",
                false, 107, ref contextUIGuids, status);
            cmdMoveTargetDown.AddControl(cmdTargetPopup, 11);
        }

        /// <summary>
        /// Creates the Task Node Popup.
        /// </summary>
        private void CreateTaskPopup() {
            //
            // Remove Command Bar
            //
            try {
                _CommandBars cmdBars = application.CommandBars;
                CommandBar delBar = (CommandBar)cmdBars[NAntAddinCommandBars.TASK];
                application.Commands.RemoveCommandBar(delBar);
            }
            catch (Exception) {}

            //
            // Re-Add Command Bar
            //
            cmdTaskPopup = (CommandBar)application.Commands.AddCommandBar(
                NAntAddinCommandBars.TASK,
                EnvDTE.vsCommandBarType.vsCommandBarTypePopup,
                null, 0);

            object[] contextUIGuids = new Object[] {};
            int status = (int)vsCommandStatus.vsCommandStatusSupported +
                (int)vsCommandStatus.vsCommandStatusEnabled;

            //
            // Re-Add Commands
            //

            cmdCutTask = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.CUT_TASK,
                "Cut",
                "Cut the NAnt Task to the Clipboard",
                false, 108, ref contextUIGuids, status);
            cmdCutTask.AddControl(cmdTaskPopup, 1);

            cmdCopyTask = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.COPY_TASK,
                "Copy",
                "Copy the NAnt Task to the Clipboard",
                false, 109, ref contextUIGuids, status);
            cmdCopyTask.AddControl(cmdTaskPopup, 2);

            cmdDeleteTask = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.DELETE_TASK,
                "Delete",
                "Delete the NAnt Task",
                false, 111, ref contextUIGuids, status);
            cmdDeleteTask.AddControl(cmdTaskPopup, 3);

            cmdMoveTaskUp = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.MOVEUP_TASK,
                "Move Up",
                "Moves the Selected NAnt Task Up",
                false, 106, ref contextUIGuids, status);
            CommandBarControl ctrlMoveUp = cmdMoveTaskUp.AddControl(cmdTaskPopup, 4);
            ctrlMoveUp.BeginGroup = true;

            cmdMoveTaskDown = application.Commands.AddNamedCommand(
                addin, NAntAddinCommands.MOVEDOWN_TASK,
                "Move Down",
                "Moves the Selected NAnt Task Down",
                false, 107, ref contextUIGuids, status);
            cmdMoveTaskDown.AddControl(cmdTaskPopup, 5);
        }

        /// <summary>
        /// Creates the NAntAddin's Context Menu.
        /// </summary>
        private void CreateContextMenu() {
            //
            // Remove Commands
            //
            foreach (Command cmd in application.Commands) {
                if (NAntAddinCommands.Commands.Contains(cmd.Name)) {
                    try {
                        cmd.Delete();
                    }
                    catch (Exception) {}
                }
            }

            CreateProjectPopup();
            CreatePropertyPopup();
            CreateTargetPopup();
            CreateTaskPopup();
        }

        /// <summary>
        /// Creates the NAntAddin's Toolbar.
        /// </summary>
        private void CreateToolbar() {
            //
            // Remove Command Bar
            //
            try {
                _CommandBars cmdBars = application.CommandBars;
                CommandBar delBar = (CommandBar)cmdBars[NAntAddinCommandBars.TOOLBAR];
                application.Commands.RemoveCommandBar(delBar);
            }
            catch (Exception) {}
        }

        /// <summary>
        /// Cleans up the NAntAddin's context menu.
        /// </summary>
        private void CleanupContextMenu() {
            cmdBuildProject.Delete();
            cmdBuildTarget.Delete();
            cmdViewCode.Delete();
            cmdAddProjectProperty.Delete();
            cmdAddTargetProperty.Delete();
            cmdAddTarget.Delete();
            cmdAddTask.Delete();
            cmdCutProperty.Delete();
            cmdCutTarget.Delete();
            cmdCutTask.Delete();
            cmdCopyProperty.Delete();
            cmdCopyTarget.Delete();
            cmdCopyTask.Delete();
            cmdPasteProject.Delete();
            cmdPasteTarget.Delete();
            cmdDeleteProperty.Delete();
            cmdDeleteTarget.Delete();
            cmdDeleteTask.Delete();
            cmdRenameProperty.Delete();
            cmdRenameTarget.Delete();
            cmdSetAsStartupTarget.Delete();
            cmdMovePropertyUp.Delete();
            cmdMoveTargetUp.Delete();
            cmdMoveTaskUp.Delete();
            cmdMovePropertyDown.Delete();
            cmdMoveTargetDown.Delete();
            cmdMoveTaskDown.Delete();

            application.Commands.RemoveCommandBar(cmdAddinPopup);
            application.Commands.RemoveCommandBar(cmdPropertyPopup);
            application.Commands.RemoveCommandBar(cmdTargetPopup);
            application.Commands.RemoveCommandBar(cmdTaskPopup);
        }
    }

    /// <summary>
    /// Logs NAnt build log messages to
    /// a Visual Studio.NET Output Window.
    /// </summary>
    internal class AddinLogListener : IBuildListener {
        private OutputWindowPane buildOutputWindow;

        /// <summary>
        /// Creates a new Addin Log Listener.
        /// </summary>
        /// <param name="BuildOutputWindow">The Output Pane to write messages to</param>
        internal AddinLogListener(OutputWindowPane BuildOutputWindow) {
            buildOutputWindow = BuildOutputWindow;
        }

        /// <summary>
        /// Signals that a build has started.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        /// <remarks>
        /// This event is fired before any targets have started.
        /// </remarks>
        void IBuildListener.BuildStarted(object sender, BuildEventArgs e) {
        }

        /// <summary>
        /// Signals that the last target has finished.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        /// <remarks>
        /// This event will still be fired if an error occurred during the build.
        /// </remarks>
        void IBuildListener.BuildFinished(object sender, BuildEventArgs e) {
            if (e.Exception == null) {
                buildOutputWindow.OutputString("\nBUILD SUCCEEDED\n");
            }
            else {
                buildOutputWindow.OutputString("\nBUILD FAILED\n\n" +
                    e.Exception.ToString());
            }
        }

        /// <summary>
        /// Signals that a target has started.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        void IBuildListener.TargetStarted(object sender, BuildEventArgs e) {
        }

        /// <summary>
        /// Signals that a target has finished.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        /// <remarks>
        /// This event will still be fired if an error occurred during the build.
        /// </remarks>
        void IBuildListener.TargetFinished(object sender, BuildEventArgs e) {
        }

        /// <summary>
        /// Signals that a task has started.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        void IBuildListener.TaskStarted(object sender, BuildEventArgs e) {
        }

        /// <summary>
        /// Signals that a task has finished.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        /// <remarks>
        /// This event will still be fired if an error occurred during the build.
        /// </remarks>
        void IBuildListener.TaskFinished(object sender, BuildEventArgs e) {
        }

        /// <summary>
        /// Signals that a message has been logged.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        void IBuildListener.MessageLogged(object sender, BuildEventArgs e) {
            buildOutputWindow.OutputString("\n" + e.Message);
        }
    }

    /// <summary>
    /// Executes a Build in a separate thread from the IDE.
    /// </summary>
    internal class BuildExecutor {
        NAntAddin.Addin addin;
        NAntProjectNode project;
        string target;
        OutputWindow outputWindow;
        OutputWindowPane buildOutputWindow;

        /// <summary>
        /// Creates a new Build Executor.
        /// </summary>
        /// <param name="Addin">The addin the build is being executed for</param>
        /// <param name="Project">The project containing the target to execute</param>
        /// <param name="Target">The target to execute or null for the default</param>
        /// <param name="OutputWindow">The output window to log build messages to</param>
        /// <param name="BuildOutputWindow">The window containing the output window</param>
        internal BuildExecutor(NAntAddin.Addin Addin, NAntProjectNode Project,
            string Target, OutputWindow OutputWindow, OutputWindowPane BuildOutputWindow) {
            addin = Addin;
            project = Project;
            target = Target;
            outputWindow = OutputWindow;
            buildOutputWindow = BuildOutputWindow;
        }

        /// <summary>
        /// Executes an NAnt Build.
        /// </summary>
        public void Run() {
            Monitor.Enter(addin.buildingMonitor);

            addin.building = true;

            outputWindow.Parent.Visible = true;
            outputWindow.Parent.Activate();

            buildOutputWindow.Clear();
            buildOutputWindow.Activate();

            buildOutputWindow.OutputString("------ Build started: Project: " + project.Name + ", Target: "
                + (target == null ? project.DefaultTarget : target) +
                " ------\n");

            buildOutputWindow.OutputString("\nExecuting Target(s)...\n");

            Level level;
            if (project.Verbose) {
                level = Level.Verbose;
            } else {
                level = Level.Info;
            }

            string location = Assembly.GetExecutingAssembly().Location;
            if (location != "") {
                location = Path.GetDirectoryName(location);
                TypeFactory.ScanDir(location);
            }

            XmlNode nAntConfigurationNode = null;

            if (location != "") {
                string nAntConfig = Path.Combine(location, "NAnt.exe.config");
                if (File.Exists(nAntConfig)) {
                    try {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(new XmlTextReader(nAntConfig));
                        XmlNodeList configNodes = doc.DocumentElement.GetElementsByTagName("nant");
                        if (configNodes.Count > 0) {
                            nAntConfigurationNode = configNodes[0];
                        }
                    }
                    catch(XmlException e) {
                        buildOutputWindow.OutputString("Failed to load NAnt.exe.config:\n");
                        buildOutputWindow.OutputString(e.Message + "\n");
                    }

                }
            }

            NAnt.Core.Project nAntProject = null;
                
            if (nAntConfigurationNode != null) {
                nAntProject = new NAnt.Core.Project(project.FileName, level, 0,
                    nAntConfigurationNode);
            } else {
                nAntProject = new NAnt.Core.Project(project.FileName, level, 0);
            }

            IBuildListener buildListener = new AddinLogListener(buildOutputWindow);
            nAntProject.BuildListeners.Add(buildListener);
            nAntProject.MessageLogged += new BuildEventHandler(buildListener.MessageLogged);
            nAntProject.BuildFinished += new BuildEventHandler(buildListener.BuildFinished);

            if (target != null) {
                System.Collections.Specialized.StringCollection targets = new System.Collections.Specialized.StringCollection();
                targets.Add(target);
                string[] buildTargets = (string[])Array.CreateInstance(typeof(string), targets.Count);
                targets.CopyTo(buildTargets, 0);
                nAntProject.BuildTargets.AddRange(buildTargets);
            }

            bool failed = false;
            try {
                failed = !nAntProject.Run();
            } catch (NAnt.Core.BuildException ex) {
                buildOutputWindow.Activate();
                buildOutputWindow.OutputString(Environment.NewLine + ex.Message);

                if (ex.InnerException != null) {
                    buildOutputWindow.OutputString(
                        Environment.NewLine + ex.InnerException.Message 
                        + Environment.NewLine);
                }

                failed = true;
            }

            buildOutputWindow.OutputString(
                "\n\n---------------------- Done ----------------------\n");
            buildOutputWindow.OutputString(
                "\n                 Build: 1 " + (failed ? "failed" : "succeeded"));

            addin.building = false;

            Monitor.Exit(addin.buildingMonitor);
        }
    }

    /// <summary>
    /// Used to host a .bmp or .ico file as the image for the
    /// <see cref="NAnt.Contrib.NAntAddin.Controls.ScriptExplorerControl"/>
    /// when docked in the tabbed windows including the Solution Explorer.
    /// </summary>
    internal class TabImageHost : System.Windows.Forms.AxHost {
        /// <summary>
        /// Creates a new <see cref="TabImageHost"/>.
        /// </summary>
        public TabImageHost() : base("52D64AAC-29C1-4EC8-BB3A-336F0D3D77CB") {
        }

        /// <summary>
        /// Retrieves an IPicture interface from a <see cref="System.Drawing.Image"/>.
        /// </summary>
        /// <param name="objImage">The <see cref="System.Drawing.Image"/> to convert.</param>
        /// <returns>An IPicture interface containing the Image.</returns>
        public static object GetIPictureDispFromPicture2(Image objImage) {
            return GetIPictureDispFromPicture(objImage);
        }
    }
}