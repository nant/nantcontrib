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

namespace NAnt.Contrib.NAntAddin.Nodes
{
	/// <summary>
	/// Attribute that an <see cref="NAntTaskNode"/> must 
	/// be marked with to show up in the "Add Task" dialog 
	/// and the 
	/// <see cref="NAnt.Contrib.NAntAddin.Controls.ScriptExplorerControl"/>.
	/// </summary>
	/// <remarks>None.</remarks>
	public class NAntTaskAttribute : Attribute
	{
		private string name, description, image;

		/// <summary>
		/// Creates a new <see cref="NAntTaskAttribute"/>.
		/// </summary>
		/// <param name="Name">The name of the task's XML element.</param>
		/// <param name="Description">A description of the task's purpose.</param>
		/// <remarks>None.</remarks>
		public NAntTaskAttribute(string Name, string Description) 
			: this(Name, Description, null)
		{
		}

		/// <summary>
		/// Creates a new <see cref="NAntTaskAttribute"/>.
		/// </summary>
		/// <param name="Name">The name of the task's XML element.</param>
		/// <param name="Description">A description of the task's purpose.</param>
		/// <param name="Image">The name of the file containing a 16x16 
		/// pixel bitmap to be used for this task's image in the 
		/// <see cref="NAnt.Contrib.NAntAddin.Controls.ScriptExplorerControl"/> 
		/// and the "Add Task" dialog.</param>
		/// <remarks>The Image parameter should contain only the name of the 
		/// .bmp file, and the file should be placed in the "TaskImages" 
		/// subdirectory of your NAnt Addin installation directory.</remarks>
		public NAntTaskAttribute(string Name, string Description, string Image)
		{
			name = Name;
			description = Description;
			image = Image;
		}

		/// <summary>
		/// Returns the name of the task's XML element.
		/// </summary>
		/// <value>The name of the task's XML element.</value>
		/// <remarks>None.</remarks>
		public string Name
		{
			get
			{
				return name;
			}
		}

		/// <summary>
		/// Returns a description of the task's purpose.
		/// </summary>
		/// <value>A description of the task's purpose.</value>
		/// <remarks>None.</remarks>
		public string Description
		{
			get
			{
				return description;
			}
		}

		/// <summary>
		/// Returns the name of the file containing a 16x16 
		/// pixel bitmap to be used for this task's image in the 
		/// <see cref="NAnt.Contrib.NAntAddin.Controls.ScriptExplorerControl"/> 
		/// and the "Add Task" dialog.
		/// </summary>
		/// <value>Should contain only the name of the 
		/// .bmp file, and the file should be placed in the "TaskImages" 
		/// subdirectory of your NAnt Addin installation directory.</value>
		/// <remarks>None.</remarks>
		public string Image
		{
			get
			{
				return image;
			}
		}
	}
}