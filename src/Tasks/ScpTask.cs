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
using System.IO;
using System.Diagnostics;

using SourceForge.NAnt;
using SourceForge.NAnt.Attributes;
using SourceForge.NAnt.Tasks;

namespace NAnt.Contrib.Tasks {
    /// <summary>Copies a file using scp to a remote server.</summary>
    /// <remarks>
    ///   <para>Copies a file using scp to a remote server.</para>
    ///   <para>The Username Environment variable is used.</para>
    /// </remarks>
    /// <example>
    ///   <para>Copy a single file to a remote server and path.</para>
    ///   <code>
    /// <![CDATA[
    ///   <scp file="myfile.zip" server="myServer" path="~" />
    /// ]]>
    ///   <para>This basically turns into "scp myfile.zip user@mySerer:~/myfile.zip".</para>
    ///   </code>
    /// </example>
    [TaskName("scp")]
    public class ScpTask : ExternalProgramBase {
        protected string _program = "scp";
        protected string _commandline = null;
        protected string _baseDirectory = null;
        protected int _timeout = Int32.MaxValue;
        protected string _server = null;
        protected string _file = null;
        protected string _path = "~";
        protected string _programPathSep = "/";
        //TODO: put this assignment in constructor, possible exception case.
        protected string _user = Environment.GetEnvironmentVariable("USERNAME");

        /// <summary>The program to execute, default is "scp".</summary>
        [TaskAttribute("program")]
        public string ProgramName  { set { _program = value; } }                
                
        /// <summary>The command line arguments.</summary>
        [TaskAttribute("options")]
        public string Arguments { set { _commandline = value; } }

        public override string ProgramFileName  { get { return _program; } }                
        public override string ProgramArguments { get { return _commandline; } }
        
        /// <summary>The directory in which the command will be executed.</summary>
        [TaskAttribute("basedir")]
        public override string BaseDirectory    { get { return Project.GetFullPath(_baseDirectory); } set { _baseDirectory = value; } }
                
        /// <summary>Stop the build if the command does not finish within the specified time.  Specified in milliseconds.  Default is no time out.</summary>
        [TaskAttribute("timeout")]
        [Int32Validator()]
        public override int TimeOut { get { return _timeout; } set { _timeout = value; }  }

        /// <summary> The file to transfer</summary>
        [TaskAttribute("file", Required=true)]
        public virtual string FileName { set { _file = value;} }

        /// <summary> The server to send the file to.</summary>
        [TaskAttribute("server", Required=true)]
        public virtual string ServerName { set { _server = value;} }

        /// <summary> The path on the remote server.
        /// <para>Defaults to "~".</para>
        /// </summary>
        [TaskAttribute("path")]
        public virtual string RemotePath { set { _server = value;} }

        /// <summary> The username to connect as.
        /// <para>Defaults to USERNAME environment var.</para>
        /// </summary>
        [TaskAttribute("user")]
        public virtual string Username{ set { _user = value;} }

        /// <summary> The Path Seperator used by the program.
        /// <para>Defaults to "/"</para>
        /// </summary>
        [TaskAttribute("program-path-sep")]
        public virtual string ProgramPathSep{ set { _programPathSep = value;} }

        protected override void PrepareProcess(ref System.Diagnostics.Process process) {
            
            //process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardError = false;
            base.PrepareProcess(ref process);
        }
        protected override void ExecuteTask() {
            Args.Add(string.Format("{3} {0}@{1}:{2}/{3}", _user, _server, _path, _file));
            Log.WriteLineIf(Project.Verbose, LogPrefix + "{0} {1}", ProgramFileName, GetCommandLine());
            
            Process process = StartProcess();
            //get inputs and outputs
            //StreamWriter stdIn = process.StandardInput;
            //StreamReader stdInAsOut = new StreamReader(process.StandardInput.BaseStream);
            //StreamReader stdErr = process.StandardError;
            StreamReader stdOut = process.StandardOutput;

            
            //stdIn.WriteLine("yes");

            // display standard output
            string output = stdOut.ReadToEnd();
            if (output.Length > 0) {
                int indentLevel = Log.IndentLevel;
                Log.IndentLevel = 0;
                Log.WriteLine(output);
                Log.IndentLevel = indentLevel;
            }
            
            /*
            // display standard Error
            output = stdErr.ReadToEnd();
            if (output.Length > 0) {
                int indentLevel = Log.IndentLevel;
                Log.IndentLevel = 0;
                Log.WriteLine(output);
                Log.IndentLevel = indentLevel;
            }
            */

            // wait for program to exit
            process.WaitForExit(TimeOut);

            // Keep the FailOnError check to prevent programs that return non-zero even if they are not returning errors.
            if (FailOnError && process!= null && process.ExitCode != 0) {
                throw new BuildException("External program returned errors, see build log for details.", Location);
            }
        }
    }
}
