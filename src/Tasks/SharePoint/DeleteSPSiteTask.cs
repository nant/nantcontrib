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
using System.IO;
using System.Globalization;

using Microsoft.SharePoint;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace NAnt.SharePoint.Tasks {
    /// <summary>
    /// Deletes a SPSite.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   Deletes a SPSite specified by a Url on the local machine.
    ///   </para>
    ///   <note>
    ///   If the <see cref="Url" /> specified does not exist, a 
    ///   <see cref="BuildException" /> will be raised.
    ///   </note>
    /// </remarks>
    /// <example>
    ///   <para>Delete a SPSite.</para>
    ///   <code>
    ///     <![CDATA[
    /// <deletespsite Url="http://myserver/sites/mysite" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Delete a SPSite. If the SPSite does not exist, the task does nothing.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <deletespsite Url="${url}" failonerror="false" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("deletespsite")]
    public class DeleteSPSiteTask : Task {

        private string _url = "";

        /// <summary>
        /// The URL for the site to be created.
        /// </summary>
        [TaskAttribute("url")]
        public string Url {
            get { return _url; }
            set { _url = value; }
        }

        /// <summary>
        /// Task for deleting a SPSite.
        /// </summary>
        protected override void ExecuteTask() {
            try {
                SPSite site = new SPSite(Url);
                site.Delete();

                Log(Level.Info, LogPrefix + "Deleting site {0}.", Url);
            } catch (FileNotFoundException ex) { 
                // The SPS API will throw an exception when you try and create an 
                // instance of SPSite for a URL that doesn't exist.  
                throw new BuildException( 
                    string.Format("Cannot delete site {0}. The site does not exist.", 
                    Url), Location, ex);
            }
        }
    }
}
