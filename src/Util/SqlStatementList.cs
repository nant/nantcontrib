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
using NAnt.Core.Attributes;
using NAnt.Core;

namespace NAnt.Contrib.Util { 
    /// <summary>
    /// Determines how the delimiter is 
    /// interpreted in a sql string
    /// </summary>
    public enum DelimiterStyle {
        /// <summary>Delimiter can appear anywhere on a line</summary>
        Normal = 0,
        /// <summary>Delimiter always appears by itself on a line</summary>
        Line = 1
    }

    /// <summary>
    /// Helper class to maintain a list of Sql Statements.
    /// </summary>
    public class SqlStatementList : IEnumerable {
        private StringCollection _statements;
        private string _delimiter;
        private DelimiterStyle _style;
        private PropertyDictionary _properties;

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
            get { return _statements[index]; }
        }

        /// <summary>
        /// Project's properties for property expansion
        /// </summary>
        public PropertyDictionary Properties {
            get { return _properties; }
            set { _properties = value; }
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="delimiter">String that separates statements from each other</param>
        /// <param name="style">Style of the delimiter</param>
        public SqlStatementList(string delimiter, DelimiterStyle style) {
            _statements = new StringCollection();
            _style     = style;
            _delimiter = delimiter;
        }

        /// <summary>
        /// Parses the Sql into the internal list using the specified delimiter
        /// and delimiter style
        /// </summary>
        /// <param name="sql"></param>
        public void ParseSql(string sql) {
            StringReader reader = new StringReader(ExpandProps(sql));
            string line = null;
            StringBuilder sqlStatement = new StringBuilder();

            while ( (line = reader.ReadLine()) != null ) {

                if ( line.Trim() == string.Empty ) {
                    continue;
                }

                if (_style == DelimiterStyle.Normal) {
                    if ( line.Trim().StartsWith("//") || line.Trim().StartsWith("--") ) {
                        continue;
                    }

                    string[] tokens = Regex.Split(line.Trim(), _delimiter);
                    foreach ( string t in tokens ) {
                        if ( t != string.Empty ) {
                            _statements.Add(t);
                        }
                    }
                }
                    // DelimiterStyle.Line
                else {
                    if (line.Trim().ToUpper().Equals(_delimiter.ToUpper())) {
                        if (sqlStatement.ToString().Trim().Length > 0)
                            _statements.Add(sqlStatement.ToString());

                        sqlStatement = new StringBuilder();
                        continue;
                    }

                    sqlStatement.Append(line + Environment.NewLine);
                }

            }
            if (sqlStatement.Length > 0)
                _statements.Add(sqlStatement.ToString());
        }
  
        /// <summary>
        /// Parses the contents of the file into the 
        /// internal list using the specified delimiter
        /// and delimiter style
        /// </summary>
        /// <param name="file">File name</param>
        public void ParseSqlFromFile(string file) {
            string statements;
            StreamReader reader = null;
            try {
                reader = new StreamReader(File.OpenRead(file));
                statements = reader.ReadToEnd();
                ParseSql(statements);
            } finally {
                if ( reader != null )
                    reader.Close();
            }
        }

        /// <summary>
        /// Allows foreach().
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator() {
            return ((IEnumerable)_statements).GetEnumerator();
        }
      

        /// <summary>
        /// Strips all single line comments 
        /// in the specified sql
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private string StripComments(string sql) {
            StringReader reader = new StringReader(sql);
            StringBuilder newSql = new StringBuilder("");

            string line = null;
            while ( (line = reader.ReadLine()) != null ) {
                line = line.Trim();
                if ( line == string.Empty ) {
                    continue;
                }
                if ( line.StartsWith("//") || line.StartsWith("--") ) {
                    continue;
                }

                newSql.Append(line + Environment.NewLine);
            }

            return newSql.ToString();
        }

        /// <summary>
        /// Expands project properties in the
        /// sql string
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private string ExpandProps(string sql) {
            if ( Properties == null )
                return sql;
            return Properties.ExpandProperties(sql, null);
        }
    }
}
