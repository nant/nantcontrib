//
// NAntContrib
// Copyright (C) 2002 Tomas Restrepo (tomasr@mvps.org)
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
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using NUnit.Framework;

using NAnt.Core;
using NAnt.Core.Attributes;

using NAnt.Contrib.Util;

namespace Tests.NAnt.Contrib.Util {
    /// <summary>
    /// SqlStatementList class tests
    /// </summary>
    [TestFixture]
    public class SqlStatementListTests {
        private const string STATEMENT_1 = "select * from tables";
        private const string STATEMENT_2 = "insert into tables values(1,2,3)";
        private const string STATEMENT_3 = "drop tables";
        private const string STATEMENT_GOTO = "goto error_handling";
        private const string DELIMITER = ";";

        private const string ProjectXml = @"<?xml version='1.0'?>
            <project name='ProjectTest' default='test'>
                <target name='test' />
            </project>";

        public void TestLoadFromString() {
            string statements = STATEMENT_1 + "\n  " + DELIMITER + STATEMENT_2
                + DELIMITER + "   \n " + STATEMENT_3;

            SqlStatementList list = new SqlStatementList(DELIMITER, DelimiterStyle.Normal);
            list.ParseSql(statements);

            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(STATEMENT_1 + Environment.NewLine, list[0]);
            Assert.AreEqual(STATEMENT_2 + Environment.NewLine, list[1]);
            Assert.AreEqual(STATEMENT_3 + Environment.NewLine, list[2]);
        }

        public void TestCommentStripping() {
            string statements = STATEMENT_1 + DELIMITER + "\n //" + STATEMENT_2
                + DELIMITER + "   \n --" + STATEMENT_3
                + DELIMITER + "\n" + STATEMENT_1;

            SqlStatementList list = new SqlStatementList(DELIMITER, DelimiterStyle.Normal);
            list.ParseSql(statements);

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(STATEMENT_1 + Environment.NewLine, list[0]);
            Assert.AreEqual(STATEMENT_1 + Environment.NewLine, list[1]);
        }

        public void TestIgnoreEmptyLines() {
            string statements = STATEMENT_1 + DELIMITER + "\n \n";

            SqlStatementList list = new SqlStatementList(DELIMITER, DelimiterStyle.Normal);
            list.ParseSql(statements);

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(STATEMENT_1 + Environment.NewLine, list[0]);
        }

        public void TestGoLineBatch() {
            string goDelimiter = "go";

            string statements =
                STATEMENT_1 + Environment.NewLine
                + STATEMENT_2 + Environment.NewLine
                + goDelimiter + Environment.NewLine
                + STATEMENT_3 + Environment.NewLine
                + goDelimiter + Environment.NewLine
                + STATEMENT_3 + Environment.NewLine
                + goDelimiter + Environment.NewLine
                + "-- " + STATEMENT_3;

            SqlStatementList list = new SqlStatementList(goDelimiter, DelimiterStyle.Line);
            list.ParseSql(statements);

            Assert.AreEqual(4, list.Count);
            Assert.AreEqual(STATEMENT_1 + Environment.NewLine + STATEMENT_2 + Environment.NewLine, list[0], "Statement 1");
            Assert.AreEqual(STATEMENT_3 + Environment.NewLine, list[1], "Statement 3.1");
            Assert.AreEqual(STATEMENT_3 + Environment.NewLine, list[2], "Statement 3.2");
            Assert.AreEqual("-- " + STATEMENT_3 + Environment.NewLine, list[3], "Comment");
        }

        public void TestDifferentGoDelimiters() {
            string goDelimiter1 = "go";
            string goDelimiter2 = "gO";

            string statements =
                STATEMENT_1 + Environment.NewLine
                + STATEMENT_2 + Environment.NewLine
                + goDelimiter1 + Environment.NewLine
                + STATEMENT_3 + Environment.NewLine
                + goDelimiter1 + Environment.NewLine
                + STATEMENT_3 + Environment.NewLine
                + goDelimiter2 + Environment.NewLine
                + "-- " + STATEMENT_3;

            SqlStatementList list = new SqlStatementList(goDelimiter1, DelimiterStyle.Line);
            list.ParseSql(statements);

            Assert.AreEqual(4, list.Count);
            Assert.AreEqual(STATEMENT_1 + Environment.NewLine + STATEMENT_2  + Environment.NewLine, list[0], "Statement 1");
            Assert.AreEqual(STATEMENT_3 + Environment.NewLine, list[1], "Statement 3.1");
            Assert.AreEqual(STATEMENT_3 + Environment.NewLine, list[2], "Statement 3.2");
            Assert.AreEqual("-- " + STATEMENT_3 + Environment.NewLine, list[3], "Comment");
        }

        public void TestLineSpawningDelimiter() {
            string delimiter = "#";

            string statements =
                STATEMENT_1 + Environment.NewLine
                + STATEMENT_2 + delimiter + STATEMENT_3 + Environment.NewLine
                + delimiter + "ABC";

            SqlStatementList list = new SqlStatementList(delimiter, DelimiterStyle.Normal);
            list.ParseSql(statements);

            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(STATEMENT_1 + Environment.NewLine + STATEMENT_2, 
                list[0], "Statement 1");
            Assert.AreEqual(STATEMENT_3 + Environment.NewLine, list[1], 
                "Statement 2");
            Assert.AreEqual("ABC" + Environment.NewLine, list[2], "Statement 3");
        }

        public void TestKeepLineFormatting() {
            string goDelimiter = "go";

            string statements = "\t" +
                STATEMENT_1 + Environment.NewLine
                + "\t" + STATEMENT_2 + Environment.NewLine;

            SqlStatementList list = new SqlStatementList(goDelimiter, DelimiterStyle.Line);
            list.ParseSql(statements);

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(statements, list[0]);
        }

        public void TestPropertyReplacement() {
            string sqlWithPropertyTags = @"use ${dbName}";
            string expectedSqlStatement = @"use master";

            string goDelimiter = "go";

            string inputStatements = "\t" +
                sqlWithPropertyTags + Environment.NewLine
                + "\t" + sqlWithPropertyTags + Environment.NewLine;

            string expectedStatements = "\t" +
                expectedSqlStatement + Environment.NewLine
                + "\t" + expectedSqlStatement + Environment.NewLine;

            SqlStatementList list = new SqlStatementList(goDelimiter, DelimiterStyle.Line);

            string buildFile = null;

            try {
                // persist buildfile
                buildFile = CreateFileWithContents(ProjectXml);

                // create project for buildfile
                Project project = new Project(buildFile, Level.Info, 0);

                list.Properties = new PropertyDictionary(project);
                list.Properties.Add("dbName", "master");

                list.ParseSql(inputStatements);

                Assert.AreEqual(1, list.Count);
                Assert.AreEqual(expectedStatements, list[0]);
            } finally {
                // make sure temp buildfile is deleted
                if (buildFile != null) {
                    File.Delete(buildFile);
                }
            }
        }

        public void TestGoSeparatorMustNotSplitGoto() {
            string goDelimiter = "go";

            string statements = STATEMENT_1 + Environment.NewLine + STATEMENT_GOTO + Environment.NewLine;

            SqlStatementList list = new SqlStatementList(goDelimiter, DelimiterStyle.Line);
            list.ParseSql(statements);

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(statements, list[0]);
        }

        private static string CreateFileWithContents(string contents) {
            // get path of new temp file
            string fileName = Path.GetTempFileName();

            // write the text into the temp file.
            using (FileStream f = new FileStream(fileName, FileMode.Create)) {
                StreamWriter s = new StreamWriter(f);
                s.Write(contents);
                s.Close();
                f.Close();
            }

            // return path of temp file
            return fileName;
        }
    }
}
