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
using System.Globalization;
using System.IO;

using SLiNgshoT.Core;

using NAnt.Core;
using NAnt.Core.Types;
using NAnt.Core.Attributes;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Converts a Visual Studio.NET Solution to a NAnt build file or nmake file.
    /// </summary>
    /// <example>
    ///   <para>
    ///   Convert the solution <c>MySolution.sln</c> to the NAnt build file 
    ///   <c>MySolution.build</c> and call the new build file.
    ///   </para>
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
    /// </example>
    /// <example>
    ///   <para>
    ///   Convert the solution <c>MySolution.sln</c> to the NAnt build file 
    ///   <c>MySolution.build</c>.  As the solution contains one or more web 
    ///   projects, one or more maps needs to be specified.
    ///   </para>
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
    [Obsolete("Use the <solution> task instead.", false)]
    public class SlingshotTask : Task {
        #region Private Instance Fields

        private string _solution;
        private string _format;
        private string _output;
        private OptionCollection _maps = new OptionCollection();
        private OptionCollection _parameters = new OptionCollection();

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The Visual Studio.NET solution file to convert.
        /// </summary>
        [TaskAttribute("solution", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Solution {
            get { return (_solution != null) ? Project.GetFullPath(_solution) : null; }
            set { _solution = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The output file format - either <c>nant</c> or <c>nmake</c>.
        /// </summary>
        [TaskAttribute("format", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Format {
            get { return _format; }
            set { _format = value; }
        }

        /// <summary>
        /// The output file name.
        /// </summary>
        [TaskAttribute("output", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Output {
            get { return (_output != null) ? Project.GetFullPath(_output) : null; }
            set { _output = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Mappings from URI to directories.  These are required for web projects.
        /// </summary>
        [BuildElementCollection("maps", "option")]
        public OptionCollection Maps {
            get { return _maps; }
        }

        /// <summary>
        /// Parameters to pass to SLiNgshoT.  The parameter <c>build.basedir</c> is required.
        /// </summary>
        [BuildElementCollection("parameters", "option")]
        public OptionCollection Parameters {
            get { return _parameters; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        protected override void ExecuteTask() {
            // display build log message
            Log(Level.Info, "Converting '{0}' to '{1}' using {2} format", 
                Solution, Output, Format);

            // get a SLiNgshoT SolutionWriter for the specified format.
            SolutionWriter solutionWriter = CreateSolutionWriter(Format);

            // make sure the specified format is supported
            if (solutionWriter == null) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "'{0}' is an unsupported format.", _format), Location);
            } 

            // copy parameters to hashtable.
            Hashtable parameters = OptionCollectionToHashtable(Parameters, "parameters"); 

            // make sure a build.basedir parameter was specified
            if (!parameters.ContainsKey("build.basedir")) {
                throw new BuildException("The <parameters> option 'build.basedir' is required.", Location);
            }

            // copy maps to hashtable
            Hashtable uriMap = OptionCollectionToHashtable(Maps, "maps"); 

            try {
                // NOTE: The default encoding is used.
                StreamWriter outputWriter = new StreamWriter(Output);

                // convert the solution
                Driver.WriteSolution(solutionWriter, outputWriter, Solution, parameters, uriMap);

                // close the output file
                outputWriter.Close();
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture, 
                    "Could not convert solution '{0}'.", Solution), Location, ex);
            }
        }

        #endregion Override implementation of Task

        #region Private Instance Methods

        /// <summary>
        /// Creates the <see cref="SolutionWriter" /> for the specified format.
        /// </summary>
        /// <returns>
        /// The <see cref="SolutionWriter" /> for the specified format, or 
        /// <see langword="null" /> if an unknown format was specified.
        /// </returns>
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

        /// <summary>
        /// Converts an <see cref="OptionCollection"/> to a <see cref="Hashtable"/>.
        /// </summary>
        private Hashtable OptionCollectionToHashtable(OptionCollection options, string optionSetName) {
            Hashtable convertedOptions = new Hashtable();

            if (options != null) {
                foreach (object option in options) {
                    string name;
                    string value;

                    Option ov = (Option) option;
                    name = ov.OptionName;
                    value = ov.Value;

                    Log(Level.Verbose, " -- {0} = {1}", name, value);

                    if (name == null) {
                        throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                            "Unspecified name for <{0}> option '{1}'.", optionSetName, name), 
                            Location);
                    } else if (value == null) {
                        throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                            "Unspecified value for <{0}> option '{1}'.", optionSetName, name), 
                            Location);
                    } else {
                        convertedOptions.Add(name, value);
                    }
                }
            }

            return convertedOptions;
        }

        #endregion Private Instance Methods
    }
}
