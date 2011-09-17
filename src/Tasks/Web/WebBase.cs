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
// Gert Driesen (drieseng@users.sourceforge.net)
// Rutger Dijkstra (R.M.Dijkstra@eyetoeye.nl)
// Payton Byrd (payton@paytonbyrd.com)

using System;
using System.Collections;
using System.DirectoryServices;
using System.Globalization;
using System.IO;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Util;

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
        private string _website;

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
            set { _virtualDirectory = value.Trim('/'); }
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
                return string.Format(CultureInfo.InvariantCulture, "{0}:{1}",
                    _serverName, _serverPort);
            }
            set {
                if (value.IndexOf(":") < 0) {
                    _serverName = value;
                } else {
                    string[] parts = value.Split(':');
                    _serverName = parts[0];
                    _serverPort = Convert.ToInt32(parts[1], CultureInfo.InvariantCulture);
                }
            }
        }

        /// <summary>
        /// The website on the IIS server.  
        /// </summary>
        /// <remarks>
        /// <para>
        /// This allows for targeting a specific virtual site on a physical box.
        /// </para>
        /// </remarks>
        [TaskAttribute("website")]
        public string Website {
            get { return _website; }
            set { _website = StringUtils.ConvertEmptyToNull(value); }
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
                return string.Format(CultureInfo.InvariantCulture, "IIS://{0}/W3SVC/{1}/Root",
                    _serverName, _serverInstance); 
            }
        }

        protected string ApplicationPath {
            get { 
                return string.Format(CultureInfo.InvariantCulture, "/LM/W3SVC/{0}/Root",
                    _serverInstance); 
            }
        }

        protected string VdirPath {
            get {
                if (this.VirtualDirectory.Length == 0) {
                    return string.Empty;
                } else {
                    return "/" + VirtualDirectory;
                }
            }
        }

        #endregion Protected Instance Properties

        #region Protected Instance Methods

        //bug-fix for DirectoryEntry.Exists which checks for the wrong COMException
        protected bool DirectoryEntryExists(string path) {
            DirectoryEntry entry = new DirectoryEntry(path);
            try {
                //trigger the *private* entry.Bind() method
                object adsobject = entry.NativeObject;
                if (adsobject == null) {
                    // done to prevent CS0219, variable is assigned but its
                    // value is never used
                }
                return true;
            } catch {
                return false;
            } finally {
                entry.Dispose();
            }
        }

        protected void CheckIISSettings() {
            if (!DirectoryEntryExists(string.Format(CultureInfo.InvariantCulture, "IIS://{0}/W3SVC", _serverName))) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "The webservice at '{0}' does not exist or is not reachable.", 
                    _serverName));
            }
            _serverInstance = FindServerInstance();
 
            if (_serverInstance == null) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "No website is bound to '{0}'.", Server));
            }
        }

        #endregion Protected Instance Methods

        #region Private Instance Methods

        private int BindingPriority(string binding) {
            string[] bindingParts = binding.Split(':');
            int port = Convert.ToInt32(bindingParts[1], CultureInfo.InvariantCulture);
            string host = bindingParts[2];
            if (port != _serverPort) {
                return 0;
            }
            if (host.Length == 0) {
                return 1;
            }
            if (host == _serverName) {
                return 2;
            }
            return 0;
        }

        private int BindingPriority(string binding, bool siteRunning) {
            int basePriority = BindingPriority(binding);
            return siteRunning ? basePriority << 4 : basePriority; 
        }

        private bool IsRunning(DirectoryEntry website) {
            return 2 == (int)website.Properties["ServerState"].Value;
        }

        private string FindServerInstance() {
            bool foundWebsite = false;
            int maxBindingPriority = 0;
            string instance = null;
            string serverComment = null;
            string websiteDefinition = null;
            DirectoryEntry webServer = new DirectoryEntry(string.Format(CultureInfo.InvariantCulture, 
                "IIS://{0}/W3SVC", _serverName));
            webServer.RefreshCache();
 
            foreach (DirectoryEntry website in webServer.Children) {
                if (website.SchemaClassName != "IIsWebServer") { 
                    website.Close();
                    continue; 
                }
                foreach (string binding in website.Properties["ServerBindings"]) {
                    int bindingPriority = BindingPriority(binding, IsRunning(website));
                    if (bindingPriority <= maxBindingPriority) {
                        continue;
                    }
                    instance = website.Name;

                    serverComment = (string) website.Properties["ServerComment"].Value;

                    if (Website != null) {
                        Log(Level.Verbose, "Examining website \"{0}\"...", serverComment);
                        if (serverComment != null && serverComment == Website) {
                            Log(Level.Verbose, "Found website \"{0}\".", Website);
                            websiteDefinition = serverComment;
                            maxBindingPriority = bindingPriority;
                            foundWebsite = true;
                            break;
                        }
                    } else {
                        websiteDefinition = serverComment;
                        maxBindingPriority = bindingPriority;
                    }
                }
                website.Close();

                if (foundWebsite) {
                    break;
                }
            }
            webServer.Close();

            // fail build if website was specified, but not found
            if (Website != null && !foundWebsite) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "The website named '{0}' does not exist.", 
                    Website), Location);
            }

            Website = websiteDefinition;

            return instance;
        }


        #endregion Private Instance Methods
    }
}
