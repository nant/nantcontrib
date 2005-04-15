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
    /// Removes a specified promotion group from versioned files.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>deletegroup</c> PCLI command to remove the promotion group from the versioned files.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Removes the <c>DEV</c> promotion group from <c>App.ico</c> in the project database specified by the
    ///   <c>project-database</c> property.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcsdeletegroup projectdatabase="${project-database}" promotiongroup="DEV" entity="/App.ico"/>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Removes the <c>DEV</c> promotion group all files in the project database specified by the
    ///   <c>project-database</c> property.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcsdeletegroup projectdatabase="${project-database}" promotiongroup="DEV" entity="/" includesubprojects="true"/>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("pvcsdeletegroup")]
    public sealed class PVCSDeleteGroupTask : PVCSSingleEntityTask {
        #region Fields

        /// <see cref="PromotionGroup"/>
        private string _promotionGroup;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the promotion group to delete.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-g</c> parameter to the <c>pcli deletegroup</c> command.
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
            arguments.Add("-g", PromotionGroup);
        }

        #endregion
    }
}
