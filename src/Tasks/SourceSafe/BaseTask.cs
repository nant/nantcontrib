#region GNU General Public License
//
// NAntContrib
// Copyright (C) 2001-2003 Gerry Shaw
//
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

// Jason Reimer, Diversant Inc. (jason.reimer@diversant.net)
#endregion

using System;
using System.Globalization;
using System.IO;

using SourceSafeTypeLib;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks.SourceSafe {

    /// <summary>
    /// The base abstract class for all Visual Source Safe Tasks.  
    /// Provides the core attributes, and functionality for opening an item 
    /// in a Visual Source Safe database.
    /// </summary>
    public abstract class BaseTask : Task {
        #region Protected Static Fields

        // Flag required for performing a recursive action
        protected const VSSFlags RecursiveFlag = VSSFlags.VSSFLAG_RECURSYES | 
            VSSFlags.VSSFLAG_FORCEDIRNO;

        #endregion Protected Static Fields

        #region Private Instance Fields

        private VSSDatabase _database;
        private IVSSItem _item;
        private FileInfo _dbPath;
        private string _path;
        private string _password = string.Empty;
        private string _user = string.Empty;
        private string _version;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The path to the folder that contains "srcsafe.ini".
        /// </summary>
        [TaskAttribute("dbpath", Required=true)]
        public FileInfo DBPath { 
            get { return _dbPath; }
            set { _dbPath = value; }
        }
        
        /// <summary>
        /// The source safe project or file path, starting with "$/".
        /// </summary>
        [TaskAttribute("path", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Path { 
            get { return _path; }
            set { _path = value; }
        }

        /// <summary>
        /// The password to use to login to the SourceSafe database.
        /// </summary>
        [TaskAttribute("password")]
        public string Password {
            get { return _password; }
            set { _password = value; }
        }

        /// <summary>
        /// The user id to use to login to the SourceSafe database.
        /// </summary>
        [TaskAttribute("user", Required=true)]
        public string User { 
            get { return _user; } 
            set { _user = value; }
        }

        /// <summary>
        /// A version of the path to reference. Accepts multiple forms, 
        /// including the label, version number, or date of the version. 
        /// If omitted, the latest version is used.
        /// </summary>
        [TaskAttribute("version")]
        public string Version { 
            get { return _version; } 
            set { _version = StringUtils.ConvertEmptyToNull(value); }
        }

        public VSSDatabase Database {
            get { return _database; }
            set { _database = value; }
        }

        public IVSSItem Item {
            get { return _item; }
            set { _item = value; }
        }

        #endregion Public Instance Properties

        #region Protected Instance Methods

        /// <summary>
        /// Opens the Source Safe database and sets the reference to the specified
        /// item and version.
        /// </summary>
        protected void Open() {
            try {
                _database = new VSSDatabase();
                _database.Open(DBPath.FullName, User, Password);
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Failed to open database \"{0}\".", DBPath.FullName),
                    Location, ex);
            }

            try {
                // get the reference to the specified project item 
                // and version
                _item = _database.get_VSSItem(Path, false);
                if (Version != null) {
                    _item = _item.get_Version(Version);
                }
            } catch (Exception ex) {
                throw new BuildException("The \"path\" and/or \"version\" is not valid.", 
                    Location, ex);
            }
        }

        #endregion Protected Instance Methods
    }
}
