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
	/// Summary description for DeleteListFromSPSiteTaskTest.
	/// </summary>
	[TestFixture]
	public class DeleteListFromSPSiteTaskTest: BuildTestBase {

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

		#region private members for test values for delete list
		private static string _defaultWebUrl = "";
		private string _listTitle = "One Crazy List To Delete";
		private string _listDescription = "This is the description for one Crazy List To Delete";
		private string _listTemplateName = "Discussion Board";
		private int _docTemplateIndex = 0;
		#endregion

		#region private members for project xml
		private static string _xmlProjectTemplateByTitle = 
			@"<project name='Delete List' default='deletelist'>
                <target name='deletelist'>
                    <deletelistfromspsite failonerror='{0}'
                    url='{1}'
                    weburl='{2}' 
                    title='{3}'/>
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
			try {
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
		/// Test deleting a list by list title
		/// </summary>		
		[Test]
		public void TestDeleteListFromSpSite() {

			Guid id = AddList();

			string[] parameters = new string[]{"true", 
												  _newSPSiteUrl,
												  _defaultWebUrl,
												  _listTitle,};
						
			string result = RunBuild(String.Format(
				CultureInfo.InvariantCulture, 
				_xmlProjectTemplateByTitle, 
				parameters));

			bool listWasDeleted = IsListDeleted(id);

			Assertion.Assert("The list was not deleted.", listWasDeleted);
			
		}

		/// <summary>
		/// Checks to see if the list was deleted
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		private bool IsListDeleted(Guid id) {
			bool rtnValue = true; 

			SPSite site = new SPSite(_newSPSiteUrl);

			SPWeb web = site.OpenWeb(_defaultWebUrl);

			foreach(SPList list in web.Lists) {
				if (list.ID == id) {
					rtnValue = false;
				}
			}
			return rtnValue;
		}

		/// <summary>
		/// Adds a list to be deleted
		/// </summary>
		private Guid AddList() {
			Guid  rtnValue = Guid.Empty;

			SPSite site = new SPSite(_newSPSiteUrl);

			SPWeb web = site.OpenWeb(_defaultWebUrl);

			rtnValue = web.Lists.Add(_listTitle,
				_listDescription,
				web.ListTemplates[_listTemplateName],
				web.DocTemplates[_docTemplateIndex]);
			
			return rtnValue;			
		}
	}
}
