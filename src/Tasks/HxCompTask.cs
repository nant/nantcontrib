//
// NAntContrib
// Copyright (C) 2001-2003 Gerry Shaw
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
// Jayme C. Edwards (jedwards@wi.rr.com)

using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;

using Microsoft.Win32;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Compiles a Microsoft HTML Help 2.0 Project.
    /// </summary>
    /// <example>
    ///   <para>Compile a help file.</para>
    ///   <code>
    ///     <![CDATA[
    /// <hxcomp contents="MyContents.HxC" output="MyHelpFile.HxS" projectroot="HelpSourceFolder" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("hxcomp")]
    public class HxCompTask : ExternalProgramBase {
        #region Private Instance Fields

        private string _args;
        private string _contents;
        private string _logfile;
        private string _unicodelogfile;
        private string _projectroot;
        private string _outputFile;
        private bool _noinformation;
        private bool _noerrors;
        private bool _nowarnings;
        private string _uncompilefile;
        private string _uncompileoutputdir;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The name of the contents (.HxC) file.
        /// </summary>
        [TaskAttribute("contents")]
        public string Contents {
            get { return _contents; }
            set { _contents = value; }
        }

        /// <summary>
        /// ANSI/DBCS log filename.
        /// </summary>
        [TaskAttribute("logfile")]
        public string LogFile {
            get { return _logfile; }
            set { _logfile = value; }
        }

        /// <summary>
        /// Unicode log filename.
        /// </summary>
        [TaskAttribute("unicodelogfile")]
        public string UnicodeLogFile {
            get { return _unicodelogfile; }
            set { _unicodelogfile = value; }
        }

        /// <summary>
        /// Root directory containing Help 2.0 project files.
        /// </summary>
        [TaskAttribute("projectroot")]
        public string ProjectRoot {
            get { return _projectroot; }
            set { _projectroot = value; }
        }

        /// <summary>
        /// Output (.HxS) filename.
        /// </summary>
        [TaskAttribute("output")] 
        public string OutputFile {
            get { return _outputFile; }
            set { _outputFile = value; }
        }

        /// <summary>
        /// Generate no informational messages.
        /// </summary>
        [TaskAttribute("noinformation")]
        [BooleanValidator()]
        public bool NoInformation {
            get { return _noinformation; }
            set { _noinformation = value; }
        }
        
        /// <summary>
        /// Generate no error messages.
        /// </summary>
        [TaskAttribute("noerrors")]
        [BooleanValidator()]
        public bool NoErrors {
            get { return _noerrors; }
            set { _noerrors = value; }
        }

        /// <summary>
        /// Generate no warning messages.
        /// </summary>
        [TaskAttribute("nowarnings")]
        [BooleanValidator()]
        public bool NoWarnings {
            get { return _nowarnings; }
            set { _nowarnings = value; }
        }
        
        /// <summary>
        /// File to be decompiled.
        /// </summary>
        [TaskAttribute("uncompilefile")] 
        public string UncompileFile {
            get { return _uncompilefile; }
            set { _uncompilefile = value; }
        }

        /// <summary>
        /// Directory to place decompiled files into.
        /// </summary>
        [TaskAttribute("uncompileoutputdir")] 
        public string UncompileOutputDir {
            get { return _uncompileoutputdir; }
            set { _uncompileoutputdir = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of ExternalProgramBase

        public override string ProgramFileName {
            get {
                RegistryKey helpPackageKey = Registry.LocalMachine.OpenSubKey(
                    @"Software\Microsoft\VisualStudio\7.0\" + 
                    @"Packages\{7D57F111-B9F3-11d2-8EE0-00C04F5E0C38}",
                    false);

                if (helpPackageKey != null) {
                    string helpPackageVal = (string)helpPackageKey.GetValue("InprocServer32", null);
                    if (helpPackageVal != null) {
                        string helpPackageDir = Path.GetDirectoryName(helpPackageVal);
                        if (helpPackageDir != null) {
                            if (Directory.Exists(helpPackageDir)) {
                                DirectoryInfo parentDir = Directory.GetParent(helpPackageDir);
                                if (parentDir != null) {
                                    helpPackageKey.Close();
                                    return Path.Combine(parentDir.FullName, "hxcomp.exe");
                                }
                            }
                        }
                    }
                    helpPackageKey.Close();
                }

                throw new BuildException(
                    "Unable to locate installation directory of " + 
                    "Microsoft Help 2.0 SDK in the registry.", Location);
            }
        }

        /// <summary>
        /// Arguments of program to execute
        /// </summary>
        public override string ProgramArguments {
            get {
                return _args;
            }
        }

        protected override void Initialize() {
            base.Initialize();

            if (OutputFile == null && UncompileFile == null) {
                throw new BuildException(
                    "Either the \"uncompilefile\" or \"output\" attribute"
                    + " should be specified.", Location);
            }
        }

        protected override void ExecuteTask() {
            // If the user wants to see the actual command the -verbose flag
            // will cause ExternalProgramBase to display the actual call.
            if (OutputFile != null ) {
                Log(Level.Info, "Compiling HTML Help 2.0 File '{0}'.", OutputFile);
            } else if (UncompileFile != null) {
                Log(Level.Info, "Decompiling HTML Help 2.0 File '{0}'.", UncompileFile);
            }

            try {
                StringBuilder arguments = new StringBuilder();
                   
                if (NoInformation) {
                    arguments.Append("-i ");
                }
                if (NoErrors) {
                    arguments.Append("-e ");
                }
                if (NoWarnings) {
                    arguments.Append("-w ");
                }
                if (!Verbose) {
                    arguments.Append("-q ");
                }
                if (Contents != null) {
                    arguments.Append("-p ");
                    arguments.Append('\"' + Contents + '\"');
                    arguments.Append(" ");
                }
                if (LogFile != null) {
                    arguments.Append("-l ");
                    arguments.Append('\"' + LogFile + '\"');
                    arguments.Append(" ");
                }
                if (UnicodeLogFile != null) {
                    arguments.Append("-n ");
                    arguments.Append('\"' + UnicodeLogFile + '\"');
                    arguments.Append(" ");
                }
                if (Output != null) {
                    arguments.Append("-o ");
                    arguments.Append('\"' + OutputFile + '\"');
                    arguments.Append(" ");
                }
                if (ProjectRoot != null) {
                    arguments.Append("-r ");
                    arguments.Append('\"' + ProjectRoot + '\"');
                    arguments.Append(" ");
                }
                if (UncompileFile != null) {
                    arguments.Append("-u ");
                    arguments.Append('\"' + UncompileFile + '\"');
                    arguments.Append(" ");
                }
                if (UncompileOutputDir != null) {
                    arguments.Append("-d ");
                    arguments.Append('\"' + UncompileOutputDir + '\"');
                    arguments.Append(" ");
                }

                _args = arguments.ToString();

                base.ExecuteTask();
            } catch (Exception ex) {
                throw new BuildException(
                    "Microsoft HTML Help 2.0 Project could not be compiled.", 
                    Location, ex);
            }
        }

        #endregion Override implementation of ExternalProgramBase
    }
}
