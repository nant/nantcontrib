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
	/// Tree Node that represents an NAnt echo task.
	/// </summary>
	/// <remarks>None.</remarks>
	[NAntTask("echo", "Display a Build Message", "echotask.bmp")]
	public class NAntEchoTaskNode : NAntTaskNode
	{
		/// <summary>
		/// Creates a new <see cref="NAntEchoTaskNode"/>.
		/// </summary>
		/// <param name="TaskElement">The task's XML element.</param>
		/// <param name="ParentElement">The parent XML element of the task.</param>
		/// <remarks>None.</remarks>
		public NAntEchoTaskNode(XmlElement TaskElement, XmlElement ParentElement) 
			: base(TaskElement, ParentElement)
		{	
		}

		/// <summary>
		/// Gets or sets the message to be displayed at build time.
		/// </summary>
		/// <value>The message to be displayed at build time.</value>
		/// <remarks>None.</remarks>
		[Description("The message displayed at build time."),Category("Data")]
		public string Message
		{
			get
			{
				return TaskElement.GetAttribute("message");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("message");
				}
				else
				{
					TaskElement.SetAttribute("message", value);
				}
				Save();
			}
		}
	}
}