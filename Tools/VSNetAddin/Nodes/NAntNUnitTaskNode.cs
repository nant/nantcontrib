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
	/// Tree Node that represents an NAnt nunit task.
	/// </summary>
	/// <remarks>None.</remarks>
	[NAntTask("nunit", "Run NUnit Tests", "nunittask.bmp")]
	public class NAntNUnitTaskNode : NAntTaskNode
	{
		/// <summary>
		/// Creates a new <see cref="NAntNUnitTaskNode"/>
		/// </summary>
		/// <param name="TaskElement">The task's XML element.</param>
		/// <param name="ParentElement">The parent XML element of the task.</param>
		/// <remarks>None.</remarks>
		public NAntNUnitTaskNode(XmlElement TaskElement, XmlElement ParentElement) 
			: base(TaskElement, ParentElement)
		{
		}

		/// <summary>
		/// Gets or sets whether the tests should run in their own AppDomain.
		/// </summary>
		/// <value>Whether the tests should run in their own AppDomain.</value>
		/// <remarks>None.</remarks>
		[Description("If the tests should run in their own AppDomain."),Category("Behavior")]
		public string Fork
		{
			get
			{
				return TaskElement.GetAttribute("fork");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("fork");
				}
				else
				{
					TaskElement.SetAttribute("fork", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets whether the build process should stop if an error occurs during the test run.
		/// </summary>
		/// <value>Whether the build process should stop if an error occurs during the test run.</value>
		/// <remarks>None.</remarks>
		[Description("If the build process should stop if an error occurs during the test run."),Category("Behavior")]
		public string HaltOnError
		{
			get
			{
				return TaskElement.GetAttribute("haltOnError");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("haltOnError");
				}
				else
				{
					TaskElement.SetAttribute("haltOnError", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets whether the build process should stop if a test fails.
		/// </summary>
		/// <value>Whether the build process should stop if a test fails.</value>
		/// <remarks>None.</remarks>
		[Description("If the build process should stop if a test fails."),Category("Behavior")]
		public string HaltOnFailure
		{
			get
			{
				return TaskElement.GetAttribute("haltOnFailure");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("haltOnFailure");
				}
				else
				{
					TaskElement.SetAttribute("haltOnFailure", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the number of milliseconds to wait before canceling tests if Fork is true.
		/// </summary>
		/// <value>The number of milliseconds to wait before canceling tests if Fork is true.</value>
		/// <remarks>None.</remarks>
		[Description("Milliseconds to wait before canceling tests if Fork is true."),Category("Behavior")]
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

		/// <summary>
		/// Gets or sets the formatters of the output of running tests.
		/// </summary>
		/// <value>The formatters of the tests.</value>
		/// <remarks>None.</remarks>
		[Editor("System.ComponentModel.Design.ArrayEditor, System.Design", 
			 typeof(System.Drawing.Design.UITypeEditor))]
		[Description("Used to format the output of test runs."), Category("Appearance")]
		public NUnitFormatter[] Formatters
		{
			get
			{
				XmlNodeList formatterElems = TaskElement.GetElementsByTagName("formatter");
				NUnitFormatter[] formatters = new NUnitFormatter[formatterElems.Count];
				NUnitFormatter[] readOnlyFormatters = new NUnitFormatter[formatterElems.Count];

				for (int i = 0; i < formatterElems.Count; i++)
				{
					XmlElement formatterElem = (XmlElement)formatterElems.Item(i);
					formatters[i] = new NUnitFormatter(formatterElem);

					if (Parent == null)
					{
						// If read only, create a proxy node
						readOnlyFormatters[i] = (NUnitFormatter)NAntReadOnlyNodeBuilder.GetReadOnlyNode(
							formatters[i]);
					}
				}
				if (Parent == null)
				{
					return readOnlyFormatters;
				}
				return formatters;
			}

			set
			{
				// Remove old formatters
				XmlNodeList formatterElems = TaskElement.SelectNodes("formatter");
				if (formatterElems != null)
				{
					foreach (XmlNode formatter in formatterElems)
					{
						TaskElement.RemoveChild(formatter);
					}
				}

				if (value != null)
				{
					for (int i = 0; i < value.Length; i++)
					{
						// Append new formatters
						value[i].AppendToParent(TaskElement);
					}
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the tests to run.
		/// </summary>
		/// <value>The tests to run.</value>
		/// <remarks>None.</remarks>
		[Editor("System.ComponentModel.Design.ArrayEditor, System.Design", 
			 typeof(System.Drawing.Design.UITypeEditor))]
		[Description("The tests to run."), Category("Data")]
		public NUnitTest[] Tests
		{
			get
			{
				XmlNodeList testElems = TaskElement.GetElementsByTagName("test");
				NUnitTest[] tests = new NUnitTest[testElems.Count];
				NUnitTest[] readOnlyTests = new NUnitTest[testElems.Count];

				for (int i = 0; i < testElems.Count; i++)
				{
					XmlElement testElem = (XmlElement)testElems.Item(i);
					tests[i] = new NUnitTest(testElem);

					if (Parent == null)
					{
						// If read only, create a proxy node
						readOnlyTests[i] = (NUnitTest)NAntReadOnlyNodeBuilder.GetReadOnlyNode(
							tests[i]);
					}
				}
				if (Parent == null)
				{
					return readOnlyTests;
				}
				return tests;
			}

			set
			{
				// Remove old tests
				XmlNodeList testElems = TaskElement.SelectNodes("test");
				if (testElems != null)
				{
					foreach (XmlNode test in testElems)
					{
						TaskElement.RemoveChild(test);
					}
				}

				if (value != null)
				{
					for (int i = 0; i < value.Length; i++)
					{
						// Append new tests
						value[i].AppendToParent(TaskElement);
					}
				}
				Save();
			}
		}
	}

	/// <summary>
	/// Specifies an NUnit Formatter.
	/// </summary>
	/// <remarks>None.</remarks>
	public class NUnitFormatter : Component, ConstructorArgsResolver
	{
		private string type;
		internal XmlElement formatterElement;

		/// <summary>
		/// Creates a new <see cref="NUnitFormatter"/>.
		/// </summary>
		/// <remarks>None.</remarks>
		public NUnitFormatter()
		{
		}

		/// <summary>
		/// Creates a new <see cref="NUnitFormatter"/>.
		/// </summary>
		/// <param name="FormatterElement">The formatter XML element.</param>
		/// <remarks>None.</remarks>
		public NUnitFormatter(XmlElement FormatterElement)
		{
			formatterElement = FormatterElement;
		}

		/// <summary>
		/// Gets or sets the type of formatter.
		/// </summary>
		/// <value>The type of formatter.</value>
		/// <remarks>None.</remarks>
		[Description("The Type of Formatter (Xml or Plain)")]
		public string Type
		{
			get
			{
				if (formatterElement != null)
				{
					return formatterElement.GetAttribute("type");
				}
				return type;
			}

			set
			{
				if (formatterElement == null)
				{
					type = value;
				}
				else
				{
					if (value == "")
					{
						formatterElement.RemoveAttribute("type");
					}
					else
					{
						formatterElement.SetAttribute("type", value);
					}
				}
			}
		}

		/// <summary>
		/// Appends the current Formatter Element to the task element.
		/// </summary>
		/// <param name="ParentElement">The parent XML Element to append to.</param>
		/// <remarks>None.</remarks>
		public void AppendToParent(XmlElement ParentElement)
		{
			if (formatterElement == null)
			{
				CreateElement(ParentElement.OwnerDocument);
			}
			ParentElement.AppendChild(formatterElement);
		}
	
		/// <summary>
		/// Creates the formatter XML element.
		/// </summary>
		/// <param name="Document">The XML Document to use.</param>
		/// <remarks>None.</remarks>
		private void CreateElement(XmlDocument Document)
		{
			formatterElement = Document.CreateElement("formatter");
			formatterElement.SetAttribute("type", type);
		}

		/// <summary>
		/// Returns the arguments that must be passed to 
		/// the constructor of an object to create the 
		/// same object.
		/// </summary>
		/// <returns>The arguments to pass.</returns>
		/// <remarks>None.</remarks>
		public Object[] GetConstructorArgs()
		{
			return new object[]
			{
				formatterElement
			};
		}
	}

	/// <summary>
	/// Specifies an NUnit Test.
	/// </summary>
	/// <remarks>None.</remarks>
	public class NUnitTest : Component, ConstructorArgsResolver
	{
		private string name, assembly, outfile;
		private XmlElement testElement;

		/// <summary>
		/// Creates a new <see cref="NUnitTest"/>.
		/// </summary>
		/// <remarks>None.</remarks>
		public NUnitTest()
		{
		}

		/// <summary>
		/// Creates a new <see cref="NUnitTest"/>.
		/// </summary>
		/// <param name="TestElement">The test XML element.</param>
		/// <remarks>None.</remarks>
		public NUnitTest(XmlElement TestElement)
		{
			testElement = TestElement;
		}

		/// <summary>
		/// Gets or sets the name of the test.
		/// </summary>
		/// <value>The name of the test.</value>
		/// <remarks>None.</remarks>
		[Description("The name of the test.")]
		public string Name
		{
			get
			{
				if (testElement != null)
				{
					return testElement.GetAttribute("name");
				}
				return name;
			}

			set
			{
				if (testElement == null)
				{
					name = value;
				}
				else
				{
					if (value == "")
					{
						testElement.RemoveAttribute("name");
					}
					else
					{
						testElement.SetAttribute("name", value);
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets the assembly in which the test resides.
		/// </summary>
		/// <value>The assembly in which the test resides.</value>
		/// <remarks>None.</remarks>
		[Description("The assembly in which the test resides.")]
		public string Assembly
		{
			get
			{
				if (testElement != null)
				{
					return testElement.GetAttribute("assembly");
				}
				return assembly;
			}

			set
			{
				if (testElement == null)
				{
					assembly = value;
				}
				else
				{
					if (value == "")
					{
						testElement.RemoveAttribute("assembly");
					}
					else
					{
						testElement.SetAttribute("assembly", value);
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets the output file to place test results in.
		/// </summary>
		/// <value>The output file to place test results in.</value>
		/// <remarks>None.</remarks>
		[Description("The output file to place test results in.")]
		public string OutFile
		{
			get
			{
				if (testElement != null)
				{
					return testElement.GetAttribute("outfile");
				}
				return outfile;
			}

			set
			{
				if (testElement == null)
				{
					outfile = value;
				}
				else
				{
					if (value == "")
					{
						testElement.RemoveAttribute("outfile");
					}
					else
					{
						testElement.SetAttribute("outfile", value);
					}
				}
			}
		}

		/// <summary>
		/// Appends the current Test Element to the task element.
		/// </summary>
		/// <param name="ParentElement">The parent XML Element to append to.</param>
		/// <remarks>None.</remarks>
		public void AppendToParent(XmlElement ParentElement)
		{
			if (testElement == null)
			{
				CreateElement(ParentElement.OwnerDocument);
			}
			ParentElement.AppendChild(testElement);
		}
	
		/// <summary>
		/// Creates the test XML element.
		/// </summary>
		/// <param name="Document">The XML Document to use.</param>
		/// <remarks>None.</remarks>
		private void CreateElement(XmlDocument Document)
		{
			testElement = Document.CreateElement("test");
			testElement.SetAttribute("name", name);
			testElement.SetAttribute("assembly", assembly);
			testElement.SetAttribute("outfile", outfile);
		}

		/// <summary>
		/// Returns the arguments that must be passed to 
		/// the constructor of an object to create the 
		/// same object.
		/// </summary>
		/// <returns>The arguments to pass.</returns>
		/// <remarks>None.</remarks>
		public Object[] GetConstructorArgs()
		{
			return new object[]
			{
				testElement
			};
		}
	}
}