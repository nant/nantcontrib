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
    /// Summary description for CreateSPSiteTaskTest.
    /// </summary>
    [TestFixture]
    public class CreateSPSiteTaskTest : BuildTestBase {
        #region private members for test values

        /// <summary>
        /// UserName that will be used to create the test user.
        /// </summary>
        private static string _userName = "DeleteSPSiteTaskUser";

        /// <summary>
        /// Password that will be given to the test user.
        /// </summary>
        private static string _password = "Delete1SPSiteTaskUserPassword";

        /// <summary>
        /// Title for test site.
        /// </summary>
        private static string _newSPSiteTitle = "Create SPSite Test 1";

        /// <summary>
        /// Abbreviatin for test site that will be used to build the Url.
        /// </summary>
        private static string _newSPSiteAbbreviation = "CSPT1";

        /// <summary>
        /// Url for test site.
        /// </summary>
        private static string _newSPSiteServerUrl = "http://" + System.Environment.MachineName;

        /// <summary>
        /// Url for test site.
        /// </summary>
        private static string _newSPSiteUrl = _newSPSiteServerUrl + "/sites/" + _newSPSiteAbbreviation;

        /// <summary>
        /// Description for test site.
        /// </summary>
        private static string _newSPSiteDescription = "New Area Description..." + 
            _newSPSiteTitle;

        /// <summary>
        /// SPS Template for test site.
        /// </summary>
        private static string _newSPSiteTemplate = "STS";

        /// <summary>
        /// SPS managed path within SharePoint for test site.
        /// </summary>
        private static string _newSPSiteManagedPath = "sites";

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
            @"<project name='Test Delete' default='create'>
                <target name='create'>
                    <createspsite failonerror='{0}'
                    url='{1}'
                    abbreviation='{2}' 
                    title='{3}' 
                    description='{4}' 
                    template='{5}' 
                    managedpath='{6}'
                    ownerlogin='{7}' 
                    ownername='{8}' 
                    owneremail='{9}' 
                    contactlogin='{10}' 
                    contactname='{11}' 
                    contactemail='{12}'/>
                </target>
            </project>";
        #endregion private members for project xml

        [SetUp]
        protected override void SetUp() {
            base.SetUp();
            
            DirectoryEntry root = new DirectoryEntry(_rootDirectory);
            DirectoryEntry userEntry = root.Children.Add(_userName, "user");
            userEntry.Invoke("SetPassword", _password);
            userEntry.CommitChanges(); 
        }

        [TearDown]
        protected override void TearDown() {
            DirectoryEntry root = new DirectoryEntry(_rootDirectory);
            DirectoryEntry userEntry = root.Children.Find(_userName);
            root.Children.Remove(userEntry);
            
            try {
                SPSite site = new SPSite(_newSPSiteUrl);
                site.Delete(); 
            } catch (FileNotFoundException) {
                // If a FileNotFoundException was raise it was because the site was 
                // correctly deleted by the test.
            }
        }

        [Test]
        public void TestCreateSPSite() {

            string[] parameters = new string[]{"true", 
                                                  _newSPSiteServerUrl,
                                                  _newSPSiteAbbreviation,
                                                  _newSPSiteTitle, 
                                                  _newSPSiteDescription, 
                                                  _newSPSiteTemplate, 
                                                  _newSPSiteManagedPath,
                                                  _newSPSiteOwnerLogin, 
                                                  _newSPSiteOwnerName, 
                                                  _newSPSiteOwnerEmail, 
                                                  _newSPSiteContactLogin, 
                                                  _newSPSiteContactName, 
                                                  _newSPSiteContactEmail,};

            string result = result = RunBuild(String.Format(
                CultureInfo.InvariantCulture, 
                _xmlProjectTemplate, 
                parameters));
            
            SPGlobalAdmin globalAdmin = new SPGlobalAdmin();
            System.Uri uri = new System.Uri(_newSPSiteServerUrl);
            SPVirtualServer vs = globalAdmin.OpenVirtualServer(uri);

            bool siteWasCreated = false;
            foreach (SPSite site in vs.Sites) {
                if (site.RootWeb.Title.Equals(_newSPSiteTitle)) {
                    siteWasCreated = true;
                }
            }
            Assertion.Assert("The site was not created.", siteWasCreated);
        }

        [Test]
        [ExpectedException(typeof(TestBuildException))]
        public void TestCreateSPSiteInvalidUrl() {
            string[] parameters = new string[]{"true", 
                                                  "foo",
                                                  _newSPSiteAbbreviation,
                                                  _newSPSiteTitle, 
                                                  _newSPSiteDescription, 
                                                  _newSPSiteTemplate, 
                                                  _newSPSiteManagedPath,
                                                  _newSPSiteOwnerLogin, 
                                                  _newSPSiteOwnerName, 
                                                  _newSPSiteOwnerEmail, 
                                                  _newSPSiteContactLogin, 
                                                  _newSPSiteContactName, 
                                                  _newSPSiteContactEmail,};

            string result = result = RunBuild(String.Format(
                CultureInfo.InvariantCulture, 
                _xmlProjectTemplate, 
                parameters));
            
            SPGlobalAdmin globalAdmin = new SPGlobalAdmin();
            System.Uri uri = new System.Uri(_newSPSiteServerUrl);
            SPVirtualServer vs = globalAdmin.OpenVirtualServer(uri);

            bool siteWasCreated = false;
            foreach (SPSite site in vs.Sites) {
                if (site.RootWeb.Title.Equals(_newSPSiteTitle)) {
                    siteWasCreated = true;
                }
            }
            Assertion.Assert("The site was not created.", siteWasCreated);
        }

        [Test]
        public void TestCreateSPSiteInvalidUrlWithoutFailOnError() {
            string[] parameters = new string[]{"false", 
                                                  "foo",
                                                  _newSPSiteAbbreviation,
                                                  _newSPSiteTitle, 
                                                  _newSPSiteDescription, 
                                                  _newSPSiteTemplate, 
                                                  _newSPSiteManagedPath,
                                                  _newSPSiteOwnerLogin, 
                                                  _newSPSiteOwnerName, 
                                                  _newSPSiteOwnerEmail, 
                                                  _newSPSiteContactLogin, 
                                                  _newSPSiteContactName, 
                                                  _newSPSiteContactEmail,};

            string result = result = RunBuild(String.Format(
                CultureInfo.InvariantCulture, 
                _xmlProjectTemplate, 
                parameters));
            
            SPGlobalAdmin globalAdmin = new SPGlobalAdmin();
            System.Uri uri = new System.Uri(_newSPSiteServerUrl);
            SPVirtualServer vs = globalAdmin.OpenVirtualServer(uri);

            bool siteWasCreated = false;
            foreach (SPSite site in vs.Sites) {
                if (site.RootWeb.Title.Equals(_newSPSiteTitle)) {
                    siteWasCreated = true;
                }
            }
            Assertion.Assert("The site was not created.", !siteWasCreated);
        }

        [Test]
        [ExpectedException(typeof(TestBuildException))]
        public void TestCreateSPSiteInvalidOwner() {
            string[] parameters = new string[]{"true", 
                                                  _newSPSiteUrl,
                                                  _newSPSiteAbbreviation,
                                                  _newSPSiteTitle, 
                                                  _newSPSiteDescription, 
                                                  _newSPSiteTemplate, 
                                                  _newSPSiteManagedPath,
                                                  "foo", 
                                                  _newSPSiteOwnerName, 
                                                  _newSPSiteOwnerEmail, 
                                                  _newSPSiteContactLogin, 
                                                  _newSPSiteContactName, 
                                                  _newSPSiteContactEmail,};

            string result = result = RunBuild(String.Format(
                CultureInfo.InvariantCulture, 
                _xmlProjectTemplate, 
                parameters));
        }

        [Test]
        [ExpectedException(typeof(TestBuildException))]
        public void TestCreateSPSiteInvalidContact() {
            string[] parameters = new string[]{"true", 
                                                  _newSPSiteUrl,
                                                  _newSPSiteAbbreviation,
                                                  _newSPSiteTitle, 
                                                  _newSPSiteDescription, 
                                                  _newSPSiteTemplate, 
                                                  _newSPSiteManagedPath,
                                                  _newSPSiteOwnerLogin, 
                                                  _newSPSiteOwnerName, 
                                                  _newSPSiteOwnerEmail, 
                                                  "foo", 
                                                  _newSPSiteContactName, 
                                                  _newSPSiteContactEmail,};

            string result = result = RunBuild(String.Format(
                CultureInfo.InvariantCulture, 
                _xmlProjectTemplate, 
                parameters));
        }

        [Test]
        [ExpectedException(typeof(TestBuildException))]
        public void TestCreateSPSiteDuplicate() {
            string[] parameters = new string[]{"true", 
                                                  _newSPSiteUrl,
                                                  _newSPSiteAbbreviation,
                                                  _newSPSiteTitle, 
                                                  _newSPSiteDescription, 
                                                  _newSPSiteTemplate, 
                                                  _newSPSiteManagedPath,
                                                  _newSPSiteOwnerLogin, 
                                                  _newSPSiteOwnerName, 
                                                  _newSPSiteOwnerEmail, 
                                                  _newSPSiteContactLogin, 
                                                  _newSPSiteContactName, 
                                                  _newSPSiteContactEmail,};

            string result = result = RunBuild(String.Format(
                CultureInfo.InvariantCulture, 
                _xmlProjectTemplate, 
                parameters));
            
            result = result = RunBuild(String.Format(
                CultureInfo.InvariantCulture, 
                _xmlProjectTemplate, 
                parameters));
        }
		
    }
}
