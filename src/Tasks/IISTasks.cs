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

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Base class for all IIS-related task.
    /// </summary>
    /// <remarks>
    /// Basically this class hold the logic to determine the IIS version as well
    /// as the IIS server/port determination/checking logic.
    /// </remarks>
    public abstract class IISTaskBase : Task {
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

        protected IISTaskBase() : base() {
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

    /// <summary>
    /// Creates or modifies a virtual directory of a web site hosted on Internet
    /// Information Server.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the virtual directory does not exist it is created, and if it already
    /// exists it is modified.
    /// </para>
    /// <para>
    /// For a list of optional parameters see <see href="ms-help://MS.VSCC/MS.MSDNVS/iisref/html/psdk/asp/aore8v5e.htm">IIsWebVirtualDir</see>.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Create a virtual directory named <c>Temp</c> pointing to <c>c:\temp</c> 
    ///   on the local machine.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <mkiisdir dirpath="c:\temp" vdirname="Temp" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Create a virtual directory named <c>Temp</c> pointing to <c>c:\temp</c> 
    ///   on machine <c>Staging</c>.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <mkiisdir iisserver="Staging" dirpath="c:\temp" vdirname="Temp" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Create a virtual directory named <c>WebServices</c> pointing to 
    ///   <c>c:\MyProject\dev</c> on the web site running on port <c>81</c> of
    ///   machine <c>MyHost</c>.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <mkiisdir iisserver="MyHost:81" dirpath="c:\MyProject\dev" vdirname="WebServices" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("mkiisdir")]
    public class MakeIISDirTask : IISTaskBase {
        #region Private Instance Fields

        private DirectoryInfo _dirPath;
        private bool _accessExecute = false;
        private bool _accessNoRemoteExecute = false;
        private bool _accessNoRemoteRead = false;
        private bool _accessNoRemoteScript = false;
        private bool _accessNoRemoteWrite = false;
        private bool _accessRead = true;
        private bool _accessSource = false;
        private bool _accessScript = true;
        private bool _accessSsl = false;
        private bool _accessSsl128 = false;
        private bool _accessSslMapCert = false;
        private bool _accessSslNegotiateCert = false;
        private bool _accessSslRequireCert = false;
        private bool _accessWrite = false;
        private bool _anonymousPasswordSync = true;
        private bool _appAllowClientDebug = false;
        private bool _appAllowDebugging = false;
        private bool _aspAllowSessionState = true;
        private bool _aspBufferingOn = true;
        private bool _aspEnableApplicationRestart = true;
        private bool _aspEnableAspHtmlFallback = false;
        private bool _aspEnableChunkedEncoding = true;
        private bool _aspErrorsToNTLog = false;
        private bool _aspEnableParentPaths = true;
        private bool _aspEnableTypelibCache = true;
        private bool _aspExceptionCatchEnable = true;
        private bool _aspLogErrorRequests = true;
        private bool _aspScriptErrorSentToBrowser = true;
        private bool _aspThreadGateEnabled = false;
        private bool _aspTrackThreadingModel = false;
        private bool _authAnonymous = true;
        private bool _authBasic = false;
        private bool _authNtlm = false;
        private bool _authPersistSingleRequest = false;
        private bool _authPersistSingleRequestIfProxy = true;
        private bool _authPersistSingleRequestAlwaysIfProxy = false;
        private bool _cacheControlNoCache = false;
        private bool _cacheIsapi = true;
        private bool _contentIndexed = true;
        private bool _cpuAppEnabled = true;
        private bool _cpuCgiEnabled = true;
        private bool _createCgiWithNewConsole = false;
        private bool _createProcessAsUser = true;
        private bool _dirBrowseShowDate = true;
        private bool _dirBrowseShowExtension = true;
        private bool _dirBrowseShowLongDate = true;
        private bool _dirBrowseShowSize = true;
        private bool _dirBrowseShowTime = true;
        private bool _dontLog = false;
        private bool _enableDefaultDoc = true;
        private bool _enableDirBrowsing = false;
        private bool _enableDocFooter = false;
        private bool _enableReverseDns = false;
        private bool _ssiExecDisable = false;
        private bool _uncAuthenticationPassthrough = false;
        private string _aspScriptErrorMessage = "An error occurred on the server when processing the URL.  Please contact the system administrator.";
        private string _defaultDoc = "Default.htm,Default.asp,index.htm,iisstart.asp,Default.aspx";

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The file system path.
        /// </summary>
        [TaskAttribute("dirpath", Required=true)]
        public DirectoryInfo DirPath {
            get { return _dirPath; }
            set { _dirPath = value; }
        }

        [TaskAttribute("accessexecute")]
        public bool AccessExecute {
            get { return _accessExecute; }
            set { _accessExecute = value; }
        }

        [TaskAttribute("accessnoremoteexecute")]
        public bool AccessNoRemoteExecute {
            get { return _accessNoRemoteExecute; }
            set { _accessNoRemoteExecute = value; }
        }

        [TaskAttribute("accessnoremoteread")]
        public bool AccessNoRemoteRead {
            get { return _accessNoRemoteRead; }
            set { _accessNoRemoteRead = value; }
        }

        [TaskAttribute("accessnoremotescript")]
        public bool AccessNoRemoteScript {
            get { return _accessNoRemoteScript; }
            set { _accessNoRemoteScript = value; }
        }

        [TaskAttribute("accessnoremotewrite")]
        public bool AccessNoRemoteWrite {
            get { return _accessNoRemoteWrite; }
            set { _accessNoRemoteWrite = value; }
        }

        [TaskAttribute("accessread")]
        public bool AccessRead {
            get { return _accessRead; }
            set { _accessRead = value; }
        }

        [TaskAttribute("accesssource")]
        public bool AccessSource {
            get { return _accessSource; }
            set { _accessSource = value; }
        }

        [TaskAttribute("accessscript")]
        public bool AccessScript {
            get { return _accessScript; }
            set { _accessScript = value; }
        }

        [TaskAttribute("accessssl")]
        public bool AccessSsl {
            get { return _accessSsl; }
            set { _accessSsl = value; }
        }

        [TaskAttribute("accessssl128")]
        public bool AccesssSl128 {
            get { return _accessSsl128; }
            set { _accessSsl128 = value; }
        }

        [TaskAttribute("accesssslmapcert")]
        public bool AccessSslMapCert {
            get { return _accessSslMapCert; }
            set { _accessSslMapCert = value; }
        }

        [TaskAttribute("accesssslnegotiatecert")]
        public bool AccessSslNegotiateCert {
            get { return _accessSslNegotiateCert; }
            set { _accessSslNegotiateCert = value; }
        }

        [TaskAttribute("accesssslrequirecert")]
        public bool AccessSslRequireCert {
            get { return _accessSslRequireCert; }
            set { _accessSslRequireCert = value; }
        }

        [TaskAttribute("accesswrite")]
        public bool AccessWrite {
            get { return _accessWrite; }
            set { _accessWrite = value; }
        }

        [TaskAttribute("anonymouspasswordsync")]
        public bool AnonymousPasswordSync {
            get { return _anonymousPasswordSync; }
            set { _anonymousPasswordSync = value; }
        }

        [TaskAttribute("appallowclientdebug")]
        public bool AppAllowClientDebug {
            get { return _appAllowClientDebug; }
            set { _appAllowClientDebug = value; }
        }

        [TaskAttribute("appallowdebugging")]
        public bool AppAllowDebugging {
            get { return _appAllowDebugging; }
            set { _appAllowDebugging = value; }
        }

        [TaskAttribute("aspallowsessionstate")]
        public bool AspAllowSessionState {
            get { return _aspAllowSessionState; }
            set { _aspAllowSessionState = value; }
        }

        [TaskAttribute("aspbufferingon")]
        public bool AspBufferingOn {
            get { return _aspBufferingOn; }
            set { _aspBufferingOn = value; }
        }

        [TaskAttribute("aspenableapplicationrestart")]
        public bool AspEnableApplicationRestart {
            get { return _aspEnableApplicationRestart; }
            set { _aspEnableApplicationRestart = value; }
        }

        [TaskAttribute("aspenableasphtmlfallback")]
        public bool AspEnableAspHtmlFallback {
            get { return _aspEnableAspHtmlFallback; }
            set { _aspEnableAspHtmlFallback = value; }
        }

        [TaskAttribute("aspenablechunkedencoding")]
        public bool AspEnableChunkedEncoding {
            get { return _aspEnableChunkedEncoding; }
            set { _aspEnableChunkedEncoding = value; }
        }

        [TaskAttribute("asperrorstontlog")]
        public bool AspErrorsToNTLog {
            get { return _aspErrorsToNTLog; }
            set { _aspErrorsToNTLog = value; }
        }

        [TaskAttribute("aspenableparentpaths")]
        public bool AspEnableParentPaths {
            get { return _aspEnableParentPaths; }
            set { _aspEnableParentPaths = value; }
        }

        [TaskAttribute("aspenabletypelibcache")]
        public bool AspEnableTypelibCache {
            get { return _aspEnableTypelibCache; }
            set { _aspEnableTypelibCache = value; }
        }

        [TaskAttribute("aspexceptioncatchenable")]
        public bool AspExceptionCatchEnable {
            get { return _aspExceptionCatchEnable; }
            set { _aspExceptionCatchEnable = value; }
        }

        [TaskAttribute("asplogerrorrequests")]
        public bool AspLogErrorRequests {
            get { return _aspLogErrorRequests; }
            set { _aspLogErrorRequests = value; }
        }

        [TaskAttribute("aspscripterrorsenttobrowser")]
        public bool AspScriptErrorSentToBrowser {
            get { return _aspScriptErrorSentToBrowser; }
            set { _aspScriptErrorSentToBrowser = value; }
        }

        [TaskAttribute("aspthreadgateenabled")]
        public bool AspThreadGateEnabled {
            get { 
                if (this.Version != IISVersion.Five) {
                    throw new BuildException("Option only applies to IIS 5.x", Location);
                }
                return _aspThreadGateEnabled;
            }
            set { 
                if (this.Version != IISVersion.Five) {
                    throw new BuildException("Option only applies to IIS 5.x", Location);
                }
                _aspThreadGateEnabled = value; 
            }
        }

        [TaskAttribute("asptrackthreadingmodel")]
        public bool AspTrackThreadingModel {
            get { 
                if (this.Version != IISVersion.Five) {
                    throw new BuildException("Option only applies to IIS 5.x", Location);
                }
                return _aspTrackThreadingModel; 
            }
            set { 
                if (this.Version != IISVersion.Five) {
                    throw new BuildException("Option only applies to IIS 5.x", Location); 
                }
                _aspTrackThreadingModel = value; 
            }
        }

        [TaskAttribute("authanonymous")]
        public bool AuthAnonymous {
            get { return _authAnonymous; }
            set { _authAnonymous = value; }
        }

        [TaskAttribute("authbasic")]
        public bool AuthBasic {
            get { return _authBasic; }
            set { _authBasic = value; }
        }

        [TaskAttribute("authntlm")]
        public bool AuthNtlm {
            get { return _authNtlm; }
            set { _authNtlm = value; }
        }

        [TaskAttribute("authpersistsinglerequest")]
        public bool AuthPersistSingleRequest{
            get { return _authPersistSingleRequest; }
            set { _authPersistSingleRequest = value; }
        }

        [TaskAttribute("authpersistsinglerequestifproxy")]
        public bool AuthPersistSingleRequestIfProxy{
            get { 
                if (this.Version != IISVersion.Five) {
                    throw new BuildException("Option only applies to IIS 5.x", Location); 
                }
                return _authPersistSingleRequestIfProxy; }
            set { 
                if (this.Version != IISVersion.Five) {
                    throw new BuildException("Option only applies to IIS 5.x", Location);
                }
                _authPersistSingleRequestIfProxy = value; 
            }
        }

        [TaskAttribute("authpersistsinglerequestalwaysifproxy")]
        public bool AuthPersistSingleRequestAlwaysIfProxy{
            get { 
                if (this.Version != IISVersion.Five) {
                    throw new BuildException("Option only applies to IIS 5.x", Location); 
                }
                return _authPersistSingleRequestAlwaysIfProxy; 
            }
            set { 
                if (this.Version != IISVersion.Five) {
                    throw new BuildException("Option only applies to IIS 5.x", Location); 
                }
                _authPersistSingleRequestAlwaysIfProxy = value; 
            }
        }

        [TaskAttribute("cachecontrolnocache")]
        public bool CacheControlNoCache {
            get { return _cacheControlNoCache; }
            set { _cacheControlNoCache = value; }
        }

        [TaskAttribute("cacheisapi")]
        public bool CacheIsapi {
            get { return _cacheIsapi; }
            set { _cacheIsapi = value; }
        }

        [TaskAttribute("contentindexed")]
        public bool ContentIndexed {
            get { return _contentIndexed; }
            set { _contentIndexed = value; }
        }

        [TaskAttribute("cpuappenabled")]
        public bool CpuAppEnabled {
            get { 
                if (this.Version != IISVersion.Five) {
                    throw new BuildException("Option only applies to IIS 5.x", Location); 
                }
                return _cpuAppEnabled; 
            }
            set { 
                if (this.Version != IISVersion.Five) {
                    throw new BuildException("Option only applies to IIS 5.x", Location);
                }
                _cpuAppEnabled = value;
            }
        }

        [TaskAttribute("cpucgienabled")]
        public bool CpuCgiEnabled{
            get { 
                if (this.Version != IISVersion.Five) {
                    throw new BuildException("Option only applies to IIS 5.x", Location);
                }
                return _cpuCgiEnabled;
            }
            set {
                if (this.Version != IISVersion.Five) {
                    throw new BuildException("Option only applies to IIS 5.x", Location);
                }
                _cpuCgiEnabled = value;
            }
        }

        [TaskAttribute("createcgiwithnewconsole")]
        public bool CreateCgiWithNewConsole {
            get { return _createCgiWithNewConsole; }
            set { _createCgiWithNewConsole = value; }
        }

        [TaskAttribute("createprocessasuser")]
        public bool CreateProcessAsUser {
            get { return _createProcessAsUser; }
            set { _createProcessAsUser = value; }
        }

        [TaskAttribute("dirbrowseshowdate")]
        public bool DirBrowseShowDate {
            get { return _dirBrowseShowDate; }
            set { _dirBrowseShowDate = value; }
        }

        [TaskAttribute("dirbrowseshowextension")]
        public bool DirBrowseShowExtension {
            get { return _dirBrowseShowExtension; }
            set { _dirBrowseShowExtension = value; }
        }

        [TaskAttribute("dirbrowseshowlongdate")]
        public bool DirBrowseShowLongDate {
            get { return _dirBrowseShowLongDate; }
            set { _dirBrowseShowLongDate = value; }
        }

        [TaskAttribute("dirbrowseshowsize")]
        public bool DirBrowseShowSize {
            get { return _dirBrowseShowSize; }
            set { _dirBrowseShowSize = value; }
        }

        [TaskAttribute("dirbrowseshowtime")]
        public bool DirBrowseShowTime {
            get { return _dirBrowseShowTime; }
            set { _dirBrowseShowTime = value; }
        }

        [TaskAttribute("dontlog")]
        public bool DontLog {
            get { return _dontLog; }
            set { _dontLog = value; }
        }

        [TaskAttribute("enabledefaultdoc")]
        public bool EnableDefaultDoc {
            get { return _enableDefaultDoc; }
            set { _enableDefaultDoc = value; }
        }

        [TaskAttribute("enabledirbrowsing")]
        public bool EnableDirBrowsing {
            get { return _enableDirBrowsing; }
            set { _enableDirBrowsing = value; }
        }

        [TaskAttribute("enabledocfooter")]
        public bool EnableDocFooter {
            get { return _enableDocFooter; }
            set { _enableDocFooter = value; }
        }

        [TaskAttribute("enablereversedns")]
        public bool EnableReverseDns {
            get { return _enableReverseDns; }
            set { _enableReverseDns = value; }
        }

        [TaskAttribute("ssiexecdisable")]
        public bool SsiExecDisable {
            get { return _ssiExecDisable; }
            set { _ssiExecDisable = value; }
        }

        [TaskAttribute("uncauthenticationpassthrough")]
        public bool UncAuthenticationPassthrough {
            get { return _uncAuthenticationPassthrough; }
            set { _uncAuthenticationPassthrough = value; }
        }

        [TaskAttribute("aspscripterrormessage")]
        public string AspScriptErrorMessage {
            get { return _aspScriptErrorMessage; }
            set { _aspScriptErrorMessage = value; }
        }

        [TaskAttribute("defaultdoc")]
        public string DefaultDoc {
            get { return _defaultDoc; }
            set { _defaultDoc = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        protected override void ExecuteTask() {
            try {
                Log(Level.Info, "Creating/modifying virtual directory '{0}' on"
                    + " '{1}'.", this.VirtualDirectory, this.Server);

                // ensure IIS is available on specified host and port
                this.DetermineIISSettings();

                DirectoryEntry folderRoot = new DirectoryEntry(this.ServerPath);
                folderRoot.RefreshCache();
                DirectoryEntry newVirDir;

                try {
                    // Try to find the directory
                    DirectoryEntry tempVirDir = folderRoot.Children.Find(this.VirtualDirectory, folderRoot.SchemaClassName);
                    newVirDir = tempVirDir;
                } catch {
                    // If the directory doesn't exist create it.
                    newVirDir = folderRoot.Children.Add(this.VirtualDirectory, folderRoot.SchemaClassName);
                    newVirDir.CommitChanges();
                }

                // Set Required Properties
                newVirDir.Properties["Path"].Value = DirPath.FullName;
                newVirDir.Properties["AppFriendlyName"].Value = this.VirtualDirectory;
                newVirDir.Properties["AppRoot"].Value = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", this.ApplicationPath, this.VirtualDirectory);

                // Set Optional Properties
                newVirDir.Properties["AccessExecute"][0] = AccessExecute;
                newVirDir.Properties["AccessNoRemoteExecute"][0] = AccessNoRemoteExecute;
                newVirDir.Properties["AccessNoRemoteRead"][0] = AccessNoRemoteRead;
                newVirDir.Properties["AccessNoRemoteScript"][0] = AccessNoRemoteScript;
                newVirDir.Properties["AccessNoRemoteWrite"][0] = AccessNoRemoteWrite;
                newVirDir.Properties["AccessRead"][0] = AccessRead;
                newVirDir.Properties["AccessSource"][0] = AccessSource;
                newVirDir.Properties["AccessScript"][0] = AccessScript;
                newVirDir.Properties["AccessSSL"][0] = AccessSsl;
                newVirDir.Properties["AccessSSL128"][0] = AccesssSl128;
                newVirDir.Properties["AccessSSLMapCert"][0] = AccessSslMapCert;
                newVirDir.Properties["AccessSSLNegotiateCert"][0] = AccessSslNegotiateCert;
                newVirDir.Properties["AccessSSLRequireCert"][0] = AccessSslRequireCert;
                newVirDir.Properties["AccessWrite"][0] = AccessWrite;
                newVirDir.Properties["AnonymousPasswordSync"][0] = AnonymousPasswordSync;
                newVirDir.Properties["AppAllowClientDebug"][0] = AppAllowClientDebug;
                newVirDir.Properties["AppAllowDebugging"][0] = AppAllowDebugging;
                newVirDir.Properties["AspBufferingOn"][0] = AspBufferingOn;
                newVirDir.Properties["AspEnableApplicationRestart"][0] = AspEnableApplicationRestart;
                newVirDir.Properties["AspEnableAspHtmlFallback"][0] = AspEnableAspHtmlFallback;
                newVirDir.Properties["AspEnableChunkedEncoding"][0] = AspEnableChunkedEncoding;
                newVirDir.Properties["AspErrorsToNTLog"][0] = AspErrorsToNTLog;
                newVirDir.Properties["AspEnableParentPaths"][0] = AspEnableParentPaths;
                newVirDir.Properties["AspEnableTypelibCache"][0] = AspEnableTypelibCache;
                newVirDir.Properties["AspExceptionCatchEnable"][0] = AspExceptionCatchEnable;
                newVirDir.Properties["AspLogErrorRequests"][0] = AspLogErrorRequests;
                newVirDir.Properties["AspScriptErrorSentToBrowser"][0] = AspScriptErrorSentToBrowser;

                if (this.Version == IISVersion.Five) {
                    newVirDir.Properties["AspThreadGateEnabled"][0] = AspThreadGateEnabled;
                    newVirDir.Properties["AspTrackThreadingModel"][0] = AspTrackThreadingModel;
                }

                newVirDir.Properties["AuthAnonymous"][0] = AuthAnonymous;
                newVirDir.Properties["AuthBasic"][0] = AuthBasic;
                newVirDir.Properties["AuthNTLM"][0] = AuthNtlm;
                newVirDir.Properties["AuthPersistSingleRequest"][0] = AuthPersistSingleRequest;

                if (this.Version == IISVersion.Five) {
                    newVirDir.Properties["AuthPersistSingleRequestIfProxy"][0] = AuthPersistSingleRequestIfProxy;
                    newVirDir.Properties["AuthPersistSingleRequestAlwaysIfProxy"][0] = AuthPersistSingleRequestAlwaysIfProxy;
                }

                newVirDir.Properties["CacheControlNoCache"][0] = CacheControlNoCache;
                newVirDir.Properties["CacheISAPI"][0] = CacheIsapi;
                newVirDir.Properties["ContentIndexed"][0] = ContentIndexed;

                if (this.Version == IISVersion.Five) {
                    newVirDir.Properties["CpuAppEnabled"][0] = CpuAppEnabled;
                    newVirDir.Properties["CpuCgiEnabled"][0] = CpuCgiEnabled;
                }

                newVirDir.Properties["CreateCGIWithNewConsole"][0] = CreateCgiWithNewConsole;
                newVirDir.Properties["CreateProcessAsUser"][0] = CreateProcessAsUser;
                newVirDir.Properties["DirBrowseShowDate"][0] = DirBrowseShowDate;
                newVirDir.Properties["DirBrowseShowExtension"][0] = DirBrowseShowExtension;
                newVirDir.Properties["DirBrowseShowLongDate"][0] = DirBrowseShowLongDate;
                newVirDir.Properties["DirBrowseShowSize"][0] = DirBrowseShowSize;
                newVirDir.Properties["DirBrowseShowTime"][0] = DirBrowseShowTime;
                newVirDir.Properties["DontLog"][0] = DontLog;
                newVirDir.Properties["EnableDefaultDoc"][0] = EnableDefaultDoc;
                newVirDir.Properties["EnableDirBrowsing"][0] = EnableDirBrowsing;
                newVirDir.Properties["EnableDocFooter"][0] = EnableDocFooter;
                newVirDir.Properties["EnableReverseDns"][0] = EnableReverseDns;
                newVirDir.Properties["SSIExecDisable"][0] = SsiExecDisable;

                if (this.Version == IISVersion.Five) {
                    newVirDir.Properties["UNCAuthenticationPassthrough"][0] = UncAuthenticationPassthrough;
                }

                newVirDir.Properties["AspScriptErrorMessage"][0] = AspScriptErrorMessage;
                newVirDir.Properties["DefaultDoc"][0] = DefaultDoc;

                // Save Changes
                newVirDir.CommitChanges();
                folderRoot.CommitChanges();
                newVirDir.Close();
                folderRoot.Close();
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture, 
                    "Error creating virtual directory '{0}' on '{1}'.", 
                    this.VirtualDirectory, this.Server), Location, ex);
            }
        }

        #endregion Override implementation of Task
    }

    /// <summary>
    /// Deletes a virtual directory from a given web site hosted on Internet 
    /// Information Server.
    /// </summary>
    /// <example>
    ///   <para>
    ///   Delete a virtual directory named <c>Temp</c> from the web site running
    ///   on port <c>80</c> of the local machine.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <deliisdir vdirname="Temp" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Delete a virtual directory named <c>Temp</c> from the website running 
    ///   on port <c>81</c> of machine <c>MyHost</c>.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <deliisdir iisserver="MyHost:81" vdirname="Temp" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("deliisdir")]
    public class DeleteIISDirTask : IISTaskBase {
        protected override void ExecuteTask() {
            try {
                Log(Level.Info, "Deleting virtual directory '{0}' on '{1}'.", 
                    this.VirtualDirectory, this.Server);

                // ensure IIS is available on specified host and port
                this.DetermineIISSettings();

                DirectoryEntry folderRoot = new DirectoryEntry(this.ServerPath);
                DirectoryEntries rootEntries = folderRoot.Children;
                folderRoot.RefreshCache();
                DirectoryEntry childVirDir = folderRoot.Children.Find(this.VirtualDirectory, folderRoot.SchemaClassName);
                rootEntries.Remove(childVirDir);

                childVirDir.Close();
                folderRoot.Close();
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Error deleting virtual directory '{0}' on '{1}'.", 
                    this.VirtualDirectory, this.Server), Location, ex);
            }
        }
    }

    /// <summary>
    /// Lists the configuration settings of a specified virtual directory in a
    /// web site hosted on Internet Information Server.
    /// </summary>
    /// <example>
    ///   <para>
    ///   List the settings of a virtual directory named <c>Temp</c>.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <iisdirinfo vdirname="Temp" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("iisdirinfo")]
    public class IISDirInfoTask : IISTaskBase {
        protected override void ExecuteTask() {
            try {
                Log(Level.Info, "Retrieving settings of virtual directory '{0}'"
                    + " on '{1}'.", this.VirtualDirectory, this.Server);

                // ensure IIS is available on specified host and port
                this.DetermineIISSettings();

                // retrieve DirectoryEntry representing root of web site
                DirectoryEntry folderRoot = new DirectoryEntry(this.ServerPath);
                folderRoot.RefreshCache();

                // locate DirectoryEntry representing virtual directory
                DirectoryEntry newVirDir = folderRoot.Children.Find(this.VirtualDirectory, folderRoot.SchemaClassName);

                // output all properties of virtual directory
                foreach (string propertyName in newVirDir.Properties.PropertyNames) {
                    object propertyValue = newVirDir.Properties[propertyName].Value;

                    if (propertyValue.GetType().IsArray) {
                        Log(Level.Info, '\t' + propertyName + ":");
                        Array propertyValues = (Array) propertyValue;
                        foreach (object value in propertyValues) {
                            Log(Level.Info, "\t\t" + value.ToString());
                        }
                    } else {
                        Log(Level.Info, '\t' + propertyName + ": " 
                            + newVirDir.Properties[propertyName].Value.ToString());
                    }
                }

                newVirDir.Close();
                folderRoot.Close();
            } catch (BuildException) {
                // re-throw exception
                throw;
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Error retrieving info for virtual directory '{0}' on '{1}'.", 
                    this.VirtualDirectory, this.Server), Location, ex);
            }
        }
    }
}
