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
using System.Windows.Forms;
using System.ComponentModel;
using NAnt.Contrib.NAntAddin.Controls;

namespace NAnt.Contrib.NAntAddin.Nodes
{
	/// <summary>
	/// Tree Node that represents an NAnt "target" element.
	/// </summary>
	/// <remarks>None.</remarks>
	public class NAntTargetNode : NAntBaseNode
	{
		private XmlElement targetElement;
		internal XmlElement parentElement;

		/// <summary>
		/// Creates a new <see cref="NAntTargetNode"/>.
		/// </summary>
		/// <param name="TargetElement">The target's XML element.</param>
		/// <param name="ParentElement">The parent XML element of the target.</param>
		/// <remarks>None.</remarks>
		public NAntTargetNode(XmlElement TargetElement, XmlElement ParentElement) 
			: base(ParentElement)
		{
			targetElement = TargetElement;
			parentElement = ParentElement;

			Text = TargetElement.GetAttribute("name");
			ImageIndex = 1;
			SelectedImageIndex = 1;

			Nodes.Add(new TreeNode(ScriptExplorerControl.DUMMY_NODE));
		}

		/// <summary>
		/// Gets the NAnt Script's "target" XML element.
		/// </summary>
		/// <value>The NAnt Script's "target" XML element.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public XmlElement TargetElement
		{
			get
			{
				return targetElement;
			}
		}

		/// <summary>
		/// Gets or sets the name of the Target.
		/// </summary>
		[Description("The name of the Target."),Category("Appearance")]
		public string Name
		{
			get
			{
				return TargetElement.GetAttribute("name");
			}

			set
			{
				TargetElement.SetAttribute("name", value);
				Save();
			}
		}

		/// <summary>
		/// Gets or sets a description of the Target.
		/// </summary>
		/// <value>A description of the Target.</value>
		/// <remarks>None.</remarks>
		[Description("A Description of the Target."),Category("Appearance")]
		public string Description
		{
			get
			{
				return TargetElement.GetAttribute("description");
			}

			set
			{
				if (value == "")
				{
					TargetElement.RemoveAttribute("description");
				}
				else
				{
					TargetElement.SetAttribute("description", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets targets that must be run before this one.
		/// </summary>
		/// <value>Targets that must be run before this one.</value>
		/// <remarks>None.</remarks>
		[Description("Targets that must be run before this one."),Category("Behavior")]
		public string Depends
		{
			get
			{
				return TargetElement.GetAttribute("depends");
			}

			set
			{
				if (value == "")
				{
					TargetElement.RemoveAttribute("depends");
				}
				else
				{
					TargetElement.SetAttribute("depends", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets a condition that must evaluate true for the Target to execute.
		/// </summary>
		/// <value>A condition that must evaluate true for the Target to execute.</value>
		/// <remarks>None.</remarks>
		[Description("Condition that must evaluate true for the Target to execute."),Category("Behavior")]
		public string If
		{
			get
			{
				return TargetElement.GetAttribute("if");
			}

			set
			{
				if (value == "")
				{
					TargetElement.RemoveAttribute("if");
				}
				else
				{
					TargetElement.SetAttribute("if", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets a condition that must evaluate false for the Target to execute.
		/// </summary>
		/// <value>A condition that must evaluate false for the Target to execute.</value>
		/// <remarks>None.</remarks>
		[Description("Condition that must evaluate false for the Target to execute."),Category("Behavior")]
		public string Unless
		{
			get
			{
				return TargetElement.GetAttribute("unless");
			}

			set
			{
				if (value == "")
				{
					TargetElement.RemoveAttribute("unless");
				}
				else
				{
					TargetElement.SetAttribute("unless", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Saves the Project containing this Target.
		/// </summary>
		/// <remarks>None.</remarks>
		private void Save()
		{
			ProjectNode.Save();
		}
	}
}