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
using System.DirectoryServices;
using System.Globalization;
using System.IO;

using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;

using NUnit.Framework;

using Tests.NAnt.Core;
using Tests.NAnt.Core.Util;

namespace NAnt.SharePoint.Tasks {
    /// <summary>
    /// Summary description for DeleteSPSiteTest.
    /// </summary>
    [TestFixture]
    public class AddRoleToSPSiteTaskTest : BuildTestBase {

        #region private members for test values

        /// <summary>
        /// A valid name for a new role to add to a site.
        /// </summary>
        private static string _testRoleName = "MyTestRole";

        /// <summary>
        /// The name of an existing role to test adding a role where
        /// that name is already in use.
        /// </summary>
        private static string _existingRoleName = "Contributor";

        private static string _baseRoleName = "Contributor";

        private static string _roleDescription = "Some description";
        /// <summary>
        /// UserName that will be used to create the test user.
        /// </summary>
        private static string _newUserName = "NewSiteUser";

        /// <summary>
        /// Login account for test site.
        /// </summary>
        private static string _newUserLogin = System.Environment.MachineName + "\\" + _newUserName;

        /// <summary>
        /// UserName that will be used to create the test user.
        /// </summary>
        private static string _userName = "AddSPSiteTaskUser";

        /// <summary>
        /// Password that will be given to the test user.
        /// </summary>
        private static string _password = "AddUserSPSiteTaskUserPassword";

        /// <summary>
        /// Url for test site.
        /// </summary>
        private static string _newSPSiteServerUrl = "http://" + System.Environment.MachineName;

        /// <summary>
        /// Title for test site.
        /// </summary>
        private static string _newSPSiteTitle = "SiteManagerTest";

        /// <summary>
        /// Url for test site.
        /// </summary>
        private static string _newSPSiteUrl = _newSPSiteServerUrl + "/sites/" + _newSPSiteTitle;
        /// <summary>
        /// Description for test site.
        /// </summary>
        private static string _newSPSiteDesc = "New Area Description..." + 
            _newSPSiteTitle;

        /// <summary>
        /// Login account for test site.
        /// </summary>
        private static string _newSPSiteOwnerLogin = System.Environment.MachineName + "\\" + _userName;

        /// <summary>
        /// Owner name for test site.
        /// </summary>
        private static string _newSPSiteOwnerName = "Donald Duck";

        /// <summary>
        /// Email for owner of test site.
        /// </summary>
        private static string _newSPSiteOwnerEmail = "Donal.Duck@disney.com";

        /// <summary>
        /// Login account for test site.
        /// </summary>
        private static string _newSPSiteContactLogin = System.Environment.MachineName + "\\" + _userName;

        /// <summary>
        /// Contact name for test site.
        /// </summary>
        private static string _newSPSiteContactName = "Daisy Duck";

        /// <summary>
        /// Email for contact of test site.
        /// </summary>
        private static string _newSPSiteContactEmail = "daisy.duck@disney.com";

        /// <summary>
        /// AD path string for the root directory of the local machine.
        /// </summary>
        private static string _rootDirectory = "WinNT://" + Environment.MachineName + ", computer";
        #endregion private members for test values

        #region private members for project xml
        private static string _xmlProjectTemplate = 
            @"<project name='Test Add User' default='addroletospsite'>
                <target name='addroletospsite'>
                    <addroletospsite url='{0}' 
                        failonerror='{1}' 
                        role='{2}' 
                        description='{3}'
                        baserole='{4}' 
                        addpermissions='{5}'/>
                </target>
            </project>";
        #endregion private members for project xml

        [SetUp]
        protected override void SetUp() {
            base.SetUp();
            
            DirectoryEntry root = new DirectoryEntry(_rootDirectory);

            DirectoryEntry newUserEntry = root.Children.Add(_newUserName, "user");
            newUserEntry.Invoke("SetPassword", _password);
            newUserEntry.CommitChanges();

            DirectoryEntry userEntry = root.Children.Add(_userName, "user");
            userEntry.Invoke("SetPassword", _password);
            userEntry.CommitChanges(); 

            Uri uri = new Uri(_newSPSiteServerUrl);
      
            SPSite site = new SPSite(uri.ToString());
        
            SPSite newSite = site.SelfServiceCreateSite(
                _newSPSiteUrl,
                _newSPSiteTitle,
                _newSPSiteDesc,
                site.RootWeb.Language,
                "STS",
                _newSPSiteOwnerLogin,
                _newSPSiteOwnerName,
                _newSPSiteOwnerEmail,
                _newSPSiteContactLogin,
                _newSPSiteContactName,
                _newSPSiteContactEmail);
        }

        [TearDown]
        protected override void TearDown() {
            DirectoryEntry root = new DirectoryEntry(_rootDirectory);
            DirectoryEntry newUserEntry = root.Children.Find(_newUserName);
            root.Children.Remove(newUserEntry);
            
            DirectoryEntry userEntry = root.Children.Find(_userName);
            root.Children.Remove(userEntry);

            try {
                SPSite site = new SPSite(_newSPSiteUrl);
                site.Delete(); 
            } catch (FileNotFoundException) {
                // If a FileNotFoundException was raised it was because the site was 
                // correctly deleted by the test.
            }
        }

        /// <summary>
        /// Tests the happy case of adding an existing user to an existing role 
        /// in a site.
        /// </summary>
        [Test]
        public void TestAddRoleToSPSite() {
            // Create some list of permissions to add to the role we are 
            // creating.
            String addPerms = SPRights.ApplyStyleSheets.ToString() + ", " +
                SPRights.ManageListPermissions.ToString() + ", " +
                SPRights.ManageRoles.ToString() + ", " +
                SPRights.CreateSSCSite.ToString();

            string result = result = RunBuild(String.Format(CultureInfo.InvariantCulture, 
                _xmlProjectTemplate, 
                _newSPSiteUrl, 
                "true", 
                _testRoleName, 
                _roleDescription, 
                _baseRoleName, 
                addPerms));

            SPSite site = new SPSite(_newSPSiteUrl);
            SPRole newRole = site.RootWeb.Roles[_testRoleName];
            Assertion.AssertNotNull("New Role is null!", newRole);
            
            // Construct the rights the role should have for comparison.  The
            // base permission of a role defaults to the permission of the 
            // "Guest" role.  From there we add anything other permissions
            // the role should have.
            SPRole guest = site.RootWeb.Roles[_baseRoleName];
            SPRights rights = guest.PermissionMask | 
                SPRights.ApplyStyleSheets |
                SPRights.ManageListPermissions | 
                SPRights.ManageRoles | 
                SPRights.CreateSSCSite;

            Assertion.AssertEquals("permissions were not correctly modified.", 
                0,
                rights.CompareTo(newRole.PermissionMask));
        }

        /// <summary>
        /// Tests the case where the role name being created is already in use.
        /// </summary>
        [Test]
        [ExpectedException(typeof(TestBuildException))]
        public void TestAddRoleToSPSiteExistingRole() {
            String addPerms = SPRights.ApplyStyleSheets.ToString() + ", " +
                SPRights.ManageListPermissions.ToString() + ", " +
                SPRights.ManageRoles.ToString() + ", " +
                SPRights.CreateSSCSite.ToString();

            string result = result = RunBuild(String.Format(CultureInfo.InvariantCulture, 
                _xmlProjectTemplate, 
                _newSPSiteUrl, 
                "true", 
                _existingRoleName, 
                _roleDescription, 
                _baseRoleName, 
                addPerms));
        }

        /// <summary>
        /// Tests the case where the role being added has no additional 
        /// permissions to set.
        /// </summary>
        [Test]
        public void TestAddRoleToSPSiteNoAdd() {
            String addPerms = "";

            string result = result = RunBuild(String.Format(CultureInfo.InvariantCulture, 
                _xmlProjectTemplate, 
                _newSPSiteUrl, 
                "true", 
                _testRoleName, 
                _roleDescription, 
                _baseRoleName, 
                addPerms));
  
            SPSite site = new SPSite(_newSPSiteUrl);
            SPRole newRole = site.RootWeb.Roles[_testRoleName];
            Assertion.AssertNotNull("New Role is null!", newRole);
            
            // Construct the rights the role should have for comparison.  
            SPRights rights = site.RootWeb.Roles[_baseRoleName].PermissionMask;

            Assertion.AssertEquals("permissions were not correctly modified.", 
                0,
                rights.CompareTo(newRole.PermissionMask));
        }

        /// <summary>
        /// Tests the case where the role being added has no base role.
        /// </summary>
        [Test]
        public void TestAddRoleToSPSiteNoBase() {
            String addPerms = SPRights.ApplyStyleSheets.ToString() + ", " +
                SPRights.ManageListPermissions.ToString() + ", " +
                SPRights.ManageRoles.ToString() + ", " +
                SPRights.CreateSSCSite.ToString();

            string result = result = RunBuild(String.Format(CultureInfo.InvariantCulture, 
                _xmlProjectTemplate, 
                _newSPSiteUrl, 
                "true", 
                _testRoleName, 
                _roleDescription, 
                "", 
                addPerms));

        
            SPSite site = new SPSite(_newSPSiteUrl);
            SPRole newRole = site.RootWeb.Roles[_testRoleName];
            Assertion.AssertNotNull("New Role is null!", newRole);

            // Construct the rights the role should have for comparison.  The
            // base permission of a role defaults to the permission of the 
            // "Guest" role.  From there we add anything other permissions
            // the role should have.
            SPRights rights = site.RootWeb.Roles["Guest"].PermissionMask | 
                SPRights.ApplyStyleSheets |
                SPRights.ManageListPermissions | 
                SPRights.ManageRoles | 
                SPRights.CreateSSCSite;

            Assertion.AssertEquals("permissions were not correctly modified.", 
                0,
                rights.CompareTo(newRole.PermissionMask));
        }        
        
        /// <summary>
        /// Tests the case where the role being added has no base role, or additional \
        /// permissions to set.
        /// </summary>
        [Test]
        public void TestAddRoleToSPSiteSimple() {
            String addPerms = "";

            string result = result = RunBuild(String.Format(CultureInfo.InvariantCulture, 
                _xmlProjectTemplate, 
                _newSPSiteUrl, 
                "true", 
                _testRoleName, 
                _roleDescription, 
                "", 
                addPerms));

        
            SPSite site = new SPSite(_newSPSiteUrl);
            SPRole newRole = site.RootWeb.Roles[_testRoleName];        
            Assertion.AssertNotNull("New Role is null!", newRole);
          
            // Construct the rights the role should have for comparison.  The
            // base permission of a role defaults to the permission of the 
            // "Guest" role.  From there we add anything other permissions
            // the role should have.
            SPRights rights = site.RootWeb.Roles["Guest"].PermissionMask;
            
            Assertion.AssertEquals("permissions were not correctly modified.", 
                0,
                rights.CompareTo(newRole.PermissionMask));
        }

    }
}
