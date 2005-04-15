#region GNU General Public License

// NAntContrib
// Copyright (C) 2004 Kent Boogaart
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
// Kent Boogaart (kentcb@internode.on.net)

#endregion

using System;
using System.Collections.Specialized;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Types {
    /// <summary>
    /// Represents a set of assemblies via their identity information.
    /// </summary>
    public class AssemblySet : DataTypeBase {
        #region Fields

        /// <see cref="AssemblyCollection"/>
        private StringCollection _assemblyCollection;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the collection of assemblies added to this assembly set.
        /// </summary>
        internal StringCollection AssemblyCollection {
            get {
                return _assemblyCollection;
            }
        }

        /// <summary>
        /// The assemblies to include.
        /// </summary>
        [BuildElementArray("assembly", Required = true)]
        public Assembly[] Assemblies {
            set {
                foreach (Assembly assembly in value) {
                    if (assembly.If && !assembly.Unless) {
                        _assemblyCollection.Add(assembly.ToString());
                    }
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructs and initializes an instance of <c>AssemblySet</c>.
        /// </summary>
        public AssemblySet() {
            _assemblyCollection = new StringCollection();
        }

        #endregion
    }
}
