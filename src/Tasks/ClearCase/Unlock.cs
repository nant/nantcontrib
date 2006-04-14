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
    /// Unlocks ClearCase elements.
    /// <seealso cref="ClearCaseLock"/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>cleartool unlock</c> command to unlock a ClearCase object.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Performs a ClearCase unlock on the object <c>stream:Application_Integration@\MyProject_PVOB</c>.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <ccunlock objsel="stream:Application_Integration@\MyProject_PVOB" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("ccunlock")]
    public class ClearCaseUnLock : ClearCaseBase {
        #region Private Instance Fields

        private string _comment;
        private FileInfo _commentFile;
        private string _pname;
        private string _objsel;

        #endregion Private Instance Fields

        #region Public Properties

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
        /// Specifies the object pathname to be unlocked.
        /// </summary>
        [TaskAttribute("pname")]
        public string Pname {
            get { return _pname; }
            set { _pname = value; }
        }

        /// <summary>
        /// Specifies the object(s) to be unlocked.
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
                arguments.Append("unlock ");

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
