//
// NAntContrib
// Copyright (C) 2001-2004
//
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
    /// Checks in files in <see href="http://www.seapine.com/surroundscm.html">Surround SCM</see>
    /// repository.
    /// </summary>
    /// <remarks>
    /// Check in updated Surround SCM files with changes, removes the lock on 
    /// the files, and makes changes available to other users.
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Check In all files and repositories from repository 'Mainline/Widget' 
    ///   recursively from the 'Widget 1.0' branch to the working directory setup 
    ///   for user 'administrator'. This call outputs the progress of the Check In 
    ///   to the console.
    ///   </para>
    ///   <code>
    /// &lt;sscmcheckin
    ///     serverconnect=&quot;localhost:4900&quot;
    ///     serverlogin=&quot;administrator:&quot;
    ///     file=&quot;/&quot;
    ///     branch=&quot;Widget 1.0&quot;
    ///     repository=&quot;Mainline/Widget&quot;
    ///     recursive=&quot;true&quot;
    ///     comment=&quot;I made some changes&quot;
    /// /&gt;
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Check in file 'Mainline/Widget/Widget.java' from the 'Widget 1.0' 
    ///   branch from the working directory setup for user 'administrator' 
    ///   with comment 'I made some changes'. Set the 'Release 1.1.1' label 
    ///   to this new version, even if the label already exists on an earlier 
    ///   version.
    ///   </para>
    ///   <code>
    /// &lt;sscmcheckin
    ///     serverconnect=&quot;localhost:4900&quot;
    ///     serverlogin=&quot;administrator:&quot;
    ///     file=&quot;Widget.java&quot;
    ///     branch=&quot;Widget 1.0&quot;
    ///     repository=&quot;Mainline/Widget&quot;
    ///     comment=&quot;I made some changes&quot;
    ///     label=&quot;Release 1.1.1&quot;
    ///     overwritelabel=&quot;true&quot;
    /// /&gt;
    ///   </code>
    /// </example>
    [TaskName("sscmcheckin")]
    public class SSCMCheckin : SSCMTask { 
        #region Private Instance Fields

        private string _branch;
        private string _repository;
        private string _file;
        private string _comment;
        private bool _skipAutomerge;
        private bool _getLocal = true;
        private bool _keepLocked;
        private string _label;
        private bool _overwriteLabel;
        private bool _quiet;
        private bool _recursive;
        private string _ttpDatabase;
        private string _ttpLogin;
        private string _ttpDefects;
        private bool _writable;
        private bool _forceUpdate;
        private bool _deleteLocal;

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
        /// repository specified by the repository option or the default 
        /// repository.
        /// </summary>
        [TaskAttribute("file", Required=false)]
        public string File { 
            get { return _file; }
            set { _file = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Comment for the check-in.
        /// </summary>
        [TaskAttribute("comment", Required=false)]
        public string Comment { 
            get { return _comment; }
            set { _comment = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Force check in without merge. Ignores code changes checked in after 
        /// the user's last checkout or merge. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("skipautomerge", Required=false)]
        public bool SkipAutomerge { 
            get { return _skipAutomerge; }
            set { _skipAutomerge = value; }
        }

        /// <summary>
        /// Get file after check in. The default is <see langword="true" />.
        /// </summary>
        [TaskAttribute("getlocal", Required=false)]
        public bool GetLocal { 
            get { return _getLocal; }
            set { _getLocal = value; }
        }

        /// <summary>
        /// Keep the lock after check in. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("keeplocked", Required=false)]
        public bool KeepLocked{ 
            get { return _keepLocked; }
            set { _keepLocked = value; }
        }

        /// <summary>
        /// A label for the check in code.
        /// </summary>
        [TaskAttribute("label", Required=false)]
        public string Label { 
            get { return _label; }
            set { _label = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Overwrite previous label on file. The default is 
        /// <see langword="false" />.
        /// </summary>
        [TaskAttribute("overwritelabel", Required=false)]
        public bool OverwriteLabel { 
            get { return _overwriteLabel; }
            set { _overwriteLabel = value; }
        }

        /// <summary>
        /// Do not list repository and local full path of the Surround 
        /// SCM server. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("quiet", Required=false)]
        public bool Quiet { 
            get { return _quiet; }
            set { _quiet = value; }
        }

        /// <summary>
        /// Recursively check in all files and sub-repositories.
        /// The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("recursive", Required=false)]
        public bool Recursive { 
            get { return _recursive; }
            set { _recursive = value; }
        }

        /// <summary>
        /// The TestTrack Pro database configuration name.
        /// </summary>
        [TaskAttribute("ttpdatabase", Required=false)]
        public string TtpDatabase { 
            get { return _ttpDatabase; }
            set { _ttpDatabase = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The TestTrack Pro username and password.
        /// </summary>
        [TaskAttribute("ttplogin", Required=false)]
        public string TtpLogin { 
            get { return _ttpLogin; }
            set { _ttpLogin = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The TestTrack Pro defect number(s) for attachment. Format is "#:#:#:#".
        /// </summary>
        [TaskAttribute("ttpdefects", Required=false)]
        public string TtpDefects { 
            get { return _ttpDefects; }
            set { _ttpDefects = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Make file writable after check in. The default is 
        /// <see langword="false" />.
        /// </summary>
        [TaskAttribute("writable", Required=false)]
        public bool Writable { 
            get { return _writable; }
            set { _writable = value; }
        }

        /// <summary>
        /// Update version even if no changes. The default is 
        /// <see langword="false" />.
        /// </summary>
        [TaskAttribute("forceupdate", Required=false)]
        public bool ForceUpdate { 
            get { return _forceUpdate; }
            set { _forceUpdate = value; }
        }

        /// <summary>
        /// Remove local file after check in. The default is 
        /// <see langword="false" />.
        /// </summary>
        [TaskAttribute("deletelocal", Required=false)]
        public bool DeleteLocal { 
            get { return _deleteLocal; }
            set { _deleteLocal = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of SSCMTask

        /// <summary>
        /// Writes the task-specific arguments to the specified 
        /// <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="argBuilder">The <see cref="StringBuilder" /> to write the task-specific arguments to.</param>
        protected override void WriteCommandLineArguments(StringBuilder argBuilder) {
            argBuilder.Append("checkin");

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

            // label
            if (Label != null) {
                argBuilder.Append(" -l\"");
                argBuilder.Append(Label);
                argBuilder.Append("\"");

                if (OverwriteLabel) {
                    argBuilder.Append(" -o");
                }
            }

            // TestTrack DB
            if (TtpDatabase != null) {
                argBuilder.Append(" -s\"");
                argBuilder.Append(TtpDatabase);
                argBuilder.Append("\"");
            }

            // TestTrack login
            if (TtpLogin != null) {
                argBuilder.Append(" -i\"");
                argBuilder.Append(TtpLogin);
                argBuilder.Append("\"");
            }

            // TestTrack defects
            if (TtpDefects != null) {
                argBuilder.Append(" -a");
                argBuilder.Append(TtpDefects);
            }

            // misc flags
            if (SkipAutomerge) {
                argBuilder.Append(" -f");
            }

            if (KeepLocked) {
                argBuilder.Append(" -k");
            }

            if (Quiet) {
                argBuilder.Append(" -q");
            }

            if (Recursive) {
                argBuilder.Append(" -r");
            }

            if (Writable) {
                argBuilder.Append(" -w");
            }

            if (ForceUpdate) {
                argBuilder.Append(" -u");
            }

            if (GetLocal) {
                argBuilder.Append(" -g");
            } else {
                argBuilder.Append(" -g-");
            }

            if (DeleteLocal) {
                argBuilder.Append(" -d");
            } else {
                argBuilder.Append(" -d-");
            }
        }

        #endregion Override implementation of SSCMTask
    }
}
