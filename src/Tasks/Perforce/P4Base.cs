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

using System;
using System.Text;
using System.Text.RegularExpressions;
using NAnt.Core;
using NAnt.Core.Tasks;
using NAnt.Core.Util;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.Perforce {
    /// <summary>
    /// Base class for Perforce (P4) NAnt tasks. See individual task for example usage.
    /// </summary>
    /// <seealso cref="P4Sync">P4Sync</seealso>    
    public abstract class P4Base: ExternalProgramBase {
        #region Private Instance Fields
        
        string _arguments = null;
        private string _perforcePort = null;
        private string _perforceClient = null;
        private string _perforceUser = null;
        private string _perforceView = null;
        private bool _scriptOutput = false;

        #endregion Private Instance Fields

        #region Public Instance Properties
        
        /// <summary>
        /// The p4d server and port to connect to; optional, default "perforce:1666".
        /// </summary>
        [TaskAttribute("port",Required=false)]
        public string Port {
            get { return _perforcePort; }
            set {_perforcePort = StringUtils.ConvertEmptyToNull(value); }

        }

        /// <summary>
        /// The p4 client spec to use; optional, defaults to the current user.
        /// </summary>
        [TaskAttribute("client",Required=false)]
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
        /// The p4 username; optional, defaults to the current user.
        /// </summary>
        [TaskAttribute("user",Required=false)]
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
        /// The client, branch or label view to operate upon; optional default "//...".
        /// </summary>
        [TaskAttribute("view",Required=false)]
        virtual public string View {
            get { return _perforceView; }
            set { _perforceView = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Prepends a descriptive field (for example, text:, info:, error:, exit:) to each 
        /// line of output produced by a Perforce command. 
        /// This is most often used when scripting.
        /// optional default false
        /// </summary>
        [TaskAttribute("script",Required=false)]
        [BooleanValidator()]
        virtual public bool Script {
            get { return _scriptOutput; }
            set { _scriptOutput = value; }
        }

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
            
        #endregion Public Instance Properties
        
        /// <summary>
        /// / Derived classes to override this
        /// </summary>
        protected abstract string CommandSpecificArguments {
            get;
        }
        
        #region Override implementation of Task
        
        /// <summary>
        /// Execute the perforce command assembled by subclasses.
        /// </summary>
        protected override void ExecuteTask() {
            StringBuilder arguments = new StringBuilder();

            //perforce global options
            if (Port != null) {
                arguments.Append(string.Format(" -p {0}", Port ) );
            }
            if (User != null ) {
                arguments.Append(string.Format(" -u {0}", User ) );
            }
            if (Client != null ) {
                arguments.Append(string.Format(" -c {0}", Client ) );
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
        #endregion Override implementation of Task
    }
}
