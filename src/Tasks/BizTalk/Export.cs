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
    /// Exports bindings for a BizTalk assembly to an assembly binding 
    /// information file.
    /// </summary>
    [TaskName("btsexport")]
    public class Export : BizTalkBase {
        #region Public Instance Properties

        /// <summary>
        /// The path to the BizTalk assembly for which to export bindings.
        /// </summary>
        [TaskAttribute("assembly", Required=true)]
        public FileInfo Assembly {
            get { return _assembly; }
            set { _assembly = value; }
        }

        /// <summary>
        /// The path to an assembly binding information file in which the 
        /// bindings will be saved.
        /// </summary>
        [TaskAttribute("bindingfile", Required=true)]
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
        /// Exports the bindings.
        /// </summary>
        /// <exception cref="BuildException">
        ///   <para>The assembly does not exist.</para>
        ///   <para>-or-</para>
        ///   <para>The bindings could not be exported.</para>
        /// </exception>
        protected override void ExecuteTask() {
            try {
                Log(Level.Verbose, "Exporting bindings for \"{0}\"...", Assembly.Name);

                // ensure assembly exists
                if (!Assembly.Exists) {
                    throw new FileNotFoundException("The assembly does not exist.");
                }

                // ensure directory for binding file exists
                if (!BindingFile.Directory.Exists) {
                    BindingFile.Directory.Create();
                    BindingFile.Directory.Refresh();
                }

                ManagementPath path = new ManagementPath("MSBTS_DeploymentService");
                ManagementClass deployClass = new ManagementClass(Scope, path, 
                    new ObjectGetOptions());
            
                ManagementBaseObject inParams = deployClass.GetMethodParameters("Export");
                inParams["Server"] = Server;
                inParams["Database"] = Database;
                inParams["Assembly"] = Assembly.FullName;

                // assembly binding information file
                inParams["Binding"] = BindingFile.FullName;

                // HTML log file to generate
                if (LogFile != null) {
                    // ensure directory for log file exists
                    if (!LogFile.Directory.Exists) {
                        LogFile.Directory.Create();
                        LogFile.Directory.Refresh();
                    }
                    inParams["Log"] = LogFile.FullName;
                }

                deployClass.InvokeMethod("Export", inParams, null);

                // log success
                Log(Level.Info, "Exported bindings for \"{0}\"", Assembly.Name);
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Bindings for assembly \"{0}\" could not be exported.", Assembly.Name),
                    Location, ex);
            }
        }

        #endregion Override implementation of Task

        #region Private Instance Fields

        private FileInfo _assembly;
        private FileInfo _bindingFile;
        private FileInfo _logFile;

        #endregion Private Instance Fields
    }
}
