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

namespace NAnt.Contrib.Tasks
{
	/// <summary>Discovers the URLs of XML web services on a web server and saves documents 
	/// related to them to the local disk. The resulting .discomap, .wsdl, and .xsd files 
	/// can be used with the <see cref="WsdlTask"/> to produce web service clients and 
	/// and abstract web service servers using ASP.NET.</summary>
	/// <example>
    ///   <para>Generate a proxy class for a web service.</para>
    ///   <code><![CDATA[<disco path="http://www.somewhere.com/myservice.wsdl" 
    ///     language="CS" namespace="MyCompany.MyService" outfile="MyService.cs" />]]></code>
	/// </example>
	[TaskName("disco")]
	public class DicsoTask : ExternalProgramBase
	{
		private string _args;
        private string _path;
		private bool _nologo;
        private bool _nosave;
        private string _outputdir;
        private string _username;
        private string _password;
        private string _domain;
        private string _proxy;
        private string _proxyusername;
        private string _proxypassword;
        private string _proxydomain;

        /// <summary>The URL or Path to discover.</summary>
        [TaskAttribute("path")]
        public string Path 
        {
            get { return _path; }
            set { _path = value; } 
        }     

        /// <summary>Suppresses the banner.</summary>
        [TaskAttribute("nologo")]        
        [BooleanValidator()]
        public bool NoLogo
        {
            get { return _nologo; }
            set { _nologo = value; }
        }

        /// <summary>Do not save the discovered documents to the local disk.</summary>
        [TaskAttribute("nosave")]        
        [BooleanValidator()]
        public bool NoSave
        {
            get { return _nosave; }
            set { _nosave = value; }
        }

        /// <summary>The output directory to save discovered documents in.</summary>
        [TaskAttribute("outputdir")]        
        public string OutputDir
        {
            get { return _outputdir; }
            set { _outputdir = value; }
        }

        /// <summary>Username of an account with credentials to access a 
        /// server that requires authentication.</summary>
        [TaskAttribute("username")]        
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        /// <summary>Password of an account with credentials to access a 
        /// server that requires authentication.</summary>
        [TaskAttribute("password")]        
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        /// <summary>Domain of an account with credentials to access a 
        /// server that requires authentication.</summary>
        [TaskAttribute("domain")]        
        public string Domain
        {
            get { return _domain; }
            set { _domain = value; }
        }

        /// <summary>URL of a proxy server to use for HTTP requests. 
        /// The default is to use the system proxy setting.</summary>
        [TaskAttribute("proxy")]        
        public string Proxy
        {
            get { return _proxy; }
            set { _proxy = value; }
        }

        /// <summary>Username of an account with credentials to access a 
        /// proxy that requires authentication.</summary>
        [TaskAttribute("proxyusername")]        
        public string ProxyUsername
        {
            get { return _proxyusername; }
            set { _proxyusername = value; }
        }

        /// <summary>Password of an account with credentials to access a 
        /// proxy that requires authentication.</summary>
        [TaskAttribute("proxypassword")]        
        public string ProxyPassword
        {
            get { return _proxypassword; }
            set { _proxypassword = value; }
        }

        /// <summary>Domain of an account with credentials to access a 
        /// proxy that requires authentication.</summary>
        [TaskAttribute("proxydomain")]        
        public string ProxyDomain
        {
            get { return _proxydomain; }
            set { _proxydomain = value; }
        }
        
        public override string ProgramFileName
        {
            get
            {
                return "disco.exe";
            }
        }

        /// <summary>
        /// Arguments of program to execute
        /// </summary>
        public override string ProgramArguments 
        {
            get
            {
                return _args;
            }
        }

        ///<summary>
        ///Initializes task and ensures the supplied attributes are valid.
        ///</summary>
        ///<param name="taskNode">Xml node used to define this task instance.</param>
        protected override void InitializeTask(System.Xml.XmlNode taskNode) 
        {
        }

        protected override void ExecuteTask()
        {
            StringBuilder arguments = new StringBuilder();

            if (NoLogo)
            {
                arguments.Append("/nologo ");
            }

            if (NoSave)
            {
                arguments.Append("/nosave ");
            }

            if (OutputDir != null)
            {
                arguments.Append(" /o:");
                arguments.Append(OutputDir);
            }
            if (Username != null)
            {
                arguments.Append(" /u:");
                arguments.Append(Username);
            }
            if (Password != null)
            {
                arguments.Append(" /p:");
                arguments.Append(Password);
            }
            if (Domain != null)
            {
                arguments.Append(" /d:");
                arguments.Append(Domain);
            }
            if (Proxy != null)
            {
                arguments.Append(" /proxy:");
                arguments.Append(Proxy);
            }
            if (ProxyUsername != null)
            {
                arguments.Append(" /pu:");
                arguments.Append(ProxyUsername);
            }
            if (ProxyPassword != null)
            {
                arguments.Append(" /pp:");
                arguments.Append(ProxyPassword);
            }
            if (ProxyDomain != null)
            {
                arguments.Append(" /pd:");
                arguments.Append(ProxyDomain);
            }

            arguments.Append(Path);

            try
            {
                _args = arguments.ToString();

                base.ExecuteTask();
            }
            catch (Exception e)
            {
                throw new BuildException(LogPrefix + "ERROR: " + e);
            }
        }
    }
}
