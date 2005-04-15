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
    /// Creates a project in a PVCS repository.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>createproject</c> PCLI command to create the project in the PVCS repository.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Creates a project called <c>Songs</c> in the project database specified by the <c>project-database</c>
    ///   property. The workfile location for the project is set to <c>C:\Work\Songs</c>.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcscreateproject projectdatabase="${project-database}" workfilelocation="C:\Work\Songs" entity="/Songs"/>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("pvcscreateproject")]
    public sealed class PVCSCreateProjectTask : PVCSSingleEntityTask {
        #region Fields

        /// <see cref="WorkfileLocation"/>
        private string _workfileLocation;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the workfile location for the created project.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-w</c> parameter to the <c>pcli createproject</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("workfilelocation", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string WorkfileLocation {
            get {
                return _workfileLocation;
            }
            set {
                _workfileLocation = value;
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

        /// <see cref="PVCSTask.AddCommandLineArguments"/>
        protected override void AddCommandLineArguments(PVCSCommandArgumentCollection arguments) {
            base.AddCommandLineArguments(arguments);

            if (WorkfileLocation != null) {
                arguments.Add("-w", WorkfileLocation);
            }
        }

        #endregion
    }
}
