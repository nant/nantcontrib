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
//
// Aaron A. Anderson (aaron@skypoint.com | aaron.anderson@farmcreditbank.com)

using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;

using SourceForge.NAnt;
using SourceForge.NAnt.Tasks;
using SourceForge.NAnt.Attributes;

namespace NAnt.Contrib.Tasks {


    /// <summary>Compiles Microsoft Visual Basic 6 programs.</summary>
    /// <remarks>
    ///     <para>Uses the VB6.EXE executable included with the Visual Basic 6 environment.</para>
    ///     <para>The compiler uses the settings and source files specified in the project or group file.</para>
    /// </remarks>
    /// <example>
    ///     <para>Build the project <c>HelloWorld.vbp</c> in the <c>build</c> directory.</para>
    ///     <code>
    ///      <![CDATA[
    ///         <vb6 project="HelloWorld.vbp" outdir="build" output="" />
    ///      ]]>
    ///     </code>
    /// </example>

    [TaskName("vb6")]
    public class Vb6Task : ExternalProgramBase {

        string _output = null;
        string _project = null;
        string _outdir = null;
        FileSet _references = new FileSet();
        FileSet _sources = new FileSet(); 
        string _programArguments = null;

        [TaskAttribute("output", Required=true)]
        public string  Output       { get { return _output; } set {_output = value;}} 

        /// <summary>Output directory for the compilation target.  This directory must exist.</summary>
        [TaskAttribute("outdir", Required=true)]
        public string  OutDir       { get { return _outdir; } set {_outdir = value;}} 

        /// <summary>Visual Basic project or group file.</summary>
        [TaskAttribute("project", Required=true)]
        public string  Proj         { get { return _project; } set {_project = value;}} 

        /// <summary>When one of the reference files specified here is modified, the project will be rebuilt.  This information will not be passed to the compiler, since it uses the project file instead.</summary>
        [FileSet("references")]
        public FileSet References   { get { return _references; } }

        /// <summary>When one of the source files specified here is modified, the project will be rebuilt.  This information will not be passed to the compiler, since it uses the project file instead.</summary>
        [FileSet("sources")]
        public FileSet Sources      { get { return _sources; } }

        public override string ProgramFileName  { get { return Name; } }
        public override string ProgramArguments { get { return _programArguments; } }

        protected string GetOutputPath() {
            return Path.GetFullPath(Path.Combine(BaseDirectory, Output));
        }

        protected virtual bool NeedsCompiling() {
            // return true as soon as we know we need to compile

            string fileName;

            FileInfo outputFileInfo = new FileInfo(GetOutputPath());
            if (!outputFileInfo.Exists) {
                return true;
            }

            //HACK:(POSSIBLY)Is there any other way to pass in a single file to check to see if it needs to be updated?
            StringCollection fileset = new StringCollection();
            fileset.Add(outputFileInfo.FullName);
            fileName = FileSet.FindMoreRecentLastWriteTime(fileset, outputFileInfo.LastWriteTime);
            if (fileName != null) {
                Log.WriteLineIf(Verbose, LogPrefix + "{0} is out of date, recompiling.", fileName);
                return true;
            }

            fileName = FileSet.FindMoreRecentLastWriteTime(Sources.FileNames, outputFileInfo.LastWriteTime);
            if (fileName != null) {
                Log.WriteLineIf(Verbose, LogPrefix + "{0} is out of date, recompiling.", fileName);
                return true;
            }

            fileName = FileSet.FindMoreRecentLastWriteTime(References.FileNames, outputFileInfo.LastWriteTime);
            if (fileName != null) {
                Log.WriteLineIf(Verbose, LogPrefix + "{0} is out of date, recompiling.", fileName);
                return true;
            }

            return false;
        }

        protected override void ExecuteTask() { 

            if (NeedsCompiling()) {

                //Using a stringbuilder vs. StreamWriter since this program will not accept response files.
                StringBuilder writer = new StringBuilder();

                try {
                    if (References.BaseDirectory == null) {
                        References.BaseDirectory = BaseDirectory;
                    }

                    writer.AppendFormat(" /make \"{0}\"", _project);

                    writer.AppendFormat(" /outdir \"{0}\"", _outdir);

                    // call base class to do the work
                    _programArguments = writer.ToString();
                    base.ExecuteTask();

                } finally {
                    writer = null;
                }
            }
        }
    }
}
