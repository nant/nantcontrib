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
//
// Bob Arnson (nant@bobs.org)

using System;
using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks.Perforce {
    /// <summary>
    /// Fetch a specific file from a Perforce depot without needing a clientspec 
    /// to map it.
    /// </summary>
    /// <example>
    ///   <code>
    ///     <![CDATA[
    /// <p4print file="//depot/foo/mainline/clientspec" outputfile=".\clientspec" />
    /// <p4client input=".\clientspec" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("p4print")]
    public class P4Print : P4Base {
        #region Private Instance Fields

        private string _file;
        private string _outputFile;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary> 
        /// The depot or local filename (including optional path) of the file 
        /// to fetch.
        /// </summary>
        [TaskAttribute("file", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string File {
            get { return _file; }
            set { _file = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary> 
        /// The local filename to write the fetched file to.
        /// </summary>
        [TaskAttribute("outputfile", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string P4OutputFile {
            get { return _outputFile; }
            set { _outputFile = StringUtils.ConvertEmptyToNull(value); }
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
        /// Builds the command string for this particular command.
        /// </summary>
        /// <returns>
        /// The command string for this particular command.
        /// </returns>
        protected string getSpecificCommandArguments( ) {
            StringBuilder arguments = new StringBuilder();
            arguments.Append("-s print -q ");
            arguments.Append(string.Format("-o {0} ", P4OutputFile));
            arguments.Append(File);
            return arguments.ToString();
        }

        #endregion Protected Instance Methods
    }
}
