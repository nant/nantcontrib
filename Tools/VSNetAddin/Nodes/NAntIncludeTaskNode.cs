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
    /// Tree Node that represents an NAnt include task.
    /// </summary>
    /// <remarks>None.</remarks>
    [NAntTask("include", "Include an External .build File", "includetask.bmp")]
    public class NAntIncludeTaskNode : NAntTaskNode {
        /// <summary>
        /// Creates a new <see cref="NAntIncludeTaskNode"/>.
        /// </summary>
        /// <param name="TaskElement">The task's XML element.</param>
        /// <param name="ParentElement">The parent XML element of the task.</param>
        /// <remarks>None.</remarks>
        public NAntIncludeTaskNode(XmlElement TaskElement, XmlElement ParentElement) 
            : base(TaskElement, ParentElement) {
        }

        /// <summary>
        /// Gets or sets the .build file to include.
        /// </summary>
        /// <remarks>The .build file to include.</remarks>
        /// <example>None.</example>
        [Description("The .build file to include."),Category("Data")]
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
    }
}
