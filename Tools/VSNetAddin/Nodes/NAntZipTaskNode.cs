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
	/// Tree Node that represents an NAnt zip task.
	/// </summary>
	/// <remarks>None.</remarks>
	[NAntTask("zip", "Create a ZIP Archive", "ziptask.bmp")]
	public class NAntZipTaskNode : NAntTaskNode
	{
		/// <summary>
		/// Creates a new <see cref="NAntZipTaskNode"/>.
		/// </summary>
		/// <param name="TaskElement">The task's XML element.</param>
		/// <param name="ParentElement">The parent XML element of the task.</param>
		/// <remarks>None.</remarks>
		public NAntZipTaskNode(XmlElement TaskElement, XmlElement ParentElement) 
			: base(TaskElement, ParentElement)
		{
		}

		/// <summary>
		/// Gets or sets the name of the .zip file to create.
		/// </summary>
		/// <value>The name of the .zip file to create.</value>
		/// <remarks>None.</remarks>
		[Description("The name of the .zip file to create."),Category("Data")]
		public string ZipFile
		{
			get
			{
				return TaskElement.GetAttribute("zipfile");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("zipfile");
				}
				else
				{
					TaskElement.SetAttribute("zipfile", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the level of zip compression.
		/// </summary>
		/// <value>The level of zip compression.</value>
		/// <remarks>None.</remarks>
		[Description("The level of zip compression."),Category("Data")]
		public string ZipLevel
		{
			get
			{
				return TaskElement.GetAttribute("ziplevel");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("ziplevel");
				}
				else
				{
					TaskElement.SetAttribute("ziplevel", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the source files to zip.
		/// </summary>
		/// <value>The source files to zip.</value>
		/// <remarks>None.</remarks>
		[Description("Source files to zip."),Category("Data")]
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