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

using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks.SurroundSCM {
    /// <summary>
    /// <see href="http://www.seapine.com/surroundscm.html">Surround SCM</see>
    /// abstract task base.
    /// </summary>
    public abstract class SSCMTask : ExternalProgramBase {
        #region Private Instance Fields

        private string _serverLogin;
        private string _serverConnect;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The address and port number of the Surround SCM server host computer. 
        /// Format is server:port. If not entered, the last saved connection 
        /// parameters are used.
        /// </summary>
        [TaskAttribute("serverconnect", Required=false)]
        public string ServerConnect {
            get { return _serverConnect; }
            set { _serverConnect = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The username and password used to login to the Surround SCM server. 
        /// Format is username:password. If not entered, the last saved login 
        /// parameters are used.
        /// </summary>
        [TaskAttribute("serverlogin", Required=false)]
        public string ServerLogin { 
            get { return _serverLogin; }
            set { _serverLogin = StringUtils.ConvertEmptyToNull(value); }
        }

        #endregion Public Instance Properties

        #region Override implementation of ExternalProgramBase

        /// <summary>
        /// Override ExeName paramater to sscm.exe for Surround SCM.
        /// </summary>
        public override string ExeName {
            get { return "sscm"; }
        }

        #region Override implementation of ExternalProgramBase

        /// <summary>
        /// Gets the command line arguments for the external program.
        /// </summary>
        /// <value>
        /// The command line arguments for the external program.
        /// </value>
        public override string ProgramArguments {
            get { return GetCommandlineArguments(); }
        }

        #endregion Override implementation of ExternalProgramBase

        #region Protected Instance Methods

        /// <summary>
        /// Writes the task-specific arguments to the specified 
        /// <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="argBuilder">The <see cref="StringBuilder" /> to write the task-specific arguments to.</param>
        protected abstract void WriteCommandLineArguments(StringBuilder argBuilder);

        #endregion Protected Instance Methods

        #endregion Protected Instance Methods

        #region Private Instance Methods

        private string GetCommandlineArguments() {
            StringBuilder argBuilder = new StringBuilder();

            // write task-specific arguments to StringBuilder
            WriteCommandLineArguments(argBuilder);

            // server connect
            if (ServerConnect != null) {
                argBuilder.Append(" -z\"");
                argBuilder.Append(ServerConnect);
                argBuilder.Append("\"");
            }

            // server login
            if (ServerLogin != null) {
                argBuilder.Append(" -y\"");
                argBuilder.Append(ServerLogin);
                argBuilder.Append("\"");
            }

            return argBuilder.ToString();
        }

        #endregion Private Instance Methods
    }
}
