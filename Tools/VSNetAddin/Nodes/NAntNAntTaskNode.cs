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

namespace NAnt.Contrib.NAntAddin.Nodes {
    /// <summary>
    /// Tree Node that represents an NAnt nant task.
    /// </summary>
    /// <remarks>None.</remarks>
    [NAntTask("nant", "Run an External .build File")]
    public class NAntNAntTaskNode : NAntTaskNode {
        /// <summary>
        /// Creates a new <see cref="NAntNAntTaskNode"/>.
        /// </summary>
        /// <param name="TaskElement">The task's XML element.</param>
        /// <param name="ParentElement">The parent XML element of the task.</param>
        /// <remarks>None.</remarks>
        public NAntNAntTaskNode(XmlElement TaskElement, XmlElement ParentElement) 
            : base(TaskElement, ParentElement) {
        }

        /// <summary>
        /// Gets or sets the NAnt .build file to execute.
        /// </summary>
        /// <value>The NAnt .build file to execute.</value>
        /// <remarks>None.</remarks>
        [Description("The NAnt .build file to execute."),Category("Data")]
        public string BuildFile {
            get {
                return TaskElement.GetAttribute("buildfile");
            }
            set {
                if (value == "") {
                    TaskElement.RemoveAttribute("buildfile");
                } else {
                    TaskElement.SetAttribute("buildfile", value);
                }
                Save();
            }
        }

        /// <summary>
        /// Gets or sets whether current property values should be inherited by the executed project.
        /// </summary>
        /// <value>Whether current property values should be inherited by the executed project.</value>
        /// <remarks>None.</remarks>
        [Description("Whether current property values should be inherited by the executed project."),Category("Behavior")]
        public string InheritAll {
            get {
                return TaskElement.GetAttribute("inheritall");
            }
            set {
                if (value == "") {
                    TaskElement.RemoveAttribute("inheritall");
                } else {
                    TaskElement.SetAttribute("inheritall", value);
                }
                Save();
            }
        }

        /// <summary>
        /// Gets or sets the directory to execute the NAnt build from.
        /// </summary>
        /// <value>The directory to execute the NAnt build from.</value>
        /// <remarks>None.</remarks>
        [Description("The directory to execute the NAnt build from."),Category("Data")]
        public string BaseDir {
            get {
                return TaskElement.GetAttribute("basedir");
            }
            set {
                if (value == "") {
                    TaskElement.RemoveAttribute("basedir");
                } else {
                    TaskElement.SetAttribute("basedir", value);
                }
                Save();
            }
        }

        /// <summary>
        /// Gets or sets the Target of the NAnt project to execute.
        /// </summary>
        /// <value>The Target of the NAnt project to execute.</value>
        /// <remarks>None.</remarks>
        [Description("The Target of the NAnt project to execute."), Category("Data")]
        public string Target {
            get {
                return TaskElement.GetAttribute("target");
            }
            set {
                if (value == "") {
                    TaskElement.RemoveAttribute("target");
                } else {
                    TaskElement.SetAttribute("target", value);
                }
                Save();
            }
        }
    }
}
