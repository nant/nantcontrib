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
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Specialized;

using SourceForge.NAnt;
using SourceForge.NAnt.Tasks;
using SourceForge.NAnt.Attributes;

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
    public class WsdlTask : MsftFXSDKExternalProgramBase {
        private string _args;
        string _path = null;
        bool _nologo = false;
        string _language = null;
        bool _forserver = false;
        string _namespace = null;
        string _outfile = null;
        string _protocol = null;
        string _username = null;
        string _password = null;
        string _domain = null;
        string _proxy = null;
        string _proxyusername = null;
        string _proxypassword = null;
        string _proxydomain = null;
        string _urlkey = null;
        string _baseurl = null;

        /// <summary>URL or Path to a WSDL, XSD, or .discomap document.</summary>
        [TaskAttribute("path")]
        public string Path {
            get { return _path; }
            set { _path = value; } 
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
            set { _language = value; }
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
            set { _namespace = value; }
        }

        /// <summary>Output filename of the created proxy. Default name is derived from the service name.</summary>
        [TaskAttribute("outfile")]        
        public string OutFile {
            get { return _outfile; }
            set { _outfile = value; }
        }

        /// <summary>Override default protocol to implement. Choose from 'SOAP', 
        /// 'HttpGet', 'HttpPost', or a custom protocol as specified in the 
        /// configuration file.</summary>
        [TaskAttribute("protocol")]        
        public string Protocol {
            get { return _protocol; }
            set { _protocol = value; }
        }

        /// <summary>Username of an account with credentials to access a 
        /// server that requires authentication.</summary>
        [TaskAttribute("username")]        
        public string Username {
            get { return _username; }
            set { _username = value; }
        }

        /// <summary>Password of an account with credentials to access a 
        /// server that requires authentication.</summary>
        [TaskAttribute("password")]        
        public string Password {
            get { return _password; }
            set { _password = value; }
        }

        /// <summary>Domain of an account with credentials to access a 
        /// server that requires authentication.</summary>
        [TaskAttribute("domain")]        
        public string Domain {
            get { return _domain; }
            set { _domain = value; }
        }

        /// <summary>URL of a proxy server to use for HTTP requests. 
        /// The default is to use the system proxy setting.</summary>
        [TaskAttribute("proxy")]        
        public string Proxy {
            get { return _proxy; }
            set { _proxy = value; }
        }

        /// <summary>Username of an account with credentials to access a 
        /// proxy that requires authentication.</summary>
        [TaskAttribute("proxyusername")]        
        public string ProxyUsername {
            get { return _proxyusername; }
            set { _proxyusername = value; }
        }

        /// <summary>Password of an account with credentials to access a 
        /// proxy that requires authentication.</summary>
        [TaskAttribute("proxypassword")]        
        public string ProxyPassword {
            get { return _proxypassword; }
            set { _proxypassword = value; }
        }

        /// <summary>Domain of an account with credentials to access a 
        /// proxy that requires authentication.</summary>
        [TaskAttribute("proxydomain")]        
        public string ProxyDomain {
            get { return _proxydomain; }
            set { _proxydomain = value; }
        }

        /// <summary>Configuration key to use in the code generation to 
        /// read the default value for the Url property. The default is 
        /// not to read from the config file.</summary>
        [TaskAttribute("urlkey")]        
        public string UrlKey {
            get { return _urlkey; }
            set { _urlkey = value; }
        }

        /// <summary>Base Url to use when calculating the Url fragment. 
        /// The UrlKey attribute must also be specified. </summary>
        [TaskAttribute("baseurl")]        
        public string BaseUrl {
            get { return _baseurl; }
            set { _baseurl = value; }
        }

        /// <summary>
        /// Arguments of program to execute
        /// </summary>
        public override string ProgramArguments {
            get { return _args; }
        }

        ///<summary>
        ///Initializes task and ensures the supplied attributes are valid.
        ///</summary>
        ///<param name="taskNode">Xml node used to define this task instance.</param>
        protected override void InitializeTask(System.Xml.XmlNode taskNode) {
        }

        protected override void ExecuteTask() {
            StringBuilder arguments = new StringBuilder();

            if (NoLogo) {
                arguments.Append("/nologo ");
            }

            if (ForServer) {
                arguments.Append("/server ");
            }

            if (Language != null) {
                arguments.Append(" /l:");
                arguments.Append(Language);
            }
            if (Namespace != null) {
                arguments.Append(" /n:");
                arguments.Append(Namespace);
            }
            if (OutFile != null) {
                arguments.Append(" /o:");
                arguments.Append(OutFile);
            }
            if (Protocol != null) {
                arguments.Append(" /protocol:");
                arguments.Append(Protocol);
            }
            if (Username != null) {
                arguments.Append(" /username:");
                arguments.Append(Username);
            }
            if (Password != null) {
                arguments.Append(" /password:");
                arguments.Append(Password);
            }
            if (Domain != null) {
                arguments.Append(" /domain:");
                arguments.Append(Domain);
            }
            if (Proxy != null) {
                arguments.Append(" /proxy:");
                arguments.Append(Proxy);
            }
            if (ProxyUsername != null) {
                arguments.Append(" /proxyusername:");
                arguments.Append(ProxyUsername);
            }
            if (ProxyPassword != null) {
                arguments.Append(" /proxypassword:");
                arguments.Append(ProxyPassword);
            }
            if (ProxyDomain != null) {
                arguments.Append(" /proxydomain:");
                arguments.Append(ProxyDomain);
            }
            if (UrlKey != null) {
                arguments.Append(" /appsettingurlkey:");
                arguments.Append(UrlKey);
            }
            if (BaseUrl != null)            {
                arguments.Append(" /appsettingbaseurl:");
                arguments.Append(BaseUrl);
            }

            arguments.Append(Path);

            try {
                _args = arguments.ToString();

                base.ExecuteTask();
            }
            catch (Exception e) {
                throw new BuildException(LogPrefix + "ERROR: " + e);
            }
        }
    }
}
