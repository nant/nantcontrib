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
    /// Use to read and manipulate existing records.
    /// </summary>
    public class InstallerRecordReader : IDisposable {
        View _view;
        Record currentRecord;

        /// <summary>
        /// Creates a new reader for the entries in the view
        /// </summary>
        /// <param name="view">Database view to read entries from. Must be Execute()'ed already.</param>
        internal InstallerRecordReader(View view) {
            _view = view;
        }

        public void Close() {
            _view.Close();
            _view = null;
            currentRecord = null;
        }

        public bool IsClosed {
            get {
                return (_view == null);
            }
        }

        public void Dispose() {
            Close();
        }

        /// <summary>
        /// Moves to the next record
        /// </summary>
        /// <returns>False iff no more records</returns>
        public bool Read() {
            currentRecord = _view.Fetch();
            return (currentRecord != null);
        }

        /// <summary>
        /// Deletes the current record. Needs no Commit().
        /// </summary>
        public void DeleteCurrentRecord() {
            assertCurrentRecord();
            _view.Modify(MsiViewModify.msiViewModifyDelete, currentRecord);
            currentRecord = null;
        }

        private void assertCurrentRecord() {
            if (currentRecord == null) {
                throw new ApplicationException("Current record must exist.");
            }
        }

        /// <summary>
        /// Set the value of a field in the current record. Remember to Commit()
        /// </summary>
        /// <param name="index">Zero-based index of the field to set</param>
        /// <param name="value">New value</param>
        public void SetValue(int index, object value) {
            assertCurrentRecord();
            InstallerTable.SetRecordField(currentRecord, value, index);
        }

        /// <summary>
        /// Get the string value of a field in the current record.
        /// </summary>
        /// <param name="index">Zero-based index of the field to get</param>
        public string GetString(int index) {
            assertCurrentRecord();
            return currentRecord.get_StringData(index+1);
        }

        /// <summary>
        /// Commits changes to the current record.
        /// </summary>
        public void Commit() {
            assertCurrentRecord();
            _view.Modify(MsiViewModify.msiViewModifyUpdate, currentRecord);
        }
    }
}
