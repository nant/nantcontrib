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
    /// Locks ClearCase elements.
    /// <seealso cref="ClearCaseUnLock"/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>cleartool lock</c> command to lock ClearCase elements.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Performs a ClearCase lock on the object <c>stream:Application_Integration@\MyProject_PVOB</c>.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <cclock objsel="stream:Application_Integration@\MyProject_PVOB" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("cclock")]
    public class ClearCaseLock : ClearCaseBase {
        #region Private Instance Fields

        private bool _replace;
        private string _nusers;
        private bool _obsolete;
        private string _comment;
        private FileInfo _commentFile;
        private string _pname;
        private string _objsel;

        #endregion Private Instance Fields

        #region Public Properties

        /// <summary>
        /// If <see langword="true" /> an existing lock can be replaced.
        /// Default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("replace")]
        [BooleanValidator()]
        public bool Replace {
            get { return _replace; }
            set { _replace = value; }
        }

        /// <summary>
        /// Specifies user(s) who can still modify the object.
        /// Only one of <see cref="Nusers" /> or <see cref="Obsolete" /> may be
        /// used.
        /// </summary>
        [TaskAttribute("nusers")]
        public string Nusers {
            get { return _nusers; }
            set { _nusers = value; }
        }

        /// <summary>
        /// If <see langword="true" /> the object will be marked obsolete.
        /// Only one of <see cref="Nusers" /> or <see cref="Obsolete" /> may 
        /// be used. Default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("obsolete")]
        [BooleanValidator()]
        public bool Obsolete {
            get { return _obsolete; }
            set { _obsolete = value; }
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
        /// Specifies the object pathname to be locked.
        /// </summary>
        [TaskAttribute("pname")]
        public string Pname {
            get { return _pname; }
            set { _pname = value; }
        }

        /// <summary>
        /// Specifies the object(s) to be locked.
        /// </summary>
        [TaskAttribute("objsel")]
        public string ObjSel {
            get { return _objsel; }
            set { _objsel = value; }
        }

        #endregion Public Properties

        /// <summary>
        /// This is an override used by the base class to get command specific args.
        /// </summary>
        protected override string CommandSpecificArguments {
            get {
                StringBuilder arguments = new StringBuilder();
                arguments.Append("lock ");

                if (Replace) {
                    arguments.Append("-replace ");
                }

                if (Nusers != null) {
                    arguments.AppendFormat("-nusers {0}", Nusers);
                } else if (Obsolete) {
                    arguments.Append("-obsolete ");
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

                if (Pname != null) {
                    arguments.AppendFormat("-pname {0}", Pname);
                }

                if (ObjSel != null) {
                    arguments.AppendFormat("{0}", ObjSel);
                }

                return arguments.ToString();
            }
        }
    }
}
