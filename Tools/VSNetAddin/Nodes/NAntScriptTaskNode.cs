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
using System.Globalization;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing.Design;
using System.ComponentModel.Design;
using System.Windows.Forms.ComponentModel;
using System.Windows.Forms.Design;

namespace NAnt.Contrib.NAntAddin.Nodes
{
	/// <summary>
	/// Tree Node that represents an NAnt script task.
	/// </summary>
	/// <remarks>None.</remarks>
	[NAntTask("script", "Run a Shell Script", "scripttask.bmp")]
	public class NAntScriptTaskNode : NAntTaskNode
	{
		/// <summary>
		/// Creates a new <see cref="NAntScriptTaskNode"/>.
		/// </summary>
		/// <param name="TaskElement">The task's XML element.</param>
		/// <param name="ParentElement">The parent XML element of the task.</param>
		/// <remarks>None.</remarks>
		public NAntScriptTaskNode(XmlElement TaskElement, XmlElement ParentElement) 
			: base(TaskElement, ParentElement)
		{
		}

		/// <summary>
		/// Gets or sets the language of the script.
		/// </summary>
		/// <value>The language of the script.</value>
		/// <remarks>None.</remarks>
		[Description("The Language of the Script."),Category("Data")]
		public ScriptLanguage Language
		{
			get
			{
				string language = TaskElement.GetAttribute("language");
				switch (language)
				{
					case "VB":
					{
						return ScriptLanguage.VB;
					}
					case "C#":
					{
						return ScriptLanguage.CSharp;
					}
					default:
					{
						return ScriptLanguage.JS;
					}
				}
			}

			set
			{
				string language = "JS";

				switch (value)
				{
					case ScriptLanguage.VB:
					{
						language = "VB";
						break;
					}
					case ScriptLanguage.CSharp:
					{
						language = "C#";
						break;
					}
				}

				TaskElement.SetAttribute("language", language);
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the class containing the main entrypoint.
		/// </summary>
		/// <value>The class containing the main entrypoint.</value>
		/// <remarks>None.</remarks>
		[Description("The class containing the main entrypoint."),Category("Data")]
		public string MainClass
		{
			get
			{
				return TaskElement.GetAttribute("mainclass");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("mainclass");
				}
				else
				{
					TaskElement.SetAttribute("mainclass", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the code in the script.
		/// </summary>
		/// <value>The code in the script.</value>
		/// <remarks>None.</remarks>
		[Description("The source code of the script."), Category("Data")]
		public string Code
		{
			get
			{
				XmlNode codeNode = TaskElement.SelectSingleNode("code/text()");
				if (codeNode != null)
				{
					return codeNode.Value;
				}
				return null;
			}

			set
			{
				XmlNode codeNode = TaskElement.SelectSingleNode("code");
				if (codeNode == null)
				{
					codeNode = TaskElement.OwnerDocument.CreateElement("code");
					TaskElement.AppendChild(codeNode);
				}

				codeNode.InnerText = value;
			}
		}
	}

	/// <summary>
	/// Defines constants for specifying the language of a script.
	/// </summary>
	/// <remarks>None.</remarks>
	public enum ScriptLanguage
	{
		/// <summary>
		/// Visual Basic
		/// </summary>
		VB, 
		/// <summary>
		/// C#
		/// </summary>
		CSharp, 
		/// <summary>
		/// Javascript
		/// </summary>
		JS
	}
}