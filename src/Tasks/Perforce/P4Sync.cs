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
    /// Synchronize client space to a Perforce depot view.
    /// </summary>  
    /// <example>
    ///   <para>
    ///   Sync to head using P4USER, P4PORT and P4CLIENT settings specified.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <p4sync 
    ///     view="//projects/foo/main/source/..."
    ///     user="fbloggs"
    ///     port="km01:1666"
    ///     client="fbloggsclient"
    /// />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>Sync to head using default p4 environment variables.</para>
    ///   <code>
    ///     <![CDATA[
    /// <p4sync view="//projects/foo/main/source/..." />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>Force a re-sync to head, refreshing all files.</para>
    ///   <code>
    ///     <![CDATA[
    /// <p4sync force="true" view="//projects/foo/main/source/..." />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>Sync to a label.</para>
    ///   <code>
    ///     <![CDATA[
    /// <p4sync label="myPerforceLabel" />
    ///     ]]>
    ///   </code>
    ///</example>
    [TaskName("p4sync")]
    public class P4Sync : P4Base {
        #region Private Instance Fields

        private string _label;
        private bool _force;

        #endregion Private Instance Fields

        #region Public Instance Properties
        
        /// <summary> Label to sync client to; optional.
        /// </summary>
        [TaskAttribute("label")]
        public string Label {
            get { return _label; }
            set { _label = StringUtils.ConvertEmptyToNull(value); }
        }
        
        /// <summary>
        /// Force a refresh of files. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("force")]
        [BooleanValidator()]
        public bool Force {
            set { _force = value;}
            get { return _force; }
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
            arguments.Append("sync ");

            if (Force) {
                arguments.Append("-f ");
            }
            if (View != null) {
                arguments.Append(View + " ");
            }
            if (Label != null) {
                arguments.Append(string.Format("@{0} ", Label));
            }

           return arguments.ToString();
        }

        #endregion Override implementation of Task
    }
}
