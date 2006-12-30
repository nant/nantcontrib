// NAnt - A .NET build tool
// Copyright (C) 2001-2007 Gerry Shaw
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
// Gert Driesen (gert.driesen@ardatis.com)

using System;
using System.Diagnostics;
using System.Globalization;

using NAnt.Core.Attributes;
using NAnt.Core.Util;

namespace NAnt.Core.Tasks {
    /// <summary>
    /// Immediately stops a given process.
    /// </summary>
    [TaskName("kill")]
    public class KillTask : Task {
        #region Private Instance Fields

        private int _processId;
        private string _machine;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The unique identifier of the process to stop.
        /// </summary>
        [TaskAttribute("pid", Required=true)]
        public int ProcessId {
            get { return _processId; }
            set { _processId = value; }
        }

        /// <summary>
        /// The name of the computer on the network on which the process must
        /// be stopped. The default is the local computer.
        /// </summary>
        [TaskAttribute("machine")]
        public string Machine {
            get { 
                if (_machine == null) {
                    return Environment.MachineName;
                }
                return _machine;
            }
            set { _machine = StringUtils.ConvertEmptyToNull(value); }
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        protected override void ExecuteTask() {
            try {
                Process process = Process.GetProcessById(ProcessId, Machine);
                process.Kill();
                process.WaitForExit ();
            } catch (Exception ex) {
                throw new BuildException (string.Format (CultureInfo.InvariantCulture,
                    "Process '{0}' could not be stopped on '{1}'.", ProcessId,
                    Machine), Location, ex);
            }
        }

        #endregion Override implementation of Task
    }
}
