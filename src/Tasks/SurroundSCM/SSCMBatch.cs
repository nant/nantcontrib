//
// NAntContrib
// Copyright (C) 2001-2004
//
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
// Matt Harp; Seapine Software, Inc.
//

using System.IO;
using System.Text;

using NAnt.Core.Attributes;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks.SurroundSCM {
    /// <summary>
    /// Processes <see href="http://www.seapine.com/surroundscm.html">Surround SCM</see> batch files.
    /// </summary>
    /// <remarks>
    /// Processes the batch commands found in the input file. Each line in the 
    /// input file should contain a single Surround SCM command including proper 
    /// command line options. The sscm command, Surround SCM server address, 
    /// port number, username and password are not required for each command line.
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Run the batch file <c>${src}/sscm.batch</c> on the server at localhost,
    ///   port 4900 with username 'administrator' and a blank password. All script
    ///   output is directed to the console.
    ///   </para>
    ///   <code>
    /// &lt;sscmbatch
    ///     serverconnect=&quot;localhost:4900&quot;
    ///     serverlogin=&quot;administrator:&quot;
    ///     input=&quot;${src}/sscm.batch&quot;
    /// /&gt;
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Run the batch file <c>${src}/sscm.batch</c> on the server at localhost, 
    ///   port 4900 with username 'administrator' and a blank password. All script 
    ///   output is redirected to <c>${dist}/sscm.batch.out</c>.
    ///   </para>
    ///   <code>
    /// &lt;sscmbatch
    ///     serverconnect=&quot;localhost:4900&quot;
    ///     serverconnect=&quot;administrator:&quot;
    ///     input=&quot;${src}/sscm.batch&quot;
    ///     output=&quot;${dist}/sscm.batch.out&quot;
    /// /&gt;
    ///   </code>
    /// </example>
    [TaskName("sscmbatch")]
    public class SSCMBatch : SSCMTask {
        #region Private Instance Fields

        private FileInfo _inputFile;
        private FileInfo _outputFile;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// File to read commands from.
        /// </summary>
        [TaskAttribute("input", Required=true)]
        public FileInfo InputFile {
            get { return _inputFile; }
            set { _inputFile = value; }
        }

        /// <summary>
        /// File to direct all standard output to. When executing commands from 
        /// the input file, all output is written to this file instead of being 
        /// displayed on the screen.
        /// </summary>
        [TaskAttribute("output", Required=false)]
        public FileInfo OutputFile {
            get { return _outputFile; }
            set { _outputFile = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of SSCMTask

        /// <summary>
        /// Writes the task-specific arguments to the specified 
        /// <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="argBuilder">The <see cref="StringBuilder" /> to write the task-specific arguments to.</param>
        protected override void WriteCommandLineArguments(StringBuilder argBuilder) {
            argBuilder.Append("batch");

            // input file
            if (InputFile != null) {
                argBuilder.Append(" \"");
                argBuilder.Append(InputFile.FullName);
                argBuilder.Append("\"");
            } else {
                argBuilder.Append(" /");
            }

            // output file
            if (OutputFile != null) {
                argBuilder.Append(" -o\"");
                argBuilder.Append(OutputFile.FullName);
                argBuilder.Append("\"");
            }
        }

        #endregion Override implementation of SSCMTask
    }
}
