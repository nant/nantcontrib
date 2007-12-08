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
using System.Collections;
using System.Diagnostics;
using System.Text;

using NAnt.Core;
using NAnt.Core.Tasks;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

using NAnt.Contrib.Types;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Base class functionality for the GAC tasks.
    /// </summary>
    /// <remarks>
    /// Concrete GAC tasks extend this class in order to obtain common functionality.
    /// </remarks>
    public abstract class GacTaskBase : ExternalProgramBase {
        #region Fields

        /// <summary>
        /// Stores the details of the assembly currently being operated against. This could be a name or
        /// path, depending on the concrete task.
        /// </summary>
        private string _currentAssembly;

        /// <summary>
        /// See <see cref="Force"/>.
        /// </summary>
        private bool _force;

        /// <summary>
        /// See <see cref="Reference"/>.
        /// </summary>
        private GacReference _reference;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the GAC operation will be forced.
        /// </summary>
        /// <remarks>
        /// The exact meaning of this property is dependent on the subclass. As such, subclasses should override this
        /// property to provide a valid description.
        /// </remarks>
        [TaskAttribute("force", Required = false)]
        [BooleanValidator]
        public virtual bool Force {
            get { return _force; }
            set { _force = value; }
        }

        /// <summary>
        /// Specifies reference details to use when working with the GAC.
        /// </summary>
        [BuildElement("reference", Required = false)]
        public GacReference Reference {
            get { return _reference; }
            set { _reference = value; }
        }

        /// <summary>
        /// Concrete GAC tasks must override this property to return an array of assembly names or paths
        /// upon which to operate.
        /// </summary>
        protected abstract ICollection AssemblyList {
            get;
        }

        /// <summary>
        /// Gets the executable name for the <c>gacutil</c> command-line tool.
        /// </summary>
        public sealed override string ExeName {
            get { return "gacutil"; }
        }

        /// <summary>
        /// Specifies whether a reference was specified for the GAC task.
        /// </summary>
        protected bool ReferenceSpecified {
            get { return (Reference != null); }
        }

        /// <summary>
        /// Gets the current assembly being operated against.
        /// </summary>
        protected string CurrentAssembly {
            get { return _currentAssembly; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructs and initialises an instance of <c>GacTask</c>.
        /// </summary>
        public GacTaskBase() {
        }

        /// <summary>
        /// Gets a value indiciating whether the external program is a managed
        /// application which should be executed using a runtime engine, if 
        /// configured. 
        /// </summary>
        /// <value>
        /// <see langword="ManagedExecutionMode.Auto" />.
        /// </value>
        /// <remarks>
        /// Modifying this property has no effect.
        /// </remarks>
        public override ManagedExecution Managed {
            get { return ManagedExecution.Auto; }
            set { }
        }

        /// <summary>
        /// Gets the program arguments with which to run the <c>gacutil</c> process.
        /// </summary>
        public sealed override string ProgramArguments {
            get {
                StringBuilder retVal = new StringBuilder();

                //never show logo
                retVal.Append("/nologo");

                if (Force) {
                    retVal.Append(" /f");
                }

                if (ReferenceSpecified && Reference.If && !Reference.Unless) {
                    retVal.Append(" /r ");

                    switch (Reference.SchemeType) {
                        case SchemeType.FilePath:
                            retVal.Append("FILEPATH");
                            break;
                        case SchemeType.Opaque:
                            retVal.Append("OPAQUE");
                            break;
                        case SchemeType.UninstallKey:
                            retVal.Append("UNINSTALL_KEY");
                            break;
                        default:
                            throw new BuildException("Unknown SchemeType: " + Reference.SchemeType);
                    }

                    retVal.Append(" \"").Append(Reference.SchemeId).Append("\" \"").Append(Reference.SchemeDescription).Append("\"");
                }

                // allow concrete task to append any arguments
                AppendProgramArguments(retVal);

                // append the name of the assembly being operated against
                retVal.Append(" \"").Append(CurrentAssembly).Append("\"");

                return retVal.ToString();
            }
        }

        /// <summary>
        /// Starts the process that is wrapped by this GAC task.
        /// </summary>
        /// <remarks>
        /// Provided only to seal the implementation of <c>StartProcess()</c>.
        /// </remarks>
        /// <returns>The process that was started.</returns>
        protected sealed override Process StartProcess() {
            return base.StartProcess();
        }

        /// <summary>
        /// Validates the task's configuration.
        /// </summary>
        protected sealed override void Initialize() {
            base.Initialize();

            if (AssemblyList.Count == 0) {
                throw new BuildException("At least one assembly must be specified.");
            }

            if (ReferenceSpecified) {
                if (Reference.SchemeId == null) {
                    throw new BuildException("Must provide a schemeid when specifying a reference.");
                }
            }
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <remarks>
        /// Provided only to seal the implementation of <c>ExecuteTask()</c>.
        /// </remarks>
        protected sealed override void ExecuteTask() {
            foreach (string assembly in AssemblyList) {
                _currentAssembly = assembly;
                BeforeExecuteTask();
                base.ExecuteTask();
            }
        }

        /// <summary>
        /// Appends any task-specific program arguments.
        /// </summary>
        /// <param name="sb">The <c>StringBuilder</c> on which to append program arguments.</param>
        /// <remarks>
        /// Subclasses must override this method to return the arguments with which to run the GAC task.
        /// </remarks>
        protected abstract void AppendProgramArguments(StringBuilder sb);

        /// <summary>
        /// Invoked prior to invoking <c>ExecuteTask()</c> on the base class.
        /// </summary>
        /// <remarks>
        /// Allows, for example, subclasses to output useful information to the log.
        /// </remarks>
        protected abstract void BeforeExecuteTask();

        #endregion
    }
}
