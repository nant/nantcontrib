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

using Microsoft.SharePoint;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;
namespace NAnt.SharePoint.Tasks
{
	/// <summary>
	/// Adds a SPList
	/// </summary>
	/// <remarks>
	///   <para>
	///   Creates a SPList to the identified SPWeb by <see cref="WebUrl"/> on 
	///   the identified SPSite by <see cref="Url"/>
	///   </para>
	///   <note>
	///   If the <see cref="Url" /> is not valid, a 
	///   <see cref="BuildException" /> will be raised.
	///   </note>
	///   <note>
	///   If the <see cref="WebUrl" /> is not valid, a 
	///   <see cref="BuildException" /> will be raised.
	///   </note>
	/// </remarks>
	/// <example>
	///   <para>Create a SPList.</para>
	///   <code>
	///     <![CDATA[
	///		<addlisttospsite 
	///     url="http://SPS2K3" 
	///     weburl="MySPWebSite"
	///     title="MySPList"
	///     description="Description of the SPList"
	///     listtemplatename="ListTemplateName"
	///     doctemplateindex="0"/>
	///     ]]>
	///   </code>
	/// </example>
	[TaskName("addlistospsite")]
	public class AddListToSPSiteTask: Task {

		#region private member variables
		private string _url = string.Empty;
		private string _webUrl = string.Empty;
		private string _title = string.Empty;
		private string _description = string.Empty;
		private string _listTemplateName = string.Empty;
		private int _docTemplateIndex = int.MinValue;
		#endregion

		#region properties
		/// <summary>
		/// The URL for the site to add the list to.
		/// </summary>
		[TaskAttribute("url")]
		public string Url {
			get{return _url;}
			set{_url = value;}
		}

		/// <summary>
		/// The URL for the web site to add the list to.
		/// </summary>
		[TaskAttribute("weburl")]
		public string WebUrl {
			get{return _webUrl;}
			set{_webUrl = value;}
		}
		
		/// <summary>
		/// The title of the list
		/// </summary>
		[TaskAttribute("title")]
		public string Title {
			get{return _title;}
			set{_title = value;}
		}

		/// <summary>
		/// The description of the list
		/// </summary>
		[TaskAttribute("description")]
		public string Description {
			get{return _description;}
			set{_description = value;}
		}

		/// <summary>
		/// The name of the ListTemplate
		/// </summary>
		[TaskAttribute("listtemplatename")]
		public string ListTemplateName {
			get{return _listTemplateName;}
			set{_listTemplateName = value;}
		}

		/// <summary>
		/// The index of the DocTemplateIndex
		/// </summary>
		[TaskAttribute("doctemplateindex")]
		public int DocTemplateIndex {
			get{return _docTemplateIndex;}
			set{_docTemplateIndex = value;}
		}
		#endregion

		/// <summary>
		/// Task for adding a SPRole to a site.
		/// </summary>
		protected override void ExecuteTask() {        
			try {			
				SPSite site = new SPSite(Url);	
				
				SPWeb web = site.OpenWeb(WebUrl);				
			
				SPListTemplate listTemplate = GetListTemplate(web);
				SPDocTemplate docTemplate = GetDocTemplate(web);

				web.Lists.Add(
					Title,
					Description,
					listTemplate,
					docTemplate);
			}
			catch (SPException ex) { 

				throw new BuildException( 
					string.Format("Cannot add list: {0} to site: {1}.", 
					Title, Url), Location, ex);
			}
			catch(System.IO.FileNotFoundException fnf) {
				throw new BuildException( 
					string.Format("Cannot add list: {0} to invalid web url: {1}.", 
					Title, WebUrl), Location, fnf);
			}
		}
		
		/// <summary>
		/// Gets a <see cref="SPListTemplate"/> by <see cref="ListTemplateName"/>
		/// </summary>
		/// <param name="web"></param>
		/// <returns></returns>
		private SPListTemplate GetListTemplate(SPWeb web) {
			SPListTemplate listTemplate = null;
			
			if (web!=null) {
				listTemplate = web.ListTemplates[ListTemplateName];
			}

			return listTemplate;
		}

		/// <summary>
		/// Gets a <see cref="SPDocTemplate"/> by <see cref="DocTemplateIndex"/>
		/// </summary>
		/// <param name="web"></param>
		/// <returns></returns>
		private SPDocTemplate GetDocTemplate(SPWeb web) {
			SPDocTemplate docTemplate = null;

			if (web!=null) {
				docTemplate = web.DocTemplates[DocTemplateIndex];
			}
			return docTemplate;
		}

	}
}
