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

using WindowsInstaller;

namespace NAnt.Contrib.Tasks.Msi {
    /// <summary>
    /// Represents a single table in a Windows Installer archive
    /// </summary>
    public class InstallerTable : IDisposable {
        View _view;
        InstallerDatabase _database;

        public InstallerTable(View view, InstallerDatabase database) {
            _view = view;
            _database = database;
        }

        public void Close() {
            if (_view != null) {
                _view.Close();
            }
            _view = null;
        }

        public void InsertRecord(params object[] fieldList) {
            // TODO: Type and length checks.

            // Create a new record of appropriate length
            Record record = _database.CreateRecord(fieldList.Length);

            // Fill in fields in the record depending on type
            for (int i = 0; i < fieldList.Length; i++) {
                SetRecordField(record, fieldList[i], i);
            }

            // Commit the new record
            _view.Modify(MsiViewModify.msiViewModifyMerge, record);
        }

        internal static void SetRecordField(Record record, object fieldValue, int index) {
            if (fieldValue == null) {
                record.set_StringData(index+1, "");
            } else if (fieldValue is int) {
                record.set_IntegerData(index+1, (int) fieldValue);
            } else if (fieldValue is string) {
                record.set_StringData(index+1, (string) fieldValue);
            } else if (fieldValue is InstallerStream) {
                record.SetStream(index+1, ((InstallerStream) fieldValue).FilePath);
            } else {
                throw new ApplicationException("Unhandled type: " + fieldValue.GetType());
            }
        }

        public void Dispose() {
            Close();
        }
    }

    public struct InstallerStream {
        public string FilePath;

        public InstallerStream(string filePath) {
            this.FilePath = filePath;
        }
    }
}
