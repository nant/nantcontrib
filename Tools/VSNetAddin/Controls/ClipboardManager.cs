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
using System.Drawing;
using System.Windows.Forms;
using NAnt.Contrib.NAntAddin.Nodes;
using NAnt.Contrib.NAntAddin.Controls;
	
namespace NAnt.Contrib.NAntAddin.Controls
{
	/// <summary>
	/// Manages NAnt objects placed on the clipboard.
	/// </summary>
	internal class ClipboardManager
	{
		private bool cutting = false;
		private NAntBaseNode baseNode = null;
		private ClipboardContents contents = ClipboardContents.EMPTY;
		internal TaskNodesTable tasksTable;

		/// <summary>
		/// Creates a new <see cref="ClipboardManager"/>.
		/// </summary>
		/// <param name="TasksTable">The table of NAnt Tasks</param>
		public ClipboardManager(TaskNodesTable TasksTable)
		{
			tasksTable = TasksTable;
		}

		/// <summary>
		/// Cuts a Property Node.
		/// </summary>
		/// <param name="PropertyNode">The NAnt Property Node to Cut</param>
		internal void Cut(NAntPropertyNode PropertyNode)
		{
			if (cutting)
			{
				baseNode.ForeColor = SystemColors.WindowText;
			}

			baseNode = PropertyNode;
			baseNode.ForeColor = SystemColors.ControlDark;
			contents = ClipboardContents.PROPERTY;
			cutting = true;
		}

		/// <summary>
		/// Cuts a Target Node.
		/// </summary>
		/// <param name="TargetNode">The NAnt Target Node to Cut</param>
		internal void Cut(NAntTargetNode TargetNode)
		{
			if (cutting)
			{
				baseNode.ForeColor = SystemColors.WindowText;
			}

			baseNode = TargetNode;
			contents = ClipboardContents.TARGET;
			baseNode.ForeColor = SystemColors.ControlDark;
			cutting = true;
		}

		/// <summary>
		/// Cuts a Task Node.
		/// </summary>
		/// <param name="TaskNode">The NAnt Task Node to Cut</param>
		internal void Cut(NAntTaskNode TaskNode)
		{
			if (cutting)
			{
				baseNode.ForeColor = SystemColors.WindowText;
			}

			baseNode = TaskNode;
			contents = ClipboardContents.TASK;
			baseNode.ForeColor = SystemColors.ControlDark;
			cutting = true;
		}

		/// <summary>
		/// Copies a Property Node.
		/// </summary>
		/// <param name="PropertyNode">The NAnt Property Node to Copy</param>
		internal void Copy(NAntPropertyNode PropertyNode)
		{
			if (cutting)
			{
				baseNode.ForeColor = SystemColors.WindowText;
			}

			baseNode = PropertyNode;
			contents = ClipboardContents.PROPERTY;
			cutting = false;
		}

		/// <summary>
		/// Copies a Target Node.
		/// </summary>
		/// <param name="TargetNode">The NAnt Target Node to Copy</param>
		internal void Copy(NAntTargetNode TargetNode)
		{
			if (cutting)
			{
				baseNode.ForeColor = SystemColors.WindowText;
			}

			baseNode = TargetNode;
			contents = ClipboardContents.TARGET;
			cutting = false;
		}

		/// <summary>
		/// Copies a Task Node.
		/// </summary>
		/// <param name="TaskNode">The NAnt Task Node to Copy</param>
		internal void Copy(NAntTaskNode TaskNode)
		{
			if (cutting)
			{
				baseNode.ForeColor = SystemColors.WindowText;
			}

			baseNode = TaskNode;
			contents = ClipboardContents.TASK;
			cutting = false;
		}

		/// <summary>
		/// Pastes the current clipboard item onto a Project.
		/// </summary>
		/// <param name="ProjectNode">The Project to Paste onto.</param>
		/// <param name="TreeView">The TreeView being Pasted to.</param>
		internal void Paste(NAntProjectNode ProjectNode, TreeView TreeView)
		{
			bool cutSucceeded = false;

			if (Contents == ClipboardContents.PROPERTY)
			{
				NAntPropertyNode propertyNode = (NAntPropertyNode)baseNode;

				//
				// Create the Property XML Element
				//
				XmlElement propElement = 
					ProjectNode.ProjectDocument.CreateElement("property");

				propElement.SetAttribute("name", propertyNode.Name);
				propElement.SetAttribute("value", propertyNode.Value);

				bool result = AddPropertyToParent(
					propElement, ProjectNode, 
					ProjectNode.ProjectDocument.DocumentElement);

				if (result && Cutting)
				{
					propertyNode.PropertyElement.ParentNode.RemoveChild(propertyNode.PropertyElement);
					propertyNode.ProjectNode.Save();
					propertyNode.Remove();

					cutSucceeded = true;
				}
			}
			else if (Contents == ClipboardContents.TARGET)
			{
				NAntTargetNode targetNode = (NAntTargetNode)baseNode;

				//
				// Create the Target XML Element
				//
				bool result = AddTargetToProject(
					(XmlElement)targetNode.TargetElement.CloneNode(true), 
					ProjectNode);

				if (result && Cutting)
				{
					targetNode.TargetElement.ParentNode.RemoveChild(targetNode.TargetElement);
					targetNode.ProjectNode.Save();
					targetNode.Remove();

					cutSucceeded = true;
				}
			}

			if (cutting)
			{
				baseNode.ForeColor = SystemColors.WindowText;
			}

			if (cutSucceeded)
			{
				cutting = false;
			}
		}

		/// <summary>
		/// Pastes the current clipboard item onto a Target.
		/// </summary>
		/// <param name="TargetNode">The Target to Paste onto.</param>
		/// <param name="TreeView">The TreeView being Pasted to.</param>
		internal void Paste(NAntTargetNode TargetNode, TreeView TreeView)
		{
			bool cutSucceeded = false;

			if (Contents == ClipboardContents.PROPERTY)
			{
				NAntPropertyNode propertyNode = (NAntPropertyNode)baseNode;

				//
				// Create the Property XML Element
				//
				XmlElement propElement = 
					TargetNode.TargetElement.OwnerDocument.CreateElement("property");

				propElement.SetAttribute("name", propertyNode.Name);
				propElement.SetAttribute("value", propertyNode.Value);

				bool result = AddPropertyToParent(
					propElement, TargetNode, 
					TargetNode.TargetElement);

				if (result && Cutting)
				{
					propertyNode.PropertyElement.ParentNode.RemoveChild(propertyNode.PropertyElement);
					propertyNode.ProjectNode.Save();
					propertyNode.Remove();

					cutSucceeded = true;
				}
			}
			else if (Contents == ClipboardContents.TASK)
			{
				NAntTaskNode taskNode = (NAntTaskNode)baseNode;

				//
				// Create the Target XML Element
				//
				bool result = AddTaskToTarget(taskNode.TaskElement, TargetNode, true);

				if (result && Cutting)
				{
					taskNode.TaskElement.ParentNode.RemoveChild(taskNode.TaskElement);
					taskNode.ProjectNode.Save();
					taskNode.Remove();

					cutSucceeded = true;
				}
			}

			if (cutting)
			{
				baseNode.ForeColor = SystemColors.WindowText;
			}

			if (cutSucceeded)
			{
				cutting = false;
			}
		}

		/// <summary>
		/// Returns true if the current item on 
		/// the clipboard is a Cut operation.
		/// </summary>
		public bool Cutting
		{
			get
			{
				return cutting;
			}
		}

		/// <summary>
		/// Returns the type of contents on the clipboard.
		/// </summary>
		public ClipboardContents Contents
		{
			get
			{
				return contents;
			}
		}

		/// <summary>
		/// Adds a Task to a Target.
		/// </summary>
		/// <param name="TaskElement">The Task's XML Element</param>
		/// <param name="TargetNode">The Target's Node</param>
		/// <param name="Pasting">If this is a Paste Operation.</param>
		/// <returns></returns>
		public bool AddTaskToTarget(XmlElement TaskElement, NAntTargetNode TargetNode, bool Pasting)
		{
			XmlNodeList taskNodes = TargetNode.TargetElement.SelectNodes("task");

			XmlElement taskElem = null;
			XmlElement parentElement = null;

			// Insert the Task XML Element

			if (TargetNode.TargetElement.OwnerDocument != TaskElement.OwnerDocument)
			{
				XmlElement taskElemCopy = (XmlElement)TargetNode.TargetElement.OwnerDocument.ImportNode(TaskElement, true);
				parentElement = TargetNode.TargetElement;
				taskElem = (XmlElement)parentElement.AppendChild(taskElemCopy);

				TargetNode.ProjectNode.Save();
			}
			else
			{
				parentElement = TargetNode.TargetElement;
				if (Pasting)
				{
					taskElem = (XmlElement)parentElement.AppendChild(
						(XmlElement)TaskElement.CloneNode(true));
				}
				else
				{
					taskElem = (XmlElement)parentElement.AppendChild(TaskElement);
				}
				TargetNode.ProjectNode.Save();
			}

			bool expand = false;

			//
			// Check if the Target Node has been expanded
			//
			for (int x = 0; x < TargetNode.Nodes.Count; x++)
			{
				if (TargetNode.Nodes[x].Text.Equals(ScriptExplorerControl.DUMMY_NODE))
				{
					expand = true;
					break;
				}
			}

			if (!expand)
			{
				// Insert the Task TreeNode
				NAntTaskNode taskNode = tasksTable.CreateNodeForTask(taskElem, parentElement);
				TargetNode.Nodes.Add(taskNode);
				taskNode.NodeFont = new Font(taskNode.TreeView.Font, FontStyle.Regular);
			}

			return true;
		}

		/// <summary>
		/// Adds a Target to a Project.
		/// </summary>
		/// <param name="TargetElement">The Target's XML Element</param>
		/// <param name="ProjectNode">The Project's Node</param>
		/// <returns>True if the operation was successful.</returns>
		public bool AddTargetToProject(XmlElement TargetElement, NAntProjectNode ProjectNode)
		{
			XmlNodeList targetNodes = ProjectNode.ProjectDocument.DocumentElement.SelectNodes("target");
	
			XmlElement parentElement = null;

			//
			// Verify that the Target doesnt Exist
			//
			for (int x = 0; x < targetNodes.Count; x++)
			{
				XmlElement curTarget = (XmlElement)targetNodes[x];
				if (curTarget.LocalName == "target")
				{
					if (curTarget.GetAttribute("name") == 
						TargetElement.GetAttribute("name"))
					{
						MessageBox.Show(
							"Target \"" + TargetElement.GetAttribute("name") + 
							"\" already exists.",	"Target Exists", 
							MessageBoxButtons.OK, MessageBoxIcon.Stop);
						return false;
					}
				}
			}

			// Insert the Target XML Element
			XmlElement targetElem = null;

			if (ProjectNode.ProjectDocument != TargetElement.OwnerDocument)
			{
				XmlElement targetElemCopy = (XmlElement)ProjectNode.ProjectDocument.ImportNode(TargetElement, true);
				parentElement = ProjectNode.ProjectDocument.DocumentElement;
				targetElem = (XmlElement)parentElement.AppendChild(targetElemCopy);

				ProjectNode.Save();
			}
			else
			{
				parentElement = ProjectNode.ProjectDocument.DocumentElement;
				targetElem = (XmlElement)parentElement.AppendChild(TargetElement);

				ProjectNode.Save();
			}

			bool expand = false;

			//
			// Check if the Parent Node has been expanded
			//
			for (int x = 0; x < ProjectNode.Nodes.Count; x++)
			{
				if (ProjectNode.Nodes[x].Text.Equals(ScriptExplorerControl.DUMMY_NODE))
				{
					expand = true;
					break;
				}
			}

			if (!expand)
			{
				// Insert the Property TreeNode
				NAntTargetNode targetNode = new NAntTargetNode(targetElem, parentElement);
				targetNode.NodeFont = new System.Drawing.Font(
					ProjectNode.TreeView.Font, System.Drawing.FontStyle.Regular);
				ProjectNode.Nodes.Add(targetNode);
			}

			ProjectNode.ReloadName();

			return true;
		}

		/// <summary>
		/// Adds a Property to a Target or Project.
		/// </summary>
		public bool AddPropertyToParent(XmlElement PropertyElement, NAntBaseNode ParentNode, XmlElement ParentElement)
		{
			XmlNodeList propertyNodes = ParentElement.SelectNodes("property");
			for (int x = 0; x < propertyNodes.Count; x++)
			{
				XmlElement propertyElem = (XmlElement)propertyNodes[x];
				if (propertyElem.LocalName == "property")
				{
					if (propertyElem.GetAttribute("name") == PropertyElement.GetAttribute("name"))
					{
						MessageBox.Show(
							"Property \"" +	PropertyElement.GetAttribute("name") + 
							"\" already exists.",	"Property Exists", 
							MessageBoxButtons.OK, MessageBoxIcon.Stop);
						return false;
					}
				}
			}

			bool hadPropertyNodes = false;

			if (propertyNodes.Count > 0)
			{
				hadPropertyNodes = true;

				// Insert the Property XML Element after the last Property Element
				ParentElement.InsertAfter(PropertyElement, 
					propertyNodes[propertyNodes.Count-1]);

				ParentNode.ProjectNode.Save();
			}
			else
			{
				if (ParentElement.FirstChild != null)
				{
					// Insert the Property XML Element as the first child
					ParentElement.InsertBefore(PropertyElement, 
						ParentElement.FirstChild);

					ParentNode.ProjectNode.Save();
				}
				else
				{
					// Insert the Property XML Element
					ParentElement.AppendChild(PropertyElement);

					ParentNode.ProjectNode.Save();
				}
			}

			bool expand = false;

			//
			// Check if the Parent Node has been expanded
			//
			for (int x = 0; x < ParentNode.Nodes.Count; x++)
			{
				if (ParentNode.Nodes[x].Text.Equals(ScriptExplorerControl.DUMMY_NODE))
				{
					expand = true;
					break;
				}
			}

			if (!expand)
			{
				if (hadPropertyNodes)
				{
					//
					// Insert the Property TreeNode after the last Property TreeNode
					//

					int lastPropertyNodeIndex = 0;
					for (int i = 0; i < ParentNode.Nodes.Count; i++)
					{
						if (ParentNode.Nodes[i] is NAntPropertyNode)
						{
							lastPropertyNodeIndex = i;
						}
					}

					NAntPropertyNode propertyNode = new NAntPropertyNode(PropertyElement, ParentElement);
					ParentNode.Nodes.Insert(lastPropertyNodeIndex + 1, propertyNode);
					propertyNode.NodeFont = new Font(propertyNode.TreeView.Font, FontStyle.Regular);
				}
				else
				{
					if (ParentNode.Nodes.Count > 0)
					{
						// Insert the Property TreeNode as the first child
						NAntPropertyNode propertyNode = new NAntPropertyNode(PropertyElement, ParentElement);
						ParentNode.Nodes.Insert(0, propertyNode);
						propertyNode.NodeFont = new Font(propertyNode.TreeView.Font, FontStyle.Regular);
					}
					else
					{
						// Insert the Property TreeNode
						NAntBaseNode baseNode = (NAntBaseNode)ParentNode;
						NAntPropertyNode propertyNode = new NAntPropertyNode(PropertyElement, ParentElement);
						ParentNode.Nodes.Add(propertyNode);
						propertyNode.NodeFont = new Font(propertyNode.TreeView.Font, FontStyle.Regular);
					}
				}
			}

			return true;
		}
	}

	/// <summary>
	/// Types of contents that can be on the clipboard.
	/// </summary>
	internal enum ClipboardContents
	{
		EMPTY, PROPERTY, TARGET, TASK
	}
}