//
// NAntContrib
// Copyright (C) 2001-2005 Gerry Shaw
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

using System;
using System.IO;

using NUnit.Framework;

namespace Tests.NAnt.Contrib {
    public sealed class TempFile {
        /// <summary>
        /// Creates a small temp file returns the file name.
        /// </summary>
        public static string Create() {
            return Create(Path.GetTempFileName());
        }

        public static string Create(string fileName) {
            string contents = "You can delete this file." + Environment.NewLine;
            return CreateWithContents(contents, fileName);
        }

        public static string CreateWithContents(string contents) {
            return CreateWithContents(contents, Path.GetTempFileName());
        }

        public static string CreateWithContents(string contents, string fileName) {
            // ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));

            // write the text into the temp file.
            using (FileStream f = new FileStream(fileName, FileMode.Create)) {
                StreamWriter s = new StreamWriter(f);
                s.Write(contents);
                s.Close();
                f.Close();
            }

            if (!File.Exists(fileName)) {
                throw new AssertionException("TempFile: " + fileName + " wasn't created.");
            }

            return fileName;
        }

        public static string Read(string fileName) {
            string contents;
            using (StreamReader s = File.OpenText(fileName)) {
                contents = s.ReadToEnd();
                s.Close();
            }
            return contents;
        }
    }
}
