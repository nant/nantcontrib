// NAnt - A .NET build tool
// Copyright (C) 2002 Galileo International
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
//
// Gordon Weakliem (gordon.weakliem@galileo.com)
// 
using System;
using System.DirectoryServices; 
using SourceForge.NAnt;
using SourceForge.NAnt.Tasks;
using SourceForge.NAnt.Attributes;

namespace Galileo.NAnt.Tasks
{
	/// <summary>
	/// Used to get the value of a property from an ADSI object.
	/// </summary>
	[TaskName("adsigetprop")]
	public class ADSIGetPropertyTask : ADSIBaseTask {
		public ADSIGetPropertyTask() {
		}

		private string _propName;
		private string _storeIn;

		/// <summary>
		/// The name of the property to get
		/// </summary>
		[TaskAttribute("propname",Required=true)]
		public String PropName
		{
			get { return _propName; }
			set { _propName = value; }
		}

		/// <summary>
		/// The system property to store the value in.
		/// </summary>
		[TaskAttribute("storein",Required=true)]
		public String StoreIn
		{
			get { return _storeIn; }
			set { _storeIn = value; }
		}

		/// <summary>
		/// Sets the specified property
		/// </summary>
		protected override void ExecuteTask() 
		{
			try
			{
				// Get the directory entry for the specified path and set the 
				// property.
				using (DirectoryEntry pathRoot = new DirectoryEntry(Path))
				{
					pathRoot.RefreshCache();
					if (Project.Properties[StoreIn] == null)
					{
						Project.Properties.Add(StoreIn,"");
					}
					if (pathRoot.Properties[PropName].Value.GetType().IsArray)
					{
						System.Text.StringBuilder sb = new System.Text.StringBuilder();
						foreach (object propValue in (Array)pathRoot.Properties[PropName].Value)
						{
							sb.AppendFormat("{0}" + Environment.NewLine,propValue);
						}
						Project.Properties[StoreIn] = sb.ToString();
					}
					else
					{
						Project.Properties[StoreIn] = pathRoot.Properties[PropName].Value.ToString();
					}
					Log.WriteLine("{0}{3}: Property {1} = {2}", 
						LogPrefix, PropName, Project.Properties[StoreIn],Path);
				}
			}
			catch (Exception e)
			{
				Log.WriteLine("{0}Error reading property {1}: {2}", 
					LogPrefix, PropName,e.Message);
				throw new BuildException(String.Format("Error reading property {0}: {1}", 
					PropName,e.Message),e);
			}
		}
	}
}
