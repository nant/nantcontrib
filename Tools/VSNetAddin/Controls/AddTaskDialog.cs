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
using System.IO;
using System.Xml;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Reflection;
using System.Windows.Forms;
using NAnt.Contrib.NAntAddin.Nodes;
using NAnt.Contrib.NAntAddin.Controls;

namespace NAnt.Contrib.NAntAddin.Dialogs
{
	/// <summary>
	/// Dialog that Adds a Property.
	/// </summary>
	internal class AddTaskDialog : Form
	{
		private NAntTargetNode targetNode;
		private ListView lstTasks;
		private Label lblSelectTask;
		private ColumnHeader columnName;
		private ColumnHeader columnDescription;
		private ImageList taskImages;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label lblDescription;
		private System.Windows.Forms.Button cmdImportTask;
		private ClipboardManager clipboard;
		private TaskNodesTable taskNodesTable;

		/// <summary>
		/// Creates a new Add Property Dialog.
		/// </summary>
		/// <param name="TaskNodesTable">The table containing Task nodes.</param>
		/// <param name="TargetNode">The Target to add the Task to.</param>
		/// <param name="TaskImages">The ImageList containing the Task's image.</param>
		/// <param name="Clipboard">The <see cref="ClipboardManager"/> to use.</param>
		internal AddTaskDialog(TaskNodesTable TaskNodesTable, NAntTargetNode TargetNode, ImageList TaskImages, ClipboardManager Clipboard)
		{
			InitializeComponent();

			taskNodesTable = TaskNodesTable;
			targetNode = TargetNode;
			taskImages = TaskImages;
			clipboard = Clipboard;

			RefreshTasks();
		}

		/// <summary>
		/// Occurs when the "OK" button is clicked.
		/// </summary>
		/// <param name="sender">The object that fired the event</param>
		/// <param name="e">Arguments passed to the event</param>
		private void btnOK_Click(object sender, System.EventArgs e)
		{
			string taskName = lstTasks.SelectedItems[0].Text;
			XmlElement taskElement = targetNode.TargetElement.OwnerDocument.CreateElement(taskName);
			if (clipboard.AddTaskToTarget(taskElement, targetNode, false))
			{
				Close();
			}
		}

		/// <summary>
		/// Occurs when the "Cancel" button is clicked.
		/// </summary>
		/// <param name="sender">The object that fired the event</param>
		/// <param name="e">Arguments passed to the event</param>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void lstTasks_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (lstTasks.SelectedItems.Count > 0)
			{
				btnOK.Enabled = true;
				ListViewItem taskItem = lstTasks.SelectedItems[0];
				lblDescription.Text = taskItem.SubItems[1].Text;
			}
			else
			{
				btnOK.Enabled = false;
				lblDescription.Text = "";
			}
		}

		private void cmdImportTask_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog fileDialog = new OpenFileDialog();
			
			fileDialog.CheckFileExists = true;
			fileDialog.CheckPathExists = true;
			fileDialog.DefaultExt = "dll";
			fileDialog.DereferenceLinks = true;
			fileDialog.Filter = "Assembly with TaskNodes (*.dll)|*.dll|All Files (*.*)|*.*";
			fileDialog.RestoreDirectory = true;
			fileDialog.Multiselect = false;
			fileDialog.ShowHelp = false;
			fileDialog.Title = "Find Assembly Containing NAnt Addin TaskNodes";
			
			DialogResult result = fileDialog.ShowDialog(this);
			
			if (result == DialogResult.OK)
			{
				if (File.Exists(fileDialog.FileName))
				{
					Assembly taskAssembly = null;
					try
					{
						taskAssembly = Assembly.LoadFrom(fileDialog.FileName);
						
						Type[] assemblyTypes = taskAssembly.GetTypes();
						ArrayList nantTaskNodeTypes = new ArrayList();
						foreach (Type type in assemblyTypes)
						{
							if (type.IsSubclassOf(typeof(NAntTaskNode)))
							{
								nantTaskNodeTypes.Add(type);
							}
						}

						if (nantTaskNodeTypes.Count < 1)
						{
							MessageBox.Show(this, 
								"No classes extending NAnt.Contrib.NAntAddin.Nodes.NAntTaskNode found.",
								"No Task Nodes Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);

							return;
						}

						Type[] taskNodeTypes = new Type[nantTaskNodeTypes.Count];
						nantTaskNodeTypes.CopyTo(taskNodeTypes);

						TaskNodeDialog taskNodeDialog = new TaskNodeDialog(taskNodeTypes);
						result = taskNodeDialog.ShowDialog(this);
						if (result == DialogResult.Retry)
						{
							XmlDocument taskNodesDoc = new XmlDocument();
							taskNodesDoc.Load(TaskNodesTable.BaseDir + TaskNodesTable.ADDIN_TASKS_DOC);

							bool taskAdded = false;

							Type[] selectedTypes = taskNodeDialog.SelectedTypes;
							for (int i = 0; i < selectedTypes.Length; i++)
							{
								StringBuilder taskElemPathBuilder = new StringBuilder();
								taskElemPathBuilder.Append("/NAntTaskNodes/NAntTaskNode[@typeName='");
								taskElemPathBuilder.Append(selectedTypes[i].FullName);
								taskElemPathBuilder.Append("']");

								XmlNode taskNode = taskNodesDoc.SelectSingleNode(taskElemPathBuilder.ToString());
								if (taskNode == null)
								{
									XmlElement newTaskElem = taskNodesDoc.CreateElement("NAntTaskNode");

									object[] taskAttributes = selectedTypes[i].GetCustomAttributes(typeof(NAntTaskAttribute), false);
									NAntTaskAttribute taskAttribute = (NAntTaskAttribute)taskAttributes[0];

									newTaskElem.SetAttribute("name", taskAttribute.Name);
									newTaskElem.SetAttribute("typeName", selectedTypes[i].FullName);
									newTaskElem.SetAttribute("assembly", fileDialog.FileName);

									taskNodesDoc.SelectSingleNode("/NAntTaskNodes").AppendChild(newTaskElem);

									taskNodesTable.Add(taskAttribute.Name, selectedTypes[i].FullName);
									try
									{
										Image taskImage = Image.FromFile(
											TaskNodesTable.BaseDir + 
											@"TaskImages\" + 
											taskAttribute.Image);

										int imageIndex = taskImages.Images.Add(
											taskImage, Color.FromArgb(255, 0, 255));

										taskNodesTable.TaskInfoTable.Add(taskAttribute.Name, new TaskInfo(fileDialog.FileName, imageIndex));
									}
									catch (Exception)
									{
										taskNodesTable.TaskInfoTable.Add(taskAttribute.Name, new TaskInfo(fileDialog.FileName, 0));
									}

									taskAdded = true;
								}
							}

							if (taskAdded)
							{
								taskNodesDoc.Save(TaskNodesTable.BaseDir + TaskNodesTable.ADDIN_TASKS_DOC);
								RefreshTasks();
							}
						}
					}
					catch (Exception ex)
					{
						MessageBox.Show(this, "Unable to read assembly.\n\n" + ex.Message, 
							"Invalid File", MessageBoxButtons.OK,
							MessageBoxIcon.Error);
					}
				}
				else
				{
					MessageBox.Show(this, "File selected does not exist", 
						"Invalid Filename", MessageBoxButtons.OK, 
						MessageBoxIcon.Error);
				}
			}
		}

		private void RefreshTasks()
		{
			lstTasks.SmallImageList = taskImages;
			lstTasks.LargeImageList = taskImages;

			lstTasks.Clear();

			IEnumerator enumTaskNodes = clipboard.tasksTable.Keys.GetEnumerator();
			while (enumTaskNodes.MoveNext())
			{
				string taskName = (string)enumTaskNodes.Current;
				Type taskType = clipboard.tasksTable.GetTaskNodeType(taskName);

				if (taskType != null)
				{
					object[] taskAttributes = taskType.GetCustomAttributes(typeof(NAntTaskAttribute), false);
					if (taskAttributes.Length > 0)
					{
						NAntTaskAttribute taskAttribute = (NAntTaskAttribute)taskAttributes[0];

						ListViewItem taskItem = lstTasks.Items.Add(taskAttribute.Name);
						taskItem.SubItems.Add(taskAttribute.Description);

						taskItem.ImageIndex = clipboard.tasksTable.GetTaskNodeImageIndex(taskAttribute.Name);
					}
				}
			}
		}

		#region Visual Studio Designer Code
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AddTaskDialog));
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.lstTasks = new System.Windows.Forms.ListView();
			this.columnName = new System.Windows.Forms.ColumnHeader();
			this.columnDescription = new System.Windows.Forms.ColumnHeader();
			this.lblSelectTask = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.lblDescription = new System.Windows.Forms.Label();
			this.cmdImportTask = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnOK
			// 
			this.btnOK.AccessibleDescription = ((string)(resources.GetObject("btnOK.AccessibleDescription")));
			this.btnOK.AccessibleName = ((string)(resources.GetObject("btnOK.AccessibleName")));
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnOK.Anchor")));
			this.btnOK.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnOK.BackgroundImage")));
			this.btnOK.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnOK.Dock")));
			this.btnOK.Enabled = ((bool)(resources.GetObject("btnOK.Enabled")));
			this.btnOK.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnOK.FlatStyle")));
			this.btnOK.Font = ((System.Drawing.Font)(resources.GetObject("btnOK.Font")));
			this.btnOK.Image = ((System.Drawing.Image)(resources.GetObject("btnOK.Image")));
			this.btnOK.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnOK.ImageAlign")));
			this.btnOK.ImageIndex = ((int)(resources.GetObject("btnOK.ImageIndex")));
			this.btnOK.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnOK.ImeMode")));
			this.btnOK.Location = ((System.Drawing.Point)(resources.GetObject("btnOK.Location")));
			this.btnOK.Name = "btnOK";
			this.btnOK.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnOK.RightToLeft")));
			this.btnOK.Size = ((System.Drawing.Size)(resources.GetObject("btnOK.Size")));
			this.btnOK.TabIndex = ((int)(resources.GetObject("btnOK.TabIndex")));
			this.btnOK.Text = resources.GetString("btnOK.Text");
			this.btnOK.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnOK.TextAlign")));
			this.btnOK.Visible = ((bool)(resources.GetObject("btnOK.Visible")));
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.AccessibleDescription = ((string)(resources.GetObject("btnCancel.AccessibleDescription")));
			this.btnCancel.AccessibleName = ((string)(resources.GetObject("btnCancel.AccessibleName")));
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnCancel.Anchor")));
			this.btnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnCancel.BackgroundImage")));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnCancel.Dock")));
			this.btnCancel.Enabled = ((bool)(resources.GetObject("btnCancel.Enabled")));
			this.btnCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnCancel.FlatStyle")));
			this.btnCancel.Font = ((System.Drawing.Font)(resources.GetObject("btnCancel.Font")));
			this.btnCancel.Image = ((System.Drawing.Image)(resources.GetObject("btnCancel.Image")));
			this.btnCancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnCancel.ImageAlign")));
			this.btnCancel.ImageIndex = ((int)(resources.GetObject("btnCancel.ImageIndex")));
			this.btnCancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnCancel.ImeMode")));
			this.btnCancel.Location = ((System.Drawing.Point)(resources.GetObject("btnCancel.Location")));
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnCancel.RightToLeft")));
			this.btnCancel.Size = ((System.Drawing.Size)(resources.GetObject("btnCancel.Size")));
			this.btnCancel.TabIndex = ((int)(resources.GetObject("btnCancel.TabIndex")));
			this.btnCancel.Text = resources.GetString("btnCancel.Text");
			this.btnCancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnCancel.TextAlign")));
			this.btnCancel.Visible = ((bool)(resources.GetObject("btnCancel.Visible")));
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// lstTasks
			// 
			this.lstTasks.AccessibleDescription = ((string)(resources.GetObject("lstTasks.AccessibleDescription")));
			this.lstTasks.AccessibleName = ((string)(resources.GetObject("lstTasks.AccessibleName")));
			this.lstTasks.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("lstTasks.Alignment")));
			this.lstTasks.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lstTasks.Anchor")));
			this.lstTasks.BackColor = System.Drawing.Color.White;
			this.lstTasks.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("lstTasks.BackgroundImage")));
			this.lstTasks.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.lstTasks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					   this.columnName,
																					   this.columnDescription});
			this.lstTasks.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lstTasks.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lstTasks.Dock")));
			this.lstTasks.Enabled = ((bool)(resources.GetObject("lstTasks.Enabled")));
			this.lstTasks.Font = ((System.Drawing.Font)(resources.GetObject("lstTasks.Font")));
			this.lstTasks.FullRowSelect = true;
			this.lstTasks.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.lstTasks.HoverSelection = true;
			this.lstTasks.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lstTasks.ImeMode")));
			this.lstTasks.LabelWrap = ((bool)(resources.GetObject("lstTasks.LabelWrap")));
			this.lstTasks.Location = ((System.Drawing.Point)(resources.GetObject("lstTasks.Location")));
			this.lstTasks.MultiSelect = false;
			this.lstTasks.Name = "lstTasks";
			this.lstTasks.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lstTasks.RightToLeft")));
			this.lstTasks.Size = ((System.Drawing.Size)(resources.GetObject("lstTasks.Size")));
			this.lstTasks.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.lstTasks.TabIndex = ((int)(resources.GetObject("lstTasks.TabIndex")));
			this.lstTasks.Text = resources.GetString("lstTasks.Text");
			this.lstTasks.Visible = ((bool)(resources.GetObject("lstTasks.Visible")));
			this.lstTasks.SelectedIndexChanged += new System.EventHandler(this.lstTasks_SelectedIndexChanged);
			// 
			// columnName
			// 
			this.columnName.Text = resources.GetString("columnName.Text");
			this.columnName.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("columnName.TextAlign")));
			this.columnName.Width = ((int)(resources.GetObject("columnName.Width")));
			// 
			// columnDescription
			// 
			this.columnDescription.Text = resources.GetString("columnDescription.Text");
			this.columnDescription.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("columnDescription.TextAlign")));
			this.columnDescription.Width = ((int)(resources.GetObject("columnDescription.Width")));
			// 
			// lblSelectTask
			// 
			this.lblSelectTask.AccessibleDescription = ((string)(resources.GetObject("lblSelectTask.AccessibleDescription")));
			this.lblSelectTask.AccessibleName = ((string)(resources.GetObject("lblSelectTask.AccessibleName")));
			this.lblSelectTask.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblSelectTask.Anchor")));
			this.lblSelectTask.AutoSize = ((bool)(resources.GetObject("lblSelectTask.AutoSize")));
			this.lblSelectTask.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblSelectTask.Dock")));
			this.lblSelectTask.Enabled = ((bool)(resources.GetObject("lblSelectTask.Enabled")));
			this.lblSelectTask.Font = ((System.Drawing.Font)(resources.GetObject("lblSelectTask.Font")));
			this.lblSelectTask.Image = ((System.Drawing.Image)(resources.GetObject("lblSelectTask.Image")));
			this.lblSelectTask.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblSelectTask.ImageAlign")));
			this.lblSelectTask.ImageIndex = ((int)(resources.GetObject("lblSelectTask.ImageIndex")));
			this.lblSelectTask.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblSelectTask.ImeMode")));
			this.lblSelectTask.Location = ((System.Drawing.Point)(resources.GetObject("lblSelectTask.Location")));
			this.lblSelectTask.Name = "lblSelectTask";
			this.lblSelectTask.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblSelectTask.RightToLeft")));
			this.lblSelectTask.Size = ((System.Drawing.Size)(resources.GetObject("lblSelectTask.Size")));
			this.lblSelectTask.TabIndex = ((int)(resources.GetObject("lblSelectTask.TabIndex")));
			this.lblSelectTask.Text = resources.GetString("lblSelectTask.Text");
			this.lblSelectTask.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblSelectTask.TextAlign")));
			this.lblSelectTask.Visible = ((bool)(resources.GetObject("lblSelectTask.Visible")));
			// 
			// panel1
			// 
			this.panel1.AccessibleDescription = ((string)(resources.GetObject("panel1.AccessibleDescription")));
			this.panel1.AccessibleName = ((string)(resources.GetObject("panel1.AccessibleName")));
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panel1.Anchor")));
			this.panel1.AutoScroll = ((bool)(resources.GetObject("panel1.AutoScroll")));
			this.panel1.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panel1.AutoScrollMargin")));
			this.panel1.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panel1.AutoScrollMinSize")));
			this.panel1.BackColor = System.Drawing.Color.Gainsboro;
			this.panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
			this.panel1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panel1.Dock")));
			this.panel1.Enabled = ((bool)(resources.GetObject("panel1.Enabled")));
			this.panel1.Font = ((System.Drawing.Font)(resources.GetObject("panel1.Font")));
			this.panel1.ForeColor = System.Drawing.Color.LightBlue;
			this.panel1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panel1.ImeMode")));
			this.panel1.Location = ((System.Drawing.Point)(resources.GetObject("panel1.Location")));
			this.panel1.Name = "panel1";
			this.panel1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panel1.RightToLeft")));
			this.panel1.Size = ((System.Drawing.Size)(resources.GetObject("panel1.Size")));
			this.panel1.TabIndex = ((int)(resources.GetObject("panel1.TabIndex")));
			this.panel1.Text = resources.GetString("panel1.Text");
			this.panel1.Visible = ((bool)(resources.GetObject("panel1.Visible")));
			// 
			// lblDescription
			// 
			this.lblDescription.AccessibleDescription = ((string)(resources.GetObject("lblDescription.AccessibleDescription")));
			this.lblDescription.AccessibleName = ((string)(resources.GetObject("lblDescription.AccessibleName")));
			this.lblDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblDescription.Anchor")));
			this.lblDescription.AutoSize = ((bool)(resources.GetObject("lblDescription.AutoSize")));
			this.lblDescription.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblDescription.Dock")));
			this.lblDescription.Enabled = ((bool)(resources.GetObject("lblDescription.Enabled")));
			this.lblDescription.Font = ((System.Drawing.Font)(resources.GetObject("lblDescription.Font")));
			this.lblDescription.Image = ((System.Drawing.Image)(resources.GetObject("lblDescription.Image")));
			this.lblDescription.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblDescription.ImageAlign")));
			this.lblDescription.ImageIndex = ((int)(resources.GetObject("lblDescription.ImageIndex")));
			this.lblDescription.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblDescription.ImeMode")));
			this.lblDescription.Location = ((System.Drawing.Point)(resources.GetObject("lblDescription.Location")));
			this.lblDescription.Name = "lblDescription";
			this.lblDescription.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblDescription.RightToLeft")));
			this.lblDescription.Size = ((System.Drawing.Size)(resources.GetObject("lblDescription.Size")));
			this.lblDescription.TabIndex = ((int)(resources.GetObject("lblDescription.TabIndex")));
			this.lblDescription.Text = resources.GetString("lblDescription.Text");
			this.lblDescription.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblDescription.TextAlign")));
			this.lblDescription.Visible = ((bool)(resources.GetObject("lblDescription.Visible")));
			// 
			// cmdImportTask
			// 
			this.cmdImportTask.AccessibleDescription = ((string)(resources.GetObject("cmdImportTask.AccessibleDescription")));
			this.cmdImportTask.AccessibleName = ((string)(resources.GetObject("cmdImportTask.AccessibleName")));
			this.cmdImportTask.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cmdImportTask.Anchor")));
			this.cmdImportTask.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cmdImportTask.BackgroundImage")));
			this.cmdImportTask.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cmdImportTask.Dock")));
			this.cmdImportTask.Enabled = ((bool)(resources.GetObject("cmdImportTask.Enabled")));
			this.cmdImportTask.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("cmdImportTask.FlatStyle")));
			this.cmdImportTask.Font = ((System.Drawing.Font)(resources.GetObject("cmdImportTask.Font")));
			this.cmdImportTask.Image = ((System.Drawing.Image)(resources.GetObject("cmdImportTask.Image")));
			this.cmdImportTask.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cmdImportTask.ImageAlign")));
			this.cmdImportTask.ImageIndex = ((int)(resources.GetObject("cmdImportTask.ImageIndex")));
			this.cmdImportTask.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cmdImportTask.ImeMode")));
			this.cmdImportTask.Location = ((System.Drawing.Point)(resources.GetObject("cmdImportTask.Location")));
			this.cmdImportTask.Name = "cmdImportTask";
			this.cmdImportTask.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cmdImportTask.RightToLeft")));
			this.cmdImportTask.Size = ((System.Drawing.Size)(resources.GetObject("cmdImportTask.Size")));
			this.cmdImportTask.TabIndex = ((int)(resources.GetObject("cmdImportTask.TabIndex")));
			this.cmdImportTask.Text = resources.GetString("cmdImportTask.Text");
			this.cmdImportTask.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cmdImportTask.TextAlign")));
			this.cmdImportTask.Visible = ((bool)(resources.GetObject("cmdImportTask.Visible")));
			this.cmdImportTask.Click += new System.EventHandler(this.cmdImportTask_Click);
			// 
			// AddTaskDialog
			// 
			this.AcceptButton = this.btnOK;
			this.AccessibleDescription = ((string)(resources.GetObject("$this.AccessibleDescription")));
			this.AccessibleName = ((string)(resources.GetObject("$this.AccessibleName")));
			this.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("$this.Anchor")));
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackColor = System.Drawing.Color.White;
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.btnCancel;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.cmdImportTask,
																		  this.lblDescription,
																		  this.panel1,
																		  this.lblSelectTask,
																		  this.lstTasks,
																		  this.btnCancel,
																		  this.btnOK});
			this.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("$this.Dock")));
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "AddTaskDialog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Visible = ((bool)(resources.GetObject("$this.Visible")));
			this.ResumeLayout(false);

		}
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;

		private System.ComponentModel.IContainer components = null;
		#endregion
	}
}