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
// Based on original work by Jayme C. Edwards (jcedwards@users.sourceforge.net)
//

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

using WindowsInstaller;

namespace NAnt.Contrib.Tasks.Msi {
    /// <summary>
    /// .NET wrapper for a Windows Installer database
    /// </summary>
    public class InstallerDatabase {
        string _archivePath;
        Database _database;
        Type _installer;
        Object _installerInstance;

        public string ArchivePath {
            get { return _archivePath; }
        }

        public InstallerDatabase(string path) {
            _archivePath = path;
           _installer = typeof(Installer);
           //
           // 2004-07-11: tomasr@mvps.org
           // Minor change to avoid 
           // "cannot create an instance of an interface" error.
           //
           _installerInstance = new Installer();
/*
            _installer = Type.GetTypeFromProgID("WindowsInstaller.Installer");
            _installerInstance = Activator.CreateInstance(_installer);
*/            
        }

        public void Open() {
            _database = (Database) _installer.InvokeMember(
                "OpenDatabase",
                BindingFlags.InvokeMethod,
                null, _installerInstance,
                new Object[] {
                                 _archivePath,
                                 MsiOpenDatabaseMode.msiOpenDatabaseModeDirect
                             });

        }

        public void Commit() {
            _database.Commit();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void Close() {
            _database.Commit();
            _database = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public InstallerTable OpenTable(string tableName) {
            try {
                return new InstallerTable(OpenView("SELECT * FROM `"+ tableName +"`"), this);
            } catch (System.Runtime.InteropServices.COMException ex) {
                if (ex.Message == "OpenView,Sql") { // && (ex.ErrorCode == 0x80004005)
                    throw new ArgumentException("Table not found: "+ tableName);
                }
                throw;
            }
        }

        public InstallerRecordReader FindRecords(string tableName, params InstallerSearchClause[] clauses) {
            View view = OpenView(getSelectStatement(tableName, clauses));
            view.Execute(null);
            return new InstallerRecordReader(view);
        }

        private string getSelectStatement(string tableName, InstallerSearchClause[] clauses) {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.AppendFormat("SELECT * FROM `{0}`", tableName);

            for (int i = 0; i < clauses.Length; i++) {
                if (i == 0) {
                    builder.Append(" WHERE ");
                } else {
                    builder.Append(" AND ");
                }
                builder.Append(clauses[i].ToSql());
            }

            return builder.ToString();
        }


        /*
         * Proxy methods for the WindowsInstaller objects
         */

        private View OpenView(string sql) {
            return _database.OpenView(sql);
        }

        private void ExecuteNonQuery(string sql) {
            View tempView = OpenView(sql);
            tempView.Execute(null);
            tempView.Close();
        }

        internal Record CreateRecord(int fields) {
            return (Record) _installer.InvokeMember(
                "CreateRecord",
                BindingFlags.InvokeMethod,
                null, _installerInstance,
                new object[] { fields });
        }

        private void ApplyTransform(string transformPath, MsiTransformError errorConditions) {
            _database.ApplyTransform(transformPath, errorConditions);
        }

        public void ApplyTransform(string transformPath) {
            ApplyTransform(transformPath, MsiTransformError.msiTransformErrorNone);
        }

        public SummaryInfo GetSummaryInformation() {
            return _database.get_SummaryInformation(200);
        }


        /*
         * Utility methods
         */

        public void Import(string tableStructureContents) {
            string tempFilePath = Path.Combine(Path.GetTempPath(), 
                Path.GetTempFileName());

            try {
                using (FileStream tableStream = File.Create(tempFilePath)) {
                    StreamWriter writer = new StreamWriter(tableStream);
                    writer.Write(tableStructureContents);
                    writer.Flush();
                    writer.Close();
                    tableStream.Close();
                }

                _database.Import(Path.GetDirectoryName(tempFilePath), 
                    Path.GetFileName(tempFilePath));
            } finally {
                File.Delete(tempFilePath);
            }
        }

        /// <summary>
        /// Drops empty tables.
        /// </summary>
        public void DropEmptyTables() {
            DropEmptyTables(false);
        }

        /// <summary>
        /// Drops the empty tables.
        /// </summary>
        /// <param name="isMergeModule">Determines if this is a merge module or not</param>
        /// <remarks>If it is a merge module, the FeatureComponents table should not be dropped.</remarks>
        public void DropEmptyTables(bool isMergeModule) {
            // Go through each table listed in _Tables
            using (InstallerRecordReader reader = FindRecords("_Tables")) {
                while (reader.Read()) {
                    string tableName = reader.GetString(0);

                    if (isMergeModule && tableName == "FeatureComponents")
                        continue;

                    if (VerifyTableEmpty(tableName)) {
                        // Drop the table
                        ExecuteNonQuery("DROP TABLE `" + tableName + "`");

                        // Delete entries in _Validation table
                        ExecuteNonQuery("DELETE FROM `_Validation` WHERE `Table` = '" + tableName + "'");
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see if the specified table is empty.
        /// </summary>
        /// <param name="TableName">Name of the table to check existance.</param>
        /// <returns>True if empy and False if full.</returns>
        public bool VerifyTableEmpty(string TableName) {
            using (InstallerRecordReader reader = FindRecords(TableName)) {
                return !reader.Read();
            }
        }

        /// <summary>
        /// Checks to see if the specified table exists in the database
        /// already.
        /// </summary>
        /// <param name="TableName">Name of the table to check existance.</param>
        /// <returns>True if successful.</returns>
        public bool VerifyTableExistance(string TableName) {
            using (InstallerRecordReader reader = FindRecords("_Tables", 
                       new InstallerSearchClause("Name", Comparison.Equals, TableName))) {
                return reader.Read();
            }
        }
    }



   /// <remarks>
   /// Helper class used to avoid errors when instantiating
   /// WindowsInstaller.Installer. 
   /// </remarks>
   [ 
      ComImport(),
      Guid("000C1090-0000-0000-C000-000000000046")
   ]
   class Installer {}

}
