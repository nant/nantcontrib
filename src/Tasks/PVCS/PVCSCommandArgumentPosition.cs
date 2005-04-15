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

namespace NAnt.Contrib.Tasks.PVCS {
    /// <summary>
    /// Defines possible values for specifying positions for PCLI command arguments and arguments to PCLI itself.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Members of this enumeration are used to specify relative positions of PCLI command arguments. All arguments
    /// given a position of <see cref="End"/> will appear after arguments with a position of <see cref="Middle"/>
    /// or <see cref="Start"/>. Similarly, arguments with a position of <see cref="Middle"/> will appear after
    /// those with a position of <see cref="Start"/> but before those with a position of <see cref="End"/>.
    /// </para>
    /// <para>
    /// No order is guaranteed for arguments with the same position. That is, if two arguments have a position of
    /// <see cref="Start"/>, it is not possible to specify which one is output to the command line first.
    /// </para>
    /// <para>
    /// The <see cref="BeforePCLICommand"/> member is special in that it ensures the argument will appear before
    /// the PCLI command name. This is useful when the argument is to PCLI itself, not the PCLI command.
    /// </para>
    /// </remarks>
    public enum PVCSCommandArgumentPosition {
        /// <summary>
        /// Arguments that should appear before the PCLI command argument. This is useful for arguments to PCLI
        /// itself (as opposed to the PCLI command).
        /// </summary>
        BeforePCLICommand = 0,
        /// <summary>
        /// PCLI command arguments that should appear before other PCLI command arguments.
        /// </summary>
        Start = 1,
        /// <summary>
        /// PCLI command arguments that should appear before other arguments with a position of <see cref="End"/>
        /// but after other arguments with a position of <see cref="Start"/>.
        /// </summary>
        Middle = 2,
        /// <summary>
        /// PCLI command arguments that should appear after other PCLI command arguments.
        /// </summary>
        End = 3
    }
}
