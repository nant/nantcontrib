//
// NAntContrib - NAntAddin
// Copyright (C) 2002 Jayme C. Edwards (jedwards@wi.rr.com)
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

using System;
using System.Collections;

namespace NAnt.Contrib.NAntAddin
{
	/// <summary>
	/// Defines the named commands exposed through 
	/// context menus when using the Addin.
	/// </summary>
	/// <remarks>None.</remarks>
	internal class NAntAddinCommands : ArrayList
	{
		private static NAntAddinCommands commands;

		public const string ADDIN_PROGID = "NAnt.Addin.";
							
							// Project
		public const string BUILD_PROJECT = "BuildProjectCmd",
							FULL_BUILD_PROJECT = ADDIN_PROGID + BUILD_PROJECT,
							VIEW_CODE = "ViewCodeCmd",
							FULL_VIEW_CODE = ADDIN_PROGID + VIEW_CODE,
							ADD_PROJECT_PROPERTY = "AddProjectPropertyCmd",
							FULL_ADD_PROJECT_PROPERTY = ADDIN_PROGID + ADD_PROJECT_PROPERTY,
							ADD_TARGET = "AddTargetCmd",
							FULL_ADD_TARGET = ADDIN_PROGID + ADD_TARGET,
							PASTE_PROJECT = "PasteProjectCmd",
							FULL_PASTE_PROJECT = ADDIN_PROGID + PASTE_PROJECT,
							// Property
							CUT_PROPERTY = "CutPropertyCmd",
							FULL_CUT_PROPERTY = ADDIN_PROGID + CUT_PROPERTY,
							COPY_PROPERTY = "CopyPropertyCmd",
							FULL_COPY_PROPERTY = ADDIN_PROGID + COPY_PROPERTY,
							DELETE_PROPERTY = "DeletePropertyCmd",
							FULL_DELETE_PROPERTY = ADDIN_PROGID + DELETE_PROPERTY,
							RENAME_PROPERTY = "RenamePropertyCmd",
							FULL_RENAME_PROPERTY = ADDIN_PROGID + RENAME_PROPERTY,
							MOVEUP_PROPERTY = "MovePropertyUpCmd",
							FULL_MOVEUP_PROPERTY = ADDIN_PROGID + MOVEUP_PROPERTY,
							MOVEDOWN_PROPERTY = "MovePropertyDownCmd",
							FULL_MOVEDOWN_PROPERTY = ADDIN_PROGID + MOVEDOWN_PROPERTY,
							// Target
							BUILD_TARGET = "BuildTargetCmd",
							FULL_BUILD_TARGET = ADDIN_PROGID + BUILD_TARGET, 
							ADD_TARGET_PROPERTY = "AddTargetPropertyCmd",
							FULL_ADD_TARGET_PROPERTY = ADDIN_PROGID + ADD_TARGET_PROPERTY,
							ADD_TASK = "AddTaskCmd",
							FULL_ADD_TASK = ADDIN_PROGID + ADD_TASK,
							CUT_TARGET = "CutTargetCmd",
							FULL_CUT_TARGET = ADDIN_PROGID + CUT_TARGET,
							COPY_TARGET = "CopyTargetCmd",
							FULL_COPY_TARGET = ADDIN_PROGID + COPY_TARGET,
							PASTE_TARGET = "PasteTargetCmd",
							FULL_PASTE_TARGET = ADDIN_PROGID + PASTE_TARGET,
							DELETE_TARGET = "DeleteTargetCmd",
							FULL_DELETE_TARGET = ADDIN_PROGID + DELETE_TARGET,
							RENAME_TARGET = "RenameTargetCmd",
							FULL_RENAME_TARGET = ADDIN_PROGID + RENAME_TARGET,
							STARTUP_TARGET = "SetAsStartupTargetCmd",
							FULL_STARTUP_TARGET = ADDIN_PROGID + STARTUP_TARGET,
							MOVEUP_TARGET = "MoveTargetUpCmd",
							FULL_MOVEUP_TARGET = ADDIN_PROGID + MOVEUP_TARGET,
							MOVEDOWN_TARGET = "MoveTargetDownCmd",
							FULL_MOVEDOWN_TARGET = ADDIN_PROGID + MOVEDOWN_TARGET,
							// Task
							CUT_TASK = "CutTaskCmd",
							FULL_CUT_TASK = ADDIN_PROGID + CUT_TASK,
							COPY_TASK = "CopyTaskCmd",
							FULL_COPY_TASK = ADDIN_PROGID + COPY_TASK, 
							DELETE_TASK = "DeleteTaskCmd", 
							FULL_DELETE_TASK = ADDIN_PROGID + DELETE_TASK, 
							MOVEUP_TASK = "MoveTaskUpCmd",
							FULL_MOVEUP_TASK = ADDIN_PROGID + MOVEUP_TASK,
							MOVEDOWN_TASK = "MoveTaskDownCmd",
							FULL_MOVEDOWN_TASK = ADDIN_PROGID + MOVEDOWN_TASK;

		/// <summary>
		/// Creates a new <see cref="NAntAddinCommands"/>.
		/// </summary>
		/// <remarks>None.</remarks>
		protected NAntAddinCommands()
		{
			Add(FULL_BUILD_PROJECT);
			Add(FULL_VIEW_CODE);
			Add(FULL_ADD_PROJECT_PROPERTY);
			Add(FULL_ADD_TARGET);
			Add(FULL_PASTE_PROJECT);

			Add(FULL_CUT_PROPERTY);
			Add(FULL_COPY_PROPERTY);
			Add(FULL_DELETE_PROPERTY);
			Add(FULL_RENAME_PROPERTY);
			Add(FULL_MOVEUP_PROPERTY);
			Add(FULL_MOVEDOWN_PROPERTY);

			Add(FULL_BUILD_TARGET);
			Add(FULL_ADD_TARGET_PROPERTY);
			Add(FULL_ADD_TASK);
			Add(FULL_CUT_TARGET);
			Add(FULL_COPY_TARGET);
			Add(FULL_PASTE_TARGET);
			Add(FULL_DELETE_TARGET);
			Add(FULL_RENAME_TARGET);
			Add(FULL_STARTUP_TARGET);
			Add(FULL_MOVEUP_TARGET);
			Add(FULL_MOVEDOWN_TARGET);

			Add(FULL_CUT_TASK);
			Add(FULL_COPY_TASK);
			Add(FULL_DELETE_TASK);
			Add(FULL_MOVEUP_TASK);
			Add(FULL_MOVEDOWN_TASK);
		}

		/// <summary>
		/// Retrieves an instance of the <see cref="NAntAddinCommands"/>.
		/// </summary>
		/// <value>An instance of the <see cref="NAntAddinCommands"/>.</value>
		/// <remarks>None.</remarks>
		public static NAntAddinCommands Commands
		{
			get
			{
				if (commands == null)
				{
					commands = new NAntAddinCommands();
				}
				return commands;
			}
		}
	}

	/// <summary>
	/// Defines the named command bars when using the Addin.
	/// </summary>
	/// <remarks>None.</remarks>
	internal class NAntAddinCommandBars
	{
		public const string PROJECT = "NAntAddinCommandBar",
							PROPERTY = "NAntAddinPropertyPopup",
							TARGET = "NAntAddinTargetPopup",
							TASK = "NAntAddinTaskPopup",
							TOOLBAR = "NAntAddinToolbar";
	}
}