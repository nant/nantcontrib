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
	/// Tree Node that represents an NAnt fail task.
	/// </summary>
	/// <remarks>None.</remarks>
	[NAntTask("fail", "Exit the Build", "exittask.bmp")]
	public class NAntFailTaskNode : NAntTaskNode
	{
		/// <summary>
		/// Creates a new <see cref="NAntFailTaskNode"/>.
		/// </summary>
		/// <param name="TaskElement">The task's XML element.</param>
		/// <param name="ParentElement">The parent XML element of the task.</param>
		/// <remarks>None.</remarks>
		public NAntFailTaskNode(XmlElement TaskElement, XmlElement ParentElement)
			: base(TaskElement, ParentElement)
		{
		}

		/// <summary>
		/// Gets or sets a message giving further information on why the build exited.
		/// </summary>
		/// <value>A message giving further information on why the build exited.</value>
		/// <remarks>None.</remarks>
		[Description("A message giving further information on why the build exited."),Category("Data")]
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