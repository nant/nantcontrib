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
	/// Tree Node that represents an NAnt delete task.
	/// </summary>
	/// <remarks>None.</remarks>
	[NAntTask("delete", "Delete Files", "deletetask.bmp")]
	public class NAntDeleteTaskNode : NAntTaskNode
	{
		/// <summary>
		/// Creates a new <see cref="NAntDeleteTaskNode"/>.
		/// </summary>
		/// <param name="TaskElement">The task's XML element.</param>
		/// <param name="ParentElement">The parent XML element of the task.</param>
		/// <remarks>None.</remarks>
		public NAntDeleteTaskNode(XmlElement TaskElement, XmlElement ParentElement) 
			: base(TaskElement, ParentElement)
		{
		}

		/// <summary>
		/// Gets or sets the directory to delete.
		/// </summary>
		/// <value>The directory to delete.</value>
		/// <remarks>None.</remarks>
		[Description("The directory to delete."),Category("Data")]
		public string Dir
		{
			get
			{
				return TaskElement.GetAttribute("dir");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("dir");
				}
				else
				{
					TaskElement.SetAttribute("dir", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the file to delete.
		/// </summary>
		/// <value>The file to delete.</value>
		/// <remarks>None.</remarks>
		[Description("The file to delete."),Category("Data")]
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
		/// Gets or sets the files and/or directories to delete.
		/// </summary>
		/// <value>The files and/or directories to delete.</value>
		/// <remarks>None.</remarks>
		[Description("Files and/or directories to delete."),Category("Data")]
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