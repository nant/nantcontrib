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

// Robert Jefferies (robert.jefferies@usbank.com)
#endregion

using System;
using System.IO;

using SourceSafeTypeLib;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.SourceSafe {
    /// <summary>
    /// Task is used to undo a checkout from SourceSafe
    /// </summary>
    /// <example>
    ///   <para>Undo a checkout of all of the files from a local sourcesafe database.</para>
    ///   <code><![CDATA[
    ///     <vssundocheckout 
    ///       user="myusername" 
    ///       password="mypassword" 
    ///       localpath="C:\Dev\Latest"
    ///       recursive="true"
    ///       dbpath="C:\VSS\srcsafe.ini"
    ///       path="$/MyProduct"
    ///     />
    ///   ]]></code>
    /// </example>
    /// <example>
    ///   <para>Checkout a file from a remote sourcesafe database.  Put it in a relative directory.</para>
    ///   <code><![CDATA[
    ///     <vssundocheckout 
    ///       user="myusername" 
    ///       password="mypassword" 
    ///       localpath="Latest"
    ///       recursive="false"
    ///       dbpath="\\MyServer\VSS\srcsafe.ini"
    ///       path="$/MyProduct/myFile.cs"
    ///     />
    ///   ]]></code>
    /// </example>
    [TaskName("vssundocheckout")]
    public sealed class UndoCheckoutTask : BaseTask {
        #region Private Instance Fields

        private bool _recursive = true;
        private DirectoryInfo _localPath;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The path to the local working directory. This is required if you wish to 
        /// have your local file replaced with the latest version from SourceSafe.
        /// </summary>
        [TaskAttribute("localpath", Required=false)]
        public DirectoryInfo LocalPath {
            get { return _localPath; }
            set { _localPath = value; }
        }

        /// <summary>
        /// Determines whether to perform a recursive undo of the checkout.
        /// The default is <see langword="true" />.
        /// </summary>
        [TaskAttribute("recursive")]
        [BooleanValidator]
        public bool Recursive {
            get { return _recursive; }
            set { _recursive = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        protected override void ExecuteTask() {
            Open();

            int flags = (Recursive ? Convert.ToInt32(RecursiveFlag) : 0);

            try {
                string localPath = LocalPath != null ? LocalPath.FullName : "";
                Item.UndoCheckout(localPath, flags);
            } catch (Exception ex) {
                throw new BuildException("The undo check-out operation failed.", 
                    Location, ex);
            }

            Log(Level.Info, "Undo of check-out completed for '{0}'.", Path);
        }

        #endregion Override implementation of Task
    }
}
