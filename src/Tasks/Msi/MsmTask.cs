//
// NAntContrib
//
// Copyright (C) 2004 Kraen Munck (kmc@innomate.com)
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
//

using System;
using System.Xml;

using NAnt.Core.Attributes;

using NAnt.Contrib.Schemas.Msi;

namespace NAnt.Contrib.Tasks.Msi {
    /// <summary>
    /// Builds a Windows Installer Merge Module (MSM) database.
    /// </summary>
    /// <remarks>
    /// Requires <c>cabarc.exe</c> in the path.  This tool is included in the 
    /// Microsoft Cabinet SDK.
    /// </remarks>
    [TaskName("msm")]
    [SchemaValidator(typeof(msm), "NAnt.Contrib.Tasks.Msi.MsiTask")]
    public class MsmTask : SchemaValidatedTask {
        #region Private Instance Fields

        private MsmCreationCommand _taskCommand;

        #endregion Private Instance Fields

        #region Override implementation of SchemaValidatedTask

        /// <summary>
        /// Initializes task and verifies parameters.
        /// </summary>
        /// <param name="TaskNode">Node that contains the XML fragment used to define this task instance.</param>
        protected override void InitializeTask(XmlNode TaskNode) {
            base.InitializeTask(TaskNode);

            _taskCommand = new MsmCreationCommand((msm) SchemaObject, this, 
                this.Location, this.XmlNode);
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        protected override void ExecuteTask() {
            _taskCommand.Execute();
        }

        #endregion Override implementation of SchemaValidatedTask
    }
}
