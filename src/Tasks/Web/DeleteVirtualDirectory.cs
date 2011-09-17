// NAntContrib
// Copyright (C) 2002 Brian Nantz bcnantz@juno.com
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307 USA
//
// Brian Nantz (bcnantz@juno.com)
// Gert Driesen (drieseng@users.sourceforge.net)

using System;
using System.Collections;
using System.DirectoryServices;
using System.Globalization;
using System.IO;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.Web {
    /// <summary>
    /// Deletes a virtual directory from a given web site hosted on Internet 
    /// Information Server.
    /// </summary>
    /// <example>
    ///   <para>
    ///   Delete a virtual directory named <c>Temp</c> from the web site running
    ///   on port <c>80</c> of the local machine. If more than one web site is
    ///   running on port <c>80</c>, take the web site bound to the hostname 
    ///   <c>localhost</c> if existent or bound to no hostname otherwise.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <deliisdir vdirname="Temp" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Delete a virtual directory named <c>Temp</c> from the website running 
    ///   on port <c>81</c> of machine <c>MyHost</c>.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <deliisdir iisserver="MyHost:81" vdirname="Temp" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("deliisdir")]
    public class DeleteVirtualDirectory : WebBase {
        protected override void ExecuteTask() {
            // ensure IIS is available on specified host and port
            this.CheckIISSettings();

            Log(Level.Info, "Deleting virtual directory '{0}' on '{1}' (website: {2}).", 
                this.VirtualDirectory, this.Server, this.Website);

            //make sure we dont delete the serverinstance ROOT
            if (this.VirtualDirectory.Length == 0)
                throw new BuildException("The root of a web site can not be deleted",Location);

            // skip further processing if virtual directory no longer exists
            if (!DirectoryEntryExists(this.ServerPath + this.VdirPath))
                return;

            try {
                DirectoryEntry vdir = new DirectoryEntry(this.ServerPath + this.VdirPath);
                vdir.Parent.Children.Remove(vdir);
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Error deleting virtual directory '{0}' on '{1}' (website: {2}).", 
                    this.VirtualDirectory, this.Server, this.Website), Location, ex);
            }
        }
    }
}
