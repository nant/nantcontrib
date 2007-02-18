//
// NAntContrib
// Copyright (C) 2002 Tomas Restrepo (tomasr@mvps.org)
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

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Mail;
using System.Xml;
using System.Xml.Xsl;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;

namespace NAnt.Contrib.Tasks { 
    /// <summary>
    /// A task that generates strongly typed WMI classes using 
    /// <c>mgmtclassgen.exe</c>.
    /// </summary>
    /// <remarks>
    /// The Management Strongly Typed Class Generator 
    /// enables you to quickly generate an early-bound 
    /// managed class for a specified Windows Management 
    /// Instrumentation (WMI) class. The generated 
    /// class simplifies the code you must write to access 
    /// an instance of the WMI class.
    /// </remarks>
    /// <example>
    ///   <code>
    ///     <![CDATA[
    /// <mgmtclassgen 
    ///     wmiclass="Win32_LogicalDisk" 
    ///     language="CS"
    ///     machine="SomeMachine"
    ///     path="Root\cimv2"
    ///     namespace="Winterdom.WMI"
    ///     out="${outputdir}\LogicalDisk.cs"
    ///     username="Administrator"
    ///     password="password"
    /// />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("mgmtclassgen")]
    public class MgmtClassGenTask : ExternalProgramBase {
        const string PROG_FILE_NAME = "mgmtclassgen.exe";

        private string _args;
        private string _wmiClass;
        private string _language;
        private string _machine;
        private string _path;
        private string _namespace;
        private string _outfile;
        private string _username;
        private string _password;

        /// <summary>
        /// Specifies the name of the WMI class
        /// to generate the strongly typed class
        /// </summary>
        [TaskAttribute("wmiclass", Required=true)]
        public string WmiClass {
            get { return _wmiClass; }
            set { _wmiClass = value; }
        }

        /// <summary>
        /// Specifies the language in which to generate
        /// the class. Possible values are: CS, VB, JS
        /// </summary>
        [TaskAttribute("language")]
        public string Language {
            get { return _language; }
            set { _language = value; }
        }

        /// <summary>
        /// Specifies the machine to connect to.
        /// </summary>
        [TaskAttribute("machine")]
        public string Machine {
            get { return _machine; }
            set { _machine = value; }
        }

        /// <summary>
        /// Specifies the path to the WMI namespace
        /// that contains the class.
        /// </summary>
        [TaskAttribute("path")]
        public string Path {
            get { return _path; }
            set { _path = value; }
        }


        /// <summary>
        /// Namespace of the generated .NET class
        /// </summary>
        [TaskAttribute("namespace")]
        public string Namespace {
            get { return _namespace; }
            set { _namespace = value; }
        }

        /// <summary>
        /// Path of the file to generate
        /// </summary>
        [TaskAttribute("out")]
        public string OutFile {
            get { return _outfile; }
            set { _outfile = value; }
        }

        /// <summary>
        /// User name to use when connecting to
        /// the specified machine
        /// </summary>
        [TaskAttribute("username")]
        public string Username {
            get { return _username; }
            set { _username = value; }
        }


        /// <summary>
        /// Password to use when connecting to the 
        /// specified machine
        /// </summary>
        [TaskAttribute("password")]
        public string Password {
            get { return _password; }
            set { _password = value; }
        }

        /// <summary>
        /// Filename of program to execute
        /// </summary>
        public override string ProgramFileName {
            get { return PROG_FILE_NAME; }
        }
        /// <summary>
        /// Arguments of program to execute
        /// </summary>
        public override string ProgramArguments {
            get { return _args; }
        }

        /// <summary>
        /// Initializes task and ensures the supplied attributes are valid.
        /// </summary>
        protected override void Initialize() {
            if (WmiClass == null) {
                throw new BuildException("MgmtClassGen attribute \"wmiclass\" is required.", Location);
            }
        }

        /// <summary>
        /// This is where the work is done
        /// </summary>
        protected override void ExecuteTask() {
            //
            // build command line
            //
            _args = WmiClass;
            if ( Language != null )
                _args += " /l " + Language;
            if ( Machine != null )
                _args += " /m " + Machine;
            if ( Path != null )
                _args += " /n " + Path;
            if ( Namespace != null )
                _args += " /o " + Namespace;
            if ( OutFile != null )
                _args += " /p " + OutFile;
            if ( Username != null )
                _args += " /u " + Username;
            if ( Password != null )
                _args += " /pw " + Password;

            base.ExecuteTask();
        }
    }
}
