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
    /// Tree Node that represents an NAnt tstamp task.
    /// </summary>
    /// <remarks>None.</remarks>
    [NAntTask("tstamp", "Set Properties with the Current Date and Time", "tstamptask.bmp")]
    public class NAntTimeStampTaskNode : NAntTaskNode {
        /// <summary>
        /// Creates a new <see cref="NAntTimeStampTaskNode"/>.
        /// </summary>
        /// <param name="TaskElement">The task's XML element.</param>
        /// <param name="ParentElement">The parent XML element of the task.</param>
        /// <remarks>None.</remarks>
        public NAntTimeStampTaskNode(XmlElement TaskElement, XmlElement ParentElement) 
            : base(TaskElement, ParentElement) {
        }

        /// <summary>
        /// Gets or sets the property to receive the date/time string in the given pattern.
        /// </summary>
        /// <value>The property to receive the date/time string in the given pattern.</value>
        /// <remarks>None.</remarks>
        [Description("Property to receive the date/time string in the given pattern."),Category("Data")]
        public string Property {
            get {
                return TaskElement.GetAttribute("property");
            }
            set {
                if (value == "") {
                    TaskElement.RemoveAttribute("property");
                } else {
                    TaskElement.SetAttribute("property", value);
                }
                Save();
            }
        }

        /// <summary>
        /// Gets or sets the date/time pattern to be used.
        /// </summary>
        /// <value>The date/time pattern to be used.</value>
        /// <remarks>None.</remarks>
        [Description("Date/time pattern to be used."),Category("Data")]
        public string Pattern {
            get {
                return TaskElement.GetAttribute("pattern");
            }
            set {
                if (value == "") {
                    TaskElement.RemoveAttribute("pattern");
                } else {
                    TaskElement.SetAttribute("pattern", value);
                }
                Save();
            }
        }

        /// <summary>
        /// Gets or sets the formatters to apply.
        /// </summary>
        /// <value>The formatters to apply.</value>
        /// <remarks>None.</remarks>
        [Description("Formatters to apply."),Category("Data")]
        public TimeStampFormatter[] Formatters {
            get {
                XmlNodeList formatterElems = TaskElement.GetElementsByTagName("formatter");
                TimeStampFormatter[] formatters = new TimeStampFormatter[formatterElems.Count];
                TimeStampFormatter[] readOnlyFormatters = new TimeStampFormatter[formatterElems.Count];

                for (int i = 0; i < formatterElems.Count; i++) {
                    XmlElement formatterElem = (XmlElement)formatterElems.Item(i);
                    formatters[i] = new TimeStampFormatter(formatterElem);

                    if (Parent == null) {
                        // If read only, create a proxy node
                        readOnlyFormatters[i] = (TimeStampFormatter)NAntReadOnlyNodeBuilder.GetReadOnlyNode(
                            formatters[i]);
                    }
                }
                if (Parent == null) {
                    return readOnlyFormatters;
                }
                return formatters;
            }
            set {
                // Remove old formatters
                XmlNodeList formatterElems = TaskElement.SelectNodes("formatter");
                if (formatterElems != null) {
                    foreach (XmlNode formatter in formatterElems) {
                        TaskElement.RemoveChild(formatter);
                    }
                }

                if (value != null) {
                    for (int i = 0; i < value.Length; i++) {
                        // Append new formatters
                        value[i].AppendToParent(TaskElement);
                    }
                }
                Save();
            }
        }
    }

    /// <summary>
    /// Specifies a TimeStamp Formatter.
    /// </summary>
    /// <remarks>None.</remarks>
    public class TimeStampFormatter : Component, ConstructorArgsResolver {
        private string property, pattern;
        private XmlElement formatterElement;

        /// <summary>
        /// Creates a new <see cref="TimeStampFormatter"/>.
        /// </summary>
        /// <remarks>None.</remarks>
        public TimeStampFormatter() {
        }

        /// <summary>
        /// Creates a new <see cref="TimeStampFormatter"/>.
        /// </summary>
        /// <param name="FormatterElement">The formatter XML element.</param>
        /// <remarks>None.</remarks>
        public TimeStampFormatter(XmlElement FormatterElement) {
            formatterElement = FormatterElement;
        }

        /// <summary>
        /// Gets or sets the property to receive the date/time string in the given pattern.
        /// </summary>
        /// <value>The property to receive the date/time string in the given pattern.</value>
        /// <remarks>None.</remarks>
        [Description("Property to receive the date/time string in the given pattern."),Category("Data")]
        public string Property {
            get {
                if (formatterElement != null) {
                    return formatterElement.GetAttribute("property");
                }
                return property;
            }
            set {
                if (formatterElement == null) {
                    property = value;
                } else {
                    if (value == "") {
                        formatterElement.RemoveAttribute("property");
                    } else {
                        formatterElement.SetAttribute("property", value);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the date/time pattern to be used.
        /// </summary>
        /// <value>The date/time pattern to be used.</value>
        /// <remarks>None.</remarks>
        [Description("Date/time pattern to be used."),Category("Data")]
        public string Pattern {
            get {
                if (formatterElement != null) {
                    return formatterElement.GetAttribute("pattern");
                }
                return pattern;
            }
            set {
                if (formatterElement == null) {
                    pattern = value;
                } else {
                    if (value == "") {
                        formatterElement.RemoveAttribute("pattern");
                    } else {
                        formatterElement.SetAttribute("pattern", value);
                    }
                }
            }
        }

        /// <summary>
        /// Appends the current Formatter Element to the task element.
        /// </summary>
        /// <param name="ParentElement">The parent XML Element to append to.</param>
        /// <remarks>None.</remarks>
        public void AppendToParent(XmlElement ParentElement) {
            if (formatterElement == null) {
                CreateElement(ParentElement.OwnerDocument);
            }
            ParentElement.AppendChild(formatterElement);
        }

        /// <summary>
        /// Creates the formatter XML element.
        /// </summary>
        /// <param name="Document">The XML Document to use.</param>
        /// <remarks>None.</remarks>
        private void CreateElement(XmlDocument Document) {
            formatterElement = Document.CreateElement("formatter");
            formatterElement.SetAttribute("property", property);
            formatterElement.SetAttribute("pattern", pattern);
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
                formatterElement
            };
        }
    }
}
