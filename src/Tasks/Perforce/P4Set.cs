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
    /// Set registry variables that perforce uses.
    /// </summary>
    /// <remarks>
    /// Note: the environment variables that p4 uses will be set, but will not
    /// be validated.
    /// </remarks>
    /// <example>
    ///   <para>Modify any of the three variables (at least one required).</para>
    ///   <code>
    ///     <![CDATA[
    /// <p4set client="myClient" user="jonb" port="server:1666" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("p4set")]
    public class P4Set : P4Base {
        #region Override implementation of P4Base

        /// <summary>
        /// This is an override used by the base class to get command specific args.
        /// </summary>
        protected override string CommandSpecificArguments  {
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
            arguments.Append("info ");

            if (User == null && Client == null && Port == null) {
                throw new BuildException("At least one of the following: \"client\", \"user\", or \"port\" is required for p4set");
            }

            if (User != null) {
                Perforce.SetVariable("P4USER",User);
            }
            if (Client != null) {
                Perforce.SetVariable("P4CLIENT",Client);
            }
            if (Port != null) {
                Perforce.SetVariable("P4PORT",Port);
            }

            return arguments.ToString();
        }

        #endregion Protected Instance Methods
    }
}
