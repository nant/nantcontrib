// NAntContrib
// Copyright (C) 2002 Scott Hernandez (ScottHernandez@hotmail.com)
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
// You should have received a copy of the GNU Lesser General Public 
// License along with this library; if not, write to the Free Software 
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// Scott Hernandez(ScottHernandez@hotmail.com)

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;
using NAnt.Core.Types;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Copies a file to a remote server using scp.
    /// </summary>
    /// <remarks>
    ///   <para>Copies a file using scp to a remote server.</para>
    ///   <para>The Username Environment variable is used.</para>
    /// </remarks>
    /// <example>
    ///   <para>Copy a single file to a remote server and path.</para>
    ///   <code>
    ///     <![CDATA[
    /// <scp file="myfile.zip" server="myServer" path="~" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("scp")]
    public class ScpTask : ExternalProgramBase {
        private string _program = "scp";
        private string _commandline;
        private DirectoryInfo _baseDirectory;
        private string _server = null;
        private string _file = null;
        private string _remotePath = "~";
        private string _programPathSep = "/";
        private string _user;

        #region Public Instance Constructor

        public ScpTask() {
            _user = Environment.GetEnvironmentVariable("USERNAME");
        }

        #endregion Public Instance Constructor

        #region Public Instance Properties

        /// <summary>
        /// The program to execute. The default is "scp".
        /// </summary>
        [TaskAttribute("program")]
        [StringValidator(AllowEmpty=false)]
        public string ProgramName {
            get { return _program; }
            set { _program = value; }
        }
                
        /// <summary>
        /// The command line arguments.
        /// </summary>
        [TaskAttribute("options")]
        public string Options {
            get { return _commandline; }
            set { _commandline = value; }
        }

        /// <summary>
        /// The file to transfer.
        /// </summary>
        [TaskAttribute("file", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public virtual string FileName {
            get { return _file; }
            set { _file = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The server to send the file to.
        /// </summary>
        [TaskAttribute("server", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public virtual string ServerName {
            get { return _server; }
            set { _server = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The path on the remote server. The default is "~".
        /// </summary>
        [TaskAttribute("path")]
        [StringValidator(AllowEmpty=false)]
        public virtual string RemotePath {
            get { return _remotePath; }
            set { _remotePath = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The username to connect as.  The default is the value of the 
        /// <c>USERNAME</c> environment variable.
        /// </summary>
        [TaskAttribute("user")]
        [StringValidator(AllowEmpty=false)]
        public virtual string UserName {
            get { return _user; }
            set { _user = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The path separator used by the program. The default is "/".
        /// </summary>
        [TaskAttribute("program-path-sep")]
        [StringValidator(AllowEmpty=false)]
        public virtual string ProgramPathSep {
            get { return _programPathSep; }
            set { _programPathSep = value; }
        }

        #endregion Public Instance Properties

        #region Override implemenation of ExternalProgramBase

        /// <summary>
        /// Gets the filename of the external program to start.
        /// </summary>
        /// <value>
        /// The filename of the external program.
        /// </value>
        public override string ProgramFileName {
            get { return ProgramName; }
        }

        /// <summary>
        /// Gets the command-line arguments for the external program.
        /// </summary>
        /// <value>
        /// The command-line arguments for the external program.
        /// </value>
        public override string ProgramArguments {
            get { return Options; }
        }
        
        /// <summary>
        /// The directory in which the command will be executed.
        /// </summary>
        [TaskAttribute("basedir")]
        public override DirectoryInfo BaseDirectory {
            get {
                if (_baseDirectory == null) {
                    return base.BaseDirectory;
                }
                return _baseDirectory;
            }
            set { _baseDirectory = value; }
        }

        protected override void ExecuteTask() {
            // scp.exe (cygwin version) requires that the source file *not* be fully qualified.
            FileInfo fileInfo = new FileInfo(Path.Combine(
                BaseDirectory.FullName, FileName));
            // use the directory in which the file is located as working directory
            BaseDirectory = fileInfo.Directory;
            // pass the file to copy
            Arguments.Add(new Argument(fileInfo.Name));
            // pass credentials and remote filename
            Arguments.Add(new Argument(string.Format(CultureInfo.InvariantCulture, 
                "{0}@{1}:{2}{3}{4}", UserName, ServerName, RemotePath, ProgramPathSep, 
                fileInfo.Name)));
            // launch the scp executable with the given arguments
            base.ExecuteTask();
        }

        #endregion Override implemenation of ExternalProgramBase
    }
}
