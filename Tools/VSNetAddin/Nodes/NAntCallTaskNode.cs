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
	/// Tree Node that represents an NAnt call task.
	/// </summary>
	/// <remarks>None.</remarks>
	[NAntTask("call", "Call an NAnt Target", "calltask.bmp")]
	public class NAntCallTaskNode : NAntTaskNode
	{
		/// <summary>
		/// Creates a new <see cref="NAntCallTaskNode"/>.
		/// </summary>
		/// <param name="TaskElement">The task's XML element.</param>
		/// <param name="ParentElement">The parent XML element of the task.</param>
		/// <remarks>None.</remarks>
		public NAntCallTaskNode(XmlElement TaskElement, XmlElement ParentElement) 
			: base(TaskElement, ParentElement)
		{
		}

		/// <summary>
		/// Gets or sets the target to call.
		/// </summary>
		/// <value>The target to call.</value>
		/// <remarks>None.</remarks>
		[Description("The target to call."),Category("Data")]
		public string Target
		{
			get
			{
				return TaskElement.GetAttribute("target");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("target");
				}
				else
				{
					TaskElement.SetAttribute("target", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets if the target should be forced to run even if it has already been executed.
		/// </summary>
		/// <value>If the target should be forced to run even if it has already been executed.</value>
		/// <remarks>None.</remarks>
		[Description("Force to run even if the target already been executed."),Category("Behavior")]
		public string Force
		{
			get
			{
				return TaskElement.GetAttribute("force");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("force");
				}
				else
				{
					TaskElement.SetAttribute("force", value);
				}
				Save();
			}
		}
	}
}