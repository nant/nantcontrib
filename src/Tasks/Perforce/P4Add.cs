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
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks.Perforce {
    /// <summary>
    /// Open file(s) in a client workspace for addition to the depot.
    /// </summary>
    /// <example>
    ///   <para>
    ///   Add all cs files under the given directory into the "new" changelist 
    ///   (will be created if it doesn't already exist).
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <p4add file="C:\Src\Project\*.cs" changelist="new" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>Add Test.txt into the default changelist.</para>
    ///   <code>
    ///     <![CDATA[
    /// <p4add file="C:\Src\Project\Test.txt" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("p4add")]
    public class P4Add : P4Base {
        #region Private Instance Fields

        private string _file;
        private string _changelist;
        private string _type;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// File(s) to add. File name can contain wildcard characters. (Note: 
        /// this is not using p4 wildcard syntax, but the OS wildcards).
        /// </summary>
        [TaskAttribute("file", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string File {
            get { return _file; }
            set { _file = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Changelist that files will be added into. Changelist will be created 
        /// if not already present.
        /// </summary>
        [TaskAttribute("changelist", Required=false)]
        public string Changelist {
            get { return _changelist; }
            set { _changelist = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// File Type settings. Applied to all files in the <see cref="File" /> 
        /// parameter.
        /// </summary>
        [TaskAttribute("type", Required=false)]
        public string Type {
            get { return _type; }
            set { _type = StringUtils.ConvertEmptyToNull(value); }
        }

        #endregion Public Instance Properties

        #region Override implementation of P4Base

        /// <summary>
        /// This is an override used by the base class to get command specific args.
        /// </summary>
        protected override string CommandSpecificArguments {
            get { return getSpecificCommandArguments(); }
        }

        #endregion Override implementation of P4Base
        
        #region Protected Instance Methods
        
        /// <summary>
        /// Build the command string for this particular command.
        /// </summary>
        /// <returns>
        /// The command string for this particular command.
        /// </returns>
        protected string getSpecificCommandArguments( ) {
            StringBuilder arguments = new StringBuilder();
            arguments.Append("add ");

            if (Changelist != null) {
                arguments.Append(string.Format("-c {0} ", Perforce.GetChangelistNumber(User, Client, Changelist, true)));
            }
            if (Type != null) {
                arguments.Append(string.Format("-t {0} ", Type));
            }
            arguments.Append(File);

            return arguments.ToString();
        }
        
        #endregion Protected Instance Methods
    }
}
