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

namespace NAnt.Contrib.Tasks.BizTalk {
    /// <summary>
    /// Deploys an assembly to a given BizTalk configuration database.
    /// </summary>
    /// <remarks>
    /// Deployment will fail if the assembly is already deployed.
    /// </remarks>
    [TaskName("btsdeploy")]
    public class Deploy : BizTalkBase {
        #region Public Instance Properties

        /// <summary>
        /// The path to the BizTalk assembly to deploy.
        /// </summary>
        [TaskAttribute("assembly", Required=true)]
        public FileInfo Assembly {
            get { return _assembly; }
            set { _assembly = value; }
        }

        /// <summary>
        /// Determines whether to install the assembly in the Global Assembly
        /// Cache. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("install")]
        public bool Install {
            get { return _install; }
            set { _install = value; }
        }

        /// <summary>
        /// The path to an assembly binding information file to import bindings
        /// from.
        /// </summary>
        [TaskAttribute("bindingfile")]
        public FileInfo BindingFile {
            get { return _bindingFile; }
            set { _bindingFile = value; }
        }

        /// <summary>
        /// The path to the HTML log file to generate.
        /// </summary>
        [TaskAttribute("logfile")]
        public FileInfo LogFile {
            get { return _logFile; }
            set { _logFile = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        /// <summary>
        /// Deploys the assembly.
        /// </summary>
        /// <exception cref="BuildException">
        ///   <para>The assembly does not exist.</para>
        ///   <para>-or-</para>
        ///   <para>The assembly binding information file does not exist.</para>
        ///   <para>-or-</para>
        ///   <para>The assembly could not be deployed.</para>
        /// </exception>
        protected override void ExecuteTask() {
            try {
                Log(Level.Verbose, "Deploying \"{0}\"...", Assembly.Name);

                // ensure assembly exists
                if (!Assembly.Exists) {
                    throw new FileNotFoundException("The assembly does not exist.");
                }

                // ensure assembly binding file
                if (BindingFile != null && !BindingFile.Exists) {
                    throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture,
                        "Assembly binding information file \"{0}\" does not exist.", 
                        BindingFile.FullName));
                }

                ManagementPath path = new ManagementPath("MSBTS_DeploymentService");
                ManagementClass deployClass = new ManagementClass(Scope, path, 
                    new ObjectGetOptions());
            
                ManagementBaseObject inParams = deployClass.GetMethodParameters("Deploy");
                inParams["Server"] = Server;
                inParams["Database"] = Database;
                inParams["Assembly"] = Assembly.FullName;

                // determine whether to install assembly in GAC
                inParams["Install"] = Install;

                // assembly binding information file
                if (BindingFile != null) {
                    inParams["Binding"] = BindingFile.FullName;
                }

                // HTML log file to generate
                if (LogFile != null) {
                    // ensure directory for log file exists
                    if (!LogFile.Directory.Exists) {
                        LogFile.Directory.Create();
                        LogFile.Directory.Refresh();
                    }
                    inParams["Log"] = LogFile.FullName;
                }

                deployClass.InvokeMethod("Deploy", inParams, null);

                // log success
                Log(Level.Info, "Deployed \"{0}\"", Assembly.Name);
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Assembly \"{0}\" could not be deployed.", Assembly.Name),
                    Location, ex);
            }
        }

        #endregion Override implementation of Task

        #region Private Instance Fields

        private FileInfo _assembly;
        private bool _install;
        private FileInfo _bindingFile;
        private FileInfo _logFile;

        #endregion Private Instance Fields
    }
}
