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
	/// Tree Node that represents an NAnt exec task.
	/// </summary>
	/// <remarks>None.</remarks>
	[NAntTask("exec", "Run a System Command", "exectask.bmp")]
	public class NAntExecuteTaskNode : NAntTaskNode
	{
		/// <summary>
		/// Creates a new <see cref="NAntExecuteTaskNode"/>.
		/// </summary>
		/// <param name="TaskElement">The task's XML element.</param>
		/// <param name="ParentElement">The parent XML element of the task.</param>
		/// <remarks>None.</remarks>
		public NAntExecuteTaskNode(XmlElement TaskElement, XmlElement ParentElement) 
			: base(TaskElement, ParentElement)
		{
		}

		/// <summary>
		/// Gets or sets the program to execute without command line arguments.
		/// </summary>
		/// <value>The program to execute without command-line arguments.</value>
		/// <remarks>None.</remarks>
		[Description("The program to execute without command line arguments."),Category("Data")]
		public string Program
		{
			get
			{
				return TaskElement.GetAttribute("program");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("program");
				}
				else
				{
					TaskElement.SetAttribute("program", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the command line arguments for the program.
		/// </summary>
		/// <value>The command line arguments for the program.</value>
		/// <remarks>None.</remarks>
		[Description("Command line arguments for the program."),Category("Data")]
		public string CommandLine
		{
			get
			{
				return TaskElement.GetAttribute("commandline");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("commandline");
				}
				else
				{
					TaskElement.SetAttribute("commandline", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the directory in which the command will be executed.
		/// </summary>
		/// <value>The directory in which the command will be executed.</value>
		/// <remarks>None.</remarks>
		[Description("The directory in which the command will be executed."),Category("Data")]
		public string BaseDir
		{
			get
			{
				return TaskElement.GetAttribute("basedir");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("basedir");
				}
				else
				{
					TaskElement.SetAttribute("basedir", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets whether the build should stop if the 
		/// command does not finish within the specified time.
		/// </summary>
		/// <value>Whether the build should stop if the command 
		/// does not finish within the specified time.</value>
		/// <remarks>None.</remarks>
		[Description("Whether the build should stop if the command does not finish within the specified time."),Category("Behavior")]
		public string Timeout
		{
			get
			{
				return TaskElement.GetAttribute("timeout");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("timeout");
				}
				else
				{
					TaskElement.SetAttribute("timeout", value);
				}
				Save();
			}
		}
	}
}