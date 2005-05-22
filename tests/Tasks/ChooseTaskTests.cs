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
// Gert Driesen (gert.driesen@ardatis.com)

using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using NUnit.Framework;

using Tests.NAnt.Contrib;

namespace NAnt.Contrib.Tests.Tasks {
    [TestFixture]
    public class ChooseTaskTest : BuildTestBase {
        [Test]
        public void Test_ConditionalExecution1() {
            const string _xml = @"
                    <project>
                        <choose>
                            <when test=""false"">
                                <property name=""when1"" value=""executed"" />
                            </when>
                            <when test=""true"">
                                <property name=""when2"" value=""executed"" />
                            </when>
                            <otherwise>
                                <property name=""otherwise"" value=""executed"" />
                            </otherwise>
                        </choose>
                        <fail if=""${property::exists('when1')}"">#1</fail>
                        <fail unless=""${property::exists('when2')}"">#2</fail>
                        <fail if=""${property::exists('otherwise')}"">#3</fail>
                    </project>";
            RunBuild(_xml);
        }

        [Test]
        public void Test_ConditionalExecution2() {
            const string _xml = @"
                    <project>
                        <choose>
                            <when test=""true"">
                                <property name=""when1"" value=""executed"" />
                            </when>
                            <when test=""false"">
                                <property name=""when2"" value=""executed"" />
                            </when>
                        </choose>
                        <fail unless=""${property::exists('when1')}"">#1</fail>
                        <fail if=""${property::exists('when2')}"">#2</fail>
                    </project>";
            RunBuild(_xml);
        }

        [Test]
        public void Test_ConditionalExecution3() {
            const string _xml = @"
                    <project>
                        <choose>
                            <when test=""false"">
                                <property name=""when1"" value=""executed"" />
                            </when>
                            <when test=""false"">
                                <property name=""when2"" value=""executed"" />
                            </when>
                        </choose>
                        <fail if=""${property::exists('when1')}"">#1</fail>
                        <fail if=""${property::exists('when2')}"">#2</fail>
                    </project>";
            RunBuild(_xml);
        }

        [Test]
        public void Test_Fallback() {
            const string _xml = @"
                    <project>
                        <choose>
                            <when test=""false"">
                                <property name=""when1"" value=""executed"" />
                            </when>
                            <otherwise>
                                <property name=""otherwise"" value=""executed"" />
                            </otherwise>
                        </choose>
                        <fail if=""${property::exists('when1')}"">#1</fail>
                        <fail unless=""${property::exists('otherwise')}"">#2</fail>
                    </project>";
            RunBuild(_xml);
        }

        [Test]
        [ExpectedException(typeof(TestBuildException))]
        public void Test_ChildOrder1() {
            const string _xml = @"
                    <project>
                        <choose>
                            <when test=""false"">
                                <property name=""when1"" value=""executed"" />
                            </when>
                            <otherwise>
                                <property name=""otherwise"" value=""executed"" />
                            </otherwise>
                            <when test=""true"">
                                <property name=""when2"" value=""executed"" />
                            </when>
                        </choose>
                    </project>";
            RunBuild(_xml);
        }

        [Test]
        [ExpectedException(typeof(TestBuildException))]
        public void Test_ChildOrder2() {
            const string _xml = @"
                    <project>
                        <choose>
                            <otherwise>
                                <property name=""otherwise"" value=""executed"" />
                            </otherwise>
                            <when test=""false"">
                                <property name=""when1"" value=""executed"" />
                            </when>
                        </choose>
                    </project>";
            RunBuild(_xml);
        }

        [Test]
        public void Test_EmptyWhenChild() {
            const string _xml = @"
                    <project>
                        <choose>
                            <when test=""true"" />
                        </choose>
                    </project>";
            RunBuild(_xml);
        }

        [Test]
        public void Test_EmptyOtherwiseChild() {
            const string _xml = @"
                    <project>
                        <choose>
                            <when test=""false"" />
                            <otherwise />
                        </choose>
                    </project>";
            RunBuild(_xml);
        }

        [Test]
        [ExpectedException(typeof(TestBuildException))]
        public void Test_MissingWhenChild1() {
            const string _xml = @"
                    <project>
                        <choose />
                    </project>";
            RunBuild(_xml);
        }

        [Test]
        [ExpectedException(typeof(TestBuildException))]
        public void Test_MissingWhenChild2() {
            const string _xml = @"
                    <project>
                        <choose>
                            <otherwise>
                                <property name=""otherwise"" value=""executed"" />
                            </otherwise>
                        </choose>
                    </project>";
            RunBuild(_xml);
        }

        [Test]
        [ExpectedException(typeof(TestBuildException))]
        public void Test_InvalidWhenCondition() {
            const string _xml = @"
                    <project>
                        <choose>
                            <when test=""whatever"">
                                <property name=""when1"" value=""executed"" />
                            </when>
                        </choose>
                    </project>";
            RunBuild(_xml);
        }

        [Test]
        [ExpectedException(typeof(TestBuildException))]
        public void Test_MissingWhenCondition() {
            const string _xml = @"
                    <project>
                        <choose>
                            <when>
                                <property name=""when1"" value=""executed"" />
                            </when>
                        </choose>
                    </project>";
            RunBuild(_xml);
        }

        [Test]
        [ExpectedException(typeof(TestBuildException))]
        public void Test_EmptyWhenCondition() {
            const string _xml = @"
                    <project>
                        <choose>
                            <when test="""">
                                <property name=""when1"" value=""executed"" />
                            </when>
                        </choose>
                    </project>";
            RunBuild(_xml);
        }

        [Test]
        [ExpectedException(typeof(TestBuildException))]
        public void Test_InvalidChild() {
            const string _xml = @"
                    <project>
                        <choose>
                            <when test=""true"">
                                <property name=""when1"" value=""executed"" />
                            </when>
                            <if test=""true"" />
                        </choose>
                    </project>";
            RunBuild(_xml);
        }

        [Test]
        [ExpectedException(typeof(TestBuildException))]
        public void Test_InvalidTasks() {
            const string _xml = @"
                    <project>
                        <choose>
                            <when test=""true"">
                                <doesnotexist />
                            </when>
                        </choose>
                    </project>";
            RunBuild(_xml);
        }

        [Test]
        [ExpectedException(typeof(TestBuildException))]
        public void Test_InvalidWhenParameter() {
            const string _xml = @"
                    <project>
                        <choose>
                            <when test=""true"" if=""true"">
                                <property name=""when1"" value=""executed"" />
                            </when>
                        </choose>
                    </project>";
            RunBuild(_xml);
        }

        [Test]
        [ExpectedException(typeof(TestBuildException))]
        public void Test_InvalidOtherwiseParameter() {
            const string _xml = @"
                    <project>
                        <choose>
                            <when test=""true"">
                                <property name=""when1"" value=""executed"" />
                            </when>
                            <otherwise if=""true"">
                                <property name=""otherwise"" value=""executed"" />
                            </otherwise>
                        </choose>
                    </project>";
            RunBuild(_xml);
        }

        [Test]
        public void Test_FailOnError_False() {
            const string _xml = @"
                    <project>
                        <choose failonerror=""false"">
                            <when test=""true"">
                                <fail>Some reason</fail>
                            </when>
                        </choose>
                    </project>";
            RunBuild(_xml);
        }

        [Test]
        [ExpectedException(typeof(TestBuildException))]
        public void Test_FailOnError_True() {
            const string _xml = @"
                    <project>
                        <choose>
                            <when test=""true"">
                                <fail>Some reason</fail>
                            </when>
                        </choose>
                    </project>";
            RunBuild(_xml);
        }
    }
}
