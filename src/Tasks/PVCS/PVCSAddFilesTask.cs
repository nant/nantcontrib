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
    /// Adds files to a PVCS repository.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>addfiles</c> PCLI command to add files to a PVCS repository.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Adds <c>File1.txt</c> and <c>File2.txt</c> to the root level of the 
    ///   project database specified by the <c>project-database</c> property.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcsaddfiles projectdatabase="${project-database}" archivedescription="Adding files to source control.">
    ///     <entities>
    ///         <entity name="C:\Data\File1.txt"/>
    ///         <entity name="C:\Data\Folder\File2.txt"/>
    ///     </entities>
    /// </pvcsaddfiles>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Adds <c>File1.txt</c> and <c>File2.txt</c> to the <c>folder</c> project
    ///   of the project database specified by the <c>project-database</c> 
    ///   property.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcsaddfiles projectdatabase="${project-database}" archivedescription="Adding files to source control." projectpath="/folder">
    ///     <entities>
    ///         <entity name="C:\Data\File1.txt"/>
    ///         <entity name="C:\Data\Folder\File2.txt"/>
    ///     </entities>
    /// </pvcsaddfiles>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Adds <c>another_file.txt</c> and all files and folders at and below 
    ///   <c>C:\Data</c> to the project database specified by the <c>project-database</c>
    ///   property.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcsaddfiles projectdatabase="${project-database}" archivedescription="Adding files to source control." includesubprojects="true">
    ///     <entities>
    ///         <entity name="C:\Data\"/>
    ///         <entity name="C:\Temp\another_file.txt"/>
    ///     </entities>
    /// </pvcsaddfiles>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Adds all files at and below <c>C:\Data\</c> to the project database specified by the <c>project-database</c>
    ///   property. Workfiles will be copied to the workfile location and will overwrite any existing files (as
    ///   dictated by the <c>copymode</c> attribute). The relevant revisions will be locked in PVCS. Added files
    ///   will be assigned the <c>SYSTEST</c> promotion group. 
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcsaddfiles projectdatabase="${project-database}" archivedescription="Files." copymode="CopyWorkfileWithOverwrite" lock="true" promotiongroup="SYSTEST" includesubprojects="true">
    ///     <entities>
    ///         <entity name="C:\Data\"/>
    ///     </entities>
    /// </pvcsaddfiles>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("pvcsaddfiles")]
    public sealed class PVCSAddFilesTask : PVCSMultipleEntityTask {
        #region Fields

        /// <see cref="ArchiveDescription"/>
        private string _archiveDescription;

        /// <see cref="CopyMode"/>
        private PVCSCopyMode _copyMode;

        /// <see cref="DeleteWorkfiles"/>
        private bool _deleteWorkfiles;

        /// <see cref="Description"/>
        private string _description;

        /// <see cref="Lock"/>
        private bool _lock;

        /// <see cref="PromotionGroup"/>
        private string _promotionGroup;

        /// <see cref="SuppressAddIfExists"/>
        private bool _suppressAddIfExists;

        /// <see cref="VersionLabel"/>
        private string _versionLabel;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the archive description for versioned files.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-t</c> parameter to the <c>pcli addfiles</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("archivedescription", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string ArchiveDescription {
            get {
                return _archiveDescription;
            }
            set {
                _archiveDescription = value;
            }
        }

        /// <summary>
        /// Gets or sets the copy mode for the operation.
        /// </summary>
        [TaskAttribute("copymode", Required = false)]
        public PVCSCopyMode CopyMode {
            get {
                return _copyMode;
            }
            set {
                _copyMode = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether workfiles will be deleted after adding them to PVCS.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-d</c> parameter to the <c>pcli addfiles</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("deleteworkfiles", Required = false)]
        [BooleanValidator]
        public bool DeleteWorkfiles {
            get {
                return _deleteWorkfiles;
            }
            set {
                _deleteWorkfiles = value;
            }
        }

        /// <summary>
        /// Gets or sets the description for versioned files.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-m</c> parameter to the <c>pcli addfiles</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("description", Required = false)]
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
        /// Gets or sets a value indicating whether versioned files should be locked after being added to PVCS.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-l</c> parameter to the <c>pcli addfiles</c> command.
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
        /// Gets or sets the promotion group to which added files will be assigned. Setting this attribute to an
        /// empty string indicates the versioned files will not be assigned to any promotion group.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-g</c> parameter to the <c>pcli addfiles</c> command.
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
        /// Gets or sets a value indicating whether workfiles shouldn't be added if they already exist in the PVCS
        /// repository.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-qw</c> parameter to the <c>pcli addfiles</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("suppressaddifexists", Required = false)]
        [BooleanValidator]
        public bool SuppressAddIfExists {
            get {
                return _suppressAddIfExists;
            }
            set {
                _suppressAddIfExists = value;
            }
        }

        /// <summary>
        /// Gets or sets the version label to assign to the added versioned files.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-v</c> parameter to the <c>pcli addfiles</c> command.
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
        /// Constructs and initializes an instance of <c>PVCSAddFilesTask</c>.
        /// </summary>
        public PVCSAddFilesTask() {
            _copyMode = PVCSCopyMode.Default;
        }

        /// <see cref="PVCSTask.AddCommandLineArguments"/>
        protected override void AddCommandLineArguments(PVCSCommandArgumentCollection arguments) {
            base.AddCommandLineArguments(arguments);

            arguments.Add("-t", ArchiveDescription);

            switch (CopyMode) {
                case PVCSCopyMode.Default:
                    break;
                case PVCSCopyMode.CopyWorkfileIfRequired:
                    arguments.Add("-c");
                    break;
                case PVCSCopyMode.CopyWorkfileWithOverwrite:
                    arguments.Add("-co");
                    break;
                default:
                    throw new BuildException("Unknown PVCSCopyMode: " + CopyMode);
            }

            if (DeleteWorkfiles) {
                arguments.Add("-d");
            }

            if (Description != null) {
                arguments.Add("-m", Description);
            }

            if (Lock) {
                arguments.Add("-l");
            }

            if (PromotionGroup != null) {
                arguments.Add("-g", PromotionGroup);
            }

            if (SuppressAddIfExists) {
                arguments.Add("-qw");
            }

            if (VersionLabel != null) {
                arguments.Add("-v", VersionLabel);
            }

        }

        #endregion

        #region Inner Types

        /// <summary>
        /// Specifies possible copy modes for the <see cref="PVCSAddFilesTask"/> task.
        /// </summary>
        public enum PVCSCopyMode {
            /// <summary>
            /// Indicates the default copy mode should be used.
            /// </summary>
            Default,
            /// <summary>
            /// Indicates that workfiles should be copied to the project workfile location is it doesn't already exist.
            /// </summary>
            CopyWorkfileIfRequired,
            /// <summary>
            /// Indicates that workfiles should be copied to the project workfile location and overwrite any existing
            /// workfile.
            /// </summary>
            CopyWorkfileWithOverwrite
        }

        #endregion
    }
}
