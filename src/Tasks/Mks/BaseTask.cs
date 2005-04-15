#region GNU General Public License
//
// NAntContrib
// Copyright (C) 2001-2003 Gerry Shaw
//
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
// Paul Francis, Edenbrook. (paul.francis@edenbrook.co.uk)
#endregion

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
//using System.IO;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks.Mks {
    /// <summary>
    /// The base abstract class for all MKS Tasks.  
    /// </summary>
    /// <remarks>
    /// Provides the core attributes, and functionality for opening an item 
    /// in a MKS database.
    /// </remarks>
    public abstract class BaseTask : Task {
        #region Private Instance Fields

        private string _password;
        private string _userName;
        private string _hostName;
        private string _port;

        #endregion Private Instance Fields

        #region Public Instance Properties
       
        /// <summary>
        /// The password to use to login to the MKS database.
        /// </summary>
        [TaskAttribute("password", Required=false)]
        public string Password {
            get { return _password; }
            set { _password = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The name of the user needed to access the MKS database.
        /// </summary>
        [TaskAttribute("username", Required=false)]
        public string UserName { 
            get { return _userName; } 
            set { _userName = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The name of the host MKS server to connect to
        /// </summary>
        [TaskAttribute("host", Required=true)]
        public virtual string Host {
            get { return _hostName; } 
            set { _hostName = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The port number on which the host server is accepting requests
        /// </summary>
        [TaskAttribute("port", Required=true)]
        public virtual string Port {
            get { return _port; } 
            set { _port = StringUtils.ConvertEmptyToNull(value); }
        }

        #endregion Public Instance Properties

        #region Protected Instance Methods

        /// <summary>
        /// Opens the MKS database and sets the reference to the specified
        /// item and version.
        /// </summary>
        protected void Open() {
            try {
                string exec = string.Format(CultureInfo.InvariantCulture,
                    "connect --hostname={0} --port={1}", new object[] {_hostName,_port});

                if (UserName != null) {
                    exec+=" --user=" + UserName;
                }

                if (Password != null) {
                    exec+=" --password=" + Password;
                }

                MKSExecute(exec);
            } catch (BuildException) {
                throw;
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Failed to connect to host \"{0}\" on port {1}.", Host, Port),
                    Location, ex);
            }
        }

        protected string MKSExecute(string command) {
            // locate Source Integrity client on PATH
            PathScanner pathScanner = new PathScanner();
            pathScanner.Add("si.exe");
            StringCollection files = pathScanner.Scan();

            if (files.Count == 0) {
                throw new BuildException("Could not find MKS Integrity Client on the system path.",
                    Location);
            }

            // set up process
            Process proc = new Process();
            proc.StartInfo.UseShellExecute=false;
            proc.StartInfo.CreateNoWindow=true;
            proc.StartInfo.RedirectStandardOutput=true;
            proc.StartInfo.RedirectStandardError=true;
            proc.StartInfo.FileName = files[0];
            proc.StartInfo.Arguments = command;
            proc.Start();
            proc.WaitForExit();

            if (proc.ExitCode != 0) {
                throw new BuildException(proc.ExitCode.ToString(), Location, 
                    new Exception(proc.StandardError.ReadToEnd()));
            } else {
                return proc.StandardOutput.ReadToEnd();
            }
        }

        #endregion Protected Instance Methods
    }
}
