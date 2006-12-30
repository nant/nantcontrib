#region GNU General Public License 
//
// NAntContrib
// Copyright (C) 2001-2003 Gerry Shaw
//
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
// Rob Jefferies (rob@houstonsigamchi.org)
#endregion

using System;
using System.Globalization;
using System.IO;
using System.Text;

using SourceSafeTypeLib;

using NAnt.Core;
using NAnt.Core.Types;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.SourceSafe {
    /// <summary>
    /// Used to add files to a Visual SourceSafe database.  If the file is currently
    /// in the SourceSafe database a message will be logged but files will continue to be added.
    /// </summary>
    /// <remarks>
    /// This version does not support recursive adds.  Only adds in the root directory will be added to the
    /// SourceSafe database.
    /// </remarks>
    /// <example>
    ///   <code><![CDATA[
    ///     <vssadd dbpath="C:\SourceSafeFolder\srcsafe.ini" user="user1" password="" path="$/Somefolder">
    ///       <fileset basedir="C:\SourceFolder\">
    ///         <include name="*.dll"/>
    ///       </fileset>
    ///     </vssadd>
    ///   ]]></code>
    /// </example>
    [TaskName("vssadd")]
    public class AddTask : BaseTask {
        #region Private Instance Fields
        
        private string _comment = "";
        private FileSet _fileset = new FileSet();
        
        #endregion Private Instance Fields

        #region Public Instance Properties
        
        /// <summary>
        /// Places a comment on all files added into the SourceSafe repository.
        /// </summary>
        [TaskAttribute("comment", Required=false)]
        public string Comment {
            get { return _comment;}
            set {_comment = value;}
        }

        /// <summary>
        /// List of files that should be added to SourceSafe.
        /// </summary>
        [BuildElement("fileset")]
        public FileSet AddFileSet {
            get { return _fileset; }
            set { _fileset = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        /// <summary>
        /// Main task execution method
        /// </summary>
        protected override void ExecuteTask() {
            Open();

            const int FILE_ALREADY_ADDED = -2147166572;

            // Attempt to add each file to SourceSafe.  If file is already in SourceSafe
            // then log a message and continue otherwise throw an exception.
            foreach (string currentFile in AddFileSet.FileNames) {
                try {
                    IVSSItem actualProject = CreateProjectPath(currentFile);

                    if (actualProject != null) {
                        actualProject.Add(currentFile, Comment, 0);
                    } else {
                        Item.Add(currentFile, Comment, 0);
                    }

                    Log(Level.Info, "Added file '{0}.", currentFile);
                } catch (System.Runtime.InteropServices.COMException ex) {
                    if (ex.ErrorCode == FILE_ALREADY_ADDED) {
                        Log(Level.Warning, "File '{0}' was already added before.", currentFile);
                    } else {
                        throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                            "Failure adding '{0}' to SourceSafe.", currentFile),
                            Location, ex);
                    }
                } catch (Exception ex) {
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                        "Failure adding '{0}' to SourceSafe.", currentFile),
                        Location, ex);
                }
            }
        }

        #endregion Override implementation of Task

        #region Protected Instance Methods

        /// <summary>
        /// Create project hierarchy in vss
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected IVSSItem CreateProjectPath(string file) {
            if (file.StartsWith(AddFileSet.BaseDirectory.FullName)) {
                // Get file path relative to fileset base directory
                string relativePath = file.Replace(AddFileSet.BaseDirectory.FullName, "").Replace('/', '\\');

                // Split path into its component parts; the last part is the filename itself
                string[] projects = relativePath.Split('\\');

                // Now create any requisite subdirectories, if they don't already exist
                IVSSItem currentItem = null;
                string oldPath = Path;

                // Trim final "/" if present (not strictly required)
                if (oldPath.EndsWith("/")) {
                    oldPath = oldPath.Substring(0, oldPath.Length-1);
                }

                for (int i = 0; i < projects.Length-1; i ++) {
                    currentItem = null;
                    string newPath = oldPath + "/" + projects[i];

                    try {
                        // Try to retrieve the VSS directory
                        currentItem = Database.get_VSSItem(newPath, false);
                    } catch {
                        // Create it if it doesn't exist
                        currentItem = Database.get_VSSItem(oldPath, false);
                        currentItem = currentItem.NewSubproject(projects[i], "NAntContrib vssadd" );
                        Log(Level.Info, "Adding VSS Project : " + newPath);
                    }

                    oldPath = newPath;
                }
                return currentItem;
            } else {
                return null;
            }
        }

        #endregion Protected Instance Methods
    }
}
