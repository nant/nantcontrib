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
//
// 8/23/2002 - RegLocator/AppSearch Added (jgeurts@sourceforge.net)
//

using System;
using System.IO;
using System.Xml;
using System.Text;
using Microsoft.Win32;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using SourceForge.NAnt;
using SourceForge.NAnt.Tasks;
using SourceForge.NAnt.Attributes;

using NAnt.Contrib.Schemas.MSI;
using WindowsInstaller;
using MsmMergeTypeLib;

namespace NAnt.Contrib.Tasks
{
    /// <summary>
    /// Builds a Windows Installer (MSI) File.
    /// </summary>
    /// <remarks>None.</remarks>
    [TaskName("msi")]
    [SchemaValidator(typeof(msi))]
    public class MSITask : SchemaValidatedTask
    {
        XmlNodeList _componentNodes;
        XmlNodeList _keyNodes;
        XmlNodeList _mergeModules;
        XmlNodeList _envVariables;
        XmlNodeList _searchNodes;

        Hashtable files = new Hashtable();
        Hashtable featureComponents = new Hashtable();
        Hashtable components = new Hashtable();
        ArrayList typeLibRecords = new ArrayList();
        Hashtable typeLibComponents = new Hashtable();

        msi msi;

        string[] commonFolderNames = new string[]
        {
	        "AdminToolsFolder", "AppDataFolder",
	        "CommonAppDataFolder", "CommonFiles64Folder",
	        "CommonFilesFolder", "DesktopFolder",
	        "FavoritesFolder", "FontsFolder",
	        "LocalAppDataFolder", "MyPicturesFolder",
	        "PersonalFolder", "ProgramFiles64Folder",
	        "ProgramFilesFolder", "ProgramMenuFolder",
	        "SendToFolder", "StartMenuFolder",
	        "StartupFolder", "System16Folder",
	        "System64Folder", "SystemFolder",
	        "TempFolder", "TemplateFolder",
	        "WindowsFolder", "WindowsVolume"
        };

        /// <summary>
        /// Initialize taks and verify parameters.
        /// </summary>
        /// <param name="TaskNode">Node that contains the XML fragment 
        /// used to define this task instance.</param>
        /// <remarks>None.</remarks>
        protected override void InitializeTask(XmlNode TaskNode)
        {
            try
            {
                base.InitializeTask(TaskNode);
            }
            catch (SchemaValidationException sve)
            {
                Log.WriteLine(LogPrefix + "ERROR: " + sve.Message);
            }

            msi = (msi)SchemaObject;

            _componentNodes = TaskNode.Clone().SelectNodes("components/component");
            ExpandPropertiesInNodes(_componentNodes);

            _searchNodes = TaskNode.Clone().SelectNodes("search/key");
            ExpandPropertiesInNodes(_searchNodes);

            _keyNodes = TaskNode.Clone().SelectNodes("registry/key");
            ExpandPropertiesInNodes(_keyNodes);

            _mergeModules = TaskNode.Clone().SelectNodes("mergemodules/merge");
            ExpandPropertiesInNodes(_mergeModules);

            _envVariables = TaskNode.Clone().SelectNodes("environment/variable");
            ExpandPropertiesInNodes(_envVariables);
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
        	
            string source = Path.GetDirectoryName(
                tasksModule.FullyQualifiedName) + "\\MSITaskTemplate.msi";

            string dest = Path.Combine(Project.BaseDirectory, 
                Path.Combine(msi.sourcedir, msi.output));

            string errors = Path.GetDirectoryName(
                tasksModule.FullyQualifiedName) + "\\MSITaskErrors.mst";

            string tempPath = Path.Combine(Project.BaseDirectory, 
                Path.Combine(msi.sourcedir, @"Temp"));

            string cabFile = Path.Combine(Project.BaseDirectory, 
                Path.Combine(msi.sourcedir, 
                Path.GetFileNameWithoutExtension(msi.output) + @".cab"));

            CleanOutput(cabFile, tempPath);

            // Copy the Template MSI File
            try
            {
                File.Copy(source, dest, true);
            }
            catch (IOException)
            {
                Log.WriteLine(LogPrefix + 
                    "ERROR: file in use or cannot be copied to output.");
                return;
            }

            try
            {
                // Open the Output Database.
                Database d = null;
                try
                {
                    d = (Database)msiType.InvokeMember(
                        "OpenDatabase", 
                        BindingFlags.InvokeMethod, 
                        null, obj, 
                        new Object[]
                        {
                            dest, 
                            MsiOpenDatabaseMode.msiOpenDatabaseModeDirect
                        });

                    if (msi.debug)
                    {
                        // If Debug is true, transform the error strings in
                        d.ApplyTransform(errors, MsiTransformError.msiTransformErrorNone);
                    }
                }
                catch (Exception e)
                {
                    Log.WriteLine(LogPrefix + "ERROR: " + e.Message);
                    CleanOutput(cabFile, tempPath);

                    return;
                }

                Log.WriteLine(LogPrefix + "Building MSI Database \"" + msi.output + "\".");

                // Load the Banner Image
                if (!LoadBanner(d))
                {
                    CleanOutput(cabFile, tempPath);
                    return;
                }

                // Load the Background Image
                if (!LoadBackground(d))
                {
                    CleanOutput(cabFile, tempPath);
                    return;
                }

                // Load the License File
                if (!LoadLicense(d))
                {
                    CleanOutput(cabFile, tempPath);
                    return;
                }

                // Load Properties
                if (!LoadProperties(d, msiType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    return;
                }

                // Load Registry Locators
                if (!LoadRegLocator(d, msiType, obj))
                {
                    return;
                }

                // Load Application Search
                if (!LoadAppSearch(d, msiType, obj))
                {
                    return;
                }

                View directoryView, asmView, asmNameView, classView, progIdView;

                // Load Directories
                if (!LoadDirectories(d, msiType, obj, out directoryView))
                {
                    CleanOutput(cabFile, tempPath);
                    return;
                }

                // Load Assemblies
                if (!LoadAssemblies(d, msiType, obj, out asmView, 
                    out asmNameView, out classView, out progIdView))
                {
                    CleanOutput(cabFile, tempPath);
                    return;
                }

                int lastSequence = 0;

                // Load Components
                if (!LoadComponents(d, msiType, obj, ref lastSequence, 
                    asmView, asmNameView, directoryView, classView, progIdView))
                {
                    CleanOutput(cabFile, tempPath);
                    return;
                }

                // Load Features
                if (!LoadFeatures(d, msiType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    return;
                }

                View registryView;

                // Load the Registry
                if (!LoadRegistry(d, msiType, obj, out registryView))
                {
                    CleanOutput(cabFile, tempPath);
                    return;
                }

                // Load TypeLibs
                if (!LoadTypeLibs(d, msiType, obj, registryView))
                {
                    CleanOutput(cabFile, tempPath);
                    return;
                }

                // Load Summary Information
                if (!LoadSummaryInfo(d))
                {
                    CleanOutput(cabFile, tempPath);
                    return;
                }

                // Load Environment Variables
                if (!LoadEnvironment(d, msiType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    return;
                }

                try
                {
                    directoryView.Close();
                    registryView.Close();
                    asmView.Close();
                    asmNameView.Close();
                    classView.Close();
                    progIdView.Close();

                    directoryView = null;
                    registryView = null;
                    asmView = null;
                    asmNameView = null;
                    classView = null;
                    progIdView = null;

                    // Commit the MSI Database
                    d.Commit();

                    d = null;

                    GC.Collect();
                }
                catch (Exception e)
                {
                    Log.WriteLine(LogPrefix + "ERROR: " + e.Message);
                    CleanOutput(cabFile, tempPath);
                    return;
                }

                // Load Merge Modules
                if (!LoadMergeModules(dest, tempPath))
                {
                    CleanOutput(cabFile, tempPath);
                    return;
                }

                try
                {
                    d = (Database)msiType.InvokeMember(
                        "OpenDatabase", 
                        BindingFlags.InvokeMethod, 
                        null, obj, 
                        new Object[]
                        {
                            dest, 
                            MsiOpenDatabaseMode.msiOpenDatabaseModeDirect
                        });
                }
                catch (Exception e)
                {
                    Log.WriteLine(LogPrefix + "ERROR: " + e.Message);
                    CleanOutput(cabFile, tempPath);
                    return;
                }

                // Reorder Files
                if (!ReorderFiles(d, ref lastSequence))
                {
                    CleanOutput(cabFile, tempPath);
                    return;
                }

                // Load Media
                if (!LoadMedia(d, msiType, obj, lastSequence))
                {
                    CleanOutput(cabFile, tempPath);
                    return;
                }

                // Compress Files
                if (!CreateCabFile(d, msiType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    return;
                }

                Log.Write(LogPrefix + "Deleting Temporary Files...");
                CleanOutput(cabFile, tempPath);
                Log.WriteLine("Done.");

                try
                {
                    Log.Write(LogPrefix + "Saving MSI Database...");

                    // Commit the MSI Database
                    d.Commit();
                }
                catch (Exception e)
                {
                    Log.WriteLine(LogPrefix + "ERROR: " + e.Message);
                    return;
                }
                Log.WriteLine("Done.");
            }
            catch (Exception e)
            {
                CleanOutput(cabFile, tempPath);
                Log.WriteLine(LogPrefix + "ERROR: " + 
                    e.GetType().FullName + " thrown:\n" + 
                    e.Message + "\n" + e.StackTrace);
            }
        }

        /// <summary>
        /// Cleans the output directory after a build.
        /// </summary>
        /// <param name="cabFile">The path to the cabinet file.</param>
        /// <param name="tempPath">The path to temporary files.</param>
        private void CleanOutput(string cabFile, string tempPath)
        {
            try
            {
                File.Delete(cabFile);
            }
            catch (Exception) {}
            try
            {
                Directory.Delete(tempPath, true);
            }
            catch (Exception) {}
        }

        /// <summary>
        /// Loads the banner iamge.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <returns>True if successful.</returns>
        private bool LoadBanner(Database Database)
        {
	        // Try to open the Banner
	        if (msi.banner != null)
	        {
		        string bannerFile = Path.Combine(Project.BaseDirectory, msi.banner);
		        if (File.Exists(bannerFile))
		        {
			        View bannerView = Database.OpenView("SELECT * FROM `Binary` WHERE `Name`='bannrbmp'");
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
				        "ERROR: Unable to open Banner Image:\n\n\t" + 
				        bannerFile + "\n\n");
			        return false;
		        }
	        }
	        return true;
        }

        /// <summary>
        /// Loads the background image.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <returns>True if successful.</returns>
        private bool LoadBackground(Database Database)
        {
	        // Try to open the Background
	        if (msi.background != null)
	        {
		        string bgFile = Path.Combine(Project.BaseDirectory, msi.background);
		        if (File.Exists(bgFile))
		        {
			        View bgView = Database.OpenView("SELECT * FROM `Binary` WHERE `Name`='dlgbmp'");
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
			        Log.WriteLine(LogPrefix + 
				        "ERROR: Unable to open Background Image:\n\n\t" + 
				        bgFile + "\n\n");
			        return false;
		        }
	        }
	        return true;
        }

        /// <summary>
        /// Loads the license file.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <returns>True if successful.</returns>
        private bool LoadLicense(Database Database)
        {
	        // Try to open the License
	        if (msi.license != null)
	        {
		        string licFile = Path.Combine(Project.BaseDirectory, msi.license);
		        if (File.Exists(licFile))
		        {
			        View licView = Database.OpenView("SELECT * FROM `Control` WHERE `Control`='AgreementText'");
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
				        Log.WriteLine(LogPrefix + 
					        "ERROR: Unable to open License File:\n\n\t" + 
					        licFile + "\n\n");
				        return false;
			        }
			        finally
			        {
				        licView.Close();

				        if (licReader != null)
				        {
					        licReader.Close();
				        }
			        }
		        }
		        else
		        {
			        Log.WriteLine(LogPrefix + 
				        "ERROR: Unable to open License File:\n\n\t" + 
				        licFile + "\n\n");
			        return false;
		        }
	        }
	        return true;
        }

        /// <summary>
        /// Loads records for the Properties table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadProperties(Database Database, Type InstallerType, Object InstallerObject)
        {
	        // Select the "Property" Table
	        View propView = Database.OpenView("SELECT * FROM `Property`");

	        // Add properties from Task definition
	        foreach (property property in msi.properties)
	        {
		        // Insert the Property
		        Record recProp = (Record)InstallerType.InvokeMember(
			        "CreateRecord", 
			        BindingFlags.InvokeMethod, 
			        null, InstallerObject, 
			        new object[] { 2 });

		        string name = property.name;
		        string sValue = property.value;

		        if (name == null || name == "")
		        {
			        Log.WriteLine(LogPrefix + 
				        "ERROR: Property with no name attribute detected.");
			        return false;
		        }

		        if (sValue == null || sValue == "")
		        {
			        Log.WriteLine(LogPrefix + 
				        "ERROR: Property " + name + 
				        " has no value.");
			        return false;
		        }

		        recProp.set_StringData(1, name);
		        recProp.set_StringData(2, sValue);
		        propView.Modify(MsiViewModify.msiViewModifyInsert, recProp);

		        if (Verbose)
		        {
			        Log.WriteLine(LogPrefix + "Setting Property: " + name);
		        }

                propView.Close();
	        }
	        return true;
        }

        /// <summary>
        /// Loads records for the Components table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <param name="LastSequence">The sequence number of the last file in the .cab</param>
        /// <param name="MsiAssemblyView">View containing the MsiAssembly table.</param>
        /// <param name="MsiAssemblyNameView">View containing the MsiAssemblyName table.</param>
        /// <param name="DirectoryView">The MSI database view.</param>
        /// <param name="ClassView">View containing the Class table.</param>
        /// <param name="ProgIdView">View containing the ProgId table.</param>
        /// <returns>True if successful.</returns>
        private bool LoadComponents(Database Database, Type InstallerType, Object InstallerObject, 
	        ref int LastSequence, View MsiAssemblyView, View MsiAssemblyNameView, 
	        View DirectoryView, View ClassView, View ProgIdView)
        {
	        // Open the "Component" Table
	        View compView = Database.OpenView("SELECT * FROM `Component`");

	        // Open the "File" Table
	        View fileView = Database.OpenView("SELECT * FROM `File`");

	        // Open the "FeatureComponents" Table
	        View featCompView = Database.OpenView("SELECT * FROM `FeatureComponents`");

	        // Open the "SelfReg" Table
	        View selfRegView = Database.OpenView("SELECT * FROM `SelfReg`");

	        // Add components from Task definition
	        int componentIndex = 0;
	        foreach (XmlNode compNode in _componentNodes)
	        {
		        XmlElement compElem = (XmlElement)compNode;

		        string name = compElem.GetAttribute("name");
		        if (name == null || name == "")
		        {
			        Log.WriteLine(LogPrefix + 
				        "ERROR: Component with no name attribute detected.");
			        return false;
		        }

		        string attr = compElem.GetAttribute("attr");

		        string directory = null;
		        XmlNode dirNode = compElem.SelectSingleNode("directory/@ref");
		        if (dirNode != null)
		        {
			        directory = dirNode.Value;
		        }

		        if (directory == null || directory == "")
		        {
			        Log.WriteLine(LogPrefix + 
				        "ERROR: Component " + name + 
				        " needs to specify a directory.");
			        return false;
		        }

		        string keyFile = null;
		        XmlNode keyFileNode = compElem.SelectSingleNode("key/@file");
		        if (keyFileNode != null)
		        {
			        keyFile = keyFileNode.Value;
		        }
		        if (keyFile == null || keyFile == "")
		        {
			        Log.WriteLine(
				        LogPrefix + "ERROR: Component " + name + 
				        " needs to specify a key.");
			        return false;
		        }

		        string id = compElem.GetAttribute("id");
		        if (id == null)
		        {
			        Log.WriteLine(
				        LogPrefix + "ERROR: Component " + name + 
				        " needs to specify an id.");
			        return false;
		        }

		        // Insert the Component
		        Record recComp = (Record)InstallerType.InvokeMember(
			        "CreateRecord", 
			        BindingFlags.InvokeMethod, 
			        null, InstallerObject, 
			        new object[] { 6 });

		        recComp.set_StringData(1, name);
		        recComp.set_StringData(2, id);
		        recComp.set_StringData(3, directory);
        		
		        recComp.set_StringData(4, 
			        (attr == null || attr == "") ? "0" : 
			        Int32.Parse(attr).ToString());

		        recComp.set_StringData(5, compElem.GetAttribute("condition"));

		        if (Verbose)
		        {
			        Log.WriteLine(LogPrefix + "Component: " + name);
		        }

		        components.Add(name, directory);

		        XmlNodeList featureComponentNodes = compElem.SelectNodes("feature");
		        foreach (XmlNode featureComponentNode in featureComponentNodes)
		        {
			        XmlElement featureComponentElem = (XmlElement)featureComponentNode;
			        featureComponents.Add(name, featureComponentElem.GetAttribute("ref"));
		        }

		        componentIndex++;

		        bool success = AddFiles(Database, DirectoryView, compElem, 
                    fileView, InstallerType, InstallerObject, 
			        directory, name, ref componentIndex, 
			        ref LastSequence, MsiAssemblyView, MsiAssemblyNameView, 
			        compView, featCompView, ClassView, ProgIdView, selfRegView);

		        if (!success)
		        {
			        return success;
		        }

		        if (files.Contains(directory + "|" + keyFile))
		        {
			        string keyFileName = (string)files[directory + "|" + keyFile];
			        if (keyFileName == "KeyIsDotNetAssembly")
			        {
				        Log.WriteLine(LogPrefix + "ERROR: Cannot specify key '" + keyFile + 
					        "' for component '" + name + "'. File has been detected as " + 
					        "being a COM component or Microsoft.NET assembly and is " + 
					        "being registered with its own component. Please specify " + 
					        "a different file in the same directory for this component's key.");
				        return false;
			        }
			        else
			        {
				        recComp.set_StringData(6, keyFileName);
				        compView.Modify(MsiViewModify.msiViewModifyInsert, recComp);
			        }
		        }
		        else
		        {
			        Log.WriteLine(
				        LogPrefix + "ERROR: KeyFile \"" + keyFile + 
				        "\" not found in Component \"" + name + "\".");
			        return false;
		        }
	        }

	        // Add featureComponents from Task definition
	        IEnumerator keyEnum = featureComponents.Keys.GetEnumerator();
	        while (keyEnum.MoveNext())
	        {
		        string component = Project.ExpandProperties((string)keyEnum.Current);
		        string feature = Project.ExpandProperties((string)featureComponents[component]);

		        if (feature == null)
		        {
			        Log.WriteLine(LogPrefix + 
				        "ERROR: Component " + component + 
				        " mapped to nonexistent feature.");
			        return false;
		        }
        		
		        // Insert the FeatureComponent
		        Record recFeatComps = (Record)InstallerType.InvokeMember(
			        "CreateRecord", 
			        BindingFlags.InvokeMethod, 
			        null, InstallerObject, 
			        new object[] { 2 });

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

            compView.Close();
            fileView.Close();
            featCompView.Close();
            selfRegView.Close();

	        return true;
        }

        /// <summary>
        /// Loads records for the Directories table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <param name="DirectoryView">The MSI database view.</param>
        /// <returns>True if successful.</returns>
        private bool LoadDirectories(Database Database, Type InstallerType, 
            Object InstallerObject, out View DirectoryView)
        {
	        // Open the "Directory" Table
	        DirectoryView = Database.OpenView("SELECT * FROM `Directory`");

            ArrayList directoryList = new ArrayList(msi.directories);       

            MSIRootDirectory targetDir = new MSIRootDirectory();
            targetDir.name = "TARGETDIR";
            targetDir.root = "";
            targetDir.foldername = "SourceDir";
            directoryList.Add(targetDir);

	        // Insert the Common Directories
	        for (int i = 0; i < commonFolderNames.Length; i++)
	        {
                MSIRootDirectory commonDir = new MSIRootDirectory();
                commonDir.name = commonFolderNames[i];
                commonDir.root = "TARGETDIR";
                commonDir.foldername = ".";
		        directoryList.Add(commonDir);
	        }

            MSIRootDirectory[] directories = new MSIRootDirectory[directoryList.Count];
            directoryList.CopyTo(directories);
            msi.directories = directories;

	        int depth = 1;

	        // Add directories from Task definition
            foreach (MSIRootDirectory directory in msi.directories)
            {
		        bool result = AddDirectory(Database, 
			        DirectoryView, null, InstallerType, 
			        InstallerObject, directory, depth);

		        if (!result)
		        {
                    DirectoryView.Close();
			        return result;
		        }
	        }

	        return true;
        }

        /// <summary>
        /// Adds a directory record to the directories table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="DirectoryView">The MSI database view.</param>
        /// <param name="ParentDirectory">The parent directory.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <param name="Directory">This directory's Schema object.</param>
        /// <param name="Depth">The tree depth of this directory.</param>
        /// <returns></returns>
        private bool AddDirectory(Database Database, View DirectoryView, 
            string ParentDirectory, 
	        Type InstallerType, object InstallerObject, 
	        MSIDirectory Directory, int Depth)
        {
	        string newParent = ParentDirectory;
            if (Directory is MSIRootDirectory)
            {
                newParent = ((MSIRootDirectory)Directory).root;
            }

	        // Insert the Directory
	        Record recDir = (Record)InstallerType.InvokeMember(
		        "CreateRecord", 
		        BindingFlags.InvokeMethod, 
		        null, InstallerObject, new object[] { 3 });

	        recDir.set_StringData(1, Directory.name);
	        recDir.set_StringData(2, newParent);
        	
	        StringBuilder relativePath = new StringBuilder();

	        GetRelativePath(Database, InstallerType, InstallerObject, 
                Directory.name, ParentDirectory, Directory.foldername, 
                relativePath, DirectoryView);

	        string basePath = Path.Combine(Project.BaseDirectory, msi.sourcedir);
	        string fullPath = Path.Combine(basePath, relativePath.ToString());
	        string path = GetShortPath(fullPath) + "|" + Directory.foldername;

            if (relativePath.ToString() == "")
            {
                return true;
            }

            if (path == "MsiTaskPathNotFound")
            {
                return false;
            }

	        if (Verbose)
	        {
		        Log.WriteLine(LogPrefix + "Directory: " + 
			        Path.Combine(Project.BaseDirectory, relativePath.ToString()));
	        }
        	
	        recDir.set_StringData(3, path);

	        DirectoryView.Modify(MsiViewModify.msiViewModifyInsert, recDir);

            if (Directory.directory != null)
            {
                foreach (MSIDirectory childDirectory in Directory.directory)
                {
                    int newDepth = Depth + 1;

                    bool result = AddDirectory(Database, DirectoryView, 
                        Directory.name, InstallerType, 
                        InstallerObject, childDirectory, newDepth);

                    if (!result)
                    {
                        return result;
                    }
                }
            }
	        return true;
        }

        /// <summary>
        /// Loads records for the Media table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <param name="LastSequence">The sequence number of the last file in the .cab.</param>
        /// <returns>True if successful.</returns>
        private bool LoadMedia(Database Database, Type InstallerType, Object InstallerObject, int LastSequence)
        {
	        // Open the "Media" Table
	        View mediaView = Database.OpenView("SELECT * FROM `Media`");

	        // Insert the Disk
	        Record recMedia = (Record)InstallerType.InvokeMember(
		        "CreateRecord", 
		        BindingFlags.InvokeMethod, 
		        null, InstallerObject, 
		        new object[] { 6 });

	        recMedia.set_StringData(1, "1");
	        recMedia.set_StringData(2, LastSequence.ToString());
	        recMedia.set_StringData(4, "#" + Path.GetFileNameWithoutExtension(msi.output) + ".cab");
	        mediaView.Modify(MsiViewModify.msiViewModifyInsert, recMedia);

	        mediaView.Close();

	        return true;
        }

        /// <summary>
        /// Loads properties for the Summary Information Stream.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <returns>True if successful.</returns>
        private bool LoadSummaryInfo(Database Database)
        {
            property productName = null;
            property manufacturer = null;
            property keywords = null;
            property comments = null;

            foreach (property prop in msi.properties)
            {
                if (prop.name == "ProductName")
                {
                    productName = prop;
                }
                else if (prop.name == "Manufacturer")
                {
                    manufacturer = prop;
                }
                else if (prop.name == "Keywords")
                {
                    keywords = prop;
                }
                else if (prop.name == "Comments")
                {
                    comments = prop;
                }
            }

	        SummaryInfo summaryInfo = Database.get_SummaryInformation(200);
	        summaryInfo.set_Property(2, productName.value);
	        summaryInfo.set_Property(3, productName.value);
	        summaryInfo.set_Property(4, manufacturer.value);

            if (keywords != null)
            {
                summaryInfo.set_Property(5, keywords.value);
            }
            if (comments != null)
            {
                summaryInfo.set_Property(6, comments.value);
            }

	        summaryInfo.set_Property(9, "{"+Guid.NewGuid().ToString().ToUpper()+"}");
	        summaryInfo.set_Property(14, 200);
	        summaryInfo.set_Property(15, 2);
        	
	        summaryInfo.Persist();

	        return true;
        }

        /// <summary>
        /// Loads environment variables for the Environment table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadEnvironment(Database Database, Type InstallerType, Object InstallerObject)
        {
            // Open the "Environment" Table
            View envView = Database.OpenView("SELECT * FROM `Environment`");

            foreach (XmlNode varNode in _envVariables)
            {
                XmlElement varElem = (XmlElement)varNode;

                string varName = varElem.GetAttribute("name");

                XmlElement compElem = (XmlElement)varElem.SelectSingleNode("component");
                if (compElem == null)
                {
                    Log.WriteLine(LogPrefix + 
                        "ERROR: Environment variable " + 
                        varName + " must specify a component");
                    return false;
                }

                string varComp = compElem.GetAttribute("ref");

                // Insert the Varible
                Record recVar = (Record)InstallerType.InvokeMember(
                    "CreateRecord", 
                    BindingFlags.InvokeMethod, 
                    null, InstallerObject, 
                    new object[] { 4 });

                recVar.set_StringData(1, "_" + Guid.NewGuid().ToString().ToUpper().Replace("-", null));
                recVar.set_StringData(2, varName);

                string appendStr = varElem.GetAttribute("append");
                if (appendStr != null && appendStr != "")
                {
                    recVar.set_StringData(3, "[~];" + appendStr);
                }

                recVar.set_StringData(4, varComp);

                envView.Modify(MsiViewModify.msiViewModifyInsert, recVar);
            }
            envView.Close();

            return true;
        }

        /// <summary>
        /// Loads records for the Features table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadFeatures(Database Database, Type InstallerType, Object InstallerObject)
        {
	        // Open the "Feature" Table
	        View featView = Database.OpenView("SELECT * FROM `Feature`");

	        // Add features from Task definition
	        int order = 1;
	        int depth = 1;
        	
            foreach (MSIFeature feature in msi.features)
            {
		        bool result = AddFeature(featView, null, InstallerType, 
			        InstallerObject, feature, depth, order);

		        if (!result)
		        {
                    featView.Close();
			        return result;
		        }
		        order++;
	        }

            featView.Close();
	        return true;
        }

        /// <summary>
        /// Adds a feature record to the Features table.
        /// </summary>
        /// <param name="FeatureView">The MSI database view.</param>
        /// <param name="ParentFeature">The name of this feature's parent.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI INstaller object.</param>
        /// <param name="Feature">This Feature's Schema element.</param>
        /// <param name="Depth">The tree depth of this feature.</param>
        /// <param name="Order">The tree order of this feature.</param>
        private bool AddFeature(View FeatureView, string ParentFeature, 
	        Type InstallerType, Object InstallerObject, 
	        MSIFeature Feature, int Depth, int Order)
        {
	        string directory = null;	        
	        if (Feature.directory != null)
	        {
		        directory = Feature.directory.name;
	        }
	        else
	        {
		        bool foundComponent = false;
        		
		        IEnumerator featComps = featureComponents.Keys.GetEnumerator();
        		
		        while (featComps.MoveNext())
		        {
			        string componentName = (string)featComps.Current;
			        string featureName = (string)featureComponents[componentName];

			        if (featureName == Feature.name)
			        {
				        directory = (string)components[componentName];
				        foundComponent = true;
			        }
		        }

		        if (!foundComponent)
		        {
			        Log.WriteLine(
				        LogPrefix + "ERROR: Feature " + Feature.name + 
				        " needs to be assigned a component or directory.");
			        return false;
		        }
	        }

	        // Insert the Feature
	        Record recFeat = (Record)InstallerType.InvokeMember(
		        "CreateRecord", 
		        BindingFlags.InvokeMethod, 
		        null, InstallerObject, 
		        new object[] { 8 });

	        recFeat.set_StringData(1, Feature.name);
	        recFeat.set_StringData(2, ParentFeature);
	        recFeat.set_StringData(3, Feature.title);
	        recFeat.set_StringData(4, Feature.description);
	        recFeat.set_IntegerData(5, Feature.display);

            if (!Feature.typical)
            {
                recFeat.set_StringData(6, "4");
            }
            else
            {
                recFeat.set_StringData(6, "3");
            }
        	
	        recFeat.set_StringData(7, directory);
	        recFeat.set_IntegerData(8, Feature.attr);

	        FeatureView.Modify(MsiViewModify.msiViewModifyInsert, recFeat);

	        if (Verbose)
	        {
		        Log.WriteLine(LogPrefix + "Feature: " + Feature.name);
	        }

            if (Feature.feature != null)
            {
                foreach (MSIFeature childFeature in Feature.feature)
                {
                    int newDepth = Depth + 1;
                    int newOrder = 1;

                    bool result = AddFeature(FeatureView, Feature.name, InstallerType, 
                        InstallerObject, childFeature, newDepth, newOrder);

                    if (!result)
                    {
                        return result;
                    }
                    newOrder++;
                }
            }
	        return true;
        }

        [DllImport("kernel32")]
        private extern static int LoadLibrary(string lpLibFileName);
        [DllImport("kernel32")]
        private extern static bool FreeLibrary(int hLibModule);
        [DllImport("kernel32", CharSet=CharSet.Ansi)]
        private extern static int GetProcAddress(int hModule, string lpProcName);

        /// <summary>
        /// Adds a file record to the Files table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="DirectoryView">The MSI database view.</param>
        /// <param name="ComponentElem">The Component's XML Element.</param>
        /// <param name="FileView">The MSI database view.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <param name="ComponentDirectory">The directory of this file's component.</param>
        /// <param name="ComponentName">The name of this file's component.</param>
        /// <param name="ComponentCount">The index in the number of components of this file's component.</param>
        /// <param name="Sequence">The installation sequence number of this file.</param>
        /// <param name="MsiAssemblyView">View containing the MsiAssembly table.</param>
        /// <param name="MsiAssemblyNameView">View containing the MsiAssemblyName table.</param>
        /// <param name="ComponentView">View containing the Components table.</param>
        /// <param name="FeatureComponentView">View containing the FeatureComponents table.</param>
        /// <param name="ClassView">View containing the Class table.</param>
        /// <param name="ProgIdView">View containing the ProgId table.</param>
        /// <param name="SelfRegView">View containing the SelfReg table.</param>
        /// <returns>True if successful.</returns>
        private bool AddFiles(Database Database, View DirectoryView, XmlElement ComponentElem, 
            View FileView, Type InstallerType, Object InstallerObject, 
	        string ComponentDirectory, string ComponentName, ref int ComponentCount, 
	        ref int Sequence, View MsiAssemblyView, View MsiAssemblyNameView, 
	        View ComponentView, View FeatureComponentView, View ClassView, View ProgIdView, 
	        View SelfRegView)
        {
	        string component = ComponentName;

            XmlElement fileSetElem = (XmlElement)ComponentElem.SelectSingleNode("fileset");
            if (fileSetElem == null)
            {
                Log.WriteLine(LogPrefix + "component is missing a fileset child element.");
                return false;
            }

            FileSet componentFiles = new FileSet();
            componentFiles.Project = Project;
            componentFiles.Initialize(fileSetElem);

            MSIDirectory componentDirInfo = FindDirectory(ComponentDirectory);
        	
	        StringBuilder relativePath = new StringBuilder();

            string newParent = null;
            if (componentDirInfo is MSIRootDirectory)
            {
                newParent = ((MSIRootDirectory)componentDirInfo).root;
            }
            else
            {
                newParent = FindParent(ComponentDirectory);
            }

	        GetRelativePath(Database, InstallerType, 
                InstallerObject, 
                ComponentDirectory, 
		        newParent, 
		        componentDirInfo.foldername, 
		        relativePath, DirectoryView);

	        string basePath = Path.Combine(Project.BaseDirectory, msi.sourcedir);
	        string fullPath = Path.Combine(basePath, relativePath.ToString());

            for (int i = 0; i < componentFiles.FileNames.Count; i++)
            {
		        // Insert the File
		        Record recFile = (Record)InstallerType.InvokeMember(
			        "CreateRecord", 
			        BindingFlags.InvokeMethod, 
			        null, InstallerObject, 
			        new object[] { 8 });

		        string fileName = Path.GetFileName(componentFiles.FileNames[i]);
		        string filePath = Path.Combine(fullPath, fileName);
        		
                XmlElement overrideElem = (XmlElement)ComponentElem.SelectSingleNode(
                    "override[@file='" + fileName +"']");

                string fileId = overrideElem == null ? 
                    "_" + Guid.NewGuid().ToString().ToUpper().Replace("-", null) :
                    overrideElem.GetAttribute("id");

		        files.Add(ComponentDirectory + "|" + fileName, fileId);
		        recFile.set_StringData(1, fileId);
        	
		        if (File.Exists(filePath))
		        {
			        try
			        {
				        recFile.set_StringData(4, new FileInfo(filePath).Length.ToString());
			        }
			        catch (Exception)
			        {
				        Log.WriteLine(LogPrefix + 
					        "ERROR: Could not open file " + filePath);
				        return false;
			        }
		        }
		        else
		        {
			        Log.WriteLine(LogPrefix + 
				        "ERROR: Could not open file " + filePath);
			        return false;
		        }

		        if (Verbose)
		        {
			        Log.WriteLine(LogPrefix + "File: " + 
				        Path.Combine(Path.Combine(msi.sourcedir, 
				        relativePath.ToString()), fileName));
		        }

		        // If the file is an assembly, create a new component to contain it, 
		        // add the new component, map the new component to the old component's 
		        // feature, and create an entry in the MsiAssembly and MsiAssemblyName 
		        // table.
		        //
		        bool isAssembly = false;
		        Assembly fileAssembly = null;
		        try
		        {
			        fileAssembly = Assembly.LoadFrom(filePath);
			        isAssembly = true;
		        }
		        catch (Exception) {}
        		
		        if (isAssembly || filePath.EndsWith(".tlb"))
		        {
			        string feature = (string)featureComponents[ComponentName];
        	
                    string asmCompName = ComponentName;

                    if (componentFiles.FileNames.Count > 1)
                    {
                        asmCompName = "C_" + fileId;
                        string newCompId = "{" + Guid.NewGuid().ToString().ToUpper() + "}";

                        recFile.set_StringData(2, asmCompName);
        			
                        // Add a record for a new Component
                        Record recComp = (Record)InstallerType.InvokeMember(
                            "CreateRecord", 
                            BindingFlags.InvokeMethod, 
                            null, InstallerObject, 
                            new object[] { 6 });

                        recComp.set_StringData(1, asmCompName);
                        recComp.set_StringData(2, newCompId);
                        recComp.set_StringData(3, ComponentDirectory);
                        recComp.set_StringData(4, "2");
                        recComp.set_StringData(5, null);
                        recComp.set_StringData(6, fileId);
                        ComponentView.Modify(MsiViewModify.msiViewModifyInsert, recComp);

                        // Map the new Component to the existing one's Feature
                        Record featComp = (Record)InstallerType.InvokeMember(
                            "CreateRecord", 
                            BindingFlags.InvokeMethod, 
                            null, InstallerObject, 
                            new object[] { 2 });

                        featComp.set_StringData(1, (string)featureComponents[ComponentName]);
                        featComp.set_StringData(2, asmCompName);
                        FeatureComponentView.Modify(MsiViewModify.msiViewModifyInsert, featComp);
                    }

			        if (isAssembly)
			        {
				        // Add a record for a new MsiAssembly
				        Record recAsm = (Record)InstallerType.InvokeMember(
					        "CreateRecord", 
					        BindingFlags.InvokeMethod, 
					        null, InstallerObject, 
					        new object[] { 5 });

				        recAsm.set_StringData(1, asmCompName);
				        recAsm.set_StringData(2, (string)featureComponents[ComponentName]);
				        recAsm.set_StringData(3, fileId);
				        recAsm.set_StringData(4, fileId);
				        recAsm.set_IntegerData(5, 0);
				        MsiAssemblyView.Modify(MsiViewModify.msiViewModifyInsert, recAsm);

				        //
				        // Add records for the Assembly Manifest
				        //

				        AssemblyName asmName = fileAssembly.GetName();

				        string name = asmName.Name;
				        string version = asmName.Version.ToString(4);
        				
				        AssemblyCultureAttribute[] cultureAttrs = 
					        (AssemblyCultureAttribute[])fileAssembly.GetCustomAttributes(
					        typeof(AssemblyCultureAttribute), true);

				        string culture = "neutral";
				        if (cultureAttrs.Length > 0)
				        {
					        culture = cultureAttrs[0].Culture;
				        }

				        string publicKey = null;
				        byte[] keyToken = asmName.GetPublicKeyToken();
				        if (keyToken != null)
				        {
					        publicKey = ByteArrayToString(keyToken);
				        }

				        if (name != null && name != "")
				        {
					        Record recAsmName = (Record)InstallerType.InvokeMember(
						        "CreateRecord", 
						        BindingFlags.InvokeMethod, 
						        null, InstallerObject, 
						        new object[] { 3 });

					        recAsmName.set_StringData(1, asmCompName);
					        recAsmName.set_StringData(2, "Name");
					        recAsmName.set_StringData(3, name);
					        MsiAssemblyNameView.Modify(MsiViewModify.msiViewModifyInsert, recAsmName);
				        }

				        if (version != null && version != "")
				        {
					        Record recAsmVersion = (Record)InstallerType.InvokeMember(
						        "CreateRecord", 
						        BindingFlags.InvokeMethod, 
						        null, InstallerObject, new object[] { 3 });

					        recAsmVersion.set_StringData(1, asmCompName);
					        recAsmVersion.set_StringData(2, "Version");
					        recAsmVersion.set_StringData(3, version);
					        MsiAssemblyNameView.Modify(MsiViewModify.msiViewModifyInsert, recAsmVersion);
				        }

				        if (culture != null && culture != "")
				        {
					        Record recAsmLocale = (Record)InstallerType.InvokeMember(
						        "CreateRecord", 
						        BindingFlags.InvokeMethod, 
						        null, InstallerObject, 
						        new object[] { 3 });

					        recAsmLocale.set_StringData(1, asmCompName);
					        recAsmLocale.set_StringData(2, "Culture");
					        recAsmLocale.set_StringData(3, culture);
					        MsiAssemblyNameView.Modify(MsiViewModify.msiViewModifyInsert, recAsmLocale);
				        }

				        if (publicKey != null && publicKey != "")
				        {
					        Record recPublicKey = (Record)InstallerType.InvokeMember(
						        "CreateRecord", 
						        BindingFlags.InvokeMethod, 
						        null, InstallerObject, 
						        new object[] { 3 });

					        recPublicKey.set_StringData(1, asmCompName);
					        recPublicKey.set_StringData(2, "PublicKeyToken");
					        recPublicKey.set_StringData(3, publicKey);
					        MsiAssemblyNameView.Modify(MsiViewModify.msiViewModifyInsert, recPublicKey);
				        }

				        bool success = CheckAssemblyForCOMInterop(
					        filePath, fileAssembly, InstallerType, 
					        InstallerObject, ComponentName, 
					        asmCompName, ClassView, ProgIdView);

				        if (!success)
				        {
					        return success;
				        }

				        // File cant be a member of both components
                        if (componentFiles.FileNames.Count > 1)
                        {
                            files.Remove(ComponentDirectory + "|" + fileName);
                            files.Add(ComponentDirectory + "|" + fileName, "KeyIsDotNetAssembly");
                        }
			        }
			        else if (filePath.EndsWith(".tlb"))
			        {
				        typeLibComponents.Add(
					        Path.GetFileName(filePath), 
					        asmCompName);
			        }
		        }

		        if (filePath.EndsWith(".dll"))
		        {
			        int hmod = LoadLibrary(filePath);
			        if (hmod != 0)
			        {
				        int regSvr = GetProcAddress(hmod, "DllRegisterServer");
				        if (regSvr != 0)
				        {
					        Log.WriteLine(LogPrefix + 
						        "Configuring " + 
						        Path.GetFileName(filePath) + 
						        " for COM Self Registration...");

					        // Add a record for a new Component
					        Record recSelfReg = (Record)InstallerType.InvokeMember(
						        "CreateRecord", 
						        BindingFlags.InvokeMethod, 
						        null, InstallerObject, 
						        new object[] { 2 });

					        recSelfReg.set_StringData(1, fileId);
					        SelfRegView.Modify(MsiViewModify.msiViewModifyInsert, recSelfReg);
				        }
				        FreeLibrary(hmod);
			        }

			        // Register COM .dlls with an embedded 
			        // type library for self registration.
		        }

		        if (File.Exists(filePath))
		        {
			        string cabDir = Path.Combine(
				        Project.BaseDirectory, 
				        Path.Combine(msi.sourcedir, "Temp"));

			        if (!Directory.Exists(cabDir))
			        {
				        Directory.CreateDirectory(cabDir);
			        }

			        string cabPath = Path.Combine(cabDir, fileId);
			        File.Copy(filePath, cabPath, true);
		        }

		        if (!isAssembly && !filePath.EndsWith(".tlb") 
                    || componentFiles.FileNames.Count == 1)
		        {
			        recFile.set_StringData(2, component);
		        }

		        recFile.set_StringData(3, GetShortFile(filePath) + "|" + fileName);
		        recFile.set_StringData(5, null);	// Version
		        recFile.set_StringData(6, null);
		        recFile.set_StringData(7, "512");
        		
		        Sequence++;
        		
		        recFile.set_StringData(8, Sequence.ToString());
		        FileView.Modify(MsiViewModify.msiViewModifyInsert, recFile);
	        }
	        return true;
        }

        /// <summary>
        /// Loads records for the Registry table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <param name="RegistryView">View containing the Registry table.</param>
        /// <returns>True if successful.</returns>
        private bool LoadRegistry(Database Database, Type InstallerType, 
	        Object InstallerObject, out View RegistryView)
        {
	        // Open the "Registry" Table
	        RegistryView = Database.OpenView("SELECT * FROM `Registry`");

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
				        LogPrefix + "ERROR: No component specified for key: " + path);
			        return false;
		        }

		        XmlNodeList values = keyNode.SelectNodes("value");
		        if (values != null)
		        {
			        foreach (XmlNode valueNode in values)
			        {
				        XmlElement valueElem = (XmlElement)valueNode;

				        // Insert the Value
				        Record recVal = (Record)InstallerType.InvokeMember(
					        "CreateRecord", 
					        BindingFlags.InvokeMethod, 
					        null, InstallerObject, 
					        new object[] { 6 });

				        recVal.set_StringData(1, "_" + 
					        Guid.NewGuid().ToString().ToUpper().Replace("-", null));
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
					        recVal.set_StringData(5, "#x" + val4);
				        }

				        recVal.set_StringData(6, componentName);
				        RegistryView.Modify(MsiViewModify.msiViewModifyInsert, recVal);
			        }
		        }
	        }

	        return true;
        }

        /// <summary>
        /// Creates the assembly and assembly name tables.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <param name="MsiAssemblyView">View containing the MsiAssembly table.</param>
        /// <param name="MsiAssemblyNameView">View containing the MsiAssemblyName table.</param>
        /// <param name="ClassView">View containing the Class table.</param>
        /// <param name="ProgIdView">View containing the ProgId table.</param>
        /// <returns></returns>
        private bool LoadAssemblies(Database Database, Type InstallerType, 
	        Object InstallerObject, out View MsiAssemblyView, 
	        out View MsiAssemblyNameView, out View ClassView, 
	        out View ProgIdView)
        {
	        MsiAssemblyView = Database.OpenView("SELECT * FROM `MsiAssembly`");
	        MsiAssemblyNameView = Database.OpenView("SELECT * FROM `MsiAssemblyName`");
	        ClassView = Database.OpenView("SELECT * FROM `Class`");
	        ProgIdView = Database.OpenView("SELECT * FROM `ProgId`");

	        return true;
        }

        /// <summary>
        /// Loads records for the RegLocator table
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadRegLocator(Database Database, Type InstallerType, 
            Object InstallerObject)
        {
            // Add properties from Task definition
            foreach (XmlNode keyNode in _searchNodes)
            {
                XmlElement keyElem = (XmlElement)keyNode;

                string type = keyElem.GetAttribute("type");

                if (type == null || type == "")
                {
                    Log.WriteLine(LogPrefix + 
                        "ERROR: Search key with no type attribute detected.");
                    return false;
                }
                switch (type)
                {
                    case "registry":
                    {

                        // Select the "RegLocator" Table
                        View regLocatorView = Database.OpenView("SELECT * FROM `RegLocator`");
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

                        if (path == null || path == "")
                        {
                            Log.WriteLine(LogPrefix + 
                                "ERROR: Search key with no path attribute detected.");
                            return false;
                        }

                        XmlNodeList values = keyNode.SelectNodes("value");
                        if (values != null)
                        {
                            foreach (XmlNode valueNode in values)
                            {
                                XmlElement valueElem = (XmlElement)valueNode;

                                string propRef = null;
                                XmlNode propNode = valueElem.SelectSingleNode("@ref");
                                if (propNode != null)
                                {
                                    propRef = propNode.Value;
                                }

                                if (propRef == null || propRef == "")
                                {
                                    Log.WriteLine(LogPrefix + 
                                        "ERROR: Search key with no ref attribute detected.");
                                    return false;
                                }
                                string signature = "SIG_" + propRef;
                                string name = valueElem.GetAttribute("name");
                                if (name == null || name == "")
                                {
                                    Log.WriteLine(LogPrefix + 
                                        "ERROR: Search key with no name attribute detected.");
                                    return false;
                                }


                                // Insert the signature to the RegLocator Table
                                Record recRegLoc = (Record)InstallerType.InvokeMember(
                                    "CreateRecord", 
                                    BindingFlags.InvokeMethod, 
                                    null, InstallerObject, 
                                    new object[] { 5 });

                                recRegLoc.set_StringData(1, signature);
                                recRegLoc.set_StringData(2, rootKey.ToString());
                                recRegLoc.set_StringData(3, path);
                                recRegLoc.set_StringData(4, name);
                                recRegLoc.set_StringData(5, "msidbLocatorTypeRawValue");

                                regLocatorView.Modify(MsiViewModify.msiViewModifyInsert, recRegLoc);

                                if (Verbose)
                                {
                                    Log.WriteLine(LogPrefix + "Setting Registry Location: " + signature);
                                }
                            }
                        }
                        regLocatorView.Close();

                        break;
                    }
                    case "file":
                    {
                        break;
                    }
                }

            }
            return true;
        }

            /// <summary>
            /// Loads records for the RegLocator table
            /// </summary>
            /// <param name="Database">The MSI database.</param>
            /// <param name="InstallerType">The MSI Installer type.</param>
            /// <param name="InstallerObject">The MSI Installer object.</param>
            /// <returns>True if successful.</returns>
            private bool LoadAppSearch(Database Database, Type InstallerType, 
                Object InstallerObject)
            {
                // Add properties from Task definition
                foreach (XmlNode keyNode in _searchNodes)
                {
                    XmlElement keyElem = (XmlElement)keyNode;

                    string type = keyElem.GetAttribute("type");

                    if (type == null || type == "")
                    {
                        Log.WriteLine(LogPrefix + 
                            "ERROR: Search key with no type attribute detected.");
                        return false;
                    }
                    switch (type)
                    {
                        case "registry":
                        {

                            // Select the "AppSearch" Table
                            View appSearchView = Database.OpenView("SELECT * FROM `AppSearch`");
                            XmlNodeList values = keyNode.SelectNodes("value");
                            if (values != null)
                            {
                                foreach (XmlNode valueNode in values)
                                {
                                    XmlElement valueElem = (XmlElement)valueNode;

                                    string propRef = null;
                                    XmlNode propNode = valueElem.SelectSingleNode("@ref");
                                    if (propNode != null)
                                    {
                                        propRef = propNode.Value;
                                    }

                                    if (propRef == null || propRef == "")
                                    {
                                        Log.WriteLine(LogPrefix + 
                                            "ERROR: Search key with no ref attribute detected.");
                                        return false;
                                    }
                                    string signature = "SIG_" + propRef;

                                    // Insert the Property/Signature into AppSearch Table
                                    Record recAppSearch = (Record)InstallerType.InvokeMember(
                                        "CreateRecord", 
                                        BindingFlags.InvokeMethod, 
                                        null, InstallerObject, 
                                        new object[] { 2 });

                                    recAppSearch.set_StringData(1, propRef);
                                    recAppSearch.set_StringData(2, signature);

                                    appSearchView.Modify(MsiViewModify.msiViewModifyInsert, recAppSearch);

                                    if (Verbose)
                                    {
                                        Log.WriteLine(LogPrefix + "Setting App Search: " + propRef);
                                    }
                                }
                            }
                            appSearchView.Close();

                            break;
                        }
                        case "file":
                        {
                            break;
                        }
                    }
                }
                return true;
            }

            /// <summary>
            /// Sets the sequence number of files to match their 
            /// storage order in the cabinet file, after some 
            /// files have had their filenames changed to go in 
            /// their own component.
            /// </summary>
            /// <param name="Database">The MSI database.</param>
            /// <param name="LastSequence">The last file's sequence number.</param>
            /// <returns>True if successful</returns>
            private bool ReorderFiles(Database Database, ref int LastSequence)
            {
	            string curPath = Path.Combine(Project.BaseDirectory, msi.sourcedir);
	            string curTempPath = Path.Combine(curPath, "Temp");

	            string[] curFileNames = Directory.GetFiles(curTempPath, "*.*");

	            LastSequence = 1;

                foreach (string curDirFileName in curFileNames)
                {
                    View curFileView = Database.OpenView(
                        "SELECT * FROM `File` WHERE `File`='" + 
                        Path.GetFileName(curDirFileName) + "'");

                    if (curFileView != null)
                    {
                        curFileView.Execute(null);
                        Record recCurFile = curFileView.Fetch();

                        if (recCurFile != null)
                        {
                            recCurFile.set_StringData(8, LastSequence.ToString());
                            curFileView.Modify(MsiViewModify.msiViewModifyUpdate, recCurFile);
                            curFileView.Close();

                            LastSequence++;
                        }
                        else
                        {
                            Log.WriteLine(LogPrefix + "File " + 
                                Path.GetFileName(curDirFileName) + 
                                " not found during reordering.");

                            curFileView.Close();

                            return false;
                        }
                    }
                    curFileView.Close();
                }

	            return true;
            }

            /// <summary>
            /// Creates a .cab file with all source files included.
            /// </summary>
            /// <param name="Database">The MSI database.</param>
            /// <param name="InstallerType">The MSI Installer type.</param>
            /// <param name="InstallerObject">The MSI Installer object.</param>
            /// <returns>True if successful.</returns>
            private bool CreateCabFile(Database Database, Type InstallerType, Object InstallerObject)
            {
	            Log.Write(LogPrefix + "Compressing Files...");

	            // Create the CabFile
	            ProcessStartInfo processInfo = new ProcessStartInfo();
            	
	            processInfo.Arguments = "-p -r -P " + 
		            Path.Combine(msi.sourcedir, "Temp") + @"\ N " + 
		            msi.sourcedir + @"\" + 
		            Path.GetFileNameWithoutExtension(msi.output) + @".cab " + 
		            Path.Combine(msi.sourcedir, "Temp") + @"\*";

	            processInfo.CreateNoWindow = false;
	            processInfo.WindowStyle = ProcessWindowStyle.Hidden;
	            processInfo.WorkingDirectory = msi.output;
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
            	
	            string cabFile = Path.Combine(Project.BaseDirectory, 
		            Path.Combine(msi.sourcedir, 
		            Path.GetFileNameWithoutExtension(msi.output) + @".cab"));

	            if (File.Exists(cabFile))
	            {
		            View cabView = Database.OpenView("SELECT * FROM `_Streams`");
		            if (Verbose)
		            {
			            Log.WriteLine();
			            Log.WriteLine(LogPrefix + "Storing Cabinet in MSI Database...");
		            }

		            Record cabRecord = (Record)InstallerType.InvokeMember(
			            "CreateRecord", 
			            BindingFlags.InvokeMethod, 
			            null, InstallerObject, 
			            new object[] { 2 });

		            cabRecord.set_StringData(1, Path.GetFileName(cabFile));
		            cabRecord.SetStream(2, cabFile);

		            cabView.Modify(MsiViewModify.msiViewModifyInsert, cabRecord);
		            cabView.Close();
	            }
	            else
	            {
		            Log.WriteLine(LogPrefix + 
			            "ERROR: Unable to open Cabinet file:\n\n\t" + 
			            cabFile + "\n\n");
		            return false;
	            }
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
            	
                Uri shortPathUri = null;
                try
                {
                    shortPathUri = new Uri("file://" + shortPath.ToString());
                }
                catch (Exception)
                {
                    Log.WriteLine(LogPrefix + "ERROR: Directory " + 
                        LongPath + " not found.");
                    return "MsiTaskPathNotFound";
                }

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
            /// <param name="Database">The MSI database.</param>
            /// <param name="InstallerType">The MSI Installer type.</param>
            /// <param name="InstallerObject">The MSI Installer object.</param>
            /// <param name="Name">The Name of the Folder</param>
            /// <param name="Parent">The Parent of the Folder</param>
            /// <param name="Default">The Relative Filesystem Path of the Folder</param>
            /// <param name="Path">The Path to the Folder from previous calls.</param>
            /// <param name="DirectoryView">The MSI database view.</param>
            private void GetRelativePath(
                Database Database, 
                Type InstallerType, 
                Object InstallerObject,
	            string Name, 
	            string Parent, 
	            string Default, 
	            StringBuilder Path, 
                View DirectoryView)
            {
	            if (Name == "TARGETDIR")
	            {
		            return;
	            }

	            for (int i = 0; i < commonFolderNames.Length; i++)
	            {
		            if (Name == commonFolderNames[i])
		            {
			            return;
		            }
	            }

                ArrayList directoryList = new ArrayList();
                foreach(MSIRootDirectory directory in msi.directories)
                {
                    directoryList.Add(directory);
                }

                foreach (property property in msi.properties)
                {
                    if (Name == property.name)
                    {
                        MSIDirectory directory = FindDirectory(Name);
                        if (directory == null)
                        {
                            MSIRootDirectory propDirectory = new MSIRootDirectory();
                            propDirectory.name = Name;
                            propDirectory.root = "TARGETDIR";
                            propDirectory.foldername = ".";

                            directoryList.Add(propDirectory);

                            MSIRootDirectory[] rootDirs = new MSIRootDirectory[directoryList.Count];
                            directoryList.CopyTo(rootDirs);

                            msi.directories = rootDirs;
                        }
                        return;
                    }
                }

	            if (Path.Length > 0)
	            {
		            Path.Insert(0, @"\");
	            }

	            Path.Insert(0, Default);
	            if (Parent != null)
	            {
                    MSIDirectory PathInfo = FindDirectory(Parent);

                    if (PathInfo == null)
                    {
                        foreach (property property in msi.properties)
                        {
                            if (Parent == property.name)
                            {
                                MSIRootDirectory directory = new MSIRootDirectory();
                                directory.name = Parent;
                                directory.root = "TARGETDIR";
                                directory.foldername = ".";

                                directoryList.Add(directory);

                                MSIRootDirectory[] rootDirs = new MSIRootDirectory[directoryList.Count];
                                directoryList.CopyTo(rootDirs);

                                msi.directories = rootDirs;

                                // Insert the Directory that is a Property
                                Record recDir = (Record)InstallerType.InvokeMember(
                                    "CreateRecord", 
                                    BindingFlags.InvokeMethod, 
                                    null, InstallerObject, new object[] { 3 });

                                recDir.set_StringData(1, Parent);
                                recDir.set_StringData(2, "TARGETDIR");
                                recDir.set_StringData(3, ".");

                                DirectoryView.Modify(MsiViewModify.msiViewModifyInsert, recDir);

                                PathInfo = FindDirectory(Parent);

                                break;
                            }
                        }
                    }

		            GetRelativePath(Database, InstallerType, InstallerObject, 
                        Parent, PathInfo is MSIRootDirectory ? ((MSIRootDirectory)PathInfo).root : null, 
                        PathInfo.foldername, Path, DirectoryView);
	            }
            }

            /// <summary>
            /// Recursively expands properties of all attributes of 
            /// a nodelist and their children.
            /// </summary>
            /// <param name="Nodes">The nodes to recurse.</param>
            void ExpandPropertiesInNodes(XmlNodeList Nodes) 
            {
	            foreach (XmlNode node in Nodes)
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

            /// <summary>
            /// Converts the Byte array in a public key 
            /// token of an assembly to a string MSI expects.
            /// </summary>
            /// <param name="ByteArray">The array of bytes.</param>
            /// <returns>The string containing the array.</returns>
            private string ByteArrayToString(Byte[] ByteArray)
            {
	            if ((ByteArray == null) || (ByteArray.Length == 0))
		            return "";
	            StringBuilder sb = new StringBuilder ();
	            sb.Append (ByteArray[0].ToString("x2"));
	            for (int i = 1; i < ByteArray.Length; i++) 
	            {
		            sb.Append(ByteArray[i].ToString("x2"));
	            }
	            return sb.ToString().ToUpper();
            }

            [DllImport("oleaut32.dll", CharSet=CharSet.Auto)]
            private static extern int LoadTypeLib(string TypeLibFileName, ref IntPtr pTypeLib);

            /// <summary>
            /// Loads TypeLibs for the TypeLib table.
            /// </summary>
            /// <param name="Database">The MSI database.</param>
            /// <param name="InstallerType">The MSI Installer type.</param>
            /// <param name="InstallerObject">The MSI Installer object.</param>
            /// <param name="RegistryView">View containing the Registry Table.</param>
            /// <returns>True if successful.</returns>
            private bool LoadTypeLibs(Database Database, Type InstallerType, object InstallerObject, View RegistryView)
            {
	            // Open the "TypeLib" Table
	            View typeLibView = Database.OpenView("SELECT * FROM `TypeLib`");

	            string runtimeVer = Environment.Version.ToString(4);

	            for (int i = 0; i < typeLibRecords.Count; i++)
	            {
		            TypeLibRecord tlbRecord = (TypeLibRecord)typeLibRecords[i];

		            IntPtr pTypeLib = new IntPtr(0);
		            int result = LoadTypeLib(tlbRecord.TypeLibFileName, ref pTypeLib);
		            if (result == 0)
		            {
			            UCOMITypeLib typeLib = (UCOMITypeLib)Marshal.GetTypedObjectForIUnknown(
				            pTypeLib, typeof(UCOMITypeLib));
			            if (typeLib != null)
			            {
				            int helpContextId;
				            string name, docString, helpFile;

				            typeLib.GetDocumentation(
					            -1, out name, out docString, 
					            out helpContextId, out helpFile);

				            IntPtr pTypeLibAttr = new IntPtr(0);
				            typeLib.GetLibAttr(out pTypeLibAttr);

				            TYPELIBATTR typeLibAttr = (TYPELIBATTR)Marshal.PtrToStructure(pTypeLibAttr, typeof(TYPELIBATTR));

				            string tlbCompName = (string)typeLibComponents[Path.GetFileName(tlbRecord.TypeLibFileName)];

				            Record recTypeLib = (Record)InstallerType.InvokeMember(
					            "CreateRecord", 
					            BindingFlags.InvokeMethod, 
					            null, InstallerObject, 
					            new object[] { 8 });

				            recTypeLib.set_StringData(1, "{"+typeLibAttr.guid.ToString().ToUpper()+"}");
				            recTypeLib.set_IntegerData(2, Marshal.GetTypeLibLcid(typeLib));
				            recTypeLib.set_StringData(3, tlbCompName);
				            recTypeLib.set_IntegerData(4, 256);
				            recTypeLib.set_StringData(5, docString == null ? name : docString);
				            recTypeLib.set_StringData(7, tlbRecord.FeatureName);
				            recTypeLib.set_IntegerData(8, 0);
            				
				            typeLib.ReleaseTLibAttr(pTypeLibAttr);

				            typeLibView.Modify(MsiViewModify.msiViewModifyInsert, recTypeLib);

				            // If a .NET type library wrapper for an assembly
				            if (tlbRecord.AssemblyName != null)
				            {
					            // Get all the types defined in the typelibrary 
					            // that are not marked "noncreatable"

					            int typeCount = typeLib.GetTypeInfoCount();
					            for (int j = 0; j < typeCount; j++)
					            {
						            UCOMITypeInfo typeInfo = null;
						            typeLib.GetTypeInfo(j, out typeInfo);

						            if (typeInfo != null)
						            {
							            IntPtr pTypeAttr = new IntPtr(0);
							            typeInfo.GetTypeAttr(out pTypeAttr);

							            TYPEATTR typeAttr = (TYPEATTR)Marshal.PtrToStructure(pTypeAttr, typeof(TYPEATTR));

							            if (typeAttr.typekind == TYPEKIND.TKIND_COCLASS 
								            && typeAttr.wTypeFlags == TYPEFLAGS.TYPEFLAG_FCANCREATE)
							            {
								            string clsid = "{" + typeAttr.guid.ToString().ToUpper() + "}";

								            if (typeInfo is UCOMITypeInfo2)
								            {
									            UCOMITypeInfo2 typeInfo2 = (UCOMITypeInfo2)typeInfo;
									            if (typeInfo2 != null)
									            {
										            //try
										            //{
											            object custData = new object();
											            Guid g = new Guid("0F21F359-AB84-41E8-9A78-36D110E6D2F9");
											            typeInfo2.GetCustData(ref g, out custData);

											            if (custData != null)
											            {
												            string className = (string)custData;

												            if (Verbose)
												            {
													            Log.WriteLine(LogPrefix + "Storing Type " + className);
												            }

												            // Insert the Class
												            Record recRegTlbRec = (Record)InstallerType.InvokeMember(
													            "CreateRecord", 
													            BindingFlags.InvokeMethod, 
													            null, InstallerObject, 
													            new object[] { 6 });

												            recRegTlbRec.set_StringData(1, 
													            "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
												            recRegTlbRec.set_IntegerData(2, 0);
												            recRegTlbRec.set_StringData(3,
													            @"CLSID\" + clsid + 
													            @"\InprocServer32");
												            recRegTlbRec.set_StringData(4, "Class");
												            recRegTlbRec.set_StringData(5, className);
												            recRegTlbRec.set_StringData(6, tlbRecord.AssemblyComponent);
												            RegistryView.Modify(MsiViewModify.msiViewModifyInsert, recRegTlbRec);

												            recRegTlbRec.set_StringData(1, 
													            "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
												            recRegTlbRec.set_StringData(4, "ThreadingModel");
												            recRegTlbRec.set_StringData(5, "Both");
												            RegistryView.Modify(MsiViewModify.msiViewModifyInsert, recRegTlbRec);

												            recRegTlbRec.set_StringData(1, 
													            "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
												            recRegTlbRec.set_StringData(4, "RuntimeVersion");
												            recRegTlbRec.set_StringData(5, System.Environment.Version.ToString(3));
												            RegistryView.Modify(MsiViewModify.msiViewModifyInsert, recRegTlbRec);

												            recRegTlbRec.set_StringData(1, 
													            "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
												            recRegTlbRec.set_StringData(4, "Assembly");
												            recRegTlbRec.set_StringData(5, tlbRecord.AssemblyName.FullName);
												            RegistryView.Modify(MsiViewModify.msiViewModifyInsert, recRegTlbRec);

												            recRegTlbRec.set_StringData(1, 
													            "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
												            recRegTlbRec.set_StringData(3,
													            @"CLSID\" + clsid + 
													            @"\Implemented Categories");
												            recRegTlbRec.set_StringData(4, "+");
												            recRegTlbRec.set_StringData(5, null);
												            RegistryView.Modify(MsiViewModify.msiViewModifyInsert, recRegTlbRec);

												            recRegTlbRec.set_StringData(1, 
													            "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
												            recRegTlbRec.set_StringData(3,
													            @"CLSID\" + clsid + 
													            @"\Implemented Categories\{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}");
												            recRegTlbRec.set_StringData(4, "+");
												            recRegTlbRec.set_StringData(5, null);
												            RegistryView.Modify(MsiViewModify.msiViewModifyInsert, recRegTlbRec);
											            }
										            //}
										            //catch (Exception) {}
									            }
								            }
							            }
							            else if (typeAttr.typekind == TYPEKIND.TKIND_DISPATCH)
							            {
								            string iid = "{" + typeAttr.guid.ToString().ToUpper() + "}";

								            string typeName, typeDocString, typeHelpFile;
								            int typeHelpContextId;

								            typeInfo.GetDocumentation(-1, out typeName, 
									            out typeDocString, out typeHelpContextId, 
									            out typeHelpFile);

								            if (typeInfo is UCOMITypeInfo2)
								            {
									            UCOMITypeInfo2 typeInfo2 = (UCOMITypeInfo2)typeInfo;
									            if (typeInfo2 != null)
									            {
										            //try
										            //{
										            object custData = new object();
										            Guid g = new Guid("0F21F359-AB84-41E8-9A78-36D110E6D2F9");
										            typeInfo2.GetCustData(ref g, out custData);

										            if (custData != null)
										            {
											            string className = (string)custData;

											            if (Verbose)
											            {
												            Log.WriteLine(LogPrefix + "Storing Interface " + className);
											            }

											            // Insert the Interface
											            Record recRegTlbRec = (Record)InstallerType.InvokeMember(
												            "CreateRecord", 
												            BindingFlags.InvokeMethod, 
												            null, InstallerObject, 
												            new object[] { 6 });

											            string typeLibComponent = (string)typeLibComponents[Path.GetFileName(tlbRecord.TypeLibFileName)];

											            recRegTlbRec.set_StringData(1, 
												            "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
											            recRegTlbRec.set_IntegerData(2, 0);
											            recRegTlbRec.set_StringData(3,
												            @"Interface\" + iid);
											            recRegTlbRec.set_StringData(4, null);
											            recRegTlbRec.set_StringData(5, typeName);
											            recRegTlbRec.set_StringData(6, typeLibComponent);
											            RegistryView.Modify(MsiViewModify.msiViewModifyInsert, recRegTlbRec);

											            recRegTlbRec.set_StringData(1, 
												            "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
											            recRegTlbRec.set_StringData(3,
												            @"Interface\" + iid + @"\TypeLib");
											            recRegTlbRec.set_StringData(4, "Version");
											            recRegTlbRec.set_StringData(5, "1.0");
											            RegistryView.Modify(MsiViewModify.msiViewModifyInsert, recRegTlbRec);

											            recRegTlbRec.set_StringData(1, 
												            "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
											            recRegTlbRec.set_StringData(4, null);
											            recRegTlbRec.set_StringData(5, "{"+typeLibAttr.guid.ToString().ToUpper()+"}");
											            RegistryView.Modify(MsiViewModify.msiViewModifyInsert, recRegTlbRec);

											            recRegTlbRec.set_StringData(1, 
												            "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
											            recRegTlbRec.set_StringData(3,
												            @"Interface\" + iid + @"\ProxyStubClsid32");
											            recRegTlbRec.set_StringData(4, null);
											            recRegTlbRec.set_StringData(5, "{00020424-0000-0000-C000-000000000046}");
											            RegistryView.Modify(MsiViewModify.msiViewModifyInsert, recRegTlbRec);

											            recRegTlbRec.set_StringData(1, 
												            "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
											            recRegTlbRec.set_StringData(3,
												            @"Interface\" + iid + @"\ProxyStubClsid");
											            recRegTlbRec.set_StringData(4, null);
											            recRegTlbRec.set_StringData(5, "{00020424-0000-0000-C000-000000000046}");
											            RegistryView.Modify(MsiViewModify.msiViewModifyInsert, recRegTlbRec);
										            }
										            //}
										            //catch (Exception) {}
									            }
								            }
							            }
						            }
					            }
				            }
			            }
		            }
	            }

	            typeLibView.Close();

	            return true;
            }

            /// <summary>
            /// Merges Merge Modules into the MSI Database.
            /// </summary>
            /// <param name="Database">The MSI Database.</param>
            /// <param name="TempPath">The path to temporary files.</param>
            /// <returns>True if successful.</returns>
            private bool LoadMergeModules(string Database, string TempPath)
            {
                MsmMergeClass merge = new MsmMergeClass();

                int index = 1;

                foreach (XmlNode mergeModuleNode in _mergeModules)
                {
                    XmlElement mergeElem = (XmlElement)mergeModuleNode;
                    XmlElement modulesElem = (XmlElement)mergeElem.SelectSingleNode("modules");

                    if (modulesElem != null)
                    {
                        string featureName = mergeElem.GetAttribute("feature");

                        if (featureName == null || featureName == "")
                        {
                            Log.WriteLine(LogPrefix + "ERROR: merge element must specify a feature attribute.");
                            return false;
                        }

                        FileSet mergeSet = new FileSet();
                        mergeSet.Project = Project;
                        mergeSet.Initialize(modulesElem);

                        foreach (string mergeModule in mergeSet.FileNames)
                        {
                            Log.WriteLine(LogPrefix + "Merging {0}", Path.GetFileName(mergeModule) + ".");
                            
                            merge.OpenModule(mergeModule, 1033);

                            try
                            {
                                merge.OpenDatabase(Database);
                            }
                            catch (FileLoadException fle)
                            {
                                Log.WriteLine(fle.Message + " " + fle.FileName + " " + fle.StackTrace);
                                return false;
                            }

                            merge.Merge(featureName, null);

                            string moduleCab = Path.Combine(Path.GetDirectoryName(Database), 
                                "mergemodule" + index + ".cab");

                            index++;

                            merge.ExtractCAB(moduleCab);

                            if (File.Exists(moduleCab))
                            {
                                // Create the CabFile
                                ProcessStartInfo processInfo = new ProcessStartInfo();
            	
                                processInfo.Arguments = "-o X " + 
                                    moduleCab + " " + Path.Combine(msi.sourcedir, @"Temp\");

                                processInfo.CreateNoWindow = false;
                                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                processInfo.WorkingDirectory = msi.output;
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
                                    Log.WriteLine("Error extracting merge module cab file: " + moduleCab);
                                    Log.WriteLine("Error was: " + e.Message);

                                    File.Delete(moduleCab);
                                    return false;
                                }

                                if (process.ExitCode != 0)
                                {
                                    Log.WriteLine();
                                    Log.WriteLine("Error extracting merge module cab file: " + moduleCab);
                                    Log.WriteLine("Application returned ERROR: " + process.ExitCode);

                                    File.Delete(moduleCab);
                                    return false;
                                }

                                File.Delete(moduleCab);
                            }

                            merge.CloseModule();
                            merge.CloseDatabase(true);
                        }
                    }
                }

                return true;
            }

            /// <summary>
            /// Enumerates the registry to see if an assembly has been registered 
            /// for COM interop, and if so adds these registry keys to the Registry 
            /// table, ProgIds to the ProgId table, classes to the Classes table, 
            /// and a TypeLib to the TypeLib table.
            /// </summary>
            /// <param name="FileName">The Assembly filename.</param>
            /// <param name="FileAssembly">The Assembly to check.</param>
            /// <param name="InstallerType">The MSI Installer type.</param>
            /// <param name="InstallerObject">The MSI Installer object.</param>
            /// <param name="ComponentName">The name of the containing component.</param>
            /// <param name="AssemblyComponentName">The name of the containing component's assembly GUID.</param>
            /// <param name="ClassView">View containing the Class table.</param>
            /// <param name="ProgIdView">View containing the ProgId table.</param>
            /// <returns>True if successful.</returns>
            private bool CheckAssemblyForCOMInterop(string FileName, Assembly FileAssembly, Type InstallerType, 
                object InstallerObject, string ComponentName, string AssemblyComponentName, View ClassView, View ProgIdView)
            {
                AssemblyName asmName = FileAssembly.GetName();
                string featureName = (string)featureComponents[ComponentName];
                string typeLibName = Path.GetFileNameWithoutExtension(FileName) + ".tlb";
                string typeLibFileName = Path.Combine(Path.GetDirectoryName(FileName), typeLibName);

                bool foundTypeLib = false;

                // Register the TypeLibrary
                RegistryKey typeLibsKey = Registry.ClassesRoot.OpenSubKey("Typelib", false);

                string[] typeLibs = typeLibsKey.GetSubKeyNames();
                foreach (string typeLib in typeLibs)
                {
                    RegistryKey typeLibKey = typeLibsKey.OpenSubKey(typeLib, false);
                    if (typeLibKey != null)
                    {
                        string[] typeLibSubKeys = typeLibKey.GetSubKeyNames();
                        foreach (string typeLibSubKey in typeLibSubKeys)
                        {
                            RegistryKey win32Key = typeLibKey.OpenSubKey(typeLibSubKey + @"\0\win32");
                            if (win32Key != null)
                            {
                                string curTypeLibFileName = (string)win32Key.GetValue(null, null);
                                if (curTypeLibFileName != null)
                                {
                                    if (curTypeLibFileName == typeLibFileName)
                                    {
                                        Log.WriteLine(LogPrefix + "Configuring " + typeLibName + " for COM Interop...");

                                        Record recTypeLib = (Record)InstallerType.InvokeMember(
                                            "CreateRecord", 
                                            BindingFlags.InvokeMethod, 
                                            null, InstallerObject, 
                                            new object[] { 8 });

                                        TypeLibRecord tlbRecord = new TypeLibRecord(
                                            typeLib, typeLibFileName, 
                                            asmName, featureName, AssemblyComponentName);

                                        typeLibRecords.Add(tlbRecord);

                                        foundTypeLib = true;
                                        win32Key.Close();
                                        break;
                                    }
                                }
                                win32Key.Close();
                            }
                        }
                        typeLibKey.Close();

                        if (foundTypeLib)
                        {
                            break;
                        }
                    }
                }
                typeLibsKey.Close();

                // Register CLSID(s)
                RegistryKey clsidsKey = Registry.ClassesRoot.OpenSubKey("CLSID", false);
                
                string[] clsids = clsidsKey.GetSubKeyNames();
                foreach (string clsid in clsids)
                {
                    RegistryKey clsidKey = clsidsKey.OpenSubKey(clsid, false);
                    if (clsidKey != null)
                    {
                        RegistryKey inprocKey = clsidKey.OpenSubKey("InprocServer32", false);
                        if (inprocKey != null)
                        {
                            string clsidAsmName = (string)inprocKey.GetValue("Assembly", null);
                            if (clsidAsmName != null)
                            {
                                if (asmName.FullName == clsidAsmName)
                                {
                                    // Register ProgId(s)
                                    RegistryKey progIdKey = clsidKey.OpenSubKey("ProgId", false);
                                    if (progIdKey != null)
                                    {
                                        string progId = (string)progIdKey.GetValue(null, null);
                                        string className = (string)clsidKey.GetValue(null, null);

                                        if (progId != null)
                                        {
                                            Record recProgId = (Record)InstallerType.InvokeMember(
                                                "CreateRecord", 
                                                BindingFlags.InvokeMethod, 
                                                null, InstallerObject, 
                                                new object[] { 6 });

                                            recProgId.set_StringData(1, progId);
                                            recProgId.set_StringData(3, clsid);
                                            recProgId.set_StringData(4, className);
                                            recProgId.set_IntegerData(6, 0);
                                            ProgIdView.Modify(MsiViewModify.msiViewModifyInsert, recProgId);

                                            Record recClass = (Record)InstallerType.InvokeMember(
                                                "CreateRecord", 
                                                BindingFlags.InvokeMethod, 
                                                null, InstallerObject, 
                                                new object[] { 13 });

                                            recClass.set_StringData(1, clsid);
                                            recClass.set_StringData(2, "InprocServer32");
                                            recClass.set_StringData(3, AssemblyComponentName);
                                            recClass.set_StringData(4, progId);
                                            recClass.set_StringData(5, className);
                                            //recClass.set_StringData(6, appId);
                                            recClass.set_IntegerData(9, 0);
                                            recClass.set_StringData(12, featureName);
                                            recClass.set_IntegerData(13, 0);
                                            ClassView.Modify(MsiViewModify.msiViewModifyInsert, recClass);
                                        }
                                        progIdKey.Close();
                                    }
                                }
                            }
                            inprocKey.Close();
                        }
                        clsidKey.Close();
                    }
                }
                clsidsKey.Close();

                return true;
            }

            private string FindParent(string DirectoryName)
            {
                foreach (MSIDirectory directory in msi.directories)
                {
                    string parent = FindParent(DirectoryName, directory);
                    if (parent != null)
                    {
                        return parent;
                    }
                }
                return null;
            }

            private string FindParent(string DirectoryName, MSIDirectory directory)
            {
                if (DirectoryName == directory.name && 
                    directory is MSIRootDirectory)
                {
                    return ((MSIRootDirectory)directory).root;
                }
                else
                {
                    if (directory.directory != null)
                    {
                        foreach (MSIDirectory directory2 in directory.directory)
                        {
                            if (directory2.name == DirectoryName)
                            {
                                return directory.name;
                            }
                            else
                            {
                                string parent = FindParent(DirectoryName, directory2);
                                if (parent != null)
                                {
                                    return parent;
                                }
                            }
                        }
                    }
                }
                return null;
            }

            private MSIDirectory FindDirectory(string DirectoryName)
            {
                foreach (MSIDirectory directory in msi.directories)
                {
                    MSIDirectory childDirectory = FindDirectory(DirectoryName, directory);
                    if (childDirectory != null)
                    {
                        return childDirectory;
                    }
                }

                return null;
            }

            private MSIDirectory FindDirectory(string DirectoryName, MSIDirectory directory)
            {
                if (directory.name == DirectoryName)
                {
                    return directory;
                }

                if (directory.directory != null)
                {
                    foreach (MSIDirectory childDirectory in directory.directory)
                    {
                        MSIDirectory childDirectory2 = FindDirectory(DirectoryName, childDirectory);
                        if (childDirectory2 != null)
                        {
                            return childDirectory2;
                        }
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Maintains a forward reference to a .tlb file 
        /// in the same directory as an assembly .dll 
        /// that has been registered for COM interop.
        /// </summary>
        internal class TypeLibRecord
        {
            private AssemblyName assemblyName;
            private string libId, typeLibFileName, 
	            featureName, assemblyComponent;

            /// <summary>
            /// Creates a new <see cref="TypeLibRecord"/>.
            /// </summary>
            /// <param name="LibId">The typelibrary id.</param>
            /// <param name="TypeLibFileName">The typelibrary filename.</param>
            /// <param name="AssemblyName">The name of the assembly.</param>
            /// <param name="FeatureName">The feature containing the typelibrary's file.</param>
            /// <param name="AssemblyComponent">The name of the Assembly's component.</param>
            public TypeLibRecord(
	            string LibId, string TypeLibFileName, 
	            AssemblyName AssemblyName, string FeatureName, 
	            string AssemblyComponent)
            {
	            libId = LibId;
	            typeLibFileName = TypeLibFileName;
	            assemblyName = AssemblyName;
	            featureName = FeatureName;
	            assemblyComponent = AssemblyComponent;
            }

            /// <summary>
            /// Retrieves the name of the Assembly's component.
            /// </summary>
            /// <value>The Assembly's component Name.</value>
            /// <remarks>None.</remarks>
            public string AssemblyComponent
            {
	            get { return assemblyComponent; }
            }

            /// <summary>
            /// Retrieves the typelibrary filename.
            /// </summary>
            /// <value>The typelibrary filename.</value>
            /// <remarks>None.</remarks>
            public string TypeLibFileName
            {
	            get { return typeLibFileName; }
            }

            /// <summary>
            /// Retrieves the typelibrary id.
            /// </summary>
            /// <value>The typelibrary id.</value>
            /// <remarks>None.</remarks>
            public string LibId
            {
	            get { return libId; }
            }

            /// <summary>
            /// Retrieves the name of the assembly.
            /// </summary>
            /// <value>The name of the assembly.</value>
            /// <remarks>None.</remarks>
            public AssemblyName AssemblyName
            {
	            get { return assemblyName; }
            }

            /// <summary>
            /// Retrieves the feature containing the typelibrary's file.
            /// </summary>
            /// <value>The feature containing the typelibrary's file.</value>
            /// <remarks>None.</remarks>
            public string FeatureName
            {
	            get { return featureName; }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CUSTDATAITEM
        {
            public Guid guid;
            public object varValue;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CUSTDATA
        {
            public int cCustData;
            public CUSTDATAITEM[] prgCustData;
        }

        [ComImport]
        [Guid("00020412-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface UCOMITypeInfo2
        {
            #region Implementation of UCOMITypeInfo
            void GetContainingTypeLib(out System.Runtime.InteropServices.UCOMITypeLib ppTLB, out int pIndex);
            void GetIDsOfNames(string[] rgszNames, int cNames, int[] pMemId);
            void GetRefTypeInfo(int hRef, out System.Runtime.InteropServices.UCOMITypeInfo ppTI);
            void GetMops(int memid, out string pBstrMops);
            void ReleaseVarDesc(System.IntPtr pVarDesc);
            void ReleaseTypeAttr(System.IntPtr pTypeAttr);
            void GetDllEntry(int memid, System.Runtime.InteropServices.INVOKEKIND invKind, out string pBstrDllName, out string pBstrName, out short pwOrdinal);
            void GetRefTypeOfImplType(int index, out int href);
            void GetTypeComp(out System.Runtime.InteropServices.UCOMITypeComp ppTComp);
            void GetTypeAttr(out System.IntPtr ppTypeAttr);
            void GetDocumentation(int index, out string strName, out string strDocString, out int dwHelpContext, out string strHelpFile);
            void AddressOfMember(int memid, System.Runtime.InteropServices.INVOKEKIND invKind, out System.IntPtr ppv);
            void GetNames(int memid, string[] rgBstrNames, int cMaxNames, out int pcNames);
            void CreateInstance(object pUnkOuter, ref System.Guid riid, out object ppvObj);
            void Invoke(object pvInstance, int memid, short wFlags, ref System.Runtime.InteropServices.DISPPARAMS pDispParams, out object pVarResult, out System.Runtime.InteropServices.EXCEPINFO pExcepInfo, out int puArgErr);
            void GetVarDesc(int index, out System.IntPtr ppVarDesc);
            void ReleaseFuncDesc(System.IntPtr pFuncDesc);
            void GetFuncDesc(int index, out System.IntPtr ppFuncDesc);
            void GetImplTypeFlags(int index, out int pImplTypeFlags);
            #endregion

            void GetTypeKind([Out] out TYPEKIND pTypeKind);
            void GetTypeFlags([Out] out int pTypeFlags);
            void GetFuncIndexOfMemId(int memid, INVOKEKIND invKind, [Out] out int pFuncIndex);
            void GetVarIndexOfMemId(int memid, [Out] out int pVarIndex);
            void GetCustData([In] ref Guid guid, [Out] out object pCustData);
            void GetFuncCustData(int index, [In] ref Guid guid, [Out] out object pVarVal);
            void GetParamCustData(int indexFunc, int indexParam, [In] ref Guid guid, [Out] out object pVarVal);
            void GetVarCustData(int index, [In] ref Guid guid, [Out] out object pVarVal);
            void GetImplTypeCustData(int index, [In] ref Guid guid, [Out] out object pVarVal);
            void GetDocumentation2(int memid, int lcid, [Out] out string pbstrHelpString, [Out] out int pdwHelpStringContext, [Out] out string pbstrHelpStringDll);
            void GetAllCustData([In,Out] ref IntPtr pCustData);
            void GetAllFuncCustData(int index, [Out] out CUSTDATA pCustData);
            void GetAllParamCustData(int indexFunc, int indexParam, [Out] out CUSTDATA pCustData);
            void GetAllVarCustData(int index, [Out] out CUSTDATA pCustData);
            void GetAllImplTypeCustData(int index, [Out] out CUSTDATA pCustData);
        }
}