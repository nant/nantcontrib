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
    /// Move opened files between changelists or change the files’ type.
    /// </summary>
    /// <remarks>
    /// This task has two different but related uses:
    /// Moving opened files between changelists (default or named).
    /// Changing the type of an opened file.
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Move the specified files matching the view into the "New" changelist.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <p4reopen view="//Root/...Version.xml" changelist="New" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Modify the specified files matching the view to the given file type. 
    ///   The change won't affect the repository until submitted.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <p4reopen view="//Root/...*.res" type="binary" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("p4reopen")]
    public class P4Reopen : P4Base {
        #region Private Instance Fields

        private string _changelist;
        private string _type;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The client, branch or label view to operate upon.
        /// </summary>
        [TaskAttribute("view", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public override string View {
            get { return base.View; }
            set { base.View = value; }
        }

        /// <summary>
        /// Changelist to place the reopened files into.
        /// </summary>
        [TaskAttribute("changelist", Required=false)]
        public string Changelist {
            get { return _changelist; }
            set { _changelist = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// File Type settings.
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
        /// Builds the command string for this particular command.
        /// </summary>
        /// <returns>
        /// The command string for this particular command.
        /// </returns>
        protected string getSpecificCommandArguments( ) {
            StringBuilder arguments = new StringBuilder();
            arguments.Append("reopen ");

            if (Changelist != null) {
                if (Changelist.ToLower() == "default") {
                    arguments.Append("-c default ");
                } else {
                    arguments.Append(string.Format("-c {0} ", Perforce.GetChangelistNumber(
                        User, Client, Changelist, true)));
                }
            }
            if (Type != null) {
                arguments.Append(string.Format("-t {0} ", Type));
            }
            arguments.Append(View);

            return arguments.ToString();
        }

        #endregion Protected Instance Methods
    }
}
