// NAnt - A .NET build tool
// Copyright (C) 2001-2002 Gerry Shaw
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
// Ian MacLean ( ian_maclean@another.com )
// Jeff Hemry ( jdhemry@qwest.net )

using System;
using System.Text;
using System.Text.RegularExpressions;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks.Perforce {
    /// <summary>
    /// Base class for Perforce (P4) NAnt tasks. See individual task for example usage.
    /// <seealso cref="P4Add">P4Add</seealso>
    /// <seealso cref="P4Change">P4Change</seealso>
    /// <seealso cref="P4Delete">P4Delete</seealso>
    /// <seealso cref="P4Edit">P4Edit</seealso>
    /// <seealso cref="P4Label">P4Label</seealso>
    /// <seealso cref="P4Labelsync">P4Labelsync</seealso>
    /// <seealso cref="P4Print">P4Print</seealso>
    /// <seealso cref="P4Reopen">P4Reopen</seealso>
    /// <seealso cref="P4Revert">P4Revert</seealso>
    /// <seealso cref="P4Submit">P4Submit</seealso>
    /// <seealso cref="P4Sync">P4Sync</seealso>
    /// </summary>
    public abstract class P4Base: ExternalProgramBase {
        #region Private Instance Fields
        
        private string _arguments;
        private string _perforcePort;
        private string _perforceClient;
        private string _perforceUser;
        private string _perforceView;
        private bool _scriptOutput;

        #endregion Private Instance Fields

        #region Public Instance Properties
        
        /// <summary>
        /// The p4 server and port to connect to. The default is "perforce:1666".
        /// </summary>
        [TaskAttribute("port", Required=false)]
        public string Port {
            get { return _perforcePort; }
            set {_perforcePort = StringUtils.ConvertEmptyToNull(value); }

        }

        /// <summary>
        /// The p4 client spec to use. The default is the current client.
        /// </summary>
        [TaskAttribute("client", Required=false)]
        public string Client {
            get { 
                if (_perforceClient == null) {
                    _perforceClient = Perforce.GetClient();
                }
                return _perforceClient; 
            }
            set { _perforceClient = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The p4 username. The default is the current user.
        /// </summary>
        [TaskAttribute("user", Required=false)]
        public string User {
            get { 
                if (_perforceUser == null){
                    _perforceUser = Perforce.GetUserName();
                }
                return _perforceUser; 
            }
            set { _perforceUser = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The client, branch or label view to operate upon. The default is
        /// "//...".
        /// </summary>
        [TaskAttribute("view", Required=false)]
        public virtual string View {
            get { return _perforceView; }
            set { _perforceView = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Prepends a descriptive field (for example, text:, info:, error:, exit:) 
        /// to each line of output produced by a Perforce command. This is most 
        /// often used when scripting. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("script", Required=false)]
        [BooleanValidator()]
        public bool Script {
            get { return _scriptOutput; }
            set { _scriptOutput = value; }
        }

        #endregion Public Instance Properties
        
        #region Override implementation of ExternalProgramBase

        /// <summary>
        /// Gets the command line arguments for the external program.
        /// </summary>
        /// <value>
        /// The command line arguments for the external program.
        /// </value>
        public override string ProgramArguments {
            get { return _arguments; }
        }

        /// <summary>
        /// Override the ExeName paramater for p4.exe
        /// </summary>
        public override string ExeName {
            get { return "p4"; }
        }
            
        /// <summary>
        /// Execute the perforce command assembled by subclasses.
        /// </summary>
        protected override void ExecuteTask() {
            StringBuilder arguments = new StringBuilder();

            //perforce global options
            if (Port != null) {
                arguments.Append(string.Format(" -p {0}", Port));
            }
            if (User != null) {
                arguments.Append(string.Format(" -u {0}", User));
            }
            if (Client != null) {
                arguments.Append(string.Format(" -c {0}", Client));
            }
            if (Script) {
                arguments.Append(" -s");
            }

            // Get the command specific arguments from the derived class
            arguments.Append(" ");
            arguments.Append(CommandSpecificArguments);

            _arguments = arguments.ToString();

            // call base class to do perform the actual call
            Log(Level.Verbose, ExeName + _arguments.ToString());

            base.ExecuteTask();
        } 

        #endregion Override implementation of ExternalProgramBase

        #region Protected Instance Methods

        /// <summary>
        /// Derived classes should override this to provide command-specific
        /// commandline arguments.
        /// </summary>
        protected abstract string CommandSpecificArguments {
            get;
        }

        #endregion Protected Instance Methods
    }
}
