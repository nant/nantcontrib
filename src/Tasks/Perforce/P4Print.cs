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
using NAnt.Core.Util;
using NAnt.Core.Tasks;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.Perforce {
    
    /// <summary>
    /// Fetch a specific file from a Perforce depot without needing a clientspec to map it. Wraps the 'p4 print' command.
    /// </summary>
    ///<example>
    /// <para>
    ///<code>
    ///     <![CDATA[
    ///<p4print file="//depot/foo/mainline/clientspec" outputfile=".\clientspec" />
    ///<p4client input=".\clientspec" />
    ///     ]]>
    ///   </code>
    /// </para>
    /// </example>
    /// <todo> fileset? </todo>
    /// <author> <a href="mailto:nant@bobs.org">Bob Arnson</a></author>
    [TaskName("p4print")]
    public class P4Print : P4Base {
        
        #region Private Instance Fields
        
        private string _file = null;
        private string _outputFile = null;
        
        #endregion Private Instance Fields
        
        #region Public Instance Properties
        
        /// <summary> 
        /// The depot or local filename (including optional path) of the file to fetch; required
        /// </summary>
        [TaskAttribute("file", Required = true)]
        public string File {
            get { return _file; }
            set { _file = StringUtils.ConvertEmptyToNull(value); }
        }       
        
        /// <summary> 
        /// The local filename to write the fetched file to; required
        /// </summary>
        [TaskAttribute("outputfile", Required = true)]
        public string P4OutputFile {
            get { return _outputFile; }
            set { _outputFile = StringUtils.ConvertEmptyToNull(value); }
        }       
        
        #endregion Public Instance Properties
        
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
            arguments.Append("-s print -q ");
            arguments.Append(string.Format("-o {0} ", P4OutputFile));
            arguments.Append(File);
            return arguments.ToString();
        }
        
        #endregion Override implementation of Task
    }
}
