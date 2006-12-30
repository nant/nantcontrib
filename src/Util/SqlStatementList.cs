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

using NAnt.Core;

namespace NAnt.Contrib.Util {
    /// <summary>
    /// Determines how the delimiter is interpreted in a SQL string.
    /// </summary>
    public enum DelimiterStyle {
        /// <summary>
        /// Delimiter can appear anywhere on a line.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Delimiter always appears by itself on a line.
        /// </summary>
        Line = 1
    }

    /// <summary>
    /// Helper class to maintain a list of SQL Statements.
    /// </summary>
    public class SqlStatementList : IEnumerable {
        private readonly StringCollection _statements;
        private readonly string _delimiter;
        private readonly DelimiterStyle _style;
        private PropertyDictionary _properties;

        /// <summary>
        /// Gets the number of statements in the list.
        /// </summary>
        public int Count {
            get { return _statements.Count; }
        }
        /// <summary>
        /// Gets the statement specified by the index.
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
            _style = style;
            _delimiter = delimiter;
        }

        /// <summary>
        /// Parses the SQL into the internal list using the specified delimiter
        /// and delimiter style
        /// </summary>
        /// <param name="sql">The SQL string to parse.</param>
        public void ParseSql(string sql) {
            StringReader reader = new StringReader(ExpandProps(sql));
            string line = null;
            StringBuilder sqlStatement = new StringBuilder();

            while ((line = reader.ReadLine()) != null) {
                if (line.Trim().Length == 0) {
                    continue;
                }

                if (_style == DelimiterStyle.Normal) {
                    if (line.Trim().StartsWith("//") || line.Trim().StartsWith("--")) {
                        continue;
                    }

                    if (!Regex.IsMatch(line.Trim(), _delimiter)) {
                        sqlStatement.Append(line.Trim() + Environment.NewLine);
                    } else {
                        if (line.Trim().Length > 0) {
                            string[] tokens = Regex.Split(line.Trim(),_delimiter);
                            for (int i = 0; i < tokens.Length; i++) {
                                string token = tokens[i];
                                if (i == 0) {
                                    // the first token of a new line is still 
                                    // part of the SQL statement that started 
                                    // on a previous line
                                    if (sqlStatement.Length > 0) {
                                        sqlStatement.Append(token);
                                        _statements.Add(sqlStatement.ToString());
                                        sqlStatement = new StringBuilder();
                                    } else {
                                        sqlStatement = new StringBuilder();
                                        if (token.Trim().Length > 0) {
                                            sqlStatement.Append(token + Environment.NewLine);
                                        }
                                    }
                                } else {
                                    if (sqlStatement.Length > 0) {
                                        _statements.Add(sqlStatement.ToString());
                                        sqlStatement = new StringBuilder();
                                    }

                                    if (token.Trim().Length > 0) {
                                        sqlStatement.Append(token + Environment.NewLine);
                                    }
                                }
                            }
                        }
                    }
                } else { // DelimiterStyle.Line
                    if (line.Trim().ToUpper().Equals(_delimiter.ToUpper())) {
                        if (sqlStatement.ToString().Trim().Length > 0) {
                            _statements.Add(sqlStatement.ToString());
                        }

                        sqlStatement = new StringBuilder();
                        continue;
                    }

                    sqlStatement.Append(line + Environment.NewLine);
                }
            }

            if (sqlStatement.Length > 0) {
                _statements.Add(sqlStatement.ToString());
            }
        }
  
        /// <summary>
        /// Parses the contents of the file into the 
        /// internal list using the specified delimiter
        /// and delimiter style
        /// </summary>
        /// <param name="file">File name</param>
        /// <param name="encoding">The encoding of the file containing the SQL statements.</param>
        public void ParseSqlFromFile(string file, Encoding encoding) {
            using (StreamReader sr = new StreamReader(File.OpenRead(file), encoding, true)) {
                string statements = sr.ReadToEnd();
                ParseSql(statements);
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
        /// Expands project properties in the
        /// sql string
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private string ExpandProps(string sql) {
            if (Properties == null) {
                return sql;
            }
            return Properties.ExpandProperties(sql, null);
        }
    }
}
