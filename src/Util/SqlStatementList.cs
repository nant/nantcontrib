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
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using SourceForge.NAnt.Attributes;
using SourceForge.NAnt;

namespace NAnt.Contrib.Util
{ 

   /// <summary>
   /// Helper class to maintain a list of Sql Statements.
   /// </summary>
   public class SqlStatementList : IEnumerable
   {
      private StringCollection _statements;

      /// <summary>
      /// Number of statements in the list
      /// </summary>
      public int Count {
         get { return _statements.Count; }
      }
      /// <summary>
      /// Get the statement specified by the index
      /// </summary>
      public string this[int index] {
         get { return _statements[index].Trim(); }
      }

      /// <summary>
      /// Initializes a new instance.
      /// </summary>
      /// <param name="statements">A string containing all sql statements</param>
      /// <param name="delimiter">String that separates statements from each other</param>
      protected SqlStatementList(string statements, string delimiter)
      {
         _statements = new StringCollection();
         string[] list = Regex.Split(statements, delimiter);
         // 
         // strip single line comments
         // TODO: Deal with multiline ("/* */") comments
         //
         string st = null;
         for ( int i=0; i < list.Length; i++ ) {
            st = list[i].Trim();
            if ( st == string.Empty ) {
               continue;
            }
            if ( st.StartsWith("//") || st.StartsWith("--") ) {
               continue;
            }

            _statements.Add(st);
         }
      }

      /// <summary>
      /// Creates a new SqlStatementList initialized from
      /// the contents of a string
      /// </summary>
      /// <param name="statements">A string containing all sql statements</param>
      /// <param name="delimiter">String that separates statements from each other</param>
      /// <returns>The new instance</returns>
      public static SqlStatementList FromString(string statements, string delimiter)
      {
         return new SqlStatementList(statements, delimiter);
      }

      /// <summary>
      /// Creates a new SqlStatementList initialized from
      /// the contents of a file
      /// </summary>
      /// <param name="file">Path of file containing all sql statements</param>
      /// <param name="delimiter">String that separates statements from each other</param>
      /// <returns>The new instance</returns>
      public static SqlStatementList FromFile(string file, string delimiter)
      {
         string statements;
         StreamReader reader = null;
         try {
            reader = new StreamReader(File.OpenRead(file));
            statements = reader.ReadToEnd();
            return new SqlStatementList(statements, delimiter);
         } finally {
            if ( reader != null )
               reader.Close();
         }
      }

      /// <summary>
      /// Allows foreach().
      /// </summary>
      /// <returns></returns>
      public IEnumerator GetEnumerator()
      {
         return ((IEnumerable)_statements).GetEnumerator();
      }
      
   } // class SqlStatementList

} // namespace NAnt.Contrib.Util
