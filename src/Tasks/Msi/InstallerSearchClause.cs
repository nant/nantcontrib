//
// NAntContrib
//
// Copyright (C) 2004 Kraen Munck (kmc@innomate.com)
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using System;

namespace NAnt.Contrib.Tasks.Msi {
    public enum Comparison { Equals };

    /// <summary>
    /// A simple class for a single search clause.
    /// TODO: more comparison types, use of the Composite pattern, etc.
    /// </summary>
    public class InstallerSearchClause {

        string _columnName;
        object _theValue;
        Comparison _searchOperator;

        public string ColumnName {
            get {
                return _columnName;
            }
        }

        public object Value {
            get {
                return _theValue;
            }
        }

        public Comparison SearchOperator {
            get {
                return _searchOperator;
            }
        }


        public InstallerSearchClause(string columnName, Comparison searchOperator, object value) {
            this._columnName = columnName;
            this._searchOperator = searchOperator;
            this._theValue = value;
        }


        public string ToSql() {
            if (_theValue == null) {
                throw new ApplicationException("Can't handle null values in search.");
            }

            return string.Format("`{0}`{1}{2}", _columnName, sqlEncode(_searchOperator), getValueString());
        }


        private string getValueString() {
            string valueString;
            if (_theValue is string) {
                valueString = sqlEncode((string) _theValue);
            } else if (_theValue is int) {
                valueString = sqlEncode((int) _theValue);
            } else {
                throw new ApplicationException("Unhandled type: "+ _theValue.GetType().ToString());
            }
            return valueString;
        }

        static string sqlEncode(Comparison comparisonOperator) {
            switch (comparisonOperator) {
                case Comparison.Equals:
                    return "=";
                default:
                    throw new ApplicationException("Unhandled operator: "+ comparisonOperator);
            }
        }

        static string sqlEncode(string value) {
            return "'"+ value.Replace("'", "''") +"'";
        }

        static string sqlEncode(int value) {
            return value.ToString();
        }
    }
}
