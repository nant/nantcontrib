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
using System.Globalization;
using System.IO;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.Mks {
    /// <summary>
    /// Checkpoints a project in an MKS Source Integrity database.
    /// </summary>
    /// <example>
    ///   <para>Checkpoint a project in an MKS database.</para>
    ///   <code><![CDATA[
    ///     <mkscheckpoint
    ///       username="myusername"
    ///       password="mypassword"
    ///       host="servername"
    ///       port="123"
    ///       project="myproject"
    ///       recursive="false"
    ///       label="test from nant"
    ///       description="this is a test description"
    ///     />
    ///   ]]></code>
    /// </example>
    
    [TaskName("mkscheckpoint")]
    public sealed class CheckpointTask : BaseTask {
        #region Private Instance Fields

        private string _label;
        private string _project;
        private bool _recursive;
        private string _description;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The label to apply to the checkpoint.
        /// </summary>
        [TaskAttribute("label", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Label {
            get { return _label; }
            set { _label = value; }
        }

        /// <summary>
        /// The project to checkpoint.
        /// </summary>
        [TaskAttribute("project", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string ProjectName {
            get { return _project; }
            set { _project = value; }
        }
    
        /// <summary>
        /// Apply label to all members. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("recursive", Required=false)]
        public bool Recursive {
            get { return _recursive; }
            set { _recursive = value; }
        }
       
        /// <summary>
        /// The description of the checkpoint.
        /// </summary>
        [TaskAttribute("description", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Description {
            get { return _description; }
            set { _description = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        protected override void ExecuteTask() {
            Open();

            try {
                string cmd = "checkpoint -L '" + Label + "'";
                if (!Recursive) {
                    cmd += " --nolabelmembers";
                }
                cmd += " -d '" + Description + "'";
                cmd += " -P '" + ProjectName + "'";

                MKSExecute(cmd);
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Failed to checkpoint project \"{0}\".", ProjectName),
                    Location, ex);
            }
        }

        #endregion Override implementation of Task
    }
}
