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
// William E. Caputo, ThoughtWorks Inc. (billc@thoughtworks.com)
#endregion

using System;
using System.Collections;
using System.IO;

using SourceSafeTypeLib;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.SourceSafe {
    /// <summary>
    /// Used to retreive an item or project from a Visual Source Safe database.
    /// </summary>
    /// <example>
    ///   <para>Get the latest files from a local sourcesafe database.</para>
    ///   <code><![CDATA[
    ///     <vssget
    ///       user="myusername"
    ///       password="mypassword"
    ///       localpath="C:\Dev\Latest"
    ///       recursive="true"
    ///       replace="true"
    ///       writable="true"
    ///       dbpath="C:\VSS\srcsafe.ini"
    ///       path="$/MyProduct"
    ///     />
    ///   ]]></code>
    /// </example>
    /// <example>
    ///   <para>Get the latest version of a file from a remote sourcesafe database.  Put it in a relative directory.</para>
    ///   <code><![CDATA[
    ///     <vssget
    ///       user="myusername"
    ///       password="mypassword"
    ///       localpath="Latest"
    ///       recursive="true"
    ///       replace="true"
    ///       writable="true"
    ///       dbpath="\\MyServer\VSS\srcsafe.ini"
    ///       path="$/MyProduct/myFile.cs"
    ///     />
    ///   ]]></code>
    /// </example>
    /// <example>
    ///   <para>Get the latest version of a file from a remote sourcesafe database. Remove any deleted files from local image.</para>
    ///   <code><![CDATA[
    ///     <vssget
    ///       user="myusername"
    ///       password="mypassword"
    ///       localpath="C:\Dev\Latest"
    ///       recursive="true"
    ///       replace="true"
    ///       writable="true"
    ///       removedeleted="true"
    ///       dbpath="\\MyServer\VSS\srcsafe.ini"
    ///       path="$/MyProduct/myFile.cs"
    ///     />
    ///   ]]></code>
    /// </example>
    [TaskName("vssget")]
    public sealed class GetTask : BaseTask {

        string _localpath = null;
        string _recursive = Boolean.TrueString;
        string _replace = Boolean.FalseString;
        string _writable = Boolean.FalseString;
        string _removeDeleted = Boolean.FalseString;
        string _useModTime = Boolean.FalseString;

        private long _deleteCount = 0;

        /// <summary>
        /// The absolute path to the local working directory. Required.
        /// </summary>
        [TaskAttribute("localpath", Required=true)]
        public string LocalPath {
            get { return _localpath; }
            set { _localpath = value; }
        }

        /// <summary>
        /// Determines whether to perform the get recursively.
        /// Default value is true when omitted.
        /// </summary>
        [TaskAttribute("recursive")]
        [BooleanValidator()]
        public string Recursive {
            get { return _recursive; }
            set { _recursive = value; }
        }

        /// <summary>
        /// Determines whether to replace writable files.
        /// Default value is false when omitted.
        /// </summary>
        [TaskAttribute("replace")]
        [BooleanValidator()]
        public string Replace {
            get { return _replace; }
            set { _replace = value; }
        }

        /// <summary>
        /// Determines whether the files will be read-only.
        /// Default value is false when omitted.
        /// </summary>
        [TaskAttribute("writable")]
        [BooleanValidator()]
        public string Writable {
            get { return _writable; }
            set { _writable = value; }
        }

        /// <summary>
        /// Determines whether files marked "deleted" in the
        /// repository will be removed from the local copy.
        /// Default value is false when omitted.
        /// </summary>
        [TaskAttribute("removedeleted")]
        [BooleanValidator()]
        public string RemoveDeleted {
            get { return _removeDeleted; }
            set { _removeDeleted = value; }
        }

        /// <summary>
        /// Determines whether the timestamp on the local copy
        /// will be the modification time (if false or omitted, 
        /// the checkout time will be used)
        /// </summary>
        [TaskAttribute("usemodtime")]
        [BooleanValidator()]
        public string UseModificationTime {
            get { return _useModTime; }
            set { _useModTime = value; }
        }

        protected override void ExecuteTask() {
            Open();

            /* -- Allowed flag categories --
             * RECURS, USERO, CMPMETHOD, TIMESTAMP, EOL, REPLACE, and FORCE
             */
            int flags = (Convert.ToBoolean(_recursive) ? Convert.ToInt32(RecursiveFlag) : 0) |
                (Convert.ToBoolean(_writable) ? Convert.ToInt32(VSSFlags.VSSFLAG_USERRONO) : Convert.ToInt32(VSSFlags.VSSFLAG_USERROYES)) |
                (Convert.ToBoolean(_replace) ? Convert.ToInt32(VSSFlags.VSSFLAG_REPREPLACE) : 0) |
                (Convert.ToBoolean(_useModTime) ? Convert.ToInt32(VSSFlags.VSSFLAG_TIMEMOD) : 0);

            // Get the version to the local path
            try {
                Item.Get(ref _localpath, flags);
            }
            catch (Exception e) {
                throw new BuildException("vssget failed", Location, e);
            }

            Log(Level.Info, LogPrefix + "Put " + Path + " to " + _localpath);

            RemoveDeletedFromLocalImage();
        }

        /// <summary>
        /// Checks to see if we should remove local copies of deleted files, and starts
        /// the scan.
        /// </summary>
        public void RemoveDeletedFromLocalImage() {
            if(Convert.ToBoolean(RemoveDeleted)) {
                Log(Level.Info, LogPrefix + "Removing deleted files from local image...");
                RemoveDeletedFromLocalImage(Item, LocalPath);
                Log(Level.Info, LogPrefix + String.Format("Removed: {0} deleted files from local image.", _deleteCount));
            }
        }

        /// <summary>
        /// Scans the Project Item for deleted files and removes their local
        /// copies from the local image of the project. Obeys the recursive setting
        /// (and thus optionally calls itself recursively).
        /// </summary>
        /// <param name="item">The VSS Item (project) to check for deletions</param>
        /// <param name="localPathPrefix">The path to the folder of the item being processed</param>
        public void RemoveDeletedFromLocalImage(IVSSItem item, string localPathPrefix) {
            IVSSItems items = item.get_Items(true);

            Hashtable deletedTable = BuildDeletedTable(items);
            
            IEnumerator ie = items.GetEnumerator();

            while(ie.MoveNext()) {
                IVSSItem i = (IVSSItem) ie.Current;
                string localPath = System.IO.Path.Combine(localPathPrefix, i.Name);

                if(IsTrulyDeleted(deletedTable, i) && Exists(localPath)) {
                    SetToWriteable(localPath);
                    Delete(localPath);
                } else {
                    if(IsProject(i) && DeletionIsRecursive()) {
                        RemoveDeletedFromLocalImage(i, System.IO.Path.Combine(localPathPrefix, i.Name) );
                    }
                }
            }
        }

        public Hashtable BuildDeletedTable(IVSSItems items) {
            Hashtable result = new Hashtable(new CaseInsensitiveHashCodeProvider(), new CaseInsensitiveComparer());

            IEnumerator ie = items.GetEnumerator();
            while(ie.MoveNext()) {
                IVSSItem item = (IVSSItem) ie.Current;
                bool currentState = result[item.Name] == null ? true : (bool) result[item.Name];
                result[item.Name] = currentState & item.Deleted;
            }

            return result;
        }

        private bool IsTrulyDeleted(Hashtable deletedTable, IVSSItem item) {
            return (bool) deletedTable[item.Name];
        }

        private bool Exists(string path) {
            return (File.Exists(path) || Directory.Exists(path));
        }

        private void SetToWriteable(string path) {
            File.SetAttributes(path, FileAttributes.Normal);

            if(IsDirectory(path)) {
                foreach(string entry in Directory.GetFileSystemEntries(path)) {
                    SetToWriteable(entry);
                }
            }
        }

        private bool IsDirectory(string path) {
            return (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
        }

        private void Delete(string path) {
            _deleteCount++;
            Log(Level.Verbose, LogPrefix + "Deleting: " + path);
            if(IsDirectory(path)) {
                Directory.Delete(path, true);
            } else {
                File.Delete(path);
            }
        }

        private bool IsProject(IVSSItem item) {
            return item.Type == (int) VSSItemType.VSSITEM_PROJECT;
        }

        private bool DeletionIsRecursive() {
            return Convert.ToBoolean(Recursive);
        }
    }
}
