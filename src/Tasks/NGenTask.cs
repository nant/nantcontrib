//
// NAntContrib
// Copyright (C) 2001-2002 Gerry Shaw
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

// Jayme C. Edwards (jedwards@wi.rr.com)

using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Specialized;

using NAnt.Core;
using NAnt.Core.Tasks;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Pre-translates native code for an assembly containing IL (Intermediary 
    /// Language bytecode) on the Windows platform.
    /// </summary>
    /// <example>
    ///   <code>
    ///     <![CDATA[
    /// <ngen assembly="MyAssembly.dll" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("ngen")]
    [ProgramLocation(LocationType.FrameworkDir)]
    public class NGenTask : ExternalProgramBase {
        private string _args;
        string _assembly = null;
        bool _show = false;
        bool _delete = false;
        bool _debug = false;
        bool _debugoptimized = false;
        bool _profiled = false;
        bool _nologo = false;
        bool _silent = false;

        /// <summary>Assembly path or display name.</summary>
        [TaskAttribute("assembly", Required=true)]
        public string Assembly {
            get { return _assembly; }
            set { _assembly = value; }
        }

        /// <summary>If existing images should be shown.</summary>
        [TaskAttribute("show")]
        [BooleanValidator()]
        public bool Show {
            get { return _show; }
            set { _show = value; }
        }

        /// <summary>If existing images should be deleted.</summary>
        [TaskAttribute("delete")]
        [BooleanValidator()]
        public bool Delete {
            get { return _delete; }
            set { _delete = value; }
        }

        /// <summary>If an image should be generated which
        /// can be used under a debugger.</summary>
        [TaskAttribute("debug")]
        [BooleanValidator()]
        public bool Debug {
            get { return _debug; }
            set { _debug = value; }
        }

        /// <summary>If an image should be generated which
        /// can be used under a debugger in optimized
        /// debugging mode.</summary>
        [TaskAttribute("debugoptimized")]
        [BooleanValidator()]
        public bool DebugOptimized {
            get { return _debugoptimized; }
            set { _debugoptimized = value; }
        }

        /// <summary>If an image should be generated which
        /// can be used under a profiler.</summary>
        [TaskAttribute("profiled")]
        [BooleanValidator()]
        public bool Profiled {
            get { return _profiled; }
            set { _profiled = value; }
        }

        /// <summary>Suppresses the banner.</summary>
        [TaskAttribute("nologo")]
        [BooleanValidator()]
        public bool NoLogo {
            get { return _nologo; }
            set { _nologo = value; }
        }

        /// <summary>Prevents NGen from displaying success message.</summary>
        [TaskAttribute("silent")]
        [BooleanValidator()]
        public bool Silent {
            get { return _silent; }
            set { _silent = value; }
        }

        /// <summary>
        /// Arguments of program to execute
        /// </summary>
        public override string ProgramArguments {
            get {
                return _args;
            }
        }

        ///<summary>
        ///Initializes task and ensures the supplied attributes are valid.
        ///</summary>
        ///<param name="taskNode">Xml node used to define this task instance.</param>
        protected override void InitializeTask(System.Xml.XmlNode taskNode) {
        }

        protected override void ExecuteTask() {
            StringBuilder arguments = new StringBuilder();

            if (NoLogo){
                arguments.Append("/nologo ");
            }

            if (Silent){
                arguments.Append("/silent ");
            }

            if (Show) {
                arguments.Append("/show ");
            }

            if (Delete) {
                arguments.Append("/delete ");
            }

            if (Debug) {
                arguments.Append("/debug ");
            }

            if (DebugOptimized) {
                arguments.Append("/debugopt ");
            }

            if (Profiled) {
                arguments.Append("/prof ");
            }

            arguments.Append(Assembly);

            try  {
                _args = arguments.ToString();

                base.ExecuteTask();
            }
            catch (Exception e) {
                throw new BuildException(LogPrefix + "ERROR: " + e);
            }
        }
    }
}
