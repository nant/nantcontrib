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

namespace NAnt.SharePoint {
    /// <summary>
    /// Creates a SPSite.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   Creates a SPSite identified by the <see cref="Url" /> on the local machine.
    ///   </para>
    ///   <note>
    ///   If the <see cref="Url" /> is not valid, a 
    ///   <see cref="BuildException" /> will be raised.
    ///   </note>
    /// </remarks>
    /// <example>
    ///   <para>Create a SPSite.</para>
    ///   <code>
    ///     <![CDATA[
    /// <createspsite url="http://SPS2K3"
    ///    abbreviation="TS1" 
    ///    title="Test Site 1" 
    ///    description="Test Site for the NAnt Create Task." 
    ///    template="STS" 
    ///    managedpath="sites"
    ///    ownerlogin="SPS2K3\donald" 
    ///    ownername="Donald Duck" 
    ///    owneremail="donald.duck@disney.com" 
    ///    contactlogin="SPS2K3\daisy" 
    ///    contactname="Daisy Duck" 
    ///    contactemail="daisy.duck@disney.com"/>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("createspsite")]
    public class CreateSPSiteTask : Task {
        #region private member variables
        private string _url = "";

        private string _abbreviation = "";

        private string _title = "";

        private string _description = "";

        private string _template = "";

        private string _managedPath = "";

        private string _ownerLogin = "";

        private string _ownerName = "";

        private string _ownerEmail = "";

        private string _contactLogin = "";

        private string _contactName = "";

        private string _contactEmail = "";
        #endregion private member variables

        #region Properties
        /// <summary>
        /// The URL for the site to be created.
        /// </summary>
        [TaskAttribute("url")]
        public string Url {
            get { return _url; }
            set { _url = value; }
        }

        /// <summary>
        /// The abbreviation for the site to be created.
        /// </summary>
        [TaskAttribute("abbreviation")]
        public string Abbreviation {
            get { return _abbreviation; }
            set { _abbreviation = value; }
        }

        /// <summary>
        /// The title for the site to be created.
        /// </summary>
        [TaskAttribute("title")]
        public string Title {
            get { return _title; }
            set { _title = value; }
        }

        /// <summary>
        /// The description for the site to be created.
        /// </summary>
        [TaskAttribute("description")]
        public string Description {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// The SharePoint template for the site to be created.
        /// </summary>
        [TaskAttribute("template")]
        public string Template {
            get { return _template; }
            set { _template = value; }
        }

        /// <summary>
        /// The SharePoint managed path that the site will be contained in.
        /// </summary>
        [TaskAttribute("managedpath")]
        public string ManagedPath {
            get { return _managedPath; }
            set { _managedPath = value; }
        }

        /// <summary>
        /// The login name of the owner for the site to be created.
        /// </summary>
        [TaskAttribute("ownerlogin")]
        public string OwnerLogin {
            get { return _ownerLogin; }
            set { _ownerLogin = value; }
        }

        /// <summary>
        /// The name of the owner for the site to be created.
        /// </summary>
        [TaskAttribute("ownername")]
        public string OwnerName {
            get { return _ownerName; }
            set { _ownerName = value; }
        }

        /// <summary>
        /// The loging name of the owner for the site to be created.
        /// </summary>
        [TaskAttribute("owneremail")]
        public string OwnerEmail {
            get { return _ownerEmail; }
            set { _ownerEmail = value; }
        }

        /// <summary>
        /// The login name of the contact for the site to be created.
        /// </summary>
        [TaskAttribute("ownerlogin")]
        public string ContactLogin {
            get { return _contactLogin; }
            set { _contactLogin = value; }
        }

        /// <summary>
        /// The name of the contact for the site to be created.
        /// </summary>
        [TaskAttribute("ownername")]
        public string ContactName {
            get { return _contactName; }
            set { _contactName = value; }
        }

        /// <summary>
        /// The loging name of the contact for the site to be created.
        /// </summary>
        [TaskAttribute("owneremail")]
        public string ContactEmail {
            get { return _contactEmail; }
            set { _contactEmail = value; }
        }
        #endregion Properties

        /// <summary>
        /// Task for creating a SPSite.
        /// </summary>
        protected override void ExecuteTask() {        
            SPSite serverSite = new SPSite(Url);
            string newSiteUrl = string.Format("{0}/{1}/{2}", Url, _managedPath, _abbreviation);
            try {
                SPSite newSite = serverSite.SelfServiceCreateSite(
                    newSiteUrl,
                    Title,
                    Description,
                    serverSite.RootWeb.Language,
                    Template,
                    OwnerLogin,
                    OwnerName,
                    OwnerEmail,
                    ContactLogin,
                    ContactName,
                    ContactEmail);
                Log(Level.Info, LogPrefix + "Creating site {0}.", newSiteUrl);
            } catch (SPException ex) { 
                throw new BuildException( 
                    string.Format("Cannot create site {0}. The site does not exist.", 
                    Url), Location, ex);
            }
        }
    }
}
