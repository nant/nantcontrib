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
    /// Tree Node that represents an NAnt lib task.
    /// </summary>
    /// <remarks>None.</remarks>
    [NAntTask("lib", "Create a Dynamic-Link Library", "libtask.bmp")]
    public class NAntLibraryTaskNode : NAntTaskNode {
        /// <summary>
        /// Creates a new <see cref="NAntLibraryTaskNode"/>.
        /// </summary>
        /// <param name="TaskElement">The task's XML element.</param>
        /// <param name="ParentElement">The parent XML element of the task.</param>
        /// <remarks>None.</remarks>
        public NAntLibraryTaskNode(XmlElement TaskElement, XmlElement ParentElement) 
            : base(TaskElement, ParentElement) {
        }

        /// <summary>
        /// Gets or sets options to pass to the compiler.
        /// </summary>
        /// <value>Options to pass to the compiler.</value>
        /// <remarks>None.</remarks>
        [Description("Options to pass to the compiler."),Category("Data")]
        public string Options {
            get {
                return TaskElement.GetAttribute("options");
            }
            set {
                if (value == "") {
                    TaskElement.RemoveAttribute("options");
                } else {
                    TaskElement.SetAttribute("options", value);
                }
                Save();
            }
        }

        /// <summary>
        /// Gets or sets the output file name.
        /// </summary>
        /// <value>The output file name.</value>
        /// <remarks>None.</remarks>
        [Description("The output file name."),Category("Data")]
        public string Output {
            get {
                return TaskElement.GetAttribute("output");
            }
            set {
                if (value == "") {
                    TaskElement.RemoveAttribute("output");
                } else {
                    TaskElement.SetAttribute("output", value);
                }
                Save();
            }
        }

        /// <summary>
        /// Gets or sets source files to combine.
        /// </summary>
        /// <value>Source files to combine.</value>
        /// <remarks>None.</remarks>
        [Description("Source files to combine."),Category("Data")]
        public Sources Sources {
            get {
                Sources sources = new Sources(TaskElement, this);
                if (Parent == null) {
                    return (Sources)NAntReadOnlyNodeBuilder.GetReadOnlyNode(sources);
                }
                return sources;
            }
            set {
                value.AppendToTask(TaskElement, "sources");
                Save();
            }
        }

        /// <summary>
        /// Gets or sets directories in which to search for library files.
        /// </summary>
        /// <value>Directories in which to search for library files.</value>
        /// <remarks>None.</remarks>
        [Description("Directories in which to search for library files."),Category("Data")]
        public LibDirs LibDirs {
            get {
                LibDirs libDirs = new LibDirs(TaskElement, this);
                if (Parent == null) {
                    return (LibDirs)NAntReadOnlyNodeBuilder.GetReadOnlyNode(libDirs);
                }
                return libDirs;
            }
            set {
                value.AppendToTask(TaskElement, "libdirs");
                Save();
            }
        }
    }

    /// <summary>
    /// An <see cref="NAntFileSet"/> that specifies directories containing libraries.
    /// </summary>
    /// <remarks>None.</remarks>
    public class LibDirs : NAntFileSet {
        /// <summary>
        /// Creates a new <see cref="LibDirs"/>.
        /// </summary>
        /// <param name="TaskElement">The libdirs XML element.</param>
        /// <param name="TaskNode">The <see cref="NAntTaskNode"/> for which this <see cref="LibDirs"/> is a property.</param>
        /// <remarks>None.</remarks>
        public LibDirs(XmlElement TaskElement, NAntTaskNode TaskNode) 
            : base(TaskNode, TaskElement, "libdirs") {
        }
    }
}