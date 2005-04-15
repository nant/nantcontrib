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
    /// Assigns a promotion group to versioned files.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>assigngroup</c> PCLI command to assign the group to versioned files.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Assigns the <c>SYSTEST</c> promotion group to all entities with the <c>DEV</c> promotion group in the
    ///   <c>folder</c> project.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcsassigngroup projectdatabase="${project-database}" entity="/folder" assignpromotiongroup="SYSTEST" promotiongroup="DEV"/>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Assigns the <c>SYSTEST</c> promotion group to revision <c>1.2</c> of all entities.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcsassigngroup projectdatabase="${project-database}" entity="/" includesubprojects="true" assignpromotiongroup="SYSTEST" revision="1.2"/>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("pvcsassigngroup")]
    public sealed class PVCSAssignGroupTask : PVCSSingleEntityTask {
        #region Fields

        /// <see cref="AssignPromotionGroup"/>
        private string _assignPromotionGroup;

        /// <see cref="PromotionGroup"/>
        private string _promotionGroup;

        /// <see cref="Revision"/>
        private double _revision;

        /// <see cref="VersionLabel"/>
        private string _versionLabel;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the promotion group to assign to the versioned files.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-g</c> parameter to the <c>pcli assigngroup</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("assignpromotiongroup", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string AssignPromotionGroup {
            get {
                return _assignPromotionGroup;
            }
            set {
                _assignPromotionGroup = value;
            }
        }

        /// <summary>
        /// Gets or sets the promotion group for the versioned files to be assigned the promotion group.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-r</c> parameter to the <c>pcli assigngroup</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("promotiongroup", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string PromotionGroup {
            get {
                return _promotionGroup;
            }
            set {
                _promotionGroup = value;
            }
        }

        /// <summary>
        /// Gets or sets the revision for the versioned files to be assigned the promotion group.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-r</c> parameter to the <c>pcli assigngroup</c> command.
        /// </para>
        /// <para>
        /// If this property has not yet been set, it will return <c>Double.MaxValue</c>.
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
        /// Gets or sets the version label for the versioned files to be assigned the promotion group.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-r</c> parameter to the <c>pcli assigngroup</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("versionlabel", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string VersionLabel {
            get {
                return _versionLabel;
            }
            set {
                _versionLabel = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructs and initializes an instance of <c>PVCSAssignGroupTask</c>.
        /// </summary>
        public PVCSAssignGroupTask() {
            _revision = Double.MaxValue;
        }

        /// <see cref="PVCSTask.AddCommandLineArguments"/>
        protected override void AddCommandLineArguments(PVCSCommandArgumentCollection arguments) {
            base.AddCommandLineArguments(arguments);

            object revisionArgument = null;
            bool promotionGroupSpecified = (PromotionGroup != null);
            bool revisionSpecified = (Revision != Double.MaxValue);
            bool versionLabelSpecified = (VersionLabel != null);
            int count = 0;

            if (promotionGroupSpecified) {
                ++count;
                revisionArgument = PromotionGroup;
            }

            if (revisionSpecified) {
                ++count;
                revisionArgument = Revision;
            }

            if (versionLabelSpecified) {
                ++count;
                revisionArgument = VersionLabel;
            }

            if (count != 1) {
                throw new BuildException("Must specify one of promotiongroup, revision or versionlabel for the pvcsassigngroup task.");
            }

            arguments.Add("-g", AssignPromotionGroup);
            arguments.Add("-r", revisionArgument);
        }

        #endregion
    }
}
