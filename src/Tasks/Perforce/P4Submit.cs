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
    /// Send changes made to open files to the depot.
    /// </summary>
    /// <example>
    ///   <para>
    ///   Submit changelist "Temp", but first revert all unchanged files in the 
    ///   changelist.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <p4submit changelist="Temp" revertunchanged="true" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>Submit changelist, but leave the files open afterwards.</para>
    ///   <code>
    ///     <![CDATA[
    /// <p4submit changelist="Temp" remainopen="true" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("p4submit")]
    public class P4Submit : P4Base {
        #region Private Instance Fields

        private string _changelist;
        private bool _remainopen;
        private bool _revertunchanged;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// Changelist to submit.
        /// </summary>
        [TaskAttribute("changelist", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Changelist {
            get { return _changelist; }
            set { _changelist = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Keep the files open after submitting. The default is 
        /// <see langword="false" />.
        /// </summary>
        [TaskAttribute("remainopen", Required=false)]
        [BooleanValidator()]
        public bool RemainOpen {
            get { return _remainopen; }
            set { _remainopen = value; }
        }

        /// <summary>
        /// Revert all unchanged or missing files from the changelist.
        /// The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("revertunchanged", Required=false)]
        [BooleanValidator()]
        public bool RevertUnchanged {
            get { return _revertunchanged; }
            set { _revertunchanged = value; }
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
            arguments.Append("submit ");

            string changelistnumber = Perforce.GetChangelistNumber(User, Client, Changelist);
            if (changelistnumber == null && View == null) {
                throw new BuildException("If the \"changelist\" does not currently"
                    + " exist, a \"view\" is required.", Location);
            } 

            if (changelistnumber == null) {
                changelistnumber = Perforce.GetChangelistNumber(User, Client, 
                    Changelist, true);
            }
            if (View != null) {
                // reopen view into changelist
                Perforce.getProcessOutput("p4", string.Format("-u {0} -c {1} reopen -c {2} {3}", 
                    User, Client, changelistnumber, View), null);
            }
            if (RevertUnchanged) {
                // revert
                Perforce.getProcessOutput("p4", string.Format("-u {0} -c {1} revert -a -c {2}", 
                    User, Client, changelistnumber), null);
            }

            if (RemainOpen) {
                arguments.Append("-r ");
            }
            arguments.Append(string.Format("-c {0} ", changelistnumber));

            return arguments.ToString();
        }

        #endregion Protected Instance Methods
    }
}
