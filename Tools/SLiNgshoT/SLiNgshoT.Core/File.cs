// File.cs - the File class
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
    /// <summary>Represents a file in a project.</summary>
    public class File {
        private Project _Project;
        private XPathNavigator _Navigator;

        internal File(Project project, XPathNavigator navigator) {
            _Project = project;
            _Navigator = navigator.Clone();
        }

        /// <summary>Gets the relative path to the file (from the
        /// project directory).</summary>
        public string RelativePath {
            get {
                return (string)_Navigator.Evaluate("string(@RelPath)");
            }
        }

        /// <summary>Gets the BuildAction for the file.</summary>
        /// <value>"Compile", "EmbeddedResource", or "Content"</value>
        public string BuildAction {
            get {
                return (string)_Navigator.Evaluate("string(@BuildAction)");
            }
        }

        /// <summary>Gets relative path to the file (from the solution
        /// directory).</summary>
        public string RelativePathFromSolutionDirectory {
            get {
                return Path.Combine(_Project.RelativePath, RelativePath);
            }
        }

        /// <summary>Gets the absolute path to the file.</summary>
        public string AbsolutePath {
            get {
                return Path.Combine(
                    _Project.Solution.SolutionDirectory,
                    RelativePathFromSolutionDirectory);
            }
        }

        /// <summary>Gets the "default" name for this resource.</summary>
        /// <remarks>This is usually the RootNamespace plus the relative
        /// path to the file with all backslashes replaced with dots.</remarks>
        public string ResourceName {
            get {
                return _Project.RootNamespace +
                    "." +
                    RelativePath.Replace('\\', '.');
            }
        }
    }
}
