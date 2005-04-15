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
    /// Changes the promotion group for specified versioned files.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>changegroup</c> PCLI command to change the group for versioned files.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Changes the promotion group for <c>file.txt</c> from <c>SYSTEST</c> to <c>DEV</c>.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcschangegroup projectdatabase="${project-database}" from="SYSTEST" to="DEV" entity="/file.txt"/>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Changes the promotion group for all files from <c>DEV</c> to <c>PROD</c>.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcschangegroup projectdatabase="${project-database}" from="DEV" to="PROD" entity="/" includesubprojects="true"/>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("pvcschangegroup")]
    public sealed class PVCSChangeGroupTask : PVCSSingleEntityTask {
        #region Fields

        /// <see cref="From"/>
        private string _from;

        /// <see cref="To"/>
        private string _to;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the promotion group to change from.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-gf</c> parameter to the <c>pcli changegroup</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("from", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string From {
            get {
                return _from;
            }
            set {
                _from = value;
            }
        }

        /// <summary>
        /// Gets or sets the promotion group to change to.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-gt</c> parameter to the <c>pcli changegroup</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("to", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string To {
            get {
                return _to;
            }
            set {
                _to = value;
            }
        }

        #endregion

        #region Methods

        /// <see cref="PVCSTask.AddCommandLineArguments"/>
        protected override void AddCommandLineArguments(PVCSCommandArgumentCollection arguments) {
            base.AddCommandLineArguments(arguments);
            arguments.Add("-gf", From);
            arguments.Add("-gt", To);
        }

        #endregion
    }
}
