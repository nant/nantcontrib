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
	/// Tree Node that represents an NAnt attrib task.
	/// </summary>
	/// <remarks>None.</remarks>
	[NAntTask("attrib", "Modify file attributes", "attrtask.bmp")]
	public class NAntAttribTaskNode : NAntTaskNode
	{
		/// <summary>
		/// Creates a new <see cref="NAntAttribTaskNode"/>.
		/// </summary>
		/// <param name="TaskElement">The task's XML element.</param>
		/// <param name="ParentElement">The parent XML element of the task.</param>
		/// <remarks>None.</remarks>
		public NAntAttribTaskNode(XmlElement TaskElement, XmlElement ParentElement) 
			: base(TaskElement, ParentElement)
		{
		}

		/// <summary>
		/// Gets or sets the file to modify the attributes of.
		/// </summary>
		/// <value>The file to modify the attributes of.</value>
		/// <remarks>None.</remarks>
		[Description("The file to modify the attributes of."),Category("Data")]
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
		/// Gets or sets if the archive flag should be set.
		/// </summary>
		/// <value>If the archive flag should be set.</value>
		/// <remarks>None.</remarks>
		[Description("If the archive flag should be set."),Category("Data")]
		public string Archive
		{
			get
			{
				return TaskElement.GetAttribute("archive");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("archive");
				}
				else
				{
					TaskElement.SetAttribute("archive", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets if the hidden flag should be set.
		/// </summary>
		/// <value>If the hidden flag should be set.</value>
		/// <remarks>None.</remarks>
		[Description("If the hidden flag should be set."),Category("Data")]
		public string Hidden
		{
			get
			{
				return TaskElement.GetAttribute("hidden");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("hidden");
				}
				else
				{
					TaskElement.SetAttribute("hidden", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets if all other attributes are set to false.
		/// </summary>
		/// <value>If all other attributes are set to false.</value>
		/// <remarks>None.</remarks>
		[Description("Sets all other set attributes to false."),Category("Data")]
		public string Normal
		{
			get
			{
				return TaskElement.GetAttribute("normal");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("normal");
				}
				else
				{
					TaskElement.SetAttribute("normal", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets if the read only flag should be set.
		/// </summary>
		/// <value>If the read only flag should be set.</value>
		/// <remarks>None.</remarks>
		[Description("If the read only flag should be set."),Category("Data")]
		public string ReadOnly
		{
			get
			{
				return TaskElement.GetAttribute("readonly");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("readonly");
				}
				else
				{
					TaskElement.SetAttribute("readonly", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets if the system flag should be set.
		/// </summary>
		/// <value>If the system flag should be set.</value>
		/// <remarks>None.</remarks>
		[Description("If the system flag should be set."),Category("Data")]
		public string System
		{
			get
			{
				return TaskElement.GetAttribute("system");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("system");
				}
				else
				{
					TaskElement.SetAttribute("system", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the source files to modify the attributes of.
		/// </summary>
		/// <value>The source files to modify the attributes of.</value>
		/// <remarks>None.</remarks>
		[Description("Source files to modify the attributes of."),Category("Data")]
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