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
    /// Creates file or repository labels for a <see href="http://www.seapine.com/surroundscm.html">Surround SCM</see>
    /// repository.
    /// </summary>
    /// <remarks>
    /// Labels provide a way to mark a specific version of a file or repository. 
    /// You can create labels for single files, multiple files, or all files in 
    /// a repository. When you create a label, a new entry is created in the history.
    /// The file, and the version number, do not change. Existing 'Release 1.0.1' 
    /// labels on a file will be moved to the tip version of the file.
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Label all files under the 'Mainline/Widget' repository recursively in 
    ///   the 'Widget 1.0' branch with 'Release 1.0.1' and the given comment.
    ///   </para>
    ///   <code>
    /// &lt;sscmlabel
    ///     serverconnect=&quot;localhost:4900&quot;
    ///     serverlogin=&quot;administrator:&quot;
    ///     branch=&quot;Widget 1.0&quot;
    ///     repository=&quot;Mainline/Widget&quot;
    ///     file=&quot;readme.txt&quot;
    ///     recursive=&quot;true&quot;
    ///     label=&quot;Release 1.0.1&quot;
    ///     overwritelabel=&quot;false&quot;
    ///     comment=&quot;This labels the final build for the release of Widget 1.0.1.&quot;
    /// /&gt;
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Label all files under the 'Mainline/Widget' repository recursively in 
    ///   the 'Widget 1.0' branch with 'Release 1.0.1' and no comments.
    ///   </para>
    ///   <code>
    /// &lt;sscmlabel
    ///     serverconnect=&quot;localhost:4900&quot;
    ///     serverlogin=&quot;administrator:&quot;
    ///     branch=&quot;Widget 1.0&quot;
    ///     repository=&quot;Mainline/Widget&quot;
    ///     file=&quot;readme.txt&quot;
    ///     recursive=&quot;true&quot;
    ///     label=&quot;Release 1.0.1&quot;
    /// /&gt;
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Label version 4 of the file 'Mainline/Widget/Widget.java' in the 
    ///   'Widget 1.0' branch with 'Release 1.0.1' and the given comment. An 
    ///   existing 'Release 1.0.1' label on this file to be moved to version 4 
    ///   of the file.
    ///   </para>
    ///   <code>
    /// &lt;sscmlabel
    ///     serverconnect=&quot;localhost:4900&quot;
    ///     serverlogin=&quot;administrator:&quot;
    ///     branch=&quot;Widget 1.0&quot;
    ///     repository=&quot;Mainline/Widget&quot;
    ///     file=&quot;readme.txt&quot;
    ///     label=&quot;Release 1.0.1&quot;
    ///     overwritelabel=&quot; true&quot; 
    ///     comment=&quot; This labels the final build for the release of Widget 1.0.1.&quot; 
    ///     version=&quot; 4&quot; 
    /// /&gt;
    ///   </code>
    /// </example>
    [TaskName("sscmlabel")]
    public class SSCMLabel : SSCMTask {
        #region Private Instance Fields

        private string _branch;
        private string _repository;
        private string _file;
        private string _label;
        private bool _recursive;
        private bool _overwrite;
        private string _comment;
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
        /// The new label to create.
        /// </summary>
        [TaskAttribute("label", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Label {
            get { return _label; }
            set { _label = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Recursively label all files. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("recursive", Required=false)]
        public bool Recursive {
            get { return _recursive; }
            set { _recursive = value; }
        }

        /// <summary>
        /// Overwrite the existing label. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("overwrite", Required=false)]
        public bool Overwrite {
            get { return _overwrite; }
            set { _overwrite = value; }
        }

        /// <summary>
        /// Comment for the label.
        /// </summary>
        [TaskAttribute("comment", Required=false)]
        public string Comment {
            get { return _comment; }
            set { _comment = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The file version to label. Ignored if a filename is not specified in 
        /// the <see cref="File" /> attribute.
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
            argBuilder.Append("label");

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

            // label
            if (Label != null) {
                argBuilder.Append(" -l\"");
                argBuilder.Append(Label);
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
            if (Version != null) {
                argBuilder.Append(" -v");
                argBuilder.Append(Version);
            }

            // misc flags
            if (Overwrite) {
                argBuilder.Append(" -o");
            }

            if (Recursive) {
                argBuilder.Append(" -r");
            }
        }

        #endregion Override implementation of SSCMTask
    }
}
