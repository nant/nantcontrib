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
    /// Adds a SPRole to an existing SPSite.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   Creates a SPRole for the site specified.
    ///   </para>
    ///   <note>
    ///   This task creates a new role with a PermissionMask of 
    ///   SPRights.EmptyMask.  If a Base Role property was provided,
    ///   the new role's permission mask will be set to the 
    ///   PermissionMask of the existing role.  Finally, the mask will be
    ///   modified by adding permissions specified by the AddPermissions
    ///   property.
    ///   If the name of the role being created is already in use a 
    ///   <see cref="BuildException" /> will be raised.
    ///   </note>
    /// </remarks>
    /// <example>
    ///   <para>Add a SPRole to a SPSite.</para>
    ///   <code>
    ///     <![CDATA[
    /// <addroletospsite 
    ///     url="http://SPS2K3" 
    ///     role="Contributor"
    ///     description="Description of the Role"
    ///     baserole="Guest"
    ///     addpermission="ApplyStyleSheets, ManageRoles, CreateSSCSite"/>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("addroletospsite")]
    public class AddRoleToSPSiteTask : Task {
        #region private member variables
        private string _url = "";

        private string _role = "";

        private string _description = "";

        private string _addPermission = "";
     
        private string _baseRole = "";

        #endregion private member variables

        #region Properties
        /// <summary>
        /// The URL for the site to add the role to.
        /// </summary>
        [TaskAttribute("url")]
        public string Url {
            get { return _url; }
            set { _url = value; }
        }

        /// <summary>
        /// The name of the Role to add to the site.
        /// </summary>
        [TaskAttribute("role")]
        public string Role {
            get { return _role; }
            set { _role = value; }
        }

        /// <summary>
        /// The additional permissions of the role to be added.
        /// </summary>
        [TaskAttribute("addpermissions")]
        public string AddPermissions {
            get { return _addPermission; }
            set { _addPermission = value; }
        }

        /// <summary>
        /// The base role of the role to be added.
        /// </summary>
        [TaskAttribute("baserole")]
        public string BaseRole {
            get { return _baseRole; }
            set { _baseRole = value; }
        }

        /// <summary>
        /// The  description for the SPRole to be added.
        /// </summary>
        [TaskAttribute("description")]
        public string Description {
            get { return _description; }
            set { _description = value; }
        }
        #endregion Properties

        /// <summary>
        /// Task for adding a SPRole to a site.
        /// </summary>
        protected override void ExecuteTask() {        
            try {
                SPSite site = new SPSite(Url);

                // Create the role by adding it to the site.
                site.RootWeb.Roles.Add(Role, Description, SPRights.EmptyMask);

                // Get a reference to the newly created role
                SPRole newRole = site.RootWeb.Roles[Role];

                // Set the permission of the new role to the role specified.
                SetBaseRolePermissions(newRole);

                // Set any additional permissions
                AddPermissionsToRole(newRole);

            } 
            catch (SPException ex) { 
                throw new BuildException( 
                    string.Format("Cannot add role: {0} to site: {1}.", 
                    Role, Url), Location, ex);
            }
        }

        /// <summary>
        /// Adds additional permissions to the mask for this role
        /// </summary>
        /// <param name="newRole">The Role whose permissions are to be 
        /// modified.</param>
        private void AddPermissionsToRole(SPRole newRole) {
            if (!AddPermissions.Equals(string.Empty)) {
                newRole.PermissionMask = newRole.PermissionMask | 
                    (SPRights)Enum.Parse(typeof(SPRights), AddPermissions);
            }
        }

        /// <summary>
        /// Sets the permission mask to the mask of the role specified in the 
        /// BaseRole property.
        /// </summary>
        /// <param name="role">The role to be modified.</param>
        private void SetBaseRolePermissions(SPRole role) {
            if (!BaseRole.Equals(string.Empty)) {
                SPRole baseRole = role.ParentWeb.Roles[BaseRole];
                role.PermissionMask = baseRole.PermissionMask;
            }
        }
    }
}
