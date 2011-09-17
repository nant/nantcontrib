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
using System.Collections;
using System.Globalization;
using System.Management;

using NAnt.Core;
using NAnt.Core.Attributes;

using NAnt.Contrib.Types.BizTalk;

namespace NAnt.Contrib.Tasks.BizTalk {
    /// <summary>
    /// Performs a set of actions on a given orchestration.
    /// </summary>
    [TaskName("btsorchestration")]
    public class Orchestration : Task {
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
        /// The name of the orchestration to perform an action on.
        /// </summary>
        [TaskAttribute("name", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string OrchestrationName {
            get { return _orchestrationName; }
            set { _orchestrationName = value; }
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
                ObjectQuery query = new ObjectQuery("SELECT * FROM MSBTS_Orchestration"
                    + " WHERE Name = " + SqlQuote(OrchestrationName));

                EnumerationOptions enumerationOptions = new EnumerationOptions();
                enumerationOptions.ReturnImmediately = false;

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(Scope, query, enumerationOptions)) {
                    ManagementObjectCollection orchestrations = searcher.Get();

                    // ensure we found a matching orchestration
                    if (orchestrations.Count == 0) {
                        throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                            "Orchestration \"{0}\" does not exist on \"{0}\".", 
                            OrchestrationName, Server), Location);
                    }

                    // perform actions on each matching orchestration
                    foreach (ManagementObject orchestration in orchestrations) {
                        try {
                            // invoke each action in the order in which they
                            // were added (in the build file)
                            foreach (IOrchestrationAction action in _actions) {
                                action.Invoke(orchestration);
                            }
                        } catch (Exception ex) {
                            if (FailOnError) {
                                throw;
                            }

                            // log exception and continue processing the actions
                            // for the next orchestration
                            Log(Level.Error, ex.Message);
                        }
                    }
                }
            } catch (BuildException) {
                throw;
            } catch (Exception ex) {
                throw new BuildException("Error looking up orchestration(s).", 
                    Location, ex);
            }
        }

        #endregion Override implementation of Task

        #region Public Instance Methods

        [BuildElement("start")]
        public void AddStartAction(StartOrchestrationAction start) {
            _actions.Add(start);
        }

        [BuildElement("stop")]
        public void AddStopAction(StopOrchestrationAction stop) {
            _actions.Add(stop);
        }

        [BuildElement("enlist")]
        public void AddEnlistAction(EnlistOrchestrationAction enlist) {
            _actions.Add(enlist);
        }

        [BuildElement("unenlist")]
        public void AddUnenlistAction(UnenlistOrchestrationAction unenlist) {
            _actions.Add(unenlist);
        }


        #endregion Public Instance Methods

        #region Private Static Methods

        private static string SqlQuote(string value) {
            return "'" + value.Replace("'", "''") + "'";
        }

        #endregion Private Static Methods

        #region Private Instance Fields

        private string _server;
        private string _orchestrationName;
        private ArrayList _actions = new ArrayList();

        #endregion Private Instance Fields

        public interface IOrchestrationAction {
            void Invoke(ManagementObject orchestration);
        }

        public abstract class OrchestrationActionBase : Element, IOrchestrationAction {
            #region Override implementation of Element

            /// <summary>
            /// Logs a message with the given priority.
            /// </summary>
            /// <param name="messageLevel">The message priority at which the specified message is to be logged.</param>
            /// <param name="message">The message to be logged.</param>
            /// <remarks>
            /// The actual logging is delegated to the project.
            /// </remarks>
            public override void Log(Level messageLevel, string message) {
                ((Task) Parent).Log(messageLevel, message);
            }

            /// <summary>
            /// Logs a message with the given priority.
            /// </summary>
            /// <param name="messageLevel">The message priority at which the specified message is to be logged.</param>
            /// <param name="message">The message to log, containing zero or more format items.</param>
            /// <param name="args">An <see cref="object" /> array containing zero or more objects to format.</param>
            /// <remarks>
            /// The actual logging is delegated to the project.
            /// </remarks>
            public override void Log(Level messageLevel, string message, params object[] args) {
                ((Task) Parent).Log(messageLevel, message, args);
            }

            #endregion Override implementation of Element

            #region Public Instance Methods

            public abstract void Invoke(ManagementObject orchestration);

            #endregion Public Instance Methods

            #region Protected Instance Methods

            protected string GetName(ManagementObject orchestration) {
                return (string) orchestration["Name"];
            }

            protected ServiceStatus GetStatus(ManagementObject orchestration) {
                uint status = (uint) orchestration["OrchestrationStatus"];
                return (ServiceStatus) Enum.ToObject(typeof(ServiceStatus), status);
            }

            protected void Enlist(ManagementObject orchestration, string hostName) {
                // set-up parameters to pass to method
                ManagementBaseObject inParams = orchestration.GetMethodParameters("Enlist");
                inParams["HostName"] = hostName;
                // enlist the orchestration
                orchestration.InvokeMethod("Enlist", inParams, null);
            }

            #endregion Protected Instance Methods
        }

        /// <summary>
        /// Starts the orchestration.
        /// </summary>
        /// <remarks>
        /// If the orchestration is not yet enlisted, then this will be done 
        /// first.
        /// </remarks>
        public class StartOrchestrationAction : OrchestrationActionBase {
            #region Public Instance Properties

            /// <summary>
            /// Specifies whether receive locations associated with this 
            /// orchestration should be automatically enabled. The default is
            /// <see langword="false" />.
            /// </summary>
            [TaskAttribute("autoenablereceivelocation")]
            public bool AutoEnableReceiveLocation  {
                get { return _autoEnableReceiveLocation; }
                set { _autoEnableReceiveLocation = value; }
            }


            /// <summary>
            /// Specifies whether service instances of this orchestration that 
            /// were manually suspended previously should be automatically 
            /// resumed. The default is <see langword="true" />.
            /// </summary>
            [TaskAttribute("autoresumeorchestration")]
            public bool AutoResumeOrchestrationInstance {
                get { return _autoResumeOrchestrationInstance; }
                set { _autoResumeOrchestrationInstance = value; }
            }

            /// <summary>
            /// Specifies whether send ports and send port groups imported by 
            /// this orchestration should be automatically started. The default
            /// is <see langword="true" />.
            /// </summary>
            [TaskAttribute("autostartsendports")]
            public bool AutoStartSendPorts {
                get { return _autoStartSendPorts; }
                set { _autoStartSendPorts = value; }
            }

            #endregion Public Instance Properties

            #region Implementation of IOrchestrationAction

            public override void Invoke(ManagementObject orchestration) {
                // get name of the orchestration
                string name = GetName(orchestration);

                Log(Level.Verbose, "Starting orchestration \"{0}\"...", name);

                try {
                    ServiceStatus status = GetStatus(orchestration);
                    switch (status) {
                        case ServiceStatus.Started:
                            Log(Level.Verbose, "Orchestration \"{0}\" is already started."
                                + " Skipping.", name);
                            break;
                        case ServiceStatus.Bound:
                            string hostName = (string) orchestration["HostName"];
                            if (hostName.Length == 0) {
                                throw new InvalidOperationException("Cannot enlist"
                                    + " the orchestration if the host is not set.");
                            }
                            // first enlist the orchestration
                            Enlist(orchestration, hostName);
                            // next, start the orchestration
                            goto default;
                        default:
                            // set-up parameters to pass to method
                            ManagementBaseObject inParams = orchestration.GetMethodParameters("Start");
                            inParams["AutoEnableReceiveLocationFlag"] = AutoEnableReceiveLocation ? 2 : 1;
                            inParams["AutoResumeOrchestrationInstanceFlag"] = AutoResumeOrchestrationInstance ? 2 : 1;
                            inParams["AutoStartSendPortsFlag"] = AutoStartSendPorts ? 2 : 1;
                            // start the orchestration
                            orchestration.InvokeMethod("Start", inParams, null);
                            // log success
                            Log(Level.Info, "Started orchestration \"{0}\"", name);
                            break;
                    }
                } catch (Exception ex) {
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                        "Orchestration \"{0}\" could not be started.", name), 
                        Location, ex);
                }
            }

            #endregion Implementation of IOrchestrationAction

            #region Private Instance Fields

            private bool _autoEnableReceiveLocation;
            private bool _autoResumeOrchestrationInstance  = true;
            private bool _autoStartSendPorts = true;

            #endregion Private Instance Fields
        }

        /// <summary>
        /// Stops the orchestration.
        /// </summary>
        /// <remarks>
        /// If the status of the orchestration is <see cref="ServiceStatus.Bound" />,
        /// <see cref="ServiceStatus.Unbound" /> or <see cref="ServiceStatus.Stopped" />,
        /// then no further processing is done.
        /// </remarks>
        public class StopOrchestrationAction : OrchestrationActionBase {
            #region Public Instance Properties

            /// <summary>
            /// Specifies whether receive locations associated with this 
            /// orchestration should be automatically disabled. The default
            /// is <see langword="false" />.
            /// </summary>
            [TaskAttribute("autodisablereceivelocation")]
            public bool AutoDisableReceiveLocation {
                get { return _autoDisableReceiveLocation; }
                set { _autoDisableReceiveLocation = value; }
            }

            /// <summary>
            /// Specifies whether instances of this orchestration should be
            /// automatically suspended. The default is <see langword="true" />.
            /// </summary>
            [TaskAttribute("autosuspendorchestrationinstance")]
            public bool AutoSuspendOrchestrationInstance {
                get { return _autoSuspendOrchestrationInstance; }
                set { _autoSuspendOrchestrationInstance = value; }
            }

            #endregion Public Instance Properties

            #region Implementation of IOrchestrationAction

            /// <summary>
            /// Stops the orchestration.
            /// </summary>
            /// <param name="orchestration">The orchestration to stop.</param>
            /// <remarks>
            /// If the status of orchestration is <see cref="ServiceStatus.Bound" />,
            /// <see cref="ServiceStatus.Unbound" /> or <see cref="ServiceStatus.Stopped" />,
            /// then no further processing is done.
            /// </remarks>
            public override void Invoke(ManagementObject orchestration) {
                // get name of the orchestration
                string name = GetName(orchestration);

                Log(Level.Verbose, "Stopping orchestration \"{0}\"...", name);

                try {
                    ServiceStatus status = GetStatus(orchestration);
                    switch (status) {
                        case ServiceStatus.Unbound:
                            Log(Level.Verbose, "Orchestration \"{0}\" is not bound."
                                + " Skipping.", name);
                            break;
                        case ServiceStatus.Bound:
                            Log(Level.Verbose, "Orchestration \"{0}\" is not started."
                                + " Skipping.", name);
                            break;
                        case ServiceStatus.Stopped:
                            Log(Level.Verbose, "Orchestration \"{0}\" is already stopped."
                                + " Skipping.", name);
                            break;
                        default:
                            // set-up parameters to pass to method
                            ManagementBaseObject inParams = orchestration.GetMethodParameters("Stop");
                            inParams["AutoDisableReceiveLocationFlag"] = AutoDisableReceiveLocation ? 2 : 1;
                            inParams["AutoSuspendOrchestrationInstanceFlag"] = AutoSuspendOrchestrationInstance ? 2 : 1;
                            // stop the orchestration
                            orchestration.InvokeMethod("Stop", inParams, null);
                            // log success
                            Log(Level.Info, "Stopped orchestration \"{0}\"", name);
                            break;
                    }
                } catch (Exception ex) {
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                        "Orchestration \"{0}\" could not be stopped.", name), 
                        Location, ex);
                }
            }

            #endregion Implementation of IOrchestrationAction

            #region Private Instance Fields

            private bool _autoDisableReceiveLocation;
            private bool _autoSuspendOrchestrationInstance = true;

            #endregion Private Instance Fields
        }

        /// <summary>
        /// Enlists the orchestration by creating its activation subscription.
        /// </summary>
        public class EnlistOrchestrationAction : OrchestrationActionBase {
            #region Public Instance Properties

            /// <summary>
            /// Specifies the BizTalk host on which to enlist the orchestration.
            /// </summary>
            [TaskAttribute("host", Required=true)]
            [StringValidator(AllowEmpty=false)]
            public string Host {
                get { return _host; }
                set { _host = value; }
            }

            #endregion Public Instance Properties

            #region Implementation of IOrchestrationAction

            public override void Invoke(ManagementObject orchestration) {
                // get name of the orchestration
                string name = GetName(orchestration);

                Log(Level.Verbose, "Enlisting orchestration \"{0}\"", name);

                try {
                    ServiceStatus status = GetStatus(orchestration);
                    switch (status) {
                        case ServiceStatus.Started:
                            Log(Level.Verbose, "Orchestration \"{0}\" is already started."
                                + " Skipping.", name);
                            break;
                        case ServiceStatus.Stopped:
                            Log(Level.Verbose, "Orchestration \"{0}\" is already enlisted."
                                + " Skipping.", name);
                            break;
                        default:
                            // enlist the orchestration
                            Enlist(orchestration, Host);
                            // log success
                            Log(Level.Info, "Enlisted orchestration \"{0}\"", name);
                            break;
                    }
                } catch (Exception ex) {
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                        "Orchestration \"{0}\" could not be enlisted.", name), 
                        Location, ex);
                }
            }

            #endregion Implementation of IOrchestrationAction

            #region Private Instance Fields

            private string _host;

            #endregion Private Instance Fields
        }

        /// <summary>
        /// Unenlists the orchestration by removing its activation subscription.
        /// </summary>
        public class UnenlistOrchestrationAction : OrchestrationActionBase {
            #region Public Instance Properties

            /// <summary>
            /// Specifies whether instances of this orchestration type should 
            /// be automatically terminated. The default is <see langword="false" />.
            /// </summary>
            [TaskAttribute("autoterminateorchestrationinstance")]
            public bool AutoTerminateOrchestrationInstance {
                get { return _autoTerminateOrchestrationInstance; }
                set { _autoTerminateOrchestrationInstance = value; }
            }

            #endregion Public Instance Properties

            #region Implementation of IOrchestrationAction

            public override void Invoke(ManagementObject orchestration) {
                // get name of the orchestration
                string name = GetName(orchestration);

                Log(Level.Verbose, "Unenlisting orchestration \"{0}\"...", name);

                try {
                    ServiceStatus status = GetStatus(orchestration);
                    switch (status) {
                        case ServiceStatus.Bound:
                        case ServiceStatus.Unbound:
                            Log(Level.Verbose, "Orchestration \"{0}\" is not enlisted."
                                + " Skipping.", name);
                            break;
                        case ServiceStatus.Started:
                        case ServiceStatus.Stopped:
                            // set-up parameters to pass to method
                            ManagementBaseObject inParams = orchestration.GetMethodParameters("Unenlist");
                            inParams["AutoTerminateOrchestrationInstanceFlag"] = AutoTerminateOrchestrationInstance ? 2 : 1;
                            // unenlist the orchestration
                            orchestration.InvokeMethod("Unenlist", inParams, null);
                            // log success
                            Log(Level.Info, "Unenlisted orchestration \"{0}\"", name);
                            break;
                    }
                } catch (Exception ex) {
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                        "Orchestration \"{0}\" could not be unenlisted.", name), 
                        Location, ex);
                }
            }

            #endregion Implementation of IOrchestrationAction

            #region Private Instance Fields

            private bool _autoTerminateOrchestrationInstance;

            #endregion Private Instance Fields
        }
    }
}
