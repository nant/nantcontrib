// Reference.cs - the Reference class
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
    /// <summary>Represents a project reference.</summary>
    public class Reference {
        private Solution _Solution;
        private XPathNavigator _Navigator;

        internal Reference(Solution solution, XPathNavigator navigator) {
            _Solution = solution;
            _Navigator = navigator.Clone();
        }

        /// <summary>Gets a string identifying the type of reference.</summary>
        /// <value>"AssemblyName", "Project" or "Guid"</value>
        public string Type {
            get {
                string type = null;

                if ((bool)_Navigator.Evaluate("boolean(@AssemblyName)")) {
                    type = "AssemblyName";
                }
                else if ((bool)_Navigator.Evaluate("boolean(@Project)")) {
                    type = "Project";
                }
                else if ((bool)_Navigator.Evaluate("boolean(@Guid)")) {
                    type = "Guid";
                }

                return type;
            }
        }

        /// <summary>Gets the "value" of this reference.</summary>
        /// <value>If the reference type is "AssemblyName" this will be the
        /// name of a system assembly (like "System.Xml"). If the reference
        /// type is "Project" this will be the name of the project.
        /// If the type is "Guid", we construct the Interop assembly name</value>
        public string Value {
            get {
                string result = null;

                switch (Type) {
                    case "AssemblyName":
                        result = (string)_Navigator.Evaluate("string(@AssemblyName)");
                        break;
                    case "Project":
                        Project project = _Solution.GetProject(
                            new Guid((string)_Navigator.Evaluate("string(@Project)")));
                        if (project != null) {
                            result = project.Name;
                        }
                        break;
                    case "Guid":    // COM interop
                        result = "Interop." + (string)_Navigator.Evaluate("string(@Name)");
                        break;
                }

                return result;
            }
        }

        /// <summary>Gets whether this reference should be copied to the build directory.</summary>
        /// <value>if the reference's Private= element is set to true, return true, else return false</value>
        public bool CopyLocal {
            get {
                bool res = false;
                if ( (bool)_Navigator.Evaluate("boolean(@Private)") ) {
                    res = Convert.ToBoolean( ((string)_Navigator.Evaluate("string(@Private)") ) );
                }
                return res;
            }
        }

        /// <summary>Gets the reference's source path if present.</summary>
        public string SourcePath {
            get {
                return (string)_Navigator.Evaluate("string(@HintPath)");
            }
        }
    }
}
