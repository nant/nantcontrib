// Solution.cs - the Solution class
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
    /// <summary>Represents a VS.NET solution.</summary>
    public class Solution {
        private string _SolutionDirectory;

        /// <summary>Gets the SolutionDirectory property.</summary>
        /// <remarks>This is the directory that contains the VS.NET
        /// solution file.</remarks>
        public string SolutionDirectory {
            get { return _SolutionDirectory; }
        }

        private string _SolutionName;

        /// <summary>Gets the SolutionName property.</summary>
        /// <remarks>This is the name of the VS.NET solution file
        /// without the .sln extension.</remarks>
        public string SolutionName {
            get { return _SolutionName; }
        }

        private Hashtable _UriMap;

        /// <summary>Reads a .sln file.</summary>
        /// <param name="path">The path to the .sln file.</param>
        /// <param name="uriPrefix"></param>
        /// <param name="filePrefix"></param>
        public void Read(string path, Hashtable uriMap) {
            _UriMap = uriMap;

            path = Path.GetFullPath(path);
            _SolutionDirectory = Path.GetDirectoryName(path);
            _SolutionName = Path.GetFileNameWithoutExtension(path);

            if ( ! System.IO.File.Exists( path ) )
                throw new ApplicationException( string.Concat( "file not found: ", path ) );

            StreamReader streamReader = null;

            try {
                streamReader = new StreamReader(path);

                string line = streamReader.ReadLine();

                if (!line.StartsWith("Microsoft Visual Studio Solution File, Format Version")) {
                    throw new ApplicationException("this is not a 'Microsoft Visual Studio Solution File' file");
                }

                bool projectDependencies = false;

                while ((line = streamReader.ReadLine()) != null) {
                    if (line.StartsWith("Project")) {
                        AddProject(line);
                    }
                    else if (line.StartsWith("\tGlobalSection(ProjectDependencies)")) {
                        projectDependencies = true;
                    }
                    else if (projectDependencies && line.StartsWith("\tEndGlobalSection")) {
                        projectDependencies = false;
                    }
                    else if (projectDependencies) {
                        AddDependency(line);
                    }
                }
            }
            finally {
                if (streamReader != null) {
                    streamReader.Close();
                }
            }
        }

        private Hashtable _Projects = new Hashtable();

        string commonProjectId = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
        string enterproseProjectId = "{FE3BBBB6-72D5-11D2-9ACE-00C04F79A2A4}";

        private void AddProject(string projectLine) {
            string pattern = @"^Project\(""(?<unknown>\S+)""\) = ""(?<name>\S+)"", ""(?<path>\S+)"", ""(?<id>\S+)""";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(projectLine);

            if (match.Success) {
                string unknown = match.Groups["unknown"].Value;
                string name = match.Groups["name"].Value;
                string path = match.Groups["path"].Value;
                string id = match.Groups["id"].Value;

                path = ResolvePath(path);

                if (unknown == commonProjectId) {
                    Project project = new Project(this, new Guid(id), name);

                    string absoluteProjectPath = Path.Combine(SolutionDirectory, path);
                    project.Read(absoluteProjectPath);

                    string relativeProjectPath = Path.GetDirectoryName(path);
                    project.RelativePath = relativeProjectPath;

                    if (project.ProjectType == "C# Local" ||
                        project.ProjectType == "C# Web" ||
                        project.ProjectType == "VB Local" ||
                        project.ProjectType == "VB Web") {
                        _Projects.Add(project.ID, project);
                    }
                } 
                else if (unknown == enterproseProjectId) {
                    EnterpriseProject etpProject = new EnterpriseProject(this, new Guid(id), name);
                    string absoluteProjectPath = Path.Combine(SolutionDirectory, path);
                    etpProject.Read(absoluteProjectPath);

                    // get the list of projects from enterprise projects
                    foreach(Project project in etpProject.GetProjects()) {
                        _Projects.Add(project.ID, project);
                    }
                }
            }
        }

        public string ResolvePath(string path) {
            if (path.StartsWith("http:")) {
                if (_UriMap != null) {
                    foreach (DictionaryEntry entry in _UriMap) {
                        if (path.ToLower().StartsWith(((string)entry.Key).ToLower())) {
                            return entry.Value + path.Substring(((string)entry.Key).Length);
                        }
                    }
                }

                throw new ApplicationException("a prefix mapping needs to be specified for " + path);
            }

            return path;
        }

        private void AddDependency(string dependencyLine) {
            string pattern = @"^\t\t(?<source>\{\S+}).\d+ = (?<target>\S+)";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(dependencyLine);

            if (match.Success) {
                string source = match.Groups["source"].Value;
                string target = match.Groups["target"].Value;

                Project sourceProject = _Projects[new Guid(source)] as Project;
                Project targetProject = _Projects[new Guid(target)] as Project;

                if (sourceProject != null && targetProject != null) {
                    AddDependency(sourceProject, targetProject);
                }
            }
        }

        private Hashtable _Dependencies = new Hashtable();

        private void AddDependency(Project source, Project target) {
            ArrayList dependencies = _Dependencies[source.ID] as ArrayList;

            if (dependencies == null) {
                dependencies = new ArrayList();
                _Dependencies.Add(source.ID, dependencies);
            }
            else if (!dependencies.Contains(target)) {
                dependencies.Add(target);
            }
        }

        /// <summary>Gets the project with the specified GUID.</summary>
        /// <param name="id">The GUID used to identify the project in the .sln file.</param>
        /// <returns>The project.</returns>
        public Project GetProject(Guid id) {
            return (Project)_Projects[id];
        }

        /// <summary>Gets the project with the specified name.</summary>
        /// <param name="name">The project name.</param>
        /// <returns>The project.</returns>
        public Project GetProject(string name) {
            foreach (Project project in _Projects.Values) {
                if (project.Name == name) {
                    return project;
                }
            }

            return null;
        }

        /// <summary>Allows you to enumerate (using foreach) over the
        /// solution's projects.</summary>
        /// <returns>An enumerable list of projects.</returns>
        public IEnumerable GetProjects() {
            return _Projects.Values;
        }

        /// <summary>Returns <see langword="true"/> if the specified
        /// <paramref name="source"/> project depends on the
        /// specified <paramref name="target"/> project.</summary>
        /// <param name="source">The source project.</param>
        /// <param name="target">The target project.</param>
        /// <returns><see langword="true"/> or <see langword="false"/>.</returns>
        public bool IsDependant(Project source, Project target) {
            bool result = false;

            ArrayList dependencies = _Dependencies[source.ID] as ArrayList;

            if (dependencies != null && dependencies.Contains(target)) {
                result = true;
            }
            else if (dependencies != null) {
                foreach (Project project in dependencies) {
                    if (IsDependant(source, project)) {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>Gets a list of the projects that the
        /// <paramref name="source"/> project depends on.</summary>
        /// <param name="source">The source project.</param>
        /// <returns>An ArrayList of projects.</returns>
        public ArrayList GetDependencies(Project source) {
            ArrayList result = null;

            ArrayList dependencies = _Dependencies[source.ID] as ArrayList;

            if (dependencies == null) {
                result = new ArrayList();
            }
            else {
                result = dependencies;
            }

            return result;
        }

        /// <summary>Gets all the dependencies of the specified
        /// <paramref name="source"/> project including referenced
        /// projects.</summary>
        /// <param name="source">The source project.</param>
        /// <returns>An ArrayList of projects.</returns>
        public ArrayList GetAllDependencies(Project source) {
            ArrayList dependencies = GetDependencies(source);

            foreach (Reference reference in source.GetReferences()) {
                if (reference.Type == "Project") {
                    Project target = GetProject(reference.Value);

                    if (!dependencies.Contains(target)) {
                        dependencies.Add(target);
                    }
                }
            }

            return dependencies;
        }

        public ArrayList GetConfigurationNames() {
            ArrayList configurationNames = new ArrayList();

            foreach (Project project in _Projects.Values) {
                foreach (Configuration configuration in project.GetConfigurations()) {
                    if (!configurationNames.Contains(configuration.Name)) {
                        configurationNames.Add(configuration.Name);
                    }
                }
            }

            return configurationNames;
        }
    }
}
