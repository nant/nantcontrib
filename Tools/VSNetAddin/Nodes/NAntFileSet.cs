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
using System.Drawing.Design;
using System.ComponentModel;
using System.ComponentModel.Design;
using NAnt.Contrib.NAntAddin.Controls;

namespace NAnt.Contrib.NAntAddin.Nodes {
    /// <summary>
    /// Provides a component with editable array properties 
    /// to represent an NAnt "fileset" type structure.
    /// </summary>
    /// <remarks>None.</remarks>
    public class NAntFileSet : Component, ConstructorArgsResolver {
        private string fileSetName;
        private string baseDir;
        private NAntTaskNode taskNode;
        private XmlElement taskElement;
        private XmlElement fileSetElement;
        private FileSystemSpecifier[] includes = new FileSystemSpecifier[0];
        private FileSystemSpecifier[] excludes = new FileSystemSpecifier[0];

        /// <summary>
        /// Creates a new <see cref="NAntFileSet"/>.
        /// </summary>
        /// <remarks>None.</remarks>
        public NAntFileSet() {
        }

        /// <summary>
        /// Creates a new <see cref="NAntFileSet"/>.
        /// </summary>
        /// <param name="TaskNode">The task's tree node.</param>
        /// <param name="TaskElement">The task's XML element.</param>
        /// <param name="FileSetName">The tag name of the File Set's XML element</param>
        /// <remarks>None.</remarks>
        public NAntFileSet(NAntTaskNode TaskNode, XmlElement TaskElement, string FileSetName) {
            taskNode = TaskNode;
            taskElement = TaskElement;
            fileSetName = FileSetName;

            fileSetElement = (XmlElement)taskElement.SelectSingleNode(FileSetName);
        }

        /// <summary>
        /// Saves the FileSet.
        /// </summary>
        /// <remarks>None.</remarks>
        internal void Save() {
            taskNode.ProjectNode.Save();
        }

        /// <summary>
        /// Gets or sets the directory from which to resolve resources.
        /// </summary>
        /// <value>The directory from which to resolve resources.</value>
        /// <remarks>None.</remarks>
        [Description("The directory from which to resolve resources."),Category("Data")]
        public string BaseDir {
            get {
                if (FileSetElement != null) {
                    return FileSetElement.GetAttribute("baseDir");
                }
                return baseDir;
            }
            set {
                if (FileSetElement == null) {
                    baseDir = value;
                } else {
                    if (value == "") {
                        fileSetElement.RemoveAttribute("baseDir");
                    } else {
                        fileSetElement.SetAttribute("baseDir", value);
                    }
                }
                Save();
            }
        }

        /// <summary>
        /// Gets or sets the paths that should be included.
        /// </summary>
        /// <value>The paths that should be included.</value>
        /// <remarks>None.</remarks>
        [Editor("System.ComponentModel.Design.ArrayEditor, System.Design", 
             typeof(System.Drawing.Design.UITypeEditor))]
        [Description("Paths that should be included.")]
        public FileSystemSpecifier[] Includes {
            get {
                if (FileSetElement == null) {
                    return includes;
                }

                XmlNodeList includesElems = FileSetElement.SelectNodes("includes");
                FileSystemSpecifier[] includeSpecs = new FileSystemSpecifier[includesElems.Count];
                FileSystemSpecifier[] readOnlyIncludes = new FileSystemSpecifier[includesElems.Count];

                for (int i = 0; i < includesElems.Count; i++) {
                    XmlElement includesElem = (XmlElement)includesElems.Item(i);
                    includeSpecs[i] = new FileSystemSpecifier(includesElem, "includes");

                    if (taskNode.Parent == null) {
                        // If read only, create a proxy node
                        readOnlyIncludes[i] = (FileSystemSpecifier)NAntReadOnlyNodeBuilder.GetReadOnlyNode(
                            includeSpecs[i]);
                    }
                }
                if (taskNode.Parent == null) {
                    return readOnlyIncludes;
                }
                return includeSpecs;
            }
            set {
                if (FileSetElement == null) {
                    CreateElement(taskElement.OwnerDocument, fileSetName);
                    taskElement.AppendChild(FileSetElement);
                }

                // Remove any old includes
                XmlNodeList includesElems = FileSetElement.SelectNodes("includes");
                if (includesElems != null) {
                    foreach (XmlNode include in includesElems) {
                        FileSetElement.RemoveChild(include);
                    }
                }

                if (value != null) {
                    for (int i = 0; i < value.Length; i++) {
                        // Append new includes
                        value[i].AppendToParent(FileSetElement, "includes");
                    }
                }

                taskNode.Save();
            }
        }

        /// <summary>
        /// Gets or sets the paths that should be excluded.
        /// </summary>
        /// <value>The paths that should be excluded.</value>
        /// <remarks>None.</remarks>
        [Editor("System.ComponentModel.Design.ArrayEditor, System.Design", 
             typeof(System.Drawing.Design.UITypeEditor))]
        [Description("Paths that should be excluded.")]
        public FileSystemSpecifier[] Excludes {
            get {
                if (FileSetElement == null) {
                    return excludes;
                }

                XmlNodeList excludesElems = FileSetElement.SelectNodes("excludes");
                FileSystemSpecifier[] excludeSpecs = new FileSystemSpecifier[excludesElems.Count];
                FileSystemSpecifier[] readOnlyExcludes = new FileSystemSpecifier[excludesElems.Count];

                for (int i = 0; i < excludesElems.Count; i++) {
                    XmlElement excludesElem = (XmlElement)excludesElems.Item(i);
                    excludeSpecs[i] = new FileSystemSpecifier(excludesElem, "excludes");

                    if (taskNode.Parent == null) {
                        // If read only, create a proxy node
                        readOnlyExcludes[i] = (FileSystemSpecifier)NAntReadOnlyNodeBuilder.GetReadOnlyNode(
                            excludeSpecs[i]);
                    }
                }
                if (taskNode.Parent == null) {
                    return readOnlyExcludes;
                }
                return excludeSpecs;
            }
            set {
                if (FileSetElement == null) {
                    CreateElement(taskElement.OwnerDocument, fileSetName);
                    taskElement.AppendChild(FileSetElement);
                }

                // Remove any old excludes
                XmlNodeList excludesElems = FileSetElement.SelectNodes("excludes");
                if (excludesElems != null) {
                    foreach (XmlNode exclude in excludesElems) {
                        FileSetElement.RemoveChild(exclude);
                    }
                }

                if (value != null) {
                    if (FileSetElement == null) {
                        excludes = new FileSystemSpecifier[value.Length];
                    }

                    for (int i = 0; i < value.Length; i++) {
                        // Append new excludes
                        value[i].AppendToParent(FileSetElement, "excludes");
                    }
                }

                taskNode.Save();
            }
        }

        /// <summary>
        /// Gets the File Set's XML element.
        /// </summary>
        /// <value>The File Set's XML element.</value>
        /// <remarks>None.</remarks>
        [Browsable(false)]
        public XmlElement FileSetElement {
            get {
                return fileSetElement;
            }
        }

        /// <summary>
        /// Appends the current Documenter Element to the documenters element.
        /// </summary>
        /// <param name="TaskElement">The task's Xml Element to append to.</param>
        /// <param name="FileSetName">The name of the fileset's XML element.</param>
        /// <remarks>None.</remarks>
        public void AppendToTask(XmlElement TaskElement, string FileSetName) {
            if (FileSetElement == null) {
                CreateElement(TaskElement.OwnerDocument, FileSetName);
            }
            TaskElement.AppendChild(FileSetElement);
        }

        /// <summary>
        /// Creates the property XML element.
        /// </summary>
        /// <param name="Document">The XML Document to use.</param>
        /// <param name="FileSetName">The name of the FileSet's XML element.</param>
        /// <remarks>None.</remarks>
        private void CreateElement(XmlDocument Document, string FileSetName) {
            fileSetName = FileSetName;
            fileSetElement = Document.CreateElement(FileSetName);
            BaseDir = baseDir;

            for (int i = 0; i < includes.Length; i++) {
                includes[i].AppendToParent(fileSetElement, FileSetName);
            }

            for (int j = 0; j < includes.Length; j++) {
                excludes[j].AppendToParent(fileSetElement, FileSetName);
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
                taskElement,
                taskNode
            };
        }
    }

    /// <summary>
    /// Specifies an included or excluded file or directory.
    /// </summary>
    /// <remarks>None.</remarks>
    public class FileSystemSpecifier : Component, ConstructorArgsResolver {
        private string name, asIs, specifierName;
        internal XmlElement specifierElement;

        /// <summary>
        /// Creates a new <see cref="FileSystemSpecifier"/>.
        /// </summary>
        /// <remarks>None.</remarks>
        public FileSystemSpecifier() {
        }

        /// <summary>
        /// Creates a new <see cref="FileSystemSpecifier"/>.
        /// </summary>
        /// <param name="SpecifierElement">The XML element with include and/or exclude XML elements as children.</param>
        /// <param name="SpecifierName">The name of the specifier's XML element.</param>
        /// <remarks>None.</remarks>
        public FileSystemSpecifier(XmlElement SpecifierElement, string SpecifierName) {
            specifierElement = SpecifierElement;
            specifierName = SpecifierName;
        }

        /// <summary>
        /// Gets or sets the path to a file or directory.
        /// </summary>
        /// <value>The path to a file or directory.</value>
        /// <remarks>None.</remarks>
        [Description("The path to a file or directory.")]
        public string Name {
            get {
                if (specifierElement != null) {
                    return specifierElement.GetAttribute("name");
                }
                return name;
            }
            set {
                if (specifierElement == null) {
                    name = value;
                } else {
                    if (value == "") {
                        specifierElement.RemoveAttribute("name");
                    } else {
                        specifierElement.SetAttribute("name", value);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the path should use the BaseDir 
        /// property of the containing <see cref="FileSet"/> to 
        /// resolve file and directory names.
        /// </summary>
        /// <value>Whether the path should use the BaseDir 
        /// property of the containing <see cref="FileSet"/> to 
        /// resolve file and directory names.</value>
        /// <remarks>None.</remarks>
        [Description("If the path should not use the BaseDir to resolve.")]
        public string AsIs {
            get {
                if (specifierElement != null) {
                    return specifierElement.GetAttribute("asis");
                }
                return asIs;
            }
            set {
                if (specifierElement == null) {
                    asIs = value;
                } else {
                    if (value == "") {
                        specifierElement.RemoveAttribute("asis");
                    } else {
                        specifierElement.SetAttribute("asis", value);
                    }
                }
            }
        }

        /// <summary>
        /// Appends the current Property Element to the documenter element.
        /// </summary>
        /// <param name="ParentElement">The parent XML Element to append to.</param>
        /// <param name="specifierName">The name of the Specifier's XML element.</param>
        /// <remarks>None.</remarks>
        public void AppendToParent(XmlElement ParentElement, string specifierName) {
            if (specifierElement == null) {
                CreateElement(ParentElement.OwnerDocument, specifierName);
            }
            ParentElement.AppendChild(specifierElement);
        }

        /// <summary>
        /// Creates the Specifier's XML element.
        /// </summary>
        /// <param name="Document">The document to create with.</param>
        /// <param name="SpecifierName">The name of the Specifier's XML element.</param>
        /// <remarks>None.</remarks>
        public void CreateElement(XmlDocument Document, string SpecifierName) {
            specifierName = SpecifierName;
            specifierElement = Document.CreateElement(SpecifierName);
            specifierElement.SetAttribute("name", name);
            specifierElement.SetAttribute("asis", asIs);
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
                specifierElement,
                specifierName
            };
        }
    }

    /// <summary>
    /// An <see cref="NAntFileSet"/> that specifies a set of files or directories.
    /// </summary>
    /// <remarks>None.</remarks>
    public class FileSet : NAntFileSet {
        /// <summary>
        /// Creates a new <see cref="FileSet"/>.
        /// </summary>
        /// <param name="TaskElement">The fileset XML element.</param>
        /// <param name="TaskNode">The <see cref="NAntTaskNode"/> for which this <see cref="FileSet"/> is a property.</param>
        /// <remarks>None.</remarks>
        public FileSet(XmlElement TaskElement, NAntTaskNode TaskNode) : base(TaskNode, TaskElement, "fileset") {
        }
    }
}
