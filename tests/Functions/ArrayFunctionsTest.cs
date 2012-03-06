// NAnt - A .NET build tool
// Copyright (C) 2001-2012 Gerry Shaw
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
// Ryan Boggs (rmboggs@users.sourceforge.net)

using System;
using NUnit.Framework;
using NAnt.Core;
using Tests.NAnt.Contrib;

namespace NAnt.Contrib.Tests.Functions
{
    [TestFixture]
    public class ArrayFunctionsTest : BuildTestBase
    {
        #region Public Instance Methods

        [Test]
        public void SortArrayTest()
        {
            string buildFrag = 
                "<project>" +
                "   <property name='array.orig' value='4 7 2 3 5 1 6 9 8' />" +
                "   <property name='array.sort' value=\"${array::sort(array.orig,' ')}\" />" +
                "   <echo message='array.sort=${array.sort}' /> " +
                "</project>";

            string result = RunBuild(buildFrag);

            StringAssert.Contains("array.sort=1 2 3 4 5 6 7 8 9", result);
        }

        [Test]
        public void ReverseArrayTest()
        {
            string buildFrag =
                "<project>" +
                "   <property name='array.orig' value='four,nine,six,two,zero,one' />" +
                "   <property name='array.reverse' value=\"${array::reverse(array.orig,',')}\" />" +
                "   <echo message='array.reverse=${array.reverse}' /> " +
                "</project>";

            string result = RunBuild(buildFrag);

            StringAssert.Contains("array.reverse=one,zero,two,six,nine,four", result);
        }

        #endregion Public Instance Methods
    }
}
