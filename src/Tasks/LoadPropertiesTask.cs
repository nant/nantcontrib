//
// NAntContrib
// Copyright (C) 2001-2008 Gerry Shaw
//
// This library is free software; you can redistribute it and/or
// modify under the terms of the GNU Lesser General Public
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
// Gert Driesen (drieseng@users.sourceforge.net)

using System;
using System.Globalization;
using System.IO;
using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Filters;
using NAnt.Core.Tasks;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Loads a file's contents as NAnt properties.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   Each line in a .properties file stores a single property, with the
    ///   name and value separated by an equals sign.
    ///   </para>
    ///   <para>
    ///   Empty lines and lines that start with a '#' character are skipped.
    ///   </para>
    /// </remarks>
    /// <example>
    ///   <code>
    ///     <![CDATA[
    /// <loadproperties file="deployment.properties" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("loadproperties")]
    public class LoadPropertiesTask : Task {
        #region Private Instance Fields

        private Encoding _encoding;
        private FileInfo _file;
        private FilterChain _filters;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The encoding to use when loading the file. The default is the
        /// system's current ANSI code page.
        /// </summary>
        [TaskAttribute("encoding")]
        public Encoding Encoding {
            get { return _encoding; }
            set { _encoding = value; }
        }

        /// <summary>
        /// The file to load properties from.
        /// </summary>
        [TaskAttribute("file", Required=true)]
        public FileInfo File {
            get { return _file; }
            set { _file = value; }
        }

        /// <summary>
        /// Chain of filters used to alter the file's content as it is
        /// copied.
        /// </summary>
        [BuildElement("filterchain")]
        public virtual FilterChain Filters {
            get { return _filters; }
            set { _filters = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        protected override void ExecuteTask() {
            if (!File.Exists)
                throw new BuildException ("The specified properties file " +
                    "does not exist.", Location);

            LoadFile(Filters);
        }

        private void LoadFile(FilterChain filterChain) {
            string content = null;

            try {
                content = FileUtils.ReadFile(File.FullName, filterChain,
                    Encoding);
            } catch (IOException ex) {
                throw new BuildException("The properties file could not be read.",
                    Location, ex);
            }

            PropertyDictionary properties = Project.Properties;

            PropertyTask propertyTask = new PropertyTask();
            propertyTask.Parent = this;
            propertyTask.Project = Project;

            using (StringReader sr = new StringReader(content)) {
                string line = sr.ReadLine();
                int current_line = 0;

                while (line != null) {
                    current_line++;

                    // skip empty lines and comments
                    if (String.IsNullOrEmpty(line) || line.StartsWith ("#")) {
                        line = sr.ReadLine ();
                        continue;
                    }

                    int equals_pos = line.IndexOf ('=');
                    if (equals_pos == -1)
                        throw new BuildException (string.Format(CultureInfo.InvariantCulture,
                            "Invalid property defined on line {0}.",  current_line),
                            Location);

                    string name = line.Substring(0, equals_pos).Trim();
                    string value = line.Substring (equals_pos + 1, 
                        line.Length - equals_pos - 1).Trim();

                    string expandedValue = properties.ExpandProperties(value,
                        Location);

                    propertyTask.PropertyName = name;
                    propertyTask.Value = expandedValue;
                    propertyTask.Execute();

                    line = sr.ReadLine ();
                }
            }
        }

        #endregion Override implementation of Task
    }
}

