// Main.cs - a SLiNgshoT console driver
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

using SLiNgshoT.Core;

class SLiNgshoT_Console
{
    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            WriteUsage();
        }
        else
        {
            string format;
            string solution;
            Hashtable parameters = new Hashtable();
            Hashtable uriMap = new Hashtable();

            Driver.ParseArgs(args, out format, out solution, parameters, uriMap);

            if (format == null)
            {
                Console.Error.WriteLine("no output format specified");
            }
            else
            {
                if (solution == null)
                {
                    solution = Driver.FindSolution(Environment.CurrentDirectory);
                }

                SolutionWriter writer = null;

                switch (format)
                {
                    case "nant":
                        writer = new NAntWriter();
                        break;
                    case "nmake":
                        writer = new NMakeWriter();
                        break;
                }

                if (writer == null)
                {
                    Console.Error.WriteLine("{0} is an unsupported format.", format);
                }
                else
                {
                    Driver.WriteSolution(writer, Console.Out, solution, parameters, uriMap);
                }
            }
        }
    }

    private static void WriteUsage()
    {
        Console.Error.WriteLine("usage: SLiNgshoT <format> [-sln solution] [-map uri-prefix file-prefix]* [name=value]*");
        Console.Error.WriteLine();
        Console.Error.Write("formats: ");
        IList outputFormats = Driver.GetOutputFormats();

        bool first = true;
        foreach (string format in outputFormats)
        {
            if (!first)
            {
                Console.Error.Write(", ");
            }
            else
            {
                first = false;
            }

            Console.Error.Write("-" + format);
        }

        Console.Error.WriteLine();
        Console.Error.WriteLine();
        Console.Error.WriteLine("if -sln is not specified, uses the only .sln file in the current directory");
        Console.Error.WriteLine();
        Console.Error.WriteLine("parameters:");

        foreach (string format in outputFormats)
        {
            OutputParameterAttribute[] outputParameters = Driver.GetOutputParameters(format);

            if (outputParameters.Length > 0)
            {
                Console.Error.WriteLine("    {0}:", format);

                foreach (OutputParameterAttribute outputParameter in outputParameters)
                {
                    Console.Error.WriteLine(
                        "      {0}: {2} ({1})", 
                        outputParameter.Name, 
                        outputParameter.Required ? "REQUIRED" : "OPTIONAL", 
                        outputParameter.Description);
                }
            }
        }

        Console.Error.WriteLine();
        Console.Error.WriteLine("examples:");
        Console.Error.WriteLine("  SLiNgshoT -nant build.basedir=..\\..\\bin");
        Console.Error.WriteLine("  SLiNgshoT -nmake -sln Example.sln -map http://localhost/ C:\\Inetpub\\wwwroot\\");
    }
}
