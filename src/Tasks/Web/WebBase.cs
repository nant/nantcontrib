// NAntContrib
// Copyright (C) 2002 Brian Nantz bcnantz@juno.com
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
// Brian Nantz (bcnantz@juno.com)
// Gert Driesen (gert.driesen@ardatis.com)

using System;
using System.Collections;
using System.DirectoryServices;
using System.Globalization;
using System.IO;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.Web {
    /// <summary>
    /// Base class for all IIS-related task.
    /// </summary>
    /// <remarks>
    /// Basically this class hold the logic to determine the IIS version as well
    /// as the IIS server/port determination/checking logic.
    /// </remarks>
    public abstract class WebBase : Task {
        /// <summary>
        /// Defines the IIS versions supported by the IIS tasks.
        /// </summary>
        protected enum IISVersion {
            None,
            Four,
            Five,
            Six
        }

        #region Private Instance Fields

        private string _virtualDirectory;
        private int _serverPort = 80;
        private string _serverName = "localhost";
        private string _serverInstance = "1";
        private IISVersion _iisVersion = IISVersion.None;

        #endregion Private Instance Fields

        #region Protected Instance Constructors

        protected WebBase() : base() {
        }

        #endregion Protected Instance Constructors

        #region Public Instance Properties

        /// <summary>
        /// Name of the IIS virtual directory.
        /// </summary>
        [TaskAttribute("vdirname", Required=true)]
        public string VirtualDirectory {
            get { return _virtualDirectory; }
            set { _virtualDirectory = value; }
        }

        /// <summary>
        /// The IIS server, which can be specified using the format <c>[host]:[port]</c>. 
        /// The default is <c>localhost:80</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This allows for targeting a specific virtual site on a physical box.
        /// </para>
        /// </remarks>
        [TaskAttribute("iisserver")]
        public string Server {
            get {
                return string.Format(CultureInfo.InvariantCulture, 
                    "{0}:{1}", _serverName, _serverPort);
            }
            set {
                if (value.IndexOf(":") < 0) {
                    _serverName = value;
                } else {
                    string[] parts = value.Split(':');
                    _serverName = parts[0];
                    _serverPort = Convert.ToInt32(parts[1], CultureInfo.InvariantCulture);

                    // ensure IIS is available on specified host and port
                    this.DetermineIISSettings();
                }
            }
        }

        #endregion Public Instance Properties

        #region Protected Instance Properties

        /// <summary>
        /// Gets the version of IIS corresponding with the current OS.
        /// </summary>
        /// <value>
        /// The version of IIS corresponding with the current OS.
        /// </value>
        protected IISVersion Version {
            get {
                if (_iisVersion == IISVersion.None) {
                    Version osVersion = Environment.OSVersion.Version;

                    if (osVersion.Major < 5) {
                        // Win NT 4 kernel -> IIS4
                        _iisVersion = IISVersion.Four;
                    } else {
                        switch (osVersion.Minor) {
                            case 0:
                                // Win 2000 kernel -> IIS5
                                _iisVersion = IISVersion.Five;
                                break;
                            case 1:
                                // Win XP kernel -> IIS5
                                _iisVersion = IISVersion.Five;
                                break;
                            case 2:
                                // Win 2003 kernel -> IIS6
                                _iisVersion = IISVersion.Six;
                                break;
                        }
                    }
                }
                return _iisVersion;
            }
        }

        protected string ServerPath {
            get { 
                return string.Format(CultureInfo.InvariantCulture, 
                    "IIS://{0}/W3SVC/{1}/Root", _serverName, 
                    _serverInstance); 
            }
        }

        protected string ApplicationPath {
            get { 
                return string.Format(CultureInfo.InvariantCulture, 
                    "/LM/W3SVC/{0}/Root", _serverInstance); 
            }
        }

        #endregion Protected Instance Properties

        #region Protected Instance Methods

        protected void DetermineIISSettings() {
            bool websiteFound = false;

            DirectoryEntry webServer = new DirectoryEntry(string.Format(
                CultureInfo.InvariantCulture, "IIS://{0}/W3SVC", _serverName));

            // refresh and cache property values for this directory entry
            webServer.RefreshCache();

            // enumerate all websites on webserver
            foreach (DirectoryEntry website in webServer.Children) {
                if (website.SchemaClassName == "IIsWebServer") {
                    if (this.MatchingPortFound(website)) {
                        websiteFound = true;
                    }
                }

                // close DirectoryEntry and release system resources
                website.Close();
            }

            // close DirectoryEntry and release system resources
            webServer.Close();

            if (!websiteFound) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture, 
                    "Server '{0}' does not exist or is not reachable.", Server));
            }
        }

        #endregion Protected Instance Methods

        #region Private Instance Methods

        private bool MatchingPortFound(DirectoryEntry website) {
            string bindings = website.Properties["ServerBindings"].Value.ToString();
            string[] bindingParts = bindings.Split(':');

            if (_serverPort == Convert.ToInt32(bindingParts[1], CultureInfo.InvariantCulture)) {
                _serverInstance = website.Name;
                return true;
            }
            return false;
        }

        #endregion Private Instance Methods
    }
}
