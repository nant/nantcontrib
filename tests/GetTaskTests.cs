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
using System.Xml;

using NUnit.Framework;

using NAnt.Core;
using NAnt.Contrib.Tasks.SourceSafe;
using SourceSafeTypeLib;

namespace NAnt.Contrib.Tests.Tasks.VSS {
    public class MockVSSItem : IVSSItem {
        private IVSSItems _items = new MockVSSItems();
        private string _localSpec = "";
        private string _name = "";
        private bool _deleted = false;
        private int _type = (int) VSSItemType.VSSITEM_FILE;

        public void SetItems(IVSSItems items) {
            _items = items;
        }

        public void SetType(int type) {
            _type = type;
        }

        #region Implemented IVSSItem Members

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public string LocalSpec {
            get { return _localSpec; }
            set { _localSpec = value; }
        }

        public bool Deleted {
            get { return _deleted; }
            set { _deleted = value; }
        }

        public int Type {
            get { return _type; }
        }

        #endregion

        #region IVSSItem Members

        public string Spec {
            get { return null; }
        }

        public IVSSVersions get_Versions(int iFlags) {
            return null;
        }

        public bool get_IsDifferent(string Local) {
            return false;
        }

        public bool Binary {
            get { return false; }
            set {}
        }

        public VSSItem Branch(string Comment, int iFlags) {
            return null;
        }

        public void Destroy() {
        }

        public IVSSItems Links {
            get { return null; }
        }

        public IVSSItems get_Items(bool IncludeDeleted){
            return _items;
        }

        public int IsCheckedOut {
            get { return 0; }
        }

        public void UndoCheckout(string Local, int iFlags) {
        }

        public int VersionNumber {
            get { return 0; }
        }

        public VSSItem get_Version(object Version) {
            return null;
        }

        public void Get(ref string Local, int iFlags) {
        }

        public void Share(VSSItem pIItem, string Comment, int iFlags) {
        }

        public VSSItem NewSubproject(string Name, string Comment) {
            return null;
        }

        public VSSItem Add(string Local, string Comment, int iFlags) {
            return null;
        }

        public void Checkin(string Comment, string Local, int iFlags) {
        }

        public void Label(string Label, string Comment) {
        }

        public VSSItem Parent {
            get { return null; }
        }

        public void Move(VSSItem pINewParent) {
        }

        public void Checkout(string Comment, string Local, int iFlags) {
        }

        public IVSSCheckouts Checkouts {
            get { return null; }
        }

        #endregion
    }

    public class MockVSSItems : NameObjectCollectionBase, IVSSItems {

        public void Add(IVSSItem i) {
            BaseAdd(i.Name, i);
        }

        #region IVSSItems Members

        public new IEnumerator GetEnumerator() {
            return BaseGetAllValues().GetEnumerator();
        }

        public VSSItem this[object sItem] {
            get { return null; }
        }

        //Count remove because its implemented on ObjectCollectionBase (and not used)

        #endregion
    }

    /// <summary>
   /// VSS Get Task tests
   /// </summary>
    [TestFixture]
    public class GetTaskTests {
        string _workingFolder = "";
        string _localItemPath = "";
        string _localNormalItemPath = "";
        string _localReadOnlyItemPath = "";
        string _localReadOnlySubProjectPath = "";
        string _localReadOnlySubItemPath = "";
        string _localSubProjectPath = "";
        string _localSubItemPath = "";

        MockVSSItem _project;
        MockVSSItem _item;
        MockVSSItem _alreadyGoneItem;
        MockVSSItem _readOnlyItem;
        MockVSSItem _readOnlySubProject;
        MockVSSItem _readOnlySubItem;
        MockVSSItem _subProject;
        MockVSSItem _subItem;
        MockVSSItem _normalItem;
        MockVSSItem _normalItemDeleted;

        GetTask _getTask;

        [SetUp]
        public void Setup() {
            //create base folder for "project"'s local copy
            _workingFolder = Path.Combine(Path.GetTempPath(), "FROM_VSS");
            Directory.CreateDirectory(_workingFolder);

            //This item represents the base project item
            _project = new MockVSSItem();
            _project.Name = "TEMPVSSPROJECT";
            _project.SetType((int) VSSItemType.VSSITEM_PROJECT);
            string localProjectPath = _workingFolder;
            Directory.CreateDirectory(localProjectPath);

            //a normal file item whose local copy is in the project's local folder;
            _normalItem = new MockVSSItem();
            _normalItem.Name = "normalItem.txt";
            _normalItem.SetType((int) VSSItemType.VSSITEM_FILE);
            _normalItem.Deleted = false;
            _localNormalItemPath = Path.Combine(localProjectPath, _normalItem.Name);
            StreamWriter s = File.CreateText(_localNormalItemPath);
            s.Close();

            //a deleted file item that represents an earlier, deleted version of the normal file item
            _normalItemDeleted = new MockVSSItem();
            _normalItemDeleted.Name = "normalItem.txt";
            _normalItemDeleted.SetType((int) VSSItemType.VSSITEM_FILE);
            _normalItemDeleted.Deleted = true;

            //a deleted file item whose local copy is located under the base project's local folder
            _item = new MockVSSItem();
            _item.Name = "tempVSSItem.txt";
            _item.SetType((int) VSSItemType.VSSITEM_FILE);
            _item.Deleted = true;
            _localItemPath = Path.Combine(localProjectPath, _item.Name);
            s = File.CreateText(_localItemPath);
            s.Close();

            //a deleted file item with *no* local copy
            _alreadyGoneItem = new MockVSSItem();
            _alreadyGoneItem.Name = "notthere.txt";
            _alreadyGoneItem.SetType((int) VSSItemType.VSSITEM_FILE);
            _alreadyGoneItem.Deleted = true;

            //a deleted file item whose local copy is located under the base project's local folder and read-only
            _readOnlyItem = new MockVSSItem();
            _readOnlyItem.Name = "readOnlyTempVSSItem.txt";
            _readOnlyItem.SetType((int) VSSItemType.VSSITEM_FILE);
            _readOnlyItem.Deleted = true;
            _localReadOnlyItemPath = Path.Combine(localProjectPath, _readOnlyItem.Name);
            s = File.CreateText(_localReadOnlyItemPath);
            s.Close();
            File.SetAttributes(_localReadOnlyItemPath, FileAttributes.ReadOnly);

            //a deleted project item whose local copy is located under the base project's local folder and read-only
            _readOnlySubProject = new MockVSSItem();
            _readOnlySubProject.Name = "READONLYSUBPROJECT";
            _readOnlySubProject.SetType((int) VSSItemType.VSSITEM_PROJECT);
            _readOnlySubProject.Deleted = true;
            _localReadOnlySubProjectPath = Path.Combine(localProjectPath, _readOnlySubProject.Name);
            Directory.CreateDirectory(_localReadOnlySubProjectPath);
            File.SetAttributes(_localReadOnlySubProjectPath, FileAttributes.ReadOnly);

            //a deleted file item whose local copy is located under the read-only sub project's local folder 
            //is itself read-only
            _readOnlySubItem = new MockVSSItem();
            _readOnlySubItem.Name = "readOnlySubItem.txt";
            _readOnlySubItem.SetType((int) VSSItemType.VSSITEM_FILE);
            _readOnlySubItem.Deleted = true;
            _localReadOnlySubItemPath = Path.Combine(_localReadOnlySubProjectPath, _readOnlySubItem.Name);
            s = File.CreateText(_localReadOnlySubItemPath);
            s.Close();
            File.SetAttributes(_localReadOnlySubItemPath, FileAttributes.ReadOnly);

            //a deleted project item whose local copy is in the base project's local folder
            _subProject = new MockVSSItem();
            _subProject.Name = "SUBPROJECT";
            _subProject.SetType((int) VSSItemType.VSSITEM_PROJECT);
            _subProject.Deleted = true;
            _localSubProjectPath = Path.Combine(localProjectPath, _subProject.Name);
            Directory.CreateDirectory(_localSubProjectPath);

            //a deleted file item whose local copy is in the sub-project's local folder
            _subItem = new MockVSSItem();
            _subItem.Name = "tempVSSSubItem.txt";
            _subItem.SetType((int) VSSItemType.VSSITEM_FILE);
            _subItem.Deleted = true;
            _localSubItemPath = Path.Combine(_localSubProjectPath, _subItem.Name);
            s = File.CreateText(_localSubItemPath);
            s.Close();

            //add the sub file item to the sub project's items collection
            MockVSSItems items = new MockVSSItems();
            items.Add(_subItem);
            _subProject.SetItems(items);

            //add the read-only sub file item to read-only sub-project's items collection
            items = new MockVSSItems();
            items.Add(_readOnlySubItem);
            _readOnlySubProject.SetItems(items);

            //create the instance of the GetTask that we will use in the tests
            _getTask = new GetTask();
            _getTask.Item = _project;
            _getTask.LocalPath = _workingFolder;
            _getTask.RemoveDeleted = Boolean.TrueString;

            //make the GetTask happy by giving it a dummy project file to reference
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<project name='test'/>");
            _getTask.Project = new Project(doc, Level.Info);
        }

        [TearDown]
        public void Teardown() {
            if(File.Exists(_localReadOnlyItemPath)) {
                File.SetAttributes(_localReadOnlyItemPath, FileAttributes.Normal);
            }

            if(Directory.Exists(_localReadOnlySubProjectPath)) {
                File.SetAttributes(_localReadOnlySubProjectPath, FileAttributes.Normal);
            }

            if(File.Exists(_localReadOnlySubItemPath)) {
                File.SetAttributes(_localReadOnlySubItemPath, FileAttributes.Normal);
            }

            Directory.Delete(_workingFolder, true);
        }

        /// <summary>
        /// Ensures items that are not marked deleted are left alone
        /// </summary>
        [Test]
        public void DeletionOfLocalFilesSkippedIfItemNotDeleted() {
            MockVSSItems items = new MockVSSItems();
            items.Add(_normalItem);
            _project.SetItems(items);

            _getTask.RemoveDeletedFromLocalImage();

            Assert.IsTrue(File.Exists(_localNormalItemPath), "Normal file should still exist");
        }

        /// <summary>
        /// This test ensures that items deleted and restored (i.e. an normal item with the same name as 
        /// a deleted item exists in the repository) are not deleted from the local image
        /// </summary>
        [Test]
        public void LeaveRestoredItemsInPlace() {
            MockVSSItems items = new MockVSSItems();
            items.Add(_normalItem);
            items.Add(_normalItemDeleted);
            _project.SetItems(items);

            _getTask.RemoveDeletedFromLocalImage();

            Assert.IsTrue(File.Exists(_localNormalItemPath), "Normal file should still exist");
        }

        /// <summary>
        /// Ensures that file items marked deleted in the project directory are removed
        /// </summary>
        [Test]
        public void DeletionOfLocalFiles() {
            MockVSSItems items = new MockVSSItems();
            items.Add(_item);
            _project.SetItems(items);

            _item.Deleted = false;
            _getTask.RemoveDeletedFromLocalImage();
            Assert.IsTrue(File.Exists(_localItemPath), "deleted file should be there");

            _item.Deleted = true;
            _getTask.RemoveDeletedFromLocalImage();
            Assert.IsFalse(File.Exists(_localItemPath), "deleted file should be gone");

        }

        /// <summary>
        /// Ensures that project items marked deleted in the project directory are removed
        /// </summary>
        [Test]
        public void DeletionOfLocalProjects() {
            MockVSSItems items = new MockVSSItems();
            items.Add(_subProject);
            _project.SetItems(items);

            _subProject.Deleted = false;
            _getTask.RemoveDeletedFromLocalImage();
            Assert.IsTrue(Directory.Exists(_localSubProjectPath), "deleted folder should be there");

            _subProject.Deleted = true;
            _getTask.RemoveDeletedFromLocalImage();
            Assert.IsFalse(Directory.Exists(_localSubProjectPath), "deleted folder should be gone");
        }

        [Test]
        public void RecursiveIsFalseSubProjectsIgnored() {
            MockVSSItems items = new MockVSSItems();
            items.Add(_item);
            items.Add(_subProject);
            _project.SetItems(items);

            _subProject.Deleted = false;
            _getTask.Recursive = Boolean.FalseString;

            Assert.IsTrue(File.Exists(_localItemPath), "file should exist");
            Assert.IsTrue(File.Exists(_localSubItemPath), "file should exist");

            _getTask.RemoveDeletedFromLocalImage();

            Assert.IsFalse(File.Exists(_localItemPath), "this file should be gone");
            Assert.IsTrue(File.Exists(_localSubItemPath), "this file should still be there");
        }

        [Test]
        public void RecursiveIsTrueSubProjectsIncluded() {
            MockVSSItems items = new MockVSSItems();
            items.Add(_item);
            items.Add(_subProject);
            _project.SetItems(items);

            _subProject.Deleted = false;
            _getTask.Recursive = Boolean.TrueString;

            Assert.IsTrue(File.Exists(_localItemPath), "file should exist");
            Assert.IsTrue(File.Exists(_localSubItemPath), "file should also exist");

            _getTask.RemoveDeletedFromLocalImage();

            Assert.IsFalse(File.Exists(_localItemPath), "this file should be gone");
            Assert.IsFalse(File.Exists(_localSubItemPath), "this file should also be gone");
        }

        [Test]
        public void HandleReadOnlyFiles() {
            MockVSSItems items = new MockVSSItems();
            items.Add(_readOnlyItem);
            _project.SetItems(items);

            Assert.IsTrue(File.Exists(_localReadOnlyItemPath), "file should exist");

            _getTask.RemoveDeletedFromLocalImage();

            Assert.IsFalse(File.Exists(_localReadOnlyItemPath), "this file should be gone");
        }

        [Test]
        public void HandleReadOnlyFolders() {
            MockVSSItems items = new MockVSSItems();
            items.Add(_readOnlySubProject);
            _project.SetItems(items);

            Assert.IsTrue(Directory.Exists(_localReadOnlySubProjectPath), "folder should exist");

            _getTask.RemoveDeletedFromLocalImage();

            Assert.IsFalse(Directory.Exists(_localReadOnlySubProjectPath), "this file should be gone");
        }

        /// <summary>
        /// Ensures that already removed items do not throw an exception
        /// </summary>
        [Test]
        public void IgnoreAlreadyRemovedItems() {
            MockVSSItems items = new MockVSSItems();
            items.Add(_alreadyGoneItem);
            _project.SetItems(items);

            _getTask.RemoveDeletedFromLocalImage();
        }

        /// Verifies that items deleted are marked deleted
        /// </summary>
        [Test]
        public void BuildDeletedTableDoesIncludeDeletedItems() {
            MockVSSItem deleted = new MockVSSItem();
            deleted.Name = "deleted";
            deleted.Deleted = true;

            MockVSSItems items = new MockVSSItems();
            items.Add(deleted);

            Hashtable table = _getTask.BuildDeletedTable(items);

            Assert.IsTrue((bool) table[deleted.Name], "deleted items should be deleted");
        }

        /// <summary>
        /// Verifies that items never deleted are not marked deleted
        /// </summary>
        [Test]
        public void BuildDeletedTableDoesNotIncludeRegularItems() {
            MockVSSItem regular = new MockVSSItem();
            regular.Name = "regular";
            regular.Deleted = false;

            MockVSSItems items = new MockVSSItems();
            items.Add(regular);

            Hashtable table = _getTask.BuildDeletedTable(items);

            Assert.IsFalse((bool) table[regular.Name], "regular items should not be deleted");
        }

        /// <summary>
        /// Verifies that items deleted and restored are not marked deleted
        /// </summary>
        [Test]
        public void BuildDeletedTableDoesNotIncludeRestoredItems() {
            MockVSSItem restored = new MockVSSItem();
            restored.Name = "restored";
            restored.Deleted = false;

            MockVSSItem restored2 = new MockVSSItem();
            restored2.Name = "restored";
            restored2.Deleted = true;

            MockVSSItems items = new MockVSSItems();
            items.Add(restored);
            items.Add(restored2); //order doesn't matter

            Hashtable table = _getTask.BuildDeletedTable(items);

            Assert.AreEqual(1, table.Count, "items have the same name, should only be one record");
            Assert.IsFalse((bool) table[restored.Name], "restored items should not be deleted");
        }

        /// <summary>
        /// Verifies that items deleted and restored an odd number of multiple times are not marked deleted
        /// </summary>
        [Test]
        public void BuildDeletedTableDoesNotIncludeMultiplyRestoredItemsOdd() {
            MockVSSItem restored = new MockVSSItem();
            restored.Name = "restored";
            restored.Deleted = false;

            MockVSSItem restored2 = new MockVSSItem();
            restored2.Name = "restored";
            restored2.Deleted = true;

            MockVSSItem restored3 = new MockVSSItem();
            restored3.Name = "restored";
            restored3.Deleted = true;

            MockVSSItem restored4 = new MockVSSItem();
            restored4.Name = "restored";
            restored4.Deleted = true;

            MockVSSItems items = new MockVSSItems();
            items.Add(restored); //order doesn't matter
            items.Add(restored2); 
            items.Add(restored3); 
            items.Add(restored4); 

            Hashtable table = _getTask.BuildDeletedTable(items);
            
            Assert.AreEqual(1, table.Count, "items have the same name, should only be one record");
            Assert.IsFalse((bool) table[restored.Name], "restored items should not be deleted");
        }

        /// <summary>
        /// Verifies that items deleted and restored an even number of multiple times are not marked deleted
        /// </summary>
        [Test]
        public void BuildDeletedTableDoesNotIncludeMultiplyRestoredItemsEven() {
            MockVSSItem restored = new MockVSSItem();
            restored.Name = "restored";
            restored.Deleted = false;

            MockVSSItem restored2 = new MockVSSItem();
            restored2.Name = "restored";
            restored2.Deleted = true;

            MockVSSItem restored3 = new MockVSSItem();
            restored3.Name = "restored";
            restored3.Deleted = true;

            MockVSSItem restored4 = new MockVSSItem();
            restored4.Name = "restored";
            restored4.Deleted = true;

            MockVSSItem restored5 = new MockVSSItem();
            restored5.Name = "restored";
            restored5.Deleted = true;

            MockVSSItems items = new MockVSSItems();
            items.Add(restored); //order doesn't matter
            items.Add(restored2); 
            items.Add(restored3); 
            items.Add(restored4); 
            items.Add(restored5); 

            Hashtable table = _getTask.BuildDeletedTable(items);

            Assert.AreEqual(1, table.Count, "items have the same name, should only be one record");
            Assert.IsFalse((bool) table[restored.Name], "restored items should not be deleted");
        }

        /// <summary>
        /// Verifies that items deleted and restored are not marked deleted even if the case of the name
        /// is different.
        /// </summary>
        [Test]
        public void BuildDeletedTableDoesNotIncludeRestoredItemsWithDifferentCaseNames() {
            MockVSSItem restored = new MockVSSItem();
            restored.Name = "restored";
            restored.Deleted = false;

            MockVSSItem restored2 = new MockVSSItem();
            restored2.Name = "Restored";
            restored2.Deleted = true;

            MockVSSItems items = new MockVSSItems();
            items.Add(restored);
            items.Add(restored2); //order doesn't matter

            Hashtable table = _getTask.BuildDeletedTable(items);

            Assert.AreEqual(1, table.Count, "items having names with case differences only are the same");
            Assert.IsFalse((bool) table[restored.Name], "restored items should not be deleted");
            Assert.IsFalse((bool) table[restored2.Name], "restored items should not be deleted");
        }

   } // class GetTaskTests
} // namespace NAnt.Contrib.Tests.Tasks.VSS