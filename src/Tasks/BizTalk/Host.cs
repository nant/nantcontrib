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
using System.Management;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks.BizTalk {
    /// <summary>
    /// Allows BizTalk (in-process) host instances to be controlled.
    /// </summary>
    /// <example>
    ///   <para>
    ///   Starts the &quot;BizTalkServerApplication&quot; host instance
    ///   on server &quot;SV-ARD-EAI&quot;.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <btshost action="Start" host="BizTalkServerApplication" server="SV-ARD-EAI" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Stops all &quot;BizTalkServerApplication&quot; host instances.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <btshost action="Stop" host="BizTalkServerApplication" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("btshost")]
    public class Host : Task {
        /// <summary>
        /// Defines the actions that can be performed on a host instance.
        /// </summary>
        [Flags]
        public enum HostAction {
            /// <summary>
            /// Starts the host instance.
            /// </summary>
            Start = 1,

            /// <summary>
            /// Stops the host instance.
            /// </summary>
            Stop = 2,

            /// <summary>
            /// Stops and restarts the host instance.
            /// </summary>
            Restart = Start | Stop
        }

        #region Public Instance Properties

        /// <summary>
        /// The name of the host on which the perform the action.
        /// </summary>
        [TaskAttribute("host", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string HostName {
            get { return _hostName; }
            set { _hostName = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The name of the BizTalk server on which to perform the action.
        /// If not specified, the action will be performed on all instances.
        /// </summary>
        [TaskAttribute("server", Required=false)]
        [StringValidator(AllowEmpty=false)]
        public string Server {
            get { return _server; }
            set { _server = StringUtils.ConvertEmptyToNull(value); }
        }

		/// <summary>
		/// The action that should be performed on the host.
		/// </summary>
        [TaskAttribute("action", Required=true)]
        public HostAction Action {
            get { return _action; }
            set { _action = value; }
        }

        #endregion Public Instance Properties

        #region Private Instance Properties

        private ManagementScope Scope {
            get {
                return new ManagementScope (@"root\MicrosoftBizTalkServer");
            }
        }

        #endregion Private Instance Properties

        #region Override implementation of Task

        protected override void ExecuteTask() {
            try {
                // we're only interested in in-process instances
                ObjectQuery query = new ObjectQuery("SELECT * FROM MSBTS_HostInstance WHERE HostType = 1");

                EnumerationOptions enumerationOptions = new EnumerationOptions();
                enumerationOptions.ReturnImmediately = false;

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher (Scope, query, enumerationOptions)) {
                    foreach (ManagementObject hostInstance in searcher.Get()) {
                        string hostName = (string) hostInstance["HostName"];
                        string runningServer = (string) hostInstance["RunningServer"];

                        // perform case-insensitive comparison
                        if (string.Compare(hostName, HostName, true, CultureInfo.CurrentCulture) != 0) {
                            continue;
                        }

                        // perform case-insensitive comparison
                        if (Server != null && string.Compare(runningServer, Server, true, CultureInfo.CurrentCulture) != 0) {
                            continue;
                        }
    								
                        if ((Action & HostAction.Stop) == HostAction.Stop) {
                            Log(Level.Verbose, "Stopping \"{0}\" on \"{1}\"...",
                                hostName, runningServer);
                            try {
                                hostInstance.InvokeMethod("Stop", null);
                            } catch (Exception ex) {
                                ReportActionFailure("stopping", runningServer,
                                    ex, FailOnError);
                            }
                        }
    								
                        if ((Action & HostAction.Start) == HostAction.Start) {
                            Log(Level.Verbose, "Starting \"{0}\" on \"{1}\"...",
                                hostName, runningServer);
                            try {
                                hostInstance.InvokeMethod("Start", null);
                            } catch (Exception ex) {
                                ReportActionFailure("starting", runningServer,
                                    ex, FailOnError);
                            }
                        }

                        Log(Level.Info, "{0} \"{1}\" on \"{2}\"",
                            GetActionFinish(), hostName, runningServer);
                    }
                }
            } catch (BuildException) {
                throw;
            } catch (Exception ex) {
                ReportActionFailure(GetActionInProgress(), Server, ex, true);
            }
        }

        #endregion Override implementation of Task

        #region Private Instance Methods

        private void ReportActionFailure(string action, string server, Exception ex, bool throwException) {
            string msg = string.Format(CultureInfo.InvariantCulture,
                "Failed {0} host \"{1}\"", action, HostName);
            if (server != null) {
                msg += " on \"" + server + "\".";
            } else {
                msg += ".";
            }

            if (throwException) {
                throw new BuildException(msg, Location, ex);
            } else {
                Log(Level.Verbose, msg);
            }
        }

        private string GetActionFinish() {
            switch (Action) {
                case HostAction.Restart:
                    return "Restarted";
                case HostAction.Start:
                    return "Started";
                default:
                    return "Stopped";
            }
        }

        private string GetActionInProgress() {
            switch (Action) {
                case HostAction.Restart:
                    return "restarting";
                case HostAction.Start:
                    return "starting";
                default:
                    return "stopping";
            }
        }

        #endregion Private Instance Methods

        #region Private Instance Fields

        private string _server;
        private string _hostName;
        private HostAction _action;

        #endregion Private Instance Fields
    }
}
