// Configuration.cs - the Configuration class
// Copyright (C) 2001, 2002  Jason Diamond
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace SLiNgshoT.Core
{
	/// <summary>Represents a configuration (usually Release or Debug).</summary>
	public class Configuration
	{
		private XPathNavigator _Navigator;

		internal Configuration(XPathNavigator navigator)
		{
			_Navigator = navigator.Clone();
		}

		/// <summary>Gets the name of the configuration.</summary>
		/// <remarks>This is usually "Debug" or "Release".</remarks>
		public string Name
		{
			get
			{
				return (string)_Navigator.Evaluate("string(@Name)");
			}
		}

		/// <summary>Gets the location of the output files (relative to the
		/// project directory) for this project's configuration.</summary>
		public string OutputPath
		{
			get
			{
				return (string)_Navigator.Evaluate("string(@OutputPath)");
			}
		}

		/// <summary>Gets the name of the file (relative to the project
		/// directory) into which documentation comments will be
		/// processed.</summary>
		public string DocumentationFile
		{
			get
			{
				return (string)_Navigator.Evaluate("string(@DocumentationFile)");
			}
		}

		public bool DebugSymbols
		{
			get
			{
				return (bool)_Navigator.Evaluate("boolean(@DebugSymbols='true')");
			}
		}

		public bool AllowUnsafeBlocks
		{
			get
			{
				return (bool)_Navigator.Evaluate("boolean(@AllowUnsafeBlocks='true')");
			}
		}

		public bool CheckForOverflowUnderflow
		{
			get
			{
				return (bool)_Navigator.Evaluate("boolean(@CheckForOverflowUnderflow='true')");
			}
		}

		public string DefineConstants
		{
			get
			{
				return (string)_Navigator.Evaluate("string(@DefineConstants)");
			}
		}

	}
}
