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

namespace NAnt.Contrib.NAntAddin.Nodes {
    /// <summary>
    /// Tree Node that represents an NAnt ndoc task.
    /// </summary>
    /// <remarks>None.</remarks>
    [NAntTask("ndoc", "Build NDoc Documentation", "ndoctask.bmp")]
    public class NAntNDocTaskNode : NAntTaskNode {
        /// <summary>
        /// Creates a new <see cref="NAntNDocTaskNode"/>.
        /// </summary>
        /// <param name="TaskElement">The task's XML element.</param>
        /// <param name="ParentElement">The parent XML element of the task.</param>
        /// <remarks>None.</remarks>
        public NAntNDocTaskNode(XmlElement TaskElement, XmlElement ParentElement) 
            : base(TaskElement, ParentElement) {
        }

        /// <summary>
        /// Gets or sets the assembly files to document.
        /// </summary>
        /// <value>The assembly files to document.</value>
        /// <remarks>None.</remarks>
        [Description("The assembly files to document."),Category("Data")]
        public Assemblies Assemblies {
            get {
                Assemblies assemblies = new Assemblies(TaskElement, this);
                if (Parent == null) {
                    return (Assemblies)NAntReadOnlyNodeBuilder.GetReadOnlyNode(assemblies);
                }
                return assemblies;
            }

            set {
                value.AppendToTask(TaskElement, "assemblies");
                Save();
            }
        }

        /// <summary>
        /// Gets or sets the namespace summary files describing what namespaces to document.
        /// </summary>
        /// <value>The namespace summary files describing what namespaces to document.</value>
        /// <remarks>None.</remarks>
        [Description("The namespace summary files describing what namespaces to document."),Category("Data")]
        public Summaries Summaries {
            get {
                Summaries summaries = new Summaries(TaskElement, this);
                if (Parent == null) {
                    return (Summaries)NAntReadOnlyNodeBuilder.GetReadOnlyNode(summaries);
                }
                return summaries;
            }
            set {
                value.AppendToTask(TaskElement, "summaries");
                Save();
            }
        }

        /// <summary>
        /// Gets or sets the documenters configured to style the output and behavior of documents.
        /// </summary>
        /// <value>The documenters configured to style the output and behavior of documents.</value>
        /// <remarks>None.</remarks>
        [Description("The documenters configured to style the output and behavior of documents."),Category("Data")]
        public NDocDocumenter[] Documenters {
            get {
                XmlNodeList documenterElems = TaskElement.SelectNodes("documenters/documenter");
                NDocDocumenter[] documenters = new NDocDocumenter[documenterElems.Count];
                NDocDocumenter[] readOnlyDocumenters = new NDocDocumenter[documenterElems.Count];

                for (int i = 0; i < documenterElems.Count; i++) {
                    XmlElement documenterElem = (XmlElement)documenterElems.Item(i);
                    documenters[i] = new NDocDocumenter(this, TaskElement, documenterElem);

                    if (Parent == null) {
                        readOnlyDocumenters[i] = (NDocDocumenter)NAntReadOnlyNodeBuilder.GetReadOnlyNode(
                            documenters[i]);
                    }
                }

                if (Parent == null) {
                    return readOnlyDocumenters;
                }
                return documenters;
            }
            set {
                if (value != null) {
                    XmlElement documentersElem = (XmlElement)TaskElement.SelectSingleNode("documenters");
                    if (documentersElem == null) {
                        documentersElem = TaskElement.OwnerDocument.CreateElement("documenters");
                        TaskElement.AppendChild(documentersElem);
                    }

                    if (documentersElem != null) {
                        XmlNodeList documenters = documentersElem.SelectNodes("documenter");
                        if (documenters != null) {
                            foreach (XmlNode documenter in documenters) {
                                if (documenter.ParentNode == documentersElem) {
                                    documentersElem.RemoveChild(documenter);
                                }
                            }
                        }
                    }

                    for (int i = 0; i < value.Length; i++) {
                        value[i].AppendToParent(documentersElem);
                    }
                }
                Save();
            }
        }
    }

    /// <summary>
    /// An <see cref="NAntFileSet"/> that specifies Microsoft.NET assemblies.
    /// </summary>
    /// <remarks>None.</remarks>
    public class Assemblies : NAntFileSet {
        /// <summary>
        /// Creates a new <see cref="Assemblies"/>.
        /// </summary>
        /// <param name="TaskElement">The assemblies XML element.</param>
        /// <param name="TaskNode">The <see cref="NAntTaskNode"/> for which this <see cref="Assemblies"/> is a property.</param>
        /// <remarks>None.</remarks>
        public Assemblies(XmlElement TaskElement, NAntTaskNode TaskNode) : base(TaskNode, TaskElement, "assemblies") {
        }
    }

    /// <summary>
    /// An <see cref="NAntFileSet"/> that specifies NDoc Summaries.
    /// </summary>
    /// <remarks>None.</remarks>
    public class Summaries : NAntFileSet {
        /// <summary>
        /// Creates a new <see cref="Summaries"/>.
        /// </summary>
        /// <param name="TaskElement">The summaries XML element.</param>
        /// <param name="TaskNode">The <see cref="NAntTaskNode"/> for which this <see cref="Summaries"/> is a property.</param>
        /// <remarks>None.</remarks>
        public Summaries(XmlElement TaskElement, NAntTaskNode TaskNode) : base(TaskNode, TaskElement, "summaries") {
        }
    }

    /// <summary>
    /// Specifies an NDoc documenter.
    /// </summary>
    /// <remarks>None.</remarks>
    public class NDocDocumenter : Component, ConstructorArgsResolver {
        private string name;
        private NAntNDocTaskNode taskNode;
        private XmlElement taskElement;
        private XmlElement documenterElement;
        private NDocProperty[] properties = new NDocProperty[0];

        /// <summary>
        /// Creates a new <see cref="NDocDocumenter"/>.
        /// </summary>
        /// <remarks>None.</remarks>
        public NDocDocumenter() {
        }

        /// <summary>
        /// Creates a new <see cref="NDocDocumenter"/>.
        /// </summary>
        /// <param name="NDocNode"></param>
        /// <param name="TaskElement"></param>
        /// <param name="DocumenterElement"></param>
        /// <remarks>None.</remarks>
        public NDocDocumenter(NAntNDocTaskNode NDocNode, XmlElement TaskElement, XmlElement DocumenterElement) {
            taskNode = NDocNode;
            taskElement = TaskElement;
            documenterElement = DocumenterElement;
        }

        /// <summary>
        /// Gets or sets the name of the documenter.
        /// </summary>
        /// <value>The name of the documenter.</value>
        /// <remarks>None.</remarks>
        [Description("The name of the documenter."),Category("Appearance")]
        public string Name {
            get {
                if (documenterElement != null) {
                    return documenterElement.GetAttribute("name");
                }
                return name;
            }

            set {
                if (documenterElement == null) {
                    name = value;
                }
                else {
                    if (value == "") {
                        documenterElement.RemoveAttribute("name");
                    }
                    else {
                        documenterElement.SetAttribute("name", value);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the properties of the documenter.
        /// </summary>
        /// <value>The values of the documenter.</value>
        /// <remarks>None.</remarks>
        [Editor("System.ComponentModel.Design.ArrayEditor, System.Design", 
             typeof(System.Drawing.Design.UITypeEditor))]
        [Description("Properties passed to the documenter.")]
        public NDocProperty[] Properties {
            get {
                if (documenterElement == null) {
                    return properties;
                }

                XmlNodeList propertyElems = documenterElement.GetElementsByTagName("property");
                NDocProperty[] props = new NDocProperty[propertyElems.Count];
                NDocProperty[] readOnlyProperties = new NDocProperty[propertyElems.Count];

                for (int i = 0; i < propertyElems.Count; i++) {
                    XmlElement propertyElem = (XmlElement)propertyElems.Item(i);
                    props[i] = new NDocProperty(propertyElem);

                    if (taskNode.Parent == null) {
                        // If read only, create a proxy node
                        readOnlyProperties[i] = (NDocProperty)NAntReadOnlyNodeBuilder.GetReadOnlyNode(
                            props[i]);
                    }
                }
                if (taskNode.Parent == null) {
                    return readOnlyProperties;
                }
                return props;
            }

            set {
                if (documenterElement == null) {
                    properties = value;
                }

                if (documenterElement != null) {
                    // Remove old properties
                    XmlNodeList props = documenterElement.SelectNodes("property");
                    if (props != null) {
                        foreach (XmlNode property in props) {
                            documenterElement.RemoveChild(property);
                        }
                    }
                }

                if (value != null) {
                    if (documenterElement == null) {
                        properties = new NDocProperty[value.Length];
                    }

                    for (int i = 0; i < value.Length; i++) {
                        if (documenterElement != null) {
                            // Append new properties
                            value[i].AppendToParent(documenterElement);
                        }
                        else {
                            properties[i] = value[i];
                        }
                    }
                }

                if (taskElement != null) {
                    taskNode.Save();
                }
            }
        }

        /// <summary>
        /// Gets the Documenter's XML element.
        /// </summary>
        /// <value>The Documenter's XML element.</value>
        /// <remarks>None.</remarks>
        [Browsable(false)]
        public XmlElement DocumenterElement {
            get {
                return documenterElement;
            }
        }

        /// <summary>
        /// Appends the current Documenter Element to the documenters element.
        /// </summary>
        /// <param name="ParentElement">The documenters Xml Element to append to.</param>
        /// <remarks>None.</remarks>
        public void AppendToParent(XmlElement ParentElement) {
            if (documenterElement == null) {
                CreateElement(ParentElement.OwnerDocument);
            }
            ParentElement.AppendChild(DocumenterElement);
        }

        /// <summary>
        /// Creates the documenter XML element.
        /// </summary>
        /// <param name="Document">The XML Document to use.</param>
        /// <remarks>None.</remarks>
        public void CreateElement(XmlDocument Document) {
            documenterElement = Document.CreateElement("documenter");
            documenterElement.SetAttribute("name", name);

            for (int i = 0; i < properties.Length; i++) {
                properties[i].AppendToParent(documenterElement);
            }
        }

        /// <summary>
        /// Returns the arguments that must be passed to 
        /// the constructor of an object to create the 
        /// same object.
        /// </summary>
        /// <returns>The arguments to pass.</returns>
        /// <remarks>None.</remarks>
        public Object[] GetConstructorArgs() {
            return new object[] {
                taskNode,
                taskElement,
                documenterElement
            };
        }
    }

    /// <summary>
    /// Specifies a property of an NDoc <see cref="NDocDocumenter"/>.
    /// </summary>
    /// <remarks>None.</remarks>
    public class NDocProperty : Component, ConstructorArgsResolver {
        private string name, sValue;
        private XmlElement propertyElement;

        /// <summary>
        /// Creates a new <see cref="NDocProperty"/>.
        /// </summary>
        /// <remarks>None.</remarks>
        public NDocProperty() {
        }

        /// <summary>
        /// Creates a new <see cref="NDocProperty"/>.
        /// </summary>
        /// <param name="PropertyElement">The property XML element.</param>
        /// <remarks>None.</remarks>
        public NDocProperty(XmlElement PropertyElement) {
            propertyElement = PropertyElement;
        }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
        /// <remarks>None.</remarks>
        [Description("The name of the property.")]
        public string Name {
            get {
                if (propertyElement != null) {
                    return propertyElement.GetAttribute("name");
                }
                return name;
            }

            set {
                if (propertyElement == null) {
                    name = value;
                }
                else {
                    if (value == "") {
                        propertyElement.RemoveAttribute("name");
                    }
                    else {
                        propertyElement.SetAttribute("name", value);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        /// <value>The value of the property.</value>
        /// <remarks>None.</remarks>
        [Description("The value of the property.")]
        public string Value {
            get {
                if (propertyElement != null) {
                    return propertyElement.GetAttribute("value");
                }
                return sValue;
            }

            set {
                if (propertyElement == null) {
                    sValue = value;
                }
                else {
                    if (value == "") {
                        propertyElement.RemoveAttribute("value");
                    }
                    else {
                        propertyElement.SetAttribute("value", value);
                    }
                }
            }
        }

        /// <summary>
        /// Appends the current Property Element to the documenter element.
        /// </summary>
        /// <param name="ParentElement">The parent XML Element to append to.</param>
        /// <remarks>None.</remarks>
        public void AppendToParent(XmlElement ParentElement) {
            if (propertyElement == null) {
                CreateElement(ParentElement.OwnerDocument);
            }
            ParentElement.AppendChild(propertyElement);
        }

        /// <summary>
        /// Creates the property XML element.
        /// </summary>
        /// <param name="Document">The XML Document to use.</param>
        /// <remarks>None.</remarks>
        private void CreateElement(XmlDocument Document) {
            propertyElement = Document.CreateElement("property");
            propertyElement.SetAttribute("name", name);
            propertyElement.SetAttribute("value", sValue);
        }

        /// <summary>
        /// Returns the arguments that must be passed to 
        /// the constructor of an object to create the 
        /// same object.
        /// </summary>
        /// <returns>The arguments to pass.</returns>
        /// <remarks>None.</remarks>
        public Object[] GetConstructorArgs() {
            return new object[] {
                propertyElement
            };
        }
    }
}
