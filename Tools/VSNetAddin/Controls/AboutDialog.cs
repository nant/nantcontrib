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
using System.Reflection;
using System.Windows.Forms;

namespace NAnt.Contrib.NAntAddin.Dialogs
{
	internal class AboutDialog : Form
	{
		/// <summary>
		/// Creates a new About Dialog.
		/// </summary>
		internal AboutDialog()
		{
			InitializeComponent();

			Assembly nAntAddinAsm = Assembly.GetAssembly(typeof(NAntAddin.Addin));
			AssemblyName nAntAddinAsmName = AssemblyName.GetAssemblyName(
				nAntAddinAsm.GetModule("NAntAddin.dll").FullyQualifiedName);
			lblProductVersion.Text = "Version: " + 
				nAntAddinAsmName.Version.Major + "." + 
				nAntAddinAsmName.Version.Minor + "." + 
				nAntAddinAsmName.Version.Revision + "." + 
				nAntAddinAsmName.Version.Build;
		}

		/// <summary>
		/// Occurs when the "OK" button is clicked.
		/// </summary>
		/// <param name="sender">The object that fired the event</param>
		/// <param name="e">Arguments passed to the event</param>
		private void btnOK_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Occurs when the "Ant Web Site" link is clicked.
		/// </summary>
		/// <param name="sender">The object that fired the event</param>
		/// <param name="e">Arguments passed to the event</param>
		private void lblNAntWebSite_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.No;
			Close();
		}

		#region Visual Studio Designer Code
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AboutDialog));
			this.panLogo = new System.Windows.Forms.Panel();
			this.lblProductName = new System.Windows.Forms.Label();
			this.lblProductVersion = new System.Windows.Forms.Label();
			this.lblCopyright = new System.Windows.Forms.Label();
			this.btnOK = new System.Windows.Forms.Button();
			this.lblNAntWebSite = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.lblProtected = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// panLogo
			// 
			this.panLogo.AccessibleDescription = ((string)(resources.GetObject("panLogo.AccessibleDescription")));
			this.panLogo.AccessibleName = ((string)(resources.GetObject("panLogo.AccessibleName")));
			this.panLogo.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panLogo.Anchor")));
			this.panLogo.AutoScroll = ((bool)(resources.GetObject("panLogo.AutoScroll")));
			this.panLogo.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panLogo.AutoScrollMargin")));
			this.panLogo.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panLogo.AutoScrollMinSize")));
			this.panLogo.BackgroundImage = ((System.Drawing.Bitmap)(resources.GetObject("panLogo.BackgroundImage")));
			this.panLogo.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panLogo.Dock")));
			this.panLogo.Enabled = ((bool)(resources.GetObject("panLogo.Enabled")));
			this.panLogo.Font = ((System.Drawing.Font)(resources.GetObject("panLogo.Font")));
			this.panLogo.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panLogo.ImeMode")));
			this.panLogo.Location = ((System.Drawing.Point)(resources.GetObject("panLogo.Location")));
			this.panLogo.Name = "panLogo";
			this.panLogo.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panLogo.RightToLeft")));
			this.panLogo.Size = ((System.Drawing.Size)(resources.GetObject("panLogo.Size")));
			this.panLogo.TabIndex = ((int)(resources.GetObject("panLogo.TabIndex")));
			this.panLogo.Text = resources.GetString("panLogo.Text");
			this.panLogo.Visible = ((bool)(resources.GetObject("panLogo.Visible")));
			// 
			// lblProductName
			// 
			this.lblProductName.AccessibleDescription = ((string)(resources.GetObject("lblProductName.AccessibleDescription")));
			this.lblProductName.AccessibleName = ((string)(resources.GetObject("lblProductName.AccessibleName")));
			this.lblProductName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblProductName.Anchor")));
			this.lblProductName.AutoSize = ((bool)(resources.GetObject("lblProductName.AutoSize")));
			this.lblProductName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblProductName.Dock")));
			this.lblProductName.Enabled = ((bool)(resources.GetObject("lblProductName.Enabled")));
			this.lblProductName.Font = ((System.Drawing.Font)(resources.GetObject("lblProductName.Font")));
			this.lblProductName.Image = ((System.Drawing.Image)(resources.GetObject("lblProductName.Image")));
			this.lblProductName.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblProductName.ImageAlign")));
			this.lblProductName.ImageIndex = ((int)(resources.GetObject("lblProductName.ImageIndex")));
			this.lblProductName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblProductName.ImeMode")));
			this.lblProductName.Location = ((System.Drawing.Point)(resources.GetObject("lblProductName.Location")));
			this.lblProductName.Name = "lblProductName";
			this.lblProductName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblProductName.RightToLeft")));
			this.lblProductName.Size = ((System.Drawing.Size)(resources.GetObject("lblProductName.Size")));
			this.lblProductName.TabIndex = ((int)(resources.GetObject("lblProductName.TabIndex")));
			this.lblProductName.Text = resources.GetString("lblProductName.Text");
			this.lblProductName.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblProductName.TextAlign")));
			this.lblProductName.Visible = ((bool)(resources.GetObject("lblProductName.Visible")));
			// 
			// lblProductVersion
			// 
			this.lblProductVersion.AccessibleDescription = ((string)(resources.GetObject("lblProductVersion.AccessibleDescription")));
			this.lblProductVersion.AccessibleName = ((string)(resources.GetObject("lblProductVersion.AccessibleName")));
			this.lblProductVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblProductVersion.Anchor")));
			this.lblProductVersion.AutoSize = ((bool)(resources.GetObject("lblProductVersion.AutoSize")));
			this.lblProductVersion.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblProductVersion.Dock")));
			this.lblProductVersion.Enabled = ((bool)(resources.GetObject("lblProductVersion.Enabled")));
			this.lblProductVersion.Font = ((System.Drawing.Font)(resources.GetObject("lblProductVersion.Font")));
			this.lblProductVersion.Image = ((System.Drawing.Image)(resources.GetObject("lblProductVersion.Image")));
			this.lblProductVersion.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblProductVersion.ImageAlign")));
			this.lblProductVersion.ImageIndex = ((int)(resources.GetObject("lblProductVersion.ImageIndex")));
			this.lblProductVersion.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblProductVersion.ImeMode")));
			this.lblProductVersion.Location = ((System.Drawing.Point)(resources.GetObject("lblProductVersion.Location")));
			this.lblProductVersion.Name = "lblProductVersion";
			this.lblProductVersion.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblProductVersion.RightToLeft")));
			this.lblProductVersion.Size = ((System.Drawing.Size)(resources.GetObject("lblProductVersion.Size")));
			this.lblProductVersion.TabIndex = ((int)(resources.GetObject("lblProductVersion.TabIndex")));
			this.lblProductVersion.Text = resources.GetString("lblProductVersion.Text");
			this.lblProductVersion.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblProductVersion.TextAlign")));
			this.lblProductVersion.Visible = ((bool)(resources.GetObject("lblProductVersion.Visible")));
			// 
			// lblCopyright
			// 
			this.lblCopyright.AccessibleDescription = ((string)(resources.GetObject("lblCopyright.AccessibleDescription")));
			this.lblCopyright.AccessibleName = ((string)(resources.GetObject("lblCopyright.AccessibleName")));
			this.lblCopyright.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblCopyright.Anchor")));
			this.lblCopyright.AutoSize = ((bool)(resources.GetObject("lblCopyright.AutoSize")));
			this.lblCopyright.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblCopyright.Dock")));
			this.lblCopyright.Enabled = ((bool)(resources.GetObject("lblCopyright.Enabled")));
			this.lblCopyright.Font = ((System.Drawing.Font)(resources.GetObject("lblCopyright.Font")));
			this.lblCopyright.Image = ((System.Drawing.Image)(resources.GetObject("lblCopyright.Image")));
			this.lblCopyright.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblCopyright.ImageAlign")));
			this.lblCopyright.ImageIndex = ((int)(resources.GetObject("lblCopyright.ImageIndex")));
			this.lblCopyright.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblCopyright.ImeMode")));
			this.lblCopyright.Location = ((System.Drawing.Point)(resources.GetObject("lblCopyright.Location")));
			this.lblCopyright.Name = "lblCopyright";
			this.lblCopyright.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblCopyright.RightToLeft")));
			this.lblCopyright.Size = ((System.Drawing.Size)(resources.GetObject("lblCopyright.Size")));
			this.lblCopyright.TabIndex = ((int)(resources.GetObject("lblCopyright.TabIndex")));
			this.lblCopyright.Text = resources.GetString("lblCopyright.Text");
			this.lblCopyright.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblCopyright.TextAlign")));
			this.lblCopyright.Visible = ((bool)(resources.GetObject("lblCopyright.Visible")));
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
			// lblNAntWebSite
			// 
			this.lblNAntWebSite.AccessibleDescription = ((string)(resources.GetObject("lblNAntWebSite.AccessibleDescription")));
			this.lblNAntWebSite.AccessibleName = ((string)(resources.GetObject("lblNAntWebSite.AccessibleName")));
			this.lblNAntWebSite.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblNAntWebSite.Anchor")));
			this.lblNAntWebSite.AutoSize = ((bool)(resources.GetObject("lblNAntWebSite.AutoSize")));
			this.lblNAntWebSite.BackColor = System.Drawing.Color.Transparent;
			this.lblNAntWebSite.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lblNAntWebSite.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblNAntWebSite.Dock")));
			this.lblNAntWebSite.Enabled = ((bool)(resources.GetObject("lblNAntWebSite.Enabled")));
			this.lblNAntWebSite.Font = ((System.Drawing.Font)(resources.GetObject("lblNAntWebSite.Font")));
			this.lblNAntWebSite.ForeColor = System.Drawing.Color.Orange;
			this.lblNAntWebSite.Image = ((System.Drawing.Image)(resources.GetObject("lblNAntWebSite.Image")));
			this.lblNAntWebSite.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblNAntWebSite.ImageAlign")));
			this.lblNAntWebSite.ImageIndex = ((int)(resources.GetObject("lblNAntWebSite.ImageIndex")));
			this.lblNAntWebSite.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblNAntWebSite.ImeMode")));
			this.lblNAntWebSite.Location = ((System.Drawing.Point)(resources.GetObject("lblNAntWebSite.Location")));
			this.lblNAntWebSite.Name = "lblNAntWebSite";
			this.lblNAntWebSite.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblNAntWebSite.RightToLeft")));
			this.lblNAntWebSite.Size = ((System.Drawing.Size)(resources.GetObject("lblNAntWebSite.Size")));
			this.lblNAntWebSite.TabIndex = ((int)(resources.GetObject("lblNAntWebSite.TabIndex")));
			this.lblNAntWebSite.Text = resources.GetString("lblNAntWebSite.Text");
			this.lblNAntWebSite.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblNAntWebSite.TextAlign")));
			this.lblNAntWebSite.Visible = ((bool)(resources.GetObject("lblNAntWebSite.Visible")));
			this.lblNAntWebSite.Click += new System.EventHandler(this.lblNAntWebSite_Click);
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
			// lblProtected
			// 
			this.lblProtected.AccessibleDescription = ((string)(resources.GetObject("lblProtected.AccessibleDescription")));
			this.lblProtected.AccessibleName = ((string)(resources.GetObject("lblProtected.AccessibleName")));
			this.lblProtected.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblProtected.Anchor")));
			this.lblProtected.AutoSize = ((bool)(resources.GetObject("lblProtected.AutoSize")));
			this.lblProtected.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblProtected.Dock")));
			this.lblProtected.Enabled = ((bool)(resources.GetObject("lblProtected.Enabled")));
			this.lblProtected.Font = ((System.Drawing.Font)(resources.GetObject("lblProtected.Font")));
			this.lblProtected.Image = ((System.Drawing.Image)(resources.GetObject("lblProtected.Image")));
			this.lblProtected.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblProtected.ImageAlign")));
			this.lblProtected.ImageIndex = ((int)(resources.GetObject("lblProtected.ImageIndex")));
			this.lblProtected.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblProtected.ImeMode")));
			this.lblProtected.Location = ((System.Drawing.Point)(resources.GetObject("lblProtected.Location")));
			this.lblProtected.Name = "lblProtected";
			this.lblProtected.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblProtected.RightToLeft")));
			this.lblProtected.Size = ((System.Drawing.Size)(resources.GetObject("lblProtected.Size")));
			this.lblProtected.TabIndex = ((int)(resources.GetObject("lblProtected.TabIndex")));
			this.lblProtected.Text = resources.GetString("lblProtected.Text");
			this.lblProtected.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblProtected.TextAlign")));
			this.lblProtected.Visible = ((bool)(resources.GetObject("lblProtected.Visible")));
			// 
			// AboutDialog
			// 
			this.AccessibleDescription = ((string)(resources.GetObject("$this.AccessibleDescription")));
			this.AccessibleName = ((string)(resources.GetObject("$this.AccessibleName")));
			this.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("$this.Anchor")));
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackColor = System.Drawing.Color.White;
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.lblProtected,
																		  this.panel1,
																		  this.lblNAntWebSite,
																		  this.btnOK,
																		  this.lblCopyright,
																		  this.lblProductVersion,
																		  this.lblProductName,
																		  this.panLogo});
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
			this.Name = "AboutDialog";
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
		private System.Windows.Forms.Panel panLogo;
		private System.Windows.Forms.Label lblProductName;
		private System.Windows.Forms.Label lblProductVersion;
		private System.Windows.Forms.Label lblCopyright;
		private System.Windows.Forms.Label lblNAntWebSite;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label lblProtected;

		private System.ComponentModel.IContainer components = null;
		#endregion
	}
}