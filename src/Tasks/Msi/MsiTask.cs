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
    /// Builds a Windows Installer (MSI) File.
    /// </summary>
    /// <remarks>
    /// Requires <c>cabarc.exe</c> in the path.  This tool is included in the 
    /// Microsoft Cabinet SDK.
    /// </remarks>
    [TaskName("msi")]
    [SchemaValidator(typeof(msi))]
    public class MsiTask : SchemaValidatedTask {
        MsiCreationCommand taskCommand;

        /// <summary>
        /// Initialize taks and verify parameters.
        /// </summary>
        /// <param name="TaskNode">Node that contains the XML fragment
        /// used to define this task instance.</param>
        /// <remarks>None.</remarks>
        protected override void InitializeTask(XmlNode TaskNode) {
            base.InitializeTask(TaskNode);

            taskCommand = new MsiCreationCommand((msi)SchemaObject, this, this.Location, this._xmlNode);
        }

        /// <summary>
        /// Executes the Task.
        /// </summary>
        /// <remarks>None.</remarks>
        protected override void ExecuteTask() {
            taskCommand.Execute();
        }
    }
}
