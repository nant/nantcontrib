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
using System.Reflection;
using System.Windows.Forms;
using NAnt.Contrib.NAntAddin.Nodes;
using NAnt.Contrib.NAntAddin.Controls;

namespace NAnt.Contrib.NAntAddin.Dialogs
{
	/// <summary>
	/// Dialog that Adds a Property.
	/// </summary>
	internal class AddPropertyDialog : Form
	{
		private NAntProjectNode projectNode;
		private NAntTargetNode targetNode;
		private ClipboardManager clipboard;

		/// <summary>
		/// Creates a new Add Property Dialog.
		/// </summary>
		/// <param name="ProjectNode">The Project to add the Property to</param>
		/// <param name="Clipboard">The <see cref="ClipboardManager"/> to use.</param>
		internal AddPropertyDialog(NAntProjectNode ProjectNode, ClipboardManager Clipboard)
		{
			InitializeComponent();

			projectNode = ProjectNode;
			clipboard = Clipboard;

			Text = Text + " to NAnt Project";
		}

		/// <summary>
		/// Creates a new Add Property Dialog.
		/// </summary>
		/// <param name="TargetNode">The Target to add the Property to</param>
		/// <param name="Clipboard">The <see cref="ClipboardManager"/> to use.</param>
		internal AddPropertyDialog(NAntTargetNode TargetNode, ClipboardManager Clipboard)
		{
			InitializeComponent();

			targetNode = TargetNode;
			clipboard = Clipboard;

			Text = Text + " to NAnt Target";
		}

		/// <summary>
		/// Occurs when the "OK" button is clicked.
		/// </summary>
		/// <param name="sender">The object that fired the event</param>
		/// <param name="e">Arguments passed to the event</param>
		private void btnOK_Click(object sender, System.EventArgs e)
		{
			//
			// Validate Fields
			//
			if (txtPropertyName.Text.Length < 1)
			{
				MessageBox.Show("The Name field is required.", "Invalid Name", 
					MessageBoxButtons.OK, MessageBoxIcon.Stop);
				return;
			}
			else if (txtPropertyValue.Text.Length < 1)
			{
				MessageBox.Show("The Value field is required.", "Invalid Value", 
					MessageBoxButtons.OK, MessageBoxIcon.Stop);
				return;
			}

			//
			// Get the XML Element and TreeNode to Add to
			//
			XmlElement propertyContainerElem = null;
			TreeNode propertyContainerNode = null;
			if (projectNode != null)
			{
				propertyContainerElem = projectNode.ProjectDocument.DocumentElement;
				propertyContainerNode = projectNode;
			}
			else
			{
				propertyContainerElem = targetNode.TargetElement;
				propertyContainerNode = targetNode;
			}	

			//
			// Create the Property XML Element
			//
			XmlElement propElement = 
				propertyContainerElem.OwnerDocument.CreateElement("property");

			propElement.SetAttribute("name", txtPropertyName.Text);
			propElement.SetAttribute("value", txtPropertyValue.Text);

			if (projectNode != null)
			{
				if (clipboard.AddPropertyToParent(
					propElement, projectNode, 
					projectNode.ProjectDocument.DocumentElement))
				{
					Close();
				}
			}
			else if (targetNode != null)
			{
				if (clipboard.AddPropertyToParent(
					propElement, targetNode, 
					targetNode.TargetElement))
				{
					Close();
				}
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

		#region Visual Studio Designer Code
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AddPropertyDialog));
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.lblPropertyName = new System.Windows.Forms.Label();
			this.txtPropertyName = new System.Windows.Forms.TextBox();
			this.lblPropertyValue = new System.Windows.Forms.Label();
			this.txtPropertyValue = new System.Windows.Forms.TextBox();
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
			// lblPropertyName
			// 
			this.lblPropertyName.AccessibleDescription = ((string)(resources.GetObject("lblPropertyName.AccessibleDescription")));
			this.lblPropertyName.AccessibleName = ((string)(resources.GetObject("lblPropertyName.AccessibleName")));
			this.lblPropertyName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblPropertyName.Anchor")));
			this.lblPropertyName.AutoSize = ((bool)(resources.GetObject("lblPropertyName.AutoSize")));
			this.lblPropertyName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblPropertyName.Dock")));
			this.lblPropertyName.Enabled = ((bool)(resources.GetObject("lblPropertyName.Enabled")));
			this.lblPropertyName.Font = ((System.Drawing.Font)(resources.GetObject("lblPropertyName.Font")));
			this.lblPropertyName.Image = ((System.Drawing.Image)(resources.GetObject("lblPropertyName.Image")));
			this.lblPropertyName.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblPropertyName.ImageAlign")));
			this.lblPropertyName.ImageIndex = ((int)(resources.GetObject("lblPropertyName.ImageIndex")));
			this.lblPropertyName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblPropertyName.ImeMode")));
			this.lblPropertyName.Location = ((System.Drawing.Point)(resources.GetObject("lblPropertyName.Location")));
			this.lblPropertyName.Name = "lblPropertyName";
			this.lblPropertyName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblPropertyName.RightToLeft")));
			this.lblPropertyName.Size = ((System.Drawing.Size)(resources.GetObject("lblPropertyName.Size")));
			this.lblPropertyName.TabIndex = ((int)(resources.GetObject("lblPropertyName.TabIndex")));
			this.lblPropertyName.Text = resources.GetString("lblPropertyName.Text");
			this.lblPropertyName.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblPropertyName.TextAlign")));
			this.lblPropertyName.Visible = ((bool)(resources.GetObject("lblPropertyName.Visible")));
			// 
			// txtPropertyName
			// 
			this.txtPropertyName.AccessibleDescription = ((string)(resources.GetObject("txtPropertyName.AccessibleDescription")));
			this.txtPropertyName.AccessibleName = ((string)(resources.GetObject("txtPropertyName.AccessibleName")));
			this.txtPropertyName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtPropertyName.Anchor")));
			this.txtPropertyName.AutoSize = ((bool)(resources.GetObject("txtPropertyName.AutoSize")));
			this.txtPropertyName.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtPropertyName.BackgroundImage")));
			this.txtPropertyName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtPropertyName.Dock")));
			this.txtPropertyName.Enabled = ((bool)(resources.GetObject("txtPropertyName.Enabled")));
			this.txtPropertyName.Font = ((System.Drawing.Font)(resources.GetObject("txtPropertyName.Font")));
			this.txtPropertyName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtPropertyName.ImeMode")));
			this.txtPropertyName.Location = ((System.Drawing.Point)(resources.GetObject("txtPropertyName.Location")));
			this.txtPropertyName.MaxLength = ((int)(resources.GetObject("txtPropertyName.MaxLength")));
			this.txtPropertyName.Multiline = ((bool)(resources.GetObject("txtPropertyName.Multiline")));
			this.txtPropertyName.Name = "txtPropertyName";
			this.txtPropertyName.PasswordChar = ((char)(resources.GetObject("txtPropertyName.PasswordChar")));
			this.txtPropertyName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtPropertyName.RightToLeft")));
			this.txtPropertyName.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtPropertyName.ScrollBars")));
			this.txtPropertyName.Size = ((System.Drawing.Size)(resources.GetObject("txtPropertyName.Size")));
			this.txtPropertyName.TabIndex = ((int)(resources.GetObject("txtPropertyName.TabIndex")));
			this.txtPropertyName.Text = resources.GetString("txtPropertyName.Text");
			this.txtPropertyName.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtPropertyName.TextAlign")));
			this.txtPropertyName.Visible = ((bool)(resources.GetObject("txtPropertyName.Visible")));
			this.txtPropertyName.WordWrap = ((bool)(resources.GetObject("txtPropertyName.WordWrap")));
			// 
			// lblPropertyValue
			// 
			this.lblPropertyValue.AccessibleDescription = ((string)(resources.GetObject("lblPropertyValue.AccessibleDescription")));
			this.lblPropertyValue.AccessibleName = ((string)(resources.GetObject("lblPropertyValue.AccessibleName")));
			this.lblPropertyValue.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblPropertyValue.Anchor")));
			this.lblPropertyValue.AutoSize = ((bool)(resources.GetObject("lblPropertyValue.AutoSize")));
			this.lblPropertyValue.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblPropertyValue.Dock")));
			this.lblPropertyValue.Enabled = ((bool)(resources.GetObject("lblPropertyValue.Enabled")));
			this.lblPropertyValue.Font = ((System.Drawing.Font)(resources.GetObject("lblPropertyValue.Font")));
			this.lblPropertyValue.Image = ((System.Drawing.Image)(resources.GetObject("lblPropertyValue.Image")));
			this.lblPropertyValue.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblPropertyValue.ImageAlign")));
			this.lblPropertyValue.ImageIndex = ((int)(resources.GetObject("lblPropertyValue.ImageIndex")));
			this.lblPropertyValue.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblPropertyValue.ImeMode")));
			this.lblPropertyValue.Location = ((System.Drawing.Point)(resources.GetObject("lblPropertyValue.Location")));
			this.lblPropertyValue.Name = "lblPropertyValue";
			this.lblPropertyValue.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblPropertyValue.RightToLeft")));
			this.lblPropertyValue.Size = ((System.Drawing.Size)(resources.GetObject("lblPropertyValue.Size")));
			this.lblPropertyValue.TabIndex = ((int)(resources.GetObject("lblPropertyValue.TabIndex")));
			this.lblPropertyValue.Text = resources.GetString("lblPropertyValue.Text");
			this.lblPropertyValue.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblPropertyValue.TextAlign")));
			this.lblPropertyValue.Visible = ((bool)(resources.GetObject("lblPropertyValue.Visible")));
			// 
			// txtPropertyValue
			// 
			this.txtPropertyValue.AccessibleDescription = ((string)(resources.GetObject("txtPropertyValue.AccessibleDescription")));
			this.txtPropertyValue.AccessibleName = ((string)(resources.GetObject("txtPropertyValue.AccessibleName")));
			this.txtPropertyValue.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtPropertyValue.Anchor")));
			this.txtPropertyValue.AutoSize = ((bool)(resources.GetObject("txtPropertyValue.AutoSize")));
			this.txtPropertyValue.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtPropertyValue.BackgroundImage")));
			this.txtPropertyValue.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtPropertyValue.Dock")));
			this.txtPropertyValue.Enabled = ((bool)(resources.GetObject("txtPropertyValue.Enabled")));
			this.txtPropertyValue.Font = ((System.Drawing.Font)(resources.GetObject("txtPropertyValue.Font")));
			this.txtPropertyValue.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtPropertyValue.ImeMode")));
			this.txtPropertyValue.Location = ((System.Drawing.Point)(resources.GetObject("txtPropertyValue.Location")));
			this.txtPropertyValue.MaxLength = ((int)(resources.GetObject("txtPropertyValue.MaxLength")));
			this.txtPropertyValue.Multiline = ((bool)(resources.GetObject("txtPropertyValue.Multiline")));
			this.txtPropertyValue.Name = "txtPropertyValue";
			this.txtPropertyValue.PasswordChar = ((char)(resources.GetObject("txtPropertyValue.PasswordChar")));
			this.txtPropertyValue.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtPropertyValue.RightToLeft")));
			this.txtPropertyValue.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtPropertyValue.ScrollBars")));
			this.txtPropertyValue.Size = ((System.Drawing.Size)(resources.GetObject("txtPropertyValue.Size")));
			this.txtPropertyValue.TabIndex = ((int)(resources.GetObject("txtPropertyValue.TabIndex")));
			this.txtPropertyValue.Text = resources.GetString("txtPropertyValue.Text");
			this.txtPropertyValue.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtPropertyValue.TextAlign")));
			this.txtPropertyValue.Visible = ((bool)(resources.GetObject("txtPropertyValue.Visible")));
			this.txtPropertyValue.WordWrap = ((bool)(resources.GetObject("txtPropertyValue.WordWrap")));
			// 
			// AddPropertyDialog
			// 
			this.AcceptButton = this.btnOK;
			this.AccessibleDescription = ((string)(resources.GetObject("$this.AccessibleDescription")));
			this.AccessibleName = ((string)(resources.GetObject("$this.AccessibleName")));
			this.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("$this.Anchor")));
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.btnCancel;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.txtPropertyValue,
																		  this.lblPropertyValue,
																		  this.txtPropertyName,
																		  this.lblPropertyName,
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
			this.Name = "AddPropertyDialog";
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
		private System.Windows.Forms.Label lblPropertyName;
		private System.Windows.Forms.TextBox txtPropertyName;
		private System.Windows.Forms.Label lblPropertyValue;
		private System.Windows.Forms.TextBox txtPropertyValue;

		private System.ComponentModel.IContainer components = null;
		#endregion
	}
}