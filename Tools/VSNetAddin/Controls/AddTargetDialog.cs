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

namespace NAnt.Contrib.NAntAddin.Dialogs {
    /// <summary>
    /// Dialog that Adds a Target.
    /// </summary>
    internal class AddTargetDialog : Form {
        private NAntProjectNode projectNode;
        private ClipboardManager clipboard;

        /// <summary>
        /// Creates a new Add Target Dialog.
        /// </summary>
        /// <param name="ProjectNode">The Project to add the Target to</param>
        /// <param name="Clipboard">The <see cref="ClipboardManager"/> to use.</param>
        internal AddTargetDialog(NAntProjectNode ProjectNode, ClipboardManager Clipboard) {
            InitializeComponent();

            projectNode = ProjectNode;
            clipboard = Clipboard;
        }

        /// <summary>
        /// Occurs when the "OK" button is clicked.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Arguments passed to the event</param>
        private void btnOK_Click(object sender, System.EventArgs e) {
            XmlNodeList targetNodes = projectNode.ProjectDocument.DocumentElement.SelectNodes("target");

            //
            // Validate Fields
            //
            if (txtTargetName.Text.Length < 1) {
                MessageBox.Show("The Name field is required.", "Invalid Name", 
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            if (txtTargetDepends.Text.Length > 0) {
                // Verify Dependent Targets exist

                int startPos = 0;

                int commaPos = txtTargetDepends.Text.IndexOf(",");
                while (commaPos != -1) {
                    string targetName = txtTargetDepends.Text.Substring(startPos, commaPos);

                    bool foundTarget = false;

                    // Look for a Target that matches the Dependent one
                    for (int i = 0; i < targetNodes.Count; i++) {
                        XmlElement targetElem = (XmlElement)targetNodes[i];
                        if (targetElem.GetAttribute("name") == targetName) {
                            foundTarget = true;
                        }
                    }

                    if (!foundTarget) {
                        MessageBox.Show(
                            "Dependent Target \"" + targetName + 
                            "\" does not exist.", "Invalid Target", 
                            MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }

                    startPos = commaPos + 1;
                    commaPos = txtTargetDepends.Text.IndexOf(",", startPos);
                }

                string targetName2 = txtTargetDepends.Text.Substring(startPos);

                bool foundTarget2 = false;

                // Look for a Target that matches the Dependent one
                for (int i = 0; i < targetNodes.Count; i++) {
                    XmlElement targetElem = (XmlElement)targetNodes[i];
                    if (targetElem.GetAttribute("name") == targetName2) {
                        foundTarget2 = true;
                    }
                }

                if (!foundTarget2) {
                    MessageBox.Show(
                        "Dependent Target \"" + targetName2 + 
                        "\" does not exist.", "Invalid Target", 
                        MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }
            }

            //
            // Create the Target XML Element
            //
            XmlElement targetElement = 
                projectNode.ProjectDocument.CreateElement("target");

            targetElement.SetAttribute("name", txtTargetName.Text);

            if (txtTargetDescription.Text.Length > 0) {
                targetElement.SetAttribute("description", txtTargetDescription.Text);
            }

            if (txtTargetDepends.Text.Length > 0) {
                targetElement.SetAttribute("depends", txtTargetDepends.Text);
            }

            if (clipboard.AddTargetToProject(targetElement, projectNode)) {
                Close();
            }
        }

        /// <summary>
        /// Occurs when the "Cancel" button is clicked.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Arguments passed to the event</param>
        private void btnCancel_Click(object sender, System.EventArgs e) {
            Close();
        }

        #region Visual Studio Designer Code
        private void InitializeComponent() {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AddTargetDialog));
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblTargetName = new System.Windows.Forms.Label();
            this.txtTargetName = new System.Windows.Forms.TextBox();
            this.lblTargetDescription = new System.Windows.Forms.Label();
            this.txtTargetDescription = new System.Windows.Forms.TextBox();
            this.txtTargetDepends = new System.Windows.Forms.TextBox();
            this.lblTargetDepends = new System.Windows.Forms.Label();
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
            // lblTargetName
            // 
            this.lblTargetName.AccessibleDescription = ((string)(resources.GetObject("lblTargetName.AccessibleDescription")));
            this.lblTargetName.AccessibleName = ((string)(resources.GetObject("lblTargetName.AccessibleName")));
            this.lblTargetName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblTargetName.Anchor")));
            this.lblTargetName.AutoSize = ((bool)(resources.GetObject("lblTargetName.AutoSize")));
            this.lblTargetName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblTargetName.Dock")));
            this.lblTargetName.Enabled = ((bool)(resources.GetObject("lblTargetName.Enabled")));
            this.lblTargetName.Font = ((System.Drawing.Font)(resources.GetObject("lblTargetName.Font")));
            this.lblTargetName.Image = ((System.Drawing.Image)(resources.GetObject("lblTargetName.Image")));
            this.lblTargetName.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblTargetName.ImageAlign")));
            this.lblTargetName.ImageIndex = ((int)(resources.GetObject("lblTargetName.ImageIndex")));
            this.lblTargetName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblTargetName.ImeMode")));
            this.lblTargetName.Location = ((System.Drawing.Point)(resources.GetObject("lblTargetName.Location")));
            this.lblTargetName.Name = "lblTargetName";
            this.lblTargetName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblTargetName.RightToLeft")));
            this.lblTargetName.Size = ((System.Drawing.Size)(resources.GetObject("lblTargetName.Size")));
            this.lblTargetName.TabIndex = ((int)(resources.GetObject("lblTargetName.TabIndex")));
            this.lblTargetName.Text = resources.GetString("lblTargetName.Text");
            this.lblTargetName.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblTargetName.TextAlign")));
            this.lblTargetName.Visible = ((bool)(resources.GetObject("lblTargetName.Visible")));
            // 
            // txtTargetName
            // 
            this.txtTargetName.AccessibleDescription = ((string)(resources.GetObject("txtTargetName.AccessibleDescription")));
            this.txtTargetName.AccessibleName = ((string)(resources.GetObject("txtTargetName.AccessibleName")));
            this.txtTargetName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtTargetName.Anchor")));
            this.txtTargetName.AutoSize = ((bool)(resources.GetObject("txtTargetName.AutoSize")));
            this.txtTargetName.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtTargetName.BackgroundImage")));
            this.txtTargetName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtTargetName.Dock")));
            this.txtTargetName.Enabled = ((bool)(resources.GetObject("txtTargetName.Enabled")));
            this.txtTargetName.Font = ((System.Drawing.Font)(resources.GetObject("txtTargetName.Font")));
            this.txtTargetName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtTargetName.ImeMode")));
            this.txtTargetName.Location = ((System.Drawing.Point)(resources.GetObject("txtTargetName.Location")));
            this.txtTargetName.MaxLength = ((int)(resources.GetObject("txtTargetName.MaxLength")));
            this.txtTargetName.Multiline = ((bool)(resources.GetObject("txtTargetName.Multiline")));
            this.txtTargetName.Name = "txtTargetName";
            this.txtTargetName.PasswordChar = ((char)(resources.GetObject("txtTargetName.PasswordChar")));
            this.txtTargetName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtTargetName.RightToLeft")));
            this.txtTargetName.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtTargetName.ScrollBars")));
            this.txtTargetName.Size = ((System.Drawing.Size)(resources.GetObject("txtTargetName.Size")));
            this.txtTargetName.TabIndex = ((int)(resources.GetObject("txtTargetName.TabIndex")));
            this.txtTargetName.Text = resources.GetString("txtTargetName.Text");
            this.txtTargetName.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtTargetName.TextAlign")));
            this.txtTargetName.Visible = ((bool)(resources.GetObject("txtTargetName.Visible")));
            this.txtTargetName.WordWrap = ((bool)(resources.GetObject("txtTargetName.WordWrap")));
            // 
            // lblTargetDescription
            // 
            this.lblTargetDescription.AccessibleDescription = ((string)(resources.GetObject("lblTargetDescription.AccessibleDescription")));
            this.lblTargetDescription.AccessibleName = ((string)(resources.GetObject("lblTargetDescription.AccessibleName")));
            this.lblTargetDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblTargetDescription.Anchor")));
            this.lblTargetDescription.AutoSize = ((bool)(resources.GetObject("lblTargetDescription.AutoSize")));
            this.lblTargetDescription.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblTargetDescription.Dock")));
            this.lblTargetDescription.Enabled = ((bool)(resources.GetObject("lblTargetDescription.Enabled")));
            this.lblTargetDescription.Font = ((System.Drawing.Font)(resources.GetObject("lblTargetDescription.Font")));
            this.lblTargetDescription.Image = ((System.Drawing.Image)(resources.GetObject("lblTargetDescription.Image")));
            this.lblTargetDescription.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblTargetDescription.ImageAlign")));
            this.lblTargetDescription.ImageIndex = ((int)(resources.GetObject("lblTargetDescription.ImageIndex")));
            this.lblTargetDescription.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblTargetDescription.ImeMode")));
            this.lblTargetDescription.Location = ((System.Drawing.Point)(resources.GetObject("lblTargetDescription.Location")));
            this.lblTargetDescription.Name = "lblTargetDescription";
            this.lblTargetDescription.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblTargetDescription.RightToLeft")));
            this.lblTargetDescription.Size = ((System.Drawing.Size)(resources.GetObject("lblTargetDescription.Size")));
            this.lblTargetDescription.TabIndex = ((int)(resources.GetObject("lblTargetDescription.TabIndex")));
            this.lblTargetDescription.Text = resources.GetString("lblTargetDescription.Text");
            this.lblTargetDescription.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblTargetDescription.TextAlign")));
            this.lblTargetDescription.Visible = ((bool)(resources.GetObject("lblTargetDescription.Visible")));
            // 
            // txtTargetDescription
            // 
            this.txtTargetDescription.AccessibleDescription = ((string)(resources.GetObject("txtTargetDescription.AccessibleDescription")));
            this.txtTargetDescription.AccessibleName = ((string)(resources.GetObject("txtTargetDescription.AccessibleName")));
            this.txtTargetDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtTargetDescription.Anchor")));
            this.txtTargetDescription.AutoSize = ((bool)(resources.GetObject("txtTargetDescription.AutoSize")));
            this.txtTargetDescription.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtTargetDescription.BackgroundImage")));
            this.txtTargetDescription.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtTargetDescription.Dock")));
            this.txtTargetDescription.Enabled = ((bool)(resources.GetObject("txtTargetDescription.Enabled")));
            this.txtTargetDescription.Font = ((System.Drawing.Font)(resources.GetObject("txtTargetDescription.Font")));
            this.txtTargetDescription.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtTargetDescription.ImeMode")));
            this.txtTargetDescription.Location = ((System.Drawing.Point)(resources.GetObject("txtTargetDescription.Location")));
            this.txtTargetDescription.MaxLength = ((int)(resources.GetObject("txtTargetDescription.MaxLength")));
            this.txtTargetDescription.Multiline = ((bool)(resources.GetObject("txtTargetDescription.Multiline")));
            this.txtTargetDescription.Name = "txtTargetDescription";
            this.txtTargetDescription.PasswordChar = ((char)(resources.GetObject("txtTargetDescription.PasswordChar")));
            this.txtTargetDescription.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtTargetDescription.RightToLeft")));
            this.txtTargetDescription.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtTargetDescription.ScrollBars")));
            this.txtTargetDescription.Size = ((System.Drawing.Size)(resources.GetObject("txtTargetDescription.Size")));
            this.txtTargetDescription.TabIndex = ((int)(resources.GetObject("txtTargetDescription.TabIndex")));
            this.txtTargetDescription.Text = resources.GetString("txtTargetDescription.Text");
            this.txtTargetDescription.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtTargetDescription.TextAlign")));
            this.txtTargetDescription.Visible = ((bool)(resources.GetObject("txtTargetDescription.Visible")));
            this.txtTargetDescription.WordWrap = ((bool)(resources.GetObject("txtTargetDescription.WordWrap")));
            // 
            // txtTargetDepends
            // 
            this.txtTargetDepends.AccessibleDescription = ((string)(resources.GetObject("txtTargetDepends.AccessibleDescription")));
            this.txtTargetDepends.AccessibleName = ((string)(resources.GetObject("txtTargetDepends.AccessibleName")));
            this.txtTargetDepends.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtTargetDepends.Anchor")));
            this.txtTargetDepends.AutoSize = ((bool)(resources.GetObject("txtTargetDepends.AutoSize")));
            this.txtTargetDepends.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtTargetDepends.BackgroundImage")));
            this.txtTargetDepends.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtTargetDepends.Dock")));
            this.txtTargetDepends.Enabled = ((bool)(resources.GetObject("txtTargetDepends.Enabled")));
            this.txtTargetDepends.Font = ((System.Drawing.Font)(resources.GetObject("txtTargetDepends.Font")));
            this.txtTargetDepends.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtTargetDepends.ImeMode")));
            this.txtTargetDepends.Location = ((System.Drawing.Point)(resources.GetObject("txtTargetDepends.Location")));
            this.txtTargetDepends.MaxLength = ((int)(resources.GetObject("txtTargetDepends.MaxLength")));
            this.txtTargetDepends.Multiline = ((bool)(resources.GetObject("txtTargetDepends.Multiline")));
            this.txtTargetDepends.Name = "txtTargetDepends";
            this.txtTargetDepends.PasswordChar = ((char)(resources.GetObject("txtTargetDepends.PasswordChar")));
            this.txtTargetDepends.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtTargetDepends.RightToLeft")));
            this.txtTargetDepends.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtTargetDepends.ScrollBars")));
            this.txtTargetDepends.Size = ((System.Drawing.Size)(resources.GetObject("txtTargetDepends.Size")));
            this.txtTargetDepends.TabIndex = ((int)(resources.GetObject("txtTargetDepends.TabIndex")));
            this.txtTargetDepends.Text = resources.GetString("txtTargetDepends.Text");
            this.txtTargetDepends.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtTargetDepends.TextAlign")));
            this.txtTargetDepends.Visible = ((bool)(resources.GetObject("txtTargetDepends.Visible")));
            this.txtTargetDepends.WordWrap = ((bool)(resources.GetObject("txtTargetDepends.WordWrap")));
            // 
            // lblTargetDepends
            // 
            this.lblTargetDepends.AccessibleDescription = ((string)(resources.GetObject("lblTargetDepends.AccessibleDescription")));
            this.lblTargetDepends.AccessibleName = ((string)(resources.GetObject("lblTargetDepends.AccessibleName")));
            this.lblTargetDepends.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblTargetDepends.Anchor")));
            this.lblTargetDepends.AutoSize = ((bool)(resources.GetObject("lblTargetDepends.AutoSize")));
            this.lblTargetDepends.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblTargetDepends.Dock")));
            this.lblTargetDepends.Enabled = ((bool)(resources.GetObject("lblTargetDepends.Enabled")));
            this.lblTargetDepends.Font = ((System.Drawing.Font)(resources.GetObject("lblTargetDepends.Font")));
            this.lblTargetDepends.Image = ((System.Drawing.Image)(resources.GetObject("lblTargetDepends.Image")));
            this.lblTargetDepends.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblTargetDepends.ImageAlign")));
            this.lblTargetDepends.ImageIndex = ((int)(resources.GetObject("lblTargetDepends.ImageIndex")));
            this.lblTargetDepends.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblTargetDepends.ImeMode")));
            this.lblTargetDepends.Location = ((System.Drawing.Point)(resources.GetObject("lblTargetDepends.Location")));
            this.lblTargetDepends.Name = "lblTargetDepends";
            this.lblTargetDepends.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblTargetDepends.RightToLeft")));
            this.lblTargetDepends.Size = ((System.Drawing.Size)(resources.GetObject("lblTargetDepends.Size")));
            this.lblTargetDepends.TabIndex = ((int)(resources.GetObject("lblTargetDepends.TabIndex")));
            this.lblTargetDepends.Text = resources.GetString("lblTargetDepends.Text");
            this.lblTargetDepends.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblTargetDepends.TextAlign")));
            this.lblTargetDepends.Visible = ((bool)(resources.GetObject("lblTargetDepends.Visible")));
            // 
            // AddTargetDialog
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
                                                                          this.txtTargetDepends,
                                                                          this.lblTargetDepends,
                                                                          this.txtTargetDescription,
                                                                          this.lblTargetDescription,
                                                                          this.txtTargetName,
                                                                          this.lblTargetName,
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
            this.Name = "AddTargetDialog";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.ShowInTaskbar = false;
            this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
            this.Text = resources.GetString("$this.Text");
            this.Visible = ((bool)(resources.GetObject("$this.Visible")));
            this.ResumeLayout(false);

        }
        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblTargetName;
        private System.Windows.Forms.Label lblTargetDepends;
        private System.Windows.Forms.TextBox txtTargetName;
        private System.Windows.Forms.Label lblTargetDescription;
        private System.Windows.Forms.TextBox txtTargetDescription;
        private System.Windows.Forms.TextBox txtTargetDepends;

        private System.ComponentModel.IContainer components = null;
        #endregion
    }
}