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
	/// Tree Node that represents an NAnt style task.
	/// </summary>
	/// <remarks>None.</remarks>
	[NAntTask("style", "Perform an XSLT Transformation", "styletask.bmp")]
	public class NAntStyleTaskNode : NAntTaskNode
	{
		/// <summary>
		/// Creates a new <see cref="NAntStyleTaskNode"/>.
		/// </summary>
		/// <param name="TaskElement">The task's XML element.</param>
		/// <param name="ParentElement">The parent XML element of the task.</param>
		/// <remarks>None.</remarks>
		public NAntStyleTaskNode(XmlElement TaskElement, XmlElement ParentElement) 
			: base(TaskElement, ParentElement)
		{	
		}

		/// <summary>
		/// Gets or sets the directory containing the XML document(s).
		/// </summary>
		/// <value>The directory containing the XML document(s).</value>
		/// <remarks>None.</remarks>
		[Description("Directory containing the XML document(s)."),Category("Data")]
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
		/// Gets or sets the directory in which to store the results.
		/// </summary>
		/// <value>The directory in which to store the results.</value>
		/// <remarks>None.</remarks>
		[Description("Directory in which to store the results."),Category("Data")]
		public string DestDir
		{
			get
			{
				return TaskElement.GetAttribute("destdir");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("destdir");
				}
				else
				{
					TaskElement.SetAttribute("destdir", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the desired file extension for targets.
		/// </summary>
		/// <value>The desired file extension for targets.</value>
		/// <remarks>None.</remarks>
		[Description("Desired file extension for targets. Default is \"html\"."),Category("Data")]
		public string Extension
		{
			get
			{
				return TaskElement.GetAttribute("extension");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("extension");
				}
				else
				{
					TaskElement.SetAttribute("extension", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the filename of the XSL stylesheet.
		/// </summary>
		/// <value>The filename of the XSL stylesheet.</value>
		/// <remarks>None.</remarks>
		[Description("Filename of the XSL stylesheet."),Category("Data")]
		public string Style
		{
			get
			{
				return TaskElement.GetAttribute("style");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("style");
				}
				else
				{
					TaskElement.SetAttribute("style", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the filename of the XML document.
		/// </summary>
		/// <value>The filename of the XML document.</value>
		/// <remarks>None.</remarks>
		[Description("Filename of the XML document."),Category("Data")]
		public string In
		{
			get
			{
				return TaskElement.GetAttribute("in");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("in");
				}
				else
				{
					TaskElement.SetAttribute("in", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the filename of the output document.
		/// </summary>
		/// <value>The filename of the output document.</value>
		/// <remarks>None.</remarks>
		[Description("Filename of the output document."),Category("Data")]
		public string Out
		{
			get
			{
				return TaskElement.GetAttribute("out");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("out");
				}
				else
				{
					TaskElement.SetAttribute("out", value);
				}
				Save();
			}
		}
	}
}