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
//using System.Security.Cryptography;
using NUnit.Framework;
using SourceForge.NAnt.Attributes;
using SourceForge.NAnt;

using NAnt.Contrib.Util;

namespace NAnt.Contrib.Tests.Util
{
   /// <summary>
   /// SqlStatementList class tests
   /// </summary>
   public class SqlStatementListTests : TestCase
   {

      const string STATEMENT_1 = "select * from tables";
      const string STATEMENT_2 = "insert into tables values(1,2,3)";
      const string STATEMENT_3 = "drop tables";
      const string DELIMITER = ";";

      public SqlStatementListTests(string name) : base(name)
      {
      }

      public void TestLoadFromString()
      {
         string statements = STATEMENT_1 + "\n  " + DELIMITER + STATEMENT_2
            + DELIMITER + "   \n " + STATEMENT_3;

         SqlStatementList list = new SqlStatementList(DELIMITER, DelimiterStyle.Normal);
         list.ParseSql(statements);

         Assertion.AssertEquals(3, list.Count);
         Assertion.AssertEquals(STATEMENT_1, list[0]);
         Assertion.AssertEquals(STATEMENT_2, list[1]);
         Assertion.AssertEquals(STATEMENT_3, list[2]);
      }


      public void TestCommentStripping()
      {
         string statements = STATEMENT_1 + DELIMITER + "\n //" + STATEMENT_2
            + DELIMITER + "   \n --" + STATEMENT_3
            + DELIMITER + "\n" + STATEMENT_1;

         SqlStatementList list = new SqlStatementList(DELIMITER, DelimiterStyle.Normal);
         list.ParseSql(statements);

         Assertion.AssertEquals(2, list.Count);
         Assertion.AssertEquals(STATEMENT_1, list[0]);
         Assertion.AssertEquals(STATEMENT_1, list[1]);
      }

      public void TestIgnoreEmptyLines()
      {
         string statements = STATEMENT_1 + DELIMITER + "\n \n";

         SqlStatementList list = new SqlStatementList(DELIMITER, DelimiterStyle.Normal);
         list.ParseSql(statements);

         Assertion.AssertEquals(1, list.Count);
         Assertion.AssertEquals(STATEMENT_1, list[0]);
      }

      public void TestGoLineBatch()
      {
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

         Assertion.AssertEquals(4, list.Count);
         Assertion.AssertEquals("Statement 1", STATEMENT_1 + Environment.NewLine + STATEMENT_2 + Environment.NewLine, list[0]);
         Assertion.AssertEquals("Statement 3.1", STATEMENT_3 + Environment.NewLine, list[1]);
         Assertion.AssertEquals("Statement 3.2", STATEMENT_3 + Environment.NewLine, list[2]);
         Assertion.AssertEquals("Comment", "-- " + STATEMENT_3 + Environment.NewLine, list[3]);
      }

      public void TestDifferentGoDelimiters()
      {
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

         Assertion.AssertEquals(4, list.Count);
         Assertion.AssertEquals("Statement 1", STATEMENT_1 + Environment.NewLine + STATEMENT_2  + Environment.NewLine, list[0]);
         Assertion.AssertEquals("Statement 3.1", STATEMENT_3 + Environment.NewLine, list[1]);
         Assertion.AssertEquals("Statement 3.2", STATEMENT_3 + Environment.NewLine, list[2]);
         Assertion.AssertEquals("Comment", "-- " + STATEMENT_3 + Environment.NewLine, list[3]);
      }

      public void TestKeepLineFormatting()
      {
         string goDelimiter = "go";

         string statements = "\t" +
            STATEMENT_1 + Environment.NewLine
            + "\t" + STATEMENT_2 + Environment.NewLine;

         SqlStatementList list = new SqlStatementList(goDelimiter, DelimiterStyle.Line);
         list.ParseSql(statements);

         Assertion.AssertEquals(1, list.Count);
         Assertion.AssertEquals(statements, list[0]);
      }

      public void TestPropertyReplacement()
      {
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

         list.Properties = new PropertyDictionary();
         list.Properties.Add("dbName", "master");

         list.ParseSql(inputStatements);

         Assertion.AssertEquals(1, list.Count);
         Assertion.AssertEquals(expectedStatements, list[0]);
      }

   } // class SqlStatementListTests

} // namespace NAnt.Contrib.Tests.Util
