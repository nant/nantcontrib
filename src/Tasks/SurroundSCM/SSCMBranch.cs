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
    /// Creates new branches for <see href="http://www.seapine.com/surroundscm.html">Surround SCM</see>.
    /// </summary>
    /// <example>
    ///   <para>
    ///   Create a new Baseline branch 'Widget 1.0' from branch 'Mainline', 
    ///   repository 'Mainline/Widget' with the given comments. All files 
    ///   are branched at the tip version.
    ///   </para>
    ///   <code>
    /// &lt;sscmbranch
    ///     serverconnect=&quot;localhost:4900&quot;
    ///     serverlogin=&quot;administrator:&quot;
    ///     branch=&quot;Widget 1.0&quot;
    ///     repository=&quot;Mainline/Widget&quot;
    ///     parent=&quot;Mainline&quot;
    ///     comment=&quot;Branch for continuing Widget 1.0 development&quot;
    ///     type=&quot;baseline&quot;
    /// /&gt;
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Create a new Workspace branch 'MyWidgetDevelopment' from branch 
    ///   'Widget 1.0', repository 'Mainline/Widget'. All files are branched 
    ///   at the tip version.
    ///   </para>
    ///   <code>
    /// &lt;sscmbranch
    ///     serverconnect=&quot;localhost:4900&quot;
    ///     serverlogin=&quot;administrator:&quot;
    ///     branch=&quot;MyWidgetDevelopment&quot;
    ///     repository=&quot;Mainline/Widget&quot;
    ///     parent=&quot;Widget 1.0&quot;
    /// /&gt;
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Create a new Snapshot branch 'Build as of 12-1-03' from branch 
    ///   'Widget 1.0', repository 'Mainline/Widget' with the given comments. 
    ///   All files are branched at their version as of 12-01-03.
    ///   </para>
    ///   <code>
    /// &lt;sscmbranch
    ///     serverconnect=&quot;localhost:4900&quot;
    ///     serverlogin=&quot;administrator:&quot;
    ///     name=&quot;Build as of 12-1-03&quot;
    ///     repository=&quot;Mainline/Widget&quot;
    ///     branch=&quot;Widget 1.0&quot;
    ///     comment=&quot;Snapshot of source as it was on December 1st, 2003&quot;
    ///     timestamp=&quot;2003120300:00:00&quot;
    ///     type=&quot;snapshot&quot;
    /// /&gt;
    ///   </code>
    /// </example>
    [TaskName("sscmbranch")]
    public class SSCMBranch : SSCMTask {
        #region Private Instance Fields

        private string _branch;
        private string _repository;
        private string _parentBranch;
        private string _comment;
        private string _byLabel;
        private string _byTimestamp;
        private bool _includeRemoved = true;
        private string _type;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The name of the branch you want to create.
        /// </summary>
        [TaskAttribute("branch", Required=true)]
        public string Branch { 
            get { return _branch; }
            set { _branch = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The full repository path.
        /// </summary>
        [TaskAttribute("repository", Required=true)]
        public string Repository { 
            get { return _repository; }
            set { _repository = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The parent branch you want to create the new, child branch from. 
        /// If not specified, the mainline branch is used.
        /// </summary>
        [TaskAttribute("parent", Required=false)]
        public string ParentBranch { 
            get { return _parentBranch; }
            set { _parentBranch = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Specifies a comment for the branch operation.
        /// </summary>
        [TaskAttribute("comment", Required=false)]
        public string Comment { 
            get { return _comment; }
            set { _comment = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Specifies which parent branch file versions are copied into the 
        /// child branch.
        /// </summary>
        [TaskAttribute("bylabel", Required=false)]
        public string ByLabel { 
            get { return _byLabel; }
            set { _byLabel = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Specifies which parent branch file versions are copied into the 
        /// child branch. Format is yyyymmddhh:mm:ss. If <see cref="ByLabel" />
        /// attribute is specified, this parameter is ignored.
        /// </summary>
        [TaskAttribute("bytimestamp", Required=false)]
        public string ByTimestamp { 
            get { return _byTimestamp; }
            set { _byTimestamp = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Include removed files when creating a branch with the 
        /// <see cref="ByLabel" /> or <see cref="ByTimestamp" /> option. 
        /// The default is <see langword="true" />.
        /// </summary>
        [TaskAttribute("includeremoved", Required=false)]
        public bool IncludeRemoved { 
            get { return _includeRemoved; }
            set { _includeRemoved = value; }
        }

        /// <summary>
        /// Specifies the type of branch you want to create. Possible values are
        /// <c>workspace</c>, <c>baseline</c>, or <c>snapshot</c>. The default is 
        /// <c>workspace</c>.
        /// </summary>
        [TaskAttribute("type", Required=false)]
        public string Type { 
            get { return _type; }
            set { _type = StringUtils.ConvertEmptyToNull(value); }
        }

        #endregion Public Instance Properties

        #region Override implementation of SSCMTask

        /// <summary>
        /// Writes the task-specific arguments to the specified 
        /// <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="argBuilder">The <see cref="StringBuilder" /> to write the task-specific arguments to.</param>
        protected override void WriteCommandLineArguments(StringBuilder argBuilder) {
            argBuilder.Append("mkbranch");

            // branch
            argBuilder.Append(" \"");
            argBuilder.Append(Branch);
            argBuilder.Append("\"");

            // repository
            argBuilder.Append(" \"");
            argBuilder.Append(Repository);
            argBuilder.Append("\"");

            // parent branch
            if (ParentBranch != null) {
                argBuilder.Append(" -b\"");
                argBuilder.Append(ParentBranch);
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

            // branch method
            if (ByLabel != null) {
                argBuilder.Append(" -l\"");
                argBuilder.Append(ByLabel);
                argBuilder.Append("\"");

                if (!IncludeRemoved) {
                    argBuilder.Append(" -i-");
                }
            } else if (ByTimestamp != null) {
                argBuilder.Append(" -t\"");
                argBuilder.Append(ByTimestamp);
                argBuilder.Append("\"");

                if (IncludeRemoved) {
                    argBuilder.Append(" -i");
                }
            }

            // type
            if (Type != null) {
                argBuilder.Append(" -s");
                argBuilder.Append(Type);
            }
        }

        #endregion Override implementation of SSCMTask
    }
}
