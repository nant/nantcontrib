// Driver.cs - the SLiNgshoT entry point
// Copyright (C) 2001, 2002  Jason Diamond
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

using System;
using System.Collections;
using System.IO;
using System.Reflection;

namespace SLiNgshoT.Core {
    public class Driver {
        public static IList GetOutputFormats() {
            ArrayList outputFormats = new ArrayList();

            Assembly assembly = typeof(Driver).Assembly;

            foreach (Type type in assembly.GetTypes()) {
                if (type.GetInterface(typeof(SolutionWriter).FullName) != null) {
                    OutputFormatAttribute outputFormat = Attribute.GetCustomAttribute(type, typeof(OutputFormatAttribute)) as OutputFormatAttribute;

                    if (outputFormat != null) {
                        outputFormats.Add(outputFormat.Name);
                    }
                }
            }

            outputFormats.Sort();

            return outputFormats;
        }

        private static Type GetSolutionWriterImplementation(string format) {
            Assembly assembly = typeof(Driver).Assembly;

            foreach (Type type in assembly.GetTypes()) {
                if (type.GetInterface(typeof(SolutionWriter).FullName) != null) {
                    OutputFormatAttribute outputFormat = Attribute.GetCustomAttribute(type, typeof(OutputFormatAttribute)) as OutputFormatAttribute;

                    if (outputFormat != null && outputFormat.Name == format) {
                        return type;
                    }
                }
            }

            return null;
        }

        public static OutputParameterAttribute[] GetOutputParameters(string format) {
            Type type = GetSolutionWriterImplementation(format);

            if (type != null) {
                object[] outputParameterAttributes = type.GetCustomAttributes(typeof(OutputParameterAttribute), true);
                OutputParameterAttribute[] outputParameters = new OutputParameterAttribute[outputParameterAttributes.Length];

                for (int i = 0; i < outputParameterAttributes.Length; ++i) {
                    outputParameters[i] = outputParameterAttributes[i] as OutputParameterAttribute;
                }

                return outputParameters;
            }

            return new OutputParameterAttribute[0];
        }

        public static void ParseArgs(string[] args, out string format, out string solution, Hashtable parameters, Hashtable uriMap) {
            format = null;
            solution = null;

            for (int i = 0; i < args.Length; ++i) {
                string arg = args[i];

                if (arg[0] == '/' || arg[0] == '-') {
                    string argName = arg.Substring(1);

                    switch (argName) {
                        case "sln":
                            solution = args[++i];
                            break;
                        case "map":
                            if (i + 2 < args.Length) {
                                string uriPrefix = args[++i];
                                string filePrefix = args[++i];
                                uriMap.Add(uriPrefix, filePrefix);
                            }
                            else {
                                Console.Error.WriteLine("not enough arguments left for -map option");
                            }
                            break;
                        default:
                            format = argName;
                            break;
                    }
                }
                else {
                    int indexOfEquals = arg.IndexOf('=');

                    if (indexOfEquals != -1) {
                        string name = arg.Substring(0, indexOfEquals);
                        string value = arg.Substring(indexOfEquals + 1);
                        parameters.Add(name, value);
                    }
                    else {
                        Console.Error.WriteLine("unknown argument: {0}", arg);
                    }
                }
            }
        }

        public static string FindSolution(string directory) {
            string[] files = Directory.GetFiles(directory, "*.sln");

            if (files.Length == 0) {
                throw new ApplicationException(String.Format("{0} does not contain any '.sln' files", directory));
            }
            else if (files.Length > 1) {
                throw new ApplicationException(String.Format("{0} contains too many '.sln' files", directory));
            }

            return files[0];
        }

        public static void WriteSolution(
            SolutionWriter writer, 
            TextWriter textWriter, 
            string sln, 
            Hashtable parameters, 
            Hashtable uriMap) {
            writer.SetOutput(textWriter);
            writer.SetParameters(parameters);

            Solution solution = new Solution();
            solution.Read(sln, uriMap);

            writer.WriteStartSolution(solution);

            foreach (Project project in solution.GetProjects()) {
                IList sourceFiles = project.GetSourceFiles();

                if (sourceFiles.Count > 0) {
                    writer.WriteStartProjectSourceFiles(project);

                    foreach (File file in sourceFiles) {
                        writer.WriteProjectSourceFile(file);
                    }

                    writer.WriteEndProjectSourceFiles();
                }

                IList resXFiles = project.GetResXResourceFiles();

                if (resXFiles.Count > 0) {
                    writer.WriteStartProjectResXResourceFiles(project);

                    foreach (File file in resXFiles) {
                        writer.WriteProjectResXResourceFile(file);
                    }

                    writer.WriteEndProjectResXResourceFiles();
                }

                IList resourceFiles = project.GetNonResXResourceFiles();

                if (resourceFiles.Count > 0) {
                    writer.WriteStartProjectNonResXResourceFiles(project);

                    foreach (File file in resourceFiles) {
                        writer.WriteProjectNonResXResourceFile(file);
                    }

                    writer.WriteEndProjectNonResXResourceFiles();
                }
            }

            foreach (Project project in solution.GetProjects()) {
                if (project.CountFiles("Compile") > 0) {
                    writer.WriteStartProject(project);

                    writer.WriteStartProjectDependencies();

                    foreach (Project dependency in solution.GetDependencies(project)) {
                        writer.WriteProjectDependency(dependency);
                    }

                    foreach (Reference reference in project.GetReferences()) {
                        if (reference.Type == "Project") {
                            writer.WriteProjectDependency(solution.GetProject(reference.Value));
                        }
                    }

                    foreach (File file in project.GetFiles()) {
                        if (!file.RelativePath.EndsWith(".licx")) {
                            writer.WriteProjectDependency(file);
                        }
                    }

                    writer.WriteEndProjectDependencies();

                    IList resXFiles = project.GetResXResourceFiles();

                    if (resXFiles.Count > 0) {
                        writer.WriteStartResXFiles();

                        foreach (File file in resXFiles) {
                            writer.WriteResXFile(file);
                        }

                        writer.WriteEndResXFiles();
                    }

                    writer.WriteStartAssembly();

                    IList sourceFiles = project.GetSourceFiles();

                    if (sourceFiles.Count > 0) {
                        writer.WriteStartSourceFiles();

                        foreach (File file in sourceFiles) {
                            writer.WriteSourceFile(file);
                        }

                        writer.WriteEndSourceFiles();
                    }

                    writer.WriteStartReferences();

                    // Write out the standard system references.

                    foreach (Reference reference in project.GetSystemReferences()) {
                        if (reference != null) {
                            // assume it's another project in sln and/or copy-local
                            string path = reference.Value + ".dll";
                            bool inBuildPath = true;
                            if ( !reference.CopyLocal && (reference.Type == "AssemblyName") ) {
                                inBuildPath = false;
                                if ( reference.SourcePath != string.Empty ) {
                                    path = reference.SourcePath;
                                }
                                if ( ! Path.IsPathRooted( path ) ) {
                                    path = Path.GetFullPath( solution.SolutionDirectory + "\\" 
                                        + project.RelativePath + "\\" + path );
                                }
                            }
                            writer.WriteReference( path , inBuildPath );
                        }
                    }

                    // Write out the project references.

                    foreach (Project referencedProject in project.GetReferencedProjects()) {
                        writer.WriteReference(referencedProject);
                    }

                    writer.WriteEndReferences();

                    writer.WriteStartResources();

                    foreach (File file in project.GetResXResourceFiles()) {
                        string path =
                            project.RootNamespace +
                            "." +
                            Path.GetFileNameWithoutExtension(file.RelativePath) +
                            ".resources";

                        writer.WriteResource(path, null, true);
                    }

                    foreach (File file in project.GetNonResXResourceFiles()) {
                        writer.WriteResource(
                            file.RelativePathFromSolutionDirectory,
                            file.ResourceName,
                            false);
                    }

                    writer.WriteEndResources();

                    writer.WriteEndAssembly();

                    // Write out the project references so that they can be copied.

                    writer.WriteStartCopyProjectAssemblies();

                    foreach (Project referencedProject in project.GetReferencedProjects()) {
                        writer.WriteCopyProjectAssembly(referencedProject);
                    }

                    writer.WriteEndCopyProjectAssemblies();

                    writer.WriteEndProject();
                }
            }

            writer.WriteStartCleanTarget();

            foreach (Project project in solution.GetProjects()) {
                writer.WriteCleanProject(project);
            }

            writer.WriteEndCleanTarget();

            writer.WriteEndSolution();
        }
    }
}