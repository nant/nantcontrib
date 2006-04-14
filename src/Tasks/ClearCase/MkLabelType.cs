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
    /// Creates a label object in a ClearCase VOB.
    /// <seealso cref="ClearCaseMkLabel"/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>cleartool mklabeltype</c> command to create a ClearCase label object.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Performs a ClearCase mklbtype to create a label type named <c>VERSION_1</c>. 
    ///   It is created as <c>ordinary</c> so it is available only to the current VOB. 
    ///   The text <c>Development version 1</c> is added as a comment.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <ccmklbtype typename="VERSION_1"
    ///         ordinary="true"
    ///         comment="Development version 1"/>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("ccmklbtype")]
    public class ClearCaseMkLbType : ClearCaseBase {
        #region Private Instance Fields

        private string _typename;
        private string _vob;
        private bool _replace;
        private bool _global;
        private bool _ordinary;
        private bool _pbranch;
        private bool _shared;
        private string _comment;
        private FileInfo _commentFile;

        #endregion Private Instance Fields

        #region Public Properties
        /// <summary>
        /// Name of the label type to create.
        /// </summary>
        [TaskAttribute("typename", Required=true)]
        public string Typename {
            get { return _typename; }
            set { _typename = value; }
        }

        /// <summary>
        /// Name of the VOB.  Must be a valid path to somewhere on a VOB.
        /// </summary>
        [TaskAttribute("vob")]
        public string Vob {
            get { return _vob; }
            set { _vob = value; }
        }

        /// <summary>
        /// If <see langword="true" />, allow an existing label definition to be replaced.
        /// The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("replace")]
        [BooleanValidator()]
        public bool Replace {
            get { return _replace; }
            set { _replace = value; }
        }

        /// <summary>
        /// Creates a label type that is global to the VOB or to VOB's that use this VOB.
        /// Either global or ordinary can be specified, not both. 
        /// The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("global")]
        [BooleanValidator()]
        public bool Global {
            get { return _global; }
            set { _global = value; }
        }

        /// <summary>
        /// Creates a label type that can be used only in the current VOB.
        /// Either global or ordinary can be specified, not both. 
        /// Although <see langword="false" /> by default, if global is also <see langword="false" /> or not specified ordinary is the default behaviour.
        /// </summary>
        [TaskAttribute("ordinary")]
        [BooleanValidator()]
        public bool Ordinary {
            get { return _ordinary; }
            set { _ordinary = value; }
        }

        /// <summary>
        /// If <see langword="true" /> the label type is allowed to be used once per branch in a given element's version tree.
        /// The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("pbranch")]
        [BooleanValidator()]
        public bool PBranch {
            get { return _pbranch; }
            set { _pbranch = value; }
        }

        /// <summary>
        /// Sets the way mastership is checked by ClearCase. See ClearCase documentation for details.
        /// The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("shared")]
        [BooleanValidator()]
        public bool Shared {
            get { return _shared; }
            set { _shared = value; }
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

        #endregion Public Properties

        /// <summary>
        /// This is an override used by the base class to get command specific args.
        /// </summary>
        protected override string CommandSpecificArguments {
            get {
                StringBuilder arguments = new StringBuilder();
                arguments.Append("mklbtype ");

                if (Replace) {
                    arguments.Append("-replace ");
                }

                if (Global) {
                    arguments.Append("-global ");
                }

                if (Ordinary) {
                    arguments.Append("-ordinary ");
                }

                if (PBranch) {
                    arguments.Append("-pbranch ");
                }

                if (Shared) {
                    arguments.Append("-shared ");
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
                arguments.Append(Typename);

                if (Vob != null) {
                    arguments.AppendFormat("@{0}", Vob);
                }

                return arguments.ToString();
            }
        }
    }
}
