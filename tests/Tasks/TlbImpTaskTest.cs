// NAnt - A .NET build tool
// Copyright (C) 2001-2002 Gerry Shaw
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

// Aaron Anderson (gerry_shaw@yahoo.com)
// Ian MacLean (ian@maclean.ms)

using System;
using System.IO;
using System.Text;
using System.Xml;

using NUnit.Framework;
using Tests.NAnt.Core;

namespace NAnt.Contrib.Tests {
    
    [TestFixture]
    public class TlbImpTaskTest : BuildTestBase {
      
        string _format = @"<?xml version='1.0' ?>
            <project>
                <tlbimp output='{0}'
                    typelib='{1}'
                    namespace='{2}'
                    asmversion='1.0.0.0'
                    primary='false'
                    unsafe='true'
                    sysarray='true'
                    strictref='true'/>
            </project>";


        /// <summary>Test to see if task can run.</summary>
        public void Test_SanityCheck() {
            string fileName = Path.Combine(TempDirName, "interop.wshom.dll");
            string xml = String.Format(_format, fileName, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "wshom.ocx"), "WScript");
            RunBuild(xml);
            Assertion.Assert("Type library '" + fileName + "' was not created.", File.Exists(fileName));
        }
    }
}
