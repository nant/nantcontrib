// SolutionWriter.cs - the abstract interface for all solution writers
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

using System.Collections;
using System.IO;

namespace SLiNgshoT.Core {
    public interface SolutionWriter {
        void SetOutput(TextWriter output);
        void SetParameters(Hashtable parameters);

        void WriteStartSolution(Solution solution);

        void WriteStartProjectSourceFiles(Project project);
        void WriteProjectSourceFile(File file);
        void WriteEndProjectSourceFiles();

        void WriteStartProjectResXResourceFiles(Project project);
        void WriteProjectResXResourceFile(File file);
        void WriteEndProjectResXResourceFiles();

        void WriteStartProjectNonResXResourceFiles(Project project);
        void WriteProjectNonResXResourceFile(File file);
        void WriteEndProjectNonResXResourceFiles();

        void WriteStartProject(Project project);

        void WriteStartProjectDependencies();
        void WriteProjectDependency(Project project);
        void WriteProjectDependency(File file);
        void WriteEndProjectDependencies();

        void WriteStartResXFiles();
        void WriteResXFile(File file);
        void WriteEndResXFiles();

        void WriteStartAssembly();

        void WriteStartSourceFiles();
        void WriteSourceFile(File file);
        void WriteEndSourceFiles();

        void WriteStartReferences();
        void WriteReference(string name, bool built);
        void WriteReference(Project project);
        void WriteEndReferences();

        void WriteStartResources();
        void WriteResource(string path, string name, bool built);
        void WriteEndResources();

        void WriteStartCopyProjectAssemblies();
        void WriteCopyProjectAssembly(Project project);
        void WriteEndCopyProjectAssemblies();

        void WriteEndAssembly();

        void WriteEndProject();

        void WriteStartCleanTarget();
        void WriteCleanProject(Project project);
        void WriteEndCleanTarget();

        void WriteEndSolution();
    }
}
