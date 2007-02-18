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
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Text;

using NAnt.Core;
using NAnt.Core.Tasks;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Abstract <see cref="Task" /> that validates inheriting classes against 
    /// an XML schema of the same name.
    /// </summary>
    public abstract class SchemaValidatedTask : Task {
        #region Private Instance Fields

        private object _schemaObject;
        private bool _validated = true;
        private ArrayList _validationExceptions = new ArrayList();

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// Returns the object from the Schema wrapper after 
        /// <see cref="Initialize()"/> is called.
        /// </summary>
        /// <value>The object from the Schema wrapper after <see cref="Initialize()"/> is called.</value>
        public Object SchemaObject {
            get { return _schemaObject; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        /// <summary>
        /// Initializes the task and verifies parameters.
        /// </summary>
        protected override void Initialize() {
            XmlElement taskXml = (XmlElement) XmlNode.Clone();

            // Expand all properties in the task and its child elements
            if (taskXml.ChildNodes != null) {
                ExpandPropertiesInNodes(taskXml.ChildNodes);
                if (taskXml.Attributes != null) {
                    foreach (XmlAttribute attr in taskXml.Attributes) {
                        attr.Value = Properties.ExpandProperties(attr.Value, Location);
                    }
                }
            }

            // Get the [SchemaValidator(type)] attribute
            SchemaValidatorAttribute[] taskValidators = 
                (SchemaValidatorAttribute[])GetType().GetCustomAttributes(
                typeof(SchemaValidatorAttribute), true);

            if (taskValidators.Length > 0) {
                SchemaValidatorAttribute taskValidator = taskValidators[0];
                XmlSerializer taskSerializer = new XmlSerializer(taskValidator.ValidatorType);

                // get embedded schema resource stream
                Stream schemaStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    taskValidator.ValidatorType.Namespace);

                // ensure schema resource was embedded
                if (schemaStream == null) {
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                        "Schema resource '{0}' could not be found.",
                        taskValidator.ValidatorType.Namespace), Location);
                }

                // load schema resource
                XmlTextReader tr = new XmlTextReader(
                    schemaStream, XmlNodeType.Element, null);
                
                // Add the schema to a schema collection
                XmlSchema schema = XmlSchema.Read(tr, null);
                XmlSchemaCollection schemas = new XmlSchemaCollection();
                schemas.Add(schema);

                string xmlNamespace = (taskValidator.XmlNamespace != null ? taskValidator.XmlNamespace : GetType().FullName);

                // Create a namespace manager with the schema's namespace
                NameTable nt = new NameTable();
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(nt);
                nsmgr.AddNamespace(string.Empty, xmlNamespace);

                // Create a textreader containing just the Task's Node
                XmlParserContext ctx = new XmlParserContext(
                    null, nsmgr, null, XmlSpace.None);
                taskXml.SetAttribute("xmlns", xmlNamespace);
                XmlTextReader textReader = new XmlTextReader(taskXml.OuterXml, 
                    XmlNodeType.Element, ctx);

                // Copy the node from the TextReader and indent it (for error
                // reporting, since NAnt does not retain formatting during a load)
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

                if (!_validated) {
                    // Log any validation errors that have ocurred
                    for (int i = 0; i < _validationExceptions.Count; i++) {
                        BuildException ve = (BuildException) _validationExceptions[i];
                        if (i == _validationExceptions.Count - 1) {
                            // If this is the last validation error, throw it
                            throw ve;
                        }
                        Log(Level.Info, ve.Message);
                    }
                }
            
                NameTable taskNameTable = new NameTable();
                XmlNamespaceManager taskNSMgr = new XmlNamespaceManager(taskNameTable);
                taskNSMgr.AddNamespace(string.Empty, xmlNamespace);

                XmlParserContext context = new XmlParserContext(
                    null, taskNSMgr, null, XmlSpace.None);

                XmlTextReader taskSchemaReader = new XmlTextReader(
                    taskXml.OuterXml, XmlNodeType.Element, context);

                // Deserialize from the Task's XML to the schema wrapper object
                _schemaObject = taskSerializer.Deserialize(taskSchemaReader);
            }
        }

        #endregion Override implementation of Task

        #region Private Instance Methods

        /// <summary>
        /// Occurs when a validation error is raised.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="args">Validation arguments passed in.</param>
        private void Task_OnSchemaValidate(object sender, ValidationEventArgs args) {
            _validated = false;
            _validationExceptions.Add(
                new BuildException("Validation Error: " + args.Message));
        }

        /// <summary>
        /// Recursively expands properties of all attributes of 
        /// a nodelist and their children.
        /// </summary>
        /// <param name="Nodes">The nodes to recurse.</param>
        private void ExpandPropertiesInNodes(XmlNodeList Nodes) {
            foreach (XmlNode node in Nodes) {
                if (node.ChildNodes != null) {
                    ExpandPropertiesInNodes(node.ChildNodes);
                    if (node.Attributes != null) {
                        foreach (XmlAttribute attr in node.Attributes) {
                            attr.Value = Properties.ExpandProperties(attr.Value, Location);
                        }
                    }
                }
            }
        }

        #endregion Private Instance Methods
    }

    /// <summary>
    /// Indicates that class should be validated by an XML Schema.
    /// </summary>
    /// <remarks>None.</remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited=false, AllowMultiple=false)]
    public class SchemaValidatorAttribute : Attribute {
        #region Private Instance Fields

        private Type _type;
        private string _namespace;

        #endregion Private Instance Fields

        #region Public Instance Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaValidatorAttribute"/>
        /// class.
        /// </summary>
        /// <param name="schemaType">The <see cref="Type"/> of the object created by <see cref="XsdTask" /> to represent the root node of your task.</param>
        public SchemaValidatorAttribute(Type schemaType) { 
            _type = schemaType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaValidatorAttribute"/>
        /// class.
        /// </summary>
        /// <param name="schemaType">The <see cref="Type" /> of the object created by <see cref="XsdTask" /> to represent the root node of your task.</param>
        /// <param name="xmlNamespace"></param>
        public SchemaValidatorAttribute(Type schemaType, string xmlNamespace) { 
            _type = schemaType;
            _namespace = xmlNamespace;
        }

        #endregion Public Instance Constructors

        #region Public Instance Properties

        /// <summary>
        /// Gets or sets the <see cref="Type"/> of the object created by 
        /// <see cref="XsdTask" /> to represent the root node of your task.
        /// </summary>
        /// <value>
        /// The <see cref="Type"/> of the object created by <see cref="XsdTask" />
        /// to represent the root node of your task.
        /// </value>
        public Type ValidatorType {
            get { return _type; }
            set { _type = value; }
        }

        public string XmlNamespace {
            get { return _namespace; }
            set { _namespace = value; }
        }

        #endregion Public Instance Properties
    }
}
