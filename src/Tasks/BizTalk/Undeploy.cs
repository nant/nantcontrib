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
    /// Removes a given assembly from a BizTalk configuration database.
    /// </summary>
    [TaskName("btsundeploy")]
    public class Undeploy : BizTalkBase {
        #region Public Instance Properties

        /// <summary>
        /// The path to the BizTalk assembly to remove.
        /// </summary>
        [TaskAttribute("assembly", Required=true)]
        public FileInfo Assembly {
            get { return _assembly; }
            set { _assembly = value; }
        }

        /// <summary>
        /// Determines whether to remove the assembly from the Global Assembly
        /// Cache. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("uninstall")]
        public bool Uninstall {
            get { return _uninstall; }
            set { _uninstall = value; }
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
        /// Removes an assembly from a BizTalk configuration database.
        /// </summary>
        /// <exception cref="BuildException">
        ///   <para>The assembly does not exist.</para>
        ///   <para>-or-</para>
        ///   <para>The assembly could not be remove from the BizTalk configuration database.</para>
        /// </exception>
        protected override void ExecuteTask() {
            try {
                Log(Level.Verbose, "Undeploying \"{0}\"...", Assembly.Name);

                // ensure assembly exists
                if (!Assembly.Exists) {
                    throw new FileNotFoundException("The assembly does not exist.");
                }

                ManagementPath path = new ManagementPath("MSBTS_DeploymentService");
                ManagementClass deployClass = new ManagementClass(Scope, path, 
                    new ObjectGetOptions());
            
                ManagementBaseObject inParams = deployClass.GetMethodParameters("Remove");
                inParams["Server"] = Server;
                inParams["Database"] = Database;
                inParams["Assembly"] = Assembly.FullName;

                // determine whether to uninstall assembly from GAC
                inParams["UnInstall"] = Uninstall;

                // HTML log file to generate
                if (LogFile != null) {
                    // ensure directory for log file exists
                    if (!LogFile.Directory.Exists) {
                        LogFile.Directory.Create();
                        LogFile.Directory.Refresh();
                    }
                    inParams["Log"] = LogFile.FullName;
                }

                deployClass.InvokeMethod("Remove", inParams, null);

                // log success
                Log(Level.Info, "Undeployed \"{0}\"", Assembly.Name);
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Assembly \"{0}\" could not be undeployed.", Assembly.Name),
                    Location, ex);
            }
        }

        #endregion Override implementation of Task

        #region Private Instance Fields

        private FileInfo _assembly;
        private bool _uninstall;
        private FileInfo _logFile;

        #endregion Private Instance Fields
    }
}
