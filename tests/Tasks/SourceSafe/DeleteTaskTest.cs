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
// Richard Adleta (richardadleta@yahoo.com)

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
    
    /// <summary>
    /// VSS Delete Task tests
    /// </summary>
    [TestFixture]
    public class DeleteTaskTests {
        string _workingFolder = "";
        
        MockVssItem _project;

        DeleteTask _deleteTask;

        [SetUp]
        public void Setup() {
            //create base folder for "project"'s local copy
            _workingFolder = Path.Combine(Path.GetTempPath(), "FROM_VSS_DELETE");
            Directory.CreateDirectory(_workingFolder);

            //This item represents the base project item
            _project = new MockVssItem();
            _project.Name = "TEMPVSSPROJECT";
            _project.SetType((int) VSSItemType.VSSITEM_PROJECT);
            string localProjectPath = _workingFolder;
            Directory.CreateDirectory(localProjectPath);

            //create the instance of the DeleteTask that we will use in the tests
            _deleteTask = new DeleteTask();
            _deleteTask.Item = _project;
            _deleteTask.Path = "$/" + _project.Name;

            //make the DeleteTask happy by giving it a dummy project file to reference
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<project name='test'/>");
            _deleteTask.Project = new Project(doc, Level.Info, 0);
        }

        [TearDown]
        public void Teardown() {
            Directory.Delete(_workingFolder, true);
        }

        /// <summary>
        /// Ensures that the delete operation occurs.
        /// </summary>
        [Test]
        public void DeleteItem() {
            _deleteTask.Destroy = false;
            _deleteTask.DeleteItem();

            Assert.IsTrue(_project.Deleted, "Delete did not occur");
            Assert.IsFalse(_project.IsDestroyed, "Item should not have been Destroyed");
        }

        /// <summary>
        /// Ensures that the destroy operation occurs.
        /// </summary>
        [Test]
        public void DestroyItem() {
            _deleteTask.Destroy = true;
            _deleteTask.DeleteItem();

            Assert.IsTrue(_project.IsDestroyed, "Destroy did not occur");
            Assert.IsFalse(_project.Deleted, "Item should not have been Deleted");
        }
   }
}