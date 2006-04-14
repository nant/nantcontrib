#region GNU Lesser General Public License
//
// NAntContrib
// Copyright (C) 2001-2006 Gerry Shaw
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
// Matt Trentini (matt.trentini@gmail.com)
//
#endregion

using System;
using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.ClearCase {
    /// <summary>
    /// Displays a ClearCase config spec.
    /// </summary>
    [TaskName("cccatcs")]
    public class ClearCaseCatCs : ClearCaseBase {
        private string _viewTag;

        /// <summary>
        /// The view tag identifying the ClearCase view that will have its 
        /// config spec displayed.
        /// </summary>
        [TaskAttribute("viewtag")]
        public virtual string ViewTag {
            get { return _viewTag; }
            set { _viewTag = value; }
        }

        /// <summary>
        /// This is an override used by the base class to get command specific args.
        /// </summary>
        protected override string CommandSpecificArguments {
            get {
                StringBuilder arguments = new StringBuilder();
                arguments.Append("catcs ");

                if (ViewTag != null) {
                    arguments.AppendFormat("-tag {0}", ViewTag);
                }

                return arguments.ToString();
            }
        }
    }
}
