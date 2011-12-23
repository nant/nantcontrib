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

namespace NAnt.Contrib.Tasks.Web {
    /// <summary>
    /// Allows an IIS application pool to be controlled.
    /// </summary>
    /// <example>
    ///   <para>
    ///   Starts the &quot;StsAdminAppPool&quot; application pool on server
    ///   &quot;SV-ARD-WEB&quot;.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <iisapppool action="Start" pool="StsAdminPool" server="SV-ARD-WEB" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Stops and restarts the &quot;DefaultAppPool&quot; application pool
    ///   on server &quot;SV-ARD-WEB&quot;.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <iisapppool action="Restart" pool="DefaultAppPool" server="SV-ARD-WEB" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("iisapppool")]
    public class ApplicationPool : Task {
        /// <summary>
        /// Defines the actions that can be performed on an application pool.
        /// </summary>
        public enum ApplicationPoolAction {
            /// <summary>
            /// Starts the application pool.
            /// </summary>
            Start = 1,

            /// <summary>
            /// Stops the application pool.
            /// </summary>
            Stop = 2,

            /// <summary>
            /// Stops and restarts the application pool.
            /// </summary>
            Restart = 3,

            /// <summary>
            /// Recycles an application pool.
            /// </summary>
            Recycle = 4
        }

        #region Public Instance Properties

        /// <summary>
        /// The name of the server on which to perform the action. The default
        /// is the local computer.
        /// </summary>
        [TaskAttribute("server", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Server {
            get {
                if (_server == null) {
                    _server = Environment.MachineName;
                }
                return _server;
            }
            set { _server = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The name of the application pool on which to perform the action.
        /// </summary>
        [TaskAttribute("pool", Required=true)]
        public string PoolName {
            get { return _poolName; }
            set { _poolName = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The action that should be performed on the application pool.
        /// </summary>
        [TaskAttribute("action", Required=true)]
        public ApplicationPoolAction Action {
            get { return _action; }
            set { _action = value; }
        }

        #endregion Public Instance Properties

        #region Private Instance Properties
        private ConnectionOptions ConnectionOptions {
            get {
                ConnectionOptions options = new ConnectionOptions();
                options.Authentication = AuthenticationLevel.PacketPrivacy;
                return options;
            }
        }

        private ManagementScope Scope {
            get {
                return new ManagementScope(String.Format(@"\\{0}\root\MicrosoftIISv2", Server), ConnectionOptions);
            }
        }

        #endregion Private Instance Properties

        #region Override implementation of Task

        protected override void ExecuteTask() {
            try {
                ManagementPath path = new ManagementPath(String.Format("IIsApplicationPool='W3SVC/AppPools/{0}'", PoolName));
                ManagementObject pool = new ManagementObject(Scope, path, null);

                if (Action == ApplicationPoolAction.Stop || Action == ApplicationPoolAction.Restart) {
                    Log(Level.Verbose, "Stopping '{0}' on '{1}'...",
                        PoolName, Server);
                    pool.InvokeMethod("Stop", new object[0]);
                }

                if (Action == ApplicationPoolAction.Start || Action == ApplicationPoolAction.Restart) {
                    Log(Level.Verbose, "Starting '{0}' on '{1}'...",
                        PoolName, Server);
                    pool.InvokeMethod("Start", new object[0]);
                }

                if (Action == ApplicationPoolAction.Recycle) {
                    Log(Level.Verbose, "Recycling '{0}' on '{1}'...",
                        PoolName, Server);
                    pool.InvokeMethod("Recycle", new object[0]);
                }

                Log(Level.Info, "{0} '{1}' on '{2}'", GetActionFinish(),
                    PoolName, Server);
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Failed {0} application pool '{1}' on '{2}'.",
                    GetActionInProgress(), PoolName, Server), Location, ex);
            }
        }

        #endregion Override implementation of Task

        #region Private Instance Methods

        private string GetActionFinish() {
            switch (Action) {
                case ApplicationPoolAction.Restart:
                    return "Restarted";
                case ApplicationPoolAction.Start:
                    return "Started";
                case ApplicationPoolAction.Stop:
                    return "Stopped";
                default:
                    return "Recycled";
            }
        }

        private string GetActionInProgress() {
            switch (Action) {
                case ApplicationPoolAction.Restart:
                    return "restarting";
                case ApplicationPoolAction.Start:
                    return "starting";
                case ApplicationPoolAction.Stop:
                    return "stopping";
                default:
                    return "recycling";
            }
        }

        #endregion Private Instance Methods

        #region Private Instance Fields

        private string _server;
        private string _poolName;
        private ApplicationPoolAction _action;

        #endregion Private Instance Fields
    }
}
