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
using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks.PVCS {
    /// <summary>
    /// Adds a user to a PVCS project or project database.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>adduser</c> PCLI command to add the user to the PVCS project or database.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Adds a user with name <c>kb</c> and password <c>*Muse*</c> to the project database specified by the
    ///   <c>project-database</c> property.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcsadduser projectdatabase="${project-database}" username="kb" password="*Muse*"/>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Adds a user with name <c>kb</c> and password <c>*Muse*</c> to the project database specified by the
    ///   <c>project-database</c> property. The user's logon will expire on the 26th of October, 2005.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcsadduser projectdatabase="${project-database}" username="kb" password="*Muse*" expirydate="10/26/2005"/>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("pvcsadduser")]
    public sealed class PVCSAddUserTask : PVCSProjectDatabaseTask {
        #region Fields

        /// <see cref="ExpiryDate"/>
        private DateTime _expiryDate;

        /// <see cref="UserPassword"/>
        private string _userPassword;

        /// <see cref="UserName"/>
        private string _userName;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the expiration date for the new user.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-e</c> parameter to the <c>pcli adduser</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("expirydate", Required = false)]
        [DateTimeValidator]
        public DateTime ExpiryDate {
            get {
                return _expiryDate;
            }
            set {
                _expiryDate = value;
            }
        }

        /// <summary>
        /// Gets or sets the password for the new user.
        /// </summary>
        [TaskAttribute("userpassword", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string UserPassword {
            get {
                return _userPassword;
            }
            set {
                _userPassword = value;
            }
        }

        /// <summary>
        /// Gets or sets the user name for the new user.
        /// </summary>
        [TaskAttribute("username", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string UserName {
            get {
                return _userName;
            }
            set {
                _userName = value;
            }
        }

        /// <see cref="PVCSProjectDatabaseTask.SupportsIncludeSubprojects"/>
        public override bool SupportsIncludeSubprojects {
            get {
                return false;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructs and initializes an instance of <c>PVCSAddUserTask</c>.
        /// </summary>
        public PVCSAddUserTask() {
            _expiryDate = DateTime.MaxValue;
        }

        /// <see cref="PVCSTask.AddCommandLineArguments"/>
        protected override void AddCommandLineArguments(PVCSCommandArgumentCollection arguments) {
            base.AddCommandLineArguments(arguments);

            if (ExpiryDate != DateTime.MaxValue) {
                arguments.Add("-e", ExpiryDate);
            }

            string userPassword = UserName;

            if (Password != null) {
                userPassword = userPassword + ":" + Password;
            }

            arguments.Add(userPassword, null, PVCSCommandArgumentPosition.End);
        }

        #endregion
    }
}
