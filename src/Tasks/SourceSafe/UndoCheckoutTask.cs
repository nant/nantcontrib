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
	///	    <vssundocheckout 
	///	      user="myusername" 
	///	      password="mypassword" 
	///	      localpath="C:\Dev\Latest"
	///	      recursive="true"
	///	      dbpath="C:\VSS\srcsafe.ini"
	///	      path="$/MyProduct"
	///	    />
	///   ]]></code>
	/// </example>
	/// <example>
	///   <para>Checkout a file from a remote sourcesafe database.  Put it in a relative directory.</para>
	///   <code><![CDATA[
	///	    <vssundocheckout 
	///	      user="myusername" 
	///	      password="mypassword" 
	///	      localpath="Latest"
	///	      recursive="false"
	///	      dbpath="\\MyServer\VSS\srcsafe.ini"
	///	      path="$/MyProduct/myFile.cs"
	///	    />
	///   ]]></code>
	/// </example>
	[TaskName("vssundocheckout")]
    public sealed class UndoCheckoutTask : BaseTask {
        
        string _recursive = Boolean.TrueString;
        string _localpath = ""; 

        /// <summary>
        /// The absolute path to the local working directory. This is required if you wish to 
        /// have your local file replaced with the latest version from SourceSafe.
        /// </summary>
        [TaskAttribute("localpath", Required=false)]
        public string LocalPath {
            get { return _localpath; }
            set { _localpath = value; }
        }

        /// <summary>
        /// Determines whether to perform a recursive UndoCheckOut. 
        /// Default value is true when omitted.
        /// </summary>
        [TaskAttribute("recursive")]
        [BooleanValidator]
        public string Recursive {
            get { return _recursive; }
            set { _recursive = value; }
        }

        protected override void ExecuteTask() {
            Open();
            
            int flags = (Convert.ToBoolean(_recursive) ? Convert.ToInt32(RecursiveFlag) : 0);

            try {
                Log(Level.Info, LogPrefix + "localpath : " + _localpath);
                Item.UndoCheckout(_localpath,flags);
            }
            catch (Exception e) {
                throw new BuildException("UndoCheckout failed", Location, e);
            }

            Log(Level.Info, LogPrefix + "UndoCheckOut " + Path);
        }		
    }
}
