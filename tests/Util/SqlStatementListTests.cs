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
using System.Security.Cryptography;
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

      public SqlStatementListTests(string name)
         : base(name)
      {
      }

      public void TestLoadFromString()
      {
         string statements = STATEMENT_1 + "\n  " + DELIMITER + STATEMENT_2
                           + DELIMITER + "   \n " + STATEMENT_3;

         SqlStatementList list = SqlStatementList.FromString(statements, DELIMITER);

         Assertion.AssertEquals(3, list.Count);
         Assertion.AssertEquals(STATEMENT_1, list[0]);
         Assertion.AssertEquals(STATEMENT_2, list[1]);
         Assertion.AssertEquals(STATEMENT_3, list[2]);
      }


      public void TestCommentStripping()
      {
         string statements = STATEMENT_1 + DELIMITER + "\n //" + STATEMENT_2
                           + DELIMITER + "   \n --" + STATEMENT_3 
                           + DELIMITER + STATEMENT_1;

         SqlStatementList list = SqlStatementList.FromString(statements, DELIMITER);

         Assertion.AssertEquals(2, list.Count);
         Assertion.AssertEquals(STATEMENT_1, list[0]);
         Assertion.AssertEquals(STATEMENT_1, list[1]);
      }

      public void TestIgnoreEmptyLines()
      {
         string statements = STATEMENT_1 + DELIMITER + "\n \n";

         SqlStatementList list = SqlStatementList.FromString(statements, DELIMITER);

         Assertion.AssertEquals(1, list.Count);
         Assertion.AssertEquals(STATEMENT_1, list[0]);
      }

   } // class SqlStatementListTests

} // namespace NAnt.Contrib.Tests.Util