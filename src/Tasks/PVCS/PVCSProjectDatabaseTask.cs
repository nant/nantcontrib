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
using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Util;

using NAnt.Contrib.Types.PVCS;

namespace NAnt.Contrib.Tasks.PVCS {
    /// <summary>
    /// A base class for PVCS tasks that deal with project databases.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class can be used as a base class for PVCS tasks that operate against a project database. It provides
    /// common attributes and functionality for such tasks.
    /// </para>
    /// </remarks>
    public abstract class PVCSProjectDatabaseTask : PVCSTask {
        #region Fields

        /// <see cref="IncludeSubprojects"/>
        private bool _includeSubprojects;

        /// <see cref="Password"/>
        private string _password;

        /// <see cref="ProjectDatabase"/>
        private string _projectDatabase;

        /// <see cref="ProjectPath"/>
        private string _projectPath;

        /// <see cref="UserId"/>
        private string _userId;

        /// <see cref="Workspace"/>
        private string _workspace;

        /// <summary>
        /// Set to <c>true</c> if the <see cref="IncludeSubprojects"/> property is manipulated. Some tasks don't
        /// support this property and so an exception will be thrown if the property is used.
        /// </summary>
        private bool _includeSubProjectsUsed;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the operation should include subprojects.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-z</c> command-line option.
        /// </para>
        /// </remarks>
        [TaskAttribute("includesubprojects", Required = false)]
        [BooleanValidator]
        public virtual bool IncludeSubprojects {
            get {
                _includeSubProjectsUsed = true;
                return _includeSubprojects;
            }
            set {
                _includeSubProjectsUsed = true;
                _includeSubprojects = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the specific task implementation supports the <c>includesubprojects</c>
        /// task attribute. If not, an exception will be thrown if an attempt is made to set the attribute.
        /// </summary>
        public virtual bool SupportsIncludeSubprojects {
            get {
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the password to use when connecting to the project database.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the password part of the <c>-id</c> command-line option.
        /// </para>
        /// </remarks>
        [TaskAttribute("password", Required = false)]
        public string Password {
            get {
                return _password;
            }
            set {
                _password = value;
            }
        }

        /// <summary>
        /// Gets or sets the user ID to use when connecting to the project database.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the user ID part of the <c>-id</c> command-line option.
        /// </para>
        /// </remarks>
        [TaskAttribute("userid", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string UserId {
            get {
                return _userId;
            }
            set {
                _userId = value;
            }
        }

        /// <summary>
        /// Gets or sets the workspace to use when connecting to the project database.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-sp</c> command-line option.
        /// </para>
        /// </remarks>
        [TaskAttribute("workspace", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string Workspace {
            get {
                return _workspace;
            }
            set {
                _workspace = value;
            }
        }

        /// <summary>
        /// Gets or sets the project database to utilize during the operation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-pr</c> command-line option.
        /// </para>
        /// </remarks>
        [TaskAttribute("projectdatabase", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string ProjectDatabase {
            get {
                return _projectDatabase;
            }
            set {
                _projectDatabase = value;
            }
        }

        /// <summary>
        /// Gets or sets the project path to utilize during the operation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-pp</c> command-line option.
        /// </para>
        /// </remarks>
        [TaskAttribute("projectpath", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string ProjectPath {
            get {
                return _projectPath;
            }
            set {
                _projectPath = value;
            }
        }

        #endregion

        #region Methods

        /// <see cref="PVCSTask.AddCommandLineArguments"/>
        protected override void AddCommandLineArguments(PVCSCommandArgumentCollection arguments) {
            if (!SupportsIncludeSubprojects && _includeSubProjectsUsed) {
                throw new BuildException(string.Format("The {0} task does not support the includesubprojects attribute.", base.Name));
            }

            if (IncludeSubprojects) {
                arguments.Add("-z");
            }

            if (ProjectDatabase != null) {
                arguments.Add("-pr", ProjectDatabase);
            }

            if (ProjectPath != null) {
                arguments.Add("-pp", ProjectPath);
            }

            if (UserId != null) {
                string commandValue = null;

                if (Password == null) {
                    commandValue = UserId;
                } else {
                    commandValue = string.Format("{0}:{1}", UserId, Password);
                }

                arguments.Add("-id", commandValue);
            }

            if (Workspace != null) {
                arguments.Add("-sp", Workspace);
            }
        }

        #endregion
    }
}
