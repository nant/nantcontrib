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
    /// Updates a ClearCase view.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>cleartool update</c> command to update a ClearCase view.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Performs a ClearCase update on the snapshot view directory <c>c:/views/viewdir</c>. 
    ///   A graphical dialog will be displayed.  
    ///   The output will be logged to <c>log.log</c> and it will overwrite any hijacked files. 
    ///   The modified time will be set to the current time.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <ccupdate viewpath="c:/views/viewdir"
    ///         graphical="false"
    ///         log="log.log"
    ///         overwrite="true"
    ///         currenttime="true"
    ///         rename="false"/>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("ccupdate")]
    public class ClearCaseUpdate : ClearCaseBase {
        #region Private Instance Fields

        private FileInfo _viewPath;
        private bool _graphical;
        private FileInfo _logFile;
        private bool _overwrite;
        private bool _rename;
        private bool _currentTime;
        private bool _preserveTime;

        #endregion Private Instance Fields

        #region Public Properties

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
        /// Displays a graphical dialog during the update.
        /// The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("graphical")]
        [BooleanValidator()]
        public bool Graphical {
            get { return _graphical; }
            set { _graphical = value; }
        }

        /// <summary>
        /// Specifies a log file for ClearCase to write to.
        /// </summary>
        [TaskAttribute("log")]
        public FileInfo LogFile {
            get { return _logFile; }
            set { _logFile = value; }
        }

        /// <summary>
        /// If <see langword="true" />, hijacked files will be overwritten.
        /// The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("overwrite")]
        [BooleanValidator()]
        public bool Overwrite {
            get { return _overwrite; }
            set { _overwrite = value; }
        }

        /// <summary>
        /// If <see langword="true" />, hijacked files will be renamed with a .keep extension.
        /// The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("rename")]
        [BooleanValidator()]
        public bool Rename {
            get { return _rename; }
            set { _rename = value; }
        }

        /// <summary>
        /// Specifies that modification time should be written as the current time. 
        /// Only one of <see cref="CurrentTime" /> or <see cref="PreserveTime" />
        /// can be specified. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("currenttime")]
        [BooleanValidator()]
        public bool CurrentTime {
            get { return _currentTime; }
            set { _currentTime = value; }
        }

        /// <summary>
        /// Specifies that modification time should preserved from the VOB time. 
        /// Only one of <see cref="CurrentTime" /> or <see cref="PreserveTime" />
        /// can be specified. The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("preservetime")]
        [BooleanValidator()]
        public bool PreserveTime {
            get { return _preserveTime; }
            set { _preserveTime = value; }
        }

        #endregion Public Properties

        /// <summary>
        /// This is an override used by the base class to get command specific args.
        /// </summary>
        protected override string CommandSpecificArguments {
            get {
                StringBuilder arguments = new StringBuilder();
                arguments.Append("update ");

                if (Graphical) {
                    arguments.Append("-graphical ");
                } else {
                    arguments.Append("-print -force ");

                    if (Overwrite) {
                        arguments.Append("-overwrite ");
                    } else if (Rename) {
                        arguments.Append("-rename ");
                    } else {
                        arguments.Append("-noverwrite ");
                    }

                    if (CurrentTime) {
                        arguments.Append("-ctime ");
                    } else if (PreserveTime) {
                        arguments.Append("-ptime ");
                    }

                    if (LogFile != null) {
                        arguments.AppendFormat("-log \"{0}\" ", LogFile.FullName);
                    }
                }

                if (ViewPath != null) {
                    arguments.AppendFormat("\"{0}\"", ViewPath.FullName);
                }

                return arguments.ToString();
            }
        }
    }
}
