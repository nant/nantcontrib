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
    /// <summary>Registers a Microsoft HTML Help 2.0 Collection.</summary>
    /// <example>
    ///   <para>Register a help namespace.</para>
    ///   <code><![CDATA[<hxreg namespace="MyProduct.MyHelp" title="MyProductHelp" collection="MyHelp.HxC" helpfile="MyHelp.HxS"/>]]></code>
    /// </example>
    [TaskName("hxreg")]
    public class HxRegTask : ExternalProgramBase
    {
        private string _args;
        string _namespace = null;
        string _title = null;
        string _collection = null;
        string _description = null;
        string _helpfile = null;
        string _index = null;
        string _searchfile = null;
        string _attrindex = null;
        string _language = null;
        string _alias = null;
        string _commandfile = null;
        bool _unregister = false;

        /// <summary>Help collection namespace.</summary>
        [TaskAttribute("namespace")]
        public string Namespace {
            get { return _namespace; }
            set { _namespace = value; } 
        }      

        /// <summary>Title identifier.</summary>
        [TaskAttribute("title")]        
        public string Title {
            get { return _title; }
            set { _title = value; }
        }

        /// <summary>Collection (.HxC) filename. </summary>
        [TaskAttribute("collection")]       
        public string Collection {
            get { return _collection; }
            set { _collection = value; }
        }

        /// <summary>Description of the namespace.</summary>
        [TaskAttribute("description")]
        public string Description {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>Help (.HxS) filename.</summary>
        [TaskAttribute("helpfile")] 
        public string HelpFile {
            get { return _helpfile; }
            set { _helpfile = value; }
        }

        /// <summary>Index (.HxI) filename.</summary>
        [TaskAttribute("index")]       
        public string Index {
            get { return _index; }
            set { _index = value; }
        }

        /// <summary>Combined full-text search (.HxQ) filename.</summary>
        [TaskAttribute("searchfile")]       
        public string SearchFile {
            get { return _searchfile; }
            set { _searchfile = value; }
        }

        /// <summary>Combined attribute index (.HxR) filename.</summary>
        [TaskAttribute("attrindex")]       
        public string AttrIndex {
            get { return _attrindex; }
            set { _attrindex = value; }
        }

        /// <summary>Language ID.</summary>
        [TaskAttribute("language")]
        public string Language {
            get { return _language; }
            set { _language = value; }
        }

        /// <summary>Alias.</summary>
        [TaskAttribute("alias")] 
        public string Alias {
            get { return _alias; }
            set { _alias = value; }
        }

        /// <summary>Filename of a file containing HxReg commands.</summary>
        [TaskAttribute("commandfile")] 
        public string CommandFile {
            get { return _commandfile; }
            set { _commandfile = value; }
        }

        /// <summary>Unregister a namespace, title, or alias.</summary>
        [TaskAttribute("unregister")] 
        [BooleanValidator()]
        public bool UnRegister 
        {
            get { return _unregister; }
            set { _unregister = value; }
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
                                    return "\"" + parentDir.FullName + "\\hxreg.exe\"";
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
            if ( HelpFile != null ) 
            {
                Log.WriteLine(LogPrefix + "{0} HTML Help 2.0 File {1}", 
                    UnRegister ? "UnRegistering" : "Registering", HelpFile);
            }
            else 
            {
                Log.WriteLine("ERROR: Must specify file to be registered or unregistered.");
                return;
            }

            try
            {
                bool firstArg = true;

                StringBuilder arguments = new StringBuilder();

                if (UnRegister)
                {
                    if (firstArg) { firstArg = false; } else { arguments.Append(" "); }
                    arguments.Append("-r ");
                }

                if (Namespace != null)
                {
                    if (firstArg) { firstArg = false; } else { arguments.Append(" "); }
                    arguments.Append("-n ");
                    arguments.Append(Namespace);
                }
                if (Title != null)
                {
                    if (firstArg) { firstArg = false; } else { arguments.Append(" "); }
                    arguments.Append("-i ");
                    arguments.Append(Title);
                }
                if (Collection != null)
                {
                    if (firstArg) { firstArg = false; } else { arguments.Append(" "); }
                    arguments.Append("-c ");
                    arguments.Append(Collection);
                }
                if (Description != null)
                {
                    if (firstArg) { firstArg = false; } else { arguments.Append(" "); }
                    arguments.Append("-d ");
                    arguments.Append(Description);
                }
                if (HelpFile != null)
                {
                    if (firstArg) { firstArg = false; } else { arguments.Append(" "); }
                    arguments.Append("-s ");
                    arguments.Append(HelpFile);
                }
                if (Index != null)
                {
                    if (firstArg) { firstArg = false; } else { arguments.Append(" "); }
                    arguments.Append("-x ");
                    arguments.Append(Index);
                }
                if (SearchFile != null)
                {
                    if (firstArg) { firstArg = false; } else { arguments.Append(" "); }
                    arguments.Append("-q ");
                    arguments.Append(SearchFile);
                }
                if (AttrIndex != null)
                {
                    if (firstArg) { firstArg = false; } else { arguments.Append(" "); }
                    arguments.Append("-t ");
                    arguments.Append(AttrIndex);
                }
                if (Language != null)
                {
                    if (firstArg) { firstArg = false; } else { arguments.Append(" "); }
                    arguments.Append("-l ");
                    arguments.Append(Language);
                }
                if (Alias != null)
                {
                    if (firstArg) { firstArg = false; } else { arguments.Append(" "); }
                    arguments.Append("-a ");
                    arguments.Append(Alias);
                }
                if (CommandFile != null)
                {
                    if (firstArg) { firstArg = false; } else { arguments.Append(" "); }
                    arguments.Append("-f ");
                    arguments.Append(CommandFile);
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
