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
    /// Add/modify/delete a client spec in perforce.
    /// </summary>
    /// <example>
    ///   <para>
    ///   Add a client (modify if already present and have sufficient rights).
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <p4client clientname="myClient" view="//root/test/..." />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>Delete a client.</para>
    ///   <code>
    ///     <![CDATA[
    /// <p4client delete="true" clientname="myClient" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("p4client")]
    public class P4Client : P4Base {
        #region Private Instance Fields

        private string _client;
        private string _root;
        private bool _delete;
        private bool _force;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// Name of client to create/delete.
        /// </summary>
        [TaskAttribute("clientname", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string ClientName {
            get { return _client; }
            set { _client = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Root path for client spec.
        /// </summary>
        [TaskAttribute("root", Required=false)]
        public string Root {
            get { return _root; }
            set { _root = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Delete the named client. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("delete", Required=false)]
        [BooleanValidator()]
        public bool Delete {
            get { return _delete; }
            set { _delete = value; }
        }

        /// <summary>
        /// Force a delete even if files are open. The default is 
        /// <see langword="false" />.
        /// </summary>
        [TaskAttribute("force", Required=false)]
        [BooleanValidator()]
        public bool Force {
            get { return _force; }
            set { _force = value; }
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
            arguments.Append("client ");

            if (!Delete && View == null) {
                throw new BuildException("<p4client> requires either a \"view\""
                    + " to create, or \"delete\"=\"true\" to delete a client.",
                    Location);
            }

            if (Delete) {
                arguments.Append("-d ");
                if (Force) {
                    arguments.Append("-f ");
                }
            } else {
                if (View == null || Root == null) {
                    throw new BuildException("A \"view\" and \"root\" are required for creating/editing with <p4client>.");
                }
                // this creates or edits the client, then the -o outputs to standard out
                Perforce.CreateClient(User, ClientName, Root, View);
                arguments.Append("-o ");
            }
            arguments.Append(ClientName);

            return arguments.ToString();
        }

        #endregion Protected Instance Methods
    }
}
