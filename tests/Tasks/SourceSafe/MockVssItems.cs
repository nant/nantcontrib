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
    
    public class MockVssItems : NameObjectCollectionBase, IVSSItems {

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

}