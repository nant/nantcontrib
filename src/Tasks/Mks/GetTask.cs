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
// Paul Francis, Edenbrook. (paul.francis@edenbrook.co.uk)
#endregion

using System;
using System.Collections;
using System.IO;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.Mks {
    /// <summary>
    /// Retrieves an item or project from MKS Source Integrity.
    /// </summary>
    /// <example>
    ///   <para>Synchronise sandbox with MKS project.</para>
    ///   <code><![CDATA[
    ///     <mksget
    ///       username="myusername"
    ///       password="mypassword"
    ///       host="servername"
    ///       port="123"
    ///       localpath="c:\sourcecode"
    ///       project="e:/MKS projects/myproject/testproject.pj"
    ///     />
    ///   ]]></code>
    /// </example>
    [TaskName("mksget")]
    public sealed class GetTask : BaseTask {
        #region Prviate Instance Fields

        private DirectoryInfo _localPath;
        private string _projectName;

        #endregion Prviate Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The path to the local working directory.
        /// </summary>
        [TaskAttribute("localpath", Required=true)]
        public DirectoryInfo LocalPath {
            get { return _localPath; }
            set { _localPath = value; }
        }

        /// <summary>
        /// The project to get from MKS.
        /// </summary>
        [TaskAttribute("project",Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string ProjectName {
            get { return _projectName; }
            set { _projectName = value; }
        }
      
        #endregion Public Instance Properties

        #region Override implementation of Task

        protected override void ExecuteTask() {
            Open();

            try {
                // create sandbox
                MKSExecute("createsandbox -Y -P '" + ProjectName + "' " 
                    + LocalPath.FullName);
            } catch (BuildException ex) {
                if (ex.RawMessage == "128") {
                    //sandbox already exists so resync it instead
                    FileInfo fi = _localPath.GetFiles(ProjectName.Substring(ProjectName.LastIndexOf("/") + 1))[0];
                    MKSExecute("resync --quiet -f -Y -S '" + fi.FullName + "'");
                } else {
                    throw ex;
                }
            }
        }

        #endregion Override implementation of Task
    }
}
