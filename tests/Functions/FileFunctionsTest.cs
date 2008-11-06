// NAnt - A .NET build tool
// Copyright (C) 2001-2008 Gerry Shaw
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

using NUnit.Framework;

using NAnt.Core;

namespace Tests.NAnt.Contrib.Functions {
    [TestFixture]
    public class FileFunctionsTest : BuildTestBase {
        #region Public Instance Methods

        [Test]
        public void GetChecksum() {
            string buildFragment1 =
                "<project>" +
                "   <echo file=\"checksum.input\">test input file</echo>" +
                "   <property name=\"checksum\" value=\"${file::get-checksum('checksum.input','MD5')}\" />" +
                "   <fail unless=\"${checksum == '91ddb9a8a8fc8f96c182b2b7e8c65f52'}\">${checksum}</fail>" +
                "</project>";

            RunBuild(buildFragment1);

            string buildFragment2 =
                "<project>" +
                "   <echo file=\"checksum.input\">test input file</echo>" +
                "   <property name=\"checksum\" value=\"${file::get-checksum('checksum.input','sha1')}\" />" +
                "   <fail unless=\"${checksum == '124a5468603f36e91980bb345aff00308267fc7e'}\">${checksum}</fail>" +
                "</project>";

            RunBuild(buildFragment2);
        }

        [Test]
        public void GetChecksum_Path_DoesNotExist() {
            string buildFragment =
                "<project>" +
                "   <property name=\"checksum\" value=\"${file::get-checksum('doesnotexist','MD5')}\" />" +
                "</project>";

            try {
                RunBuild(buildFragment);
                Assert.Fail ("#1");
            } catch (TestBuildException ex) {
                Assert.IsNotNull (ex.InnerException, "#2");
                BuildException be = ex.InnerException as BuildException;
                Assert.IsNotNull (be, "#3");
                Assert.AreEqual (typeof (BuildException), be.GetType (), "#4");
                Assert.IsNotNull (be.InnerException, "#5");
                Assert.AreEqual (typeof (FileNotFoundException), be.InnerException.GetType (), "#6");
            }
        }

        [Test]
        public void GetChecksum_Algorithm_DoesNotExist() {
            string buildFragment =
                "<project>" +
                "   <echo file=\"checksum.input\">test input file</echo>" +
                "   <property name=\"checksum\" value=\"${file::get-checksum('checksum.input','ZZZ')}\" />" +
                "</project>";

            try {
                RunBuild(buildFragment);
                Assert.Fail ("#1");
            } catch (TestBuildException ex) {
                Assert.IsNotNull (ex.InnerException, "#2");
                BuildException be = ex.InnerException as BuildException;
                Assert.IsNotNull (be, "#3");
                Assert.AreEqual (typeof (BuildException), be.GetType (), "#4");
                Assert.IsNotNull (be.InnerException, "#5");
                Assert.AreEqual (typeof (ArgumentException), be.InnerException.GetType (), "#6");
            }
        }

        #endregion Public Instance Methods
    }
}
