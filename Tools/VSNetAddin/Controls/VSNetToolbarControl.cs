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

namespace NAnt.Contrib.NAntAddin.Controls
{
	/// <summary>
	/// Control that extends the <see cref="ToolBar"/> class to 
	/// allow forwarding of <see cref="ToolBarButtonClickEventArgs"/> 
	/// to the <see cref="VSNetToolbarControl"/>.
	/// </summary>
	/// <remarks>None.</remarks>
	public class VSNetToolbarDesigner : ToolBar
	{
		/// <summary>
		/// Creates a new <see cref="VSNetToolbarControl"/>.
		/// </summary>
		/// <remarks>None.</remarks>
		public VSNetToolbarDesigner()
		{
		}

		/// <summary>
		/// Invokes the <see cref="ToolBar"/>'s Click event 
		/// using the supplied <see cref="ToolBarButtonClickEventArgs"/>.
		/// </summary>
		/// <param name="e">Arguments passed to the Click event.</param>
		/// <remarks>None.</remarks>
		public void DoClick(ToolBarButtonClickEventArgs e)
		{
			OnButtonClick(e);
		}
	}

	/// <summary>
	/// Control that provides the Visual Studio.NET like Toolbar.
	/// </summary>
	/// <remarks>None.</remarks>
	public class VSNetToolbarControl : Panel
	{
		private int buttonSpacing = 1;
		private ToolBarButton overButton;
		private ToolBarButton downButton;
		private VSNetToolbarDesigner toolBar;
		private ToolTip toolTip = new ToolTip();
		private bool needsOffScreenRedraw = false;

		private static Brush downBrush = new SolidBrush(Color.FromArgb(152, 181, 226));
		private static Brush hoverBrush = new SolidBrush(Color.FromArgb(193, 210, 238));
		private static Brush controlBrush = SystemBrushes.Control;
		private static Pen controlPen = SystemPens.Control;
		private static Pen controlDarkPen = new Pen(Color.FromArgb(197, 194, 184));
		private static Pen outlinePen = new Pen(Color.FromArgb(49, 106, 197), 1);
		private static Pen bottomPen = new Pen(SystemColors.ControlDark);

		/// <summary>
		/// Creates a new <see cref="VSNetToolbarControl"/>.
		/// </summary>
		/// <remarks>None.</remarks>
		public VSNetToolbarControl()
		{
			Height = 27;
			SetBounds(Bounds.X, Bounds.Y, Width, Height);

			InitializeComponent();
		}

		/// <summary>
		/// Forces the <see cref="VSNetToolbarControl"/> to repaint itself.
		/// </summary>
		/// <remarks>None.</remarks>
		public void Repaint()
		{
			Graphics g = CreateGraphics();
			OnPaint(new PaintEventArgs(g, ClientRectangle));
			g.Dispose();
		}

		/// <summary>
		/// Gets or sets the <see cref="VSNetToolbarDesigner"/> 
		/// that has been configured with <see cref="ToolBarButton"/>s and an 
		/// <see cref="ImageList"/>.
		/// </summary>
		/// <value>the <see cref="VSNetToolbarDesigner"/> 
		/// that has been configured with <see cref="ToolBarButton"/>s and an 
		/// <see cref="ImageList"/>.</value>
		/// <remarks>None.</remarks>
		public VSNetToolbarDesigner ToolBar
		{
			get
			{
				return toolBar;
			}

			set
			{
				toolBar = value;
			}
		}

		/// <summary>
		/// Occurs when the control should repaint itself.
		/// </summary>
		/// <param name="e">Arguments passed to the event.</param>
		/// <remarks>None.</remarks>
		protected override void OnPaint(PaintEventArgs e)
		{
			e.Graphics.Clear(SystemColors.Control);

			if (toolBar != null)
			{
				e.Graphics.DrawLine(bottomPen, 
					ClientRectangle.Left-1, 
					ClientRectangle.Bottom-1, 
					ClientRectangle.Right-1, 
					ClientRectangle.Bottom-1);

				int xOffset = 1;
				int yOffset = 1;

				int buttonWidth = toolBar.ButtonSize.Width;
				int buttonHeight = toolBar.ButtonSize.Height;

				for (int i = 0; i < toolBar.Buttons.Count; i++)
				{
					ToolBarButton button = toolBar.Buttons[i];
					if (button.Style == ToolBarButtonStyle.PushButton && button.Visible)
					{
						Rectangle buttonRect = new Rectangle(
							xOffset, yOffset, buttonWidth, buttonHeight);

						if (toolBar.ImageList != null)
						{
							if (button.Enabled)
							{
								Point clientPoint = PointToClient(Form.MousePosition);

								if (buttonRect.Contains(clientPoint))
								{
									DrawHoverButton(e.Graphics, buttonRect, button);
								}
								else
								{
									DrawNormalButton(e.Graphics, buttonRect, button);
								}
							}
							else
							{
								DrawDisabledButton(e.Graphics, buttonRect, button);
							}
						}

						xOffset += buttonWidth + buttonSpacing;
					}
					else if (button.Style == ToolBarButtonStyle.Separator && button.Visible)
					{
						e.Graphics.DrawLine(
							controlDarkPen, xOffset + 1, 2, 
							xOffset + 1, buttonHeight - 1);

						xOffset += 4;
					}
				}
			}
		}

		/// <summary>
		/// Occurs when the mouse leaves the control's display area.
		/// </summary>
		/// <param name="e">Arguments passed to the event.</param>
		/// <remarks>None.</remarks>
		protected override void OnMouseLeave(EventArgs e)
		{
			if (toolBar != null)
			{
				int xOffset = 1;
				int yOffset = 1;

				int buttonWidth = toolBar.ButtonSize.Width;
				int buttonHeight = toolBar.ButtonSize.Height;

				Graphics g = CreateGraphics();
				
				Rectangle oldRect = new Rectangle(0, 0, 0, 0);

				for (int i = 0; i < toolBar.Buttons.Count; i++)
				{
					ToolBarButton button = toolBar.Buttons[i];
					if (button.Style == ToolBarButtonStyle.PushButton && button.Visible)
					{
						Rectangle buttonRect = new Rectangle(
							xOffset, yOffset, buttonWidth, buttonHeight);

						if (toolBar.ImageList != null)
						{
							if (button == overButton)
							{
								oldRect = new Rectangle(buttonRect.Location, buttonRect.Size);
							}
						}

						xOffset += buttonWidth + buttonSpacing;
					}
					else if (button.Style == ToolBarButtonStyle.Separator && button.Visible)
					{
						xOffset += 4;
					}
				}

				if (overButton != null)
				{
					DrawNormalButton(g, oldRect, overButton);

					overButton = null;
				}

				toolTip.SetToolTip(this, null);

				g.Dispose();
			}
		}

		/// <summary>
		/// Occurs when the mouse is pressed down on the control.
		/// </summary>
		/// <param name="e">Arguments passed to the event.</param>
		/// <remarks>None.</remarks>
		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (toolBar != null)
			{
				int xOffset = 1;
				int yOffset = 1;

				int buttonWidth = toolBar.ButtonSize.Width;
				int buttonHeight = toolBar.ButtonSize.Height;

				Graphics g = CreateGraphics();
				
				for (int i = 0; i < toolBar.Buttons.Count; i++)
				{
					ToolBarButton button = toolBar.Buttons[i];
					if (button.Style == ToolBarButtonStyle.PushButton && button.Visible)
					{
						Rectangle buttonRect = new Rectangle(
							xOffset, yOffset, buttonWidth, buttonHeight);

						if (toolBar.ImageList != null)
						{
							if (buttonRect.Contains(e.X, e.Y) 
								&& toolBar.ImageList != null 
								&& e.Button == MouseButtons.Left)
							{
								if (button.Enabled)
								{
									DrawDownButton(g, buttonRect, button);

									downButton = button;
									needsOffScreenRedraw = true;
								}
							}
						}
						xOffset += buttonWidth + buttonSpacing;
					}
					else if (button.Style == ToolBarButtonStyle.Separator && button.Visible)
					{
						xOffset += 4;
					}
				}

				g.Dispose();
			}
		}

		/// <summary>
		/// Occurs when the mouse button is released 
		/// after being pressed down on the control.
		/// </summary>
		/// <param name="e">Arguments passed to the event.</param>
		/// <remarks>None.</remarks>
		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (downButton != null)
			{
				int xOffset = 1;
				int yOffset = 1;

				int buttonWidth = toolBar.ButtonSize.Width;
				int buttonHeight = toolBar.ButtonSize.Height;

				Graphics g = CreateGraphics();

				for (int i = 0; i < toolBar.Buttons.Count; i++)
				{
					ToolBarButton button = toolBar.Buttons[i];
					if (button.Style == ToolBarButtonStyle.PushButton && button.Visible)
					{
						Rectangle buttonRect = new Rectangle(
							xOffset, yOffset, buttonWidth, buttonHeight);

						if (toolBar.ImageList != null)
						{
							if (toolBar.ImageList != null)
							{
								if (button == downButton && 
									buttonRect.Contains(e.X, e.Y) 
									&& e.Button == MouseButtons.Left)
								{
									DrawHoverButton(g, buttonRect, button);

									toolBar.DoClick(new ToolBarButtonClickEventArgs(button));

									Repaint();
								}
								else if (button == downButton && e.Button == MouseButtons.Left)
								{
									DrawNormalButton(g, buttonRect, button);
								}
								else if (button == downButton 
									&& e.Button == MouseButtons.Right 
									&& !buttonRect.Contains(e.X, e.Y))
								{
									DrawNormalButton(g, buttonRect, button);

									downButton = null;
									needsOffScreenRedraw = false;
								}
							}
						}
						xOffset += buttonWidth + buttonSpacing;
					}
					else if (button.Style == ToolBarButtonStyle.Separator && button.Visible)
					{
						xOffset += 4;
					}
				}

				g.Dispose();
			}

			if (e.Button == MouseButtons.Left)
			{
				downButton = null;
				needsOffScreenRedraw = false;
			}
		}

		/// <summary>
		/// Occurs when the mouse is moved over the display area of the control.
		/// </summary>
		/// <param name="e">Arguments passed to the event.</param>
		/// <remarks>None.</remarks>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (toolBar != null)
			{
				int xOffset = 1;
				int yOffset = 1;

				int buttonWidth = toolBar.ButtonSize.Width;
				int buttonHeight = toolBar.ButtonSize.Height;

				Graphics g = CreateGraphics();
				
				ToolBarButton newOverButton = null;
				Rectangle newRect = new Rectangle(0, 0, 0, 0);
				Rectangle oldRect = new Rectangle(0, 0, 0, 0);
				Rectangle downRect = new Rectangle(0, 0, 0, 0);

				for (int i = 0; i < toolBar.Buttons.Count; i++)
				{
					ToolBarButton button = toolBar.Buttons[i];
					if (button.Style == ToolBarButtonStyle.PushButton && button.Visible)
					{
						Rectangle buttonRect = new Rectangle(
							xOffset, yOffset, buttonWidth, buttonHeight);

						if (toolBar.ImageList != null)
						{
							if (buttonRect.Contains(e.X, e.Y) && toolBar.ImageList != null)
							{
								newOverButton = button;
								newRect = new Rectangle(buttonRect.Location, buttonRect.Size);
							}
							else if (button == overButton)
							{
								oldRect = new Rectangle(buttonRect.Location, buttonRect.Size);
							}
							if (button == downButton)
							{
								downRect = new Rectangle(buttonRect.Location, buttonRect.Size);
							}
						}

						xOffset += buttonWidth + buttonSpacing;
					}
					else if (button.Style == ToolBarButtonStyle.Separator && button.Visible)
					{
						xOffset += 4;
					}
				}

				if (overButton != null && (newOverButton != overButton))
				{
					if (overButton.Enabled)
					{
						DrawNormalButton(g, oldRect, overButton);
					}
				}

				if (newOverButton != null && (newOverButton != overButton) && downButton == null)
				{
					if (newOverButton.Enabled)
					{
						DrawHoverButton(g, newRect, newOverButton);
					}

					if (newOverButton.ToolTipText != null)
					{
						toolTip.SetToolTip(this, newOverButton.ToolTipText);
					}

					overButton = newOverButton;
				}
				else if (downButton != null && newOverButton != downButton)
				{
					if (needsOffScreenRedraw)
					{
						DrawHoverButton(g, downRect, downButton);

						needsOffScreenRedraw = false;
					}
				}
				else if (downButton != null && newOverButton == downButton)
				{
					DrawDownButton(g, downRect, downButton);

					needsOffScreenRedraw = true;
				}

				if (newOverButton == null)
				{
					toolTip.SetToolTip(this, null);
					overButton = null;
				}

				g.Dispose();
			}
		}

		private void DrawNormalButton(Graphics g, Rectangle ButtonRect, ToolBarButton Button)
		{
			int buttonWidth = toolBar.ButtonSize.Width;
			int buttonHeight = toolBar.ButtonSize.Height;

			Rectangle outlineRect = new Rectangle(ButtonRect.Location, ButtonRect.Size);
			outlineRect.X += 1;
			outlineRect.Y += 1;
			outlineRect.Width -= 2;
			outlineRect.Height -= 3;

			g.FillRectangle(controlBrush, outlineRect);
			g.DrawRectangle(controlPen, outlineRect);

			Image buttonImage = toolBar.ImageList.Images[Button.ImageIndex];
			g.DrawImageUnscaled(buttonImage, 
				ButtonRect.Left + (buttonWidth / 2)
				- (toolBar.ImageList.ImageSize.Width / 2), 
				(ButtonRect.Top + (buttonHeight / 2)) 
				- (toolBar.ImageList.ImageSize.Height / 2), 
				buttonWidth, buttonHeight);
		}

		private void DrawDisabledButton(Graphics g, Rectangle ButtonRect, ToolBarButton Button)
		{
			int buttonWidth = toolBar.ButtonSize.Width;
			int buttonHeight = toolBar.ButtonSize.Height;

			Rectangle outlineRect = new Rectangle(ButtonRect.Location, ButtonRect.Size);
			outlineRect.X += 1;
			outlineRect.Y += 1;
			outlineRect.Width -= 2;
			outlineRect.Height -= 3;

			g.FillRectangle(controlBrush, outlineRect);
			g.DrawRectangle(controlPen, outlineRect);

			Image buttonImage = toolBar.ImageList.Images[Button.ImageIndex];
			ControlPaint.DrawImageDisabled(
				g, buttonImage, 
				ButtonRect.Left + (buttonWidth / 2)
				- (toolBar.ImageList.ImageSize.Width / 2), 
				(ButtonRect.Top + (buttonHeight / 2)) 
				- (toolBar.ImageList.ImageSize.Height / 2), 
				SystemColors.Control);
		}

		private void DrawHoverButton(Graphics g, Rectangle ButtonRect, ToolBarButton Button)
		{
			int buttonWidth = toolBar.ButtonSize.Width;
			int buttonHeight = toolBar.ButtonSize.Height;

			Rectangle outlineRect = new Rectangle(ButtonRect.Location, ButtonRect.Size);
			outlineRect.X += 1;
			outlineRect.Y += 1;
			outlineRect.Width -= 2;
			outlineRect.Height -= 3;

			g.FillRectangle(hoverBrush, outlineRect);
			g.DrawRectangle(outlinePen, outlineRect);

			Image buttonImage = toolBar.ImageList.Images[Button.ImageIndex];
			ControlPaint.DrawImageDisabled(
				g, buttonImage, 
				ButtonRect.Left + (buttonWidth / 2)
				- (toolBar.ImageList.ImageSize.Width / 2), 
				(ButtonRect.Top + (buttonHeight / 2)) 
				- (toolBar.ImageList.ImageSize.Height / 2), 
				SystemColors.Control);

			g.DrawImageUnscaled(buttonImage, 
				ButtonRect.Left + (buttonWidth / 2)
				- (toolBar.ImageList.ImageSize.Width / 2) -1, 
				(ButtonRect.Top + (buttonHeight / 2)) 
				- (toolBar.ImageList.ImageSize.Height / 2) -1, 
				buttonWidth, buttonHeight);
		}

		private void DrawDownButton(Graphics g, Rectangle ButtonRect, ToolBarButton Button)
		{
			int buttonWidth = toolBar.ButtonSize.Width;
			int buttonHeight = toolBar.ButtonSize.Height;

			Rectangle outlineRect = new Rectangle(ButtonRect.Location, ButtonRect.Size);
			outlineRect.X += 1;
			outlineRect.Y += 1;
			outlineRect.Width -= 2;
			outlineRect.Height -= 3;

			g.FillRectangle(downBrush, outlineRect);
			g.DrawRectangle(outlinePen, outlineRect);

			Image buttonImage = toolBar.ImageList.Images[Button.ImageIndex];
			g.DrawImageUnscaled(buttonImage, 
				ButtonRect.Left + (buttonWidth / 2)
				- (toolBar.ImageList.ImageSize.Width / 2), 
				(ButtonRect.Top + (buttonHeight / 2)) 
				- (toolBar.ImageList.ImageSize.Height / 2), 
				buttonWidth, buttonHeight);
		}
	
		#region Visual Studio Designer Code
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.SuspendLayout();
			// 
			// VSNetToolbarControl
			// 
			this.Name = "VSNetToolbarControl";
			this.Size = new System.Drawing.Size(312, 27);
			this.ResumeLayout(false);
		}

		/// <summary>
		/// Occurs when the control should release resources.
		/// </summary>
		/// <param name="disposing">Whether the control is being disposed.</param>
		/// <remarks>None.</remarks>
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

		private System.ComponentModel.IContainer components;
		#endregion						
	}
}