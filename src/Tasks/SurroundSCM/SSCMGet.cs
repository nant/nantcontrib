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

using System.IO;
using System.Text;

using NAnt.Core.Attributes;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks.SurroundSCM {
    /// <summary>
    /// Gets files from a <see href="http://www.seapine.com/surroundscm.html">Surround SCM</see>
    /// repository.
    /// </summary>
    /// <remarks>
    /// You can get a single file, multiple files, or a repository. A read-only 
    /// copy of the file is created in the specified directory.
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Get all files and repositories from repository 'Mainline/Widget' 
    ///   recursively from the 'Widget 1.0' branch to the working directory 
    ///   setup for user 'administrator'. This call forces the file retrieval 
    ///   from the server even if the local file is current and overwrites any 
    ///   local files that are writable with the server copy.
    ///   </para>
    ///   <code>
    /// &lt;sscmget
    ///     serverconnect=&quot;localhost:4900&quot;
    ///     serverlogin=&quot;administrator:&quot;
    ///     file=&quot;/&quot;
    ///     branch=&quot;Widget 1.0&quot;
    ///     repository=&quot;Mainline/Widget&quot;
    ///     recursive=&quot;true&quot;
    ///     force=&quot;true&quot;
    ///     overwrite=&quot;true&quot;
    /// /&gt;
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Get version 1 of the file 'Mainline/Widget/Widget.java' from the 
    ///   'Widget 1.0' branch to the working directory setup for user 'administrator'.
    ///   Writable local files are not overwritten, even if they are out of date.
    ///   </para>
    ///   <code>
    /// &lt;sscmget
    ///     serverconnect=&quot;localhost:4900&quot;
    ///     serverlogin=&quot;administrator:&quot;
    ///     quiet=&quot;true&quot;
    ///     file=&quot;Widget.java&quot;
    ///     branch=&quot;Widget 1.0&quot;
    ///     repository=&quot;Mainline/Widget&quot;
    ///     overwrite=&quot;false&quot;
    ///     writable=&quot;true&quot;
    ///     version=&quot;1&quot;
    /// /&gt;
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Get all files and repositories labeled with 'Release 1.0.0' (even those 
    ///   removed from Surround) from repository 'Mainline/Widget' recursively 
    ///   from the 'Widget 1.0' branch to the '${build}/src' directory. Writable 
    ///   local files are overwritten with the server copy.
    ///   </para>
    ///   <code>
    /// &lt;sscmget
    ///     serverconnect=&quot;localhost:4900&quot;
    ///     serverlogin=&quot;administrator:&quot;
    ///     quiet=&quot;true&quot;
    ///     file=&quot;/&quot;
    ///     branch=&quot;Widget 1.0&quot;
    ///     repository=&quot;Mainline/Widget&quot;
    ///     recursive=&quot;true&quot;
    ///     label=&quot;Release 1.0.1&quot;
    ///     destdir=&quot;${build}/src&quot;
    ///     overwrite=&quot;true&quot;
    /// /&gt;
    ///   </code>
    /// </example>
    [TaskName("sscmget")]
    public class SSCMGet : SSCMTask { 
        #region Private Instance Fields

        private string _branch;
        private string _repository;
        private string _file;
        private DirectoryInfo _destinationDirectory;
        private bool _writable;
        private bool _force;
        private string _byLabel;
        private string _byTimestamp;
        private bool _includeRemoved = true;
        private bool _quiet;
        private bool _recursive;
        private bool _overwrite;
        private string _timestamp;
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
        /// File or repository name. Can be / or empty, which means the repository 
        /// specified by the <see cref="Repository" /> attribute or the default 
        /// repository.
        /// </summary>
        [TaskAttribute("file", Required=false)]
        public string File { 
            get { return _file; }
            set { _file = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The local directory you want to get the files to. If 
        /// <see cref="File" /> is a repository, a subrepository with the same 
        /// name as the repository is created and files are copied to it. If 
        /// <see cref="File" /> is specified as / or not set, files are copied to 
        /// the local directory. If not specified, files are copied to the 
        /// working directory.
        /// </summary>
        [TaskAttribute("destdir", Required=false)]
        public DirectoryInfo DestinationDirectory { 
            get { return _destinationDirectory; }
            set { _destinationDirectory = value; }
        }

        /// <summary>
        /// Make local file editable or writable. The default is 
        /// <see langword="false" />.
        /// </summary>
        [TaskAttribute("writable", Required=false)]
        public bool Writable { 
            get { return _writable; }
            set { _writable = value; }
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
        /// Label to search for when getting a file. If a file version is 
        /// specified, this parameter is ignored.
        /// </summary>
        [TaskAttribute("bylabel", Required=false)]
        public string ByLabel { 
            get { return _byLabel; }
            set { _byLabel = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Timestamp to use when getting files. Format is yyyymmddhh:mm:ss. 
        /// If <see cref="ByLabel" /> is specified, this parameter is ignored. 
        /// Requires Surround SCM 3.0 or later.
        /// </summary>
        [TaskAttribute("bytimestamp", Required=false)]
        public string ByTimestamp { 
            get { return _byTimestamp; }
            set { _byTimestamp = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Include removed files when getting files by label or timestamp. 
        /// The default is <see langword="true" />. Ignored if a label or 
        /// timestamp is not specified.
        /// </summary>
        [TaskAttribute("includeremoved", Required=false)]
        public bool IncludeRemoved { 
            get { return _includeRemoved; }
            set { _includeRemoved = value; }
        }

        /// <summary>
        /// Do not list repository and local full path of files. The default is 
        /// <see langword="false" />.
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
        /// The file version to get. Ignored if a filename is not specified in 
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
            argBuilder.Append("get");

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

            // destination dir
            if (DestinationDirectory != null) {
                argBuilder.Append(" -d\"");
                argBuilder.Append(DestinationDirectory.FullName);
                argBuilder.Append( "\"");
            }

            // Cannot specify version and label and timestamp at the same time. If user gives a
            // version, we'll assume that since it's more specific than a label.

            // version
            if (File != null && Version != null) {
                argBuilder.Append(" -v");
                argBuilder.Append(Version);
                argBuilder.Append("");
            } if (ByLabel != null) { // bylabel
                    argBuilder.Append(" -l\"");
                    argBuilder.Append(ByLabel);
                    argBuilder.Append("\"");

                    if (IncludeRemoved) {
                        argBuilder.Append(" -i");
                    }
                } else if (ByTimestamp != null) { // bytimestamp
                    argBuilder.Append(" -s\"");
                    argBuilder.Append(ByTimestamp);
                    argBuilder.Append("\"");

                    if (IncludeRemoved) {
                        argBuilder.Append(" -i");
                    }
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
                argBuilder.Append( " -q");
            }

            if (Writable) {
                argBuilder.Append( " -e");
            }

            if (Force) {
                argBuilder.Append( " -f");
            }

            if (Recursive) {
                argBuilder.Append( " -r");
            }
        }

        #endregion Override implementation of SSCMTask
    }
}
