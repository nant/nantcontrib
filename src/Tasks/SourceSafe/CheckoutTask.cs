#region GNU General Public License
//
// NAntContrib
// Copyright (C) 2001-2003 Gerry Shaw
//
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

// Jason Reimer, Diversant Inc. (jason.reimer@diversant.net)
#endregion

using System;
using SourceSafeTypeLib;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.SourceSafe {
    /// <summary>
    /// Task used to checkout files from Visual Source Safe.
    /// </summary>
    /// <example>
    ///   <para>Checkout the latest files from a local sourcesafe database.</para>
    ///   <code><![CDATA[
    ///     <vsscheckout 
    ///       user="myusername" 
    ///       password="mypassword" 
    ///       localpath="C:\Dev\Latest"
    ///       recursive="true"
    ///       writable="true"
    ///       dbpath="C:\VSS\srcsafe.ini"
    ///       path="$/MyProduct"
    ///     />
    ///   ]]></code>
    /// </example>
    /// <example>
    ///   <para>Checkout a file from a remote sourcesafe database.  Put it in a relative directory.</para>
    ///   <code><![CDATA[
    ///     <vsscheckout 
    ///       user="myusername" 
    ///       password="mypassword" 
    ///       localpath="Latest"
    ///       recursive="false"
    ///       writable="true"
    ///       dbpath="\\MyServer\VSS\srcsafe.ini"
    ///       path="$/MyProduct/myFile.cs"
    ///     />
    ///   ]]></code>
    /// </example>
    [TaskName("vsscheckout")]
    public sealed class CheckoutTask : BaseTask {
        private string _localpath = "";
        private string _recursive = Boolean.TrueString;
        private string _writable = Boolean.TrueString;

        /// <summary>
        /// The absolute path to the local working directory. Required.
        /// </summary>
        [TaskAttribute("localpath", Required=true)]
        public string LocalPath {
            get { return _localpath; }
            set { _localpath = value; }
        }

        /// <summary>
        /// Determines whether to perform a recursive checkout. 
        /// Default value is true when omitted.
        /// </summary>
        [TaskAttribute("recursive")]
        [BooleanValidator]
        public string Recursive {
            get { return _recursive; }
            set { _recursive = value; }
        }

        /// <summary>
        /// Determines whether to leave the file(s) as writable. 
        /// Default value is true when omitted.
        /// </summary>
        [TaskAttribute("writable")]
        [BooleanValidator]
        public string Writable {
            get { return _writable; }
            set { _writable = value; }
        }

        protected override void ExecuteTask() {
            Open();

            /* -- Allowed flag categories --
             * GET, RECURS, USERO, CMPMETHOD, TIMESTAMP, EOL, REPLACE, 
             * FORCE, and CHKEXCLUSIVE
             */
            int flags = (Convert.ToBoolean(_recursive) ? Convert.ToInt32(RecursiveFlag) : 0) |
                (Convert.ToBoolean(_writable) ? Convert.ToInt32(VSSFlags.VSSFLAG_USERRONO) : Convert.ToInt32(VSSFlags.VSSFLAG_USERROYES));

            try {
                Item.Checkout("", _localpath, flags);
            } catch (Exception e) {
                throw new BuildException("Checkout failed", Location, e);
            }

            Log(Level.Info, "Checked out " + Path);
        }
    }
}
