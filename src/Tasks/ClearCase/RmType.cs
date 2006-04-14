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

using NAnt.Contrib.Types.ClearCase;

namespace NAnt.Contrib.Tasks.ClearCase {
    /// <summary>
    /// Removes elements from a ClearCase VOB.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>cleartool rmtype</c> command to remove a ClearCase object.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Performs a ClearCase rmtype to remove a <see cref="NAnt.Contrib.Types.ClearCase.TypeKind.Label" />
    ///   type named <c>VERSION_1</c>. 
    ///   Comment text from the file <c>acomment.txt</c> is added as a comment. 
    ///   All instances of the type are removed, including the type object itself.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <ccrmtype typekind="Label"
    ///         typename="VERSION_1"
    ///         commentfile="acomment.txt"
    ///         removeall="true" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("ccrmtype")]
    public class ClearCaseRmType : ClearCaseBase {
        #region Private Instance Fields

        private TypeKind _typekind;
        private string _typename;
        private bool _ignore;
        private bool _removeAll;
        private string _comment;
        private FileInfo _commentFile;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The kind of type to remove.
        /// </summary>
        [TaskAttribute("typekind", Required=true)]
        public TypeKind TypeKind {
            get { return _typekind; }
            set { _typekind = value; }
        }

        /// <summary>
        /// The name of the object to remove.
        /// </summary>
        [TaskAttribute("typename", Required=true)]
        public string TypeName {
            get { return _typename; }
            set { _typename = value; }
        }

        /// <summary>
        /// Used with <see cref="NAnt.Contrib.Types.ClearCase.TypeKind.Trigger" /> types only. 
        /// Forces removal of <see cref="NAnt.Contrib.Types.ClearCase.TypeKind.Trigger" /> type even if a 
        /// pre-operation trigger would prevent its removal.
        /// The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("ignore")]
        [BooleanValidator()]
        public bool Ignore {
            get { return _ignore; }
            set { _ignore = value; }
        }

        /// <summary>
        /// Removes all instances of a type and the type object itself.
        /// The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("removeall")]
        [BooleanValidator()]
        public bool RemoveAll {
            get { return _removeAll; }
            set { _removeAll = value; }
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

        #endregion Public Instance Properties

        /// <summary>
        /// This is an override used by the base class to get command specific args.
        /// </summary>
        protected override string CommandSpecificArguments {
            get {
                StringBuilder arguments = new StringBuilder();
                arguments.Append("rmtype ");

                if (Ignore) {
                    arguments.Append("-ignore ");
                }

                if (RemoveAll) {
                    arguments.Append("-rmall -force ");
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

                arguments.AppendFormat("{0}:{1}", TypeKind, TypeName);

                return arguments.ToString();
            }
        }
    }
}
