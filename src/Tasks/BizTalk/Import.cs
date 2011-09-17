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
    /// Imports bindings from a given assembly binding information file into
    /// the specified BizTalk configuration database.
    /// </summary>
    [TaskName("btsimport")]
    public class Import : BizTalkBase {
        #region Public Instance Properties

        /// <summary>
        /// The path to the assembly binding information file containing the
        /// bindings to import.
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
        /// Imports the assembly binding information file.
        /// </summary>
        /// <exception cref="BuildException">
        ///   <para>The assembly binding information file does not exist.</para>
        ///   <para>-or-</para>
        ///   <para>The assembly binding information file could not be imported.</para>
        /// </exception>
        protected override void ExecuteTask() {
            try {
                Log(Level.Verbose, "Importing bindings from \"{0}\"...", BindingFile.Name);

                // ensure assembly binding file exists
                if (!BindingFile.Exists) {
                    throw new FileNotFoundException("Assembly binding information"
                        + " file does not exist.");
                }

                ManagementPath path = new ManagementPath("MSBTS_DeploymentService");
                ManagementClass deployClass = new ManagementClass(Scope, path, 
                    new ObjectGetOptions());
            
                ManagementBaseObject inParams = deployClass.GetMethodParameters("Import");
                inParams["Server"] = Server;
                inParams["Database"] = Database;

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

                deployClass.InvokeMethod("Import", inParams, null);

                // log success
                Log(Level.Info, "Imported bindings from \"{0}\"", BindingFile.Name);
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Assembly binding information file \"{0}\" could not be imported.", 
                    BindingFile.Name), Location, ex);
            }
        }

        #endregion Override implementation of Task

        #region Private Instance Fields

        private FileInfo _bindingFile;
        private FileInfo _logFile;

        #endregion Private Instance Fields
    }
}
