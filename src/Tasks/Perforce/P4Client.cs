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
using NAnt.Core.Util;
using NAnt.Core.Tasks;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.Perforce {
    /// <summary>Add/Modify/Delete a client spec in perforce.
    /// <example>
    /// <para>Add a client (Modify if already present and have sufficient rights)</para>
    /// <code>
    ///        <![CDATA[
    ///        <p4client clientname="myClient" view="//root/test/..." />
    ///        ]]>
    /// </code>
    /// <para>Delete a client</para>
    /// <code>
    ///     <![CDATA[
    ///        <p4client delete="true" clientname="myClient" />
    ///        ]]>
    /// </code>
    /// </example>
    /// </summary>
    [TaskName("p4client")]
    public class P4Client : P4Base {
        #region Private Instance Fields

        private string _clientname = null;
        private string _root = null;
        private bool _delete = false;
        private bool _force = false;

        #endregion

        #region Public Instance Fields

        /// <summary>
        /// Name of client to create/delete. required.
        /// </summary>
        [TaskAttribute("clientname",Required=true)]
        public string Clientname  {
            get { return _clientname; }
            set { _clientname = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Root path for client spec.
        /// </summary>
        [TaskAttribute("root",Required=false)]
        public string Root {
            get { return _root; }
            set { _root = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Delete the named client. default is false. optional.
        /// </summary>
        [TaskAttribute("delete",Required=false)]
        [BooleanValidator()]
        virtual public bool Delete {
            get { return _delete; }
            set { _delete = value; }
        }

        /// <summary>
        /// Force a delete even if files open. default is false. optional.
        /// </summary>
        [TaskAttribute("force",Required=false)]
        [BooleanValidator()]
        virtual public bool Force {
            get { return _force; }
            set { _force = value; }
        }

        #endregion

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
            arguments.Append("client ");

            if ( Clientname == null) {
                throw new BuildException("A \"clientname\" is required for p4client");
            }
            if ( !Delete && View == null ) {
                throw new BuildException("p4client requires either a \"view\" to create, or delete=true to delete a client.");
            }

            if ( Delete ) {
                arguments.Append("-d ");
                if ( Force ) {
                    arguments.Append("-f ");
                }
            } else {
                if ( ( View == null ) || ( Root == null ) ) {
                    throw new BuildException("A \"view\" and \"root\" are required for creating/editing with p4client.");
                }
                // this creates or edits the client, then the -o outputs to standard out
                Perforce.CreateClient(User,Clientname,Root,View);
                arguments.Append("-o ");
            }
            arguments.Append( Clientname );

            return arguments.ToString();
        }
        #endregion Override implementation of Task
    }
}
