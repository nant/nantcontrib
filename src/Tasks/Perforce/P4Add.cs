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
using NAnt.Core;
using NAnt.Core.Util;
using NAnt.Core.Tasks;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.Perforce {
    /// <summary>
    /// Open file(s) in a client workspace for addition to the depot. Wraps the 'p4 add' command.
    /// The P4Submit command is required to submit to the perforce server.
    /// </summary>
    /// <example>
    /// <para>Add all cs files under the given directory into the "new" changelist 
    /// (will be created if it doesn't already exist)</para>
    /// <code>
    ///        <![CDATA[
    ///    <p4add file="C:\Src\Project\*.cs" changelist="new" />
    ///        ]]>
    /// </code>
    /// <para>Add the Test.txt into the default changelist</para>
    /// <code>
    ///        <![CDATA[
    ///    <p4add file="C:\Src\Project\Test.txt" />
    ///        ]]>
    /// </code>
    /// </example>
    [TaskName("p4add")]
    public class P4Add : P4Base {
        #region Private Instance Fields

        private string _file = null;
        private string _changelist = null;
        private string _type = null;

        #endregion

        #region Public Instance Fields

        /// <summary>
        /// File(s) to add. File name can contain wildcard characters. (Note: this is not using p4 wildcard syntax, but the OS wildcards) required
        /// </summary>
        [TaskAttribute("file",Required=true)]
        public string File {
            get { return _file; }
            set { _file = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Changelist that files will be added into. Changelist will be created if not already present. optional.
        /// </summary>
        [TaskAttribute("changelist",Required=false)]
        public string Changelist {
            get { return _changelist; }
            set { _changelist = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// File Type settings. Applied to all files in the File parameter. optional.
        /// </summary>
        [TaskAttribute("type",Required=false)]
        public string Type {
            get { return _type; }
            set { _type = StringUtils.ConvertEmptyToNull(value); }
        }

        #endregion

        /// <summary>
        /// This is an override used by the base class to get command specific args.
        /// </summary>
        protected override string CommandSpecificArguments {
            get { return getSpecificCommandArguments(); }
        }
        
        #region Override implementation of Task
        
        /// <summary>
        /// local method to build the command string for this particular command
        /// </summary>
        /// <returns></returns>
        protected string getSpecificCommandArguments( ) {
            StringBuilder arguments = new StringBuilder();
            arguments.Append("add ");

            if ( File == null) {
                throw new BuildException("A \"file\" attribute is required for p4add");
            }

            if ( Changelist != null ) {
                arguments.Append( string.Format("-c {0} ", Perforce.GetChangelistNumber( User, Client, Changelist, true )));
            }
            if ( Type != null ) {
                arguments.Append( string.Format("-t {0} ", Type ));
            }
            arguments.Append( File );

            return arguments.ToString();
        }
        
        #endregion Override implementation of Task
    }
}
