// NAntContrib
// Copyright (C) 2001-2002 Gerry Shaw
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
// Gerry Shaw (gerry_shaw@yahoo.com)

using System;
using System.IO;
using System.Reflection;
using System.Xml;

using SourceForge.NAnt;
using SourceForge.NAnt.Attributes;

namespace NAnt.Contrib.Tasks {

    /// <summary>Loads tasks from a specified assembly.</summary>
    /// <remarks>
    ///   <para>NAnt by default will scan any assemblies ending in *Task.dll in the same directory as the NAnt.  You can use this task to include assemblies in different locations.</para>
    /// </remarks>
    /// <example>
    ///   <para>Include the tasks in an assembly.</para>
    ///   <code><![CDATA[<taskdef assembly='ProjectSpecific.dll'/>]]></code>
    /// </example>
    [TaskName("taskdef")]
    public class TaskDefTask : Task {

        string _assemblyFileName = null;

        /// <summary>File name of the assembly containing the NAnt task.</summary>
        [TaskAttribute("assembly")]
        public string AssemblyFileName {
            get { return _assemblyFileName; }
            set { _assemblyFileName = value; }
        }

        protected override void ExecuteTask() {
            string assemblyFileName = Project.GetFullPath(AssemblyFileName);
            try {
                int taskCount = TaskFactory.AddTasks(Assembly.LoadFrom(assemblyFileName));
                Log.WriteLine(LogPrefix + "Added {0} tasks from {1}", taskCount, assemblyFileName);
            } catch (Exception e) {
                throw new BuildException("Could not add tasks from " + assemblyFileName, Location, e);
            }
        }
    }
}
