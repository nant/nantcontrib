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
    ///   <para>
    ///   If the virtual directory does not exist it is created, and if it already
    ///   exists it is modified. Only the IIS-properties specified will be set. If set
    ///   by other means (e.g. the Management Console), the unspecified properties retain their current value, 
    ///   otherwise they are inherited from the parent. 
    ///   </para>
    ///   <para>
    ///   For a list of optional parameters see <see href="ms-help://MS.VSCC/MS.MSDNVS/iisref/html/psdk/asp/aore8v5e.htm">IIsWebVirtualDir</see>.
    ///   </para>
    ///   <para>
    ///   More information on metabase parameters is available <see href="http://msdn.microsoft.com/library/default.asp?url=/library/en-us/iissdk/iis/alphabeticmetabasepropertylist.asp">here</see>.
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
        /// <summary>
        /// The different ways a (virtual) directory in IIS can be configured 
        /// as an application.
        /// </summary>
        public enum AppType {
            /// <summary>
            /// Virtual directory is not configured as an application.
            /// </summary>
            None = -1,

            /// <summary>
            /// Virtual directory is configured as an in-process application.
            /// </summary>
            InProcess = 0,

            /// <summary>
            /// Virtual directory is configured as a pooled out-of-process 
            /// application. For IIS4 this is the same as <see cref="AppType.OutOfProcess" />.
            /// </summary>
            Pooled = 2,

            /// <summary>
            /// Virtual directory is configured as an out-of-process application.
            /// </summary>
            OutOfProcess = 1
        }

        private class IisPropertyAttribute: Attribute {
        }

        #region Private Instance Fields

        private DirectoryInfo _dirPath;
        private bool _accessExecute;
        private bool _accessNoRemoteExecute;
        private bool _accessNoRemoteRead;
        private bool _accessNoRemoteScript;
        private bool _accessNoRemoteWrite;
        private bool _accessRead = true;
        private bool _accessSource;
        private bool _accessScript = true;
        private bool _accessSsl;
        private bool _accessSsl128;
        private bool _accessSslMapCert;
        private bool _accessSslNegotiateCert;
        private bool _accessSslRequireCert;
        private bool _accessWrite;
        private bool _anonymousPasswordSync = true;
        private bool _appAllowClientDebug;
        private bool _appAllowDebugging;
        private AppType _apptype = AppType.Pooled;
        private bool _aspAllowSessionState = true;
        private bool _aspBufferingOn = true;
        private bool _aspEnableApplicationRestart = true;
        private bool _aspEnableAspHtmlFallback;
        private bool _aspEnableChunkedEncoding = true;
        private bool _aspErrorsToNTLog;
        private bool _aspEnableParentPaths = true;
        private bool _aspEnableTypelibCache = true;
        private bool _aspExceptionCatchEnable = true;
        private bool _aspLogErrorRequests = true;
        private bool _aspScriptErrorSentToBrowser = true;
        private bool _aspThreadGateEnabled;
        private bool _aspTrackThreadingModel;
        private bool _authAnonymous = true;
        private bool _authBasic;
        private bool _authNtlm;
        private bool _authPersistSingleRequest;
        private bool _authPersistSingleRequestIfProxy = true;
        private bool _authPersistSingleRequestAlwaysIfProxy;
        private bool _cacheControlNoCache;
        private bool _cacheIsapi = true;
        private bool _contentIndexed = true;
        private bool _cpuAppEnabled = true;
        private bool _cpuCgiEnabled = true;
        private bool _createCgiWithNewConsole;
        private bool _createProcessAsUser = true;
        private bool _dirBrowseShowDate = true;
        private bool _dirBrowseShowExtension = true;
        private bool _dirBrowseShowLongDate = true;
        private bool _dirBrowseShowSize = true;
        private bool _dirBrowseShowTime = true;
        private bool _dontLog;
        private bool _enableDefaultDoc = true;
        private bool _enableDirBrowsing;
        private bool _enableDocFooter;
        private bool _enableReverseDns;
        private bool _ssiExecDisable;
        private bool _uncAuthenticationPassthrough;
        private string _aspScriptErrorMessage = "An error occurred on the server when processing the URL.  Please contact the system administrator.";
        private string _defaultDoc = "Default.htm,Default.asp,index.htm,iisstart.asp,Default.aspx";
        private string _uncUserName;
        private string  _uncPassword;
        private string _appFriendlyName;
        private string _appPoolId;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The user-friendly name of the package or application.
        /// </summary>
        [TaskAttribute("appfriendlyname"), IisProperty]
        public string  AppFriendlyName {
            get { return _appFriendlyName; }
            set { _appFriendlyName = value; }
        }

        /// <summary>
        /// The file system path.
        /// </summary>
        [TaskAttribute("dirpath", Required=true)]
        public DirectoryInfo DirPath {
            get { return _dirPath; }
            set { _dirPath = value; }
        }

        /// <summary>
        /// Indicates whether the file or the contents of the folder may be 
        /// executed, regardless of file type. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("accessexecute"),IisProperty]
        public bool AccessExecute {
            get { return _accessExecute; }
            set { _accessExecute = value; }
        }

        /// <summary>
        /// Indicates whether remote requests to execute applications are denied; 
        /// only requests from the same computer as the IIS server succeed if 
        /// <see cref="AccessExecute" /> is set to <see langword="true" />. You 
        /// cannot set <see cref="AccessNoRemoteExecute" /> to <see langword="false" />
        /// to enable remote requests, and set <see cref="AccessExecute" /> to 
        /// <see langword="false" /> to disable local requests. The default is
        /// <see langword="false" />.
        /// </summary>
        [TaskAttribute("accessnoremoteexecute"),IisProperty]
        public bool AccessNoRemoteExecute {
            get { return _accessNoRemoteExecute; }
            set { _accessNoRemoteExecute = value; }
        }

        /// <summary>
        /// Indicates whether remote requests to view files are denied; 
        /// only requests from the same computer as the IIS server succeed if 
        /// <see cref="AccessExecute" /> is set to <see langword="true" />. You 
        /// cannot set <see cref="AccessNoRemoteRead" /> to <see langword="false" />
        /// to enable remote requests, and set <see cref="AccessRead" /> to 
        /// <see langword="false" /> to disable local requests. The default is
        /// <see langword="false" />.
        /// </summary>
        [TaskAttribute("accessnoremoteread"),IisProperty]
        public bool AccessNoRemoteRead {
            get { return _accessNoRemoteRead; }
            set { _accessNoRemoteRead = value; }
        }

        /// <summary>
        /// A value of true indicates that remote requests to view dynamic content are denied; only requests from the same computer as the IIS server succeed if the AccessScript property is set to true. You cannot set AccessNoRemoteScript to false to enable remote requests, and set AccessScript to false to disable local requests.
        /// </summary>
        [TaskAttribute("accessnoremotescript"),IisProperty]
        public bool AccessNoRemoteScript {
            get { return _accessNoRemoteScript; }
            set { _accessNoRemoteScript = value; }
        }

        /// <summary>
        /// indicates that remote requests to create or change files are denied; only requests from the same computer as the IIS server succeed if the AccessWrite property is set to true. You cannot set AccessNoRemoteWrite to false to enable remote requests, and set AccessWrite to false to disable local requests.
        /// </summary>
        [TaskAttribute("accessnoremotewrite"),IisProperty]
        public bool AccessNoRemoteWrite {
            get { return _accessNoRemoteWrite; }
            set { _accessNoRemoteWrite = value; }
        }

        /// <summary>
        /// Indicates whether the file or the contents of the folder may be 
        /// read. The default is <see langword="true" />.
        /// </summary>
        [TaskAttribute("accessread"),IisProperty]
        public bool AccessRead {
            get { return _accessRead; }
            set { _accessRead = value; }
        }

        /// <summary>
        /// Indicates whether users are allowed to access source code if either 
        /// Read or Write permissions are set. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("accesssource"),IisProperty]
        public bool AccessSource {
            get { return _accessSource; }
            set { _accessSource = value; }
        }

        /// <summary>
        /// Indicates whether the file or the contents of the folder may be 
        /// executed if they are script files or static content. The default
        /// is <see langword="true" />.
        /// </summary>
        [TaskAttribute("accessscript"),IisProperty]
        public bool AccessScript {
            get { return _accessScript; }
            set { _accessScript = value; }
        }

        /// <summary>
        /// Indicates whether file access requires SSL file permission processing, 
        /// with or without a client certificate. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("accessssl"),IisProperty]
        public bool AccessSsl {
            get { return _accessSsl; }
            set { _accessSsl = value; }
        }

        /// <summary>
        /// Indicates whether file access requires SSL file permission processing 
        /// with a minimum key size of 128 bits, with or without a client 
        /// certificate. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("accessssl128"),IisProperty]
        public bool AccesssSl128 {
            get { return _accessSsl128; }
            set { _accessSsl128 = value; }
        }

        /// <summary>
        /// Indicates whether SSL file permission processing maps a client 
        /// certificate to a Microsoft Windows ® operating system user-account. 
        /// <see cref="AccessSslNegotiateCert" /> must also be set to 
        /// <see langword="true" /> for the mapping to occur. The default is
        /// <see langword="false" />.
        /// </summary>
        [TaskAttribute("accesssslmapcert"),IisProperty]
        public bool AccessSslMapCert {
            get { return _accessSslMapCert; }
            set { _accessSslMapCert = value; }
        }

        /// <summary>
        /// Indicates whether SSL file access processing requests a certificate 
        /// from the client. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("accesssslnegotiatecert"),IisProperty]
        public bool AccessSslNegotiateCert {
            get { return _accessSslNegotiateCert; }
            set { _accessSslNegotiateCert = value; }
        }

        /// <summary>
        /// Indicates whether SSL file access processing requests a certificate 
        /// from the client. If the client provides no certificate, the connection 
        /// is closed. <see cref="AccessSslNegotiateCert" /> must also be set to 
        /// <see langword="true" /> when using <see cref="AccessSslRequireCert" />.
        /// The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("accesssslrequirecert"),IisProperty]
        public bool AccessSslRequireCert {
            get { return _accessSslRequireCert; }
            set { _accessSslRequireCert = value; }
        }

        /// <summary>
        /// Indicates whether users are allowed to upload files and their 
        /// associated properties to the enabled directory on your server or 
        /// to change content in a Write-enabled file. The default is
        /// <see langword="false" />.
        /// </summary>
        [TaskAttribute("accesswrite"),IisProperty]
        public bool AccessWrite {
            get { return _accessWrite; }
            set { _accessWrite = value; }
        }

        /// <summary>
        /// Indicates whether IIS should handle the user password for anonymous 
        /// users attempting to access resources. The default is 
        /// <see langword="false" />.
        /// </summary>
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

        /// <summary>
        /// Specifies whether ASP client-side debugging is enabled. The default
        /// is <see langword="false" />.
        /// </summary>
        [TaskAttribute("appallowclientdebug"),IisProperty]
        public bool AppAllowClientDebug {
            get { return _appAllowClientDebug; }
            set { _appAllowClientDebug = value; }
        }

        /// <summary>
        /// Specifies whether ASP debugging is enabled on the server. The default
        /// is <see langword="false" />.
        /// </summary>
        [TaskAttribute("appallowdebugging"),IisProperty]
        public bool AppAllowDebugging {
            get { return _appAllowDebugging; }
            set { _appAllowDebugging = value; }
        }

        /// <summary>
        /// Specifies the application pool where the application is routed
        /// (IIS 6.0 or higher).
        /// </summary>
        [TaskAttribute("apppoolid"),IisProperty]
        public string AppPoolId {
            get { return _appPoolId; }
            set { _appPoolId = value; }
        }

        /// <summary>
        /// Enables session state persistence for the ASP application. The 
        /// default is <see langword="true" />.
        /// </summary>
        [TaskAttribute("aspallowsessionstate"),IisProperty]
        public bool AspAllowSessionState {
            get { return _aspAllowSessionState; }
            set { _aspAllowSessionState = value; }
        }

        /// <summary>
        /// Specifies whether output from an ASP application will be buffered. 
        /// If <see langword="true" />, all output from the application is 
        /// collected in the buffer before the buffer is flushed to the client. 
        /// With buffering on, the ASP application has to completely process the 
        /// ASP script before the client receives any output. The default is 
        /// <see langword="true" />.
        /// </summary>
        [TaskAttribute("aspbufferingon"),IisProperty]
        public bool AspBufferingOn {
            get { return _aspBufferingOn; }
            set { _aspBufferingOn = value; }
        }

        /// <summary>
        /// Determines whether an ASP application can be automatically restarted. 
        /// When changes are made to Global.asa or metabase properties that affect 
        /// an application, the application will not restart unless the 
        /// <see cref="AspEnableApplicationRestart" /> property is set to 
        /// <see langword="false" />. The default is <see langword="true" />.
        /// </summary>
        /// <remarks>
        /// When this property is changed from <see langword="false" /> to 
        /// <see langword="true" />, the application immediately restarts.
        /// </remarks>
        [TaskAttribute("aspenableapplicationrestart"),IisProperty]
        public bool AspEnableApplicationRestart {
            get { return _aspEnableApplicationRestart; }
            set { _aspEnableApplicationRestart = value; }
        }

        /// <summary>
        /// Controls the behavior of ASP when a new request is to be rejected 
        /// due to a full request queue. If <see langword="true" />, an .htm file 
        /// with a similar name as the requested .asp file, will be sent instead 
        /// of the .asp file. The naming convention for the .htm file is the 
        /// name of the .asp file with _asp appended.  The default is
        /// <see langword="false" />.
        /// </summary>
        [TaskAttribute("aspenableasphtmlfallback"),IisProperty]
        public bool AspEnableAspHtmlFallback {
            get { return _aspEnableAspHtmlFallback; }
            set { _aspEnableAspHtmlFallback = value; }
        }

        /// <summary>
        /// Specifies whether HTTP 1.1 chunked transfer encoding is enabled for 
        /// the World Wide Web Publishing Service (WWW service). The default is
        /// <see langword="false" />.
        /// </summary>
        [TaskAttribute("aspenablechunkedencoding"),IisProperty]
        public bool AspEnableChunkedEncoding {
            get { return _aspEnableChunkedEncoding; }
            set { _aspEnableChunkedEncoding = value; }
        }

        /// <summary>
        /// Specifies which ASP errors are written to the Windows event log. 
        /// </summary>
        /// <remarks>
        ///   <para>
        ///   ASP errors are written to the client browser and to the IIS log files 
        ///   by default. <see cref="AspLogErrorRequests" /> is set to <see langword="true" />
        ///   by default, and is modified by <see cref="AspErrorsToNTLog" /> in 
        ///   the following way:
        ///   </para>
        ///   <para>
        ///   If <see cref="AspLogErrorRequests" /> is set to <see langword="false" />, 
        ///   then ASP errors are not written to the Windows event log, regardless 
        ///   of the value of <see cref="AspErrorsToNTLog" />.
        ///   </para>
        ///   <para> 
        ///   If <see cref="AspLogErrorRequests" /> is set to <see langword="true" />, 
        ///   and if IIS fails to write an item to the IIS log file, the item is 
        ///   written to the Windows event log as a warning, regardless of the 
        ///   value of <see cref="AspErrorsToNTLog" />. 
        ///   </para>
        ///   <para>
        ///   If <see cref="AspLogErrorRequests" /> is set to <see langword="true" />
        ///   and <see cref="AspErrorsToNTLog" /> is set to <see langword="false" />, 
        ///   then only the most serious ASP errors are sent to the Windows event log. 
        ///   Serious ASP error numbers are: 100, 101, 102, 103, 104, 105, 106, 107, 
        ///   115, 190, 191, 192, 193, 194, 240, 241, and 242.
        ///   </para>
        ///   <para>
        ///   If <see cref="AspLogErrorRequests" /> is set to <see langword="true" />
        ///   and <see cref="AspErrorsToNTLog" /> is set to <see langword="true" />, 
        ///   then all ASP errors are written to the Windows event log.
        ///   </para>
        /// </remarks>
        [TaskAttribute("asperrorstontlog"),IisProperty]
        public bool AspErrorsToNTLog {
            get { return _aspErrorsToNTLog; }
            set { _aspErrorsToNTLog = value; }
        }

        /// <summary>
        /// Specifies whether an ASP page allows paths relative to the current 
        /// directory. The default is <see langword="true" />.
        /// </summary>
        [TaskAttribute("aspenableparentpaths"),IisProperty]
        public bool AspEnableParentPaths {
            get { return _aspEnableParentPaths; }
            set { _aspEnableParentPaths = value; }
        }

        /// <summary>
        /// Specifies whether type libraries are cached on the server. The 
        /// default is <see langword="true" />.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///   The World Wide Web Publishing Service (WWW service) setting for 
        ///   this property is applicable to all in-process and pooled out-of-process
        ///   application nodes, at all levels.
        ///   </para>
        ///   <para>
        ///   Metabase settings at the Web server level or lower are ignored
        ///   for in-process and pooled out-of-process applications. However, 
        ///   settings at the Web server level or lower are used if that node
        ///   is an isolated out-of-process application.
        ///   </para>
        /// </remarks>
        [TaskAttribute("aspenabletypelibcache"),IisProperty]
        public bool AspEnableTypelibCache {
            get { return _aspEnableTypelibCache; }
            set { _aspEnableTypelibCache = value; }
        }

        /// <summary>
        /// Specifies whether ASP pages trap exceptions thrown by components. 
        /// If set to <see langword="false" />, the Microsoft Script Debugger tool
        /// does not catch exceptions sent by the component that you are debugging.
        /// The default is <see langword="true" />.
        /// </summary>
        [TaskAttribute("aspexceptioncatchenable"),IisProperty]
        public bool AspExceptionCatchEnable {
            get { return _aspExceptionCatchEnable; }
            set { _aspExceptionCatchEnable = value; }
        }

        /// <summary>
        /// Controls whether the Web server writes ASP errors to the application 
        /// section of the Windows event log. The default is <see langword="true" />.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///   ASP errors are written to the client browser and to the IIS log files 
        ///   by default. <see cref="AspLogErrorRequests" /> is set to <see langword="true" />
        ///   by default, and is modified by <see cref="AspErrorsToNTLog" /> in 
        ///   the following way:
        ///   </para>
        ///   <para>
        ///   If <see cref="AspLogErrorRequests" /> is set to <see langword="false" />, 
        ///   then ASP errors are not written to the Windows event log, regardless 
        ///   of the value of <see cref="AspErrorsToNTLog" />.
        ///   </para>
        ///   <para> 
        ///   If <see cref="AspLogErrorRequests" /> is set to <see langword="true" />, 
        ///   and if IIS fails to write an item to the IIS log file, the item is 
        ///   written to the Windows event log as a warning, regardless of the 
        ///   value of <see cref="AspErrorsToNTLog" />. 
        ///   </para>
        ///   <para>
        ///   If <see cref="AspLogErrorRequests" /> is set to <see langword="true" />
        ///   and <see cref="AspErrorsToNTLog" /> is set to <see langword="false" />, 
        ///   then only the most serious ASP errors are sent to the Windows event log. 
        ///   Serious ASP error numbers are: 100, 101, 102, 103, 104, 105, 106, 107, 
        ///   115, 190, 191, 192, 193, 194, 240, 241, and 242.
        ///   </para>
        ///   <para>
        ///   If <see cref="AspLogErrorRequests" /> is set to <see langword="true" />
        ///   and <see cref="AspErrorsToNTLog" /> is set to <see langword="true" />, 
        ///   then all ASP errors are written to the Windows event log.
        ///   </para>
        /// </remarks>
        [TaskAttribute("asplogerrorrequests"),IisProperty]
        public bool AspLogErrorRequests {
            get { return _aspLogErrorRequests; }
            set { _aspLogErrorRequests = value; }
        }

        /// <summary>
        /// Specifies whether the Web server writes debugging specifics
        /// (file name, error, line number, description) to the client browser, 
        /// in addition to logging them to the Windows Event Log. The default
        /// is <see langword="true" />.
        /// </summary>
        [TaskAttribute("aspscripterrorsenttobrowser"),IisProperty]
        public bool AspScriptErrorSentToBrowser {
            get { return _aspScriptErrorSentToBrowser; }
            set { _aspScriptErrorSentToBrowser = value; }
        }

        /// <summary>
        /// Indicates whether IIS thread gating is enabled (only applies to IIS 4 and 5).
        /// The default is <see langword="false" />.
        /// </summary>
        /// <remarks>
        /// IIS performs thread gating to dynamically control the number of 
        /// concurrently executing threads, in response to varying load conditions. 
        /// </remarks>
        [TaskAttribute("aspthreadgateenabled"),IisProperty]
        public bool AspThreadGateEnabled {
            get { 
                if (this.Version != IISVersion.Five || this.Version != IISVersion.Four) {
                    throw new BuildException("Option only applies to IIS version 5.x or less", Location);
                }
                return _aspThreadGateEnabled;
            }
            set { 
                if (this.Version != IISVersion.Five || this.Version != IISVersion.Four) {
                    throw new BuildException("Option only applies to IIS version 5.x or less", Location);
                }
                _aspThreadGateEnabled = value; 
            }
        }

        /// <summary>
        /// Specifies whether IIS checks the threading model of any components 
        /// that your application creates. The default is <see langword="false" />.
        /// </summary>
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

        /// <summary>
        /// Specifies Anonymous authentication as one of the possible authentication
        /// schemes returned to clients as being available. The default is
        /// <see langword="true" />.
        /// </summary>
        [TaskAttribute("authanonymous"),IisProperty]
        public bool AuthAnonymous {
            get { return _authAnonymous; }
            set { _authAnonymous = value; }
        }

        /// <summary>
        /// Specifies Basic authentication as one of the possible authentication 
        /// schemes returned to clients as being available. The default is
        /// <see langword="false" />.
        /// </summary>
        [TaskAttribute("authbasic"),IisProperty]
        public bool AuthBasic {
            get { return _authBasic; }
            set { _authBasic = value; }
        }

        /// <summary>
        /// Specifies Integrated Windows authentication as one of the possible 
        /// authentication schemes returned to clients as being available. The
        /// default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("authntlm"),IisProperty]
        public bool AuthNtlm {
            get { return _authNtlm; }
            set { _authNtlm = value; }
        }

        /// <summary>
        /// Specifies that authentication persists only for a single request on 
        /// a connection. IIS resets the authentication at the end of each request, 
        /// and forces re-authentication on the next request of the session.
        /// </summary>
        /// <remarks>
        /// [IIS 6.0] When the AuthPersistSingleRequest flag is set to true when 
        /// using NTLM authentication, IIS 6.0 automatically reauthenticates every 
        /// request, even those on the same connection.
        /// </remarks>
        [TaskAttribute("authpersistsinglerequest"),IisProperty]
        public bool AuthPersistSingleRequest {
            get { return _authPersistSingleRequest; }
            set { _authPersistSingleRequest = value; }
        }

        /// <summary>
        /// Specifies authentication will persist only across single requests 
        /// on a connection if the connection is by proxy. Applies to IIS 5.0 
        /// and 5.1. The default is <see langword="false" />
        /// </summary>
        /// <remarks>
        /// IIS will reset the authentication at the end of the request if the current authenticated 
        /// request is by proxy and it is not the special case where IIS is running MSPROXY
        /// </remarks>
        [TaskAttribute("authpersistsinglerequestifproxy"),IisProperty]
        public bool AuthPersistSingleRequestIfProxy {
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

        /// <summary>
        /// Specifies whether authentication is valid for a single request 
        /// if by proxy. IIS will reset the authentication at the end of the 
        /// request and force re-authentication on the next request if the 
        /// current authenticated request is by proxy of any type. Applies to
        /// IIS 5.0 and 5.1. The default is <see langword="false" />.
        /// </summary>
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

        /// <summary>
        /// Specifies whether the HTTP 1.1 directive to prevent caching of content
        /// should be sent to clients. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("cachecontrolnocache"),IisProperty]
        public bool CacheControlNoCache {
            get { return _cacheControlNoCache; }
            set { _cacheControlNoCache = value; }
        }

        /// <summary>
        /// Indicates whether ISAPI extensions are cached in memory after first 
        /// use. The default is <see langword="true" />.
        /// </summary>
        [TaskAttribute("cacheisapi"),IisProperty]
        public bool CacheIsapi {
            get { return _cacheIsapi; }
            set { _cacheIsapi = value; }
        }

        /// <summary>
        /// Specifies whether the installed content indexer should index content 
        /// under this directory tree. The default is <see langword="true" />.
        /// </summary>
        [TaskAttribute("contentindexed"),IisProperty]
        public bool ContentIndexed {
            get { return _contentIndexed; }
            set { _contentIndexed = value; }
        }

        /// <summary>
        /// Specifies whether process accounting and throttling should be performed 
        /// for ISAPI extensions and ASP applications. The default is
        /// <see langword="true" />.
        /// </summary>
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

        /// <summary>
        /// Indicates whether IIS should perform process accounting for CGI 
        /// applications. The default is <see langword="true" />.
        /// </summary>
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

        /// <summary>
        /// Indicates whether a CGI application runs in its own console. The
        /// default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("createcgiwithnewconsole"),IisProperty]
        public bool CreateCgiWithNewConsole {
            get { return _createCgiWithNewConsole; }
            set { _createCgiWithNewConsole = value; }
        }

        /// <summary>
        /// Specifies whether a CGI process is created in the system context 
        /// or in the context of the requesting user. The default is
        /// <see langword="true" />.
        /// </summary>
        [TaskAttribute("createprocessasuser"),IisProperty]
        public bool CreateProcessAsUser {
            get { return _createProcessAsUser; }
            set { _createProcessAsUser = value; }
        }

        /// <summary>
        /// Specifies whether date information is displayed when browsing 
        /// directories. The default is <see langword="true" />.
        /// </summary>
        [TaskAttribute("dirbrowseshowdate"),IisProperty]
        public bool DirBrowseShowDate {
            get { return _dirBrowseShowDate; }
            set { _dirBrowseShowDate = value; }
        }

        /// <summary>
        /// Specifies whether file extensions are displayed when browsing 
        /// directories. The default is <see langword="true" />.
        /// </summary>
        [TaskAttribute("dirbrowseshowextension"),IisProperty]
        public bool DirBrowseShowExtension {
            get { return _dirBrowseShowExtension; }
            set { _dirBrowseShowExtension = value; }
        }

        /// <summary>
        /// Specifies whether date information is displayed in extended format 
        /// when displaying directories. The default is <see langword="true" />.
        /// </summary>
        [TaskAttribute("dirbrowseshowlongdate"),IisProperty]
        public bool DirBrowseShowLongDate {
            get { return _dirBrowseShowLongDate; }
            set { _dirBrowseShowLongDate = value; }
        }

        /// <summary>
        /// Specifies whether file size information is displayed when displaying 
        /// directories. The default is <see langword="true" />.
        /// </summary>
        [TaskAttribute("dirbrowseshowsize"),IisProperty]
        public bool DirBrowseShowSize {
            get { return _dirBrowseShowSize; }
            set { _dirBrowseShowSize = value; }
        }

        /// <summary>
        /// Specifies whether file creation time is displayed when browsing 
        /// directories. The default is <see langword="true" />.
        /// </summary>
        [TaskAttribute("dirbrowseshowtime"),IisProperty]
        public bool DirBrowseShowTime {
            get { return _dirBrowseShowTime; }
            set { _dirBrowseShowTime = value; }
        }

        /// <summary>
        /// Specifies whether client requests are written to the IIS log files.
        /// The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("dontlog"),IisProperty]
        public bool DontLog {
            get { return _dontLog; }
            set { _dontLog = value; }
        }

        /// <summary>
        /// When set to true, the default document (specified by the DefaultDoc property) for a directory is loaded when the directory is browsed.
        /// </summary>
        [TaskAttribute("enabledefaultdoc"),IisProperty]
        public bool EnableDefaultDoc {
            get { return _enableDefaultDoc; }
            set { _enableDefaultDoc = value; }
        }

        /// <summary>
        /// Specifies whether directory browsing is enabled. The default is
        /// <see langword="false" />.
        /// </summary>
        [TaskAttribute("enabledirbrowsing"),IisProperty]
        public bool EnableDirBrowsing {
            get { return _enableDirBrowsing; }
            set { _enableDirBrowsing = value; }
        }

        /// <summary>
        /// Enables or disables custom footers. The default is 
        /// <see langword="false" />.
        /// </summary>
        [TaskAttribute("enabledocfooter"),IisProperty]
        public bool EnableDocFooter {
            get { return _enableDocFooter; }
            set { _enableDocFooter = value; }
        }

        /// <summary>
        /// Enables or disables reverse Domain Name Server (DNS) lookups for 
        /// the World Wide Web Publishing Service (WWW service). The default is
        /// <see langword="false" />.
        /// </summary>
        [TaskAttribute("enablereversedns"),IisProperty]
        public bool EnableReverseDns {
            get { return _enableReverseDns; }
            set { _enableReverseDns = value; }
        }

        /// <summary>
        /// Specifies whether server-side include (SSI) #exec directives are 
        /// disabled under this path. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("ssiexecdisable"),IisProperty]
        public bool SsiExecDisable {
            get { return _ssiExecDisable; }
            set { _ssiExecDisable = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [TaskAttribute("uncauthenticationpassthrough"),IisProperty]
        public bool UncAuthenticationPassthrough {
            get { return _uncAuthenticationPassthrough; }
            set { _uncAuthenticationPassthrough = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [TaskAttribute("aspscripterrormessage"),IisProperty]
        public string AspScriptErrorMessage {
            get { return _aspScriptErrorMessage; }
            set { _aspScriptErrorMessage = value; }
        }

        /// <summary>
        /// One or more file names of default documents that will be returned to 
        /// the client if no file name is included in the client's request.
        /// </summary>
        [TaskAttribute("defaultdoc"),IisProperty]
        public string DefaultDoc {
            get { return _defaultDoc; }
            set { _defaultDoc = value; }
        }

        /// <summary>
        /// Specifies the user name for Universal Naming Convention (UNC) virtual 
        /// roots.
        /// </summary>
        [TaskAttribute("uncusername"),IisProperty]
        public string UncUserName {
            get { return _uncUserName; }
            set { _uncUserName = value; }
        }

        /// <summary>
        /// Specifies the encrypted password used to gain access to UNC 
        /// (Universal Naming Convention) virtual roots.
        /// </summary>
        [TaskAttribute("uncpassword"),IisProperty]
        public string UncPassword {
            get { return _uncPassword; }
            set { _uncPassword = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        protected override void ExecuteTask() {
            // ensure IIS is available on specified host and port
            this.CheckIISSettings();

            Log(Level.Info, "Creating/modifying virtual directory '{0}' on"
                + " '{1}' (website: {2}).", this.VirtualDirectory, this.Server,
                this.Website);

            try {
                DirectoryEntry vdir = 
                    GetOrMakeNode(this.ServerPath,this.VdirPath,"IIsWebVirtualDir");
                vdir.RefreshCache();

                vdir.Properties["Path"].Value = DirPath.FullName;
                CreateApplication(vdir);
                SetIisProperties(vdir);
  
                vdir.CommitChanges();
                vdir.Close();
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture, 
                    "Error creating virtual directory '{0}' on '{1}' (website: {2}).", 
                    this.VirtualDirectory, this.Server, this.Website), Location, ex);
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
                if (!IsIisProperty(prop)) {
                    continue;
                }
                string propertyName = AttributeName(prop);
                if (taskElement.HasAttribute(propertyName)) {
                    Log(Level.Debug, "Setting {0} = {1}", propertyName, prop.GetValue(this, null));
                    vdir.Properties[propertyName][0] = prop.GetValue(this, null);
                } 
            }
        }

        private void CreateApplication(DirectoryEntry vdir) {
            Log(Level.Debug, "Setting application type \"{0}\".", AppCreate);
            if (AppCreate == AppType.None) {
                vdir.Invoke("AppDelete");
            } else if (Version == IISVersion.Four) {
                vdir.Invoke("AppCreate", AppCreate == AppType.InProcess);
            } else {
                vdir.Invoke("AppCreate2", (int) AppCreate);
            }
        }

        #endregion Private Instance Methods
    }
}
