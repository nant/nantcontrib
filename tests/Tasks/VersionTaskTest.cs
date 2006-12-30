//
// NAntContrib
// Copyright (C) 2004 James Morris (jason.morris@intel.com)
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

using NUnit.Framework;

using NAnt.Core;
using NAnt.Contrib.Tasks;

namespace Tests.NAnt.Contrib.Tasks {
    [TestFixture]
    public class VersionTaskTest {
        VersionTask _task;

        [SetUp]
        public void SetUp() {
            _task = new VersionTask();
        }

        [TearDown]
        public void TearDown() {}

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void TestBadMajorNumber() {
            new Version("a.1.2.0");
        } 

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void TestBadMinorNumber() {
            new Version("1.a.2.0");
        } 

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void TestBadBuildNumber() {
            new Version("1.1.a.0");
        } 

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void TestBadrevisionNumber() {
            new Version("1.1.2.a");
        }

        [Test]
        public void TestVersionNumberToShort() {
            Version version = new Version("1.1.2");
            Assert.AreEqual(-1, version.Revision);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestVersionNumberToLong() {
            new Version("1.1.2.0.0");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void TestMissingMajorNumber() {
            new Version(".1.2.0");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void TestMissingMinorNumber() {
            new Version("1..2.0");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void TestMissingBuildNumber() {
            new Version("1.1..0");
        }
        
        [Test]
        [ExpectedException(typeof(FormatException))]
        public void TestMissingRevisionNumber() {
            new Version("1.1.2.");
        }

        [Test]
        public void TestDefaultPrefix() {
            Assert.AreEqual("buildnumber", _task.Prefix);
        }

        [Test]
        public void TestSetPrefix() {
            _task.Prefix = "build";
            Assert.AreEqual("build", _task.Prefix);
        }

        [Test]
        public void TestSetEmptyPrefix() {
            _task.Prefix = string.Empty;
            Assert.IsNull(_task.Prefix);
        }

        [Test]
        public void TestGoodBuildTypes() {
            Assert.AreEqual("Increment", VersionTask.BuildNumberAlgorithm.Increment.ToString());
            Assert.AreEqual("NoIncrement", VersionTask.BuildNumberAlgorithm.NoIncrement.ToString());
            Assert.AreEqual("MonthDay", VersionTask.BuildNumberAlgorithm.MonthDay.ToString());
        }

        [Test]
        public void TestGoodRevisionTypes() {
            Assert.AreEqual("Increment", VersionTask.RevisionNumberAlgorithm.Increment.ToString());
            Assert.AreEqual("Automatic", VersionTask.RevisionNumberAlgorithm.Automatic.ToString());
        }

        [Test]
        public void TestDefaultBuildType() {
            Assert.AreEqual(VersionTask.BuildNumberAlgorithm.MonthDay, _task.BuildType);
        }

        [Test]
        public void TestSetBuildType() {
            _task.BuildType = VersionTask.BuildNumberAlgorithm.Increment;
            Assert.AreEqual(VersionTask.BuildNumberAlgorithm.Increment, _task.BuildType);
        }

        [Test]
        public void TestDefaultRevisionType() {
            Assert.AreEqual(VersionTask.RevisionNumberAlgorithm.Automatic, _task.RevisionType);
        }

        [Test]
        public void TestSetRevisionType() {
            _task.RevisionType = VersionTask.RevisionNumberAlgorithm.Increment;
            Assert.AreEqual(VersionTask.RevisionNumberAlgorithm.Increment, _task.RevisionType);
        }
    }
}
