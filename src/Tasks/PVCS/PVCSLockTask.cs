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
    /// Locks a revision of the specified versioned files.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task uses the <c>lock</c> PCLI command to lock the versioned files.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Locks <c>App.ico</c> in the project database specified by the <c>project-database</c> property.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcslock projectdatabase="${project-database}">
    ///     <entities>
    ///         <entity name="/App.ico"/>
    ///     </entities>
    /// </pvcslock>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Locks all files at and below <c>folder</c> in the project database specified by the <c>project-database</c>
    ///   property.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <pvcslock projectdatabase="${project-database}" includesubprojects="true">
    ///     <entities>
    ///         <entity name="/folder"/>
    ///     </entities>
    /// </pvcslock>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("pvcslock")]
    public sealed class PVCSLockTask : PVCSMultipleEntityTask {
        #region Fields

        /// <see cref="NoBranching"/>
        private bool _noBranching;

        /// <see cref="NoMultilock"/>
        private bool _noMultilock;

        /// <see cref="PromotionGroup"/>
        private string _promotionGroup;

        /// <see cref="Revision"/>
        private double _revision;

        /// <see cref="YesToBranching"/>
        private bool _yesToBranching;

        /// <see cref="YesToMultilock"/>
        private bool _yesToMultilock;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether locking files will take place if checking in those files would
        /// result in a branch.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-nb</c> parameter to the <c>pcli lock</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("nobranching", Required = false)]
        [BooleanValidator]
        public bool NoBranching {
            get {
                return _noBranching;
            }
            set {
                _noBranching = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether already locked revisions will be locked.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-nm</c> parameter to the <c>pcli lock</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("nomultilock", Required = false)]
        [BooleanValidator]
        public bool NoMultilock {
            get {
                return _noMultilock;
            }
            set {
                _noMultilock = value;
            }
        }

        /// <summary>
        /// Gets or sets the promotion group to assign the locked revision.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-g</c> parameter to the <c>pcli lock</c> command.
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
        /// Gets or sets the revision to lock.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-r</c> parameter to the <c>pcli lock</c> command.
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
        /// Gets or sets a value indicating whether revisions will be locked even if that will result in a branch
        /// upon check in.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-yb</c> parameter to the <c>pcli lock</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("yestobranching", Required = false)]
        [BooleanValidator]
        public bool YesToBranching {
            get {
                return _yesToBranching;
            }
            set {
                _yesToBranching = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether revisions will be locked even if that will result in multiple
        /// locks against the same revision.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is equivalent to the <c>-ym</c> parameter to the <c>pcli lock</c> command.
        /// </para>
        /// </remarks>
        [TaskAttribute("yestomultilock", Required = false)]
        [BooleanValidator]
        public bool YesToMultilock {
            get {
                return _yesToMultilock;
            }
            set {
                _yesToMultilock = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructs and initializes an instance of <c>PVCSLock</c>.
        /// </summary>
        public PVCSLockTask() {
            _revision = Double.MaxValue;
        }

        /// <see cref="PVCSTask.AddCommandLineArguments"/>
        protected override void AddCommandLineArguments(PVCSCommandArgumentCollection arguments) {
            base.AddCommandLineArguments(arguments);

            if (NoBranching) {
                arguments.Add("-nb");
            }

            if (NoMultilock) {
                arguments.Add("-nm");
            }

            if (PromotionGroup != null) {
                arguments.Add("-g", PromotionGroup);
            }

            if (Revision != Double.MaxValue) {
                arguments.Add("-r", Revision);
            }

            if (YesToBranching) {
                arguments.Add("-yb");
            }

            if (YesToMultilock) {
                arguments.Add("-ym");
            }
        }

        #endregion
    }
}
