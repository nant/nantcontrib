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
using System.Reflection;
using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;

using NAnt.Contrib.Types.BizTalk;

namespace NAnt.Contrib.Tasks.BizTalk {
    /// <summary>
    /// Removes all bindings for a given assembly from a BizTalk configuration
    /// database.
    /// </summary>
    [TaskName("btsunbind")]
    public class Unbind : Task {
        #region Public Instance Properties

        /// <summary>
        /// The path to the BizTalk assembly for which to remove all bindings.
        /// </summary>
        [TaskAttribute("assembly", Required=true)]
        public FileInfo Assembly {
            get { return _assembly; }
            set { _assembly = value; }
        }

        /// <summary>
        /// The name of the BizTalk server on which to perform the operation.
        /// </summary>
        [TaskAttribute("server", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Server {
            get { return _server; }
            set { _server = value; }
        }

        /// <summary>
        /// The assembly qualified name of the receive pipeline to set when 
        /// unbinding a receive pipeline.
        /// </summary>
        [TaskAttribute("receivepipeline", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string ReceivePipeline {
            get { return _receivePipeline; }
            set { _receivePipeline = value; }
        }

        /// <summary>
        /// The assembly qualified name of the SEND pipeline to set when 
        /// unbinding a send pipeline.
        /// </summary>
        [TaskAttribute("sendpipeline", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string SendPipeline {
            get { return _sendPipeline; }
            set { _sendPipeline = value; }
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

        private AssemblyName AssemblyName {
            get {
                if (_assemblyName == null) {
                    _assemblyName = AssemblyName.GetAssemblyName(Assembly.FullName);
                }
                return _assemblyName;
            }
        }

        #endregion Private Instance Properties

        #region Override implementation of Task

        /// <summary>
        /// Removes bindings for the specified assembly.
        /// </summary>
        /// <exception cref="BuildException">
        ///   <para>The assembly does not exist.</para>
        ///   <para>-or-</para>
        ///   <para>The bindings could not be removed.</para>
        /// </exception>
        protected override void ExecuteTask() {
            try {
                Log(Level.Verbose, "Removing bindings for \"{0}\"...", Assembly.Name);

                // ensure assembly exists
                if (!Assembly.Exists) {
                    throw new FileNotFoundException("The assembly does not exist.");
                }

                UnbindReceivePorts();
                UnbindReceiveLocations();
                UnbindSendPorts();
                UnenlistOrchestrations();

                // success message
                Log(Level.Info, "Removed bindings for \"{0}\"", Assembly.Name);
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Bindings could not be removed for assembly \"{0}\".", Assembly.Name),
                    Location, ex);
            }
        }

        #endregion Override implementation of Task

        #region Private Instance Methods

        private void UnbindSendPorts() {
            ObjectQuery query = new ObjectQuery("SELECT * FROM MSBTS_SendPort");

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher (Scope, query, EnumerationOptions)) {
                foreach (ManagementObject sendPort in searcher.Get()) {
                    string sendPipeline = (string) sendPort["SendPipeline"];
                    if (sendPipeline.Length != 0) {
                        if (IsPartOfAssembly(sendPipeline)) {
                            // unbind pipeline
                            sendPort["SendPipeline"] = SendPipeline;
                        }
                    }

                    string receivePipeline = (string) sendPort["ReceivePipeline"];
                    if (receivePipeline.Length != 0) {
                        if (IsPartOfAssembly(receivePipeline)) {
                            // unbind pipeline
                            sendPort["ReceivePipeline"] = ReceivePipeline;
                        }
                    }

                    string inboundTransforms = (string) sendPort["InboundTransforms"];
                    if (inboundTransforms.Length != 0) {
                        sendPort["InboundTransforms"] = CleanTransforms(inboundTransforms);
                    }

                    string outboundTransforms = (string) sendPort["OutboundTransforms"];
                    if (outboundTransforms.Length != 0) {
                        sendPort["OutboundTransforms"] = CleanTransforms(outboundTransforms);
                    }

                    // commit changes
                    sendPort.Put();
                }
            }
        }

        private void UnbindReceivePorts() {
            ObjectQuery query = new ObjectQuery("SELECT * FROM MSBTS_ReceivePort");

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher (Scope, query, EnumerationOptions)) {
                foreach (ManagementObject receivePort in searcher.Get()) {
                    string sendPipeline = (string) receivePort["SendPipeline"];
                    if (sendPipeline.Length != 0) {
                        if (IsPartOfAssembly(sendPipeline)) {
                            // unbind pipeline
                            receivePort["SendPipeline"] = SendPipeline;
                        }
                    }

                    string inboundTransforms = (string) receivePort["InboundTransforms"];
                    if (inboundTransforms.Length != 0) {
                        receivePort["InboundTransforms"] = CleanTransforms(inboundTransforms);
                    }

                    string outboundTransforms = (string) receivePort["OutboundTransforms"];
                    if (outboundTransforms.Length != 0) {
                        receivePort["OutboundTransforms"] = CleanTransforms(outboundTransforms);
                    }

                    // commit changes
                    receivePort.Put();
                }
            }
        }

        private void UnbindReceiveLocations() {
            ObjectQuery query = new ObjectQuery("SELECT * FROM MSBTS_ReceiveLocation");

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher (Scope, query, EnumerationOptions)) {
                foreach (ManagementObject receiveLocation in searcher.Get()) {
                    string receivePipeline = (string) receiveLocation["PipelineName"];
                    if (receivePipeline.Length != 0) {
                        if (IsPartOfAssembly(receivePipeline)) {
                            // unbind pipeline
                            receiveLocation["PipelineName"] = ReceivePipeline;
                        }
                    }

                    // commit changes
                    receiveLocation.Put();
                }
            }
        }

        private void UnenlistOrchestrations() {
            ObjectQuery query = new ObjectQuery("SELECT * FROM MSBTS_Orchestration");

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher (Scope, query, EnumerationOptions)) {
                foreach (ManagementObject orchestration in searcher.Get()) {
                    ServiceStatus status = GetOrchestrationStatus(orchestration);

                    // skip orchestrations that are not enlisted
                    if (status == ServiceStatus.Bound || status == ServiceStatus.Unbound) {
                        continue;
                    }

                    // for now, we ignore the culture and the public key token
                    string assemblyName = (string) orchestration["AssemblyName"];
                    string assemblyVersion = (string) orchestration["AssemblyVersion"];

                    if (AssemblyName.Name != assemblyName) {
                        continue;
                    }

                    if (AssemblyName.Version.ToString() != assemblyVersion) {
                        continue;
                    }

                    ManagementBaseObject inParams = orchestration.GetMethodParameters("Unenlist");
                    inParams["AutoTerminateOrchestrationInstanceFlag"] = 1; // do not termine

                    // unenlist the orchestration
                    orchestration.InvokeMethod("Unenlist", inParams, null);
                }
            }
        }

        private string CleanTransforms(string transforms) {
            StringBuilder sb = new StringBuilder();
            string[] parts = transforms.Split('|');
            foreach (string part in parts) {
                if (!IsPartOfAssembly(part)) {
                    if (sb.Length != 0) {
                        sb.Append('|');
                    }
                    sb.Append(part);
                }
            }
            return sb.ToString();
        }

        private bool IsPartOfAssembly(string artifact) {
            string assemblyName = artifact.Substring(artifact.IndexOf(",") + 1);
            return AssemblyName.FullName == assemblyName.Trim();
        }

        private ServiceStatus GetOrchestrationStatus(ManagementObject orchestration) {
            uint status = (uint) orchestration["OrchestrationStatus"];
            return (ServiceStatus) Enum.ToObject(typeof(ServiceStatus), status);
        }

        #endregion Private Instance Methods

        #region Private Instance Fields

        private string _server;
        private FileInfo _assembly;
        private string _receivePipeline;
        private string _sendPipeline;
        private ManagementScope _scope;
        private EnumerationOptions _enumerationOptions;
        private AssemblyName _assemblyName;

        #endregion Private Instance Fields
    }
}
