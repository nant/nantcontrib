#region GNU Lesser General Public License
//
// NAntContrib
// Copyright (C) 2001-2006 Gerry Shaw
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307 USA
//
// Matt Trentini (matt.trentini@gmail.com)
//
#endregion

using System;
using System.IO;
using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.ClearCase {
    /// <summary>
    /// Uncheckout ClearCase elements.
    /// <seealso cref="ClearCaseCheckIn"/>
    /// <seealso cref="ClearCaseCheckOut"/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>cleartool unco</c> command to remove a ClearCase object.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Does a ClearCase uncheckout on the file <c>c:/views/viewdir/afile</c>. 
    ///   A copy of the file called <c>c:/views/viewdir/afile.keep</c> is kept.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <ccuncheckout viewpath="c:/views/viewdir/afile"
    ///         keepcopy="true"/>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("ccuncheckout")]
    public class ClearCaseUnCheckOut : ClearCaseBase {
        #region Private Instance Fields

        private FileInfo _viewPath;
        private bool _keepCopy = true;

        #endregion Private Instance Fields
 
        #region Public Properties

        /// <summary>
        /// Path to the ClearCase view file or directory that the command will
        /// operate on.
        /// </summary>
        [TaskAttribute("viewpath")]
        public FileInfo ViewPath {
            get { return _viewPath; }
            set { _viewPath = value; }
        }

        /// <summary>
        /// If <see langword="true" />, a view-private copy of the file with a
        /// .keep extension will be kept. Default is <see langword="true" />.
        /// </summary>
        [TaskAttribute("keepcopy")]
        [BooleanValidator()]
        public bool KeepCopy {
            get { return _keepCopy; }
            set { _keepCopy = value; }
        }

        #endregion Public Properties

        /// <summary>
        /// This is an override used by the base class to get command specific args.
        /// </summary>
        protected override string CommandSpecificArguments {
            get {
                StringBuilder arguments = new StringBuilder();
                arguments.Append("unco ");

                if (KeepCopy) {
                    arguments.Append("-keep ");
                } else {
                    arguments.Append("-rm ");
                }

                if (ViewPath != null) {
                    arguments.AppendFormat("\"{0}\"", ViewPath.FullName);
                }

                return arguments.ToString();
            }
        }
    }
}
