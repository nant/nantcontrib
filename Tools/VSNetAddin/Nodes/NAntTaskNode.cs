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
    /// Tree Node that represents an NAnt task's element.
    /// </summary>
    /// <remarks>None.</remarks>
    public class NAntTaskNode : NAntBaseNode {
        private XmlElement taskElement;
        internal XmlElement parentElement;

        /// <summary>
        /// Creates a new NAnt Task Node.
        /// </summary>
        /// <param name="TaskElement">The task's XML element.</param>
        /// <param name="ParentElement">The parent XML element of the task.</param>
        /// <remarks>None.</remarks>
        public NAntTaskNode(XmlElement TaskElement, XmlElement ParentElement) : base(ParentElement) {
            taskElement = TaskElement;
            parentElement = ParentElement;

            Text = TaskElement.LocalName;
            ImageIndex = 3;
            SelectedImageIndex = 3;
        }

        /// <summary>
        /// Saves the Project containing this Task.
        /// </summary>
        /// <remarks>None.</remarks>
        public void Save() {
            ProjectNode.Save();
        }

        /// <summary>
        /// Gets the task's XML element.
        /// </summary>
        /// <value>The task's XML element.</value>
        /// <remarks>None.</remarks>
        [Browsable(false)]
        public XmlElement TaskElement {
            get {
                return taskElement;
            }
        }

        /// <summary>
        /// Gets or sets if errors will cause the script to fail.
        /// </summary>
        /// <value>If errors will cause the script to fail.</value>
        /// <remarks>None.</remarks>
        [Description("If errors will cause the script to fail."),Category("Behavior")]
        public string FailOnError {
            get {
                return TaskElement.GetAttribute("failonerror");
            }
            set {
                if (value == "") {
                    TaskElement.RemoveAttribute("failonerror");
                } else {
                    TaskElement.SetAttribute("failonerror", value);
                }
                Save();
            }
        }

        /// <summary>
        /// Gets or sets if detailed log messages are reported.
        /// </summary>
        /// <value>If detailed log messages are reported.</value>
        /// <remarks>None.</remarks>
        [Description("If detailed log messages are reported."),Category("Behavior")]
        public string Verbose {
            get {
                return TaskElement.GetAttribute("verbose");
            }
            set {
                if (value == "") {
                    TaskElement.RemoveAttribute("verbose");
                } else {
                    TaskElement.SetAttribute("verbose", value);
                }
                Save();
            }
        }

        /// <summary>
        /// Gets or sets the condition that must evaluate true for the task to execute.
        /// </summary>
        /// <value>The condition that must evaluate true for the task to execute.</value>
        /// <remarks>None.</remarks>
        [Description("Condition that must evaluate true for the task to execute."),Category("Behavior")]
        public string If {
            get {
                return TaskElement.GetAttribute("if");
            }
            set {
                if (value == "") {
                    TaskElement.RemoveAttribute("if");
                } else {
                    TaskElement.SetAttribute("if", value);
                }
                Save();
            }
        }

        /// <summary>
        /// Gets or sets the condition that must evaluate false for the task to execute.
        /// </summary>
        /// <value>The condition that must evaluate false for the task to execute.</value>
        /// <remarks>None.</remarks>
        [Description("Condition that must evaluate false for the task to execute."),Category("Behavior")]
        public string Unless {
            get {
                return TaskElement.GetAttribute("unless");
            }
            set {
                if (value == "") {
                    TaskElement.RemoveAttribute("unless");
                } else {
                    TaskElement.SetAttribute("unless", value);
                }
                Save();
            }
        }
    }
}
