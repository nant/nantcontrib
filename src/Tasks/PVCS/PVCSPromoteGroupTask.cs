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
    /// Promotes versioned files to the next promotion group.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>promotegroup</c> PCLI command to promote versioned files.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Promotes all files in the root of the project database specified by the <c>project-database</c> property.
    ///   The files are promoted from the <c>DEV</c> promotion group to the next. Promotion will not take place across
    ///   branches.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcspromotegroup projectdatabase="${project-database}" promotiongroup="DEV" entity="/" acrossbranches="false"/>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Promotes all files in the project database specified by the <c>project-database</c> property. The files are
    ///   promoted from the <c>SYSTEST</c> promotion group to the next. Promotion will take place across branches.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcspromotegroup projectdatabase="${project-database}" promotiongroup="SYSTEST" entity="/" includesubprojects="true" acrossbranches="true"/>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("pvcspromotegroup")]
    public sealed class PVCSPromoteGroupTask : PVCSSingleEntityTask {
        #region Fields

        /// <see cref="AcrossBranches"/>
        private bool _acrossBrances;

        /// <see cref="PromotionGroup"/>
        private string _promotionGroup;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the promotion may occur across branches.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-nb</c> and <c>-yb</c> parameters to the <c>pcli promotegroup</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("acrossbranches", Required = true)]
        [BooleanValidator]
        public bool AcrossBranches {
            get {
                return _acrossBrances;
            }
            set {
                _acrossBrances = value;
            }
        }

        /// <summary>
        /// Gets or sets the promotion group to be promoted.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-g</c> parameter to the <c>pcli promotegroup</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("promotiongroup", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string PromotionGroup {
            get {
                return _promotionGroup;
            }
            set {
                _promotionGroup = value;
            }
        }

        #endregion

        #region Methods

        /// <see cref="PVCSTask.AddCommandLineArguments"/>
        protected override void AddCommandLineArguments(PVCSCommandArgumentCollection arguments) {
            base.AddCommandLineArguments(arguments);
            arguments.Add(AcrossBranches ? "-yb" : "-nb");
            arguments.Add("-g", PromotionGroup);
        }

        #endregion
    }
}
