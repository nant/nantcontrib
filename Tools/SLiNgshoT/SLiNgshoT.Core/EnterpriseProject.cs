// EnterpriseProject.cs - the EnterpriseProject class
// Copyright (C) 2001, 2002  Jason Diamond
// Copyright (C) 2002 Szymon Kobalczyk
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
    /// <summary>Represents an enterprise project in a solution.</summary>
    public class EnterpriseProject {
        internal EnterpriseProject(EnterpriseProject parent, Solution solution, Guid id, string name) {
            _ParentProject = parent;
            _Solution = solution;
            _ID = id;
            _Name = name;
        }

        internal EnterpriseProject(Solution solution, Guid id, string name) {
            _ParentProject = null;
            _Solution = solution;
            _ID = id;
            _Name = name;
        }

        private ArrayList _SubProjects = new ArrayList();

        /// <summary>Gets the enterprise projects that are sub projects of this project.</summary>
        public ArrayList SubProjects {
            get { return _SubProjects; }
        }

        private EnterpriseProject _ParentProject;

        /// <summary>Gets the enterprise project that contains this project.</summary>
        public EnterpriseProject ParentProject {
            get { return _ParentProject; }
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

            if (extension == ".etp") {
                _ProjectDocument = new XPathDocument(path);
                _ProjectNavigator = _ProjectDocument.CreateNavigator();

                _RelativePath = Path.GetDirectoryName(path);

                this.GetSubProjects();
            }
        }

        private ArrayList _Files;

        /// <summary>Gets an enumerable list of files contained in this project.</summary>
        public IEnumerable GetFiles() {
            if (_Files == null) {
                _Files = new ArrayList();

                XPathNodeIterator nodes =
                    _ProjectNavigator.Select("/EFPROJECT/GENERAL/Views/ProjectExplorer/File");

                while (nodes.MoveNext()) {
                    _Files.Add(nodes.Current.Value);
                }
            }
            return _Files;
        }

        protected string GetReferenceID(string file) {
            XPathNodeIterator nodes =
                _ProjectNavigator.Select(String.Format(
                "/EFPROJECT/GENERAL/REFERENCES/Reference[FILE='{0}']",
                file));

            if (nodes.MoveNext()) {
                return (string)nodes.Current.Evaluate("string(GUIDPROJECTID)");
            }

#warning VS.NET has some bug and writes References node with two different capitalizations

            nodes =_ProjectNavigator.Select(String.Format(
                "/EFPROJECT/GENERAL/References/Reference[FILE='{0}']",
                file));

            if (nodes.MoveNext()) {
                return (string)nodes.Current.Evaluate("string(GUIDPROJECTID)");
            }

            return "";
        }

        protected IList GetSubProjects() {
            ArrayList list = new ArrayList();

            foreach (string file in GetFiles()) {
                string extension = Path.GetExtension(file);

                if (extension == ".etp") {
                    string id = GetReferenceID(file);
                    string name = Path.GetFileNameWithoutExtension(file);
                    EnterpriseProject subproject = new EnterpriseProject(this, _Solution, new Guid(id), name);

                    string absoluteProjectPath = Path.Combine(_RelativePath, file);
                    subproject.Read(absoluteProjectPath);

                    _SubProjects.Add(subproject);
                }
            }

            return list;
        }

        ArrayList _Projects; 

        public IList GetProjects() {
            if (_Projects == null) {
                _Projects = new ArrayList();

                foreach (string file in GetFiles()) {
                    string extension = Path.GetExtension(file);

                    if (extension == ".csproj" || extension == ".vcproj") {
                        string name = Path.GetFileNameWithoutExtension(file);
                        string id = GetReferenceID(file);
                        string path = _Solution.ResolvePath(file);

                        Project project = new Project(_Solution, new Guid(id), name);

                        string absoluteProjectPath = Path.Combine(_RelativePath, path);
                        project.Read(absoluteProjectPath);

                        string relativeProjectPath = Path.GetDirectoryName(absoluteProjectPath);
                        project.RelativePath = relativeProjectPath;

                        if (project.ProjectType == "C# Local" ||
                            project.ProjectType == "C# Web") {
                            _Projects.Add(project);
                        }
                    }
                }

                foreach (EnterpriseProject project in _SubProjects) {
                    _Projects.AddRange(project.GetProjects());
                }
            }

            return _Projects;
        }
    }
}
