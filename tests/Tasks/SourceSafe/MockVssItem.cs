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

using SourceSafeTypeLib;

using NAnt.Core;

using NAnt.Contrib.Tasks.SourceSafe;

namespace Tests.NAnt.Contrib.Tasks.SourceSafe {
    public class MockVssItem : IVSSItem {
        private IVSSItems _items = new MockVssItems();
        private string _localSpec = "";
        private string _name = "";
        private bool _deleted = false;
        private int _type = (int) VSSItemType.VSSITEM_FILE;
        private bool _destroyed = false;

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
            _destroyed=true;
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

        public bool IsDestroyed {
            get { return _destroyed; }
        }
    }
}