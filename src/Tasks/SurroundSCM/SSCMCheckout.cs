//
// NAntContrib
// Copyright (C) 2001-2004
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
// Matt Harp; Seapine Software, Inc.
//

using System.Text;

using NAnt.Core.Attributes;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks.SurroundSCM {
    /// <summary>
    /// Checks out files from a <see href="http://www.seapine.com/surroundscm.html">Surround SCM</see>
    /// repository.
    /// </summary>
    /// <remarks>
    /// You can check out single files, multiple files, or a full repository. 
    /// Surround SCM creates a read-write copy of the files in the working 
    /// directory.
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Check Out all files and repositories from repository 'Mainline/Widget' 
    ///   recursively from the 'Widget 1.0' branch to the working directory setup 
    ///   for user 'administrator'. This call forces the file retrieval from the 
    ///   server even if the local file is current and overwrites any writable
    ///   local files with the server copy.
    ///   </para>
    ///   <code>
    /// &lt;sscmcheckout
    ///     serverconnect=&quot;localhost:4900&quot;
    ///     serverlogin=&quot;administrator:&quot;
    ///     file=&quot;/&quot;
    ///     branch=&quot;Widget 1.0&quot;
    ///     repository=&quot;Mainline/Widget&quot;
    ///     recursive=&quot;true&quot;
    ///     force=&quot;true&quot;
    ///     comment=&quot;This is my Check Out comment&quot;
    /// /&gt;
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Check Out version 1 of the file 'Mainline/Widget/Widget.java' exclusively 
    ///   from the 'Widget 1.0' branch to the working directory setup for user 
    ///   'administrator'. Writable local files are not overwritten, even if they 
    ///   are out of date.
    ///   </para>
    ///   <code>
    /// &lt;sscmcheckout
    ///     serverconnect=&quot;localhost:4900&quot;
    ///     serverlogin=&quot;administrator:&quot;
    ///     quiet=&quot;true&quot;
    ///     file=&quot;Widget.java&quot;
    ///     branch=&quot;Widget 1.0&quot;
    ///     repository=&quot;Mainline/Widget&quot;
    ///     overwrite=&quot;false&quot;
    ///     writable=&quot;true&quot;
    ///     version=&quot;1&quot;
    ///     exclusive=&quot;true&quot;
    /// /&gt;
    ///   </code>
    /// </example>
    [TaskName("sscmcheckout")]
    public class SSCMCheckout : SSCMTask {
        #region Private Instance Fields

        private string _branch;
        private string _repository;
        private string _file;
        private string _comment;
        private bool _force;
        private bool _quiet;
        private bool _recursive;
        private bool _overwrite;
        private string _timestamp;
        private bool _exclusive;
        private string _version;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// Surround SCM branch name. The default is pulled from the local 
        /// working directory.
        /// </summary>
        [TaskAttribute("branch", Required=false)]
        public string Branch { 
            get { return _branch; }
            set { _branch = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Surround SCM repository path. The default is pulled from the local 
        /// working directory.
        /// </summary>
        [TaskAttribute("repository", Required=false)]
        public string Repository { 
            get { return _repository; }
            set { _repository = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// File or repository name. Can be / or empty, which means the 
        /// repository specified by the <see cref="Repository" /> attribute
        /// or the default repository.
        /// </summary>
        [TaskAttribute("file", Required=false)]
        public string File { 
            get { return _file; }
            set { _file = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Comment for the check-out.
        /// </summary>
        [TaskAttribute("comment", Required=false)]
        public string Comment { 
            get { return _comment; }
            set { _comment = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Force file retrieval from server regardless of the local copy status.
        /// The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("force", Required=false)]
        public bool Force { 
            get { return _force; }
            set { _force = value; }
        }

        /// <summary>
        /// Do not list repository and local full path of the Surround SCM server.
        /// The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("quiet", Required=false)]
        public bool Quiet { 
            get { return _quiet; }
            set { _quiet = value; }
        }

        /// <summary>
        /// Recursively get files and sub-repositories. The default is 
        /// <see langword="false" />.
        /// </summary>
        [TaskAttribute("recursive", Required=false)]
        public bool Recursive { 
            get { return _recursive; }
            set { _recursive = value; }
        }

        /// <summary>
        /// Specifies whether to overwrite local writable files. The default is
        /// <see langword="false" />.
        /// </summary>
        [TaskAttribute("overwrite", Required=false)]
        public bool Overwrite { 
            get { return _overwrite; }
            set { _overwrite = value; }
        }

        /// <summary>
        /// Specifies how to set the local file's date/time. Possible values are
        /// <c>current</c>, <c>modify</c> or <c>checkin</c>.
        /// </summary>
        [TaskAttribute("timestamp", Required=false)]
        public string Timestamp { 
            get { return _timestamp; }
            set { _timestamp = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Exclusively lock the files. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("exclusive", Required=false)]
        public bool Exclusive { 
            get { return _exclusive; }
            set { _exclusive = value; }
        }

        /// <summary>
        /// Specifies the file version to check out. Ignored if no specific 
        /// filename is set using the <see cref="File" /> attribute.
        /// </summary>
        [TaskAttribute("version", Required=false)]
        public string Version { 
            get { return _version; }
            set { _version = StringUtils.ConvertEmptyToNull(value); }
        }

        #endregion Public Instance Properties

        #region Override implementation of SSCMTask

        /// <summary>
        /// Writes the task-specific arguments to the specified 
        /// <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="argBuilder">The <see cref="StringBuilder" /> to write the task-specific arguments to.</param>
        protected override void WriteCommandLineArguments(StringBuilder argBuilder) {
            argBuilder.Append("checkout");

            // filename
            if (File != null && File != "/") {
                argBuilder.Append(" \"");
                argBuilder.Append(File);
                argBuilder.Append("\"");
            } else {
                argBuilder.Append(" /");
            }

            // branch
            if (Branch != null) {
                argBuilder.Append(" -b\"");
                argBuilder.Append(Branch);
                argBuilder.Append("\"");
            }

            // repository
            if (Repository != null) {
                argBuilder.Append(" -p\"");
                argBuilder.Append(Repository);
                argBuilder.Append("\"");
            }

            // comment
            if (Comment != null) {
                argBuilder.Append(" -cc\"");
                argBuilder.Append(Comment);
                argBuilder.Append("\"");
            } else {
                argBuilder.Append(" -c-");
            }

            // version
            if (File != null && Version != null) {
                argBuilder.Append(" -v");
                argBuilder.Append(Version);
                argBuilder.Append("");
            }

            // CLI always overwrites local file if users tells us to force the fetch.
            if (!Force) {
                if (Overwrite) {
                    argBuilder.Append(" -wreplace");
                } else {
                    argBuilder.Append(" -wskip");
                }
            }

            // timestamp
            if (Timestamp != null) {
                argBuilder.Append(" -t");
                argBuilder.Append(Timestamp);
            }

            // misc flags
            if (Quiet) {
                argBuilder.Append(" -q");
            }
            if (Force) {
                argBuilder.Append(" -f");
            }
            if (Recursive) {
                argBuilder.Append(" -r");
            }
            if (Exclusive) {
                argBuilder.Append(" -e");
            }
        }

        #endregion Override implementation of SSCMTask
    }
}
