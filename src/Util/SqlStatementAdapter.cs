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
using System.Text;

namespace NAnt.Contrib.Util {
    /// <summary>
    /// Helper class to adapt SQL statements from some
    /// input into something OLEDB can consume directly
    /// </summary>
    public class SqlStatementAdapter {
        public static readonly string SEPARATOR = Environment.NewLine;
        private readonly SqlStatementList _list;

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// 
        public SqlStatementAdapter(SqlStatementList stmtList) {
            _list = stmtList;
        }

        /// <summary>
        /// Adapts a set of Sql statements from a string.
        /// </summary>
        /// <param name="sql">A string containing the original sql statements</param>
        public string AdaptSql(string sql) {
            StringBuilder newSql = new StringBuilder("");

            _list.ParseSql(sql);
            foreach (string s in _list) {
                newSql.Append(s + SEPARATOR);
            }
            return newSql.ToString();

        }

        /// <summary>
        /// Adapts a set of Sql statements from a string.
        /// </summary>
        /// <param name="file">Path of file containing all sql statements</param>
        /// <param name="encoding">The encoding of the file containing the SQL statements.</param>
        /// <returns>The new instance</returns>
        public string AdaptSqlFile(string file, Encoding encoding) {
            StringBuilder newSql = new StringBuilder("");

            _list.ParseSqlFromFile(file, encoding);
            foreach (string sql in _list) {
                newSql.Append(sql + SEPARATOR);
            }
            return newSql.ToString();
        }
    }
}
