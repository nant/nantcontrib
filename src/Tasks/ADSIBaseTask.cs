// NAnt - A .NET build tool
// Copyright (C) 2002 Galileo International
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// Gordon Weakliem (gordon.weakliem@galileo.com)
// 

using System;
using System.DirectoryServices; 

using NAnt.Core;
using NAnt.Core.Tasks;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Base NAnt task for working with ADSI.  This task contains only the path of the ADSI
    /// object that you want to work with.
    /// </summary>
    public abstract class ADSIBaseTask : Task {
        #region Private Instance Fields

        private string _path;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The ADSI path of the location where we want to work with.
        /// </summary>
        [TaskAttribute("path", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public String Path {
            get { return _path; }
            set { _path = value; }
        }

        #endregion Public Instance Properties

        // TODO: not at all tested at this point!
#if false
    private string _host = "LocalHost";
    /// <summary>
    /// The Host where the target object is located, default is LocalHost
    /// </summary>
    [TaskAttribute("host", Required=false)]
    public String Host
    {
      get { return _host; }
      set { _host = value; }
    }

    private string _username;
    /// <summary>
    /// The username to authenticate on the host with
    /// </summary>
    /// <remarks>
    /// When you connect using IIS Admin Objects (IIS ADSI provider)from a remote client, you can't 
    /// specify explicit credentials, the program must run with the credentials valid to manipulate 
    /// the metabase on the remote server.  That means that the user running this program must be 
    /// an account with "administrative" privileges on the IIS server (unless you changed the master 
    /// operators property settings of IIS), with the same password as this account on the remote 
    /// server.
    /// Say you have Server A running IIS, Server B running this program.  Say you have on A an 
    /// account named "IISADMIN" with pwd "IISPWD" and this account is member of the 
    /// administrators alias on A, and on serverB the same account named "IISADMIN" with pwd "IISPWD"
    /// When you run this program, in a logon session as "IISADMIN", password="IISPWD", the program 
    /// will succeed.  Anything else will fail with "Access Denied".
    /// </remarks>
    [TaskAttribute("username", Required=false)]
    public String Username
    {
      get { return _username; }
      set { _username = value; }
    }
    private string _password;
    /// <summary>
    /// The password to authenticate with.
    /// </summary>
    [TaskAttribute("password", Required=true)]
    public String Password
    {
      get { return _password; }
      set { _password = value; }
    }
#endif
    }
}
