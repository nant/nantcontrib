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
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using NAnt.Contrib.NAntAddin.Nodes;

namespace NAnt.Contrib.NAntAddin.Dialogs
{
	/// <summary>
	/// Dialog that selects types from an assembly that 
	/// inherit from <see cref="NAntTaskNode"/>.
	/// </summary>
	/// <remarks>None.</remarks>
	internal class TaskNodeDialog : Form
	{
		private Type[] nodeTypes;

		/// <summary>
		/// Creates a new <see cref="TaskNodeDialog"/>.
		/// </summary>
		/// <param name="NodeTypes">The types of nodes that can be selected from.</param>
		/// <remarks>None.</remarks>
		internal TaskNodeDialog(Type[] NodeTypes)
		{
			InitializeComponent();

			nodeTypes = NodeTypes;

			for (int i = 0; i < nodeTypes.Length; i++)
			{
				Type nodeType = nodeTypes[i];
				
				object[] taskAttributes = nodeType.GetCustomAttributes(typeof(NAntTaskAttribute), false);
				if (taskAttributes.Length > 0)
				{
					NAntTaskAttribute taskAttribute = (NAntTaskAttribute)taskAttributes[0];

					ListViewItem taskItem = lstTasks.Items.Add(taskAttribute.Name);
					taskItem.UseItemStyleForSubItems = false;

					if (!File.Exists(TaskNodesTable.BaseDir + @"TaskImages\" + taskAttribute.Image))
					{
						taskItem.SubItems.Add(taskAttribute.Image, Color.Red, SystemColors.Window, lstTasks.Font);
					}
					else
					{
						taskItem.SubItems.Add(taskAttribute.Image);
					}
				}
			}
		}

		/// <summary>
		/// Retrieves the types that were selected.
		/// </summary>
		/// <value>The types that were selected.</value>
		/// <remarks>None.</remarks>
		public Type[] SelectedTypes
		{
			get
			{
				Type[] selectedTypes = new Type[lstTasks.SelectedItems.Count];
				for (int i = 0; i < selectedTypes.Length; i++)
				{
					selectedTypes[i] = nodeTypes[lstTasks.SelectedItems[i].Index];
				}
				return selectedTypes;
			}
		}

		/// <summary>
		/// Occurs when the "OK" button is clicked.
		/// </summary>
		/// <param name="sender">The object that fired the event</param>
		/// <param name="e">Arguments passed to the event</param>
		/// <remarks>None.</remarks>
		private void btnOK_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Retry;
			Close();
		}

		#region Visual Studio Designer Code
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TaskNodeDialog));
			this.grpTaskNodes = new System.Windows.Forms.GroupBox();
			this.lstTasks = new System.Windows.Forms.ListView();
			this.columnType = new System.Windows.Forms.ColumnHeader();
			this.columnImage = new System.Windows.Forms.ColumnHeader();
			this.btnOK = new System.Windows.Forms.Button();
			this.cmdCancel = new System.Windows.Forms.Button();
			this.btnLocateImage = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.grpTaskNodes.SuspendLayout();
			this.SuspendLayout();
			// 
			// grpTaskNodes
			// 
			this.grpTaskNodes.AccessibleDescription = ((string)(resources.GetObject("grpTaskNodes.AccessibleDescription")));
			this.grpTaskNodes.AccessibleName = ((string)(resources.GetObject("grpTaskNodes.AccessibleName")));
			this.grpTaskNodes.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("grpTaskNodes.Anchor")));
			this.grpTaskNodes.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("grpTaskNodes.BackgroundImage")));
			this.grpTaskNodes.Controls.AddRange(new System.Windows.Forms.Control[] {
																					   this.lstTasks});
			this.grpTaskNodes.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("grpTaskNodes.Dock")));
			this.grpTaskNodes.Enabled = ((bool)(resources.GetObject("grpTaskNodes.Enabled")));
			this.grpTaskNodes.Font = ((System.Drawing.Font)(resources.GetObject("grpTaskNodes.Font")));
			this.grpTaskNodes.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("grpTaskNodes.ImeMode")));
			this.grpTaskNodes.Location = ((System.Drawing.Point)(resources.GetObject("grpTaskNodes.Location")));
			this.grpTaskNodes.Name = "grpTaskNodes";
			this.grpTaskNodes.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("grpTaskNodes.RightToLeft")));
			this.grpTaskNodes.Size = ((System.Drawing.Size)(resources.GetObject("grpTaskNodes.Size")));
			this.grpTaskNodes.TabIndex = ((int)(resources.GetObject("grpTaskNodes.TabIndex")));
			this.grpTaskNodes.TabStop = false;
			this.grpTaskNodes.Text = resources.GetString("grpTaskNodes.Text");
			this.grpTaskNodes.Visible = ((bool)(resources.GetObject("grpTaskNodes.Visible")));
			// 
			// lstTasks
			// 
			this.lstTasks.AccessibleDescription = ((string)(resources.GetObject("lstTasks.AccessibleDescription")));
			this.lstTasks.AccessibleName = ((string)(resources.GetObject("lstTasks.AccessibleName")));
			this.lstTasks.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("lstTasks.Alignment")));
			this.lstTasks.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lstTasks.Anchor")));
			this.lstTasks.BackColor = System.Drawing.SystemColors.Window;
			this.lstTasks.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("lstTasks.BackgroundImage")));
			this.lstTasks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					   this.columnType,
																					   this.columnImage});
			this.lstTasks.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lstTasks.Dock")));
			this.lstTasks.Enabled = ((bool)(resources.GetObject("lstTasks.Enabled")));
			this.lstTasks.Font = ((System.Drawing.Font)(resources.GetObject("lstTasks.Font")));
			this.lstTasks.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lstTasks.ImeMode")));
			this.lstTasks.LabelWrap = ((bool)(resources.GetObject("lstTasks.LabelWrap")));
			this.lstTasks.Location = ((System.Drawing.Point)(resources.GetObject("lstTasks.Location")));
			this.lstTasks.Name = "lstTasks";
			this.lstTasks.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lstTasks.RightToLeft")));
			this.lstTasks.Size = ((System.Drawing.Size)(resources.GetObject("lstTasks.Size")));
			this.lstTasks.TabIndex = ((int)(resources.GetObject("lstTasks.TabIndex")));
			this.lstTasks.Text = resources.GetString("lstTasks.Text");
			this.lstTasks.View = System.Windows.Forms.View.Details;
			this.lstTasks.Visible = ((bool)(resources.GetObject("lstTasks.Visible")));
			this.lstTasks.SelectedIndexChanged += new System.EventHandler(this.lstTasks_SelectedIndexChanged);
			// 
			// columnType
			// 
			this.columnType.Text = resources.GetString("columnType.Text");
			this.columnType.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("columnType.TextAlign")));
			this.columnType.Width = ((int)(resources.GetObject("columnType.Width")));
			// 
			// columnImage
			// 
			this.columnImage.Text = resources.GetString("columnImage.Text");
			this.columnImage.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("columnImage.TextAlign")));
			this.columnImage.Width = ((int)(resources.GetObject("columnImage.Width")));
			// 
			// btnOK
			// 
			this.btnOK.AccessibleDescription = ((string)(resources.GetObject("btnOK.AccessibleDescription")));
			this.btnOK.AccessibleName = ((string)(resources.GetObject("btnOK.AccessibleName")));
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnOK.Anchor")));
			this.btnOK.BackColor = System.Drawing.SystemColors.Control;
			this.btnOK.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnOK.BackgroundImage")));
			this.btnOK.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnOK.Dock")));
			this.btnOK.Enabled = ((bool)(resources.GetObject("btnOK.Enabled")));
			this.btnOK.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnOK.FlatStyle")));
			this.btnOK.Font = ((System.Drawing.Font)(resources.GetObject("btnOK.Font")));
			this.btnOK.ForeColor = System.Drawing.Color.Black;
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
			// cmdCancel
			// 
			this.cmdCancel.AccessibleDescription = ((string)(resources.GetObject("cmdCancel.AccessibleDescription")));
			this.cmdCancel.AccessibleName = ((string)(resources.GetObject("cmdCancel.AccessibleName")));
			this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cmdCancel.Anchor")));
			this.cmdCancel.BackColor = System.Drawing.SystemColors.Control;
			this.cmdCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cmdCancel.BackgroundImage")));
			this.cmdCancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cmdCancel.Dock")));
			this.cmdCancel.Enabled = ((bool)(resources.GetObject("cmdCancel.Enabled")));
			this.cmdCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("cmdCancel.FlatStyle")));
			this.cmdCancel.Font = ((System.Drawing.Font)(resources.GetObject("cmdCancel.Font")));
			this.cmdCancel.ForeColor = System.Drawing.Color.Black;
			this.cmdCancel.Image = ((System.Drawing.Image)(resources.GetObject("cmdCancel.Image")));
			this.cmdCancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cmdCancel.ImageAlign")));
			this.cmdCancel.ImageIndex = ((int)(resources.GetObject("cmdCancel.ImageIndex")));
			this.cmdCancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cmdCancel.ImeMode")));
			this.cmdCancel.Location = ((System.Drawing.Point)(resources.GetObject("cmdCancel.Location")));
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cmdCancel.RightToLeft")));
			this.cmdCancel.Size = ((System.Drawing.Size)(resources.GetObject("cmdCancel.Size")));
			this.cmdCancel.TabIndex = ((int)(resources.GetObject("cmdCancel.TabIndex")));
			this.cmdCancel.Text = resources.GetString("cmdCancel.Text");
			this.cmdCancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cmdCancel.TextAlign")));
			this.cmdCancel.Visible = ((bool)(resources.GetObject("cmdCancel.Visible")));
			this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
			// 
			// btnLocateImage
			// 
			this.btnLocateImage.AccessibleDescription = ((string)(resources.GetObject("btnLocateImage.AccessibleDescription")));
			this.btnLocateImage.AccessibleName = ((string)(resources.GetObject("btnLocateImage.AccessibleName")));
			this.btnLocateImage.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnLocateImage.Anchor")));
			this.btnLocateImage.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnLocateImage.BackgroundImage")));
			this.btnLocateImage.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnLocateImage.Dock")));
			this.btnLocateImage.Enabled = ((bool)(resources.GetObject("btnLocateImage.Enabled")));
			this.btnLocateImage.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnLocateImage.FlatStyle")));
			this.btnLocateImage.Font = ((System.Drawing.Font)(resources.GetObject("btnLocateImage.Font")));
			this.btnLocateImage.Image = ((System.Drawing.Image)(resources.GetObject("btnLocateImage.Image")));
			this.btnLocateImage.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnLocateImage.ImageAlign")));
			this.btnLocateImage.ImageIndex = ((int)(resources.GetObject("btnLocateImage.ImageIndex")));
			this.btnLocateImage.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnLocateImage.ImeMode")));
			this.btnLocateImage.Location = ((System.Drawing.Point)(resources.GetObject("btnLocateImage.Location")));
			this.btnLocateImage.Name = "btnLocateImage";
			this.btnLocateImage.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnLocateImage.RightToLeft")));
			this.btnLocateImage.Size = ((System.Drawing.Size)(resources.GetObject("btnLocateImage.Size")));
			this.btnLocateImage.TabIndex = ((int)(resources.GetObject("btnLocateImage.TabIndex")));
			this.btnLocateImage.Text = resources.GetString("btnLocateImage.Text");
			this.btnLocateImage.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnLocateImage.TextAlign")));
			this.btnLocateImage.Visible = ((bool)(resources.GetObject("btnLocateImage.Visible")));
			this.btnLocateImage.Click += new System.EventHandler(this.btnLocateImage_Click);
			// 
			// label1
			// 
			this.label1.AccessibleDescription = ((string)(resources.GetObject("label1.AccessibleDescription")));
			this.label1.AccessibleName = ((string)(resources.GetObject("label1.AccessibleName")));
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label1.Anchor")));
			this.label1.AutoSize = ((bool)(resources.GetObject("label1.AutoSize")));
			this.label1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label1.Dock")));
			this.label1.Enabled = ((bool)(resources.GetObject("label1.Enabled")));
			this.label1.Font = ((System.Drawing.Font)(resources.GetObject("label1.Font")));
			this.label1.Image = ((System.Drawing.Image)(resources.GetObject("label1.Image")));
			this.label1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.ImageAlign")));
			this.label1.ImageIndex = ((int)(resources.GetObject("label1.ImageIndex")));
			this.label1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label1.ImeMode")));
			this.label1.Location = ((System.Drawing.Point)(resources.GetObject("label1.Location")));
			this.label1.Name = "label1";
			this.label1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label1.RightToLeft")));
			this.label1.Size = ((System.Drawing.Size)(resources.GetObject("label1.Size")));
			this.label1.TabIndex = ((int)(resources.GetObject("label1.TabIndex")));
			this.label1.Text = resources.GetString("label1.Text");
			this.label1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.TextAlign")));
			this.label1.Visible = ((bool)(resources.GetObject("label1.Visible")));
			// 
			// TaskNodeDialog
			// 
			this.AccessibleDescription = ((string)(resources.GetObject("$this.AccessibleDescription")));
			this.AccessibleName = ((string)(resources.GetObject("$this.AccessibleName")));
			this.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("$this.Anchor")));
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.label1,
																		  this.btnLocateImage,
																		  this.cmdCancel,
																		  this.btnOK,
																		  this.grpTaskNodes});
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
			this.Name = "TaskNodeDialog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Visible = ((bool)(resources.GetObject("$this.Visible")));
			this.grpTaskNodes.ResumeLayout(false);
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
		private System.Windows.Forms.GroupBox grpTaskNodes;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.ListView lstTasks;
		private System.Windows.Forms.ColumnHeader columnType;
		private System.Windows.Forms.ColumnHeader columnImage;
		private System.Windows.Forms.Button cmdCancel;
		private System.Windows.Forms.Button btnLocateImage;
		private System.Windows.Forms.Label label1;

		private System.ComponentModel.IContainer components = null;
		#endregion

		private void cmdCancel_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void btnLocateImage_Click(object sender, System.EventArgs e)
		{
			if (lstTasks.SelectedItems.Count > 1)
			{
				MessageBox.Show(this, "Please select only one node at a time when locating the image.",
					"Multiple Nodes Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if (lstTasks.SelectedItems.Count == 0)
			{
				MessageBox.Show(this, "Please select a node first to locate its image.",
					"No Node Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			ListViewItem taskItem = lstTasks.SelectedItems[0];
			Type selectedType = nodeTypes[taskItem.Index];

			object[] taskAttributes = selectedType.GetCustomAttributes(typeof(NAntTaskAttribute), false);
			NAntTaskAttribute taskAttribute = (NAntTaskAttribute)taskAttributes[0];

			if (!File.Exists(TaskNodesTable.BaseDir + "TaskImages" + taskAttribute.Image))
			{
				taskItem.SubItems[1].ForeColor = Color.Red;
			}
			else
			{
				taskItem.SubItems[1].ForeColor = lstTasks.ForeColor;
				MessageBox.Show(this, "Selected node already has a locateable image.",
					"Image Already Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			OpenFileDialog fileDialog = new OpenFileDialog();
			fileDialog.CheckFileExists = true;
			fileDialog.CheckPathExists = true;
			fileDialog.DefaultExt = "dll";
			fileDialog.DereferenceLinks = true;
			fileDialog.Filter = "16x16 Bitmap (*.bmp)|*.bmp";
			fileDialog.RestoreDirectory = true;
			fileDialog.Multiselect = false;
			fileDialog.ShowHelp = false;
			fileDialog.Title = "Find Image for NAnt Task Node";
			DialogResult result = fileDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				if (File.Exists(fileDialog.FileName))
				{
					string fileName = Path.GetFileName(fileDialog.FileName);
					string path = TaskNodesTable.BaseDir + @"TaskImages\" + 
						taskAttribute.Image;

					File.Copy(fileDialog.FileName, path);

					taskItem.SubItems[1].ForeColor = lstTasks.ForeColor;
					btnLocateImage.Enabled = false;
					btnOK.Enabled = true;
				}
			}
		}

		private void lstTasks_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (lstTasks.SelectedItems.Count == 1)
			{
				ListViewItem taskItem = lstTasks.SelectedItems[0];
				Type selectedType = nodeTypes[taskItem.Index];

				object[] taskAttributes = selectedType.GetCustomAttributes(typeof(NAntTaskAttribute), false);
				NAntTaskAttribute taskAttribute = (NAntTaskAttribute)taskAttributes[0];

				if (!File.Exists(TaskNodesTable.BaseDir + @"TaskImages\" + taskAttribute.Image))
				{
					taskItem.SubItems[1].ForeColor = Color.Red;
					btnOK.Enabled = true;
					btnLocateImage.Enabled = true;
				}
				else
				{
					taskItem.SubItems[1].ForeColor = lstTasks.ForeColor;
					btnOK.Enabled = false;
					btnLocateImage.Enabled = false;
				}
			}
			
			if (lstTasks.SelectedItems.Count > 0)
			{
				bool missingImage = false;

				if (lstTasks.SelectedItems.Count != 1)
				{
					btnLocateImage.Enabled = false;
				}

				for (int i = 0; i < lstTasks.SelectedItems.Count; i++)
				{
					ListViewItem taskItem = lstTasks.SelectedItems[i];

					Type selectedType = nodeTypes[taskItem.Index];

					object[] taskAttributes = selectedType.GetCustomAttributes(typeof(NAntTaskAttribute), false);
					NAntTaskAttribute taskAttribute = (NAntTaskAttribute)taskAttributes[0];

					if (!File.Exists(TaskNodesTable.BaseDir + @"TaskImages\" + taskAttribute.Image))
					{
						taskItem.SubItems[1].ForeColor = Color.Red;
						missingImage = true;
						break;
					}
				}

				btnOK.Enabled = !missingImage;
			}
		}
	}
}