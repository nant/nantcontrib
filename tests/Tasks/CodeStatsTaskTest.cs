// NAnt - A .NET build tool
// Copyright (C) 2001-2005 Gert Driesen
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
// Gert Driesen (drieseng@users.sourceforge.net)

using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using NUnit.Framework;

using Tests.NAnt.Contrib;

namespace NAnt.Contrib.Tests.Tasks {
    [TestFixture]
    public class CodeStatsTaskTest : BuildTestBase {
        [Test]
        public void Test_CatchExceptionWithoutMessage() {
            CreateTempFile("test.cs",
                "// comment" + Environment.NewLine
                + "//" + Environment.NewLine + Environment.NewLine
                + "/*" + Environment.NewLine 
                + "comment" + Environment.NewLine
                + "*/" + Environment.NewLine + Environment.NewLine
                + "using System;");

            string _xml = @"
                    <project>
                        <codestats output='test.xml' append='false' buildname='MyTestBuild'>
                            <counts>
                                <count label='C#'>
                                    <fileset>
                                        <include name='**/*.cs' />
                                    </fileset>
                                </count>
                            </counts>
                        </codestats>
                    </project>";
            RunBuild(_xml);

            using (FileStream fs = new FileStream(Path.Combine(TempDirName, "test.xml"), FileMode.Open, FileAccess.Read)) {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(fs);

                XmlElement lineCountElement = (XmlElement) xmlDoc.SelectSingleNode(
                    "/code-summaries/code-summary/linecount");

                Assert.AreEqual("8", lineCountElement.GetAttribute("totalLineCount"));
                Assert.AreEqual("2", lineCountElement.GetAttribute("emptyLineCount"));
                Assert.AreEqual("5", lineCountElement.GetAttribute("commentLineCount"));

                fs.Close();
            }
        }
    }
}
