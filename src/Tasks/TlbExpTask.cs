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
// Aaron Anderson (aaron@skypoint.com | aaron.anderson@farmcreditbank.com)

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

using SourceForge.NAnt;
using SourceForge.NAnt.Tasks;
using SourceForge.NAnt.Attributes;

namespace NAnt.Contrib.Tasks {


    /// <summary>Exports a .NET assembly to a type library that can be used from unmanaged code (wraps Microsoft's tlbexp.exe).</summary>
    /// <remarks>
    ///   <para><a href="ms-help://MS.NETFrameworkSDK/cptools/html/cpgrftypelibraryexportertlbexpexe.htm">See the Microsoft.NET Framework SDK documentation for details.</a></para>
    /// </remarks>
    /// <example>
    ///   <para>Export <c>DotNetAssembly.dll</c> to <c>LegacyCOM.dll</c>.</para>
    ///   <code><![CDATA[<tlbexp assembly="DotNetAssembly.dll" output="LegacyCOM.dll"/>]]></code>
    /// </example>
    [TaskName("tlbexp")]
    public class TlbExpTask : ExternalProgramBase {
        
        string _assembly = null;
        string _output = null;
        string _names = null;
        string _programArguments = null;

        /// <summary>Specifies the assembly that gets exported.</summary>
        /// <remarks><a href="ms-help://MS.NETFrameworkSDK/cptools/html/cpgrftypelibraryexportertlbexpexe.htm">See the Microsoft.NET Framework SDK documentation for details.</a></remarks>
        /// <value></value>
        [TaskAttribute("assembly", Required=true)]
        public string Assembly        { get { return _assembly; } set { _assembly = value; } }

        /// <summary>Specifies the <b>/out</b> option that gets passed to the type library exporter.</summary>
        /// <remarks><a href="ms-help://MS.NETFrameworkSDK/cptools/html/cpgrftypelibraryexportertlbexpexe.htm">See the Microsoft.NET Framework SDK documentation for details.</a></remarks>
        /// <value></value>
        [TaskAttribute("output", Required=true)]
        public string Output        { get { return _output; } set { _output = value; } }

        /// <summary>Specifies the <b>/names</b> option that gets passed to the type library exporter.</summary>
        /// <remarks><a href="ms-help://MS.NETFrameworkSDK/cptools/html/cpgrftypelibraryexportertlbexpexe.htm">See the Microsoft.NET Framework SDK documentation for details.</a></remarks>
        /// <value></value>
        [TaskAttribute("names")]
        public string Names     { get { return _names; } set { _names = value; } }

        public override string ProgramFileName  { get { return Name; } }
        public override string ProgramArguments { get { return _programArguments; } }

        protected string GetOutputPath() {
            return Path.GetFullPath(Path.Combine(BaseDirectory, Output));
        }

        protected virtual bool NeedsCompiling() {
            // return true as soon as we know we need to compile

            FileInfo outputFileInfo = new FileInfo(GetOutputPath());
            if (!outputFileInfo.Exists) {
                return true;
            }

            //HACK:(POSSIBLY)Is there any other way to pass in a single file to check to see if it needs to be updated?
            StringCollection fileset = new StringCollection();
            fileset.Add(outputFileInfo.FullName);
            string fileName = FileSet.FindMoreRecentLastWriteTime(fileset, outputFileInfo.LastWriteTime);
            if (fileName != null) {
                Log.WriteLineIf(Verbose, LogPrefix + "{0} is out of date, recompiling.", fileName);
                return true;
            }

            // if we made it here then we don't have to reimport the typelib.
            return false;
        }

        protected override void ExecuteTask() {

            //Check to see if any of the underlying interop dlls or the typelibs have changed
            //Otherwise, it's not necessary to reimport.
            if (NeedsCompiling()) {

                //Using a stringbuilder vs. StreamWriter since this program will not accept response files.
                StringBuilder writer = new StringBuilder();

                try {

                    writer.Append("\"" + _assembly + "\"");

                    // Any option that specifies a file name must be wrapped in quotes
                    // to handle cases with spaces in the path.
                    writer.AppendFormat(" /out:\"{0}\"", GetOutputPath());

                    // Microsoft common compiler options
                    writer.Append(" /nologo");

                    if (Names != null) {
                        writer.AppendFormat(" /names:{0}", Names);
                    }    

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
