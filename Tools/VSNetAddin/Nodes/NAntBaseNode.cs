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
using System.Reflection.Emit;
using System.Globalization;
using System.Windows.Forms;
using System.ComponentModel;

namespace NAnt.Contrib.NAntAddin.Nodes
{
	/// <summary>
	/// The base class from which all tree nodes in the 
	/// <see cref="NAnt.Contrib.NAntAddin.Controls.ScriptExplorerControl"/> 
	/// inherit.
	/// </summary>
	/// <remarks>None.</remarks>
	public class NAntBaseNode : TreeNode, ConstructorArgsResolver
	{
		private XmlElement parentElement;

		/// <summary>
		/// Creates a new <see cref="NAntBaseNode"/>.
		/// </summary>
		/// <param name="ParentElement">The parent XML element.</param>
		/// <remarks>None.</remarks>
		public NAntBaseNode(XmlElement ParentElement)
		{
			parentElement = ParentElement;
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public XmlElement ParentElement
		{
			get
			{
				return parentElement;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new Color BackColor
		{
			get
			{
				return base.BackColor;
			}

			set
			{
				base.BackColor = value;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new Rectangle Bounds
		{
			get
			{
				return base.Bounds;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new bool Checked
		{
			get
			{
				return base.Checked;
			}

			set
			{
				base.Checked = value;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new TreeNode FirstNode
		{
			get
			{
				return base.FirstNode;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new TreeNode LastNode
		{
			get
			{
				return base.LastNode;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new TreeNode NextNode
		{
			get
			{
				return base.NextNode;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new TreeNode NextVisibleNode
		{
			get
			{
				return base.NextVisibleNode;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new Color ForeColor
		{
			get
			{
				return base.ForeColor;
			}

			set
			{
				base.ForeColor = value;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new string FullPath
		{
			get
			{
				return base.FullPath;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new IntPtr Handle
		{
			get
			{
				return base.Handle;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new int ImageIndex
		{
			get
			{
				return base.ImageIndex;
			}

			set
			{
				base.ImageIndex = value;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new int Index
		{
			get
			{
				return base.Index;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new int SelectedImageIndex
		{
			get
			{
				return base.SelectedImageIndex;
			}

			set
			{
				base.SelectedImageIndex = value;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new Object Tag
		{
			get
			{
				return base.Tag;
			}

			set
			{
				base.Tag = value;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new bool IsEditing
		{
			get
			{
				return base.IsEditing;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new bool IsExpanded
		{
			get
			{
				return base.IsExpanded;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new bool IsSelected
		{
			get
			{
				return base.IsSelected;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new bool IsVisible
		{
			get
			{
				return base.IsVisible;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new Font NodeFont
		{
			get
			{
				return base.NodeFont;
			}

			set
			{
				base.NodeFont = value;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new TreeNodeCollection Nodes
		{
			get
			{
				return base.Nodes;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new TreeNode Parent
		{
			get
			{
				return base.Parent;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new TreeNode PrevNode
		{
			get
			{
				return base.PrevNode;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new TreeNode PrevVisibleNode
		{
			get
			{
				return base.PrevVisibleNode;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new string Text
		{
			get
			{
				return base.Text;
			}

			set
			{
				base.Text = value;
			}
		}

		/// <summary>
		/// See the Microsoft Documentation.
		/// </summary>
		/// <value>See the Microsoft Documentation.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public new TreeView TreeView
		{
			get
			{
				return base.TreeView;
			}
		}

		/// <summary>
		/// Retrieves the Project Node containing this Node.
		/// </summary>
		/// <returns>An NAntProjectNode for the Project.</returns>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public NAntProjectNode ProjectNode
		{
			get
			{
				if (this is NAntProjectNode)
				{
					return (NAntProjectNode)this;
				}
				else if (this is NAntTargetNode)
				{
					return (NAntProjectNode)Parent;
				}
				else if (this is NAntPropertyNode)
				{
					if (Parent is NAntTargetNode)
					{
						return (NAntProjectNode)Parent.Parent;
					}
					else
					{
						return (NAntProjectNode)Parent;
					}
				}
				else // Task
				{
					return (NAntProjectNode)Parent.Parent;
				}
			}
		}

		/// <summary>
		/// Returns the arguments that must be passed to 
		/// the constructor of an object to create the 
		/// same object.
		/// </summary>
		/// <returns>The arguments to pass.</returns>
		/// <remarks>None.</remarks>
		public Object[] GetConstructorArgs()
		{
			if (this is NAntTargetNode)
			{
				NAntTargetNode targetNode = (NAntTargetNode)this;

				return new object[]
				{
					targetNode.TargetElement,
					targetNode.parentElement
				};
			}
			else if (this is NAntProjectNode)
			{
				NAntProjectNode projectNode = (NAntProjectNode)this;

				return new Object[]
				{
					projectNode.addin,
					projectNode.project,
					projectNode.ProjectItem
				};
			}
			else if (this is NAntPropertyNode)
			{
				NAntPropertyNode propertyNode = (NAntPropertyNode)this;

				return new Object[]
				{
					propertyNode.PropertyElement,
					propertyNode.parentElement
				};
			}
			else if (this is NAntTargetNode)
			{
				NAntTargetNode targetNode = (NAntTargetNode)this;

				return new Object[]
				{
					targetNode.TargetElement,
					targetNode.parentElement
				};
			}
			else if (this is NAntTaskNode)
			{
				NAntTaskNode taskNode = (NAntTaskNode)this;

				return new Object[]
				{
					taskNode.TaskElement,
					taskNode.parentElement
				};
			}

			return new object[0];
		}
	}
}