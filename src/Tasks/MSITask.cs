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
using System.Diagnostics;
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
		string _banner;
		string _background;
		string _license;
		string _sourceDir;
		
		XmlNodeList _featureNodes;
		XmlNodeList _componentNodes;
		XmlNodeList _fileNodes;
		XmlNodeList _propertyNodes;
		XmlNodeList _directoryNodes;
		XmlNodeList _keyNodes;

		Hashtable directories = new Hashtable();
		Hashtable features = new Hashtable();
		Hashtable featureComponents = new Hashtable();
		Hashtable components = new Hashtable();
		Hashtable properties = new Hashtable();

		FileSet _mergemodules = new FileSet();

		/// <summary>
		/// Output filename of the MSI file.
		/// </summary>
		[TaskAttribute("output", Required=true)]
		public string Output 
		{
			get
			{
				return Project.ExpandProperties(_output);
			}
			
			set
			{
				_output = value;
			}
		}

		/// <summary>
		/// Root directory relative to installation folders of source files.
		/// </summary>
		[TaskAttribute("sourcedir", Required=true)]
		public string SourceDirectory 
		{
			get
			{
				return Project.ExpandProperties(_sourceDir);
			}

			set
			{
				_sourceDir = value;
			}
		}

		/// <summary>
		/// Whether to include debug error messages.
		/// </summary>
		[TaskAttribute("debug", Required=true)]
		public string Debug 
		{
			get
			{
				return Project.ExpandProperties(_debug);
			}

			set
			{
				_debug = value;
			}
		}

		/// <summary>
		/// Banner Image.
		/// </summary>
		[TaskAttribute("banner", Required=false)]
		public string Banner 
		{
			get
			{
				return Project.ExpandProperties(_banner);
			}

			set
			{
				_banner = value;
			}
		}

		/// <summary>
		/// Background Image.
		/// </summary>
		[TaskAttribute("background", Required=false)]
		public string Background 
		{
			get
			{
				return Project.ExpandProperties(_background);
			}

			set
			{
				_background = value;
			}
		}

		/// <summary>
		/// Rich text format license file to use.
		/// </summary>
		[TaskAttribute("license", Required=true)]
		public string License 
		{
			get
			{
				return Project.ExpandProperties(_license);
			}

			set
			{
				_license = value;
			}
		}

		/// <summary>The set of merge models to merge.</summary>
		[FileSet("mergemodules")]
		public FileSet MergeModules
		{
			get
			{
				return _mergemodules;
			}
		}

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

			_keyNodes = taskNode.Clone().SelectNodes("registry/key");
			ExpandPropertiesInNodes(_keyNodes);
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
			string dest = Path.Combine(Project.BaseDirectory, Path.Combine(SourceDirectory, Output));
			string errors = Path.GetDirectoryName(tasksModule.FullyQualifiedName) + "\\MSITaskErrors.mst";
			
			// Copy the Template MSI File
			try
			{
				File.Copy(source, dest, true);
			}
			catch (IOException)
			{
				Log.WriteLine(LogPrefix + "Error: file in use or cannot be copied to output.");
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
					// If Debug is true, transform the error strings in
					d.ApplyTransform(errors, MsiTransformError.msiTransformErrorNone);
				}
			}
			catch (Exception e)
			{
				Log.WriteLine(LogPrefix + "Error: " + e.Message);
				return;
			}

			Log.WriteLine(LogPrefix + "Building MSI Database \"" + Output + "\".");

			// Load the Banner Image
			if (!LoadBanner(d))
			{
				return;
			}

			// Load the Background Image
			if (!LoadBackground(d))
			{
				return;
			}

			// Load the License File
			if (!LoadLicense(d))
			{
				return;
			}

			// Load Properties
			if (!LoadProperties(d, msiType, obj, ref properties))
			{
				return;
			}

			// Load Directories
			if (!LoadDirectories(d, msiType, obj, ref directories))
			{
				return;
			}

			// Load Features
			if (!LoadFeatures(d, msiType, obj, ref features))
			{
				return;
			}

			int lastSequence = 0;

			// Load Components
			if (!LoadComponents(d, msiType, obj, ref components, ref featureComponents, ref lastSequence))
			{
				return;
			}

			// Load the Registry
			if (!LoadRegistry(d, msiType, obj))
			{
				return;
			}

			// Load Media
			if (!LoadMedia(d, msiType, obj, lastSequence))
			{
				return;
			}

			// Load Summary Information
			if (!LoadSummaryInfo(d))
			{
				return;
			}

			// Compress Files
			if (!CreateCabFile(d, msiType, obj))
			{
				return;
			}

			try
			{
				Log.Write(LogPrefix + "Saving MSI Database...");

				// Commit the MSI Database
				d.Commit();
				d = null;
			}
			catch (Exception e)
			{
				Log.WriteLine(LogPrefix + "Error: " + e.Message);
				return;
			}

			Log.WriteLine("Done.");
		}

		/// <summary>
		/// Loads the banner iamge.
		/// </summary>
		/// <param name="d">The MSI database.</param>
		/// <returns>True if successful.</returns>
		private bool LoadBanner(Database d)
		{
			// Try to open the Banner
			if (Banner != null)
			{
				string bannerFile = Path.Combine(Project.BaseDirectory, Banner);
				if (File.Exists(bannerFile))
				{
					View bannerView = d.OpenView("SELECT * FROM `Binary` WHERE `Name`='bannrbmp'");
					bannerView.Execute(null);
					Record bannerRecord = bannerView.Fetch();
					if (Verbose)
					{
						Log.WriteLine(LogPrefix + "Storing Banner: " + bannerFile + ".");
					}

					// Write the Banner file to the MSI database
					bannerRecord.SetStream(2, bannerFile);
					bannerView.Modify(MsiViewModify.msiViewModifyUpdate, bannerRecord);
					bannerView.Close();
				}
				else
				{
					Log.WriteLine(LogPrefix + 
						"Error: Unable to open Banner Image:\n\n\t" + 
						bannerFile + "\n\n");
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Loads the background image.
		/// </summary>
		/// <param name="d">The MSI database.</param>
		/// <returns>True if successful.</returns>
		private bool LoadBackground(Database d)
		{
			// Try to open the Background
			if (Background != null)
			{
				string bgFile = Path.Combine(Project.BaseDirectory, Background);
				if (File.Exists(bgFile))
				{
					View bgView = d.OpenView("SELECT * FROM `Binary` WHERE `Name`='dlgbmp'");
					bgView.Execute(null);
					Record bgRecord = bgView.Fetch();
					if (Verbose)
					{
						Log.WriteLine(LogPrefix + "Storing Background: " + bgFile + ".");
					}

					// Write the Background file to the MSI database
					bgRecord.SetStream(2, bgFile);
					bgView.Modify(MsiViewModify.msiViewModifyUpdate, bgRecord);
					bgView.Close();
				}
				else
				{
					Log.WriteLine(LogPrefix + "Error: Unable to open Background Image:\n\n\t" + bgFile + "\n\n");
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Loads the license file.
		/// </summary>
		/// <param name="d">The MSI database.</param>
		/// <returns>True if successful.</returns>
		private bool LoadLicense(Database d)
		{
			// Try to open the License
			if (License != null)
			{
				string licFile = Path.Combine(Project.BaseDirectory, License);
				if (File.Exists(licFile))
				{
					View licView = d.OpenView("SELECT * FROM `Control` WHERE `Control`='AgreementText'");
					licView.Execute(null);
					Record licRecord = licView.Fetch();
					if (Verbose)
					{
						Log.WriteLine(LogPrefix + "Storing License: " + licFile + ".");
					}
					StreamReader licReader = null;
					try
					{
						licReader = File.OpenText(licFile);
						licRecord.set_StringData(10, licReader.ReadToEnd());
						licView.Modify(MsiViewModify.msiViewModifyUpdate, licRecord);
					}
					catch (IOException)
					{
						Log.WriteLine(LogPrefix + "Error opening License: " + licFile + ".");
						return false;
					}
					finally
					{
						licView.Close();
					}
				}
				else
				{
					Log.WriteLine(LogPrefix + "Error: Unable to open License:\n\n\t" + licFile + "\n\n");
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Loads records for the Properties table.
		/// </summary>
		/// <param name="d">The MSI database.</param>
		/// <param name="msiType">The MSI Installer type.</param>
		/// <param name="obj">The MSI Installer object.</param>
		/// <param name="properties">Array of properties.</param>
		/// <returns>True if successful.</returns>
		private bool LoadProperties(Database d, Type msiType, Object obj, ref Hashtable properties)
		{
			// Select the "Property" Table
			View propView = d.OpenView("SELECT * FROM `Property`");

			// Add properties from Task definition
			foreach (XmlNode propNode in _propertyNodes)
			{
				XmlElement propElem = (XmlElement)propNode;

				// Insert the Property
				Record recProp = (Record)msiType.InvokeMember(
					"CreateRecord", 
					BindingFlags.InvokeMethod, 
					null, obj, new object[] { 2 });

				string name = propElem.GetAttribute("name");
				string sValue = propElem.GetAttribute("value");

				recProp.set_StringData(1, name);
				recProp.set_StringData(2, sValue);
				propView.Modify(MsiViewModify.msiViewModifyInsert, recProp);

				properties.Add(name, sValue);

				if (Verbose)
				{
					Log.WriteLine(LogPrefix + "Setting Property: " + name);
				}
			}

			return true;
		}

		/// <summary>
		/// Loads records for the Components table.
		/// </summary>
		/// <param name="d">The MSI database.</param>
		/// <param name="msiType">The MSI Installer type.</param>
		/// <param name="obj">The MSI Installer object.</param>
		/// <param name="components">Array of components.</param>
		/// <param name="featureComponents">Array of feature component mappings.</param>
		/// <param name="lastSequence">The sequence number of the last file in the .cab</param>
		/// <returns>True if successful.</returns>
		private bool LoadComponents(Database d, Type msiType, Object obj, ref Hashtable components, ref Hashtable featureComponents, ref int lastSequence)
		{
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

			// Add components from Task definition
			int componentIndex = 1;
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
				XmlNode keyFileNode = compElem.SelectSingleNode("key/@file");
				if (keyFileNode != null)
				{
					keyFile = keyFileNode.Value;
				}

				string name = compElem.GetAttribute("name");
				string dir = directory;

				// Insert the Component
				Record recComp = (Record)msiType.InvokeMember(
					"CreateRecord", 
					BindingFlags.InvokeMethod, 
					null, obj, new object[] { 6 });
				recComp.set_StringData(1, name);
				recComp.set_StringData(2, compElem.GetAttribute("id"));
				recComp.set_StringData(3, dir);
				recComp.set_StringData(4, (attr == null || attr == "") ? "0" : 
					Int32.Parse(attr).ToString());
				recComp.set_StringData(5, compElem.GetAttribute("condition"));

				compView.Modify(MsiViewModify.msiViewModifyInsert, recComp);

				if (Verbose)
				{
					Log.WriteLine(LogPrefix + "Component: " + name);
				}

				components.Add(name, directory);

				Hashtable files = AddFiles(directories, fileView, msiType, obj, dir, name, componentIndex++, ref lastSequence);
				if (files == null)
				{
					return true;
				}

				if (files.Contains(keyFile))
				{
					recComp.set_StringData(6, (string)files[keyFile]);
					compView.Modify(MsiViewModify.msiViewModifyUpdate, recComp);
				}
				else
				{
					Log.WriteLine(
						LogPrefix + "Error: KeyFile \"" + keyFile + 
						"\" not found in Component \"" + name + "\".");
					return false;
				}

				XmlNodeList featureComponentNodes = compElem.SelectNodes("feature");
				foreach (XmlNode featureComponentNode in featureComponentNodes)
				{
					XmlElement featureComponentElem = (XmlElement)featureComponentNode;
					featureComponents.Add(name, featureComponentElem.GetAttribute("ref"));
				}
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

				if (Verbose)
				{
					Log.WriteLine(LogPrefix + 
						"Mapping \"" + feature + 
						"\" to \"" + component + "\".");
				}
			}

			return true;
		}

		/// <summary>
		/// Loads records for the Directories table.
		/// </summary>
		/// <param name="d">The MSI database.</param>
		/// <param name="msiType">The MSI Installer type.</param>
		/// <param name="obj">The MSI Installer object.</param>
		/// <param name="directories">Array of directories.</param>
		/// <returns>True if successful.</returns>
		private bool LoadDirectories(Database d, Type msiType, Object obj, ref Hashtable directories)
		{
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

				string basePath = Path.Combine(Project.BaseDirectory, _sourceDir);
				string fullPath = Path.Combine(basePath, relativePath.ToString());
				string path = GetShortPath(fullPath) + "|" + sDefault;

				if (Verbose)
				{
					Log.WriteLine(LogPrefix + "Directory: " + Path.Combine(Path.Combine(_sourceDir, relativePath.ToString()), sDefault));
				}
				
				recDir.set_StringData(3, path);
				dirView.Modify(MsiViewModify.msiViewModifyInsert, recDir);
			}

			return true;
		}

		/// <summary>
		/// Loads records for the Media table.
		/// </summary>
		/// <param name="d">The MSI database.</param>
		/// <param name="msiType">The MSI Installer type.</param>
		/// <param name="obj">The MSI Installer object.</param>
		/// <param name="lastSequence">The sequence number of the last file in the .cab.</param>
		/// <returns>True if successful.</returns>
		private bool LoadMedia(Database d, Type msiType, Object obj, int lastSequence)
		{
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
			recMedia.set_StringData(2, lastSequence.ToString());
			recMedia.set_StringData(4, "#" + Path.GetFileNameWithoutExtension(Output) + ".cab");
			mediaView.Modify(MsiViewModify.msiViewModifyInsert, recMedia);

			return true;
		}

		/// <summary>
		/// Loads properties for the Summary Information Stream.
		/// </summary>
		/// <param name="d">The MSI database.</param>
		/// <returns>True if successful.</returns>
		private bool LoadSummaryInfo(Database d)
		{
			SummaryInfo summaryInfo = d.get_SummaryInformation(200);
			summaryInfo.set_Property(2, properties["ProductName"]);
			summaryInfo.set_Property(3, properties["ProductName"]);
			summaryInfo.set_Property(4, properties["Manufacturer"]);
			summaryInfo.set_Property(5, properties["Keywords"]);
			summaryInfo.set_Property(6, properties["Comments"]);
			summaryInfo.set_Property(9, "{"+Guid.NewGuid().ToString().ToUpper()+"}");
			summaryInfo.set_Property(14, 200);
			summaryInfo.set_Property(15, 2);
			
			summaryInfo.Persist();

			return true;
		}

		/// <summary>
		/// Loads records for the Features table.
		/// </summary>
		/// <param name="d">The MSI database.</param>
		/// <param name="msiType">The MSI Installer type.</param>
		/// <param name="obj">The MSI Installer object.</param>
		/// <param name="features">Array of features.</param>
		/// <returns>True if successful.</returns>
		private bool LoadFeatures(Database d, Type msiType, Object obj, ref Hashtable features)
		{
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

			// Re-Open the "Feature" Table
			featView = d.OpenView("SELECT * FROM `Feature`");

			// Add features from Task definition
			int order = 1;
			int depth = 1;
			foreach (XmlNode featureNode in _featureNodes)
			{
				XmlElement featureElem = (XmlElement)featureNode;
				AddFeature(features, featView, null, msiType, obj, featureElem, depth, order);

				string name = Project.ExpandProperties(featureElem.GetAttribute("name"));

				order++;
			}

			return true;
		}

		/// <summary>
		/// Adds a feature record to the Features table.
		/// </summary>
		/// <param name="Features">Array of features.</param>
		/// <param name="view">The MSI database view.</param>
		/// <param name="parent">The name of this feature's parent.</param>
		/// <param name="msiType">The MSI Installer type.</param>
		/// <param name="obj">The MSI INstaller object.</param>
		/// <param name="featureElem">This Feature's XML element.</param>
		/// <param name="depth">The tree depth of this feature.</param>
		/// <param name="order">The tree order of this feature.</param>
		private void AddFeature(Hashtable Features, View view, string parent, Type msiType, Object obj, XmlElement featureElem, int depth, int order)
		{
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
			recFeat.set_StringData(2, parent);
			recFeat.set_StringData(3, Project.ExpandProperties(featureElem.GetAttribute("title")));
			recFeat.set_StringData(4, Project.ExpandProperties(description));
			recFeat.set_StringData(5, Project.ExpandProperties(featureElem.GetAttribute("display")));
			recFeat.set_StringData(6, depth.ToString());
			recFeat.set_StringData(7, Project.ExpandProperties(directory));
			recFeat.set_StringData(8, (attr == null || attr == "") ? "0" : 
				Int32.Parse(Project.ExpandProperties(attr)).ToString());

			view.Modify(MsiViewModify.msiViewModifyInsert, recFeat);
			Features.Add(name, recFeat);

			if (Verbose)
			{
				Log.WriteLine(LogPrefix + "Feature: " + name);
			}

			XmlNodeList childNodes = featureElem.SelectNodes("feature");
			if (childNodes != null)
			{
				int newDepth = depth + 1;
				int newOrder = 1;

				foreach (XmlNode childNode in childNodes)
				{
					AddFeature(Features, view, name, msiType, obj, (XmlElement)childNode, newDepth, order);
					newOrder++;
				}
			}
		}

		/// <summary>
		/// Adds a file record to the Files table.
		/// </summary>
		/// <param name="directories">Array of directories</param>
		/// <param name="fileView">The MSI database view.</param>
		/// <param name="msiType">The MSI Installer type.</param>
		/// <param name="obj">The MSI Installer object.</param>
		/// <param name="componentDir">The directory of this file's component.</param>
		/// <param name="componentName">The name of this file's component.</param>
		/// <param name="componentCount">The index in the number of components of this file's component.</param>
		/// <param name="sequence">The installation sequence number of this file.</param>
		/// <returns>An array of files added by the component specified.</returns>
		private Hashtable AddFiles(Hashtable directories, View fileView, Type msiType, Object obj, string componentDir, string componentName, int componentCount, ref int sequence)
		{
			Hashtable files = new Hashtable();

			string component = componentName;

			object[] componentDirInfo = (object[])directories[componentDir];
			StringBuilder relativePath = new StringBuilder();
			GetRelativePath(directories, componentDir, (string)componentDirInfo[0], (string)componentDirInfo[1], relativePath);

			string basePath = Path.Combine(Project.BaseDirectory, _sourceDir);
			string fullPath = Path.Combine(basePath, relativePath.ToString());

			string[] dirFiles = Directory.GetFiles(fullPath);
			for (int i = 0; i < dirFiles.Length; i++)
			{
				// Insert the File
				Record recFile = (Record)msiType.InvokeMember(
					"CreateRecord", 
					BindingFlags.InvokeMethod, 
					null, obj, new object[] { 8 });
				recFile.set_StringData(2, component);

				string fileName = Path.GetFileName(dirFiles[i]);
				string filePath = Path.Combine(fullPath, fileName);

				StringBuilder newCompName = new StringBuilder();
				newCompName.Append(componentCount.ToString());
				newCompName.Append(fileName);

				recFile.set_StringData(1, newCompName.ToString());

				files.Add(fileName, newCompName.ToString());
			
				if (File.Exists(filePath))
				{
					recFile.set_StringData(3, GetShortFile(filePath) + "|" + fileName);

					FileStream fileStream = null;
					try
					{
						fileStream = File.OpenRead(filePath);
						recFile.set_StringData(4, fileStream.Length.ToString());

						string cabDir = Path.Combine(Project.BaseDirectory, Path.Combine(SourceDirectory, "Temp"));
						if (!Directory.Exists(cabDir))
						{
							Directory.CreateDirectory(cabDir);
						}

						string cabPath = Path.Combine(cabDir, newCompName.ToString());
						
						File.Copy(filePath, cabPath, true);
					}
					catch (Exception)
					{
						Log.WriteLine(LogPrefix + "ERROR: Could not open file " + filePath);
						return null;
					}
				}
				else
				{
					Log.WriteLine(LogPrefix + "ERROR: Could not open file " + filePath);
					return null;
				}

				if (Verbose)
				{
					Log.WriteLine(LogPrefix + "File: " + Path.Combine(Path.Combine(_sourceDir, relativePath.ToString()), fileName));
				}

				recFile.set_StringData(5, null);	// Version
				recFile.set_StringData(6, null);
				recFile.set_StringData(7, "0");
				sequence++;
				recFile.set_StringData(8, sequence.ToString());
				fileView.Modify(MsiViewModify.msiViewModifyInsert, recFile);
			}

			return files;
		}

		/// <summary>
		/// Loads records for the Registry table.
		/// </summary>
		/// <param name="d">The MSI database.</param>
		/// <param name="msiType">The MSI Installer type.</param>
		/// <param name="obj">The MSI Installer object.</param>
		/// <returns>True if successful.</returns>
		private bool LoadRegistry(Database d, Type msiType, Object obj)
		{
			// Create the "Registry" Table
			View regView = d.OpenView(
				"CREATE TABLE `Registry` (" + 
				"`Registry` CHAR(72) NOT NULL, " + 
				"`Root` SHORT NOT NULL, " + 
				"`Key` CHAR(255) NOT NULL LOCALIZABLE, " + 
				"`Name` CHAR(255) LOCALIZABLE, " + 
				"`Value` CHAR(0) LOCALIZABLE, " + 
				"`Component_` CHAR(72) NOT NULL " + 
				"PRIMARY KEY `Registry`)");
			regView.Execute(null);

			// Re-Open the "Registry" Table
			regView = d.OpenView("SELECT * FROM `Registry`");

			foreach(XmlNode keyNode in _keyNodes)
			{
				XmlElement keyElem = (XmlElement)keyNode;
				string root = keyElem.GetAttribute("root");
				string path = keyElem.GetAttribute("path");

				int rootKey = -1;
				switch (root)
				{
					case "classes":
					{
						rootKey = 0;
						break;
					}
					case "user":
					{
						rootKey = 1;
						break;
					}
					case "machine":
					{
						rootKey = 2;
						break;
					}
					case "users":
					{
						rootKey = 3;
						break;
					}
				}

				string componentName = null;
				XmlNode compNameNode = keyElem.SelectSingleNode("component/@ref");
				if (compNameNode != null)
				{
					componentName = compNameNode.Value;
				}

				if (componentName == null || componentName == "")
				{
					Log.WriteLine(
						LogPrefix + "Error: No component specified for key: " + path);
					return false;
				}

				XmlNodeList values = keyNode.SelectNodes("value");
				if (values != null)
				{
					foreach (XmlNode valueNode in values)
					{
						XmlElement valueElem = (XmlElement)valueNode;

						// Insert the Value
						Record recVal = (Record)msiType.InvokeMember(
							"CreateRecord", 
							BindingFlags.InvokeMethod, 
							null, obj, new object[] { 6 });
						recVal.set_StringData(1, Guid.NewGuid().ToString().ToUpper());
						recVal.set_StringData(2, rootKey.ToString());
						recVal.set_StringData(3, path);
						recVal.set_StringData(4, valueElem.GetAttribute("name"));

						string sValue = valueElem.GetAttribute("value");
						string sDword = valueElem.GetAttribute("dword");

						if (sValue != null & sValue != "")
						{
							recVal.set_StringData(5, sValue);
						}
						else if (sDword != null && sDword != "")
						{
							string sDwordMsi = "#" + Int32.Parse(sDword);
							recVal.set_StringData(5, sDwordMsi);
						}
						else
						{
							string val1 = valueElem.InnerText.Replace(",", null);
							string val2 = val1.Replace(" ", null);
							string val3 = val2.Replace("\n", null);
							string val4 = val3.Replace("\r", null);
							recVal.set_StringData(5, val4);
						}

						recVal.set_StringData(6, componentName);

						regView.Modify(MsiViewModify.msiViewModifyInsert, recVal);
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Creates a .cab file with all source files included.
		/// </summary>
		/// <param name="d">The MSI database.</param>
		/// <param name="msiType">The MSI Installer type.</param>
		/// <param name="obj">The MSI Installer object.</param>
		/// <returns>True if successful.</returns>
		private bool CreateCabFile(Database d, Type msiType, Object obj)
		{
			Log.Write(LogPrefix + "Compressing Files...");

			// Create the CabFile
			ProcessStartInfo processInfo = new ProcessStartInfo();
			
			processInfo.Arguments = "-p -r -P " + 
				Path.Combine(SourceDirectory, "Temp") + @"\ N " + 
				SourceDirectory + @"\" + 
				Path.GetFileNameWithoutExtension(Output) + @".cab " + 
				Path.Combine(SourceDirectory, "Temp") + @"\*";

			processInfo.CreateNoWindow = false;
			processInfo.WindowStyle = ProcessWindowStyle.Hidden;
			processInfo.WorkingDirectory = Output;
			processInfo.FileName = "cabarc";

			Process process = new Process();
			process.StartInfo = processInfo;
			process.EnableRaisingEvents = true;
			process.Start();

			try
			{
				process.WaitForExit();
			}
			catch (Exception e)
			{
				Log.WriteLine();
				Log.WriteLine("Error creating cab file: " + e.Message);
				return false;
			}

			if (process.ExitCode != 0)
			{
				Log.WriteLine();
				Log.WriteLine("Error creating cab file, application returned error " + 
					process.ExitCode + ".");
				return false;
			}

			Log.WriteLine("Done.");
			
			string cabFile = Path.Combine(Project.BaseDirectory, Path.Combine(SourceDirectory, Path.GetFileNameWithoutExtension(Output) + @".cab"));
			if (File.Exists(cabFile))
			{
				View cabView = d.OpenView("SELECT * FROM `_Streams`");
				if (Verbose)
				{
					Log.WriteLine();
					Log.WriteLine(LogPrefix + "Storing Cabinet in MSI Database...");
				}

				Record cabRecord = (Record)msiType.InvokeMember(
					"CreateRecord", 
					BindingFlags.InvokeMethod, 
					null, obj, new object[] { 2 });

				cabRecord.set_StringData(1, Path.GetFileName(cabFile));
				cabRecord.SetStream(2, cabFile);

				cabView.Modify(MsiViewModify.msiViewModifyInsert, cabRecord);
				cabView.Close();
			}
			else
			{
				Log.WriteLine(LogPrefix + "Error: Unable to open Cabinet file:\n\n\t" + cabFile + "\n\n");
				return false;
			}

			Log.Write(LogPrefix + "Deleting Temporary Files...");

			File.Delete(cabFile);
			Directory.Delete(
				Path.Combine(Project.BaseDirectory, 
				Path.Combine(SourceDirectory, @"Temp")), true);

			Log.WriteLine("Done.");

			return true;
		}

		[DllImport("kernel32.dll", CharSet=CharSet.Auto)]
		private static extern int GetShortPathName(string LongPath, StringBuilder ShortPath, int BufferSize); 
		
		/// <summary>
		/// Retrieves a DOS 8.3 filename for a file.
		/// </summary>
		/// <param name="LongFile">The file to shorten.</param>
		/// <returns>The new shortened file.</returns>
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

		/// <summary>
		/// Retrieves a DOS 8.3 filename for a directory.
		/// </summary>
		/// <param name="LongPath">The path to shorten.</param>
		/// <returns>The new shortened path.</returns>
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

		/// <summary>
		/// Retrieves the relative path of a file based on 
		/// the component it belongs to and its entry in 
		/// the MSI directory table.
		/// </summary>
		/// <param name="directories">Array of directory information</param>
		/// <param name="Name">The Name of the Folder</param>
		/// <param name="Parent">The Parent of the Folder</param>
		/// <param name="Default">The Relative Filesystem Path of the Folder</param>
		/// <param name="Path">The Path to the Folder from previous calls.</param>
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

		/// <summary>
		/// Recursively expands properties of all attributes of 
		/// a nodelist and their children.
		/// </summary>
		/// <param name="nodes">The nodes to recurse.</param>
		void ExpandPropertiesInNodes(XmlNodeList nodes) 
		{
			foreach (XmlNode node in nodes)
			{
				if (node.ChildNodes != null)
				{
					ExpandPropertiesInNodes(node.ChildNodes);
					if (node.Attributes != null)
					{
						foreach (XmlAttribute attr in node.Attributes) 
						{
							attr.Value = Project.ExpandProperties(attr.Value);
						}
					}
				}
			}
		}
	}
}