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
   /// Determines how the delimiter is 
   /// interpreted in a sql string
   /// </summary>
   public enum DelimiterStyle 
   {
      /// <summary>Delimiter can appear anywhere on a line</summary>
      Normal = 0,
      /// <summary>Delimiter always appears by itself on a line</summary>
      Line = 1
   } // enum DelimiterStyle

   /// <summary>
   /// Helper class to adapt SQL statements from some
   /// input into something OLEDB can consume directly
   /// </summary>
   public class SqlStatementAdapter
   {
      public static readonly string SEPARATOR = Environment.NewLine;
      private string _delimiter;


      /// <summary>
      /// Creates a new instance
      /// </summary>
      /// <param name="delimiter">String that separates statements from each other</param>
      /// <param name="style">Style of the delimiter</param>
      public SqlStatementAdapter(string delimiter, DelimiterStyle style)
      {
         if ( style == DelimiterStyle.Line ) {
            _delimiter = "^" + delimiter + "$";
         } else {
            _delimiter = delimiter;
         }
      }

      /// <summary>
      /// Adapts a set of Sql statements from a string.
      /// </summary>
      /// <param name="sql">A string containing the original sql statements</param>
      public string AdaptSql(string sql)
      {
         StringBuilder newsql = new StringBuilder("");

         StringReader reader = new StringReader(sql);
         string line = null;
         while ( (line = reader.ReadLine()) != null ) {
            line = line.Trim();
            if ( line == string.Empty ) {
               continue;
            }
            if ( line.StartsWith("//") || line.StartsWith("--") ) {
               continue;
            }
            string[] tokens = Regex.Split(line, _delimiter);
            foreach ( string t in tokens ) {
               if ( t != string.Empty ) {
                  newsql.Append(t + SEPARATOR);
               }
            }
         }
         return newsql.ToString();
      }

      /// <summary>
      /// Adapts a set of Sql statements from a string.
      /// </summary>
      /// <param name="file">Path of file containing all sql statements</param>
      /// <returns>The new instance</returns>
      public string AdaptSqlFile(string file)
      {
         string statements;
         StreamReader reader = null;
         try {
            reader = new StreamReader(File.OpenRead(file));
            statements = reader.ReadToEnd();
            return AdaptSql(statements);
         } finally {
            if ( reader != null )
               reader.Close();
         }
      }

   } // class SqlStatementAdapter

} // namespace NAnt.Contrib.Util
