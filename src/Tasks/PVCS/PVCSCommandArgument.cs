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
using System.Text;

namespace NAnt.Contrib.Tasks.PVCS {
    /// <summary>
    /// Encapsulates the details of a PVCS command argument.
    /// </summary>
    /// <remarks>
    /// <para>
    /// PVCS tasks must "fill in" a collection of arguments to be passed to the PVCS command line interface (PCLI).
    /// This class represents one such argument.
    /// </para>
    /// <para>
    /// Each argument consists of a command and an optional command value. The command is always passed to the PVCS
    /// command line utility and is therefore required. An example of a command is "-g" which is passed to many
    /// PVCS command line utilities to specify a promotion group.
    /// </para>
    /// <para>
    /// The command value is used to specify extra information to the command. For example, if the command is "-g"
    /// then the command value would be the name of the promotion group.
    /// </para>
    /// <para>
    /// The command can be assigned a position (see the <see cref="Position"/> property). This position defines
    /// where the command appears relative to other commands. For example, some commands must appear after other
    /// commands. Therefore, they should be assigned a position of <see cref="PVCSCommandArgumentPosition.Start"/>.
    /// </para>
    /// </remarks>
    public sealed class PVCSCommandArgument : IComparable {
        #region Fields

        /// <see cref="Command"/>
        private string _command;

        /// <see cref="CommandValue"/>
        private object _commandValue;

        /// <see cref="Position"/>
        private PVCSCommandArgumentPosition _position;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a <c>string</c> that contains the command to pass to PVCS.
        /// </summary>
        public string Command {
            get {
                return _command;
            }
        }

        /// <summary>
        /// Gets the value to append to <see cref="Command"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If this property is <c>null</c>, no value will be appended to the command.
        /// </para>
        /// </remarks>
        public object CommandValue {
            get {
                return _commandValue;
            }
        }

        /// <summary>
        /// Gets the position for the command.
        /// </summary>
        public PVCSCommandArgumentPosition Position {
            get {
                return _position;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructs an instance of <c>PVCSCommandArgument</c> with the specified information. No value is
        /// applied and the argument has a position of <see cref="PVCSCommandArgumentPosition.Middle"/>.
        /// </summary>
        /// <param name="command">The command string.</param>
        public PVCSCommandArgument(string command) : this(command, null, PVCSCommandArgumentPosition.Middle) {
        }

        /// <summary>
        /// Constructs an instance of <c>PVCSCommandArgument</c> with the specified information. The argument has
        /// a position of <see cref="PVCSCommandArgumentPosition.Middle"/>.
        /// </summary>
        /// <param name="command">The command string.</param>
        /// <param name="commandValue">The value for the command, or <c>null</c> if no value applies.</param>
        public PVCSCommandArgument(string command, object commandValue) : this(command, commandValue, PVCSCommandArgumentPosition.Middle) {
        }

        /// <summary>
        /// Constructs an instance of <c>PVCSCommandArgument</c> with the specified information.
        /// </summary>
        /// <param name="command">The command string.</param>
        /// <param name="commandValue">The value for the command, or <c>null</c> if no value applies.</param>
        /// <param name="position">The position for the command.</param>
        public PVCSCommandArgument(string command, object commandValue, PVCSCommandArgumentPosition position) {
            if (command == null) {
                throw new ArgumentNullException("command", "The command cannot be null");
            }

            _command = command;
            _commandValue = commandValue;
            _position = position;
        }

        /// <summary>
        /// Compares two PVCS command arguments based on their position.
        /// </summary>
        /// <param name="o">The PVCS command argument to compare to <c>this</c>.</param>
        /// <returns>
        /// Less than zero if this instance is less than <paramref name="o"/>.
        /// Zero if this instance is equal to <paramref name="o"/>.
        /// Greater than zero if this instance is greater than <paramref name="o"/>.
        /// </returns>
        public int CompareTo(object o) {
            PVCSCommandArgument arg = o as PVCSCommandArgument;

            if (arg == null) {
                throw new ArgumentException("o is not a PVCSCommandArgument");
            }

            return Position.CompareTo(arg.Position);
        }

        /// <summary>
        /// Converts this command argument to its <c>string</c> representation.
        /// </summary>
        /// <returns>The <c>string</c> representation of this command argument.</returns>
        public override string ToString() {
            StringBuilder retVal = new StringBuilder();

            retVal.Append(Command);

            if (CommandValue != null) {
                if (CommandValue is string) {
                    retVal.Append(EscapeStringCommandArgument((string) CommandValue));
                } else if (CommandValue is DateTime) {
                    retVal.Append("\"").Append(((DateTime) CommandValue).ToString("g")).Append("\"");
                } else if ((CommandValue is int) || (CommandValue is long) || (CommandValue is float) || (CommandValue is double)) {
                    retVal.Append(CommandValue);
                } else {
                    retVal.Append(EscapeStringCommandArgument(CommandValue.ToString()));
                }
            }

            return retVal.ToString();
        }

        /// <summary>
        /// Escapes a <c>string</c> command line argument.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method attempts to deal with the mess of keeping both PCLI and the shell happy with string
        /// arguments. It's not perfect yet (try an argument with several backslashes before a double quote). It
        /// would be nice to have a regex to handle this but I wouldn't even bother until this logic is spot on.
        /// </para>
        /// </remarks>
        /// <param name="argument">The string argument to escape.</param>
        /// <returns>The escaped string argument.</returns>
        private string EscapeStringCommandArgument(string argument) {
            StringBuilder retVal = new StringBuilder();
            bool bs = false;

            for (int i = 0; i < argument.Length; ++i) {
                char c = argument[i];

                if (c == '"') {
                    if (bs) {
                        //escape the \" by converting it to \\"
                        retVal.Append("\\\\\"");
                    } else {
                        //escape the " by converting it to \"
                        retVal.Append("\\\"");
                    }

                    bs = false;
                } else {
                    retVal.Append(c);
                    bs = (c == '\\');
                }
            }

            //insert a starting quote at the beginning of the argument
            retVal.Insert(0, "\"");

            //if the argument ends with a backslash, escape the backslash before appending the ending quote
            if (retVal[retVal.Length - 1] == '\\') {
                retVal.Append("\\");
            }

            //add the ending quote
            retVal.Append("\"");

            return retVal.ToString();
        }

        #endregion
    }
}
