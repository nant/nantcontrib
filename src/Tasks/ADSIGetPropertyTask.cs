// NAnt - A .NET build tool
// Copyright (C) 2002 Galileo International
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// Gordon Weakliem (gordon.weakliem@galileo.com)
// 

using System;
using System.DirectoryServices; 
using System.Globalization;

using NAnt.Core;
using NAnt.Core.Types;
using NAnt.Core.Tasks;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Used to get the value of a property from an ADSI object.
    /// </summary>
    [TaskName("adsigetprop")]
    public class ADSIGetPropertyTask : ADSIBaseTask {
        #region Private Instance Fields

        private string _propName;
        private string _storeIn;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The name of the property to get.
        /// </summary>
        [TaskAttribute("propname", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public String PropName {
            get { return _propName; }
            set { _propName = value; }
        }

        /// <summary>
        /// The name of the property to store the value in.
        /// </summary>
        [TaskAttribute("storein", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public String StoreIn {
            get { return _storeIn; }
            set { _storeIn = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        /// <summary>
        /// Sets the specified property
        /// </summary>
        protected override void ExecuteTask() {
            try {
                // Get the directory entry for the specified path and set the 
                // property.
                using (DirectoryEntry pathRoot = new DirectoryEntry(Path)) {
                    pathRoot.RefreshCache();

                    if (pathRoot.Properties[PropName].Value.GetType().IsArray) {
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        foreach (object propValue in (Array) pathRoot.Properties[PropName].Value) {
                            sb.AppendFormat("{0}" + Environment.NewLine, propValue);
                        }
                        Project.Properties[StoreIn] = sb.ToString();
                    } else {
                        Project.Properties[StoreIn] = pathRoot.Properties[PropName].Value.ToString();
                    }
                }
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Error reading property '{0}'.", PropName), ex);
            }
        }

        #endregion Override implementation of Task
    }
}
