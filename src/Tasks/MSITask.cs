//
// NAntContrib
//
// Copyright (C) 2002 Jayme C. Edwards (jedwards@wi.rr.com)
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
using System.IO;
using System.Xml;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Specialized;

using WindowsInstaller;

using SourceForge.NAnt;
using SourceForge.NAnt.Tasks;
using SourceForge.NAnt.Attributes;

namespace NAnt.Contrib.Tasks
{
    /// <summary>Builds a Windows Installer (MSI) File.</summary>
    /// <remarks>None.</remarks>
    /// <example>
    ///   <code>
    ///     <![CDATA[
    /// <msi>
    /// </msi>
    ///     ]]>
    ///   </code>
    /// </example>
	[TaskName("msi")]
	public class MSITask : Task
	{
		string _output;
		string _debug;
		string _productName;
		string _productLogo;
		string _license;
		string _baseDir;
		XmlNodeList _featureNodes;
		XmlNodeList _componentNodes;
		XmlNodeList _fileNodes;
		XmlNodeList _propertyNodes;
		XmlNodeList _directoryNodes;
		FileSet _mergemodules = new FileSet();

		/// <summary>
		/// Output filename of the MSI file.
		/// </summary>
		[ TaskAttribute("output", Required=true) ]
		public string Output 
		{
			get { return Project.ExpandProperties(_output); }
			set { _output = value; }
		}

		/// <summary>
		/// Base Directory.
		/// </summary>
		[ TaskAttribute("basedir", Required=true) ]
		public string BaseDirectory 
		{
			get { return Project.ExpandProperties(_baseDir); }
			set { _baseDir = value; }
		}

		/// <summary>
		/// Whether to include debug error messages.
		/// </summary>
		[ TaskAttribute("debug", Required=true) ]
		public string Debug 
		{
			get { return Project.ExpandProperties(_debug); }
			set { _debug = value; }
		}

		/// <summary>
		/// Name of the Product being installed.
		/// </summary>
		[ TaskAttribute("productname", Required=true) ]
		public string ProductName 
		{
			get { return Project.ExpandProperties(_productName); }
			set { _productName = value; }
		}

		/// <summary>
		/// Product Logo.
		/// </summary>
		[ TaskAttribute("productlogo", Required=false) ]
		public string ProductLogo 
		{
			get { return Project.ExpandProperties(_productLogo); }
			set { _productLogo = value; }
		}

		/// <summary>
		/// Rich text format license file to use.
		/// </summary>
		[ TaskAttribute("license", Required=true) ]
		public string License 
		{
			get { return Project.ExpandProperties(_license); }
			set { _license = value; }
		}

		/// <summary>The set of merge models to merge.</summary>
		[FileSet("mergemodules")]
		public FileSet MergeModules      { get { return _mergemodules; } }

		/// <summary>
		/// Initialize taks and verify parameters.
		/// </summary>
		/// <param name="taskNode">Node that contains the XML fragment used to define this task instance.</param>
		protected override void InitializeTask(XmlNode taskNode)
		{
			_featureNodes = taskNode.Clone().SelectNodes("features/feature");
			ExpandPropertiesInNodes(_featureNodes);

			_componentNodes = taskNode.Clone().SelectNodes("components/component");
			ExpandPropertiesInNodes(_componentNodes);

			_fileNodes = taskNode.Clone().SelectNodes("files/file");
			ExpandPropertiesInNodes(_fileNodes);

			_propertyNodes = taskNode.Clone().SelectNodes("properties/property");
			ExpandPropertiesInNodes(_propertyNodes);

			_directoryNodes = taskNode.Clone().SelectNodes("directories/directory");
			ExpandPropertiesInNodes(_directoryNodes);
		}

		/// <summary>
		/// Executes the Task.
		/// </summary>
		/// <remarks>None.</remarks>
		protected override void ExecuteTask()
		{
			// Create WindowsInstaller.Installer
			Type msiType = Type.GetTypeFromProgID("WindowsInstaller.Installer");
			Object obj = Activator.CreateInstance(msiType);

			// Open the Template MSI File
			Module tasksModule = Assembly.GetExecutingAssembly().GetModule("NAnt.Contrib.Tasks.dll");
			string source = Path.GetDirectoryName(tasksModule.FullyQualifiedName) + "\\MSITaskTemplate.msi";
			string dest = Path.Combine(Project.BaseDirectory, Output);
			string errors = Path.GetDirectoryName(tasksModule.FullyQualifiedName) + "\\MSITaskErrors.mst";
			
			// Copy the Template MSI File
			try
			{
				File.Copy(source, dest, true);
			}
			catch (IOException)
			{
				Log.WriteLine(LogPrefix + "Error: file in use or cant be copied to output.");
				return;
			}

			// Open the Output Database.
			Database d = null;
			try
			{
				d = (Database)msiType.InvokeMember(
					"OpenDatabase", 
					BindingFlags.InvokeMethod, 
					null, obj, new Object[] {dest, 
					MsiOpenDatabaseMode.msiOpenDatabaseModeDirect});

				if (Debug == "true")
				{
					d.ApplyTransform(errors, MsiTransformError.msiTransformErrorNone);
				}
			}
			catch (Exception e)
			{
				Log.WriteLine(LogPrefix + "Error: " + e.Message);
				return;
			}

			// Select the "Property" Table
			View propView = d.OpenView("SELECT * FROM `Property`");

			Hashtable properties = new Hashtable();

			// Add properties from Task definition
			foreach (XmlNode propNode in _propertyNodes)
			{
				XmlElement propElem = (XmlElement)propNode;

				// Insert the Property
				Record recProp = (Record)msiType.InvokeMember(
					"CreateRecord", 
					BindingFlags.InvokeMethod, 
					null, obj, new object[] { 2 });

				string name = Project.ExpandProperties(propElem.GetAttribute("name"));
				string sValue = Project.ExpandProperties(propElem.GetAttribute("value"));

				recProp.set_StringData(1, name);
				recProp.set_StringData(2, sValue);
				propView.Modify(MsiViewModify.msiViewModifyInsert, recProp);

				properties.Add(name, sValue);

				Log.WriteLine(LogPrefix + "Setting Property: " + name);
			}

			// Create the "Directory" Table
			View dirView = d.OpenView(
				"CREATE TABLE `Directory` (" + 
				"`Directory` CHAR(72) NOT NULL, " + 
				"`Directory_Parent` CHAR(72), " + 
				"`DefaultDir` CHAR(255) NOT NULL LOCALIZABLE " + 
				"PRIMARY KEY `Directory`)");
			dirView.Execute(null);

			// Re-Open the "Directory" Table
			dirView = d.OpenView("SELECT * FROM `Directory`");

			Hashtable directories = new Hashtable();

			// Insert the TARGETDIR Directory
			Record recTargetDir = (Record)msiType.InvokeMember(
				"CreateRecord", 
				BindingFlags.InvokeMethod, 
				null, obj, new object[] { 3 });
			recTargetDir.set_StringData(1, "TARGETDIR");
			recTargetDir.set_StringData(2, null);
			recTargetDir.set_StringData(3, "SourceDir");
			dirView.Modify(MsiViewModify.msiViewModifyInsert, recTargetDir);

			directories.Add("TARGETDIR", new object[] { null, "SourceDir" });

			// Insert the ProgramFilesFolder Directory
			Record recProgFilesDir = (Record)msiType.InvokeMember(
				"CreateRecord", 
				BindingFlags.InvokeMethod, 
				null, obj, new object[] { 3 });
			recProgFilesDir.set_StringData(1, "ProgramFilesFolder");
			recProgFilesDir.set_StringData(2, "TARGETDIR");
			recProgFilesDir.set_StringData(3, ".");
			dirView.Modify(MsiViewModify.msiViewModifyInsert, recProgFilesDir);

			directories.Add("ProgramFilesFolder", new object[] { "TARGETDIR", "." });

			// Pre-cache the directories for building paths
			foreach (XmlNode directoryNode in _directoryNodes)
			{
				XmlElement directoryElem = (XmlElement)directoryNode;

				string name = Project.ExpandProperties(directoryElem.GetAttribute("name"));
				string parent = Project.ExpandProperties(directoryElem.GetAttribute("parent"));
				string sDefault = Project.ExpandProperties(directoryElem.GetAttribute("default"));

				directories.Add(name, new object[] { parent, sDefault });
			}

			// Add directories from Task definition
			foreach (XmlNode directoryNode in _directoryNodes)
			{
				XmlElement directoryElem = (XmlElement)directoryNode;

				string name = Project.ExpandProperties(directoryElem.GetAttribute("name"));
				string parent = Project.ExpandProperties(directoryElem.GetAttribute("parent"));
				string sDefault = Project.ExpandProperties(directoryElem.GetAttribute("default"));

				// Insert the Directory
				Record recDir = (Record)msiType.InvokeMember(
					"CreateRecord", 
					BindingFlags.InvokeMethod, 
					null, obj, new object[] { 3 });
				recDir.set_StringData(1, name);
				recDir.set_StringData(2, parent);
				
				StringBuilder relativePath = new StringBuilder();
				GetRelativePath(directories, name, parent, sDefault, relativePath);

				string basePath = Path.Combine(Project.BaseDirectory, _baseDir);
				string fullPath = Path.Combine(basePath, relativePath.ToString());
				string path = GetShortPath(fullPath) + "|" + sDefault;

				Log.WriteLine(LogPrefix + "Directory: " + Path.Combine(Path.Combine(_baseDir, relativePath.ToString()), sDefault));
				
				recDir.set_StringData(3, path);
				dirView.Modify(MsiViewModify.msiViewModifyInsert, recDir);
			}

			// Create the "Feature" Table
			View featView = d.OpenView(
				"CREATE TABLE `Feature` (" + 
				"`Feature` CHAR(38) NOT NULL, " + 
				"`Feature_Parent` CHAR(38), " + 
				"`Title` CHAR(64) LOCALIZABLE, " + 
				"`Description` CHAR(255) LOCALIZABLE, " + 
				"`Display` SHORT, " + 
				"`Level` SHORT NOT NULL, " + 
				"`Directory_` CHAR(72), " + 
				"`Attributes` SHORT NOT NULL " + 
				"PRIMARY KEY `Feature`)");
			featView.Execute(null);

			Hashtable featureComponents = new Hashtable();

			// Re-Open the "Feature" Table
			featView = d.OpenView("SELECT * FROM `Feature`");

			// Add features from Task definition
			foreach (XmlNode featureNode in _featureNodes)
			{
				XmlElement featureElem = (XmlElement)featureNode;

				string attr = featureElem.GetAttribute("attr");

				string description = null;
				XmlNode descNode = featureElem.SelectSingleNode("description/text()");
				if (descNode != null)
				{
					description = descNode.Value;
				}

				string directory = null;
				XmlNode dirNode = featureElem.SelectSingleNode("directory/@ref");
				if (dirNode != null)
				{
					directory = dirNode.Value;
				}

				string name = Project.ExpandProperties(featureElem.GetAttribute("name"));

				// Insert the Feature
				Record recFeat = (Record)msiType.InvokeMember(
					"CreateRecord", 
					BindingFlags.InvokeMethod, 
					null, obj, new object[] { 8 });
				recFeat.set_StringData(1, name);
				recFeat.set_StringData(2, Project.ExpandProperties(featureElem.GetAttribute("parent")));
				recFeat.set_StringData(3, Project.ExpandProperties(featureElem.GetAttribute("title")));
				recFeat.set_StringData(4, Project.ExpandProperties(description));
				recFeat.set_StringData(5, Project.ExpandProperties(featureElem.GetAttribute("display")));
				recFeat.set_StringData(6, Project.ExpandProperties(featureElem.GetAttribute("level")));
				recFeat.set_StringData(7, Project.ExpandProperties(directory));
				recFeat.set_StringData(8, (attr == null || attr == "") ? "0" : 
					Int32.Parse(Project.ExpandProperties(attr)).ToString());
				featView.Modify(MsiViewModify.msiViewModifyInsert, recFeat);

				Log.WriteLine(LogPrefix + "Feature: " + name);

				XmlNodeList featureComponentNodes = featureElem.SelectNodes("component");
				foreach (XmlNode featureComponentNode in featureComponentNodes)
				{
					XmlElement featureComponentElem = (XmlElement)featureComponentNode;
					featureComponents.Add(featureComponentElem.GetAttribute("ref"), name);
				}
			}

			Hashtable components = new Hashtable();

			// Create the "Component" Table
			View compView = d.OpenView(
				"CREATE TABLE `Component` (" + 
				"`Component` CHAR(72) NOT NULL, " + 
				"`ComponentId` CHAR(38), " + 
				"`Directory_` CHAR(72) NOT NULL, " + 
				"`Attributes` SHORT NOT NULL, " + 
				"`Condition` CHAR(255), " + 
				"`KeyPath` CHAR(72) " + 
				"PRIMARY KEY `Component`)");
			compView.Execute(null);

			// Re-Open the "Component" Table
			compView = d.OpenView("SELECT * FROM `Component`");

			// Add components from Task definition
			foreach (XmlNode compNode in _componentNodes)
			{
				XmlElement compElem = (XmlElement)compNode;

				string attr = compElem.GetAttribute("attr");

				string directory = null;
				XmlNode dirNode = compElem.SelectSingleNode("directory/@ref");
				if (dirNode != null)
				{
					directory = dirNode.Value;
				}

				string keyFile = null;
				XmlNode keyFileNode = compElem.SelectSingleNode("file/@ref");
				if (keyFileNode != null)
				{
					keyFile = keyFileNode.Value;
				}

				string name = Project.ExpandProperties(compElem.GetAttribute("name"));
				string dir = Project.ExpandProperties(directory);

				// Insert the Component
				Record recComp = (Record)msiType.InvokeMember(
					"CreateRecord", 
					BindingFlags.InvokeMethod, 
					null, obj, new object[] { 6 });
				recComp.set_StringData(1, name);
				recComp.set_StringData(2, Project.ExpandProperties(compElem.GetAttribute("id")));
				recComp.set_StringData(3, dir);
				recComp.set_StringData(4, (attr == null || attr == "") ? "0" : 
					Int32.Parse(Project.ExpandProperties(attr)).ToString());
				recComp.set_StringData(5, Project.ExpandProperties(compElem.GetAttribute("condition")));
				recComp.set_StringData(6, Project.ExpandProperties(keyFile));
				compView.Modify(MsiViewModify.msiViewModifyInsert, recComp);

				Log.WriteLine(LogPrefix + "Component: " + name);

				components.Add(name, directory);
			}

			// Create the "File" Table
			View fileView = d.OpenView(
				"CREATE TABLE `File` (" + 
				"`File` CHAR(72) NOT NULL, " + 
				"`Component_` CHAR(72) NOT NULL, " + 
				"`FileName` CHAR(255) NOT NULL LOCALIZABLE, " + 
				"`FileSize` LONG NOT NULL, " + 
				"`Version` CHAR(72), " + 
				"`Language` CHAR(20), " + 
				"`Attributes` SHORT, " + 
				"`Sequence` SHORT NOT NULL " + 
				"PRIMARY KEY `File`)");
			fileView.Execute(null);

			// Re-Open the "File" Table
			fileView = d.OpenView("SELECT * FROM `File`");

			// Add files from Task definition
			foreach (XmlNode fileNode in _fileNodes)
			{
				XmlElement fileElem = (XmlElement)fileNode;

				string attr = fileElem.GetAttribute("attr");

				string component = null;
				XmlNode compNode = fileElem.SelectSingleNode("component/@ref");
				if (compNode != null)
				{
					component = Project.ExpandProperties(compNode.Value);
				}

				string componentDir = (string)components[component];
				object[] componentDirInfo = (object[])directories[componentDir];
				StringBuilder relativePath = new StringBuilder();
				GetRelativePath(directories, componentDir, (string)componentDirInfo[0], (string)componentDirInfo[1], relativePath);

				string basePath = Path.Combine(Project.BaseDirectory, _baseDir);
				string fullPath = Path.Combine(basePath, relativePath.ToString());

				// Insert the File
				Record recFile = (Record)msiType.InvokeMember(
					"CreateRecord", 
					BindingFlags.InvokeMethod, 
					null, obj, new object[] { 8 });
				recFile.set_StringData(1, Project.ExpandProperties(fileElem.GetAttribute("name")));
				recFile.set_StringData(2, component);

				string fileName = Project.ExpandProperties(fileElem.GetAttribute("file"));
				string filePath = Path.Combine(fullPath, fileName);
				
				if (File.Exists(filePath))
				{
					recFile.set_StringData(3, GetShortFile(filePath) + "|" + fileName);

					FileStream fileStream = null;
					try
					{
						fileStream = File.OpenRead(filePath);
						recFile.set_StringData(4, fileStream.Length.ToString());
					}
					catch (Exception)
					{
						Log.WriteLine(LogPrefix + "ERROR: Could not open file " + filePath);
						return;
					}
				}
				else
				{
					Log.WriteLine(LogPrefix + "ERROR: Could not open file " + filePath);
					return;
				}

				Log.WriteLine(LogPrefix + "File: " + Path.Combine(Path.Combine(_baseDir, relativePath.ToString()), fileName));

				recFile.set_StringData(5, Project.ExpandProperties(fileElem.GetAttribute("version")));
				recFile.set_StringData(6, Project.ExpandProperties(fileElem.GetAttribute("lang")));
				recFile.set_StringData(7, (attr == null || attr == "") ? "0" : 
					Int32.Parse(Project.ExpandProperties(attr)).ToString());
				recFile.set_StringData(8, Project.ExpandProperties(fileElem.GetAttribute("order")));
				fileView.Modify(MsiViewModify.msiViewModifyInsert, recFile);
			}

			// Create the "FeatureComponents" Table
			View featCompView = d.OpenView(
				"CREATE TABLE `FeatureComponents` (" + 
				"`Feature_` CHAR(38) NOT NULL, " + 
				"`Component_` CHAR(72) NOT NULL " + 
				"PRIMARY KEY `Feature_`, `Component_`)");
			featCompView.Execute(null);

			// Re-Open the "FeatureComponents" Table
			featCompView = d.OpenView("SELECT * FROM `FeatureComponents`");

			// Add featureComponents from Task definition
			IEnumerator keyEnum = featureComponents.Keys.GetEnumerator();
			while (keyEnum.MoveNext())
			{
				string component = Project.ExpandProperties((string)keyEnum.Current);
				string feature = Project.ExpandProperties((string)featureComponents[component]);
				
				// Insert the FeatureComponent
				Record recFeatComps = (Record)msiType.InvokeMember(
					"CreateRecord", 
					BindingFlags.InvokeMethod, 
					null, obj, new object[] { 2 });
				recFeatComps.set_StringData(1, feature);
				recFeatComps.set_StringData(2, component);
				featCompView.Modify(MsiViewModify.msiViewModifyInsert, recFeatComps);

				Log.WriteLine(LogPrefix + 
					"Mapping \"" + feature + 
					"\" to \"" + component + "\".");
			}

			// Create the "Media" Table
			View mediaView = d.OpenView(
				"CREATE TABLE `Media` (" + 
				"`DiskId` SHORT NOT NULL, " + 
				"`LastSequence` SHORT NOT NULL, " + 
				"`DiskPrompt` CHAR(64) LOCALIZABLE, " + 
				"`Cabinet` CHAR(255), " + 
				"`VolumeLabel` CHAR(32), " + 
				"`Source` CHAR(72) " + 
				"PRIMARY KEY `DiskId`)");
			mediaView.Execute(null);

			// Re-Open the "Media" Table
			mediaView = d.OpenView("SELECT * FROM `Media`");

			// Insert the Disk
			Record recMedia = (Record)msiType.InvokeMember(
				"CreateRecord", 
				BindingFlags.InvokeMethod, 
				null, obj, new object[] { 6 });
			recMedia.set_StringData(1, "1");
			recMedia.set_StringData(2, "1");
			mediaView.Modify(MsiViewModify.msiViewModifyInsert, recMedia);

			SummaryInfo summaryInfo = d.get_SummaryInformation(200);
			summaryInfo.set_Property(2, properties["ProductName"]);
			summaryInfo.set_Property(3, properties["ProductName"]);
			summaryInfo.set_Property(4, properties["Manufacturer"]);
			summaryInfo.set_Property(5, properties["Keywords"]);
			summaryInfo.set_Property(6, properties["Comments"]);
			summaryInfo.set_Property(9, "{"+Guid.NewGuid().ToString().ToUpper()+"}");
			summaryInfo.set_Property(14, 200);
			summaryInfo.set_Property(15, 0);
			
			summaryInfo.Persist();

			Log.WriteLine();
			Log.Write(LogPrefix + "Saving " + Output + "...");

			try
			{
				d.Commit();
				d = null;
			}
			catch (Exception e)
			{
				Log.WriteLine(LogPrefix + "Error: " + e.Message);
			}
			Log.WriteLine("Done.");
		}

		/// <summary>Perform macro expansion for the given XmlNodeList.</summary>
		void ExpandPropertiesInNodes(XmlNodeList nodes) 
		{
			foreach(XmlNode node in nodes ) 
			{
				if (node.ChildNodes != null)
				{
					ExpandPropertiesInNodes(node.ChildNodes);
					if (node.Attributes != null)
					{
						foreach( XmlAttribute attr in node.Attributes ) 
						{
							attr.Value = Project.ExpandProperties(attr.Value);
						}
					}
				}
			}
		}

		[DllImport("kernel32.dll", CharSet=CharSet.Auto)]
		private static extern int GetShortPathName(string LongPath, StringBuilder ShortPath, int BufferSize); 
		
		private string GetShortFile(string LongFile)
		{
			if (LongFile.Length <= 8)
			{
				return LongFile;
			}

			StringBuilder shortPath = new StringBuilder(255);
			int result = GetShortPathName(LongFile, shortPath, shortPath.Capacity);
			return Path.GetFileName(shortPath.ToString());
		}

		private string GetShortPath(string LongPath)
		{
			if (LongPath.Length <= 8)
			{
				return LongPath;
			}

			StringBuilder shortPath = new StringBuilder(255);
			int result = GetShortPathName(LongPath, shortPath, shortPath.Capacity);
			Uri shortPathUri = new Uri("file://" + shortPath.ToString());

			string[] shortPathSegments = shortPathUri.Segments;
			if (shortPathSegments.Length == 0)
			{
				return LongPath;
			}
			if (shortPathSegments.Length == 1)
			{
				return shortPathSegments[0];
			}
			return shortPathSegments[shortPathSegments.Length-1];
		}

		private void GetRelativePath(
			Hashtable directories, 
			string Name, 
			string Parent, 
			string Default, 
			StringBuilder Path)
		{
			if (Name == "ProgramFilesFolder" || Name == "TARGETDIR")
			{
				return;
			}

			if (Path.Length > 0)
			{
				Path.Insert(0, @"\");
			}

			Path.Insert(0, Default);

			if (Parent != null)
			{
				object[] PathInfo = (object[])directories[Parent];
				GetRelativePath(directories, Parent, (string)PathInfo[0], (string)PathInfo[1], Path);
			}
		}
	}
}