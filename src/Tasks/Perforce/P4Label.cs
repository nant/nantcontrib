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
    /// <summary>
    /// Create or edit a label specification and its view. Wraps the 'p4 label' command.
    /// </summary>
    /// <example>
    /// <para>Create a new label called "SDK_V1.2"</para>
    /// <code>
    ///        <![CDATA[
    ///    <p4label labelname="SDK_V1.2" />
    ///        ]]>
    /// </code>
    /// <para>Delete the previously created label.</para>
    /// <code>
    ///        <![CDATA[
    ///    <p4label labelname="SDK_V1.2" delete="true" />
    ///        ]]>
    /// </code>
    /// </example>
    [TaskName("p4label")]
    public class P4Label : P4Base {
        #region Private Instance Fields

        private string _labelname = null;
        private bool _delete = false;

        #endregion

        #region Public Instance Fields

        /// <summary>
        /// Name of label to create/delete. required.
        /// </summary>
        [TaskAttribute("labelname",Required=true)]
        public string Labelname {
            get { return _labelname; }
            set { _labelname = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Delete the named label. default is false. optional.
        /// </summary>
        [TaskAttribute("delete",Required=false)]
        [BooleanValidator()]
        virtual public bool Delete {
            get { return _delete; }
            set { _delete = value; }
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
            arguments.Append("label ");

            if ( Labelname == null) {
                throw new BuildException("A \"labelname\" is required for p4label");
            }
            if ( !Delete && View == null ) {
                throw new BuildException("p4label requires either a \"view\" to create, or delete=true to delete a label.");
            }

            if ( Delete ) {
                arguments.Append("-d ");
            } else {
                // this creates or edits the label, then the -o outputs to standard out
                Perforce.CreateLabel(User,Labelname,View);
                arguments.Append("-o ");
            }
            arguments.Append( Labelname );

            return arguments.ToString();
        }
        
        #endregion Override implementation of Task
    }
}
