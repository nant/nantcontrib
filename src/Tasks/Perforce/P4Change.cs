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
    /// Create or delete a changelist specification.
    /// </summary>
    /// <example>
    ///   <para>Create a new changelist called "mynewchange".</para>
    ///   <code>
    ///     <![CDATA[
    /// <p4change changelist="mynewchange" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>Delete the changelist called "mynewchange".</para>
    ///   <code>
    ///     <![CDATA[
    /// <p4change changelist="mynewchange" delete="true" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("p4change")]
    public class P4Change : P4Base {
        #region Private Instance Fields

        private string _changelist;
        private bool _delete;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// Changelist to create or delete.
        /// </summary>
        [TaskAttribute("changelist", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Changelist {
            get { return _changelist; }
            set { _changelist = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// If <see langword="false" /> causes passed in changelist to be 
        /// deleted. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("delete", Required=false)]
        [BooleanValidator()]
        public bool Delete {
            get { return _delete; }
            set { _delete = value; }
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
            arguments.Append("change ");

            if ( Changelist == null) {
                throw new BuildException("A \"changelist\" is required for p4edit");
            }

            string _changelistnumber = null;
            if ( Delete ) {
                arguments.Append( "-d ");
                _changelistnumber = Perforce.GetChangelistNumber( User, Client, Changelist );
            } else {
                _changelistnumber = Perforce.GetChangelistNumber( User, Client, Changelist, true );
            }
            arguments.Append( _changelistnumber );

            return arguments.ToString();
        }

        #endregion Protected Instance Methods
    }
}
