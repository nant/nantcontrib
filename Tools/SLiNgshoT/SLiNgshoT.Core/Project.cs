// Project.cs - the Project class
// Copyright (C) 2001, 2002  Jason Diamond
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

using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace SLiNgshoT.Core {
    /// <summary>Represents a project in a solution.</summary>
    public class Project {
        internal Project(Solution solution, Guid id, string name) {
            _Solution = solution;
            _ID = id;
            _Name = name;
        }

        private Solution _Solution;

        /// <summary>Gets the solution that contains this project.</summary>
        public Solution Solution {
            get { return _Solution; }
        }

        private string _RelativePath;

        /// <summary>Gets or sets the relative path (from the solution
        /// directory) to the project directory.</summary>
        public string RelativePath {
            get { return _RelativePath; }
            set { _RelativePath = value; }
        }

        private Guid _ID;

        /// <summary>Gets the GUID that identifies the project.</summary>
        public Guid ID {
            get { return _ID; }
        }

        private string _Name;

        /// <summary>Gets the name of the project.</summary>
        public string Name {
            get { return _Name; }
        }

        private XPathDocument _ProjectDocument;
        private XPathNavigator _ProjectNavigator;

        /// <summary>Reads the project file from the specified path.</summary>
        /// <param name="path">The path to the project file.</param>
        public void Read(string path) {
            string extension = Path.GetExtension(path);

            if (extension == ".csproj" || extension == ".vcproj" || extension == ".vbproj") {
                _ProjectDocument = new XPathDocument(path);
                _ProjectNavigator = _ProjectDocument.CreateNavigator();
            }
        }

        /// <summary>Gets a string that represents the type of project.</summary>
        /// <value>"Visual C++", "C# Local", "C# Web", "VB Local", or "VB Web"</value>
        public string ProjectType {
            get {
                string projectType = "";

                if (_ProjectNavigator != null) {
                    if ((bool)_ProjectNavigator.Evaluate("boolean(VisualStudioProject/@ProjectType='Visual C++')")) {
                        projectType = "Visual C++";
                    }
                    else if ((bool)_ProjectNavigator.Evaluate("boolean(VisualStudioProject/CSHARP/@ProjectType='Local')")) {
                        projectType = "C# Local";
                    }
                    else if ((bool)_ProjectNavigator.Evaluate("boolean(VisualStudioProject/CSHARP/@ProjectType='Web')")) {
                        projectType = "C# Web";
                    }
                    else if((bool)_ProjectNavigator.Evaluate("boolean(VisualStudioProject/VisualBasic/@ProjectType='Local')")) {
                        projectType = "VB Local";
                    }
                    else if((bool)_ProjectNavigator.Evaluate("boolean(VisualStudioProject/VisualBasic/@ProjectType='Web')")) {
                        projectType = "VB Web";
                    }
                }

                return projectType;
            }
        }

        /// <summary>Gets the name of the assembly this project generates.</summary>
        public string AssemblyName {
            get {
                string assemblyName = "";

                if (_ProjectNavigator != null) {
                    switch (this.ProjectType) {
                        case "C# Local":
                        case "C# Web":
                            assemblyName = (string)_ProjectNavigator.Evaluate("string(/VisualStudioProject/CSHARP/Build/Settings/@AssemblyName)");
                            break;
                        case "VB Local":
                        case "VB Web":
                            assemblyName = (string)_ProjectNavigator.Evaluate("string(/VisualStudioProject/VisualBasic/Build/Settings/@AssemblyName)");
                            break;
                        default:
                            assemblyName = "";
                            break;
                    }
                }

                return assemblyName;
            }
        }

        /// <summary>Gets the output type of the project.</summary>
        /// <value>"Library", "Exe", or "WinExe"</value>
        public string OutputType {
            get {
                string outputType = "";

                if (_ProjectNavigator != null) {
                    switch (this.ProjectType) {
                        case "C# Local":
                        case "C# Web":
                            outputType = (string)_ProjectNavigator.Evaluate("string(/VisualStudioProject/CSHARP/Build/Settings/@OutputType)");
                            break;
                        case "VB Local":
                        case "VB Web":
                            outputType = (string)_ProjectNavigator.Evaluate("string(/VisualStudioProject/VisualBasic/Build/Settings/@OutputType)");
                            break;
                        default:
                            outputType = "";
                            break;
                    }
                }

                return outputType;
            }
        }

        /// <summary>Gets the filename of the generated assembly.</summary>
        public string OutputFile {
            get {
                string extension = "";

                switch (OutputType) {
                    case "Library":
                        extension = ".dll";
                        break;
                    case "Exe":
                        extension = ".exe";
                        break;
                    case "WinExe":
                        extension = ".exe";
                        break;
                }

                return AssemblyName + extension;
            }
        }

        /// <summary>Gets the default namespace for the project.</summary>
        public string RootNamespace {
            get {
                switch (this.ProjectType) {
                    case "C# Local":
                    case "C# Web":
                        return (string)_ProjectNavigator.Evaluate("string(/VisualStudioProject/CSHARP/Build/Settings/@RootNamespace)");
                    case "VB Local":
                    case "VB Web":
                        return (string)_ProjectNavigator.Evaluate("string(/VisualStudioProject/VisualBasic/Build/Settings/@RootNamespace)");
                    default:
                        return "";
                }
            }
        }

        public ArrayList GetConfigurations() {
            ArrayList configurations = new ArrayList();

            XPathNodeIterator nodes;

            switch (this.ProjectType) {
                case "VB Local":
                case "VB Web":
                    nodes =
                        _ProjectNavigator.Select("/VisualStudioProject/VisualBasic/Build/Settings/Config");
                    break;
                default:
                    nodes =
                        _ProjectNavigator.Select("/VisualStudioProject/CSHARP/Build/Settings/Config");
                    break;
            }

            while (nodes.MoveNext()) {
                configurations.Add(new Configuration(nodes.Current));
            }

            return configurations;
        }

        /// <summary>Gets the configuration with the specified name.</summary>
        /// <param name="name">"Debug" or "Release"</param>
        /// <returns>A Configuration object.</returns>
        public Configuration GetConfiguration(string name) {
            XPathNavigator navigator = null;
            XPathNodeIterator nodes;

            switch (this.ProjectType) {
                case "VB Local":
                case "VB Web":
                    nodes = _ProjectNavigator.Select(
                        String.Format(
                        "/VisualStudioProject/VisualBasic/Build/Settings/Config[@Name='{0}']",
                        name));
                    break;
                default:
                    nodes = _ProjectNavigator.Select(
                        String.Format(
                        "/VisualStudioProject/CSHARP/Build/Settings/Config[@Name='{0}']",
                        name));
                    break;
            }

            if (nodes.MoveNext()) {
                navigator = nodes.Current;
            }

            // 2003-04-18 - jean rajotte - make this safe
            Configuration res = null;
            if ( navigator != null ) {
                res = new Configuration(navigator);
            }
            return res;

        }

        /// <summary>Gets the relative path (from the project directory) to the
        /// assembly this project generates.</summary>
        /// <param name="name">A configuration name.</param>
        public string GetRelativeOutputPathForConfiguration(string name) {
            return Path.Combine(
                Path.Combine(RelativePath, GetConfiguration(name).OutputPath),
                OutputFile);
        }

        /// <summary>Gets the relative path (from the project directory) to the
        /// XML documentation this project generates.</summary>
        /// <param name="name">A configuration name.</param>
        public string GetRelativePathToDocumentationFile(string name) {
            string path = null;

            string documentationFile = GetConfiguration(name).DocumentationFile;

            if (documentationFile != null && documentationFile.Length > 0) {
                path = Path.Combine(RelativePath, documentationFile);
            }

            return path;
        }

        private ArrayList _References;

        /// <summary>Gets an enumerable list of reference objects this project
        /// references.</summary>
        public IEnumerable GetReferences() {
            if (_References == null) {
                if (_ProjectNavigator != null) {
                    _References = new ArrayList();

                    XPathNodeIterator nodes = null;

                    switch (this.ProjectType) {
                        case "VB Local":
                        case "VB Web":
                            nodes =
                                _ProjectNavigator.Select("/VisualStudioProject/VisualBasic/Build/References/Reference");
                            break;
                        default:
                            nodes =
                                _ProjectNavigator.Select("/VisualStudioProject/CSHARP/Build/References/Reference");
                            break;
                    }

                    while (nodes.MoveNext()) {
                        Reference reference = new Reference(_Solution, nodes.Current);
                        _References.Add(reference);
                    }
                }
            }

            return _References;
        }

        /// <summary>Gets a list of projects that this project references.</summary>
        public IList GetSystemReferences() {
            ArrayList projects = new ArrayList();

            foreach (Reference reference in GetReferences()) {
                if ((reference.Type == "AssemblyName") || (reference.Type == "Guid")) {
                    projects.Add(reference);
                }
            }

            return projects;
        }

        /// <summary>Gets a list of projects that this project references.</summary>
        public IList GetReferencedProjects() {
            ArrayList projects = new ArrayList();

            foreach (Reference reference in GetReferences()) {
                if (reference.Type == "Project") {
                    Project project = _Solution.GetProject(reference.Value);

                    // Handle references to projects that cannot be found in the solution.
                    if (project == null) {
                        throw new ApplicationException("Project '" + this.Name + "' contains reference to unknown project '" + reference.Value + "'.");
                    }
                    projects.Add(project);
                }
            }

            return projects;
        }

        private ArrayList _Files;

        /// <summary>Gets an enumerable list of files contained in this project.</summary>
        public IEnumerable GetFiles() {
            if (_Files == null) {
                _Files = new ArrayList();

                XPathNodeIterator nodes;
  
                switch (this.ProjectType) {
                    case "VB Local":
                    case "VB Web":
                        nodes =
                            _ProjectNavigator.Select("/VisualStudioProject/VisualBasic/Files/Include/File");
                        break;
                    default:
                        nodes =
                            _ProjectNavigator.Select("/VisualStudioProject/CSHARP/Files/Include/File");
                        break;
                }


                while (nodes.MoveNext()) {
                    _Files.Add(new File(this, nodes.Current));
                }
            }

            return _Files;
        }

        /// <summary>Counts the files with the specified build action property.</summary>
        /// <param name="buildAction">"Compile", "EmbeddedResource", or "Content"</param>
        public int CountFiles(string buildAction) {
            int count = 0;

            foreach (File file in GetFiles()) {
                if (file.BuildAction == buildAction) {
                    ++count;
                }
            }

            return count;
        }

        public IList GetSourceFiles() {
            ArrayList list = new ArrayList();

            foreach (File file in GetFiles()) {
                if (file.BuildAction == "Compile") {
                    list.Add(file);
                }
            }

            return list;
        }

        public IList GetResXResourceFiles() {
            ArrayList list = new ArrayList();

            foreach (File file in GetFiles()) {
                if (file.BuildAction == "EmbeddedResource") {
                    if (Path.GetExtension(file.RelativePath).ToLower() == ".resx") {
                        long length = GetFileLength(file.AbsolutePath);

                        if (length > 0) {
                            list.Add(file);
                        }
                    }
                }
            }

            return list;
        }

        private static long GetFileLength(string path) {
            long length = 0;
            Stream stream = null;

            try {
                stream = System.IO.File.OpenRead(path);
                length = stream.Length;
            }
            finally {
                if (stream != null) {
                    stream.Close();
                }
            }

            return length;
        }

        public IList GetNonResXResourceFiles() {
            ArrayList list = new ArrayList();

            foreach (File file in GetFiles()) {
                if (file.BuildAction == "EmbeddedResource") {
                    if (Path.GetExtension(file.RelativePath).ToLower() != ".resx") {
                        list.Add(file);
                    }
                }
            }

            return list;
        }

        public string GetImports() {
            string importsString = "";

            if (this.ProjectType.StartsWith("VB")) {
                XPathNodeIterator nodes =
                    _ProjectNavigator.Select("/VisualStudioProject/VisualBasic/Build/Imports/Import");

                while (nodes.MoveNext()) {
                    if (importsString.Length > 0) { importsString += ","; }
                    importsString += nodes.Current.Evaluate("string(@Namespace)");
                }
            }

            return importsString;
        }
    }
}
