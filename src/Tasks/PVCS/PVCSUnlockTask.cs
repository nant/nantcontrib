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
using NAnt.Core.Types;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks.PVCS {
    /// <summary>
    /// Unlocks revisions of versioned files in a PVCS repository.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>unlock</c> PCLI command to perform the unlock operation.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Unlocks <c>App.ico</c> in the project database specified by the <c>project-database</c> property.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcsunlock projectdatabase="${project-database}">
    ///     <entities>
    ///         <entity name="/App.ico"/>
    ///     </entities>
    /// </pvcsunlock>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Unlocks all files in the project specified by the <c>project-database</c> property. Locks by all users are
    ///   removed.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcsunlock projectdatabase="${project-database}" includesubprojects="true" unlockmode="AllUsers">
    ///     <entities>
    ///         <entity name="/"/>
    ///     </entities>
    /// </pvcsunlock>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("pvcsunlock")]
    public sealed class PVCSUnlockTask : PVCSMultipleEntityTask {
        #region Fields

        /// <see cref="Revision"/>
        private double _revision;

        /// <see cref="UnlockMode"/>
        private PVCSUnlockMode _unlockMode;

        /// <see cref="User"/>
        private string _user;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the revision number to use for the new revision.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-r</c> parameter to the <c>pcli unlock</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("revision", Required = false)]
        public double Revision {
            get {
                return _revision;
            }
            set {
                _revision = value;
            }
        }

        /// <summary>
        /// Gets or sets the unlock mode for the operation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-u</c> parameter to the <c>pcli unlock</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("unlockmode", Required = false)]
        public PVCSUnlockMode UnlockMode {
            get {
                return _unlockMode;
            }
            set {
                _unlockMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the user whose locked files are to be unlocked. This is relevant only when
        /// <see cref="UnlockMode"/> is set to <see cref="PVCSUnlockMode.SpecifiedUser"/>.
        /// </summary>
        [TaskAttribute("user", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string User {
            get {
                return _user;
            }
            set {
                _user = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructs and initializes an instance of <c>PVCSUnlock</c>.
        /// </summary>
        public PVCSUnlockTask() {
            _revision = Double.MaxValue;
            _unlockMode = PVCSUnlockMode.CurrentUser;
        }

        /// <see cref="PVCSTask.AddCommandLineArguments"/>
        protected override void AddCommandLineArguments(PVCSCommandArgumentCollection arguments) {
            base.AddCommandLineArguments(arguments);

            if (Revision != Double.MaxValue) {
                arguments.Add("-r", Revision);
            }

            string commandValue = null;

            switch (UnlockMode) {
                case PVCSUnlockMode.AllUsers:
                    commandValue = "*";
                    break;
                case PVCSUnlockMode.CurrentUser:
                    break;
                case PVCSUnlockMode.SpecifiedUser:
                    if (User == null) {
                        throw new BuildException("Must specify the user when unlockmode is set to SpecifiedUser.");
                    }

                    commandValue = User;
                    break;
                default:
                    throw new BuildException("Unknown PVCSUnlockMode: " + UnlockMode);
            }

            arguments.Add("-u", commandValue);
        }

        #endregion

        #region Inner Types

        /// <summary>
        /// Specifies possible modes for the <see cref="PVCSUnlockTask"/> task.
        /// </summary>
        public enum PVCSUnlockMode {
            /// <summary>
            /// All locks held by the current user are removed.
            /// </summary>
            CurrentUser,
            /// <summary>
            /// All locks held by a specified user are removed.
            /// </summary>
            SpecifiedUser,
            /// <summary>
            /// All locks held by all users are removed.
            /// </summary>
            AllUsers
        }

        #endregion
    }
}
