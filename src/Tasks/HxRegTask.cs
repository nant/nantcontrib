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

using NAnt.Core;
using NAnt.Core.Tasks;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Registers a Microsoft HTML Help 2.0 Collection.
    /// </summary>
    /// <example>
    ///   <para>Register a help namespace.</para>
    ///   <code>
    ///     <![CDATA[
    /// <hxreg namespace="MyProduct.MyHelp" title="MyProductHelp" collection="MyHelp.HxC" helpfile="MyHelp.HxS" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("hxreg")]
    public class HxRegTask : ExternalProgramBase {
        #region Private Instance Fields

        private string _args;
        private string _namespace;
        private string _title;
        private string _collection;
        private string _description;
        private FileInfo _helpfile;
        private string _index;
        private string _searchfile;
        private string _attrindex;
        private string _language;
        private string _alias;
        private string _commandfile;
        private bool _unregister;

        #endregion Private Instance Fields

        #region Public Instance Properties

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
        [TaskAttribute("helpfile", Required=true)] 
        public FileInfo HelpFile {
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
        public bool UnRegister {
            get { return _unregister; }
            set { _unregister = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of ExternalProgramBase

        public override string ProgramFileName {
            get {
                RegistryKey helpPackageKey = Registry.LocalMachine.OpenSubKey(
                    @"Software\Microsoft\VisualStudio\7.0\" + 
                    @"Packages\{7D57F111-B9F3-11d2-8EE0-00C04F5E0C38}",
                    false);

                if (helpPackageKey != null) {
                    string helpPackageVal = (string)helpPackageKey.GetValue("InprocServer32", null);
                    if (helpPackageVal != null) {
                        string helpPackageDir = Path.GetDirectoryName(helpPackageVal);
                        if (helpPackageDir != null) {
                            if (Directory.Exists(helpPackageDir)) {
                                DirectoryInfo parentDir = Directory.GetParent(helpPackageDir);
                                if (parentDir != null) {
                                    helpPackageKey.Close();
                                    return Path.Combine(parentDir.FullName, "hxreg.exe");
                                }
                            }
                        }
                    }
                    helpPackageKey.Close();
                }

                throw new BuildException(
                    "Unable to locate installation directory of " + 
                    "Microsoft Help 2.0 SDK in the registry.", Location);
            }
        }

        /// <summary>
        /// Arguments of program to execute
        /// </summary>
        public override string ProgramArguments {
            get {
                return _args;
            }
        }

        protected override void ExecuteTask() {
            Log(Level.Info, "{0} HTML Help 2.0 File '{1}'.", 
                UnRegister ? "UnRegistering" : "Registering", HelpFile.FullName);

            try {
                StringBuilder arguments = new StringBuilder();

                if (UnRegister) {
                    arguments.Append("-r ");
                }
                if (Namespace != null) {
                    arguments.Append("-n ");
                    arguments.Append('\"' + Namespace + '\"');
                    arguments.Append(" ");
                }
                if (Title != null) {
                    arguments.Append("-i ");
                    arguments.Append('\"' + Title + '\"');
                    arguments.Append(" ");
                }
                if (Collection != null) {
                    arguments.Append("-c ");
                    arguments.Append('\"' + Collection + '\"');
                    arguments.Append(" ");
                }
                if (Description != null) {
                    arguments.Append("-d ");
                    arguments.Append('\"' + Description + '\"');
                    arguments.Append(" ");
                }
                if (HelpFile != null) {
                    arguments.Append("-s ");
                    arguments.Append('\"' + HelpFile.FullName + '\"');
                    arguments.Append(" ");
                }
                if (Index != null) {
                    arguments.Append("-x ");
                    arguments.Append('\"' + Index + '\"');
                    arguments.Append(" ");
                }
                if (SearchFile != null) {
                    arguments.Append("-q ");
                    arguments.Append('\"' + SearchFile + '\"');
                    arguments.Append(" ");
                }
                if (AttrIndex != null) {
                    arguments.Append("-t ");
                    arguments.Append('\"' + AttrIndex + '\"');
                    arguments.Append(" ");
                }
                if (Language != null) {
                    arguments.Append("-l ");
                    arguments.Append('\"' + Language + '\"');
                    arguments.Append(" ");
                }
                if (Alias != null) {
                    arguments.Append("-a ");
                    arguments.Append('\"' + Alias + '\"');
                    arguments.Append(" ");
                }
                if (CommandFile != null) {
                    arguments.Append("-f ");
                    arguments.Append('\"' + CommandFile + '\"');
                }

                _args = arguments.ToString();

                base.ExecuteTask();
            } catch (Exception ex) {
                throw new BuildException(
                    "Microsoft HTML Help 2.0 Collection could not be registered.", 
                    Location, ex);
            }
        }

        #endregion Override implementation of ExternalProgramBase
    }
}
