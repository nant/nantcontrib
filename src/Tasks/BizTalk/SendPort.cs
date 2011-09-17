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
    /// Allows BizTalk send ports to be controlled.
    /// </summary>
    /// <example>
    ///   <para>
    ///   Starts the &quot;UmeHttpSendPort&quot; send port on server 
    ///   &quot;SV-ARD-EAI&quot;.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <btssendport action="Start" port="UmeHttpSendPort" server="SV-ARD-EAI" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Unenlists the &quot;UmeHttpSendPort&quot; send port on server
    ///   &quot;SV-ARD-EAI&quot;.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <btssendport action="UnEnlist" port="UmeHttpSendPort" server="SV-ARD-EAI" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("btssendport")]
    public class SendPort : Task {
        /// <summary>
        /// Defines the actions that can be performed on a BizTalk send port.
        /// </summary>
        public enum SendPortAction {
            /// <summary>
            /// Starts the send port.
            /// </summary>
            Start = 1,

            /// <summary>
            /// Stops the send port.
            /// </summary>
            Stop = 2,

            /// <summary>
            /// Stops and restarts the send port.
            /// </summary>
            Restart = 3,

            /// <summary>
            /// Enlists the send port.
            /// </summary>
            Enlist = 4,

            /// <summary>
            /// Unenlists the send port.
            /// </summary>
            UnEnlist = 5
        }

        #region Public Instance Properties

        /// <summary>
        /// The name of the send port on which the perform the action.
        /// </summary>
        [TaskAttribute("port", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string PortName {
            get { return _portName; }
            set { _portName = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The name of the BizTalk server on which to perform the action.
        /// </summary>
        [TaskAttribute("server", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Server {
            get { return _server; }
            set { _server = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The action that should be performed on the send port.
        /// </summary>
        [TaskAttribute("action", Required=true)]
        public SendPortAction Action {
            get { return _action; }
            set { _action = value; }
        }

        #endregion Public Instance Properties

        #region Private Instance Properties

        private ManagementScope Scope {
            get {
                return new ManagementScope (@"\\" + Server + "\\root\\MicrosoftBizTalkServer");
            }
        }

        #endregion Private Instance Properties

        #region Override implementation of Task

        protected override void ExecuteTask() {
            try {
                ObjectQuery query = new ObjectQuery("SELECT * FROM MSBTS_SendPort"
                    + " WHERE Name = " + SqlQuote(PortName));

                EnumerationOptions enumerationOptions = new EnumerationOptions();
                enumerationOptions.ReturnImmediately = false;

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(Scope, query, enumerationOptions)) {
                    ManagementObjectCollection sendPorts = searcher.Get();

                    // ensure we found a matching send port
                    if (sendPorts.Count == 0) {
                        throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                            "Send port \"{0}\" does not exist on \"{1}\".", 
                            PortName, Server), Location);
                    }

                    // perform actions on each matching send port
                    foreach (ManagementObject sendPort in sendPorts) {
                        if (Action == SendPortAction.Stop || Action == SendPortAction.Restart) {
                            Log(Level.Verbose, "Stopping \"{0}\" on \"{1}\"...",
                                PortName, Server);
                            try {
                                ManagementBaseObject inParams = sendPort.GetMethodParameters("Stop");
                                sendPort.InvokeMethod("Stop", inParams, null);
                            } catch (Exception ex) {
                                ReportActionFailure("stopping", ex, FailOnError);
                            }
                        }

                        if (Action == SendPortAction.Start || Action == SendPortAction.Restart) {
                            Log(Level.Verbose, "Starting \"{0}\" on \"{1}\"...",
                                PortName, Server);
                            try {
                                ManagementBaseObject inParams = sendPort.GetMethodParameters("Start");
                                sendPort.InvokeMethod("Start", inParams, null);
                            } catch (Exception ex) {
                                ReportActionFailure("starting", ex, FailOnError);
                            }
                        }

                        if (Action == SendPortAction.Enlist) {
                            Log(Level.Verbose, "Enlisting \"{0}\" on \"{1}\"...",
                                PortName, Server);
                            try {
                                ManagementBaseObject inParams = sendPort.GetMethodParameters("Enlist");
                                sendPort.InvokeMethod("Enlist", inParams, null);
                            } catch (Exception ex) {
                                ReportActionFailure("enlisting", ex, FailOnError);
                            }
                        }

                        if (Action == SendPortAction.UnEnlist) {
                            Log(Level.Verbose, "Unenlisting \"{0}\" on \"{1}\"...",
                                PortName, Server);
                            try {
                                ManagementBaseObject inParams = sendPort.GetMethodParameters("UnEnlist");
                                sendPort.InvokeMethod("UnEnlist", inParams, null);
                            } catch (Exception ex) {
                                ReportActionFailure("unenlisting", ex, FailOnError);
                            }
                        }

                        Log(Level.Info, "{0} \"{1}\" on \"{2}\"",
                            GetActionFinish(), PortName, Server);
                    }
                }
            } catch (BuildException) {
                throw;
            } catch (Exception ex) {
                ReportActionFailure(GetActionInProgress(), ex, true);
            }
        }

        #endregion Override implementation of Task

        #region Private Static Methods

        private static string SqlQuote(string value) {
            return "'" + value.Replace("'", "''") + "'";
        }

        #endregion Private Static Methods

        #region Private Instance Methods

        private void ReportActionFailure(string action, Exception ex, bool throwException) {
            string msg = string.Format(CultureInfo.InvariantCulture,
                "Failed {0} send port \"{1}\" on \"{2}\".", action, PortName,
                Server);

            if (throwException) {
                throw new BuildException(msg, Location, ex);
            } else {
                Log(Level.Verbose, msg);
            }
        }

        private string GetActionFinish() {
            switch (Action) {
                case SendPortAction.Restart:
                    return "Restarted";
                case SendPortAction.Start:
                    return "Started";
                case SendPortAction.Enlist:
                    return "Enlisted";
                case SendPortAction.UnEnlist:
                    return "Unenlisted";
                default:
                    return "Stopped";
            }
        }

        private string GetActionInProgress() {
            switch (Action) {
                case SendPortAction.Restart:
                    return "restarting";
                case SendPortAction.Start:
                    return "starting";
                case SendPortAction.Enlist:
                    return "enlisting";
                case SendPortAction.UnEnlist:
                    return "unenlisting";
                default:
                    return "stopping";
            }
        }

        #endregion Private Instance Methods

        #region Private Instance Fields

        private string _server;
        private string _portName;
        private SendPortAction _action;

        #endregion Private Instance Fields
    }
}
