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
    /// Removes a SPRole from an existing SPSite.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   Removes a SPRole from the site specified.
    ///   </para>
    ///   <note>
    ///   If an error occurs removing the role a 
    ///   <see cref="BuildException" /> will be raised.
    ///   </note>
    /// </remarks>
    /// <example>
    ///   <para>Remove a SPRole from a SPSite.</para>
    ///   <code>
    ///     <![CDATA[
    /// <removerolefromspsite 
    ///     url="http://SPS2K3" 
    ///     role="Contributor"/>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("removerolefromspsite")]
    public class RemoveRoleFromSPSiteTask : Task {
        #region private member variables
        private string _url = "";

        private string _role = "";

        #endregion private member variables

        #region Properties
        /// <summary>
        /// The URL for the site to remove a role from.
        /// </summary>
        [TaskAttribute("url")]
        public string Url {
            get { return _url; }
            set { _url = value; }
        }

        /// <summary>
        /// The name of the Role to remove from the site.
        /// </summary>
        [TaskAttribute("role")]
        public string Role {
            get { return _role; }
            set { _role = value; }
        }
        #endregion Properties

        /// <summary>
        /// Task for removing a SPRole from a site.
        /// </summary>
        protected override void ExecuteTask() {        
            try {
                SPSite site = new SPSite(Url);

                // Create the role by adding it to the site.
                site.RootWeb.Roles.Remove(Role);
            } 
            catch (SPException ex) { 
                throw new BuildException( 
                    string.Format("Cannot add role: {0} to site: {1}.", 
                    Role, Url), Location, ex);
            }
        }
    }
}
