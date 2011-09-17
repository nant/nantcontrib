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
    /// Allows BizTalk receive locations to be controlled.
    /// </summary>
    /// <example>
    ///   <para>
    ///   Enables the &quot;HttpReceive&quot; receive location on server
    ///   &quot;SV-ARD-EAI&quot;.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <btsreceivelocation action="Enable" location="HttpReceive" server="SV-ARD-EAI" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Disables the &quot;HttpReceive&quot; receive location on server
    ///   &quot;SV-ARD-EAI&quot;.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <btsreceivelocation action="Disable" location="HttpReceive" server="SV-ARD-EAI" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("btsreceivelocation")]
    public class ReceiveLocation : Task {
        /// <summary>
        /// Defines the actions that can be performed on a BizTalk receive
        /// location.
        /// </summary>
        public enum ReceiveLocationAction {
            /// <summary>
            /// Enables the receive location.
            /// </summary>
            Enable = 1,

            /// <summary>
            /// Disables the receive location.
            /// </summary>
            Disable = 2
        }

        #region Public Instance Properties

        /// <summary>
        /// The name of the receive location on which the perform the action.
        /// </summary>
        [TaskAttribute("location", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string LocationName {
            get { return _locationName; }
            set { _locationName = StringUtils.ConvertEmptyToNull(value); }
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
        /// The action that should be performed on the receive location.
        /// </summary>
        [TaskAttribute("action", Required=true)]
        public ReceiveLocationAction Action {
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
                ObjectQuery query = new ObjectQuery("SELECT * FROM MSBTS_ReceiveLocation"
                    + " WHERE Name = " + SqlQuote(LocationName));

                EnumerationOptions enumerationOptions = new EnumerationOptions();
                enumerationOptions.ReturnImmediately = false;

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(Scope, query, enumerationOptions)) {
                    ManagementObjectCollection receiveLocations = searcher.Get();

                    // ensure we found a matching receive location
                    if (receiveLocations.Count == 0) {
                        throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                            "Receive location \"{0}\" does not exist on \"{1}\".", 
                            LocationName, Server), Location);
                    }

                    // perform actions on each matching receive location
                    foreach (ManagementObject receiveLocation in receiveLocations) {
                        switch (Action) {
                            case ReceiveLocationAction.Disable:
                                Log(Level.Verbose, "Disabling \"{0}\" on \"{1}\"...",
                                    LocationName, Server);
                                try {
                                    ManagementBaseObject inParams = receiveLocation.GetMethodParameters("Disable");
                                    receiveLocation.InvokeMethod("Disable", inParams, null);
                                } catch (Exception ex) {
                                    ReportActionFailure("disabling", ex, FailOnError);
                                }
                                break;
                            case ReceiveLocationAction.Enable:
                                Log(Level.Verbose, "Enabling \"{0}\" on \"{1}\"...",
                                    LocationName, Server);
                                try {
                                    ManagementBaseObject inParams = receiveLocation.GetMethodParameters("Enable");
                                    receiveLocation.InvokeMethod("Enable", inParams, null);
                                } catch (Exception ex) {
                                    ReportActionFailure("enabling", ex, FailOnError);
                                }
                                break;
                        }

                        Log(Level.Info, "{0} \"{1}\" on \"{2}\"",
                            GetActionFinish(), LocationName, Server);
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
                "Failed {0} receive location \"{1}\" on \"{2}\".", action,
                LocationName, Server);

            if (throwException) {
                throw new BuildException(msg, Location, ex);
            } else {
                Log(Level.Verbose, msg);
            }
        }

        private string GetActionFinish() {
            switch (Action) {
                case ReceiveLocationAction.Enable:
                    return "Enabled";
                default:
                    return "Disabled";
            }
        }

        private string GetActionInProgress() {
            switch (Action) {
                case ReceiveLocationAction.Enable:
                    return "enabling";
                default:
                    return "disabling";
            }
        }

        #endregion Private Instance Methods

        #region Private Instance Fields

        private string _server;
        private string _locationName;
        private ReceiveLocationAction _action;

        #endregion Private Instance Fields
    }
}
