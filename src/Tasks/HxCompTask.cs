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
using Microsoft.Win32;
using System.Collections.Specialized;

using SourceForge.NAnt;
using SourceForge.NAnt.Tasks;
using SourceForge.NAnt.Attributes;

namespace NAnt.Contrib.Tasks
{
	/// <summary>Compiles a Microsoft HTML Help 2.0 Project.</summary>
	/// <example>
	///   <para>Compile a help file.</para>
	///   <code><![CDATA[<hxcomp contents="MyContents.HxC" output="MyHelpFile.HxS" projectroot="HelpSourceFolder"/>]]></code>
	/// </example>
    [TaskName("hxcomp")]
    public class HxCompTask : ExternalProgramBase
	{
		private string _args;
		string _contents = null;
		string _logfile = null;
		string _unicodelogfile = null;
		string _projectroot = null;
		string _output = null;
		bool _noinformation = false;
		bool _noerrors = false;
		bool _nowarnings = false;
		bool _quiet = false;
		string _uncompilefile = null;
		string _uncompileoutputdir = null;

        /// <summary>The name of the contents (.HxC) file.</summary>
        [TaskAttribute("contents")]
        public string Contents {
            get { return _contents; }
            set { _contents = value; } 
        }      

        /// <summary>ANSI/DBCS log filename.</summary>
        [TaskAttribute("logfile")]        
        public string LogFile {
            get { return _logfile; }
            set { _logfile = value; }
        }

        /// <summary>Unicode log filename. </summary>
        [TaskAttribute("unicodelogfile")]       
        public string UnicodeLogFile {
            get { return _unicodelogfile; }
            set { _unicodelogfile = value; }
        }

        /// <summary>Root directory containing Help 2.0 project files.</summary>
        [TaskAttribute("projectroot")]
        public string ProjectRoot {
            get { return _projectroot; }
            set { _projectroot = value; }
        }

        /// <summary>Output (.HxS) filename.</summary>
        [TaskAttribute("output")] 
        public string Output {
            get { return _output; }
            set { _output = value; }
        }

        /// <summary>Generate no informational messages.</summary>
        [TaskAttribute("noinformation")]       
        [BooleanValidator()]
        public bool NoInformation {
            get { return _noinformation; }
            set { _noinformation = value; }
        }
        
        /// <summary>Generate no error messages.</summary>
        [TaskAttribute("noerrors")]       
		[BooleanValidator()]
        public bool NoErrors {
            get { return _noerrors; }
            set { _noerrors = value; }
        }

		/// <summary>Generate no warning messages.</summary>
		[TaskAttribute("nowarnings")]       
		[BooleanValidator()]
		public bool NoWarnings 
		{
			get { return _nowarnings; }
			set { _nowarnings = value; }
		}
        
        /// <summary>Quiet mode.</summary>
        [TaskAttribute("quiet")]
        [BooleanValidator()]
        public bool Quiet {
            get { return _quiet; }
            set { _quiet = value; }
        }

		/// <summary>File to be decompiled.</summary>
		[TaskAttribute("uncompilefile")] 
		public string UncompileFile {
			get { return _uncompilefile; }
			set { _uncompilefile = value; }
		}

		/// <summary>Directory to place decompiled files into.</summary>
		[TaskAttribute("uncompileoutputdir")] 
		public string UncompileOutputDir {
		get { return _uncompileoutputdir; }
		set { _uncompileoutputdir = value; }
		}
       
		public override string ProgramFileName
		{
            get
			{
				RegistryKey helpPackageKey = Registry.LocalMachine.OpenSubKey(
					@"Software\Microsoft\VisualStudio\7.0\" + 
					@"Packages\{7D57F111-B9F3-11d2-8EE0-00C04F5E0C38}",
					false);

				if (helpPackageKey != null)
				{
					string helpPackageVal = (string)helpPackageKey.GetValue("InprocServer32", null);
					if (helpPackageVal != null)
					{
						string helpPackageDir = Path.GetDirectoryName(helpPackageVal);
						if (helpPackageDir != null)
						{
							if (Directory.Exists(helpPackageDir))
							{
								DirectoryInfo parentDir = Directory.GetParent(helpPackageDir);
								if (parentDir != null)
								{
									helpPackageKey.Close();
									return "\"" + parentDir.FullName + "\\hxcomp.exe\"";
								}
							}
						}
					}
					helpPackageKey.Close();
				}

				throw new Exception(
					"Unable to locate installation directory of " + 
					"Microsoft Help 2.0 SDK in the registry.");
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

        protected override void ExecuteTask()
		{
            // If the user wants to see the actual command the -verbose flag
            // will cause ExternalProgramBase to display the actual call.
			if ( Output != null ) 
			{
				Log.WriteLine(LogPrefix + "Compiling HTML Help 2.0 File {0}", Output);
			}
			// Lop thru fileset 
			else if (UncompileFile != null) 
			{    
				Log.WriteLine(LogPrefix + "Decompiling HTML Help 2.0 File {0}", UncompileFile);
			}
			else 
			{
				Log.WriteLine("ERROR: Must specify file to be compiled or decompiled.");
				return;
			}

			try
			{
				bool firstArg = true;

				StringBuilder arguments = new StringBuilder();
	               
				if (NoInformation)
				{
					if (firstArg) { firstArg = false; } else { arguments.Append(" "); }
					arguments.Append("-i");
				}
				if (NoErrors)
				{
					if (firstArg) { firstArg = false; } else { arguments.Append(" "); }
					arguments.Append("-e");
				}
				if (NoWarnings)
				{
					if (firstArg) { firstArg = false; } else { arguments.Append(" "); }
					arguments.Append("-w");
				}
				if (Quiet)
				{
					if (firstArg) { firstArg = false; } else { arguments.Append(" "); }
					arguments.Append("-q");
				}
				if (Contents != null)
				{
					if (firstArg) { firstArg = false; } else { arguments.Append(" "); }
					arguments.Append("-p ");
					arguments.Append(Contents);
				}
				if (LogFile != null)
				{
					if (firstArg) { firstArg = false; } else { arguments.Append(" "); }
					arguments.Append("-l ");
					arguments.Append(LogFile);
				}
				if (UnicodeLogFile != null)
				{
					if (firstArg) { firstArg = false; } else { arguments.Append(" "); }
					arguments.Append("-n ");
					arguments.Append(UnicodeLogFile);
				}
				if (Output != null)
				{
					if (firstArg) { firstArg = false; } else { arguments.Append(" "); }
					arguments.Append("-o ");
					arguments.Append(Output);
				}
				if (ProjectRoot != null)
				{
					if (firstArg) { firstArg = false; } else { arguments.Append(" "); }
					arguments.Append("-r ");
					arguments.Append(ProjectRoot);
				}
				if (UncompileFile != null)
				{
					if (firstArg) { firstArg = false; } else { arguments.Append(" "); }
					arguments.Append("-u ");
					arguments.Append(UncompileFile);
				}
				if (UncompileOutputDir != null)
				{
					if (firstArg) { firstArg = false; } else { arguments.Append(" "); }
					arguments.Append("-d ");
					arguments.Append(UncompileOutputDir);
				}

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
