//
// NAnt.SharePoint Microsoft Sharepoint Server utility tasks.
// Copyright (C) 2004 Interlink Group, LLC
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
using System;
using System.Globalization;
using System.IO;
using System.Web;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace NAnt.SharePoint {
    /// <summary>
    /// Removes a user from an existing SPSite.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   Removes a user for the site specified.
    ///   </para>
    ///   <note>
    ///   If the user can not be removed, a 
    ///   <see cref="BuildException" /> will be raised.
    ///   </note>
    /// </remarks>
    /// <example>
    ///   <para>Remove a user from a SPSite.</para>
    ///   <code>
    ///     <![CDATA[
    ///  <removeuserfromspsite url="http://SPS2K3" 
    ///    loginId="SPS2K3\theUser"/>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("removeuserfromspsite")]
    public class RemoveUserFromSPSiteTask : Task {
        #region private member variables
        private string _url = "";

        private string _loginId = "";

        #endregion private member variables

        #region Properties
        /// <summary>
        /// The URL for the site remove the SPUser from.
        /// </summary>
        [TaskAttribute("url")]
        public string Url {
            get { return _url; }
            set { _url = value; }
        }

         /// <summary>
        /// The login for the user being removed
        /// </summary>
        [TaskAttribute("loginId")]
        public string LoginId {
            get { return _loginId; }
            set { _loginId = value; }
        }

        #endregion Properties

        /// <summary>
        /// Task for removing an SPUser from a site.
        /// </summary>
        protected override void ExecuteTask() {        
            try {
                SPSite site = new SPSite(Url);
                site.RootWeb.Users.Remove(LoginId);
            } 
            catch (SPException ex) { 
                throw new BuildException( 
                    string.Format("Cannot add user: {0} to site: {1}.", 
                    LoginId, Url), Location, ex);
            }
        }
    }
}
