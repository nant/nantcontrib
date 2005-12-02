//
// NAntContrib
// Copyright (C) 2001-2005 Gerry Shaw
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

// Tim Armbruster http://www.tfarmbruster.com

using System;
using System.Collections.Specialized; 
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;
using NAnt.Core.Types;
using NAnt.Core.Util;
using NAnt.Contrib.Util;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Reads String values in INI files.
    /// </summary>
    ///   <para>Reads the value for <c>AutoRefresh</c> in the <c>MS Transaction Server</c> section 
    ///   of the <c>VendorMISMO2.ini</c> ini file.  Stores the value in the "myvar" property.</para>
    ///   <example>
    ///    <code>
    ///     <![CDATA[
    /// <iniread property="myvar" filename="VendorMISMO2.ini" section="MS Transaction Server" key="AutoRefresh" default="1"/>
    ///     ]]>
    ///   </code>
    ///   <para>The file contents look like this:</para>
    ///   <code>
    ///   [MS Transaction Server]
    ///   Executable="VendorMISMO2.dll"
    ///   AutoRefresh=1
    ///   </code>
    /// </example>
    [TaskName("iniread")]
    public class IniReadTask : Task {
        
        #region Private Instance Fields

        private string _iniFile = null;
        private string _key = null;
        private string _default = null;
        private string _section = null;
        private string _property = null;
        
        #endregion Private Instance Fields

        #region Public Instance Properties
        
        /// <summary>
        /// INI File to Write To.
        /// </summary>
        [TaskAttribute("filename", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string FileName {
            get { return Project.GetFullPath(_iniFile); }
            set { _iniFile = StringUtils.ConvertEmptyToNull(value); }
        } 

        /// <summary>
        /// Key to set the value for.
        /// </summary>
        [TaskAttribute("key", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Key {
            get { return (_key); }
            set { _key = StringUtils.ConvertEmptyToNull(value); }
        } 

        /// <summary>
        /// value to set.  
        /// </summary>
        [TaskAttribute("default", Required=true)]
        [StringValidator(AllowEmpty=true)]
        public string Default {
            get { return _default; }
            set { _default = value; }
        }

        /// <summary>
        /// Section in the INI file.
        /// </summary>
        [TaskAttribute("section", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Section {
            get { return (_section); }
            set { _section = StringUtils.ConvertEmptyToNull(value); }
        }
 
        /// <summary>
        /// property where we store the return value.
        /// </summary>
        [TaskAttribute("property", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Property {
            get { return (_property); }
            set { _property = StringUtils.ConvertEmptyToNull(value); }
        } 

        #endregion Public Instance Properties

        protected override void ExecuteTask() {
            string whatImDoing = String.Format("Retrieving {0}/{1} from file: {2}", 
                Section, Key, FileName);

            // display build log message
            Log(Level.Verbose,  LogPrefix + whatImDoing);

            // perform operation
            try {
                IniFile iniFile = new IniFile(FileName);
                Properties[Property] = iniFile.GetString(Section, Key, Default);
            }
            catch (Exception e) {
                throw new BuildException("Failed: " + whatImDoing, Location,e);
            }
        }
    }
}

