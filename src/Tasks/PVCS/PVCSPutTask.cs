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
using NAnt.Core.Attributes;
using NAnt.Core.Types;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks.PVCS {
    /// <summary>
    /// Puts files into a PVCS repository.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>put</c> PCLI command to put the files into PVCS.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Puts the file called <c>App.ico</c> into the project database specified by the <c>project-database</c>
    ///   property. The description for the change is <c>Added more colour</c>.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcsput projectdatabase="${project-database}" description="Added more colour">
    ///     <entities>
    ///         <entity name="/App.ico"/>
    ///     </entities>
    /// </pvcsput>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Puts all files into the project database specified by the <c>project-database</c> property. The description
    ///   for the changes is <c>Major changes</c>. Even if the workfiles have not been changed, they will result in a
    ///   new revision in PVCS.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcsput projectdatabase="${project-database}" description="Major changes" checkinunchanged="true" includesubprojects="true">
    ///     <entities>
    ///         <entity name="/"/>
    ///     </entities>
    /// </pvcsput>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Puts <c>file.txt</c> and all files in <c>folder</c> into the project database specified by the
    ///   <c>project-database</c> property. The description for the changes is <c>Some changes</c>. A new branch is
    ///   forcibly created via the <c>forcebranch</c> attribute. Leading and trailing whitespace is ignored when
    ///   determining whether the workfile has been altered.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcsput projectdatabase="${project-database}" description="Some changes" forcebranch="true" ignorespaces="true">
    ///     <entities>
    ///         <entity name="/folder"/>
    ///         <entity name="/file.txt"/>
    ///     </entities>
    /// </pvcsput>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("pvcsput")]
    public sealed class PVCSPutTask : PVCSMultipleEntityTask {
        #region Fields

        /// <see cref="BaseProjectPath"/>
        private string _baseProjectPath;

        /// <see cref="CheckInUnchanged"/>
        private bool _checkInUnchanged;

        /// <see cref="Description"/>
        private string _description;

        /// <see cref="FloatLabel"/>
        private bool _floatLabel;

        /// <see cref="ForceBranch"/>
        private bool _forceBranch;

        /// <see cref="IgnoreSpaces"/>
        private bool _ignoreSpaces;

        /// <see cref="KeepWorkfile"/>
        private bool _keepWorkfile;

        /// <see cref="Location"/>
        private string _location;

        /// <see cref="Lock"/>
        private bool _lock;

        /// <see cref="OverrideWorkfileLocation"/>
        private bool _overrideWorkfileLocation;

        /// <see cref="PromotionGroup"/>
        private string _promotionGroup;

        /// <see cref="ReassignLabelIfExists"/>
        private bool _reassignLabelIfExists;

        /// <see cref="Revision"/>
        private double _revision;

        /// <see cref="UseSameDescription"/>
        private bool _useSameDescription;

        /// <see cref="VersionLabel"/>
        private string _versionLabel;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the base project path.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-bp</c> parameter to the <c>pcli put</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("baseprojectpath", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string BaseProjectPath {
            get {
                return _baseProjectPath;
            }
            set {
                _baseProjectPath = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether unchanged workfiles should be checked in.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-yf</c> parameter to the <c>pcli put</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("checkinunchanged", Required = false)]
        [BooleanValidator]
        public bool CheckInUnchanged {
            get {
                return _checkInUnchanged;
            }
            set {
                _checkInUnchanged = value;
            }
        }

        /// <summary>
        /// Gets or sets the description to be applied to the checked in revisions.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-m</c> parameter to the <c>pcli put</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("description", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string Description {
            get {
                return _description;
            }
            set {
                _description = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the version label specified by <see cref="VersionLabel"/>
        /// should float.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-fv</c> parameter to the <c>pcli put</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("floatlabel", Required = false)]
        [BooleanValidator]
        public bool FloatLabel {
            get {
                return _floatLabel;
            }
            set {
                _floatLabel = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a new branch will be created.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-fb</c> parameter to the <c>pcli put</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("forcebranch", Required = false)]
        [BooleanValidator]
        public bool ForceBranch {
            get {
                return _forceBranch;
            }
            set {
                _forceBranch = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether leading and trailing spaces should be ignored when determining
        /// whether the revision has changed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-b</c> parameter to the <c>pcli put</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("ignorespaces", Required = false)]
        [BooleanValidator]
        public bool IgnoreSpaces {
            get {
                return _ignoreSpaces;
            }
            set {
                _ignoreSpaces = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the workfile should kept in its original state.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-k</c> parameter to the <c>pcli put</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("keepworkfile", Required = false)]
        [BooleanValidator]
        public bool KeepWorkfile {
            get {
                return _keepWorkfile;
            }
            set {
                _keepWorkfile = value;
            }
        }

        /// <summary>
        /// Gets or sets an alternative location for workfiles.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-a</c> parameter to the <c>pcli put</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("location", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public new string Location {
            get {
                return _location;
            }
            set {
                _location = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the files should be locked after the put operation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-l</c> parameter to the <c>pcli put</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("lock", Required = false)]
        [BooleanValidator]
        public bool Lock {
            get {
                return _lock;
            }
            set {
                _lock = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the workfile location for files should be overridden.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-o</c> parameter to the <c>pcli put</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("overrideworkfilelocation", Required = false)]
        [BooleanValidator]
        public bool OverrideWorkfileLocation {
            get {
                return _overrideWorkfileLocation;
            }
            set {
                _overrideWorkfileLocation = value;
            }
        }

        /// <summary>
        /// Gets or sets the promotion in use. If a promotion group is specified, this option identifies the
        /// promotion group to which the revision is currently assigned. If no promotion group is specified (ie.
        /// this property is set to an empty string), this option indicates that one is not identifying the
        /// revision by promotion group.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-g</c> parameter to the <c>pcli put</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("promotiongroup", Required = false)]
        [StringValidator(AllowEmpty = true)]
        public string PromotionGroup {
            get {
                return _promotionGroup;
            }
            set {
                _promotionGroup = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the version label specified by <see cref="VersionLabel"/>
        /// should be reassigned if it already exists.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-yv</c> parameter to the <c>pcli put</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("reassignlabelifexists", Required = false)]
        [BooleanValidator]
        public bool ReassignLabelIfExists {
            get {
                return _reassignLabelIfExists;
            }
            set {
                _reassignLabelIfExists = value;
            }
        }

        /// <summary>
        /// Gets or sets the revision number to use for the new revision.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-r</c> parameter to the <c>pcli put</c> command.
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
        /// Gets or sets a value indicating whether the same description should be used for all versioned items.
        /// This is <c>true</c> by default.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-ym</c> parameter to the <c>pcli put</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("usesamedescription", Required = false)]
        [BooleanValidator]
        public bool UseSameDescription {
            get {
                return _useSameDescription;
            }
            set {
                _useSameDescription = value;
            }
        }

        /// <summary>
        /// Gets or sets the version label to assign to the new revisions.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-v</c> parameter to the <c>pcli put</c> command.
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
        /// Constructs and initializes an instance of <c>PVCSPut</c>.
        /// </summary>
        public PVCSPutTask() {
            _revision = Double.MaxValue;
            _useSameDescription = true;
        }

        /// <see cref="PVCSTask.AddCommandLineArguments"/>
        protected override void AddCommandLineArguments(PVCSCommandArgumentCollection arguments) {
            base.AddCommandLineArguments(arguments);

            if (BaseProjectPath != null) {
                arguments.Add("-bp", BaseProjectPath);
            }

            if (CheckInUnchanged) {
                arguments.Add("-yf");
            }

            if (Description != null) {
                arguments.Add("-m", Description);
            }

            if (FloatLabel) {
                arguments.Add("-fv");
            }

            if (ForceBranch) {
                arguments.Add("-fb");
            }

            if (IgnoreSpaces) {
                arguments.Add("-b");
            }

            if (KeepWorkfile) {
                arguments.Add("-k");
            }

            if (Location != null) {
                arguments.Add("-a", Location);
            }

            if (Lock) {
                arguments.Add("-l");
            }

            if (OverrideWorkfileLocation) {
                arguments.Add("-o");
            }

            if (PromotionGroup != null) {
                arguments.Add("-g", PromotionGroup);
            }

            if (ReassignLabelIfExists) {
                arguments.Add("-yv");
            }

            if (Revision != Double.MaxValue) {
                arguments.Add("-r", Revision);
            }

            if (UseSameDescription) {
                arguments.Add("-ym");
            }

            if (VersionLabel != null) {
                arguments.Add("-v", VersionLabel);
            }
        }

        #endregion
    }
}
