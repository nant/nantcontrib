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
// Richard Adleta (richardadleta@yahoo.com)
#endregion

using System;
using System.IO;

using SourceSafeTypeLib;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.SourceSafe {
    /// <summary>
    /// Used to delete or Destroy files or projects in Visual Source Safe.
    /// </summary>
    /// <example>
    ///   <para>Delete a project from a local sourcesafe database.</para>
    ///   <code><![CDATA[
    ///     <vssdelete 
    ///       user="myusername" 
    ///       password="mypassword" 
    ///       dbpath="C:\VSS\srcsafe.ini"
    ///       path="$/MyProduct"
    ///     />
    ///   ]]></code>
    /// </example>
    /// <example>
    ///   <para>Delete a file from the remote sourcesafe database.</para>
    ///   <code><![CDATA[
    ///     <vsscheckin 
    ///       user="myusername" 
    ///       password="mypassword" 
    ///       dbpath="\\MyServer\VSS\srcsafe.ini"
    ///       path="$/MyProduct/myFile.cs"
    ///     />
    ///   ]]></code>
    /// </example>
    /// <example>
    ///   <para>Destroy a project from a local sourcesafe database.</para>
    ///   <code><![CDATA[
    ///     <vssdelete 
    ///       user="myusername" 
    ///       password="mypassword" 
    ///       dbpath="C:\VSS\srcsafe.ini"
    ///       path="$/MyProduct"
    ///       Destroy="true"
    ///     />
    ///   ]]></code>
    /// </example>
    /// <example>
    ///   <para>Destroy a file from the remote sourcesafe database.</para>
    ///   <code><![CDATA[
    ///     <vssdelete 
    ///       user="myusername" 
    ///       password="mypassword" 
    ///       dbpath="\\MyServer\VSS\srcsafe.ini"
    ///       path="$/MyProduct/myFile.cs"
    ///       Destroy="true"
    ///     />
    ///   ]]></code>
    /// </example>
    [TaskName("vssdelete")]
    public sealed class DeleteTask : BaseTask {
        #region Private Instance Fields
        
        private bool _destroy;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// Determines whether or not the item is Destroyed. 
        /// The default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("destroy")]
        [BooleanValidator()]
        public bool Destroy { 
            get { return _destroy; }
            set { _destroy = value; }
        }
        
        #endregion Public Instance Properties

        #region Public Instance Methods

        /// <summary>
        /// Deletes the item unless <see cref="Destroy"/> is <see langword="true" />
        /// then the item is destroyed.
        /// </summary>
        public void DeleteItem() {
            if (Destroy) {
                try {
                    Item.Destroy();
                    Log(Level.Info, "Destroyed '{0}'.", Path);
                } catch (Exception ex) {
                    throw new BuildException("The destroy operation failed.", 
                        Location, ex);
                }
            } else {
                try {
                    Item.Deleted = true;
                    Log(Level.Info, "Deleted '{0}'.", Path);
                } catch (Exception ex) {
                    throw new BuildException("The delete operation failed.", 
                        Location, ex);
                }
            }
        }

        #endregion Public Instance Methods

        protected override void ExecuteTask() {            
            Open();

            this.DeleteItem();
        }
    }
}