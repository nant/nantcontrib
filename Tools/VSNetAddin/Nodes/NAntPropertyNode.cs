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
using System.Xml;
using System.ComponentModel;
using System.Windows.Forms;
using NAnt.Contrib.NAntAddin.Controls;

namespace NAnt.Contrib.NAntAddin.Nodes
{
	/// <summary>
	/// Tree Node that represents an NAnt "property" element.
	/// </summary>
	/// <remarks>None.</remarks>
	public class NAntPropertyNode : NAntTaskNode
	{
		internal XmlElement propertyElement;

		/// <summary>
		/// Creates a new <see cref="NAntPropertyNode"/>.
		/// </summary>
		/// <param name="PropertyElement">The property's XML element.</param>
		/// <param name="ParentElement">The parent XML element of the property.</param>
		/// <remarks>None.</remarks>
		public NAntPropertyNode(XmlElement PropertyElement, XmlElement ParentElement) 
			: base(PropertyElement, ParentElement)
		{
			propertyElement = PropertyElement;
			parentElement = ParentElement;

			Text = PropertyElement.GetAttribute("name");
			ImageIndex = 2;
			SelectedImageIndex = 2;
		}

		/// <summary>
		/// Gets the NAnt Script's "property" XML element.
		/// </summary>
		/// <value>The NAnt Script's "property" XML element.</value>
		/// <remarks>None.</remarks>
		[Browsable(false)]
		public XmlElement PropertyElement
		{
			get
			{
				return propertyElement;
			}
		}

		/// <summary>
		/// Gets or sets the name of the Property.
		/// </summary>
		/// <value>The name of the Property.</value>
		/// <remarks>None.</remarks>
		[Description("The name of the Property."),Category("Appearance")]
		public string Name
		{
			get
			{
				return PropertyElement.GetAttribute("name");
			}

			set
			{
				PropertyElement.SetAttribute("name", value);
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the value of the Property.
		/// </summary>
		/// <value>The value of the Property.</value>
		/// <remarks>None.</remarks>
		[Description("The value of the Property."),Category("Data")]
		public string Value
		{
			get
			{
				return PropertyElement.GetAttribute("value");
			}

			set
			{
				PropertyElement.SetAttribute("value", value);
				Save();
			}
		}
	}
}