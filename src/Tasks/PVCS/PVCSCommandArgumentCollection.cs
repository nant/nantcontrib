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
using System.Collections;

namespace NAnt.Contrib.Tasks.PVCS {
    /// <summary>
    /// Implements a type-safe collection of <see cref="PVCSCommandArgument"/>s.
    /// </summary>
    public sealed class PVCSCommandArgumentCollection : CollectionBase {
        #region Properties

        /// <summary>
        /// Allows the <see cref="PVCSCommandArgument"/> objects in the collection to be manipulated.
        /// </summary>
        public PVCSCommandArgument this[int index] {
            get {
                return (PVCSCommandArgument) List[index];
            }
            set {
                List[index] = value;
            }
        }

        #endregion

        #region Methods

        /// <see cref="IList.Add(object)"/>
        public void Add(PVCSCommandArgument commandArgument) {
            List.Add(commandArgument);
        }

        /// <summary>
        /// Adds a new command argument to this collection with the specified information.
        /// </summary>
        /// <param name="command">The command string for the new command.</param>
        public void Add(string command) {
            List.Add(new PVCSCommandArgument(command));
        }

        /// <summary>
        /// Adds a new command argument to this collection with the specified information.
        /// </summary>
        /// <param name="command">The command string for the new command.</param>
        /// <param name="commandValue">
        /// The command value for the new command, or <c>null</c> if no value applies.
        /// </param>
        public void Add(string command, object commandValue) {
            List.Add(new PVCSCommandArgument(command, commandValue));
        }

        /// <summary>
        /// Adds a new command argument to this collection with the specified information.
        /// </summary>
        /// <param name="command">The command string for the new command.</param>
        /// <param name="commandValue">
        /// The command value for the new command, or <c>null</c> if no value applies.
        /// </param>
        /// <param name="position">The position for the new command.</param>
        public void Add(string command, object commandValue, PVCSCommandArgumentPosition position) {
            List.Add(new PVCSCommandArgument(command, commandValue, position));
        }

        /// <summary>
        /// Adds all specified command arguments to this collection.
        /// </summary>
        /// <param name="commandArguments">The collection of command arguments to add.</param>
        public void AddRange(ICollection commandArguments) {
            foreach (PVCSCommandArgument commandArgument in commandArguments) {
                Add(commandArgument);
            }
        }

        /// <see cref="IList.IndexOf(object)"/>
        public int IndexOf(PVCSCommandArgument commandArgument) {
            return List.IndexOf(commandArgument);
        }

        /// <see cref="IList.Insert(int, object)"/>
        public void Insert(int index, PVCSCommandArgument commandArgument) {
            List.Insert(index, commandArgument);
        }

        /// <see cref="IList.Remove(object)"/>
        public void Remove(PVCSCommandArgument commandArgument) {
            List.Remove(commandArgument);
        }

        /// <see cref="IList.Contains(object)"/>
        public bool Contains(PVCSCommandArgument commandArgument) {
            return List.Contains(commandArgument);
        }

        /// <summary>
        /// Retrieves an array of <see cref="PVCSCommandArgument"/> objects in this collection.
        /// </summary>
        /// <returns>An array of command arguments in this collection.</returns>
        public PVCSCommandArgument[] ToArray() {
            PVCSCommandArgument[] retVal = new PVCSCommandArgument[List.Count];

            for (int i = 0; i < retVal.Length; ++i) {
                retVal[i] = this[i];
            }

            return retVal;
        }

        /// <see cref="CollectionBase.OnInsert(int, object)"/>
        protected override void OnInsert(int index, object val) {
            if (!(val is PVCSCommandArgument)) {
                throw new ArgumentException("val must be of type PVCSCommandArgument.", "val");
            }
        }

        /// <see cref="CollectionBase.OnSet(int, object, object)"/>
        protected override void OnSet(int index, object oldValue, object newValue)  {
            if (!(newValue is PVCSCommandArgument)) {
                throw new ArgumentException("val must be of type PVCSCommandArgument.", "newValue");
            }
        }

        /// <see cref="CollectionBase.OnValidate(object)"/>
        protected override void OnValidate(object val)  {
            if (!(val is PVCSCommandArgument)) {
                throw new ArgumentException("val must be of type PVCSCommandArgument.", "val");
            }
        }

        #endregion
    }
}
