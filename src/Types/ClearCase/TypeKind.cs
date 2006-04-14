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
using System.ComponentModel;
using System.Globalization;

namespace NAnt.Contrib.Types.ClearCase {
    public enum TypeKind {
        Attribute,
        Branch,
        Element,
        Hyperlink,
        Label,
        Trigger
    }

    /// <summary>
    /// Specialized <see cref="EnumConverter" /> that supports converting
    /// a <see cref="TypeKind" /> to a string value that can be used in
    /// ClearCase commandline tools.
    /// </summary>
    public class TypeKindConverter : EnumConverter {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeKindConverter" />
        /// class.
        /// </summary>
        public TypeKindConverter() : base(typeof(TypeKind)) {
        }

        /// <summary>
        /// Introduces specialized behavior for converting a <see cref="TypeKind" />
        /// value to a string that can be used in ClearCase commandline tools.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="culture">A <see cref="CultureInfo"/> object. If a <see langword="null"/> is passed, the current culture is assumed.</param>
        /// <param name="value">The <see cref="object"/> to convert.</param>
        /// <param name="destinationType">The <see cref="Type" /> which <paramref name="value" /> should be converted to.</param>
        /// <returns>
        /// An <see cref="object"/> that represents the converted value.
        /// </returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == typeof(string) && value != null && value.GetType() == base.EnumType) {
                TypeKind typeKind = (TypeKind) value;
                switch (typeKind) {
                    case TypeKind.Attribute:
                        return "attype";
                    case TypeKind.Branch:
                        return "brtype";
                    case TypeKind.Element:
                        return "eltype";
                    case TypeKind.Hyperlink:
                        return "hltype";
                    case TypeKind.Label:
                        return "lbtype";
                    case TypeKind.Trigger:
                        return "trtype";
                    default:
                        throw new InvalidEnumArgumentException("value", (int) value,
                            EnumType);
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
