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
using System.Diagnostics;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.ClearCase {
    /// <summary>
    /// Base class for all the ClearCase tasks.
    /// </summary>
    public abstract class ClearCaseBase : NAnt.Core.Tasks.ExternalProgramBase {
        private const string _defaultExeFilename = "cleartool.exe";
        private const int _defaultTimeOut = 180000; // 3 minutes
        private string _arguments;

        /// <summary>
        /// Base Constructor.
        /// </summary>
        public ClearCaseBase() {
            ExeName = _defaultExeFilename;
            TimeOut = _defaultTimeOut;
        }

        /// <summary>
        /// Derived classes should override this to provide command-specific
        /// commandline arguments.
        /// </summary>
        protected abstract string CommandSpecificArguments {
            get;
        }   

        /// <summary>
        /// Overrides the base class.
        /// </summary>
        public override string ProgramArguments {
            get { return _arguments; }
        }

        /// <summary>
        /// Execute the perforce command assembled by subclasses.
        /// </summary>
        protected override void ExecuteTask() {
            _arguments = CommandSpecificArguments;

            // call base class to do perform the actual call
            Log(Level.Verbose, ExeName + _arguments);

            base.ExecuteTask();
        } 
    }
}
