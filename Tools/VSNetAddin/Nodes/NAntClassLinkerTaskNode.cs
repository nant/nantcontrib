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
	/// Tree Node that represents an NAnt cl task.
	/// </summary>
	/// <remarks>None.</remarks>
	[NAntTask("cl", "Compile a C/C++ Program", "cltask.bmp")]
	public class NAntClassLinkerTaskNode : NAntTaskNode
	{
		/// <summary>
		/// Creates a new <see cref="NAntClassLinkerTaskNode"/>.
		/// </summary>
		/// <param name="TaskElement">The task's XML element.</param>
		/// <param name="ParentElement">The parent XML element of the task.</param>
		/// <remarks>None.</remarks>
		public NAntClassLinkerTaskNode(XmlElement TaskElement, XmlElement ParentElement) 
			: base(TaskElement, ParentElement)
		{
		}

		/// <summary>
		/// Gets or sets options to pass to the compiler.
		/// </summary>
		/// <value>Options to pass to the compiler.</value>
		/// <remarks>None.</remarks>
		[Description("Options to pass to the compiler."),Category("Data")]
		public string Options
		{
			get
			{
				return TaskElement.GetAttribute("options");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("options");
				}
				else
				{
					TaskElement.SetAttribute("options", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the directory where all output files are placed.
		/// </summary>
		/// <value>The directory where all output files are placed.</value>
		/// <remarks>None.</remarks>
		[Description("Directory where all output files are placed."),Category("Data")]
		public string OutputDir
		{
			get
			{
				return TaskElement.GetAttribute("outputdir");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("outputdir");
				}
				else
				{
					TaskElement.SetAttribute("outputdir", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the precompiled header file.
		/// </summary>
		/// <value>The precompiled header file.</value>
		/// <remarks>None.</remarks>
		[Description("The precompiled header file."),Category("Data")]
		public string PchFile
		{
			get
			{
				return TaskElement.GetAttribute("pchfile");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("pchfile");
				}
				else
				{
					TaskElement.SetAttribute("pchfile", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets source files to compile.
		/// </summary>
		/// <value>Source files to compile.</value>
		/// <remarks>None.</remarks>
		[Description("Source files to compile."),Category("Data")]
		public Sources Sources
		{
			get
			{
				Sources sources = new Sources(TaskElement, this);
				if (Parent == null)
				{
					return (Sources)NAntReadOnlyNodeBuilder.GetReadOnlyNode(sources);
				}
				return sources;
			}

			set
			{
				value.AppendToTask(TaskElement, "sources");
				Save();
			}
		}

		/// <summary>
		/// Gets or sets directories in which to search for include files.
		/// </summary>
		/// <value>Directories in which to search for include files.</value>
		/// <remarks>None.</remarks>
		[Description("Directories in which to search for include files."),Category("Data")]
		public IncludeDirs IncludeDirs
		{
			get
			{
				IncludeDirs includeDirs = new IncludeDirs(TaskElement, this);
				if (Parent == null)
				{
					return (IncludeDirs)NAntReadOnlyNodeBuilder.GetReadOnlyNode(includeDirs);
				}
				return includeDirs;
			}

			set
			{
				value.AppendToTask(TaskElement, "includedirs");
				Save();
			}
		}
	}

	/// <summary>
	/// An <see cref="NAntFileSet"/> that specifies included directories.
	/// </summary>
	/// <remarks>None.</remarks>
	public class IncludeDirs : NAntFileSet
	{
		/// <summary>
		/// Creates a new <see cref="IncludeDirs"/>.
		/// </summary>
		/// <param name="TaskElement">The includedirs XML element.</param>
		/// <param name="TaskNode">The <see cref="NAntTaskNode"/> for which this <see cref="IncludeDirs"/> is a property.</param>
		/// <remarks>None.</remarks>
		public IncludeDirs(XmlElement TaskElement, NAntTaskNode TaskNode) : base(TaskNode, TaskElement, "includedirs")
		{
			
		}
	}
}