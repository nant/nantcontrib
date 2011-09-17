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
// Gert Driesen (drieseng@users.sourceforge.net)

using System;

using NUnit.Framework;

using Tests.NAnt.Contrib;

namespace Tests.NAnt.Contrib.Tasks {
    /// <summary>
    /// Tests the &lt;cd&gt; task.
    /// </summary>
    [TestFixture]
    public class ChangeDirectoryTest : BuildTestBase {
        [Test]
        public void Test_RelativePath() {
            string _xml = @"
                    <project>
                        <!-- store original working directory -->
                        <property name=""original.workingdir"" value=""${directory::get-current-directory()}"" />
                        <!-- ensure directory exists -->
                        <mkdir dir=""subdir1"" />
                        <!-- change working directory -->
                        <cd dir=""subdir1"" />
                        <!-- verify success -->
                        <fail if=""${directory::get-current-directory() != path::combine(project::get-base-directory(), 'subdir1')}"" />
                        <!-- restore original working directory to allow test cleanup -->
                        <cd dir=""${original.workingdir}"" />
                    </project>";
            RunBuild(_xml);
        }

        [Test]
        public void Test_AbsolutePath() {
            string _xml = @"
                    <project>
                        <!-- store original working directory -->
                        <property name=""original.workingdir"" value=""${directory::get-current-directory()}"" />
                        <!-- ensure directory exists -->
                        <mkdir dir=""subdir2"" />
                        <!-- change working directory -->
                        <cd dir=""${path::combine(project::get-base-directory(), 'subdir2')}"" />
                        <!-- verify success -->
                        <fail if=""${directory::get-current-directory() != path::combine(project::get-base-directory(), 'subdir2')}"" />
                        <!-- restore original working directory to allow test cleanup -->
                        <cd dir=""${original.workingdir}"" />
                    </project>";
            RunBuild(_xml);
        }

        [Test]
        [ExpectedException(typeof(TestBuildException))]
        public void Test_NoDir() {
            string _xml = @"
                    <project>
                        <cd />
                    </project>";
            RunBuild(_xml);
        }

        [Test]
        [ExpectedException(typeof(TestBuildException))]
        public void Test_NonExistingDirectory() {
            string _xml = @"
                    <project>
                        <cd dir=""doesnotexist"" />
                    </project>";
            RunBuild(_xml);
        }
    }
}
