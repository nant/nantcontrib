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

namespace NAnt.Contrib.Tests
{ 


   /// <summary>
   /// All Tests
   /// </summary>
   public class AllTests
   {
      public static ITest Suite {
         get {

            // Force the loading of the correct NAnt.Contrib assembly to test against.
            Assembly assembly = Assembly.GetExecutingAssembly(); // build/NAnt.Tests.dll
            string path = Path.GetDirectoryName(assembly.Location);

            // force NAnt.Contrib.Tasks.dll to be loaded
            string corePath = Path.Combine(path, "NAnt.Contrib.Tasks.dll");
            Assembly tasks = Assembly.LoadFrom(corePath);
            // add tests in NAnt.Contrib.Tasks.DLL to NAnt
            TaskFactory.AddTasks(tasks);

            // Use reflection to automagically scan all the classes that 
            // inherit from TestCase and add them to the suite.
            TestSuite suite = new TestSuite("NAnt.Contrib Tests");
            foreach(Type type in assembly.GetTypes()) {
               if (type.IsSubclassOf(typeof(TestCase)) && !type.IsAbstract) {
                  suite.AddTestSuite(type);
               }
            }
            return suite;
         }
      }

   } // class AllTests

} // namespace NAnt.Contrib.Tests
