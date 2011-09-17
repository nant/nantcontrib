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
// Tomas Restrepo (tomasr@mvps.org)

using System;
using System.Globalization;
using System.IO;
using System.Management;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.BizTalk {
    /// <summary>
    /// Allows stopping, starting and resetting of BizTalk in-process host 
    /// instances on the specified server.
    /// </summary>
    [TaskName("btsreset")]
    public class Reset : Task {
        /// <summary>
        /// Defines the possible actions that can be performed on the BizTalk 
        /// in-process host instances.
        /// </summary>
        [Flags]
        public enum ResetAction {
            /// <summary>
            /// Stops all in-process host instances.
            /// </summary>
            Stop = 0x01,

            /// <summary>
            /// Starts all in-process host instances.
            /// </summary>
            Start = 0x10,

            /// <summary>
            /// Stops and restarts all in-process host instances.
            /// </summary>
            Reset = Stop | Start
        }

        #region Public Instance Properties

        /// <summary>
        /// The name of the BizTalk server on which to perform the action.
        /// </summary>
        [TaskAttribute("server", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Server {
            get { return _server; }
            set { _server = value; }
        }

        /// <summary>
        /// Specifies the action(s) to perform on the BizTalk host instances. The
        /// default is <see cref="ResetAction.Reset" />.
        /// </summary>
        [TaskAttribute("action")]
        public ResetAction Action {
            get { return _action; }
            set { _action = value; }
        }

        #endregion Public Instance Properties

        #region Private Instance Properties

        private ManagementScope Scope {
            get {
                if (_scope == null) {
                    _scope = new ManagementScope (@"\\" + Server + "\\root\\MicrosoftBizTalkServer");
                }
                return _scope;
            }
        }

        private EnumerationOptions EnumerationOptions {
            get {
                if (_enumerationOptions == null) {
                    _enumerationOptions = new EnumerationOptions();
                    _enumerationOptions.ReturnImmediately = false;
                }
                return _enumerationOptions;
            }
        }

        #endregion Private Instance Properties

        #region Override implementation of Task

        protected override void ExecuteTask() {
            try {
                // we're only interested in in-process instances
                ObjectQuery query = new ObjectQuery("SELECT * FROM MSBTS_HostInstance WHERE HostType = 1");

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher (Scope, query, EnumerationOptions)) {
                    if ((Action & ResetAction.Stop) == ResetAction.Stop) {
                        foreach (ManagementObject hostInstance in searcher.Get()) {
                            if ((uint) hostInstance["ServiceState"] != 1) {
                                Log(Level.Verbose, "Stopping {0}...", hostInstance["HostName"]);
                                hostInstance.InvokeMethod("Stop", null);
                            }
                        }
                    }

                    if ((Action & ResetAction.Start) == ResetAction.Start) {
                        foreach (ManagementObject hostInstance in searcher.Get()) {
                            if ((uint) hostInstance["ServiceState"] == 1) {
                                Log(Level.Verbose, "Starting {0}...", hostInstance["HostName"]);
                                hostInstance.InvokeMethod("Start", null);
                            }
                        }
                    }
                }

                // log success
                Log(Level.Info, "{0} host instances on server \"{1}\"", 
                    GetActionFinish(), Server);
            } catch (Exception ex) {
                throw new BuildException("Reset was not successful.", Location, ex);
            }
        }

        #endregion Override implementation of Task

        #region Private Instance Methods

        private string GetActionFinish() {
            switch (Action) {
                case ResetAction.Reset:
                    return "Resetted";
                case ResetAction.Start:
                    return "Started";
                default:
                    return "Stopped";
            }
        }

        #endregion Private Instance Methods

        #region Private Instance Fields

        private string _server;
        private ResetAction _action = ResetAction.Reset;
        private ManagementScope _scope;
        private EnumerationOptions _enumerationOptions;

        #endregion Private Instance Fields
    }
}


