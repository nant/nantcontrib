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
using System.ComponentModel;
using System.Windows.Forms;

namespace NAnt.Contrib.NAntAddin.Nodes
{
	/// <summary>
	/// Tree Node that represents an NAnt sysinfo task.
	/// </summary>
	/// <remarks>None.</remarks>
	[NAntTask("sysinfo", "Set Properties with System Information", "sysinfotask.bmp")]
	public class NAntSystemInfoTaskNode : NAntTaskNode
	{
		/// <summary>
		/// Creates a new <see cref="NAntSystemInfoTaskNode"/>.
		/// </summary>
		/// <param name="TaskElement">The task's XML element.</param>
		/// <param name="ParentElement">The parent XML element of the task.</param>
		/// <remarks>None.</remarks>
		public NAntSystemInfoTaskNode(XmlElement TaskElement, XmlElement ParentElement) 
			: base(TaskElement, ParentElement)
		{	
		}

		/// <summary>
		/// Gets or sets the string to prefix the property names with.
		/// </summary>
		/// <value>The string to prefix the property names with. Default is "sys."</value>
		/// <remarks>None.</remarks>
		[Description("String to prefix the property names with. Default is \"sys\"."),Category("Data")]
		public string Prefix
		{
			get
			{
				return TaskElement.GetAttribute("prefix");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("prefix");
				}
				else
				{
					TaskElement.SetAttribute("prefix", value);
				}
				Save();
			}
		}
	}
}