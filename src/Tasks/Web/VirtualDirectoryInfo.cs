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
// Gert Driesen (gert.driesen@ardatis.com)

using System;
using System.Collections;
using System.DirectoryServices;
using System.Globalization;
using System.IO;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.Web {
    /// <summary>
    /// Lists the configuration settings of a specified virtual directory in a
    /// web site hosted on Internet Information Server.
    /// </summary>
    /// <example>
    ///   <para>
    ///   List the settings of a virtual directory named <c>Temp</c>.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <iisdirinfo vdirname="Temp" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("iisdirinfo")]
    public class VirtualDirectoryInfo : WebBase {
        protected override void ExecuteTask() {
            Log(Level.Info, "Retrieving settings of virtual directory '{0}'"
                + " on '{1}'.", this.VirtualDirectory, this.Server);

            // ensure IIS is available on specified host and port
            this.CheckIISSettings();

            try {
                // retrieve DirectoryEntry representing root of web site
                DirectoryEntry folderRoot = new DirectoryEntry(this.ServerPath);
                folderRoot.RefreshCache();

                // locate DirectoryEntry representing virtual directory
                DirectoryEntry newVirDir = folderRoot.Children.Find(this.VirtualDirectory, folderRoot.SchemaClassName);

                // output all properties of virtual directory
                foreach (string propertyName in newVirDir.Properties.PropertyNames) {
                    object propertyValue = newVirDir.Properties[propertyName].Value;

                    if (propertyValue.GetType().IsArray) {
                        Log(Level.Info, '\t' + propertyName + ":");
                        Array propertyValues = (Array) propertyValue;
                        foreach (object value in propertyValues) {
                            Log(Level.Info, "\t\t" + value.ToString());
                        }
                    } else {
                        Log(Level.Info, '\t' + propertyName + ": " 
                            + newVirDir.Properties[propertyName].Value.ToString());
                    }
                }

                newVirDir.Close();
                folderRoot.Close();
            } catch (BuildException) {
                // re-throw exception
                throw;
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Error retrieving info for virtual directory '{0}' on '{1}'.", 
                    this.VirtualDirectory, this.Server), Location, ex);
            }
        }
    }
}
