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
// Rutger Dijkstra (R.M.Dijkstra@eyetoeye.nl)

using System;
using System.Collections;
using System.DirectoryServices;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.Web {
    /// <summary>
    /// Creates or modifies a virtual directory of a web site hosted on Internet
    /// Information Server.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the virtual directory does not exist it is created, and if it already
    /// exists it is modified. Only the IIS-properties specified will be set. If set
    /// by other means (e.g. the Management Console), the unspecified properties retain their current value, 
    /// otherwise they are inherited from the parent. 
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
    ///   Configure the home directory of for http://svc.here.dev/ to point to
    ///   D:\Develop\Here and require authentication
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <mkiisdir iisserver="svc.here.dev" dirpath="D:\Develop\Here" vdirname="/" authanonymous="false"/>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Create a virtual directory named <c>WebServices/Dev</c> pointing to 
    ///   <c>c:\MyProject\dev</c> on the web site running on port <c>81</c> of
    ///   machine <c>MyHost</c>.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <mkiisdir iisserver="MyHost:81" dirpath="c:\MyProject\dev" vdirname="WebServices/Dev" />
    ///     ]]>
    ///   </code>
    ///   Note that if <c>WebServices</c> is neither an existing virtual directory nor an
    ///   existing physical subdirectory of the web root, your IIS Management Console
    ///   will get confused. Even though <c>http://MyHost:81/WebServices/Dev/theService.asmx</c>
    ///   may be a perfectly working webservice, the Management Console will not show it.
    /// </example>
    [TaskName("mkiisdir")]
    public class CreateVirtualDirectory : WebBase {
        public enum AppType {
            None = -1,
            InProcess = 0,
            Pooled = 2,
            OutOfProcess = 1
        }

        private class IisPropertyAttribute: Attribute {
        }

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
        private AppType _apptype = AppType.Pooled;
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

        [TaskAttribute("accessexecute"),IisProperty]
        public bool AccessExecute {
            get { return _accessExecute; }
            set { _accessExecute = value; }
        }

        [TaskAttribute("accessnoremoteexecute"),IisProperty]
        public bool AccessNoRemoteExecute {
            get { return _accessNoRemoteExecute; }
            set { _accessNoRemoteExecute = value; }
        }

        [TaskAttribute("accessnoremoteread"),IisProperty]
        public bool AccessNoRemoteRead {
            get { return _accessNoRemoteRead; }
            set { _accessNoRemoteRead = value; }
        }

        [TaskAttribute("accessnoremotescript"),IisProperty]
        public bool AccessNoRemoteScript {
            get { return _accessNoRemoteScript; }
            set { _accessNoRemoteScript = value; }
        }

        [TaskAttribute("accessnoremotewrite"),IisProperty]
        public bool AccessNoRemoteWrite {
            get { return _accessNoRemoteWrite; }
            set { _accessNoRemoteWrite = value; }
        }

        [TaskAttribute("accessread"),IisProperty]
        public bool AccessRead {
            get { return _accessRead; }
            set { _accessRead = value; }
        }

        [TaskAttribute("accesssource"),IisProperty]
        public bool AccessSource {
            get { return _accessSource; }
            set { _accessSource = value; }
        }

        [TaskAttribute("accessscript"),IisProperty]
        public bool AccessScript {
            get { return _accessScript; }
            set { _accessScript = value; }
        }

        [TaskAttribute("accessssl"),IisProperty]
        public bool AccessSsl {
            get { return _accessSsl; }
            set { _accessSsl = value; }
        }

        [TaskAttribute("accessssl128"),IisProperty]
        public bool AccesssSl128 {
            get { return _accessSsl128; }
            set { _accessSsl128 = value; }
        }

        [TaskAttribute("accesssslmapcert"),IisProperty]
        public bool AccessSslMapCert {
            get { return _accessSslMapCert; }
            set { _accessSslMapCert = value; }
        }

        [TaskAttribute("accesssslnegotiatecert"),IisProperty]
        public bool AccessSslNegotiateCert {
            get { return _accessSslNegotiateCert; }
            set { _accessSslNegotiateCert = value; }
        }

        [TaskAttribute("accesssslrequirecert"),IisProperty]
        public bool AccessSslRequireCert {
            get { return _accessSslRequireCert; }
            set { _accessSslRequireCert = value; }
        }

        [TaskAttribute("accesswrite"),IisProperty]
        public bool AccessWrite {
            get { return _accessWrite; }
            set { _accessWrite = value; }
        }

        [TaskAttribute("anonymouspasswordsync"),IisProperty]
        public bool AnonymousPasswordSync {
            get { return _anonymousPasswordSync; }
            set { _anonymousPasswordSync = value; }
        }
        /// <summary>
        /// Specifies what type of application to create for this virtual directory. 
        /// The default is <see cref="AppType.Pooled" />.
        /// </summary>
        [TaskAttribute("appcreate")]
        public AppType AppCreate {
            get { return _apptype; }
            set { _apptype = value; }
        }

        [TaskAttribute("appallowclientdebug"),IisProperty]
        public bool AppAllowClientDebug {
            get { return _appAllowClientDebug; }
            set { _appAllowClientDebug = value; }
        }

        [TaskAttribute("appallowdebugging"),IisProperty]
        public bool AppAllowDebugging {
            get { return _appAllowDebugging; }
            set { _appAllowDebugging = value; }
        }

        [TaskAttribute("aspallowsessionstate"),IisProperty]
        public bool AspAllowSessionState {
            get { return _aspAllowSessionState; }
            set { _aspAllowSessionState = value; }
        }

        [TaskAttribute("aspbufferingon"),IisProperty]
        public bool AspBufferingOn {
            get { return _aspBufferingOn; }
            set { _aspBufferingOn = value; }
        }

        [TaskAttribute("aspenableapplicationrestart"),IisProperty]
        public bool AspEnableApplicationRestart {
            get { return _aspEnableApplicationRestart; }
            set { _aspEnableApplicationRestart = value; }
        }

        [TaskAttribute("aspenableasphtmlfallback"),IisProperty]
        public bool AspEnableAspHtmlFallback {
            get { return _aspEnableAspHtmlFallback; }
            set { _aspEnableAspHtmlFallback = value; }
        }

        [TaskAttribute("aspenablechunkedencoding"),IisProperty]
        public bool AspEnableChunkedEncoding {
            get { return _aspEnableChunkedEncoding; }
            set { _aspEnableChunkedEncoding = value; }
        }

        [TaskAttribute("asperrorstontlog"),IisProperty]
        public bool AspErrorsToNTLog {
            get { return _aspErrorsToNTLog; }
            set { _aspErrorsToNTLog = value; }
        }

        [TaskAttribute("aspenableparentpaths"),IisProperty]
        public bool AspEnableParentPaths {
            get { return _aspEnableParentPaths; }
            set { _aspEnableParentPaths = value; }
        }

        [TaskAttribute("aspenabletypelibcache"),IisProperty]
        public bool AspEnableTypelibCache {
            get { return _aspEnableTypelibCache; }
            set { _aspEnableTypelibCache = value; }
        }

        [TaskAttribute("aspexceptioncatchenable"),IisProperty]
        public bool AspExceptionCatchEnable {
            get { return _aspExceptionCatchEnable; }
            set { _aspExceptionCatchEnable = value; }
        }

        [TaskAttribute("asplogerrorrequests"),IisProperty]
        public bool AspLogErrorRequests {
            get { return _aspLogErrorRequests; }
            set { _aspLogErrorRequests = value; }
        }

        [TaskAttribute("aspscripterrorsenttobrowser"),IisProperty]
        public bool AspScriptErrorSentToBrowser {
            get { return _aspScriptErrorSentToBrowser; }
            set { _aspScriptErrorSentToBrowser = value; }
        }

        [TaskAttribute("aspthreadgateenabled"),IisProperty]
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

        [TaskAttribute("asptrackthreadingmodel"),IisProperty]
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

        [TaskAttribute("authanonymous"),IisProperty]
        public bool AuthAnonymous {
            get { return _authAnonymous; }
            set { _authAnonymous = value; }
        }

        [TaskAttribute("authbasic"),IisProperty]
        public bool AuthBasic {
            get { return _authBasic; }
            set { _authBasic = value; }
        }

        [TaskAttribute("authntlm"),IisProperty]
        public bool AuthNtlm {
            get { return _authNtlm; }
            set { _authNtlm = value; }
        }

        [TaskAttribute("authpersistsinglerequest"),IisProperty]
        public bool AuthPersistSingleRequest{
            get { return _authPersistSingleRequest; }
            set { _authPersistSingleRequest = value; }
        }

        [TaskAttribute("authpersistsinglerequestifproxy"),IisProperty]
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

        [TaskAttribute("authpersistsinglerequestalwaysifproxy"),IisProperty]
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

        [TaskAttribute("cachecontrolnocache"),IisProperty]
        public bool CacheControlNoCache {
            get { return _cacheControlNoCache; }
            set { _cacheControlNoCache = value; }
        }

        [TaskAttribute("cacheisapi"),IisProperty]
        public bool CacheIsapi {
            get { return _cacheIsapi; }
            set { _cacheIsapi = value; }
        }

        [TaskAttribute("contentindexed"),IisProperty]
        public bool ContentIndexed {
            get { return _contentIndexed; }
            set { _contentIndexed = value; }
        }

        [TaskAttribute("cpuappenabled"),IisProperty]
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

        [TaskAttribute("cpucgienabled"),IisProperty]
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

        [TaskAttribute("createcgiwithnewconsole"),IisProperty]
        public bool CreateCgiWithNewConsole {
            get { return _createCgiWithNewConsole; }
            set { _createCgiWithNewConsole = value; }
        }

        [TaskAttribute("createprocessasuser"),IisProperty]
        public bool CreateProcessAsUser {
            get { return _createProcessAsUser; }
            set { _createProcessAsUser = value; }
        }

        [TaskAttribute("dirbrowseshowdate"),IisProperty]
        public bool DirBrowseShowDate {
            get { return _dirBrowseShowDate; }
            set { _dirBrowseShowDate = value; }
        }

        [TaskAttribute("dirbrowseshowextension"),IisProperty]
        public bool DirBrowseShowExtension {
            get { return _dirBrowseShowExtension; }
            set { _dirBrowseShowExtension = value; }
        }

        [TaskAttribute("dirbrowseshowlongdate"),IisProperty]
        public bool DirBrowseShowLongDate {
            get { return _dirBrowseShowLongDate; }
            set { _dirBrowseShowLongDate = value; }
        }

        [TaskAttribute("dirbrowseshowsize"),IisProperty]
        public bool DirBrowseShowSize {
            get { return _dirBrowseShowSize; }
            set { _dirBrowseShowSize = value; }
        }

        [TaskAttribute("dirbrowseshowtime"),IisProperty]
        public bool DirBrowseShowTime {
            get { return _dirBrowseShowTime; }
            set { _dirBrowseShowTime = value; }
        }

        [TaskAttribute("dontlog"),IisProperty]
        public bool DontLog {
            get { return _dontLog; }
            set { _dontLog = value; }
        }

        [TaskAttribute("enabledefaultdoc"),IisProperty]
        public bool EnableDefaultDoc {
            get { return _enableDefaultDoc; }
            set { _enableDefaultDoc = value; }
        }

        [TaskAttribute("enabledirbrowsing"),IisProperty]
        public bool EnableDirBrowsing {
            get { return _enableDirBrowsing; }
            set { _enableDirBrowsing = value; }
        }

        [TaskAttribute("enabledocfooter"),IisProperty]
        public bool EnableDocFooter {
            get { return _enableDocFooter; }
            set { _enableDocFooter = value; }
        }

        [TaskAttribute("enablereversedns"),IisProperty]
        public bool EnableReverseDns {
            get { return _enableReverseDns; }
            set { _enableReverseDns = value; }
        }

        [TaskAttribute("ssiexecdisable"),IisProperty]
        public bool SsiExecDisable {
            get { return _ssiExecDisable; }
            set { _ssiExecDisable = value; }
        }

        [TaskAttribute("uncauthenticationpassthrough"),IisProperty]
        public bool UncAuthenticationPassthrough {
            get { return _uncAuthenticationPassthrough; }
            set { _uncAuthenticationPassthrough = value; }
        }

        [TaskAttribute("aspscripterrormessage"),IisProperty]
        public string AspScriptErrorMessage {
            get { return _aspScriptErrorMessage; }
            set { _aspScriptErrorMessage = value; }
        }

        [TaskAttribute("defaultdoc"),IisProperty]
        public string DefaultDoc {
            get { return _defaultDoc; }
            set { _defaultDoc = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        protected override void ExecuteTask() {
            Log(Level.Info, "Creating/modifying virtual directory '{0}' on"
                + " '{1}'.", this.VirtualDirectory, this.Server);

            // ensure IIS is available on specified host and port
            this.CheckIISSettings();

            try {
                DirectoryEntry vdir = 
                    GetOrMakeNode(this.ServerPath,this.VdirPath,"IIsWebVirtualDir");
                vdir.RefreshCache();

                vdir.Properties["Path"].Value = DirPath.FullName;
                this.CreateApplication(vdir);
                this.SetIisProperties(vdir);
  
                vdir.CommitChanges();
                vdir.Close();
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture, 
                    "Error creating virtual directory '{0}' on '{1}'.", 
                    this.VirtualDirectory, this.Server), Location, ex);
            }
        }

        #endregion Override implementation of Task

        #region Private Instance Methods

        private DirectoryEntry GetOrMakeNode(string basePath, string relPath, string schemaClassName) {
            if(DirectoryEntryExists(basePath + relPath)) {
                return new DirectoryEntry(basePath+relPath);
            }
            DirectoryEntry parent = new DirectoryEntry(basePath);
            parent.RefreshCache();
            DirectoryEntry child = parent.Children.Add(relPath.Trim('/'), schemaClassName);
            child.CommitChanges();
            parent.CommitChanges();
            parent.Close();
            return child;
        }

        private bool IsIisProperty(PropertyInfo prop) {
            return prop.GetCustomAttributes(typeof(IisPropertyAttribute),true).Length > 0;
        }

        private string AttributeName(PropertyInfo prop) {
            return ((TaskAttributeAttribute)prop.GetCustomAttributes(typeof(TaskAttributeAttribute),true)[0]).Name;
        }

        // Set the IIS properties that have been specified
        private void SetIisProperties(DirectoryEntry vdir) {
            XmlElement taskElement = (XmlElement)this.XmlNode;
            foreach(PropertyInfo prop in this.GetType().GetProperties()) {
                if(!IsIisProperty(prop)) continue;
                string propertyName = AttributeName(prop);
                if( taskElement.HasAttribute(propertyName)) {
                    Log(Level.Debug, "setting {0} = {1}",propertyName,prop.GetValue(this, null));
                    vdir.Properties[propertyName][0] = prop.GetValue(this, null);
                } 
            }
        }

        private void CreateApplication(DirectoryEntry vdir) {
            Log(Level.Debug,"setting application type {0}",AppCreate);
            if(AppCreate == AppType.None) {
                vdir.Invoke("AppDelete");
            } else if(this.Version == IISVersion.Four) {
                vdir.Invoke("AppCreate", AppCreate == AppType.InProcess);
            } else {
                vdir.Invoke("AppCreate2", (int) AppCreate);
            }
        }

        #endregion Private Instance Methods
    }
}
