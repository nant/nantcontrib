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


namespace NAnt.SharePoint.Tasks
{
	/// <summary>
	/// Summary description for AddListToSPSiteTaskTest.
	/// </summary>
	[TestFixture]
	public class AddListToSPSiteTaskTest : BuildTestBase {

		#region private members for test values for creating site
		/// <summary>
		/// UserName that will be used to create the test user.
		/// </summary>
		private static string _userName = "DeleteSPSiteTaskUser";

		/// <summary>
		/// Password that will be given to the test user.
		/// </summary>
		private static string _password = "Add1SPSiteTaskUserPassword";
		/// <summary>
		/// Title for test site.
		/// </summary>
		private static string _newSPSiteTitle = "SiteManagerTest";

		/// <summary>
		/// Url for test site.
		/// </summary>
		private static string _newSPSiteServerUrl = "http://" + System.Environment.MachineName;

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


		#region private members for test values for creating list
		private static string _defaultWebUrl = "";
		private static string _invalidWebUrl = "InvalidUrl";
		private static string _invalidDocTemplateIndex = "-1";
		private static string _invalidTemplateName = "InvalidTemplateName";
		private string _listTitle = "One Crazy List";
		private string _listDescription = "This is the description for one Crazy List";
		private string _listTemplateName = "Discussion Board";
		private string _docTemplateIndex = "0";
		#endregion

		#region private members for project xml
		private static string _xmlProjectTemplate = 
			@"<project name='Add List' default='addlist'>
                <target name='addlist'>
                    <addlistospsite failonerror='{0}'
                    url='{1}'
                    weburl='{2}' 
                    title='{3}' 
                    description='{4}' 
                    listtemplatename='{5}' 
                    doctemplateindex='{6}'/>
                </target>
            </project>";
		#endregion private members for project xml

		[SetUp]
		protected override void SetUp() {
			base.SetUp();
			try {
				DirectoryEntry root = new DirectoryEntry(_rootDirectory);
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
			catch {
			}
		}

		[TearDown]
		protected override void TearDown() {
			try 
			{
				DirectoryEntry root = new DirectoryEntry(_rootDirectory);
				DirectoryEntry userEntry = root.Children.Find(_userName);
				root.Children.Remove(userEntry);
				root.CommitChanges();
    
				SPSite site = new SPSite(_newSPSiteUrl);
				site.Delete(); 
			} catch  {
			}
		}

		
		/// <summary>
		/// Test adding a list to the defualt web site
		/// </summary>
		[Test]
		public void TestAddListToSpSiteDefaultWeb() {
			string[] parameters = new string[]{"true", 
												  _newSPSiteUrl,
												  _defaultWebUrl,
												  _listTitle, 
												  _listDescription, 
												  _listTemplateName, 
												  _docTemplateIndex,};
						
			string result = result = RunBuild(String.Format(
				CultureInfo.InvariantCulture, 
				_xmlProjectTemplate, 
				parameters));

			SPSite site = new SPSite(_newSPSiteUrl);

			SPWeb web = site.OpenWeb(_defaultWebUrl);
			
			bool listWasAdded = false;
			foreach(SPList list in web.Lists) {
				if (list.Title == _listTitle) {
					listWasAdded = true;
					break;
				}
			}
			
			Assertion.Assert("The list was not created.", listWasAdded);

			DeleteList(_defaultWebUrl);			
		}

		/// <summary>
		/// Test adding a list to an invalid web site
		/// </summary>
		[Test]		
		[ExpectedException(typeof(TestBuildException))]
		public void TestAddListToSpSiteInvalidWeb() {
			string[] parameters = new string[]{"true", 
												  _newSPSiteUrl,
												  _invalidWebUrl,
												  _listTitle, 
												  _listDescription, 
												  _listTemplateName, 
												  _docTemplateIndex,};
						
			string result = result = RunBuild(String.Format(
				CultureInfo.InvariantCulture, 
				_xmlProjectTemplate, 
				parameters));
		}

		/// <summary>
		/// Test adding a list with an invalid template name.
		/// </summary>
		[Test]
		[ExpectedException(typeof(TestBuildException))]
		public void TestAddListToSpSiteInvalidTemplateName(){
			string[] parameters = new string[]{"true", 
												  _newSPSiteUrl,
												  _defaultWebUrl,
												  _listTitle, 
												  _listDescription, 
												  _invalidTemplateName, 
												  _docTemplateIndex,};
						
			string result = result = RunBuild(String.Format(
				CultureInfo.InvariantCulture, 
				_xmlProjectTemplate, 
				parameters));
			
		}

		/// <summary>
		/// Test adding a list with an invalid docTemplateIndex.
		/// </summary>
		[Test]
		[ExpectedException(typeof(TestBuildException))]
		public void TestAddListToSpSiteInvalidDocTemplateIndex() {
			string[] parameters = new string[]{"true", 
												  _newSPSiteUrl,
												  _defaultWebUrl,
												  _listTitle, 
												  _listDescription, 
												  _listTemplateName, 
												  _invalidDocTemplateIndex,};
						
			string result = result = RunBuild(String.Format(
				CultureInfo.InvariantCulture, 
				_xmlProjectTemplate, 
				parameters));
		}

		/// <summary>
		/// Deletes site
		/// </summary>
		private void DeleteList(string webUrl) {
			SPSite site = new SPSite(_newSPSiteUrl);	
				
			SPWeb web = site.OpenWeb(webUrl);				
			
			foreach(SPList list in web.Lists) {
				if (list.Title == _listTitle) {
					web.Lists.Delete(list.ID);
					break;
				}
			}
			
		}
	}
}
