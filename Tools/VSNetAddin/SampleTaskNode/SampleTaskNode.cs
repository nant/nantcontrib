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

using NAnt.Contrib.NAntAddin.Nodes;

namespace NAntAddinSample
{
	/// <summary>
	/// Tree Node that represents a "sampletask" task.
	/// </summary>
	/// <remarks>None.</remarks>
	[NAntTask("sampletask", "Sample Task", "sampletask.bmp")]
	public class SampleTaskNode : NAntTaskNode
	{
		/// <summary>
		/// Creates a new <see cref="SampleTaskNode"/>.
		/// </summary>
		/// <param name="TaskElement">The task's XML element.</param>
		/// <param name="ParentElement">The parent XML element of the task.</param>
		/// <remarks>None.</remarks>
		public SampleTaskNode(XmlElement TaskElement, XmlElement ParentElement) 
			: base(TaskElement, ParentElement)
		{
		}

		/// <summary>
		/// Gets or sets a sample task property.
		/// </summary>
		/// <value>A sample task property.</value>
		/// <remarks>None.</remarks>
		[Description("A sample task property."),Category("Data")]
		public string SampleProperty
		{
			get
			{
				return TaskElement.GetAttribute("sampleproperty");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("sampleproperty");
				}
				else
				{
					TaskElement.SetAttribute("sampleproperty", value);
				}
				Save();
			}
		}
	}
}