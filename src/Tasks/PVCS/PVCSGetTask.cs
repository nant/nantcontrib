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
    /// Gets files from a PVCS repository.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>get</c> PCLI command to get the versioned files from PVCS.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Gets the versioned file called <c>App.ico</c> from the project database specified by the
    ///   <c>project-database</c> property.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcsget projectdatabase="${project-database}">
    ///     <entities>
    ///         <entity name="/App.ico"/>
    ///     </entities>
    /// </pvcsget>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    /// <para>
    /// Gets the versioned file called <c>App.ico</c> from the project database specified by the
    /// <c>project-database</c> property. The file is also locked.
    /// </para>
    /// <code>
    /// <![CDATA[
    /// <pvcsget projectdatabase="${project-database}" lock="true">
    ///     <entities>
    ///         <entity name="/App.ico"/>
    ///     </entities>
    /// </pvcsget>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Gets all revisions assigned the <c>SYSTEST</c> promotion group from the project database specified by the
    ///   <c>project-database</c> property. The workfiles are touched after the get operation.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcsget projectdatabase="${project-database}" includesubprojects="true" promotiongroup="SYSTEST" touch="true">
    ///     <entities>
    ///         <entity name="/"/>
    ///     </entities>
    /// </pvcsget>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("pvcsget")]
    public sealed class PVCSGetTask : PVCSMultipleEntityTask {
        #region Fields

        /// <see cref="BaseProjectPath"/>
        private string _baseProjectPath;

        /// <see cref="Location"/>
        private string _location;

        /// <see cref="Lock"/>
        private bool _lock;

        /// <see cref="MakeWritable"/>
        private bool _makeWritable;

        /// <see cref="MaxDateTime"/>
        private DateTime _maxDateTime;

        /// <see cref="OverrideWorkfileLocation"/>
        private bool _overrideWorkfileLocation;

        /// <see cref="PromotionGroup"/>
        private string _promotionGroup;

        /// <see cref="Revision"/>
        private double _revision;

        /// <see cref="Touch"/>
        private bool _touch;

        /// <see cref="UpdateOnly"/>
        private bool _updateOnly;

        /// <see cref="VersionLabel"/>
        private string _versionLabel;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the base project path.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-bp</c> parameter to the <c>pcli get</c> command.
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
        /// Gets or sets an alternative location for workfiles.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-a</c> parameter to the <c>pcli get</c> command.
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
        /// Gets or sets a value indicating whether revisions involved in the get operation should be locked.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-l</c> parameter to the <c>pcli get</c> command.
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
        /// Gets or sets whether the workfiles should be made writable.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-w</c> parameter to the <c>pcli get</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("makewritable", Required = false)]
        [BooleanValidator]
        public bool MakeWritable {
            get {
                return _makeWritable;
            }
            set {
                _makeWritable = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum date and time of workfiles to retrieve.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-d</c> parameter to the <c>pcli get</c> command.
        /// </para>
        /// <para>
        /// If this property has not yet been set, it will return <c>DateTime.MaxValue</c>.
        /// </para>
        /// </remarks>
        [TaskAttribute("maxdatetime", Required = false)]
        [DateTimeValidator]
        public DateTime MaxDateTime {
            get {
                return _maxDateTime;
            }
            set {
                _maxDateTime = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the workfile location for files should be overridden.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-o</c> parameter to the <c>pcli get</c> command.
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
        /// Gets or sets the promotion group to get.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-g</c> parameter to the <c>pcli get</c> command.
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
        /// Gets or sets the revision to get against.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-r</c> parameter to the <c>pcli get</c> command.
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
        /// Gets or sets a value indicating whether workfiles should be touched after the get.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-t</c> parameter to the <c>pcli get</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("touch", Required = false)]
        [BooleanValidator]
        public bool Touch {
            get {
                return _touch;
            }
            set {
                _touch = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether workfiles should only be gotten if they are newer than the
        /// current workfile.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-u</c> parameter to the <c>pcli get</c> command (without specifying a
        /// date or time).
        /// </para>
        /// </remarks>
        [TaskAttribute("updateonly", Required = false)]
        [BooleanValidator]
        public bool UpdateOnly {
            get {
                return _updateOnly;
            }
            set {
                _updateOnly = value;
            }
        }

        /// <summary>
        /// Gets or sets the version label to get against.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-v</c> parameter to the <c>pcli get</c> command.
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
        /// Constructs and initializes an instance of <c>PVCSGetTask</c>.
        /// </summary>
        public PVCSGetTask() {
            _maxDateTime = DateTime.MaxValue;
            _revision = Double.MaxValue;
        }

        /// <see cref="PVCSTask.AddCommandLineArguments"/>
        protected override void AddCommandLineArguments(PVCSCommandArgumentCollection arguments) {
            base.AddCommandLineArguments(arguments);

            if (BaseProjectPath != null) {
                arguments.Add("-bp", BaseProjectPath);
            }

            if (Location != null) {
                arguments.Add("-a", Location);
            }

            if (Lock) {
                arguments.Add("-l");
            }

            if (MakeWritable) {
                arguments.Add("-w");
            }

            if (!MaxDateTime.Equals(DateTime.MaxValue)) {
                arguments.Add("-d", MaxDateTime);
            }

            if (OverrideWorkfileLocation) {
                arguments.Add("-o");
            }

            if (PromotionGroup != null) {
                arguments.Add("-g", PromotionGroup);
            }

            if (Revision != Double.MaxValue) {
                arguments.Add("-r", Revision);
            }

            if (Touch) {
                arguments.Add("-t");
            }

            if (UpdateOnly) {
                arguments.Add("-u");
            }

            if (VersionLabel != null) {
                arguments.Add("-v", VersionLabel);
            }
        }

        #endregion
    }
}
