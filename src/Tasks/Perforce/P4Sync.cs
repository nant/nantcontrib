// NAnt - A .NET build tool
// Copyright (C) 2001-2002 Gerry Shaw
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
// Ian MacLean ( ian_maclean@another.com )

using System;
using System.Text;
using NAnt.Core;
using NAnt.Core.Util;
using NAnt.Core.Tasks;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.Perforce {

    /// <summary>Synchronize client space to a Perforce depot view.  
    /// The API allows additional functionality of the "p4 sync" command
    /// (such as "p4 sync -f //...#have" or other exotic invocations).
    /// </summary>
    ///<example>
    ///<para>Sync to head using P4USER, P4PORT and P4CLIENT settings specified</para>    
    ///<code>
    ///     <![CDATA[
    ///<p4Sync view="//projects/foo/main/source/..." user="fbloggs" port="km01:1666" client="fbloggsclient" />
    ///     ]]>
    ///   </code>
    ///<para>Sync to head using default p4 environment variables</para>    
    ///<code>
    ///     <![CDATA[
    ///<p4sync p4view="//projects/foo/main/source/..." />
    ///     ]]>
    ///   </code>    
    ///<para>Force a re-sync to head, refreshing all files</para>
    ///<code>
    ///     <![CDATA[
    ///<p4sync force="true" view="//projects/foo/main/source/..." />
    ///     ]]>
    ///   </code>
    ///<para>Sync to a label</para>
    ///<code>
    ///     <![CDATA[
    ///<p4-sync label="myPerforceLabel" />
    ///     ]]>
    ///   </code>
    ///</example>
    /// <todo> Add decent label error handling for non-exsitant labels</todo>    
    [TaskName("p4sync")]
    public class P4Sync : P4Base {
        
        #region Private Instance Fields
        
        private string _label = null;
        private bool _force = false;
        
        #endregion Private Instance Fields
        
        #region Public Instance Properties
        
        /// <summary> Label to sync client to; optional.
        /// </summary>
        [TaskAttribute("label")]
        public string Label {
            get { return _label; }
            set { _label = StringUtils.ConvertEmptyToNull(value); }
        }       
        
        /// <summary> force a refresh of files, if this attribute is set; false by default.
        /// </summary>
        [TaskAttribute("force")]
        [BooleanValidator()]
        public bool Force {
            set { _force = value;}
            get { return _force; }
        }
        
        #endregion Public Instance Properties
        
        /// <summary>
        /// This is an override used by the base class to get command specific args.
        /// </summary>
        protected override string CommandSpecificArguments {
            get { return getSpecificCommandArguments(); }
        }
        
        #region Override implementation of Task
        
        /// <summary>
        /// local method to build the command string for this particular command
        /// </summary>
        /// <returns></returns>
        protected string getSpecificCommandArguments( ) {
            StringBuilder arguments = new StringBuilder();
            arguments.Append("sync ");
            
            if ( View  != null ) {
                arguments.Append( View );
            }
            if ( Label  != null ) {
                arguments.Append( string.Format("@{0}", Label ));
            }
            if ( Force ) {
                arguments.Append( " -f");
            }
           
           return arguments.ToString();
        }
        
        #endregion Override implementation of Task
    }
}
