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
    /// Open file(s) in a client workspace for deletion from the depot.
    /// </summary>
    /// <example>
    ///   <para>
    ///   Mark all cs files under the give view for deletion and place them in 
    ///   the "Deleting" changelist.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <p4delete view="//Root/ProjectX/Test/...*.cs" changelist="Deleting" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("p4delete")]
    public class P4Delete : P4Base {
        #region Private Instance Fields

        private string _changelist;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// Changelist to place the marked for deletion item into.
        /// </summary>
        [TaskAttribute("changelist", Required=false)]
        public string Changelist {
            get { return _changelist; }
            set { _changelist = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The client, branch or label view to operate upon.
        /// </summary>
        [TaskAttribute("view", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public override string View {
            get { return base.View; }
            set { base.View = value; }
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
            arguments.Append("delete ");

            if (Changelist != null) {
                arguments.Append(string.Format("-c {0} ", Perforce.GetChangelistNumber(
                    User, Client, Changelist, true)));
            }
            arguments.Append(View);

            return arguments.ToString();
        }
        
        #endregion Protected Instance Methods
    }
}
