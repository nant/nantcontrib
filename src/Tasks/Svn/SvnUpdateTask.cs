// NAnt - A .NET build tool
// Copyright (C) 2001-2003 Gerry Shaw
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
// Clayton Harbour (claytonharbour@sporadicism.com)

using System;
using System.Text;
using System.IO;
using System.Diagnostics;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;
using NAnt.Core.Types;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks.Svn {
    /// <summary>
    /// Executes the svn update specified by the command attribute.
    /// </summary>
    /// <example>
    ///   <para>Update Gentle.NET.</para>
    ///   <code>
    ///     <![CDATA[
    /// <svn-update
    ///     destination="c:\dev\src\gentle.net" 
    ///     uri="http://www.mertner.com/svn/repos/projects/gentle" 
    ///     recursive="true"
    ///     quiet="true"
    ///     username="anonymoose"
    ///     password="Canada" 
    ///     revision="HEAD"
    ///     cach-auth="false"
    ///     config-dir="c:\home"
    /// />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("svn-update")]
    public class SvnUpdateTask : AbstractSvnTask {
        #region Private Instance Fields

        private string COMMAND_NAME = "update";

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// Gets the svn command to execute.
        /// </summary>
        /// <value>
        /// The svn command to execute. The default value is "update".
        /// </value>
        public override string CommandName {
            get { return this.COMMAND_NAME; }
            set { this.COMMAND_NAME = value; }
        }

        #endregion Public Instance Properties
    }
}
