// NAnt - A .NET build tool
// Copyright (C) 2001-2003 Gerry Shaw
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
// Clayton Harbour (claytonharbour@sporadicism.com)

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;
using NAnt.Core.Types;
using NAnt.Core.Util;

namespace NAnt.SourceControl.Tasks {
    /// <summary>
    /// Executes the svn checkout command.
    /// </summary>
    /// <example>
    ///   <para>Checkout Gentle .Net.</para>
    ///   <code>
    ///     <![CDATA[
    /// <svn-update     destination="c:/dev/src/gentle.net" 
    ///                 uri="http://www.mertner.com/svn/repos/projects/gentle" 
    ///                 recursive="true"
    ///                 quiet="true"
    ///                 username="anonymoose"
    ///                 password="Canada" 
    ///                 revision="HEAD"
    ///                 cach-auth="false"
    ///                 config-dir="c:\home"
    ///                 />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("svn-checkout")]
    public class SvnCheckoutTask : AbstractSvnTask {

        #region Private Instance Fields
        private string COMMAND_NAME = "checkout";
        #endregion

        #region Public Instance Constructors
        /// <summary>
        /// Initialize the task, and set the default parameters.
        /// </summary>
        public SvnCheckoutTask () {
            this.Quiet = true;
        }
        #endregion
        #region Public Instance Properties
        /// <summary>
        /// The svn command to execute.  This value is defined in a constant and
        ///     is equal to the value <code>checkout</code>.
        /// </summary>
        public override string CommandName {
            get {return this.COMMAND_NAME;}
            set {this.COMMAND_NAME = value;}
        }

        /// <summary>
        /// <code>true</code> if the output should be minimized.  Defaults to 
        ///     <code>true</code>.
        /// </summary>
        [TaskAttribute("quiet", Required=false)]
        [BooleanValidator()]
        public bool Quiet {
            get {return ((Option)this.CommandOptions["quiet"]).IfDefined;}
            set {this.SetCommandOption("quiet", "quiet", value);}
        }

        /// <summary>
        /// <code>true</code> if the command should be executed recursively.
        ///     The default is <code>true</code>.
        /// </summary>
        [TaskAttribute("recursive", Required=false)]
        [BooleanValidator()]
        public bool Recursive {
            get {return ((Option)this.CommandOptions["recursive"]).IfDefined;}
            set {this.SetCommandOption("recursive", "recursive", !value);}
        }

        /// <summary>
        /// The revision to checkout.  If no revision is specified then subversion
        ///     will return the <code>HEAD</code>.
        ///     
        ///        A revision argument can be one of:
        ///        NUMBER       revision number
        ///        "{" DATE "}" revision at start of the date
        ///        "HEAD"       latest in repository
        ///        "BASE"       base rev of item's working copy
        ///        "COMMITTED"  last commit at or before BASE
        ///        "PREV"       revision just before COMMITTED
        ///   
        /// </summary>
        [TaskAttribute("revision", Required=false)]
        public string Revision {
            get {return ((Option)this.CommandOptions["revision"]).Value;}
            set {
                string number_regex = @"(\+|-)?[0-9][0-9]*(\.[0-9]*)?";
                string date_regex = @"(?<Month>\d{1,2})/(?<Day>\d{1,2})/(?<Year>(?:\d{4}|\d{2}))";
                string magic_ref_regex = @"(HEAD)|(BASE)|(COMMITTED)|(PREV)";
                
                if ((new Regex(number_regex)).IsMatch(value) ||
                    ((new Regex(date_regex)).IsMatch(value)) ||
                    ((new Regex(magic_ref_regex)).IsMatch(value))) {
                    this.SetCommandOption("revision", String.Format("revision={0}", value), true);
                } else {
                    throw new BuildException (
                        String.Format("Invalid argument specified: {0}.", value));
                }
            }
        }

		#endregion

	}
}
