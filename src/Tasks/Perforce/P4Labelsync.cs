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

using System;
using System.Text;
using NAnt.Core;
using NAnt.Core.Util;
using NAnt.Core.Tasks;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.Perforce {
    /// <summary>Synchronize a label with the contents of the current client workspace.
    /// <example>
    /// <para>Apply a previously created label to the specified view.</para>
    /// <code>
    ///        <![CDATA[
    ///    <p4labelsync labelname="SDK_V1.2" view="//Root/..." />
    ///        ]]>
    /// </code>
    /// </example>
    /// </summary>
    public class P4Labelsync : P4Base {
        #region Private Instance Fields

        private string _labelname = null;
        private bool _delete = false;

        #endregion

        #region Public Instance Fields

        /// <summary>
        /// Labelname to sync the specified or default view with. required.
        /// </summary>
        [TaskAttribute("labelname",Required=true)]
        public string Labelname  {
            get { return _labelname; }
            set { _labelname = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Delete the view defined in the label, or matching the input view from the label. optional.
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
            arguments.Append("labelsync ");

            if ( Labelname == null) {
                throw new BuildException("A \"labelname\" is required for p4labelsync");
            }

            if ( Delete ) {
                arguments.Append("-d ");
            }
            if ( View != null ) {
                arguments.Append( View );
            }

            return arguments.ToString();
        }
        
        #endregion Override implementation of Task
    }
}
