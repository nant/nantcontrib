//
// NAntContrib
// Copyright (C) 2001-2005 Gerry Shaw
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
// Gert Driesen (drieseng@users.sourceforge.net)

using System;
using System.Management;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.BizTalk {
    public abstract class BizTalkBase : Task {
        #region Public Instance Properties

        /// <summary>
        /// The name of the management SQL database.
        /// </summary>
        [TaskAttribute("database", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Database {
            get { return _database; }
            set { _database = value; }
        }

        /// <summary>
        /// The name of the SQL Server where the management database is
        /// located.
        /// </summary>
        [TaskAttribute("server", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Server {
            get { return _server; }
            set { _server = value; }
        }

        #endregion Public Instance Properties

        #region Protected Instance Properties

        protected ManagementScope Scope {
            get { 
                if (_scope == null) {
                    _scope = new ManagementScope("root\\MicrosoftBizTalkServer");
                }
                return _scope;
            }
        }

        #endregion Protected Instance Properties

        #region Private Instance Fields

        private string _database;
        private string _server;
        private ManagementScope _scope;

        #endregion Private Instance Fields
    }
}
