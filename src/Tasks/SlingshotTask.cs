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
// Bernard Vander Beken

using System;
using System.Collections;
using System.IO;

using SourceForge.NAnt.Attributes;

using SLiNgshoT.Core;

namespace SourceForge.NAnt.Tasks {

    /// <summary>Converts a Visual Studio.NET Solution to a NAnt build file or nmake file.</summary>
    /// <example>
    ///   <para>Convert the Solution <c>MySolution.sln</c> to the NAnt build file <c>MySolution.build</c> and call the new build file.</para>
    ///   <code>
    /// <![CDATA[
    /// <slingshot solution="MySolution.sln" format="nant" output="MySolution.build"> 
    ///     <parameters>
    ///         <option name="build.basedir" value="..\bin"/>
    ///     </parameters> 
    /// </slingshot>
    /// <nant buildfile="MySolution.build"/>
    /// ]]>
    ///   </code>
    ///   <para>Convert the Solution <c>MySolution.sln</c> to the NAnt build file <c>MySolution.build</c>.  Since the Solution contains one or more web projects, one or more maps needs to be specified.</para>
    ///   <code>
    /// <![CDATA[
    /// <slingshot solution="MySolution.sln" format="nant" output="MySolution.build">
    ///     <parameters>
    ///         <option name="build.basedir" value="..\bin"/>
    ///     </parameters> 
    ///     <maps>
    ///         <option name="http://localhost" value="C:\Inetpub\wwwroot"/>
    ///     </maps>
    /// </slingshot>
    /// ]]>
    ///   </code>
    /// </example>
    [TaskName("slingshot")]
    public class SlingshotTask : Task {

        string _solution = null;
        string _format = null;
        string _output = null;

        OptionSet _maps = new OptionSet();
        OptionSet _parameters = new OptionSet();

        /// <summary>The Visual Studio.NET Solution file to convert.</summary>
        [TaskAttribute("solution", Required=true)]
        public string Solution {
            get { return _solution; }
            set { _solution = value; }
        }

        /// <summary>The output file format, <c>nant</c> or <c>nmake</c>.</summary>
        [TaskAttribute("format", Required=true)]
        public string Format {
            get { return _format; }
            set { _format = value; }
        }

        /// <summary>The output file name.</summary>
        [TaskAttribute("output", Required=true)]
        public string Output {
            get { return _output; }
            set { _output = value; }
        }

        /// <summary>Mappings from URI to directories.  These are required for web projects.</summary>
        [OptionSetAttribute("maps")]
        public OptionSet Maps {
           get { return _maps; }
        }

        /// <summary>Parameters to pass to SLiNgshoT.  The parameter <c>build.basedir</c> is required.</summary>
        [OptionSetAttribute("parameters")]
        public OptionSet Parameters {
           get { return _parameters; }
        }

        protected override void ExecuteTask() {
            // display build log message
            Log.WriteLine(LogPrefix + "Converting {0} to {1} using {2} format", _solution, _output, _format);

            // Get a SLiNgshoT SolutionWriter for the specified format.
            SolutionWriter solutionWriter = CreateSolutionWriter(_format);

            // If the format was invalid
            if (solutionWriter == null) {
                string msg = String.Format("'{0}' is an unsupported format", _format);
                throw new BuildException(msg);
            } 

            // Copy parameters to hashtable.
            Hashtable parameters = OptionSetToHashtable(_parameters, "parameters"); 

            // The build.basedir parameter is required.
            if (!parameters.ContainsKey("build.basedir")) {
                throw new BuildException("The <parameters> option 'build.basedir' is required.");
            }

            // Copy maps to hashtable
            Hashtable uriMap = OptionSetToHashtable(_maps, "maps"); 

            try {
                // NOTE: The default encoding is used.
                StreamWriter outputWriter = new StreamWriter(_output);

                // Convert the solution.
                Driver.WriteSolution(solutionWriter, outputWriter, _solution, parameters, uriMap);

                outputWriter.Close();
            } catch (Exception e) {
                string msg = String.Format("Could not convert solution.", _solution);
                throw new BuildException(msg, Location, e);
            }
        }

        /// <summary>Creates and returns the SolutionWriter for the specified format. Returns <c>null</c> if an unknown format was specified.</summary>
        // TODO: This may belong in SLiNgshoT.Core (eg Driver.cs)
        private SolutionWriter CreateSolutionWriter(string format) {

            SolutionWriter writer = null;

            switch (format) {
                case "nant":
                    writer = new NAntWriter();
                    break;
                case "nmake":
                    writer = new NMakeWriter();
                    break;
            }

            return writer;
        }

        /// <summary>Converts an <see cref="OptionSet"/> to a <see cref="Hashtable"/>.</summary>
        private Hashtable OptionSetToHashtable(OptionSet options, string optionSetName) {

            Hashtable convertedOptions = new Hashtable();

            if (options != null) {
                foreach (OptionValue option in options) {
                    string name  = option.Name;
                    string value = option.Value;

                    // name must be specified
                    if (name == null) {
                        string msg = String.Format("Unspecified name for <{0}> option '{1}'", optionSetName, name);
                        throw new BuildException(msg);
                    // value must be specified
                    } else if (value == null) {
                        string msg = String.Format("Unspecified value for <{0}> option '{1}'", optionSetName, name);
                        throw new BuildException(msg);
                    } else {
                        convertedOptions.Add(name, value);
                    }
                }
            }

            return convertedOptions;
        }

    }
}
