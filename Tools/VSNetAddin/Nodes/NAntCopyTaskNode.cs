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
	/// Tree Node that represents an NAnt copy task.
	/// </summary>
	/// <remarks>None.</remarks>
	[NAntTask("copy", "Copy Files", "copytask.bmp")]
	public class NAntCopyTaskNode : NAntTaskNode
	{
		/// <summary>
		/// Creates a new <see cref="NAntCopyTaskNode"/>.
		/// </summary>
		/// <param name="TaskElement">The task's XML element.</param>
		/// <param name="ParentElement">The parent XML element of the task.</param>
		/// <remarks>None.</remarks>
		public NAntCopyTaskNode(XmlElement TaskElement, XmlElement ParentElement) 
			: base(TaskElement, ParentElement)
		{
		}

		/// <summary>
		/// Gets or sets the file to copy.
		/// </summary>
		/// <value>The file to copy.</value>
		/// <remarks>None.</remarks>
		[Description("The file to copy."),Category("Data")]
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
		/// Gets or sets the file to copy to.
		/// </summary>
		/// <value>The file to copy to.</value>
		/// <remarks>None.</remarks>
		[Description("The file to copy to."),Category("Data")]
		public string ToFile
		{
			get
			{
				return TaskElement.GetAttribute("tofile");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("tofile");
				}
				else
				{
					TaskElement.SetAttribute("tofile", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the directory to copy to.
		/// </summary>
		/// <value>The directory to copy to.</value>
		/// <remarks>None.</remarks>
		[Description("The directory to copy to."),Category("Data")]
		public string ToDir
		{
			get
			{
				return TaskElement.GetAttribute("todir");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("todir");
				}
				else
				{
					TaskElement.SetAttribute("todir", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets if existing files should be overwritten even if newer.
		/// </summary>
		/// <value>If existing files should be overwritten even if newer.</value>
		/// <remarks>None.</remarks>
		[Description("If existing files should be overwritten even if newer."),Category("Behavior")]
		public string Overwrite
		{
			get
			{
				return TaskElement.GetAttribute("overwrite");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("overwrite");
				}
				else
				{
					TaskElement.SetAttribute("overwrite", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the source files to copy.
		/// </summary>
		/// <value>The source files to copy.</value>
		/// <remarks>None.</remarks>
		[Description("Source files to copy."),Category("Data")]
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