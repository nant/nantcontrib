//
// NAntContrib
// Copyright (C) 2001-2002 Gerry Shaw
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

// Jayme C. Edwards (jedwards@wi.rr.com)

using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Specialized;

using NAnt.Core;
using NAnt.Core.Util;
using NAnt.Core.Tasks;
using NAnt.Core.Types;
using NAnt.DotNet.Tasks;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks {
    /// <summary>Generates code for web service clients and xml web services
    /// using ASP.NET from WSDL contract files, XSD Schemas and .discomap
    /// discovery documents. Can be used in conjunction with .disco files.</summary>
    /// <example>
    ///   <para>Generate a proxy class for a web service.</para>
    ///   <code><![CDATA[<wsdl path="http://www.somewhere.com/myservice.wsdl"
    ///     language="CS" namespace="MyCompany.MyService" outfile="MyService.cs" />]]></code>
    /// </example>
    [TaskName("wsdl")]
    [ProgramLocation(LocationType.FrameworkSdkDir)]
    public class WsdlTask : ExternalProgramBase {
        
        #region Private Instance Fields
        
        private StringBuilder _argumentBuilder;
        private string _path;
        private bool _nologo;
        private string _language;
        private bool _forserver;
        private string _namespace;
        private string _outfile;
        private string _protocol;
        private string _username;
        private string _password;
        private string _domain;
        private string _proxy;
        private string _proxyusername;
        private string _proxypassword;
        private string _proxydomain;
        private string _urlkey;
        private string _baseurl;
        
        #endregion Private Instance Fields
        
        #region Public Instance Properties

        /// <summary>URL or Path to a WSDL, XSD, or .discomap document.</summary>
        [TaskAttribute("path")]
        public string Path {
            get { return _path; }
            set { _path = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>Suppresses the banner.</summary>
        [TaskAttribute("nologo")]
        [BooleanValidator()]
        public bool NoLogo {
            get { return _nologo; }
            set { _nologo = value; }
        }

        /// <summary>Language of generated code. 'CS', 'VB', 'JS',
        /// or the fully-qualified name of a class implementing
        /// System.CodeDom.Compiler.CodeDomCompiler. </summary>
        [TaskAttribute("language")]
        public string Language {
            get { return _language; }
            set { _language = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>Compiles server-side ASP.NET abstract classes
        /// based on the web service contract. The default is to
        /// create client side proxy classes. </summary>
        [TaskAttribute("forserver")]
        [BooleanValidator()]
        public bool ForServer {
            get { return _forserver; }
            set { _forserver = value; }
        }

        /// <summary>Microsoft.NET namespace of generated classes.</summary>
        [TaskAttribute("namespace")]
        public string Namespace {
            get { return _namespace; }
            set { _namespace = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>Output filename of the created proxy. Default name is derived from the service name.</summary>
        [TaskAttribute("outfile")]
        public string OutFile {
            get { return _outfile; }
            set { _outfile = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>Override default protocol to implement. Choose from 'SOAP',
        /// 'HttpGet', 'HttpPost', or a custom protocol as specified in the
        /// configuration file.</summary>
        [TaskAttribute("protocol")]
        public string Protocol {
            get { return _protocol; }
            set { _protocol = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>Username of an account with credentials to access a
        /// server that requires authentication.</summary>
        [TaskAttribute("username")]
        public string Username {
            get { return _username; }
            set { _username = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>Password of an account with credentials to access a
        /// server that requires authentication.</summary>
        [TaskAttribute("password")]
        public string Password {
            get { return _password; }
            set { _password = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>Domain of an account with credentials to access a
        /// server that requires authentication.</summary>
        [TaskAttribute("domain")]
        public string Domain {
            get { return _domain; }
            set { _domain = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>URL of a proxy server to use for HTTP requests.
        /// The default is to use the system proxy setting.</summary>
        [TaskAttribute("proxy")]
        public string Proxy {
            get { return _proxy; }
            set { _proxy = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>Username of an account with credentials to access a
        /// proxy that requires authentication.</summary>
        [TaskAttribute("proxyusername")]
        public string ProxyUsername {
            get { return _proxyusername; }
            set { _proxyusername = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>Password of an account with credentials to access a
        /// proxy that requires authentication.</summary>
        [TaskAttribute("proxypassword")]
        public string ProxyPassword {
            get { return _proxypassword; }
            set { _proxypassword = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>Domain of an account with credentials to access a
        /// proxy that requires authentication.</summary>
        [TaskAttribute("proxydomain")]
        public string ProxyDomain {
            get { return _proxydomain; }
            set { _proxydomain = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>Configuration key to use in the code generation to
        /// read the default value for the Url property. The default is
        /// not to read from the config file.</summary>
        [TaskAttribute("urlkey")]
        public string UrlKey {
            get { return _urlkey; }
            set { _urlkey = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>Base Url to use when calculating the Url fragment.
        /// The UrlKey attribute must also be specified. </summary>
        [TaskAttribute("baseurl")]
        public string BaseUrl {
            get { return _baseurl; }
            set { _baseurl = StringUtils.ConvertEmptyToNull(value); }
        }

        #endregion Public Instance Properties

        #region Override implementation of ExternalProgramBase

        /// <summary>
        /// Gets a value indiciating whether the external program is a managed
        /// application which should be executed using a runtime engine, if 
        /// configured. 
        /// </summary>
        /// <value>
        /// <see langword="ManagedExecutionMode.Auto" />.
        /// </value>
        /// <remarks>
        /// Modifying this property has no effect.
        /// </remarks>
        public override ManagedExecution Managed {
            get { return ManagedExecution.Auto; }
            set { }
        }

        /// <summary>
        /// Gets the command-line arguments for the external program.
        /// </summary>
        /// <value>
        /// The command-line arguments for the external program.
        /// </value>
        public override string ProgramArguments {
            get {
                if (_argumentBuilder != null) {
                    return _argumentBuilder.ToString();
                } else {
                    return null;
                }
            }
        }

        protected override void ExecuteTask() {
            _argumentBuilder = new StringBuilder();

            if (NoLogo) {
                _argumentBuilder.Append(" /nologo ");
            }

            if (ForServer) {
                _argumentBuilder.Append(" /server ");
            }

            if ( Language != null) {
                _argumentBuilder.AppendFormat(CultureInfo.InvariantCulture, " /l:\"{0}\"", Language);
            }
            if (Namespace != null) {
                _argumentBuilder.AppendFormat(CultureInfo.InvariantCulture, " /n:\"{0}\"", Namespace);
            }
            if (OutFile != null) {
                _argumentBuilder.AppendFormat(CultureInfo.InvariantCulture, " /o:\"{0}\"", OutFile);
            }
            if (Protocol != null) {
                _argumentBuilder.AppendFormat(CultureInfo.InvariantCulture, " /protocol:\"{0}\"", Protocol);
            }
            if (Username != null) {
                _argumentBuilder.AppendFormat(CultureInfo.InvariantCulture, " /username:\"{0}\"", Username);
            }
            if (Password != null) {
                _argumentBuilder.AppendFormat(CultureInfo.InvariantCulture, " /password:\"{0}\"", Password);
            }
            if (Domain != null) {
                _argumentBuilder.AppendFormat(CultureInfo.InvariantCulture, " /domain:\"{0}\"", Domain);
            }
            if (Proxy != null) {
                _argumentBuilder.AppendFormat(CultureInfo.InvariantCulture, " /proxy:\"{0}\"", Proxy);
            }
            if (ProxyUsername != null) {
                _argumentBuilder.AppendFormat(CultureInfo.InvariantCulture, " /proxyusername:\"{0}\"", ProxyUsername);
            }
            if (ProxyPassword != null) {
                _argumentBuilder.AppendFormat(CultureInfo.InvariantCulture, " /proxypassword:\"{0}\"", ProxyPassword);
            }
            if (ProxyDomain != null) {
                _argumentBuilder.AppendFormat(CultureInfo.InvariantCulture, " /proxydomain:\"{0}\"", ProxyDomain);
            }
            if (UrlKey != null) {
                _argumentBuilder.AppendFormat(CultureInfo.InvariantCulture, " /appsettingurlkey:\"{0}\"", UrlKey);
            }
            if (BaseUrl != null) {
                _argumentBuilder.AppendFormat(CultureInfo.InvariantCulture, " appsettingbaseurl:\"{0}\"", BaseUrl);
            }

            _argumentBuilder.AppendFormat(CultureInfo.InvariantCulture, " \"{0}\"", Path);

            // call base class to do perform the actual call
            base.ExecuteTask();
        }
        #endregion Override implementation of ExternalProgramBase
    }
}
