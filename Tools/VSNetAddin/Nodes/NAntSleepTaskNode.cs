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
    /// Tree Node that represents an NAnt sleep task.
    /// </summary>
    /// <remarks>None.</remarks>
    [NAntTask("sleep", "Pause the Build", "sleeptask.bmp")]
    public class NAntSleepTaskNode : NAntTaskNode {
        /// <summary>
        /// Creates a new <see cref="NAntSleepTaskNode"/>.
        /// </summary>
        /// <param name="TaskElement">The task's XML element.</param>
        /// <param name="ParentElement">The parent XML element of the task.</param>
        /// <remarks>None.</remarks>
        public NAntSleepTaskNode(XmlElement TaskElement, XmlElement ParentElement) 
            : base(TaskElement, ParentElement) {
        }

        /// <summary>
        /// Gets or sets the hours to add to the sleep time.
        /// </summary>
        /// <value>The hours to add to the sleep time.</value>
        /// <remarks>None.</remarks>
        [Description("Hours to add to the sleep time."),Category("Data")]
        public string Hours {
            get {
                return TaskElement.GetAttribute("hours");
            }
            set {
                if (value == "") {
                    TaskElement.RemoveAttribute("hours");
                } else {
                    TaskElement.SetAttribute("hours", value);
                }
                Save();
            }
        }

        /// <summary>
        /// Gets or sets the minutes to add to the sleep time.
        /// </summary>
        /// <value>The minutes to add to the sleep time.</value>
        /// <remarks>None.</remarks>
        [Description("Minutes to add to the sleep time."),Category("Data")]
        public string Minutes {
            get {
                return TaskElement.GetAttribute("minutes");
            }
            set {
                if (value == "") {
                    TaskElement.RemoveAttribute("minutes");
                } else {
                    TaskElement.SetAttribute("minutes", value);
                }
                Save();
            }
        }

        /// <summary>
        /// Gets or sets the seconds to add to the sleep time.
        /// </summary>
        /// <value>The seconds to add to the sleep time.</value>
        /// <remarks>None.</remarks>
        [Description("Seconds to add to the sleep time."),Category("Data")]
        public string Seconds {
            get {
                return TaskElement.GetAttribute("seconds");
            }
            set {
                if (value == "") {
                    TaskElement.RemoveAttribute("seconds");
                } else {
                    TaskElement.SetAttribute("seconds", value);
                }
                Save();
            }
        }

        /// <summary>
        /// Gets or sets the milliseconds to add to the sleep time.
        /// </summary>
        /// <value>The milliseconds to add to the sleep time.</value>
        /// <remarks>None.</remarks>
        [Description("Milliseconds to add to the sleep time."),Category("Data")]
        public string Milliseconds {
            get {
                return TaskElement.GetAttribute("milliseconds");
            }
            set {
                if (value == "") {
                    TaskElement.RemoveAttribute("milliseconds");
                } else {
                    TaskElement.SetAttribute("milliseconds", value);
                }
                Save();
            }
        }
    }
}
