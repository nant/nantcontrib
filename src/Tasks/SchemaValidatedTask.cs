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
using System.Resources;
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
        private object _schemaObject;
        private bool _validated = true;
        private ArrayList _validationExceptions = new ArrayList();
        protected XmlNode _xmlNode;

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
        /// Occurs when a validation error is raised.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="args">Validation arguments passed in.</param>
        private void Task_OnSchemaValidate(object sender, ValidationEventArgs args)
        {
            _validated = false;
            _validationExceptions.Add(
                new BuildException("Validation Error: " + args.Message));
        }

        /// <summary>
        /// Initializes the task and verifies parameters.
        /// </summary>
        /// <param name="TaskNode">Node that contains the 
        /// XML fragment used to define this task instance.</param>
        /// <remarks>None.</remarks>
        protected override void InitializeTask(XmlNode TaskNode)
        {
            _xmlNode = TaskNode;
            XmlNode taskNode = TaskNode.Clone();

            // Expand all properties in the task and its child elements
            if (taskNode.ChildNodes != null)
            {
                ExpandPropertiesInNodes(taskNode.ChildNodes);
                if (taskNode.Attributes != null)
                {
                    foreach (XmlAttribute attr in taskNode.Attributes) 
                    {
                        attr.Value = Properties.ExpandProperties(attr.Value, Location);
                    }
                }
            }

            // Get the [SchemaValidator(type)] attribute
            SchemaValidatorAttribute[] taskValidators = 
                (SchemaValidatorAttribute[])GetType().GetCustomAttributes(
                typeof(SchemaValidatorAttribute), true);

            if (taskValidators.Length > 0)
            {
                SchemaValidatorAttribute taskValidator = taskValidators[0];
                XmlSerializer taskSerializer = new XmlSerializer(taskValidator.ValidatorType);

                // Load the embedded schema resource
                ResourceManager resMgr = new ResourceManager(
                    taskValidator.ValidatorType.Namespace,
                    Assembly.GetExecutingAssembly());

                // Get the "schema" named resource string
                string schemaXml = resMgr.GetString("schema");
                XmlTextReader tr = new XmlTextReader(
                    schemaXml, XmlNodeType.Element, null);
                
                // Add the schema to a schema collection
                XmlSchema schema = XmlSchema.Read(tr, null);
                XmlSchemaCollection schemas = new XmlSchemaCollection();
                schemas.Add(schema);

                // Create a namespace manager with the schema's namespace
                NameTable nt = new NameTable();
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(nt);
                nsmgr.AddNamespace(String.Empty, GetType().FullName);

                // Create a textreader containing just the Task's Node
                XmlParserContext ctx = new XmlParserContext(
                    null, nsmgr, null, XmlSpace.None);
                ((XmlElement)TaskNode).SetAttribute("xmlns", GetType().FullName);
                XmlTextReader textReader = new XmlTextReader(
                    ((XmlElement)TaskNode).OuterXml, XmlNodeType.Element, ctx);

                // Copy the node from the TextReader and indent it (for error
                // reporting, since NAnt does not retain formatting during a load).
                StringWriter stringWriter = new StringWriter();
                XmlTextWriter textWriter = new XmlTextWriter(stringWriter);
                textWriter.Formatting = Formatting.Indented;
                
                textWriter.WriteNode(textReader, true);

                //textWriter.Close();
                XmlTextReader formattedTextReader = new XmlTextReader(
                    stringWriter.ToString(), XmlNodeType.Document, ctx);

                // Validate the Task's XML against its schema
                XmlValidatingReader validatingReader = new XmlValidatingReader(
                    formattedTextReader);
                validatingReader.ValidationType = ValidationType.Schema;
                validatingReader.Schemas.Add(schemas);
                validatingReader.ValidationEventHandler += 
                    new ValidationEventHandler(Task_OnSchemaValidate);
                
                while (validatingReader.Read()) {
                    // Read strictly for validation purposes
                }
                validatingReader.Close();

                if (!_validated)
                {
                    // Log any validation errors that have ocurred
                    for (int i = 0; i < _validationExceptions.Count; i++)
                    {
                        BuildException ve = (BuildException)_validationExceptions[i];
                        if (i == _validationExceptions.Count - 1)
                        {
                            // If this is the last validation error, throw it
                            throw ve;
                        }
                        Log.WriteLine(LogPrefix + ve.Message);
                    }
                }
            
            

                NameTable taskNameTable = new NameTable();
                XmlNamespaceManager taskNSMgr = new XmlNamespaceManager(taskNameTable);
                taskNSMgr.AddNamespace("", GetType().FullName);

                XmlParserContext context = new XmlParserContext(
                    null, taskNSMgr, null, XmlSpace.None);

                XmlTextReader taskSchemaReader = new XmlTextReader(
                    taskNode.OuterXml, XmlNodeType.Element, context);

                // Deserialize from the Task's XML to the schema wrapper object
                _schemaObject = taskSerializer.Deserialize(taskSchemaReader);
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
                            attr.Value = Properties.ExpandProperties(attr.Value, Location);
                        }
                    }
                }
            }
        }
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