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
    /// Tree Node that represents an NAnt get task.
    /// </summary>
    [NAntTask("get", "Download Files from a URL", "gettask.bmp")]
    public class NAntGetTaskNode : NAntTaskNode {
        /// <summary>
        /// Creates a new <see cref="NAntGetTaskNode"/>.
        /// </summary>
        /// <param name="TaskElement">The task's XML element.</param>
        /// <param name="ParentElement">The parent XML element of the task.</param>
        /// <remarks>None.</remarks>
        public NAntGetTaskNode(XmlElement TaskElement, XmlElement ParentElement) 
            : base(TaskElement, ParentElement) {
        }

        /// <summary>
        /// Gets or sets the URL of the file to download.
        /// </summary>
        /// <value>The URL of the file to download.</value>
        /// <remarks>None.</remarks>
        [Description("The URL of the file to download."),Category("Data")]
        public string Src {
            get {
                return TaskElement.GetAttribute("src");
            }
            set {
                if (value == "") {
                    TaskElement.RemoveAttribute("src");
                } else {
                    TaskElement.SetAttribute("src", value);
                }
                Save();
            }
        }

        /// <summary>
        /// Gets or sets the filename to save the downloaded file to.
        /// </summary>
        /// <value>The filename to save the downloaded file to.</value>
        /// <remarks>None.</remarks>
        [Description("The filename to save the downloaded file to."),Category("Data")]
        public string Dest {
            get {
                return TaskElement.GetAttribute("dest");
            }
            set {
                if (value == "") {
                    TaskElement.RemoveAttribute("dest");
                } else {
                    TaskElement.SetAttribute("dest", value);
                }
                Save();
            }
        }

        /// <summary>
        /// Gets or sets If inside a firewall, proxy server/port information.
        /// </summary>
        /// <value>If inside a firewall, proxy server/port information.</value>
        /// <remarks>None.</remarks>
        [Description("If inside a firewall, proxy server/port information. Example: proxy.mycompany.com:8080"),Category("Data")]
        public string HttpProxy {
            get {
                return TaskElement.GetAttribute("httpproxy");
            }
            set {
                if (value == "") {
                    TaskElement.RemoveAttribute("httpproxy");
                } else {
                    TaskElement.SetAttribute("httpproxy", value);
                }
                Save();
            }
        }

        /// <summary>
        /// Gets or sets whether to log errors but not treat as fatal.
        /// </summary>
        /// <value>Whether to log errors but not treat as fatal.</value>
        /// <remarks>None.</remarks>
        [Description("Whether to log errors but not treat as fatal."),Category("Behavior")]
        public string IgnoreErrors {
            get {
                return TaskElement.GetAttribute("ignoreerrors");
            }
            set {
                if (value == "") {
                    TaskElement.RemoveAttribute("ignoreerrors");
                } else {
                    TaskElement.SetAttribute("ignoreerrors", value);
                }
                Save();
            }
        }

        /// <summary>
        /// Gets or sets whether to download the file(s) based on the timestamp of the local copy.
        /// </summary>
        /// <value>Whether to download the file(s) based on the timestamp of the local copy.</value>
        /// <remarks>None.</remarks>
        [Description("Download the file(s) based on the timestamp of the local copy."),Category("Behavior")]
        public string UseTimeStamp {
            get {
                return TaskElement.GetAttribute("usetimestamp");
            }
            set {
                if (value == "") {
                    TaskElement.RemoveAttribute("usetimestamp");
                } else {
                    TaskElement.SetAttribute("usetimestamp", value);
                }
                Save();
            }
        }

        /// <summary>
        /// Gets or sets the files to download.
        /// </summary>
        /// <value>The files to download</value>
        /// <remarks>None.</remarks>
        [Description("Files to download."),Category("Data")]
        public FileSet FileSet {
            get {
                FileSet fileSet = new FileSet(TaskElement, this);
                if (Parent == null) {
                    return (FileSet)NAntReadOnlyNodeBuilder.GetReadOnlyNode(fileSet);
                }
                return fileSet;
            }
            set {
                value.AppendToTask(TaskElement, "fileset");
                Save();
            }
        }
    }
}
