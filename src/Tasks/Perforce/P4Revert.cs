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
    /// Discard changes made to open files.
    /// </summary>
    /// <example>
    ///   <para>Revert all txt files in a given changelist.</para>
    ///   <code>
    ///     <![CDATA[
    /// <p4revert changelist="Test" view="//...*.txt" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Revert all unchanged files opened in the given changelist.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <p4revert changelist="Test" revertunchanged="true" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>Revert all unchanged files opened in any changelist.</para>
    ///   <code>
    ///     <![CDATA[
    /// <p4revert revertunchanged="true" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("p4revert")]
    public class P4Revert : P4Base {
        #region Private Instance Fields

        private string _changelist;
        private bool _revertunchanged;

        #endregion Private Instance Fields

        #region Private Static Fields

        private static string Usage = @"At least one of the following parameters are required when using p4revert;
            changelist       : Changelist name to revert
            view             : View pattern to revert (can be used with changelist)
            revertunchanged  : Revert unchanged files (all or just on specified changelist or view)";

        #endregion Private Static Fields

        #region Public Instance Properties

        /// <summary>
        /// Changelist to perform the revert action on. optional.
        /// </summary>
        [TaskAttribute("changelist", Required=false)]
        public string Changelist {
            get { return _changelist; }
            set { _changelist = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Revert all unchanged or missing files from the changelist. default is false. optional.
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
            arguments.Append("revert ");

            if (!RevertUnchanged && Changelist == null && View == null) {
                throw new BuildException(Usage);
            }

            if (RevertUnchanged) {
                arguments.Append("-a ");
            }
            if (Changelist != null) {
                arguments.Append(string.Format("-c {0} ", Perforce.GetChangelistNumber(User, Client, Changelist)));
            }
            if (View != null) {
                arguments.Append(View);
            }

            return arguments.ToString();
        }

        #endregion Protected Instance Methods
    }
}
