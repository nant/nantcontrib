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
using NAnt.Contrib.NAntAddin.Controls;

namespace NAnt.Contrib.NAntAddin.Nodes
{
	/// <summary>
	/// Tree Node that represents an NAnt touch task.
	/// </summary>
	/// <remarks>None.</remarks>
	[NAntTask("touch", "Change the Modification Date/Time on Files", "touchtask.bmp")]
	public class NAntTouchTaskNode : NAntTaskNode
	{
		/// <summary>
		/// Creates a new <see cref="NAntTouchTaskNode"/>.
		/// </summary>
		/// <param name="TaskElement">The task's XML element.</param>
		/// <param name="ParentElement">The parent XML element of the task.</param>
		/// <remarks>None.</remarks>
		public NAntTouchTaskNode(XmlElement TaskElement, XmlElement ParentElement) 
			: base(TaskElement, ParentElement)
		{
		}

		/// <summary>
		/// Gets or sets the assembly filename.
		/// </summary>
		/// <value>The assembly filename.</value>
		/// <remarks>None.</remarks>
		[Description("Assembly Filename (required unless a fileset is specified)."),Category("Data")]
		public string File
		{
			get
			{
				return TaskElement.GetAttribute("file");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("file");
				}
				else
				{
					TaskElement.SetAttribute("file", value);
				}
				Save();
			}
		}		

		/// <summary>
		/// Gets or sets the new modification time of the 
		/// file in milliseconds since midnight Jan 1 1970.
		/// </summary>
		/// <value>The new modification time of the file 
		/// in milliseconds since midnight Jan 1 1970.</value>
		/// <remarks>None.</remarks>
		[Description("New modification time of the file in milliseconds since midnight Jan 1 1970."),Category("Data")]
		public string Millis
		{
			get
			{
				return TaskElement.GetAttribute("millis");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("millis");
				}
				else
				{
					TaskElement.SetAttribute("millis", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the new modification time of the file in the format MM/DD/YYYY HH:MM AM_or_PM.
		/// </summary>
		/// <value>The new modification time of the file in the format MM/DD/YYYY HH:MM AM_or_PM.</value>
		/// <remarks>None.</remarks>
		[Description("New modification time of the file in the format MM/DD/YYYY HH:MM AM_or_PM."),Category("Data")]
		public string DateTime
		{
			get
			{
				return TaskElement.GetAttribute("datetime");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("datetime");
				}
				else
				{
					TaskElement.SetAttribute("datetime", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the fileset to use instead of single file.
		/// </summary>
		/// <value>The fileset to use instead of single file.</value>
		/// <remarks>None.</remarks>
		[Description("Fileset to use instead of single file."),Category("Data")]
		public FileSet FileSet
		{
			get
			{
				FileSet fileSet = new FileSet(TaskElement, this);
				if (Parent == null)
				{
					return (FileSet)NAntReadOnlyNodeBuilder.GetReadOnlyNode(fileSet);
				}
				return fileSet;
			}

			set
			{
				value.AppendToTask(TaskElement, "fileset");
				Save();
			}
		}
	}
}