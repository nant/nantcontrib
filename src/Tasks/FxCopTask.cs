//
// NAntContrib
// Copyright (C) 2005
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
// Author: James Geurts (jgeurts@users.sourceforge.net)

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;
using NAnt.Core.Types;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Analyzes managed code assemblies and reports information about the 
    /// assemblies, such as possible design, localization, performance, and 
    /// security improvements.
    /// </summary>
    /// <remarks>
    ///   <note>
    ///   this task relies on fxcopcmd.exe being in your PATH environment variable.  
    ///   You can download the latest FxCop from <see href="http://www.gotdotnet.com/team/fxcop/" />.
    ///   </note>
    /// </remarks>
    /// <example>
    ///   <code>
    ///     <![CDATA[
    /// <fxcop directOutputToConsole="true" projectFile="${build.dir}\Sample.fxcop">
    ///     <targets>
    ///         <include name="${build.dir}\bin\*.dll" />
    ///     </targets>
    ///     <rules>
    ///         <include name="${build.dir}\rules\*.dll" />
    ///     </rules>
    /// </fxcop>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("fxcop")]
    public class FxCopTask : ExternalProgramBase {
        private const string defaultExeFilename = "fxcopcmd.exe";

        private bool _applyOutXsl;
        private bool _directOutputToConsole;
        private DirSet _dependencyDirectories = new DirSet();
        private FileSet _targetAssemblies = new FileSet();
        private string _consoleXslFilename;
        private FileSet _importFiles = new FileSet();
        private string _analysisReportFilename;
        private string _outputXslFilename;
        private string _platformDirectory;
        private string _projectFile;
        private FileSet _ruleLibraries = new FileSet();
        private bool _includeSummaryReport;
        private string _typeList;
        private bool _saveResults;
        private bool _failOnAnalysisError;

        private DirectoryInfo _baseDirectory;
        private StringBuilder _programArguments = new StringBuilder();

        #region Properties

        /// <summary>
        /// Applies the XSL transformation specified in /outXsl to the analysis report before saving the file.
        /// </summary>
        [TaskAttribute("applyOutXsl", Required=false)]
        public bool ApplyOutXsl {
            get { return _applyOutXsl; }
            set { _applyOutXsl = value; }
        }

        /// <summary>
        /// Directs analysis output to the console or to the Output window in Visual Studio .NET. By default, the XSL file FxCopConsoleOutput.xsl is applied to the output before it is displayed.
        /// </summary>
        [TaskAttribute("directOutputToConsole", Required=false)]
        public bool DirectOutputToConsole {
            get { return _directOutputToConsole; }
            set { _directOutputToConsole = value; }
        }

        /// <summary>
        /// Specifies the XSL or XSLT file that contains a transformation to be applied to the analysis output before it is displayed in the console.
        /// </summary>
        [TaskAttribute("consoleXsl", Required=false)]
        public string ConsoleXslFilename {
            get { return _consoleXslFilename; }
            set { _consoleXslFilename = value; }
        }

        /// <summary>
        /// Specifies additional directories to search for assembly dependencies. FxCopCmd always searches the target assembly directory and the current working directory.
        /// </summary>
        [BuildElement("dependencyDirectories")]
        public DirSet DependencyDirectories {
            get { return _dependencyDirectories; }
            set { _dependencyDirectories = value; }
        }

        /// <summary>
        /// Specifies the target assembly to analyze.
        /// </summary>
        [BuildElement("targets")]
        public FileSet TargetAssemblies {
            get { return _targetAssemblies; }
            set { _targetAssemblies = value; }
        }

        /// <summary>
        /// Specifies the name of an analysis report or project file to import. Any messages in the imported file that are marked as excluded are not included in the analysis results.
        /// </summary>
        [BuildElement("imports")]
        public FileSet ImportFiles {
            get { return _importFiles; }
            set { _importFiles = value; }
        }

        /// <summary>
        /// Specifies the file name for the analysis report.
        /// </summary>
        [TaskAttribute("analysisReportFilename", Required=false)]
        public string AnalysisReportFilename {
            get { return _analysisReportFilename; }
            set { _analysisReportFilename = value; }
        }

        /// <summary>
        /// Specifies the XSL or XSLT file that is referenced by the xml-stylesheet processing instruction in the analysis report.
        /// </summary>
        [TaskAttribute("outputXslFilename", Required=false)]
        public string OutputXslFilename {
            get { return _outputXslFilename; }
            set { _outputXslFilename = value; }
        }

        /// <summary>
        /// Specifies the location of the version of Mscorlib.dll that was used when building the target assemblies if this version is not installed on the computer running FxCopCmd.
        /// </summary>
        [TaskAttribute("platformDirectory", Required=false)]
        public string PlatformDirectory {
            get { return _platformDirectory; }
            set { _platformDirectory = value; }
        }

        /// <summary>
        /// Specifies the filename of FxCop project file.
        /// </summary>
        [TaskAttribute("projectFile", Required=false)]
        public string ProjectFile {
            get { return _projectFile; }
            set { _projectFile = value; }
        }

        /// <summary>
        /// Specifies the filename(s) of FxCop project file(s).
        /// </summary>
        [BuildElement("rules")]
        public FileSet RuleLibraries {
            get { return _ruleLibraries; }
            set { _ruleLibraries = value; }
        }

        /// <summary>
        /// Includes a summary report with the informational messages returned by FxCopCmd.
        /// </summary>
        [TaskAttribute("includeSummaryReport", Required=false)]
        public bool IncludeSummaryReport {
            get { return _includeSummaryReport; }
            set { _includeSummaryReport = value; }
        }


        /// <summary>
        /// Comma-separated list of type names to analyze.  This option disables analysis of assemblies, namespaces, and resources; only the specified types and their members are included in the analysis.  
        /// Use the wildcard character '*' at the end of the name to select multiple types.
        /// </summary>
        [TaskAttribute("typeList", Required=false)]
        public string TypeList {
            get { return _typeList; }
            set { _typeList = value; }
        }

        /// <summary>
        /// Saves the results of the analysis in the project file.
        /// </summary>
        [TaskAttribute("saveResults", Required=false)]
        public bool SaveResults {
            get { return _saveResults; }
            set { _saveResults = value; }
        }

        /// <summary>
        /// Determines if the task should fail when analysis errors occur
        /// </summary>
        [TaskAttribute("failOnAnalysisError", Required=false)]
        public bool FailOnAnalysisError {
            get { return _failOnAnalysisError; }
            set { _failOnAnalysisError = value; }
        }

        #endregion

        /// <summary>
        /// Creates a new <see cref="FxCopTask"/> instance.
        /// </summary>
        public FxCopTask() {
            ExeName = defaultExeFilename;
        }

        /// <summary>
        /// The directory in which the command will be executed.
        /// </summary>
        /// <value>
        /// The directory in which the command will be executed. The default 
        /// is the project's base directory.
        /// </value>
        /// <remarks>
        /// <para>
        /// It will be evaluated relative to the project's
        /// base directory if it is relative.
        /// </para>
        /// </remarks>
        [TaskAttribute("basedir", Required=false)]
        public override DirectoryInfo BaseDirectory {
            get {
                if (_baseDirectory == null) {
                    return base.BaseDirectory;
                }
                return _baseDirectory;
            }
            set { _baseDirectory = value; }
        }

        /// <summary>
        /// Gets the program arguments.
        /// </summary>
        public override string ProgramArguments {
            get { return _programArguments.ToString(); }
        }

        /// <summary>
        /// Performs logic before the external process is started
        /// </summary>
        /// <param name="process">Process.</param>
        protected override void PrepareProcess(Process process) {
            BuildArguments();

            Log(Level.Verbose, "Working directory: {0}",
                BaseDirectory);
            Log(Level.Verbose, "Arguments: {0}", ProgramArguments);

            base.PrepareProcess(process);
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        protected override void ExecuteTask() 
        {
            base.ExecuteTask();

            if (FailOnAnalysisError) {
                if (File.Exists(AnalysisReportFilename)) {
                    throw new BuildException(
                        "Analysis error messages found.", 
                        Location);
                }
            }
        }

        /// <summary>
        /// Builds the arguments to pass to the exe.
        /// </summary>
        private void BuildArguments() {
            if (ApplyOutXsl) {
                _programArguments.Append("/aXsl ");
            }

            if (DirectOutputToConsole) {
                _programArguments.Append("/c ");
            }

            if (!String.IsNullOrEmpty(ConsoleXslFilename)) {
                _programArguments.AppendFormat("/cXsl:\"{0}\" ", ConsoleXslFilename);
            }

            foreach (string directoryName in DependencyDirectories.DirectoryNames) {
                _programArguments.AppendFormat("/d:\"{0}\" ", directoryName);
            }

            foreach (string filename in TargetAssemblies.FileNames) {
                _programArguments.AppendFormat("/f:\"{0}\" ", filename);
            }

            foreach (string filename in ImportFiles.FileNames) {
                _programArguments.AppendFormat("/i:\"{0}\" ", filename);
            }

            if (!String.IsNullOrEmpty(AnalysisReportFilename) || FailOnAnalysisError) {
                if (String.IsNullOrEmpty(AnalysisReportFilename)) {
                    AnalysisReportFilename = Path.GetTempFileName();
                }
                _programArguments.AppendFormat("/o:\"{0}\" ", AnalysisReportFilename);
            }

            if (!String.IsNullOrEmpty(OutputXslFilename)) {
                _programArguments.AppendFormat("/oXsl:\"{0}\" ", OutputXslFilename);
            }

            if (!String.IsNullOrEmpty(PlatformDirectory)) {
                _programArguments.AppendFormat("/plat:\"{0}\" ", PlatformDirectory);
            }

            if (!String.IsNullOrEmpty(ProjectFile)) {
                _programArguments.AppendFormat("/p:\"{0}\" ", ProjectFile);
            }

            foreach (string ruleFile in RuleLibraries.FileNames) {
                _programArguments.AppendFormat("/r:\"{0}\" ", ruleFile);
            }

            if (IncludeSummaryReport) {
                _programArguments.Append("/s ");
            }

            if (!String.IsNullOrEmpty(TypeList)) {
                _programArguments.AppendFormat("/t:{0} ", TypeList);
            }

            if (SaveResults) {
                _programArguments.Append("/u ");
            }

            if (this.Verbose) {
                _programArguments.Append("/v ");
            }
        }
    }
}