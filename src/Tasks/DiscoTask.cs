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
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Discovers the URLs of XML web services on a web server and saves documents
    /// related to them to the local disk. The resulting .discomap, .wsdl, and .xsd files
    /// can be used with the <see cref="WsdlTask" /> to produce web service clients and
    /// and abstract web service servers using ASP.NET.
    /// </summary>
    /// <example>
    ///   <para>Generate a proxy class for a web service.</para>
    ///   <code>
    ///     <![CDATA[
    /// <disco 
    ///     path="http://www.somewhere.com/myservice.wsdl"
    ///     language="CS" 
    ///     namespace="MyCompany.MyService" 
    ///     outfile="MyService.cs" 
    /// />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("disco")]
    [ProgramLocation(LocationType.FrameworkSdkDir)]
    public class DicsoTask : ExternalProgramBase {
        #region Private Instance Fields
        
        private StringBuilder _argumentBuilder = null;
        private string _path = null;
        private bool _nologo = true;
        private bool _nosave;
        private string _outputdir = null;
        private string _username = null;
        private string _password = null;
        private string _domain = null;
        private string _proxy = null;
        private string _proxyusername = null;
        private string _proxypassword = null;
        private string _proxydomain = null;
        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>The URL or Path to discover.</summary>
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

        /// <summary>Do not save the discovered documents to the local disk.</summary>
        [TaskAttribute("nosave")]
        [BooleanValidator()]
        public bool NoSave {
            get { return _nosave; }
            set { _nosave = value; }
        }

        /// <summary>The output directory to save discovered documents in.</summary>
        [TaskAttribute("outputdir")]
        public string OutputDir {
            get { return _outputdir; }
            set { _outputdir = StringUtils.ConvertEmptyToNull(value); }
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
        #endregion Public Instance Properties

        #region Override implementation of ExternalProgramBase

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
        /// <summary>
        /// Discover the details for the specified web service.
        /// </summary>
        protected override void ExecuteTask() {
            _argumentBuilder = new StringBuilder();

            if (NoLogo) {
                _argumentBuilder.Append(" /nologo ");
            }

            if (NoSave) {
                _argumentBuilder.Append("/nosave ");
            }

            if (OutputDir != null) {
                _argumentBuilder.AppendFormat(CultureInfo.InvariantCulture, " /o:\"{0}\"", OutputDir);
            }
            if (Username != null) {
                _argumentBuilder.AppendFormat(CultureInfo.InvariantCulture, " /u:\"{0}\"", Username);
            }
            if (Password != null) {
                _argumentBuilder.AppendFormat(CultureInfo.InvariantCulture, " /p:\"{0}\"", Password);
            }
            if (Domain != null) {
                _argumentBuilder.AppendFormat(CultureInfo.InvariantCulture, " /d:{0}", Domain);
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
            
            _argumentBuilder.AppendFormat(CultureInfo.InvariantCulture, " \"{0}\"", Path);

            // call base class to do perform the actual call
            base.ExecuteTask();

        }
        #endregion Override implementation of ExternalProgramBase
    }
}
