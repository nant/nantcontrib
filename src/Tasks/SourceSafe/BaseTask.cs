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
using SourceSafeTypeLib;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.SourceSafe {

    /// <summary>
    /// The base abstract class for all Visual Source Safe Tasks.  
    /// Provides the core attributes, and functionality for opening an item 
    /// in a Visual Source Safe database.
    /// </summary>
    public abstract class BaseTask : Task {
        // Flag required for performing a recursive action
        protected const VSSFlags RecursiveFlag = VSSFlags.VSSFLAG_RECURSYES | 
            VSSFlags.VSSFLAG_FORCEDIRNO;

        VSSDatabase _database = null;
        IVSSItem _item = null;

        string _dbpath = "";
        string _path = "";
        string _password = "";
        string _user = "";
        string _version = "";
        /// <summary>
        /// The absolute path to the folder that contains the srcsafe.ini. Required.
        /// </summary>
        [TaskAttribute("dbpath", Required=true)]
        public string dbPath { 
            get { return _dbpath; }
            set { _dbpath = value; }
        }
        
        /// <summary>
        /// The source safe project or file path, starting with "$/".  Required.
        /// </summary>
        [TaskAttribute("path", Required=true)]
        public string Path { 
            get { return _path; }
            set { _path = value; }
        }

        /// <summary>
        /// The password to use to login to the Source Safe database.
        /// </summary>
        [TaskAttribute("password")]
        public string Password {
            get { return _password; }
            set { _password = value; }
        }

        /// <summary>
        /// The user id to use to login to the Source Safe database. Required.
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
            set { _version = value; }
        }

        public VSSDatabase Database { get { return _database; } set { _database = value; } }
        public IVSSItem Item { get { return _item; } set { _item = value; } }
        
        /// <summary>
        /// Opens the Source Safe database and sets the reference to the specified
        /// item and version.
        /// </summary>
        protected void Open() {
            try {
                _database = new VSSDatabase();
                _database.Open(_dbpath, _user, _password);
            }
            catch (Exception e) {
                throw new BuildException("Failed to open database", Location, e);
            }

            try {
                // Get the reference to the specified project item 
                // and version
                _item = _database.get_VSSItem(_path, false);
                if (_version.Length > 0) {
                    _item = _item.get_Version(_version);
                }
            }
            catch (Exception e) {
                throw new BuildException("path and/or version not valid", Location, e);
            }
        }
    }
}
