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

using SourceForge.NAnt;
using SourceForge.NAnt.Tasks;
using SourceForge.NAnt.Attributes;

namespace NAnt.Contrib.Tasks
{
	/// <summary>Generates a Windows Forms Control that wraps 
	/// ActiveX Controls defined in an OCX.</summary>
	/// <example>
	///   <code><![CDATA[<aximp ocx="MyControl.ocx" out="MyFormsControl.dll" />]]></code>
	/// </example>
	[TaskName("aximp")]
	public class AxImpTask : ExternalProgramBase
	{
		private string _args;
		string _ocx = null;
        string _out = null;
        string _publickeyfile = null;
        string _keyfile = null;
        string _keycontainer = null;
        bool _delaysign = false;
        bool _generatesource = false;
        bool _nologo = false;
        bool _silent = false;
        
		/// <summary>Filename of the .ocx file.</summary>
       	[TaskAttribute("ocx", Required=true)]
        public string Ocx {
            get { return _ocx; }
            set { _ocx = value; } 
        }

        /// <summary>Filename of the generated assembly.</summary>
        [TaskAttribute("out")]
        public string Out 
        {
            get { return _out; }
            set { _out = value; } 
        }

        /// <summary>File containing strong name public key.</summary>
        [TaskAttribute("publickeyfile")]
        public string PublicKeyFile 
        {
            get { return _publickeyfile; }
            set { _publickeyfile = value; } 
        }      

        /// <summary>File containing strong name key pair.</summary>
        [TaskAttribute("keyfile")]
        public string KeyFile {
            get { return _keyfile; }
            set { _keyfile = value; }
        }

        /// <summary>File of key container holding strong name key pair.</summary>
        [TaskAttribute("keycontainer")]
        public string KeyContainer 
        {
            get { return _keycontainer; }
            set { _keycontainer = value; }
        }

        /// <summary>Force strong name delay signing .</summary>
        [TaskAttribute("delaysign")]        
        [BooleanValidator()]
        public bool DelaySign
        {
            get { return _delaysign; }
            set { _delaysign = value; }
        }

        /// <summary>If C# source code for the Windows 
        /// Form wrapper should be generated.</summary>
        [TaskAttribute("generatesource")]        
        [BooleanValidator()]
        public bool GenerateSource
        {
            get { return _generatesource; }
            set { _generatesource = value; }
        }

        /// <summary>Suppresses the banner.</summary>
        [TaskAttribute("nologo")]        
        [BooleanValidator()]
        public bool NoLogo
        {
            get { return _nologo; }
            set { _nologo = value; }
        }

        /// <summary>Prevents AxImp from displaying success message.</summary>
        [TaskAttribute("silent")]        
        [BooleanValidator()]
        public bool Silent
        {
            get { return _silent; }
            set { _silent = value; }
        }
        
        public override string ProgramFileName
        {
            get
            {
                return "aximp.exe";
            }
        }

        /// <summary>
        /// Arguments of program to execute
        /// </summary>
        public override string ProgramArguments 
        {
            get
            {
                return _args;
            }
        }

        ///<summary>
        ///Initializes task and ensures the supplied attributes are valid.
        ///</summary>
        ///<param name="taskNode">Xml node used to define this task instance.</param>
        protected override void InitializeTask(System.Xml.XmlNode taskNode) 
        {
        }

        protected override void ExecuteTask()
        {
            StringBuilder arguments = new StringBuilder();

            arguments.Append(Ocx);

            if (DelaySign)
            {
                arguments.Append("/delaysign ");
            }

            if (GenerateSource)
            {
                arguments.Append("/source ");
            }

            if (NoLogo)
            {
                arguments.Append("/nologo ");
            }

            if (Silent)
            {
                arguments.Append("/silent ");
            }

            if (Out != null)
            {
                arguments.Append(" /out:");
                arguments.Append(Out);
            }
            
            if (PublicKeyFile != null)
            {
                arguments.Append(" /publickey:");
                arguments.Append(PublicKeyFile);
            }

            if (KeyFile != null)
            {
                arguments.Append(" /keyfile:");
                arguments.Append(KeyFile);
            }

            if (KeyContainer != null)
            {
                arguments.Append(" /keycontainer:");
                arguments.Append(KeyContainer);
            }

            try
            {
                _args = arguments.ToString();

                base.ExecuteTask();
            }
            catch (Exception e)
            {
                throw new BuildException(LogPrefix + "ERROR: " + e);
            }
        }
    }
}
