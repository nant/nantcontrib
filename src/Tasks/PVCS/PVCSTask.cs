#region GNU General Public License

// NAntContrib
// Copyright (C) 2004 Kent Boogaart
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
// Kent Boogaart (kentcb@internode.on.net)

#endregion

using System;
using System.Diagnostics;
using System.Text;

using NAnt.Core;
using NAnt.Core.Tasks;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.PVCS {
    /// <summary>
    /// Base class functionality for all PVCS tasks.
    /// </summary>
    public abstract class PVCSTask : ExternalProgramBase {
        #region Fields

        /// <see cref="PVCSBin"/>
        private string _pvcsBin;

        /// <summary>
        /// This is the PCLI process that is run by this task.
        /// </summary>
        private Process _process;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the location of the PVCS binary command-line tools.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Generally, the PVCS command-line tools will be available on the current path. However, if this is not
        /// the case then this property allows an exact location to be specified. If this property is not set, the
        /// task will assume that the PVCS binaries are available on the current path.
        /// </para>
        /// </remarks>
        [TaskAttribute("pvcsbin", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string PVCSBin {
            get {
                return _pvcsBin;
            }
            set {
                _pvcsBin = value;
            }
        }

        /// <summary>
        /// Gets or sets the process that is run as a result of running this task.
        /// </summary>
        private Process Process {
            get {
                return _process;
            }
            set {
                _process = value;
            }
        }

        /// <summary>
        /// Gets the program arguments with which to run the wrapped PVCS process.
        /// </summary>
        public sealed override string ProgramArguments {
            get {
                return GetProgramArguments();
            }
        }

        /// <summary>
        /// Gets the executable name for the command-line tool to run for the PVCS task.
        /// </summary>
        public sealed override string ExeName {
            get {
                return PVCSBin + "pcli.exe";
            }
        }

        /// <summary>
        /// Gets the PCLI command name that corresponds to the operation the task performs.
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default, this property will return the name of the task minus the starting "pvcs". Subclasses need
        /// only override this property if there is a mismatch between the task name and the PCLI command name.
        /// </para>
        /// </remarks>
        protected virtual string PCLICommandName {
            get {
                string retVal = base.Name;

                if (retVal.StartsWith("pvcs")) {
                    retVal = retVal.Substring(4);
                }

                return retVal;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the process that is wrapped by this PVCS task.
        /// </summary>
        /// <remarks>
        /// Provided only to seal the implementation of <c>StartProcess()</c>.
        /// </remarks>
        /// <returns>The process that was started.</returns>
        protected sealed override Process StartProcess() {
            return base.StartProcess();
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <remarks>
        /// Provided only to seal the implementation of <c>ExecuteTask()</c>.
        /// </remarks>
        protected sealed override void ExecuteTask() {
            base.ExecuteTask();
        }

        /// <summary>
        /// Prepares the process wrapped by this task for execution.
        /// </summary>
        /// <param name="process">The process to prepare for execution.</param>
        protected override void PrepareProcess(Process process) {
            base.PrepareProcess(process);
            Process = process;
            //listen for when the process finishes so we can output a more useful error message when it fails
            process.Exited += new EventHandler(process_Exited);

            Log(Level.Info, "Starting process: {0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments);
        }

        /// <summary>
        /// Allows tasks to add their task-specific arguments to the collection of arguments to be passed to the
        /// PVCS command-line tool.
        /// </summary>
        /// <param name="arguments">The collection of arguments.</param>
        protected abstract void AddCommandLineArguments(PVCSCommandArgumentCollection arguments);

        /// <summary>
        /// Constructs the program arguments that should be used when executing the wrapped PVCS process.
        /// </summary>
        /// <returns>A <c>string</c> containing the program arguments.</returns>
        private string GetProgramArguments() {
            StringBuilder retVal = new StringBuilder();

            PVCSCommandArgumentCollection arguments = new PVCSCommandArgumentCollection();
            //no banner
            arguments.Add("-nb", null, PVCSCommandArgumentPosition.BeforePCLICommand);
            arguments.Add(PCLICommandName, null, PVCSCommandArgumentPosition.Start);

            //allow subclasses to add their task-specific arguments
            AddCommandLineArguments(arguments);

            PVCSCommandArgument[] argumentsArray = arguments.ToArray();
            Array.Sort(argumentsArray);

            for (int i = 0; i < argumentsArray.Length; ++i) {
                Log(Level.Debug, "{0}: {1}", i, argumentsArray[i]);

                if (i != 0) {
                    retVal.Append(" ");
                }

                retVal.Append(argumentsArray[i]);
            }

            return retVal.ToString();
        }

        #endregion

        #region Event Handlers

        private void process_Exited(object sender, EventArgs e) {
            if (Process.ExitCode < 0) {
                string message;

                switch (Process.ExitCode) {
                    case -2:
                        message = string.Format("The specified PCLI command ({0}) or user function was not found.", PCLICommandName);
                        break;
                    case -3:
                        message = "A non-PCLI related error (eg. file permissions) or a command error occurred.";
                        break;
                    case -6:
                        message = "An invalid option was specified for the command.";
                        break;
                    case -7:
                        message = "An argument was specified for an option that does not take an argument.";
                        break;
                    case -8:
                        message = "An argument is required for a flag but was not specified.";
                        break;
                    case -9:
                        message = "The wrong type is specified for an option's argument.";
                        break;
                    case -10:
                        message = "The specified file cannot be read.";
                        break;
                    case -11:
                        message = "A required option for a command was not specified.";
                        break;
                    case -12:
                        message = "There has been a security exception. The necessary privileges for the command are not granted.";
                        break;
                    default:
                        message = "An unknown problem has occurred.";
                        break;
                }

                Log(Level.Error, message);
            }
        }

        #endregion
    }
}
