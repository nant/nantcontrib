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
// Gert Driesen (drieseng@users.sourceforge.net)

using System;
using System.Diagnostics;
using System.Globalization;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;

using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Waits for a given process on the local computer to exit.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   When used in combination with the <see cref="ExecTask" />, it allows
    ///   processed to be spawned for a certain duration or task, and then 
    ///   wait until the process is finished before continueing.
    ///   </para>
    ///   <para>
    ///   When the process identified by <see cref="ProcessId" /> is no longer
    ///   running, then the outcome is considered successful.
    ///   </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Starts two batch processes, and waits for them to finish.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <exec program="batch1.exe" pidproperty="batch1.pid" spawn="true" />
    /// <exec program="batch2.exe" pidproperty="batch2.pid" spawn="true" />
    /// <waitforexit pid="${batch1.pid}" />
    /// <waitforexit pid="${batch2.pid}" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("waitforexit")]
    public class WaitForExitTask : Task {
        #region Private Instance Fields

        private int _processId;
        private int _timeout = Int32.MaxValue;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The unique identifier of the process to wait for.
        /// </summary>
        [TaskAttribute("pid", Required=true)]
        public int ProcessId {
            get { return _processId; }
            set { _processId = value; }
        }

        /// <summary>
        /// The maximum amount of time to wait until the process is exited,
        /// expressed in milliseconds.  The default is to wait indefinitely.
        /// </summary>
        [TaskAttribute("timeout")]
        [Int32Validator()]
        public int TimeOut {
            get { return _timeout; }
            set { _timeout = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        protected override void ExecuteTask() {
            Process process = null;
            try {
                try {
                    process = Process.GetProcessById(ProcessId);
                } catch (ArgumentException) {
                    Log (Level.Verbose, "Process '{0}' was not running.",
                        ProcessId);
                    return;
                }
                bool exited = process.WaitForExit(TimeOut);
                if (!exited) {
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                        "Process '{0}' did not exit within the configured interval.",
                        ProcessId), Location);
                }
            } catch (BuildException) {
                throw;
            } catch (Exception ex) {
                throw new BuildException (string.Format (CultureInfo.InvariantCulture,
                    "Failure waiting for process '{0}' to exit.", ProcessId),
                    Location, ex);
            }
        }

        #endregion Override implementation of Task
    }
}
