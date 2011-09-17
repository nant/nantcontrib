//
// NAntContrib
// Copyright (C) 2001-2005 Gerry Shaw
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
// Gert Driesen (drieseng@users.sourceforge.net)

using System;
using System.Globalization;
using System.IO;

using NAnt.Core;
using NAnt.Core.Tasks;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Changes the current working directory.
    /// </summary>
    /// <example>
    ///   <para>
    ///   Changes the current working directory to the &quot;subdir&quot; 
    ///   directory, relative to the project base directory.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <cd dir="subdir" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("cd")]
    public class ChangeDirectory : Task {
        #region Private Instance Fields
        
        private DirectoryInfo _directory;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The path to which the current working directory should be set.
        /// </summary>
        [TaskAttribute("dir", Required=true)]
        public DirectoryInfo Directory {
            get { return _directory; }
            set { _directory = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        /// <summary>
        /// Changes the current directory.
        /// </summary>
        protected override void ExecuteTask() {
            try {
                // change working directory
                System.IO.Directory.SetCurrentDirectory(Directory.FullName);
                // inform user
                Log(Level.Info, "Current directory changed to \"{0}\".", 
                    Directory.FullName);
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Working directory could not be set to \"{0}\".", Directory.FullName),
                    Location, ex);
            }
        }

        #endregion Override implementation of Task
    }
}
