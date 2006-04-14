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
    /// Checks files into a ClearCase VOB.
    /// <seealso cref="ClearCaseCheckOut"/>
    /// <seealso cref="ClearCaseUnCheckOut"/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>cleartool checkin</c> command to check in ClearCase elements.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Performs a ClearCase checkin on the file <c>c:/views/viewdir/afile</c>. 
    ///   All warning messages are suppressed, and the element is checked in even if identical to the original.
    ///   Comment text from the file <c>acomment.txt</c> is added to ClearCase as a comment. All warning messages are suppressed. The file is checked in even if it is identical to the original.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <cccheckin viewpath="c:/views/viewdir/afile"
    ///         commentfile="acomment.txt"
    ///         nowarn="true"
    ///         identical="true"/>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("cccheckin")]
    public class ClearCaseCheckIn : ClearCaseBase {
        #region Private Instance Fields

        private FileInfo _viewPath;
        private string _comment;
        private FileInfo _commentFile;
        private bool _noWarn;
        private bool _preserveTime;
        private bool _keepCopy = true;
        private bool _identical;

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
        /// Specify a comment. Only one of <see cref="Comment" /> or 
        /// <see cref="CommentFile" /> may be used.
        /// </summary>
        [TaskAttribute("comment")]
        public string Comment {
            get { return _comment; }
            set { _comment = value; }
        }

        /// <summary>
        /// Specify a file containing a comment. Only one of <see cref="Comment" />
        /// or <see cref="CommentFile" /> may be used.
        /// </summary>
        [TaskAttribute("commentfile")]
        public FileInfo CommentFile {
            get { return _commentFile; }
            set { _commentFile = value; }
        }

        /// <summary>
        /// <see langword="true" /> if warning messages should be suppressed.
        /// The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("nowarn")]
        [BooleanValidator()]
        public bool NoWarn {
            get { return _noWarn; }
            set { _noWarn = value; }
        }

        /// <summary>
        /// If <see langword="true" />, the modification time will be preserved.
        /// Default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("preservetime")]
        [BooleanValidator()]
        public bool PreserveTime {
            get { return _preserveTime; }
            set { _preserveTime = value; }
        }

        /// <summary>
        /// <see langword="true" /> to keep a view-private copy of the file with
        /// a .keep extension. Default is <see langword="true" />.
        /// </summary>
        [TaskAttribute("keepcopy")]
        [BooleanValidator()]
        public bool KeepCopy {
            get { return _keepCopy; }
            set { _keepCopy = value; }
        }

        /// <summary>
        /// If <see langword="true" />, files may be checked in even if identical
        /// to the original.  Default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("identical")]
        [BooleanValidator()]
        public bool Identical {
            get { return _identical; }
            set { _identical = value; }
        }

        #endregion Public Properties

        /// <summary>
        /// This is an override used by the base class to get command specific args.
        /// </summary>
        protected override string CommandSpecificArguments {
            get {
                StringBuilder arguments = new StringBuilder();
                arguments.Append("checkin ");

                // Either Comment or CommentFile
                // TODO: Throw exception if both are specified.
                if (Comment != null) {
                    arguments.AppendFormat("-comment \"{0}\" ", Comment);
                } else if (CommentFile != null) {
                    arguments.AppendFormat("-cfile \"{0}\" ", CommentFile.FullName);
                } else {
                    arguments.Append("-ncomment ");
                }

                if (NoWarn) {
                    arguments.Append("-nwarn ");
                }

                if (KeepCopy) {
                    arguments.Append("-keep ");
                } else {
                    arguments.Append("-rm ");
                }

                if (PreserveTime) {
                    arguments.Append("-ptime ");
                }

                if (Identical) {
                    arguments.Append("-identical ");
                }

                if (ViewPath != null) {
                    arguments.AppendFormat("\"{0}\"", ViewPath.FullName);
                }

                return arguments.ToString();
            }
        }
    }
}
