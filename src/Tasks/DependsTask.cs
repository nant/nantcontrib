//
// NAntContrib
// Copyright (C) 2004 James C. Papp
//
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
// James C. Papp <jpapp-at-ellorasoftDOTcom>
// Scott Hernandez(ScottHernandezAThotmail....com)



using System.Collections.Specialized;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// On execution guarantees the listed dependencies are resolved before continuing. It is 
    /// particularly useful for handling dynamic dependencies that change based on some input 
    /// conditions/parameters, or when the dependencies are not known until runtime.
    /// </summary>
    /// <remarks>The depends task never forces the execution of any target that has already been executed. It works just like the depends attribute of a <see cref="Target"/>.</remarks>
    [TaskName("depends")]
    public class DependsTask : Task {
        #region Private Instance Fields

        private StringCollection _dependencies = new StringCollection();

        #endregion Private Instance Fields

        #region Public Instance Properties
        /// <summary>
        /// A space or comma separated dependency list of targets.  
        /// </summary>
        /// <remarks>
        /// Expressions get evaluated when the task is executed.  
        /// </remarks>
        [TaskAttribute("on", Required=true, ExpandProperties=true)]
        public string DependsCollection {
            set {
                foreach (string str in value.Split(new char[] {' ', ','})) {
                    string dependency = str.Trim();
                    if (dependency.Length > 0) {
                        _dependencies.Add(dependency);
                    }
                }
            }
        }

        #endregion Public Instance Properties
        
        #region Override implementation of Task

        /// <summary>
        /// Executes the specified task.
        /// </summary>
        protected override void ExecuteTask() {
          Target owningTarget = Parent as Target;
          StringCollection dependencies = _dependencies;

            foreach(string target in dependencies) {
                
                // check to see if one of the targets we are going execute includes the current target.
                // if so, throw
                if (owningTarget != null) {
                    if (owningTarget.Name == target) {
                        throw new BuildException("Depends task cannot depend on its own parent.", Location);
                    }

                    // topologically sorted list of targets that will be executed
                    TargetCollection targets = Project.TopologicalTargetSort(target, Project.Targets);

                    // check if owning target is part of list of targets that will be executed again
                    if (targets.Find(owningTarget.Name) != null) {
                        // check if owning target is actually a dependency of the target that should be executed
                        if (targets.IndexOf(targets.Find(owningTarget.Name)) < targets.IndexOf(targets.Find(target))) {
                            throw new BuildException("Circular dependency: " + targets.ToString(" <- ") + " <- " + owningTarget.Name);
                        }
                    }
                }

                Project.Execute(target, false);  
            }
        }

        #endregion Override implementation of Task
    }
}