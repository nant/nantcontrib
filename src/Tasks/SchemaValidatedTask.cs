//
// NAntContrib
//
// Copyright (C) 2002 Jayme C. Edwards (jedwards@wi.rr.com)
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;

using SourceForge.NAnt;
using SourceForge.NAnt.Tasks;
using SourceForge.NAnt.Attributes;

namespace NAnt.Contrib.Tasks
{
    /// <summary>
    /// Abstract Task that Validates inheriting classes 
    /// against an XML Schema of the same name.
    /// </summary>
    /// <remarks>None.</remarks>
    public abstract class SchemaValidatedTask : Task
    {
        private bool _validate = true;
        private object _schemaObject;

        /// <summary>
        /// Returns or sets whether to validate the 
        /// Task against its XML Schema.
        /// </summary>
        /// <value>Whether to validate the 
        /// Task against its XML Schema.</value>
        /// <remarks>None.</remarks>
        [TaskAttribute("validate", Required=false)]
        [BooleanValidator()]
        public bool Validate 
        {
            get
            {
                return _validate;
            }
        	
            set
            {
                _validate = value;
            }
        }

        /// <summary>
        /// Returns the object from the Schema wrapper after 
        /// <see cref="InitializeTask"/> is called.
        /// </summary>
        /// <value>The object from the Schema wrapper after 
        /// <see cref="InitializeTask"/> is called.</value>
        /// <remarks>None.</remarks>
        public Object SchemaObject
        {
            get
            {
                return _schemaObject;
            }
        }

        /// <summary>
        /// Initializes the task and verifies parameters.
        /// </summary>
        /// <param name="TaskNode">Node that contains the 
        /// XML fragment used to define this task instance.</param>
        /// <remarks>None.</remarks>
        protected override void InitializeTask(XmlNode TaskNode)
        {
            XmlNode taskNode = TaskNode.Clone();
            if (taskNode.ChildNodes != null)
            {
                ExpandPropertiesInNodes(taskNode.ChildNodes);
                if (taskNode.Attributes != null)
                {
                    foreach (XmlAttribute attr in taskNode.Attributes) 
                    {
                        attr.Value = Project.ExpandProperties(attr.Value);
                    }
                }
            }

            if (Validate)
            {
                SchemaValidatorAttribute[] taskValidators = 
                    (SchemaValidatorAttribute[])GetType().GetCustomAttributes(
                    typeof(SchemaValidatorAttribute), true);

                if (taskValidators.Length > 0)
                {
                    SchemaValidatorAttribute taskValidator = taskValidators[0];
                    XmlSerializer taskSerializer = new XmlSerializer(taskValidator.ValidatorType);

                    NameTable taskNameTable = new NameTable();
                    XmlNamespaceManager taskNSMgr = new XmlNamespaceManager(taskNameTable);
                    taskNSMgr.AddNamespace("", GetType().FullName);

                    XmlParserContext context = new XmlParserContext(
                        null, taskNSMgr, null, XmlSpace.None);

                    XmlTextReader taskSchemaReader = new XmlTextReader(
                        taskNode.OuterXml, XmlNodeType.Element, context);

                    _schemaObject = taskSerializer.Deserialize(taskSchemaReader);
                }
            }
        }

        /// <summary>
        /// Recursively expands properties of all attributes of 
        /// a nodelist and their children.
        /// </summary>
        /// <param name="Nodes">The nodes to recurse.</param>
        private void ExpandPropertiesInNodes(XmlNodeList Nodes) 
        {
            foreach (XmlNode node in Nodes)
            {
                if (node.ChildNodes != null)
                {
                    ExpandPropertiesInNodes(node.ChildNodes);
                    if (node.Attributes != null)
                    {
                        foreach (XmlAttribute attr in node.Attributes) 
                        {
                            attr.Value = Project.ExpandProperties(attr.Value);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Wraps an exception thrown by an XML Schema 
    /// being validated against a task.
    /// </summary>
    /// <remarks>None.</remarks>
    public class SchemaValidationException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="SchemaValidationException"/>.
        /// </summary>
        /// <param name="msg">String containing the error message.</param>
        /// <remarks>None.</remarks>
        public SchemaValidationException(string msg) : base (msg) {}
    }

    /// <summary>
    /// Indicates that class should be validated by an XML Schema.
    /// </summary>
    /// <remarks>None.</remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited=false, AllowMultiple=false)]
    public class SchemaValidatorAttribute : Attribute
    {
        private Type _type;

        /// <summary>
        /// Creates a new <see cref="SchemaValidatorAttribute"/>.
        /// </summary>
        /// <param name="schemaType">The <see cref="Type"/> of the object 
        /// created by the xsd NAnt task to represent the root node of 
        /// your task.</param>
        /// <remarks>None.</remarks>
        public SchemaValidatorAttribute(Type schemaType)
        { 
            _type = schemaType;
        }

        /// <summary>
        /// Returns or sets The <see cref="Type"/> of the object 
        /// created by the xsd NAnt task to represent the root node of 
        /// your task.
        /// </summary>
        /// <value>The <see cref="Type"/> of the object 
        /// created by the xsd NAnt task to represent the root node of 
        /// your task.</value>
        /// <remarks>None.</remarks>
        public Type ValidatorType
        {
            get
            {
                return _type;
            }

            set
            {
                _type = value;
            }
        }
    }
}