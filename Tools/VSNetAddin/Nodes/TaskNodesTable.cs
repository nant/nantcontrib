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
using System.IO;
using System.Xml;
using System.Drawing;
using System.Reflection;
using System.Collections;
using System.Windows.Forms;

namespace NAnt.Contrib.NAntAddin.Nodes
{
	/// <summary>
	/// Maintains a list of all available NAnt Task Nodes.
	/// </summary>
	internal class TaskNodesTable : Hashtable
	{
		private const string NANT_MODULE_NAME = "NAntAddin.dll";
		internal const string ADDIN_TASKS_DOC = "NAntTaskNodes.xml";
		private const string TASK_NODE_PATH = "/NAntTaskNodes/NAntTaskNode";
		private const string TASK_IMAGES_DIR = @"\TaskImages\";
		private const string ASSEMBLY_ATTR = "assembly";
		private const string TASK_NAME_ATTR = "name";
		private const string TASK_TYPENAME_ATTR = "typeName";

		private static XmlDocument taskNodesDoc = new XmlDocument();
		
		private ImageList treeImages;
		private Hashtable taskInfoTable = new Hashtable();

		/// <summary>
		/// Creates a new TaskNodesTable. Loads the collection of 
		/// tasks from the NAntTaskNodes.xml configuration file.
		/// </summary>
		public TaskNodesTable(ImageList TreeImages)
		{
			treeImages = TreeImages;

			taskNodesDoc.Load(BaseDir + ADDIN_TASKS_DOC);

			XmlNodeList taskNodes = taskNodesDoc.SelectNodes(TASK_NODE_PATH);
			for (int i = 0; i < taskNodes.Count; i++)
			{
				XmlElement taskNode = (XmlElement)taskNodes[i];

				string taskName = taskNode.Attributes[TASK_NAME_ATTR].Value;
				string taskType = taskNode.Attributes[TASK_TYPENAME_ATTR].Value;
				string assembly = taskNode.GetAttribute(ASSEMBLY_ATTR);
				Add(taskName, taskType);

				Type taskTypeObj = null;
				try
				{
					if (assembly != null && assembly != "")
					{
						Assembly taskAssembly = Assembly.LoadFrom(assembly);
						taskTypeObj = taskAssembly.GetType(taskType);
					}
					else
					{
						taskTypeObj = Type.GetType(taskType);
					}
					
					if (taskTypeObj != null)
					{
						object[] taskAttributes = taskTypeObj.GetCustomAttributes(
							typeof(NAntTaskAttribute), false);

						string imageFile = null;

						if (taskAttributes.Length > 0)
						{
							NAntTaskAttribute taskAttribute = (NAntTaskAttribute)taskAttributes[0];
							imageFile = taskAttribute.Image;
						}

						if (imageFile == null || imageFile == "")
						{
							taskInfoTable.Add(taskName, new TaskInfo(assembly, 0));
						}
						else
						{
							try
							{
								Image taskImage = Image.FromFile(
									BaseDir + 
									TASK_IMAGES_DIR + 
									imageFile);

								int imageIndex = treeImages.Images.Add(
									taskImage, Color.FromArgb(255, 0, 255));

								TaskInfo taskInfo = new TaskInfo(assembly, imageIndex);
								taskInfoTable.Add(taskName, taskInfo);
							}
							catch (Exception)
							{
								taskInfoTable.Add(taskName, new TaskInfo(assembly, 0));
							}
						}
					}
				}
				catch (Exception) {}
			}
		}

		/// <summary>
		/// Retrieves the base directory to load the configuration file from.
		/// </summary>
		public static string BaseDir
		{
			get
			{
				Module curModule = Assembly.GetAssembly(
					typeof(NAntAddin.Addin)).GetModule(NANT_MODULE_NAME);

				string modulePath = Path.GetDirectoryName(
					curModule.FullyQualifiedName);
			
				// If the compiled help file is in the same dir, 
				// we are running out of the installed version
				if (File.Exists(
					(modulePath.EndsWith(@"\") ? 
						modulePath : 
						modulePath + @"\") + 
						"NAntAddinDocs.HxS"))
				{
					return modulePath.EndsWith(@"\") ? modulePath : modulePath + @"\";
				}
				else
				{
					Uri modUri = new Uri(new Uri(modulePath), @"../", true);
					return modUri.LocalPath;
				}
			}
		}

		internal Hashtable TaskInfoTable
		{
			get
			{
				return taskInfoTable;
			}
		}

		internal ImageList TreeImages
		{
			get
			{
				return treeImages;
			}
		}

		/// <summary>
		/// Retrieves the .NET type name for a task.
		/// </summary>
		/// <param name="TaskName">The name of the task</param>
		/// <returns>The .NET type name of the task</returns>
		public Type GetTaskNodeType(string TaskName)
		{
			if (taskInfoTable.ContainsKey(TaskName))
			{
				string className = (string)this[TaskName];
				TaskInfo info = (TaskInfo)taskInfoTable[TaskName];
				if (info.AssemblyFile != null && info.AssemblyFile != "")
				{
					Assembly typeAssembly = null;
					try
					{
						typeAssembly = Assembly.LoadFrom(info.AssemblyFile);

						if (typeAssembly.GetType(className) != null)
						{
							return typeAssembly.GetType(className);
						}
						else 
						{
							return typeof(NAntTaskNode);
						}
					}
					catch (Exception)
					{
						return typeof(NAntTaskNode);
					}
				}
				return Type.GetType(className);
			}

			return typeof(NAntTaskNode);
		}

		/// <summary>
		/// Retrieves the index in the Script Explorer 
		/// Tree's ImageList of a Tasks's Image.
		/// </summary>
		/// <param name="TaskName">The name of the task</param>
		/// <returns>The index of the task image</returns>
		public int GetTaskNodeImageIndex(string TaskName)
		{
			if (taskInfoTable.ContainsKey(TaskName))
			{
				return (int)((TaskInfo)taskInfoTable[TaskName]).ImageIndex;
			}

			return 0;
		}

		/// <summary>
		/// Retrieves the assembly to find the Task's Type in.
		/// </summary>
		/// <param name="TaskName">The name of the task</param>
		/// <returns>The assembly containing the task</returns>
		public string GetTaskNodeAssembly(string TaskName)
		{
			if (taskInfoTable.ContainsKey(TaskName))
			{
				return (string)((TaskInfo)taskInfoTable[TaskName]).AssemblyFile;
			}

			return null;
		}

		/// <summary>
		/// Creates an instance of the .NET type for a task.
		/// </summary>
		/// <param name="TaskElement">The XML element defining the name of the task</param>
		/// <param name="ParentElement">The XML element in the document to create the new task</param>
		/// <returns>The TreeNode representing the new task</returns>
		public NAntTaskNode CreateNodeForTask(XmlElement TaskElement, XmlElement ParentElement)
		{
			if (taskInfoTable.ContainsKey(TaskElement.LocalName))
			{
				string className = (string)this[TaskElement.LocalName];
				TaskInfo info = (TaskInfo)taskInfoTable[TaskElement.LocalName];

				NAntTaskNode taskNode = null;
				if (info.AssemblyFile != null && info.AssemblyFile != "")
				{
					try
					{
						Assembly typeAssembly = Assembly.LoadFrom(info.AssemblyFile);
						taskNode = (NAntTaskNode)typeAssembly.CreateInstance(className, false, 
							BindingFlags.Public | BindingFlags.CreateInstance | BindingFlags.Instance, null, 
							new object[]
							{
								TaskElement, 
								ParentElement 
							}, 
							null, 
							new object[0]);

						if (taskNode == null)
						{
							taskNode = new NAntTaskNode(TaskElement, ParentElement);
						}
					}
					catch (Exception)
					{
						taskNode = new NAntTaskNode(TaskElement, ParentElement);
					}
				}
				else
				{
					taskNode = (NAntTaskNode)Activator.CreateInstance(
						Type.GetType(className), 
						new object[]
						{ 
							TaskElement, 
							ParentElement 
						});
				}
				object[] taskAttributes = taskNode.GetType().GetCustomAttributes(
					typeof(NAntTaskAttribute), false);
				if (taskAttributes.Length > 0)
				{
					NAntTaskAttribute taskAttribute = (NAntTaskAttribute)taskAttributes[0];
					taskNode.ImageIndex = GetTaskNodeImageIndex(taskAttribute.Name);
					taskNode.SelectedImageIndex = GetTaskNodeImageIndex(taskAttribute.Name);
				}

				return taskNode;
			}
			else
			{
				return new NAntTaskNode(TaskElement, ParentElement);
			}
		}
	}

	internal class TaskInfo
	{
		private string assemblyFile;
		private int imageIndex;

		public TaskInfo(string AssemblyFile, int ImageIndex)
		{
			assemblyFile = AssemblyFile;
			imageIndex = ImageIndex;
		}

		public string AssemblyFile
		{
			get
			{
				return assemblyFile;
			}
		}

		public int ImageIndex
		{
			get
			{
				return imageIndex;
			}
		}
	}
}