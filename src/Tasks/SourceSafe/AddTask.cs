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

// Rob Jefferies (rob@houstonsigamchi.org)
#endregion

using System;
using SourceSafeTypeLib;
using NAnt.Core;
using NAnt.Core.Types;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.SourceSafe {
    /// <summary>
    /// Used to add files to a Visual SourceSafe database.  If the file is currently
    /// in the SourceSafe database a message will be logged but files will continue to be added.
    /// </summary>
    /// <remarks>    
    /// This version does not support recursive adds.  Only adds in the root directory will be added to the
    /// SourceSafe database.
    /// </remarks>
    /// <example>
    ///   <code><![CDATA[
    ///     <vssadd dbpath="C:\SourceSafeFolder\srcsafe.ini" user="user1" password="" path="$/Somefolder">
    ///       <fileset basedir="C:\SourceFolder\">  
    ///         <includes name="*.dll"/>
    ///       </fileset>
    ///     </vssadd>
    ///   ]]></code>
    /// </example>
    [TaskName("vssadd")]
    public sealed class AddTask : BaseTask {

        string _comment = "";
        FileSet _fileset=new FileSet();

        /// <summary>
        /// Places a comment on all files added into the SourceSafe repository.  Not Required.
        /// </summary>
        [TaskAttribute("comment",Required=false)]
        public string Comment {
            get { return _comment;}
            set {_comment = value;}
        }

        /// <summary>
        /// List of files that should be added to SourceSafe.  Note: Recursive adds not supported.
        /// </summary>
        [BuildElement("fileset")]
        public FileSet AddFileSet {
            get { return _fileset; }
            set { _fileset = value; }
        }

        protected override void ExecuteTask() {
            Open();
            
            const int FILE_ALREADY_ADDED = -2147166572;
          
            // Attempt to add each file to SourceSafe.  If file is already in SourceSafe
            // then log a message and continue otherwise throw an exception.
            foreach( string currentFile in _fileset.FileNames ) {
                try {
                    Item.Add(currentFile,_comment,0);
                    Log(Level.Info, LogPrefix + "Added File : " + currentFile);       
                } catch (System.Runtime.InteropServices.COMException e) {
                    if (e.ErrorCode  == FILE_ALREADY_ADDED) {
                        Log(Level.Info, LogPrefix + "File already added : " + currentFile);                                         
                        // just continue here 
                    } else {                                             
                        throw new BuildException("Adding files to SourceSafe failed.", Location, e);                                                     
                    }
                } 
                    // All other exceptions
                catch (Exception e) {                                            
                    throw new BuildException("Adding files to SourceSafe failed.", Location, e);                                                                       
                }
            }
        }              
    }
}
