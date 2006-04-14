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
    /// Applies a ClearCase label.
    /// <seealso cref="ClearCaseMkLbType"/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>cleartool mklabel</c> command to apply a ClearCase label to specified elements.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Performs a ClearCase mklabel on the file <c>c:/views/viewdir/afile</c> under 
    ///   the <c>main</c> branch for <c>version 2</c> (<c>\main\2</c>).  All matching
    ///   elements will be applied with label <c>VERSION_1</c>.
    ///   <c>Some comment text</c> is added as a comment.  Subdirectories will be recursed.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <ccmklabel viewpath="c:/views/viewdir/afile"
    ///         comment="Some comment text"
    ///         recurse="true"
    ///         version="\main\2"
    ///         typename="VERSION_1"/>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("ccmklabel")]
    public class ClearCaseMkLabel : ClearCaseBase {
        #region Private Instance Fields

        private string _typename;
        private FileInfo _viewPath;
        private bool _replace;
        private bool _recurse;
        private string _version;
        private string _vob;
        private string _comment;
        private FileInfo _commentFile;
        private bool _follow;

        #endregion Private Instance Fields
        
        #region Public Properties
        /// <summary>
        /// Name of the label type
        /// </summary>
        [TaskAttribute("typename", Required=true)]
        public string Typename {
            get { return _typename; }
            set { _typename = value; }
        }

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
        /// If <see langword="true" />, allow the replacement of a 
        /// label of the same type on the same branch.
        /// The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("replace")]
        [BooleanValidator()]
        public bool Replace {
            get { return _replace; }
            set { _replace = value; }
        }

        /// <summary>
        /// If <see langword="true" />, process each subdirectory recursively under the viewpath.
        /// Default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("recurse")]
        [BooleanValidator()]
        public bool Recurse {
            get { return _recurse; }
            set { _recurse = value; }
        }

        /// <summary>
        /// Identify a specific version to attach the label to.
        /// </summary>
        [TaskAttribute("version")]
        public string Version {
            get { return _version; }
            set { _version = value; }
        }

        /// <summary>
        /// Path to the ClearCase view file or directory that the command will operate on.
        /// </summary>
        [TaskAttribute("vob")]
        public string Vob {
            get { return _vob; }
            set { _vob = value; }
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
        /// For any VOB symbolic links encountered, labels the corresponding target.
        /// </summary>
        [TaskAttribute("follow")]
        [BooleanValidator()]
        public bool Follow {
            get { return _follow; }
            set { _follow = value; }
        }

        #endregion Public Properties

        /// <summary>
        /// This is an override used by the base class to get command specific args.
        /// </summary>
        protected override string CommandSpecificArguments {
            get {
                StringBuilder arguments = new StringBuilder();
                arguments.Append("mklabel ");

                if (Replace) {
                    arguments.Append("-replace ");
                }

                if (Recurse) {
                    arguments.Append("-recurse ");
                }

                if (Follow) {
                    arguments.Append("-follow ");
                }

                if (Version != null) {
                    arguments.AppendFormat("-version {0} ", Version);
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
                
                // Should always have a label typename
                arguments.Append(Typename + " ");

                if (ViewPath != null) {
                    arguments.AppendFormat("\"{0}\"", ViewPath.FullName);
                }

                return arguments.ToString();
            }
        }
    }
}
