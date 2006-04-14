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
    /// Checks files out of a ClearCase VOB.
    /// <seealso cref="ClearCaseCheckIn"/>
    /// <seealso cref="ClearCaseUnCheckOut"/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>cleartool checkout</c> command to check out ClearCase elements.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Performs a ClearCase checkout on the file <c>c:/views/viewdir/afile</c>. 
    ///   It is checked out as reserved on branch called <c>abranch</c>. 
    ///   All warning messages are suppressed. 
    ///   <c>Some comment text</c> is added to ClearCase as a comment.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <cccheckout viewpath="c:/views/viewdir/afile"
    ///         reserved="true"
    ///         branch="abranch"
    ///         nowarn="true"
    ///         comment="Some comment text"/>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("cccheckout")]
    public class ClearCaseCheckOut : ClearCaseBase {
        #region Private Instance Fields

        private FileInfo _viewPath;
        private bool _reserved = true;
        private FileInfo _out;
        private bool _noData;
        private string _branch;
        private bool _version;
        private bool _noWarn;
        private string _comment;
        private FileInfo _commentFile;
        private bool _preserveTime;

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
        /// <see langword="true" /> to check the element out as reserved.
        /// Default is <see langword="true" />.
        /// </summary>
        [TaskAttribute("reserved")]
        [BooleanValidator()]
        public bool Reserved {
            get { return _reserved; }
            set { _reserved = value; }
        }

        /// <summary>
        /// Creates a writable file under a different filename.
        /// </summary>
        [TaskAttribute("out")]
        public FileInfo OutFile {
            get { return _out; }
            set { _out = value; }
        }

        /// <summary>
        /// If <see langword="true" />, checks out the file but does not create
        /// an editable file containing its data. Default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("nodata")]
        [BooleanValidator()]
        public bool NoData {
            get { return _noData; }
            set { _noData = value; }
        }

        /// <summary>
        /// Specify a branch to check out the file to.
        /// </summary>
        [TaskAttribute("branch")]
        public string Branch {
            get { return _branch; }
            set { _branch = value; }
        }

        /// <summary>
        /// If <see langword="true" />, checkouts of elements with a version
        /// other than main latest will be allowed. Default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("version")]
        [BooleanValidator()]
        public bool Version {
            get { return _version; }
            set { _version = value; }
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
        /// If <see langword="true" />, the modification time will be preserved.
        /// Default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("preservetime")]
        [BooleanValidator()]
        public bool PreserveTime {
            get { return _preserveTime; }
            set { _preserveTime = value; }
        }

        #endregion Public Properties

        /// <summary>
        /// This is an override used by the base class to get command specific args.
        /// </summary>
        protected override string CommandSpecificArguments {
            get {
                StringBuilder arguments = new StringBuilder();
                arguments.Append("checkout ");

                if (Reserved) {
                    arguments.Append("-reserved ");
                } else {
                    arguments.Append("-unreserved ");
                }

                if (OutFile != null) {
                    arguments.AppendFormat("-out \"{0}\" ", OutFile.FullName);
                }

                if (NoData) {
                    arguments.Append("-ndata ");
                }

                if (Branch != null) {
                    arguments.AppendFormat("-branch {0} ", Branch);
                }

                if (Version) {
                    arguments.Append("-version ");
                }

                if (NoWarn) {
                    arguments.Append("-nwarn ");
                }

                // Either Comment or CommentFile
                // TODO: Throw exception if both are specified.
                if (Comment != null) {
                    arguments.AppendFormat("-comment \"{0}\" ", Comment);
                } else if (CommentFile != null) {
                    arguments.AppendFormat("-cfile \"{0}\" ", CommentFile.FullName);
                } else {
                    arguments.Append("-ncomment ");
                }

                if (PreserveTime) {
                    arguments.Append("-ptime ");
                }

                if (ViewPath != null) {
                    arguments.AppendFormat("\"{0}\"", ViewPath.FullName);
                }

                return arguments.ToString();
            }
        }
    }
}
