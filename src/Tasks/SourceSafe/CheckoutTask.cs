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
using System.IO;

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
        #region Private Instance Fields

        private DirectoryInfo _localPath;
        private bool _recursive = true;
        private bool _writable = true;
        private FileTimestamp _fileTimestamp = FileTimestamp.Current;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The path to the local working directory.
        /// </summary>
        [TaskAttribute("localpath", Required=true)]
        public DirectoryInfo LocalPath {
            get { return _localPath; }
            set { _localPath = value; }
        }

        /// <summary>
        /// Determines whether to perform a recursive checkout.
        /// The default is <see langword="true" />.
        /// </summary>
        [TaskAttribute("recursive")]
        [BooleanValidator]
        public bool Recursive {
            get { return _recursive; }
            set { _recursive = value; }
        }

        /// <summary>
        /// Determines whether to leave the file(s) as writable. 
        /// The default is <see langword="true" />.
        /// </summary>
        [TaskAttribute("writable")]
        [BooleanValidator]
        public bool Writable {
            get { return _writable; }
            set { _writable = value; }
        }


        /// <summary>
        /// Set the behavior for timestamps of local files. The default is
        /// <see cref="F:FileTimestamp.Current" />.
        /// </summary>
        [TaskAttribute("filetimestamp")]
        public FileTimestamp FileTimestamp {
            get { return _fileTimestamp; }
            set { _fileTimestamp = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        protected override void ExecuteTask() {
            Open();

            /* -- Allowed flag categories --
             * GET, RECURS, USERO, CMPMETHOD, TIMESTAMP, EOL, REPLACE, 
             * FORCE, and CHKEXCLUSIVE
             */
            int flags = (Recursive ? Convert.ToInt32(RecursiveFlag) : 0) |
                (Writable ? Convert.ToInt32(VSSFlags.VSSFLAG_USERRONO) : Convert.ToInt32(VSSFlags.VSSFLAG_USERROYES))
                | GetFileTimestampFlags(FileTimestamp);

            try {
                switch (Item.Type) {
                    case (int) VSSItemType.VSSITEM_PROJECT:
                        Item.Checkout("", LocalPath.FullName, flags);
                        break;
                    case (int) VSSItemType.VSSITEM_FILE:
                        string filePath = System.IO.Path.Combine(LocalPath.FullName, 
                            Item.Name);
                        Item.Checkout("", filePath, flags);
                        break;
                }

            } catch (Exception ex) {
                throw new BuildException("The check-out operation failed.", 
                    Location, ex);
            }

            Log(Level.Info, "Checked out '{0}'.", Path);
        }

        #endregion Override implementation of Task
    }
}
