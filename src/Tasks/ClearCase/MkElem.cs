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
    /// Creates elements in a ClearCase VOB.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>cleartool mkelem</c> command to create ClearCase elements.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Performs a ClearCase mkelem on the file <c>c:/views/viewdir/afile</c> with element type <c>text_file</c>. 
    ///   It checks in the file after creation and adds <c>Some comment text</c> as a comment.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <ccmkelem viewpath="c:/views/viewdir/afile"
    ///         eltype="text_file"
    ///         checkin="true"
    ///         comment="Some comment text"/>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("ccmkelem")]
    public class ClearCaseMkElem : ClearCaseBase {
        #region Private Instance Fields

        private FileInfo _viewPath;
        private string _comment;
        private FileInfo _commentFile;
        private bool _noWarn;
        private bool _noCheckout;
        private bool _checkin;
        private bool _preserveTime;
        private bool _master;
        private string _elType;
        private bool _mkPath;

        #endregion Private Instance Fields

        #region Public Properties

        /// <summary>
        /// Path to the ClearCase view file or directory that the command will
        /// operate on.
        /// </summary>
        [TaskAttribute("viewpath", Required=true)]
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
        /// If <see langword="true" />, warning will be suppressed.
        /// The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("nowarn")]
        [BooleanValidator()]
        public bool NoWarn {
            get { return _noWarn; }
            set { _noWarn = value; }
        }

        /// <summary>
        /// Perform a checkout after element creation.
        /// Default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("nocheckout")]
        [BooleanValidator()]
        public bool NoCheckout {
            get { return _noCheckout; }
            set { _noCheckout = value; }
        }

        /// <summary>
        /// Checkin element after creation.
        /// Default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("checkin")]
        [BooleanValidator()]
        public bool Checkin {
            get { return _checkin; }
            set { _checkin = value; }
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
        ///	Assign mastership of the main branch to the current site.
        ///	Default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("master")]
        [BooleanValidator()]
        public bool Master {
            get { return _master; }
            set { _master = value; }
        }
		
        /// <summary>
        /// Element type to use during element creation.
        /// </summary>
        [TaskAttribute("eltype")]
        public string ElType {
            get { return _elType; }
            set { _elType = value; }
        }

        /// <summary>
        ///	Create elements from the view-private parent directories.
        ///	Default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("mkpath")]
        [BooleanValidator()]
        public bool MkPath {
            get { return _mkPath; }
            set { _mkPath = value; }
        }

        #endregion Public Properties

        /// <summary>
        /// This is an override used by the base class to get command specific args.
        /// </summary>
        protected override string CommandSpecificArguments {
            get {
                StringBuilder arguments = new StringBuilder();
                arguments.Append("mkelem ");

                if (ElType != null) {
                    arguments.AppendFormat("-eltype {0} ", ElType);
                }

                if (NoCheckout) {
                    arguments.Append("-nco ");
                } else if (Checkin) {
                    arguments.Append("-ci ");
                    if (PreserveTime) {
                        arguments.Append("-ptime ");
                    }
                }

                if (MkPath) {
                    arguments.Append("-mkpath ");
                }

                if (Master) {
                    arguments.Append("-master ");
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

                if (ViewPath != null) {
                    arguments.AppendFormat("\"{0}\"", ViewPath.FullName);
                }

                return arguments.ToString();
            }
        }
    }
}
