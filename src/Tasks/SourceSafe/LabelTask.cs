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
using System.Globalization;

using SourceSafeTypeLib;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.SourceSafe {
    /// <summary>
    /// Used to apply a label to a Visual Source Safe item.
    /// </summary>
    /// <example>
    ///   <para>Label all files in a local sourcesafe database. (Automatically applies the label recursively)</para>
    ///   <code><![CDATA[
    ///     <vsslabel 
    ///       user="myusername" 
    ///       password="mypassword" 
    ///       dbpath="C:\VSS\srcsafe.ini"
    ///       path="$/MyProduct"
    ///       comment="NAnt label"
    ///       label="myLabel"
    ///     />
    ///   ]]></code>
    /// </example>
    /// <example>
    ///   <para>Label a file in a remote sourcesafe database.</para>
    ///   <code><![CDATA[
    ///     <vsslabel 
    ///       user="myusername" 
    ///       password="mypassword" 
    ///       dbpath="\\MyServer\VSS\srcsafe.ini"
    ///       path="$/MyProduct/myFile.cs"
    ///       comment="NAnt label"
    ///       label="myLabel"
    ///     />
    ///   ]]></code>
    /// </example>
    [TaskName("vsslabel")]
    public sealed class LabelTask : BaseTask {
        #region Private Instance Fields

        private string _comment = "";
        private string _label;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The label comment.
        /// </summary>
        [TaskAttribute("comment")]
        public string Comment {
            get { return _comment; }
            set { _comment = value; }
        }

        /// <summary>
        /// The name of the label.
        /// </summary>
        [TaskAttribute("label", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Label {
            get { return _label; }
            set { _label = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        protected override void ExecuteTask() {
            Open();

            try {
                Item.Label(Label, Comment);
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Failed to apply label '{0}' to '{1}'.", Label, Path),
                    Location, ex);
            }

            Log(Level.Info, "Applied label '{0}' to '{1}'.", _label, Path);
        }

        #endregion Override implementation of Task
    }
}
