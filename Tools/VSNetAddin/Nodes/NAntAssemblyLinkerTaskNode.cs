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
	/// Tree Node that represents an NAnt al task.
	/// </summary>
	/// <remarks>None.</remarks>
	[NAntTask("al", "Link Files into an Assembly", "altask.bmp")]
	public class NAntAssemblyLinkerTaskNode : NAntTaskNode
	{
		/// <summary>
		/// Creates a new <see cref="NAntAssemblyLinkerTaskNode"/>.
		/// </summary>
		/// <param name="TaskElement">The task's XML element.</param>
		/// <param name="ParentElement">The parent XML element of the task.</param>
		/// <remarks>None.</remarks>
		public NAntAssemblyLinkerTaskNode(XmlElement TaskElement, XmlElement ParentElement) 
			: base(TaskElement, ParentElement)
		{
		}

		/// <summary>
		/// Gets or sets the output file for the assembly manifest.
		/// </summary>
		/// <value>The output file for the assembly manifest.</value>
		/// <remarks>None.</remarks>
		[Description("Output file for the assembly manifest."),Category("Data")]
		public string Output
		{
			get
			{
				return TaskElement.GetAttribute("output");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("output");
				}
				else
				{
					TaskElement.SetAttribute("output", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the type of file generated at compilation.
		/// </summary>
		/// <value>The type of file generated at compilation.</value>
		/// <remarks>None.</remarks>
		[Description("The type of file generated at compilation."),Category("Behavior")]
		public AssemblyLinkerTarget Target
		{
			get
			{
				string language = TaskElement.GetAttribute("target");
				switch (language)
				{
					case "win":
					{
						return AssemblyLinkerTarget.WinExecutable;
					}
					case "lib":
					{
						return AssemblyLinkerTarget.Library;
					}
					default:
					{
						return AssemblyLinkerTarget.Executable;
					}
				}
			}

			set
			{
				string language = "exe";

				switch (value)
				{
					case AssemblyLinkerTarget.WinExecutable:
					{
						language = "win";
						break;
					}
					case AssemblyLinkerTarget.Library:
					{
						language = "lib";
						break;
					}
				}

				TaskElement.SetAttribute("target", language);
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the culture string associated with the output assembly.
		/// </summary>
		/// <value>The culture string associated with the output assembly.</value>
		/// <remarks>None.</remarks>
		[Description("Culture string associated with the output assembly."),Category("Appearance")]
		public string Culture
		{
			get
			{
				return TaskElement.GetAttribute("culture");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("culture");
				}
				else
				{
					TaskElement.SetAttribute("culture", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the assembly from which to get all options except the culture field.
		/// </summary>
		/// <value>The assembly from which to get all options except the culture field.</value>
		/// <remarks>None.</remarks>
		[Description("Assembly from which to get all options except the culture field."),Category("Data")]
		public string Template
		{
			get
			{
				return TaskElement.GetAttribute("template");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("template");
				}
				else
				{
					TaskElement.SetAttribute("template", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the source files to embed.
		/// </summary>
		/// <value>The source files to embed.</value>
		/// <remarks>None.</remarks>
		[Description("Source files to embed."),Category("Data")]
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
	}

	/// <summary>
	/// Defines constants for specifying the target of an assembly linking.
	/// </summary>
	/// <remarks>None.</remarks>
	public enum AssemblyLinkerTarget
	{
		/// <summary>
		/// The target is a library (.dll).
		/// </summary>
		Library,
		/// <summary>
		/// The target is a command-line executable (.exe).
		/// </summary>
		Executable,
		/// <summary>
		/// The target is a windows executable (.exe).
		/// </summary>
		WinExecutable
	}
}