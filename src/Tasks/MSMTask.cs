#region GNU General Public License
//
// NAntContrib
//
// Copyright (C) 2003 James Geurts (jgeurts@users.sourceforge.net)
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
#endregion

using System;
using System.IO;
using System.Text;
using Microsoft.Win32;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using NAnt.Core;
using NAnt.Core.Tasks;
using NAnt.Core.Types;
using NAnt.Core.Attributes;

using NAnt.Contrib.Schemas.MSM;
using WindowsInstaller;
using MsmMergeTypeLib;

namespace NAnt.Contrib.Tasks
{
    /// <summary>
    /// Builds a Windows Installer Merge Module (MSM) File.
    /// </summary>
    /// <remarks>Requires <c>cabarc.exe</c> in the path.  This tool is included in the Microsoft Cabinet SDK.</remarks>
    [TaskName("msm")]
    [SchemaValidator(typeof(msm))]
    public class MSMTask : SchemaValidatedTask
    {
        msm msm;

        Hashtable files = new Hashtable();
        Hashtable featureComponents = new Hashtable();
        Hashtable components = new Hashtable();
        ArrayList typeLibRecords = new ArrayList();
        Hashtable typeLibComponents = new Hashtable();

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
            base.InitializeTask(TaskNode);

            msm = (msm)SchemaObject;
        }

        /// <summary>
        /// Executes the Task.
        /// </summary>
        /// <remarks>None.</remarks>
        protected override void ExecuteTask()
        {
            // Create WindowsInstaller.Installer
            Type msmType = Type.GetTypeFromProgID("WindowsInstaller.Installer");
            Object obj = Activator.CreateInstance(msmType);

            // Open the Template MSM File
            Module tasksModule = Assembly.GetExecutingAssembly().GetModule("NAnt.Contrib.Tasks.dll");
            
            string source = Path.GetDirectoryName(
                tasksModule.FullyQualifiedName) + "\\MSMTaskTemplate.msm";

            string dest = Path.Combine(Project.BaseDirectory, 
                Path.Combine(msm.sourcedir, msm.output));

            string errors = Path.GetDirectoryName(
                tasksModule.FullyQualifiedName) + "\\MSMTaskErrors.mst";

            string tempPath = Path.Combine(Project.BaseDirectory, 
                Path.Combine(msm.sourcedir, @"Temp"));

            string cabFile = Path.Combine(Project.BaseDirectory, 
                Path.Combine(msm.sourcedir, @"MergeModule.CABinet"));

            CleanOutput(cabFile, tempPath);

            // Copy the Template MSM File
            try
            {
                File.Copy(source, dest, true);
                File.SetAttributes(dest, System.IO.FileAttributes.Normal);
            }
            catch (IOException)
            {
                throw new BuildException(LogPrefix + 
                    "ERROR: File in use or cannot be copied to output. (" + source + ")");
            }

            try
            {
                // Open the Output Database.
                Database d = null;
                try
                {
                    d = (Database)msmType.InvokeMember(
                        "OpenDatabase",
                        BindingFlags.InvokeMethod, 
                        null, obj, 
                        new Object[]
                        {
                            dest,
                            MsiOpenDatabaseMode.msiOpenDatabaseModeDirect
                        });

                    if (msm.debug)
                    {
                        // If Debug is true, transform the error strings in
                        d.ApplyTransform(errors, MsiTransformError.msiTransformErrorNone);
                    }
                }
                catch (Exception e)
                {                   
                    CleanOutput(cabFile, tempPath);
                    System.Console.WriteLine(e.ToString());
                    throw new Win32Exception();
                }

                Log(Level.Info, LogPrefix + "Building MSM Database \"" + msm.output + "\".");

                // Load the Banner Image
                if (!LoadBanner(d))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load the Background Image
                if (!LoadBackground(d))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load the License File
                if (!LoadLicense(d))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load the ModuleSignature table
                if (!LoadModuleSignature(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load Properties
                if (!LoadProperties(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load Registry Locators
                if (!LoadRegLocator(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load Application Search
                if (!LoadAppSearch(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Add user defined table(s) to the database
                if (!AddTables(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }        
        
                // Load module dependencies
                if (!LoadModuleDependency(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load module exclusions
                if (!LoadModuleExclusion(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load module sequences
                if (!LoadModuleSequence(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load module ignore table
                if (!LoadModuleIgnoreTable(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load module substitution table
                if (!LoadModuleSubstitution(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load module configuration table
                if (!LoadModuleConfiguration(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                try
                {
                    // Commit the MSM Database
                    d.Commit();

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                catch (Exception e)
                {
                    CleanOutput(cabFile, tempPath);
                    System.Console.WriteLine(e.ToString());
                    throw new Win32Exception();
                }

                View directoryView, asmView, asmNameView, classView, progIdView;

                // Load Directories
                if (!LoadDirectories(d, msmType, obj, out directoryView))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load Assemblies
                if (!LoadAssemblies(d, msmType, obj, out asmView, 
                    out asmNameView, out classView, out progIdView))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                int lastSequence = 0;

                // Load Components
                if (!LoadComponents(d, msmType, obj, ref lastSequence, 
                    asmView, asmNameView, directoryView, classView, progIdView))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                try
                {
                    directoryView.Close();
                    asmView.Close();
                    asmNameView.Close();
                    classView.Close();
                    progIdView.Close();
                    directoryView = null;
                    asmView = null;
                    asmNameView = null;
                    classView = null;
                    progIdView = null;

                    // Commit the MSM Database
                    d.Commit();

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                catch (Exception e)
                {
                    CleanOutput(cabFile, tempPath);
                    System.Console.WriteLine(e.ToString());
                    throw new Win32Exception();
                }

                // Load Dialog Data
                if (!LoadDialog(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load Dialog Control Data
                if (!LoadControl(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load Dialog Control Condition Data
                if (!LoadControlCondition(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load Dialog Control Event Data
                if (!LoadControlEvent(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                View registryView;

                // Load the Registry
                if (!LoadRegistry(d, msmType, obj, out registryView))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load TypeLibs
                if (!LoadTypeLibs(d, msmType, obj, registryView))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                try
                {
                    registryView.Close();
                    registryView = null;

                    // Commit the MSM Database
                    d.Commit();

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                catch (Exception e)
                {
                    CleanOutput(cabFile, tempPath);
                    System.Console.WriteLine(e.ToString());
                    throw new Win32Exception();
                }

                // Load Icon Data
                if (!LoadIcon(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load Shortcut Data
                if (!LoadShortcut(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load Binary Data
                if (!LoadBinary(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load Custom Actions
                if (!LoadCustomAction(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load Sequences
                if (!LoadSequence(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load the application mappings
                if (!LoadAppMappings(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }
                
                // Load the url properties to convert
                // url properties to a properties object
                if (!LoadUrlProperties(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load the vdir properties to convert
                // a vdir to an url
                if (!LoadVDirProperties(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load the application root properties
                // to make a virtual directory an virtual
                // application
                if (!LoadAppRootCreate(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load IIS Directory Properties
                if (!LoadIISProperties(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load Summary Information
                if (!LoadSummaryInfo(d))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Load Environment Variables
                if (!LoadEnvironment(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                try
                {
                    // Commit the MSM Database
                    d.Commit();

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                catch (Exception e)
                {
                    CleanOutput(cabFile, tempPath);
                    System.Console.WriteLine(e.ToString());
                    throw new Win32Exception();
                }

                // Reorder Files
                if (!ReorderFiles(d, ref lastSequence))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Compress Files
                if (!CreateCabFile(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // Delete unused tables
                if (!DropEmptyTables(d, msmType, obj))
                {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                Log(Level.Info, LogPrefix + "Deleting Temporary Files...");
                CleanOutput(cabFile, tempPath);
                Log(Level.Info, "Done.");

                try
                {
                    Log(Level.Info, LogPrefix + "Saving MSM Database...");

                    // Commit the MSM Database
                    d.Commit();
                    d = null;

                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.ToString());
                    throw new Win32Exception();
                }
                Log(Level.Info, "Done.");
            }
            catch (Exception e)
            {
                CleanOutput(cabFile, tempPath);
                throw new BuildException(LogPrefix + "ERROR: " + 
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
        /// Loads the banner image.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <returns>True if successful.</returns>
        private bool LoadBanner(Database Database)
        {
            // Try to open the Banner
            if (msm.banner != null)
            {
                string bannerFile = Path.Combine(Project.BaseDirectory, msm.banner);
                if (File.Exists(bannerFile))
                {
                    View bannerView = Database.OpenView("SELECT * FROM `Binary` WHERE `Name`='bannrbmp'");
                    bannerView.Execute(null);
                    Record bannerRecord = bannerView.Fetch();
                    if (Verbose)
                    {
                        Log(Level.Info, LogPrefix + "Storing Banner:\n\t" + bannerFile);
                    }

                    // Write the Banner file to the MSM database
                    bannerRecord.SetStream(2, bannerFile);
                    bannerView.Modify(MsiViewModify.msiViewModifyUpdate, bannerRecord);
                    bannerView.Close();
                    bannerView = null;
                }
                else
                {
                    Log(Level.Info, LogPrefix + 
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
        /// <param name="Database">The MSM database.</param>
        /// <returns>True if successful.</returns>
        private bool LoadBackground(Database Database)
        {
            // Try to open the Background
            if (msm.background != null)
            {
                string bgFile = Path.Combine(Project.BaseDirectory, msm.background);
                if (File.Exists(bgFile))
                {
                    View bgView = Database.OpenView("SELECT * FROM `Binary` WHERE `Name`='dlgbmp'");
                    bgView.Execute(null);
                    Record bgRecord = bgView.Fetch();
                    if (Verbose)
                    {
                        Log(Level.Info, LogPrefix + "Storing Background:\n\t" + bgFile);
                    }

                    // Write the Background file to the MSM database
                    bgRecord.SetStream(2, bgFile);
                    bgView.Modify(MsiViewModify.msiViewModifyUpdate, bgRecord);
                    bgView.Close();
                    bgView = null;
                }
                else
                {
                    Log(Level.Error, LogPrefix + 
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
        /// <param name="Database">The MSM database.</param>
        /// <returns>True if successful.</returns>
        private bool LoadLicense(Database Database)
        {

            // Try to open the License
            if (msm.license != null)
            {
                string licFile = Path.Combine(Project.BaseDirectory, msm.license);
                if (File.Exists(licFile))
                {
                    View licView = Database.OpenView("SELECT * FROM `Control` WHERE `Control`='AgreementText'");
                    licView.Execute(null);
                    Record licRecord = licView.Fetch();
                    
                    Log(Level.Info, LogPrefix + "Storing License:\n\t" + licFile);
                    
                    StreamReader licReader = null;
                    try
                    {
                        licReader = File.OpenText(licFile);
                        licRecord.set_StringData(10, licReader.ReadToEnd());
                        licView.Modify(MsiViewModify.msiViewModifyUpdate, licRecord);
                    }
                    catch (IOException)
                    {
                       Log(Level.Error, LogPrefix + 
                            "ERROR: Unable to open License File:\n\n\t" + 
                            licFile + "\n\n");
                        return false;
                    }
                    finally
                    {
                        licView.Close();
                        licView = null;
                        if (licReader != null)
                        {
                            licReader.Close();
                            licReader = null;
                        }
                    }
                }
                else
                {
                    Log(Level.Error, LogPrefix + 
                        "ERROR: Unable to open License File:\n\n\t" + 
                        licFile + "\n\n");
                    return false;
                }
            }
            else
            {
                // Delete the license control
                View tempView = Database.OpenView("DELETE FROM `Control` WHERE `Control`='AgreementText'");
                tempView.Execute(null);
                tempView.Close();
                tempView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the ModuleSignature table.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadModuleSignature(Database Database, Type InstallerType, Object InstallerObject)
        {
            if (msm.id != null)
            {
                View modsigView = Database.OpenView("SELECT * FROM `ModuleSignature`");

                string id = msm.id;
                int language = Convert.ToInt32(msm.language);
                string version = msm.version;
              
                Log(Level.Info, LogPrefix + "Storing Module Signature:\n\tId:\t\t" + id + "\n\tVersion:\t" + version + "\n\tLanguage:\t" + language);
               
                Record recModSig = (Record)InstallerType.InvokeMember(
                    "CreateRecord", 
                    BindingFlags.InvokeMethod, 
                    null, InstallerObject, 
                    new object[] { 3 });


                recModSig.set_StringData(1, id);
                recModSig.set_IntegerData(2, language);
                recModSig.set_StringData(3, version);
                modsigView.Modify(MsiViewModify.msiViewModifyMerge, recModSig);

                modsigView.Close();
                modsigView = null;
            }
            return true;
        }


        /// <summary>
        /// Loads records for the ModuleDependency table.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadModuleDependency(Database Database, Type InstallerType, Object InstallerObject)
        {
            if (msm.moduledependencies != null)
            {
                View modDepView = Database.OpenView("SELECT * FROM `ModuleDependency`");

                string id = msm.id;
                int language = Convert.ToInt32(msm.language);

                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Adding Module Dependencies:");
                }

                // Add properties from Task definition
                foreach (MSMModuleDependency dependency in msm.moduledependencies)
                {
                    // Insert the Property
                    Record recModDep = (Record)InstallerType.InvokeMember(
                        "CreateRecord", 
                        BindingFlags.InvokeMethod, 
                        null, InstallerObject, 
                        new object[] { 5 });

                    string requiredId = dependency.id;
                    int requiredLang = Convert.ToInt32(dependency.language);
                    string requiredVersion = dependency.version;

                    if (requiredId == null || requiredId == "")
                    {
                        Log(Level.Error, LogPrefix + 
                            "ERROR: Dependency with no id attribute detected.");
                        return false;
                    }

                    recModDep.set_StringData(1, id);
                    recModDep.set_IntegerData(2, language);
                    recModDep.set_StringData(3, requiredId);
                    recModDep.set_IntegerData(4, requiredLang);
                    recModDep.set_StringData(5, requiredVersion);

                    modDepView.Modify(MsiViewModify.msiViewModifyMerge, recModDep);

                    if (Verbose)
                    {
                        Log(Level.Info, "\t" + requiredId);
                    }

                }

                modDepView.Close();
                modDepView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the ModuleExclusion table.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadModuleExclusion(Database Database, Type InstallerType, Object InstallerObject)
        {
            if (msm.moduleexclusions != null)
            {
                View modExView = Database.OpenView("SELECT * FROM `ModuleExclusion`");

                string id = msm.id;
                int language = Convert.ToInt32(msm.language);

                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Adding Module Exclusions:");
                }

                // Add properties from Task definition
                foreach (MSMModuleExclusion exclusion in msm.moduleexclusions)
                {
                    // Insert the Property
                    Record recModEx = (Record)InstallerType.InvokeMember(
                        "CreateRecord", 
                        BindingFlags.InvokeMethod, 
                        null, InstallerObject, 
                        new object[] { 6 });

                    string excludedId = exclusion.id;
                    int excludedLang = Convert.ToInt32(exclusion.language);
                    string excludedMinVersion = exclusion.minversion;
                    string excludedMaxVersion = exclusion.maxversion;

                    if (excludedId == null || excludedId == "")
                    {
                        Log(Level.Error, LogPrefix + 
                            "ERROR: Exclusion with no id attribute detected.");
                        return false;
                    }

                    recModEx.set_StringData(1, id);
                    recModEx.set_IntegerData(2, language);
                    recModEx.set_StringData(3, excludedId);
                    recModEx.set_IntegerData(4, excludedLang);
                    recModEx.set_StringData(5, excludedMinVersion);
                    recModEx.set_StringData(6, excludedMaxVersion);

                    modExView.Modify(MsiViewModify.msiViewModifyMerge, recModEx);

                    if (Verbose)
                    {
                        Log(Level.Info, "\t" + excludedId);
                    }

                }

                modExView.Close();
                modExView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the ModuleInstallUISequence, ModuleInstallExecuteSequence,
        /// ModuleAdminUISequence, ModuleAdminExecute, and ModuleAdvtExecuteSequence tables.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadModuleSequence(Database Database, Type InstallerType, Object InstallerObject)
        {
            // Add custom actions from Task definition
            if (msm.modulesequences != null)
            {
                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Adding Module Install/Admin Sequences:");
                }

                // Open the sequence tables
                View installExecuteView = Database.OpenView("SELECT * FROM `ModuleInstallExecuteSequence`");
                View installUIView = Database.OpenView("SELECT * FROM `ModuleInstallUISequence`");
                View adminExecuteView = Database.OpenView("SELECT * FROM `ModuleAdminExecuteSequence`");
                View adminUIView = Database.OpenView("SELECT * FROM `ModuleAdminUISequence`");
                View advtExecuteView = Database.OpenView("SELECT * FROM `ModuleAdvtExecuteSequence`");

                // Add binary data from Task definition
                foreach (MSMModuleSequence sequence in msm.modulesequences)
                {
                    if (Verbose)
                    {
                        Log(Level.Info, "\t" + sequence.action + " to the module" + sequence.type.ToString() + "sequence table.");
                    }

                    // Insert the record to the respective table
                    Record recSequence = (Record)InstallerType.InvokeMember(
                        "CreateRecord", 
                        BindingFlags.InvokeMethod, 
                        null, InstallerObject, 
                        new object[] { 5 });

                    recSequence.set_StringData(1, sequence.action);
                    recSequence.set_IntegerData(2, Convert.ToInt32(sequence.sequence));
                    recSequence.set_StringData(3, sequence.baseaction);
                    recSequence.set_IntegerData(4, Convert.ToInt32(sequence.after));
                    recSequence.set_StringData(5, sequence.condition);
                    switch(sequence.type.ToString())
                    {
                        case "installexecute":
                            installExecuteView.Modify(MsiViewModify.msiViewModifyMerge, recSequence);
                            break;
                        case "installui":
                            installUIView.Modify(MsiViewModify.msiViewModifyMerge, recSequence);
                            break;
                        case "adminexecute":
                            adminExecuteView.Modify(MsiViewModify.msiViewModifyMerge, recSequence);
                            break;
                        case "adminui":
                            adminUIView.Modify(MsiViewModify.msiViewModifyMerge, recSequence);
                            break;
                        case "advtexecute":
                            advtExecuteView.Modify(MsiViewModify.msiViewModifyMerge, recSequence);
                            break;
                    }
                }
                installExecuteView.Close();
                installUIView.Close();
                adminExecuteView.Close();
                adminUIView.Close();
                advtExecuteView.Close();

                installExecuteView = null;
                installUIView = null;
                adminExecuteView = null;
                adminUIView = null;
                advtExecuteView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the ModuleIgnoreTable table.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadModuleIgnoreTable(Database Database, Type InstallerType, Object InstallerObject)
        {
            if (msm.moduleignoretables != null)
            {
                View modIgnoreTableView = Database.OpenView("SELECT * FROM `ModuleIgnoreTable`");

                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Adding Tables To Ignore:");
                }

                // Add properties from Task definition
                foreach (MSMModuleIgnoreTable table in msm.moduleignoretables)
                {
                    // Insert the Property
                    Record recModIgnoreTable = (Record)InstallerType.InvokeMember(
                        "CreateRecord", 
                        BindingFlags.InvokeMethod, 
                        null, InstallerObject, 
                        new object[] { 1 });

                    string tableName = table.name;

                    if (tableName == null || tableName == "")
                    {
                        Log(Level.Error, LogPrefix + 
                            "ERROR: Table with no name attribute detected.");
                        return false;
                    }

                    recModIgnoreTable.set_StringData(1, tableName);

                    modIgnoreTableView.Modify(MsiViewModify.msiViewModifyMerge, recModIgnoreTable);

                    if (Verbose)
                    {
                        Log(Level.Info, "\t" + tableName);
                    }

                }

                modIgnoreTableView.Close();
                modIgnoreTableView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the ModuleSubstitution table.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadModuleSubstitution(Database Database, Type InstallerType, Object InstallerObject)
        {
            if (msm.modulesubstitutions != null)
            {
                View modSubstitutionView = Database.OpenView("SELECT * FROM `ModuleSubstitution`");

                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Adding Module Substitutions:");
                }

                // Add properties from Task definition
                foreach (MSMModuleSubstitution substitution in msm.modulesubstitutions)
                {
                    // Insert the Property
                    Record recModSubstitutionTable = (Record)InstallerType.InvokeMember(
                        "CreateRecord", 
                        BindingFlags.InvokeMethod, 
                        null, InstallerObject, 
                        new object[] { 4 });

                    string tableName = substitution.table;
                    string row = substitution.row;
                    string column = substitution.column;
                    string newValue = substitution.value;

                    if (tableName == null || tableName == "")
                    {
                        Log(Level.Error, LogPrefix + 
                            "ERROR: Substitution with no table attribute detected.");
                        return false;
                    }

                    recModSubstitutionTable.set_StringData(1, tableName);
                    recModSubstitutionTable.set_StringData(2, row);
                    recModSubstitutionTable.set_StringData(3, column);
                    recModSubstitutionTable.set_StringData(4, newValue);

                    modSubstitutionView.Modify(MsiViewModify.msiViewModifyMerge, recModSubstitutionTable);

                    if (Verbose)
                    {
                        Log(Level.Info, "\tRow: " + row + "\tColumn: " + column);
                    }

                }

                modSubstitutionView.Close();
                modSubstitutionView = null;
            }

            return true;
            
        }

        /// <summary>
        /// Loads records for the ModuleConfiguration table.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadModuleConfiguration(Database Database, Type InstallerType, Object InstallerObject)
        {
            if (msm.moduleconfigurations != null)
            {
                View modConfigurationView = Database.OpenView("SELECT * FROM `ModuleConfiguration`");

                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Adding Module Configurations:");
                }

                // Add properties from Task definition
                foreach (MSMModuleConfiguration configuration in msm.moduleconfigurations)
                {
                    // Insert the Property
                    Record recModConfigurationTable = (Record)InstallerType.InvokeMember(
                        "CreateRecord", 
                        BindingFlags.InvokeMethod, 
                        null, InstallerObject, 
                        new object[] { 10 });

                    string name = configuration.name;
                    int format = 0;

                    switch (configuration.format.ToString())
                    {
                        case "text":
                            format = 0;
                            break;
                        case "key":
                            format = 1;
                            break;
                        case "integer":
                            format = 2;
                            break;
                        case "bitfield":
                            format = 3;
                            break;
                    }

                    string type = configuration.type;
                    string contextData = configuration.contextdata;
                    string defaultValue = configuration.defaultvalue;
                    int attr = Convert.ToInt32(configuration.attr);
                    string displayName = configuration.displayname;
                    string description = configuration.description;
                    string helpLocation = configuration.helplocation;
                    string helpKeyword = configuration.helpkeyword;

                    if (name == null || name == "")
                    {
                        Log(Level.Error, LogPrefix + 
                            "ERROR: Configuration with no name attribute detected.");
                        return false;
                    }

                    recModConfigurationTable.set_StringData(1, name);
                    recModConfigurationTable.set_IntegerData(2, format);
                    recModConfigurationTable.set_StringData(3, type);
                    recModConfigurationTable.set_StringData(4, contextData);
                    recModConfigurationTable.set_StringData(5, defaultValue);
                    recModConfigurationTable.set_IntegerData(6, attr);
                    recModConfigurationTable.set_StringData(7, displayName);
                    recModConfigurationTable.set_StringData(8, description);
                    recModConfigurationTable.set_StringData(9, helpLocation);
                    recModConfigurationTable.set_StringData(10, helpKeyword);

                    modConfigurationView.Modify(MsiViewModify.msiViewModifyMerge, recModConfigurationTable);

                    if (Verbose)
                    {
                        Log(Level.Info, "\t" + name);
                    }

                }

                modConfigurationView.Close();
                modConfigurationView = null;
            }

            return true;
            
        }

        /// <summary>
        /// Loads records for the Properties table.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadProperties(Database Database, Type InstallerType, Object InstallerObject)
        {
            // Select the "Property" Table
            View propView = Database.OpenView("SELECT * FROM `Property`");

            if (Verbose)
            {
                Log(Level.Info, LogPrefix + "Adding Properties:");
            }

            // Add properties from Task definition
            foreach (property property in msm.properties)
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
                    Log(Level.Error, LogPrefix + 
                        "ERROR: Property with no name attribute detected.");
                    return false;
                }

                if (sValue == null || sValue == "")
                {
                    Log(Level.Error, LogPrefix + 
                        "ERROR: Property " + name + 
                        " has no value.");
                    return false;
                }

                recProp.set_StringData(1, name);
                recProp.set_StringData(2, sValue);
                propView.Modify(MsiViewModify.msiViewModifyMerge, recProp);

                if (Verbose)
                {
                    Log(Level.Info, "\t" + name);
                }

            }
            propView.Close();
            propView = null;

            return true;
        }

        /// <summary>
        /// Loads records for the Components table.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <param name="LastSequence">The sequence number of the last file in the .cab</param>
        /// <param name="MsiAssemblyView">View containing the MsiAssembly table.</param>
        /// <param name="MsiAssemblyNameView">View containing the MsiAssemblyName table.</param>
        /// <param name="DirectoryView">The MSM database view.</param>
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

            // Open the "ModuleComponents" Table
            View modComponentView = Database.OpenView("SELECT * FROM `ModuleComponents`");

            // Add components from Task definition
            int componentIndex = 0;

            if (Verbose)
            {
                Log(Level.Info, LogPrefix + "Add Files:");
            }

            if (msm.components != null)
            {
                foreach (MSMComponent component in msm.components)
                {
                    // Insert the Component
                    Record recComp = (Record)InstallerType.InvokeMember(
                        "CreateRecord", 
                        BindingFlags.InvokeMethod, 
                        null, InstallerObject, 
                        new object[] { 6 });

                    recComp.set_StringData(1, component.name);
                    recComp.set_StringData(2, component.id);
                    recComp.set_StringData(3, component.directory);
                    recComp.set_IntegerData(4, component.attr);

                    featureComponents.Add(component.name, component.feature);

                    // Add ModuleComponent
                    Record recModComp = (Record)InstallerType.InvokeMember(
                        "CreateRecord", 
                        BindingFlags.InvokeMethod, 
                        null, InstallerObject, 
                        new object[] { 3 });

                    recModComp.set_StringData(1, component.name);
                    recModComp.set_StringData(2, msm.id);
                    recModComp.set_IntegerData(3, Convert.ToInt32(msm.language));
                    modComponentView.Modify(MsiViewModify.msiViewModifyMerge, recModComp);

                    componentIndex++;

                    bool success = AddFiles(Database, DirectoryView, component, 
                        fileView, InstallerType, InstallerObject, 
                        component.directory, component.name, ref componentIndex, 
                        ref LastSequence, MsiAssemblyView, MsiAssemblyNameView, 
                        compView, featCompView, ClassView, ProgIdView, selfRegView, modComponentView);

                    if (!success)
                    {
                        return success;
                    }

                    if (files.Contains(component.directory + "|" + component.key.file))
                    {
                        string keyFileName = (string)files[component.directory + "|" + component.key.file];
                        if (keyFileName == "KeyIsDotNetAssembly")
                        {
                            Log(Level.Error, LogPrefix + "ERROR: Cannot specify key '" + component.key.file + 
                                "' for component '" + component.name + "'. File has been detected as " + 
                                "being a COM component or Microsoft.NET assembly and is " + 
                                "being registered with its own component. Please specify " + 
                                "a different file in the same directory for this component's key.");
                            return false;
                        }
                        else
                        {
                            recComp.set_StringData(6, keyFileName);
                            compView.Modify(MsiViewModify.msiViewModifyMerge, recComp);
                        }
                    }
                    else
                    {
                        Log(Level.Error, 
                            LogPrefix + "ERROR: KeyFile \"" + component.key.file + 
                            "\" not found in Component \"" + component.name + "\".");
                        return false;
                    }
                }

                // Add featureComponents from Task definition
                IEnumerator keyEnum = featureComponents.Keys.GetEnumerator();

                while (keyEnum.MoveNext())
                {
                    string component = Properties.ExpandProperties((string)keyEnum.Current, Location);
                    string feature = Properties.ExpandProperties((string)featureComponents[component], Location);
                    
                    // Insert the FeatureComponent
                    Record recFeatComps = (Record)InstallerType.InvokeMember(
                        "CreateRecord", 
                        BindingFlags.InvokeMethod, 
                        null, InstallerObject, 
                        new object[] { 2 });

                    recFeatComps.set_StringData(1, feature);
                    recFeatComps.set_StringData(2, component);
                    featCompView.Modify(MsiViewModify.msiViewModifyMerge, recFeatComps);
                }

            }

            compView.Close();
            fileView.Close();
            featCompView.Close();
            selfRegView.Close();
            modComponentView.Close();

            compView = null;
            fileView = null;
            featCompView = null;
            selfRegView = null;
            modComponentView = null;

            return true;
        }

        /// <summary>
        /// Loads records for the Directories table.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <param name="DirectoryView">The MSM database view.</param>
        /// <returns>True if successful.</returns>
        private bool LoadDirectories(Database Database, Type InstallerType, 
            Object InstallerObject, out View DirectoryView)
        {
                // Open the "Directory" Table
                DirectoryView = Database.OpenView("SELECT * FROM `Directory`");

            if (msm.directories != null)
            {
                ArrayList directoryList = new ArrayList(msm.directories);       

                MSMRootDirectory targetDir = new MSMRootDirectory();
                targetDir.name = "TARGETDIR";
                targetDir.root = "";
                targetDir.foldername = "SourceDir";
                directoryList.Add(targetDir);

                // Insert the Common Directories
                for (int i = 0; i < commonFolderNames.Length; i++)
                {
                    MSMRootDirectory commonDir = new MSMRootDirectory();
                    commonDir.name = commonFolderNames[i];
                    commonDir.root = "TARGETDIR";
                    commonDir.foldername = ".";
                    directoryList.Add(commonDir);
                }

                MSMRootDirectory[] directories = new MSMRootDirectory[directoryList.Count];
                directoryList.CopyTo(directories);
                msm.directories = directories;

                int depth = 1;

                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Adding Directories:");
                }

                // Add directories from Task definition
                foreach (MSMRootDirectory directory in msm.directories)
                {
                    bool result = AddDirectory(Database, 
                        DirectoryView, null, InstallerType, 
                        InstallerObject, directory, depth);

                    if (!result)
                    {
                        DirectoryView.Close();
                        DirectoryView = null;
                        return result;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Adds a directory record to the directories table.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="DirectoryView">The MSM database view.</param>
        /// <param name="ParentDirectory">The parent directory.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <param name="Directory">This directory's Schema object.</param>
        /// <param name="Depth">The tree depth of this directory.</param>
        /// <returns></returns>
        private bool AddDirectory(Database Database, View DirectoryView, 
            string ParentDirectory, 
            Type InstallerType, object InstallerObject, 
            MSMDirectory Directory, int Depth)
        {
            string newParent = ParentDirectory;
            if (Directory is MSMRootDirectory)
            {
                newParent = ((MSMRootDirectory)Directory).root;
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

            string basePath = Path.Combine(Project.BaseDirectory, msm.sourcedir);
            string fullPath = Path.Combine(basePath, relativePath.ToString());
            string path = GetShortPath(fullPath) + "|" + Directory.foldername;
            if (Directory.foldername == ".")
                path = Directory.foldername;

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
                Log(Level.Info, "\t" + 
                    Path.Combine(Project.BaseDirectory, Path.Combine(msm.sourcedir, relativePath.ToString())));
            }
            
            recDir.set_StringData(3, path);

            DirectoryView.Modify(MsiViewModify.msiViewModifyMerge, recDir);

            if (Directory.directory != null)
            {
                foreach (MSMDirectory childDirectory in Directory.directory)
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
        /// Loads properties for the Summary Information Stream.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <returns>True if successful.</returns>
        private bool LoadSummaryInfo(Database Database)
        {
            property productName = null;
            property manufacturer = null;
            property keywords = null;
            property comments = null;

            foreach (property prop in msm.properties)
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
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadEnvironment(Database Database, Type InstallerType, Object InstallerObject)
        {
            // Open the "Environment" Table
            View envView = Database.OpenView("SELECT * FROM `Environment`");

            if (msm.environment != null)
            {
                foreach (MSMVariable variable in msm.environment)
                {
                    // Insert the Varible
                    Record recVar = (Record)InstallerType.InvokeMember(
                        "CreateRecord", 
                        BindingFlags.InvokeMethod, 
                        null, InstallerObject, 
                        new object[] { 4 });

                    recVar.set_StringData(1, "_" + Guid.NewGuid().ToString().ToUpper().Replace("-", null));
                    recVar.set_StringData(2, variable.name);

                    if (variable.append != null && variable.append != "")
                    {
                        recVar.set_StringData(3, "[~];" + variable.append);
                    }

                    recVar.set_StringData(4, variable.component);

                    envView.Modify(MsiViewModify.msiViewModifyMerge, recVar);
                }
            }
            envView.Close();
            envView = null;

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
        /// <param name="Database">The MSM database.</param>
        /// <param name="DirectoryView">The MSM database view.</param>
        /// <param name="Component">The Component's XML Element.</param>
        /// <param name="FileView">The MSM database view.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
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
        /// <param name="modComponentView">View containing the ModuleComponent table.</param>
        /// <returns>True if successful.</returns>
        private bool AddFiles(Database Database, View DirectoryView, MSMComponent Component, 
            View FileView, Type InstallerType, Object InstallerObject, 
            string ComponentDirectory, string ComponentName, ref int ComponentCount, 
            ref int Sequence, View MsiAssemblyView, View MsiAssemblyNameView, 
            View ComponentView, View FeatureComponentView, View ClassView, View ProgIdView, 
            View SelfRegView, View modComponentView)
        {

            XmlElement fileSetElem = (XmlElement)((XmlElement)_xmlNode).SelectSingleNode(
                "components/component[@id='" + Component.id + "']/fileset");

            FileSet componentFiles = new FileSet();
            componentFiles.Project = Project;
            componentFiles.Initialize(fileSetElem);

            MSMDirectory componentDirInfo = FindDirectory(ComponentDirectory);
            
            StringBuilder relativePath = new StringBuilder();

            string newParent = null;
            if (componentDirInfo is MSMRootDirectory)
            {
                newParent = ((MSMRootDirectory)componentDirInfo).root;
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

            string basePath = Path.Combine(Project.BaseDirectory, msm.sourcedir);
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
                
                MSMFileOverride fileOverride = null;

                if (Component.forceid != null)
                {
                    foreach (MSMFileOverride curOverride in Component.forceid)
                    {
                        if (curOverride.file == fileName)
                        {
                            fileOverride = curOverride;
                            break;
                        }
                    }
                }

                string fileId = fileOverride == null ? 
                    "_" + Guid.NewGuid().ToString().ToUpper().Replace("-", null) :
                    fileOverride.id;

                files.Add(Component.directory + "|" + fileName, fileId);
                recFile.set_StringData(1, fileId);
            
                if (File.Exists(filePath))
                {
                    try
                    {
                        recFile.set_StringData(4, new FileInfo(filePath).Length.ToString());
                    }
                    catch (Exception)
                    {
                        Log(Level.Error, LogPrefix + 
                            "ERROR: Could not open file " + filePath);
                        return false;
                    }
                }
                else
                {
                    Log(Level.Error, LogPrefix + 
                        "ERROR: Could not open file " + filePath);
                    return false;
                }

                if (Verbose)
                {
                    Log(Level.Info, "\t" + 
                        Path.Combine(Project.BaseDirectory, Path.Combine(Path.Combine(msm.sourcedir, 
                        relativePath.ToString()), fileName)));
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
                        ComponentView.Modify(MsiViewModify.msiViewModifyMerge, recComp);

                        // Map the new Component to the existing one's Feature
                        Record featComp = (Record)InstallerType.InvokeMember(
                            "CreateRecord", 
                            BindingFlags.InvokeMethod, 
                            null, InstallerObject, 
                            new object[] { 2 });

                        featComp.set_StringData(1, (string)featureComponents[ComponentName]);
                        featComp.set_StringData(2, asmCompName);
                        FeatureComponentView.Modify(MsiViewModify.msiViewModifyMerge, featComp);

                        // Add the new component to the modulecomponents table
                        Record recModComp = (Record)InstallerType.InvokeMember(
                            "CreateRecord", 
                            BindingFlags.InvokeMethod, 
                            null, InstallerObject, 
                            new object[] { 3 });

                        recModComp.set_StringData(1, asmCompName);
                        recModComp.set_StringData(2, msm.id);
                        recModComp.set_IntegerData(3, Convert.ToInt32(msm.language));
                        modComponentView.Modify(MsiViewModify.msiViewModifyMerge, recModComp);
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
                        MsiAssemblyView.Modify(MsiViewModify.msiViewModifyMerge, recAsm);

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
                            MsiAssemblyNameView.Modify(MsiViewModify.msiViewModifyMerge, recAsmName);
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
                            MsiAssemblyNameView.Modify(MsiViewModify.msiViewModifyMerge, recAsmVersion);
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
                            MsiAssemblyNameView.Modify(MsiViewModify.msiViewModifyMerge, recAsmLocale);
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
                            MsiAssemblyNameView.Modify(MsiViewModify.msiViewModifyMerge, recPublicKey);
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
                            Log(Level.Info, LogPrefix + 
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
                            SelfRegView.Modify(MsiViewModify.msiViewModifyMerge, recSelfReg);
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
                        Path.Combine(msm.sourcedir, "Temp"));

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
                    recFile.set_StringData(2, Component.name);
                }

                recFile.set_StringData(3, GetShortFile(filePath) + "|" + fileName);
                recFile.set_StringData(5, null);    // Version
                recFile.set_StringData(6, null);
                recFile.set_StringData(7, "512");
                
                Sequence++;
                
                recFile.set_StringData(8, Sequence.ToString());
                FileView.Modify(MsiViewModify.msiViewModifyMerge, recFile);
            }
            return true;
        }

        /// <summary>
        /// Loads records for the Registry table.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <param name="RegistryView">View containing the Registry table.</param>
        /// <returns>True if successful.</returns>
        private bool LoadRegistry(Database Database, Type InstallerType, 
            Object InstallerObject, out View RegistryView)
        {
            // Open the "Registry" Table
            RegistryView = Database.OpenView("SELECT * FROM `Registry`");

            if (msm.registry != null)
            {
                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Adding Registry Values:");
                }

                foreach (MSMRegistryKey key in msm.registry)
                {
                    int rootKey = -1;
                    switch (key.root.ToString())
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

                    foreach (MSMRegistryKeyValue value in key.value)
                    {
                        // Insert the Value
                        Record recVal = (Record)InstallerType.InvokeMember(
                            "CreateRecord", 
                            BindingFlags.InvokeMethod, 
                            null, InstallerObject, 
                            new object[] { 6 });

                        recVal.set_StringData(1, "_" + 
                            Guid.NewGuid().ToString().ToUpper().Replace("-", null));
                        recVal.set_StringData(2, rootKey.ToString());
                        recVal.set_StringData(3, key.path);
                        recVal.set_StringData(4, value.name);

                        if (Verbose)
                        {
                            string keypath = GetDisplayablePath(key.path);
                            Log(Level.Info, "\t" + keypath + @"#" + value.name);
                        }

                        if (value.value != null && value.value != "")
                        {
                            recVal.set_StringData(5, value.value);
                        }
                        else if (value.dword != null && value.dword != "")
                        {
                            string sDwordMsi = "#" + Int32.Parse(value.dword);
                            recVal.set_StringData(5, sDwordMsi);
                        }
                        else
                        {
                            string val1 = value.Value.Replace(",", null);
                            string val2 = val1.Replace(" ", null);
                            string val3 = val2.Replace("\n", null);
                            string val4 = val3.Replace("\r", null);
                            recVal.set_StringData(5, "#x" + val4);
                        }

                        recVal.set_StringData(6, key.component);
                        RegistryView.Modify(MsiViewModify.msiViewModifyMerge, recVal);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Creates the assembly and assembly name tables.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
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
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadRegLocator(Database Database, Type InstallerType, 
            Object InstallerObject)
        {
            // Add properties from Task definition
            if (msm.search != null)
            {
                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Adding Locators:");
                }

                foreach (searchKey key in msm.search)
                {
                    switch (key.type.ToString())
                    {
                        case "registry":
                        {
                            // Select the "RegLocator" Table
                            View regLocatorView = Database.OpenView("SELECT * FROM `RegLocator`");

                            int rootKey = -1;
                            switch (key.root.ToString())
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

                            if (key.value != null)
                            {
                                foreach (searchKeyValue value in key.value)
                                {
                                    string signature = "SIG_" + value.setproperty;

                                    // Insert the signature to the RegLocator Table
                                    Record recRegLoc = (Record)InstallerType.InvokeMember(
                                        "CreateRecord", 
                                        BindingFlags.InvokeMethod, 
                                        null, InstallerObject, 
                                        new object[] { 5 });

                                    recRegLoc.set_StringData(1, signature);
                                    recRegLoc.set_StringData(2, rootKey.ToString());
                                    recRegLoc.set_StringData(3, key.path);
                                    recRegLoc.set_StringData(4, value.name);
                                    // 2 represents msidbLocatorTypeRawValue
                                    recRegLoc.set_IntegerData(5, 2);

                                    regLocatorView.Modify(MsiViewModify.msiViewModifyMerge, recRegLoc);

                                    if (Verbose)
                                    {
                                        string path = GetDisplayablePath(key.path);
                                        Log(Level.Info, "\t" + path + @"#" + value.name);
                                    }
                                }
                            }
                            regLocatorView.Close();
                            regLocatorView = null;

                            break;
                        }
                        case "file":
                        {
                            break;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Loads records for the RegLocator table
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadAppSearch(Database Database, Type InstallerType, 
            Object InstallerObject)
        {
            // Add properties from Task definition
            if (msm.search != null)
            {
                foreach (searchKey key in msm.search)
                {
                    switch (key.type.ToString())
                    {
                        case "registry":
                        {
                            // Select the "AppSearch" Table
                            View appSearchView = Database.OpenView("SELECT * FROM `AppSearch`");
                                
                            if (key.value != null)
                            {
                                foreach (searchKeyValue value in key.value)
                                {
                                    string signature = "SIG_" + value.setproperty;

                                    // Insert the Property/Signature into AppSearch Table
                                    Record recAppSearch = (Record)InstallerType.InvokeMember(
                                        "CreateRecord", 
                                        BindingFlags.InvokeMethod, 
                                        null, InstallerObject, 
                                        new object[] { 2 });

                                    recAppSearch.set_StringData(1, value.setproperty);
                                    recAppSearch.set_StringData(2, signature);

                                    appSearchView.Modify(MsiViewModify.msiViewModifyMerge, recAppSearch);
                                }
                            }
                            appSearchView.Close();
                            appSearchView = null;

                            break;
                        }
                        case "file":
                        {
                            break;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Loads records for the Icon table.  
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadIcon(Database Database, Type InstallerType, 
            Object InstallerObject)
        {
            if (msm.icons != null)
            {

                // Open the Icon Table
                View iconView = Database.OpenView("SELECT * FROM `Icon`");

                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Adding Icon Data:");
                }
                
                // Add binary data from Task definition
                foreach (MSMIcon icon in msm.icons)
                {
                    if (Verbose)
                    {
                        Log(Level.Info, "\t" + Path.Combine(Project.BaseDirectory, icon.value));
                    }

                    if (File.Exists(Path.Combine(Project.BaseDirectory, icon.value)))
                    {
                        // Insert the icon data
                        Record recIcon = (Record)InstallerType.InvokeMember(
                            "CreateRecord", 
                            BindingFlags.InvokeMethod, 
                            null, InstallerObject, 
                            new object[] { 2 });

                        recIcon.set_StringData(1, icon.name);
                        recIcon.SetStream(2, Path.Combine(Project.BaseDirectory, icon.value));
                        iconView.Modify(MsiViewModify.msiViewModifyMerge, recIcon);

                    }
                    else
                    {
                        Log(Level.Error, LogPrefix + 
                            "ERROR: Unable to open file:\n\n\t" + 
                            Path.Combine(Project.BaseDirectory, icon.value) + "\n\n");

                        iconView.Close();
                        iconView = null;
                        return false;
                    }
                }

                iconView.Close();
                iconView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the Shortcut table.  
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadShortcut(Database Database, Type InstallerType, 
            Object InstallerObject)
        {
            // Add properties from Task definition
            if (msm.shortcuts != null)
            {
                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Adding Shortcuts:");
                }

                View shortcutView = Database.OpenView("SELECT * FROM `Shortcut`");

                foreach (MSMShortcut shortcut in msm.shortcuts)
                {
                    if (Verbose)
                    {
                        Log(Level.Info, "\t" + shortcut.name);
                    }

                    // Insert the record into the table
                    Record shortcutRec = (Record)InstallerType.InvokeMember(
                        "CreateRecord", 
                        BindingFlags.InvokeMethod, 
                        null, InstallerObject, 
                        new object[] { 12 });

                    shortcutRec.set_StringData(1, shortcut.name);
                    shortcutRec.set_StringData(2, shortcut.directory);
                    shortcutRec.set_StringData(3, shortcut.filename);
                    shortcutRec.set_StringData(4, shortcut.component);
                    shortcutRec.set_StringData(5, shortcut.target);
                    shortcutRec.set_StringData(6, shortcut.arguments);
                    shortcutRec.set_StringData(7, shortcut.description);
                    shortcutRec.set_StringData(8, shortcut.hotkey);
                    shortcutRec.set_StringData(9, shortcut.icon);
                    shortcutRec.set_IntegerData(10, shortcut.iconindex);
                    shortcutRec.set_IntegerData(11, shortcut.showcmd);
                    shortcutRec.set_StringData(12, shortcut.wkdir);

                    shortcutView.Modify(MsiViewModify.msiViewModifyMerge, shortcutRec);

                }
                shortcutView.Close();
                shortcutView = null;
            }
            return true;

        }

        /// <summary>
        /// Adds custom table(s) to the msi database
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool AddTables(Database Database, Type InstallerType, 
            Object InstallerObject)
        {
            // Add properties from Task definition
            if (msm.tables != null)
            {
                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Adding Tables:");
                }

                foreach (MSMTable table in msm.tables)
                {
                    if (Verbose)
                    {
                        Log(Level.Info, "\t" + table.name);
                    }
                                    
                    bool tableExists = true;
                    try
                    {
                        View tableView = Database.OpenView("SELECT * FROM `" + table.name + "`");
                    
                        if (Verbose)
                        {
                            Log(Level.Info, "\t\tTable exists.. skipping");
                        }
                        tableExists = true;
                        tableView.Close();
                        tableView = null;
                    }
                    catch (Exception)
                    {
                        tableExists = false;
                    }

                    if (!tableExists)
                    {
                        if (Verbose)
                        {
                            Log(Level.Info, "\t\tAdding table structure...");
                        }

                        View validationView = Database.OpenView("SELECT * FROM `_Validation`");

                        string tableStructureColumns = "";
                        string tableStructureColumnTypes = "";
                        string tableStructureKeys = table.name;
                        bool firstColumn = true;

                        ArrayList columnList = new ArrayList();

                        foreach (MSMTableColumn column in table.columns)
                        {
                            // Add this column to the column list
                            MSMRowColumnData currentColumn = new MSMRowColumnData();

                            currentColumn.name = column.name;
                            currentColumn.id = columnList.Count;

                            Record recValidation = (Record)InstallerType.InvokeMember(
                                "CreateRecord", 
                                BindingFlags.InvokeMethod, 
                                null, InstallerObject, 
                                new object[] { 10 });

                            recValidation.set_StringData(1, table.name);
                            recValidation.set_StringData(2, column.name);
                            if (column.nullable)
                                recValidation.set_StringData(3, "Y");
                            else
                                recValidation.set_StringData(3, "N");
                            recValidation.set_StringData(4, column.minvalue);
                            recValidation.set_StringData(5, column.maxvalue);
                            recValidation.set_StringData(6, column.keytable);
                            recValidation.set_StringData(7, column.keycolumn);


                            if (!firstColumn)
                            {
                                tableStructureColumns += "\t";
                                tableStructureColumnTypes += "\t";
                            }
                            else
                                firstColumn = false;

                            tableStructureColumns += column.name;

                            switch(column.category.ToString())
                            {
                                case "Text":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "s0";
                                    recValidation.set_StringData(8, "Text");
                                    currentColumn.type = "string";
                                    break;
                                case "UpperCase":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "s72";
                                    recValidation.set_StringData(8, "UpperCase");
                                    currentColumn.type = "string";
                                    break;
                                case "LowerCase":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "s72";
                                    recValidation.set_StringData(8, "LowerCase");
                                    currentColumn.type = "string";
                                    break;
                                case "Integer":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "i2";
                                    recValidation.set_StringData(8, "Integer");
                                    currentColumn.type = "int";
                                    break;
                                case "DoubleInteger":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "i4";
                                    recValidation.set_StringData(8, "DoubleInteger");
                                    currentColumn.type = "int";
                                    break;
                                case "Time/Date":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "i4";
                                    recValidation.set_StringData(8, "Time/Date");
                                    currentColumn.type = "int";
                                    break;
                                case "Identifier":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "s72";
                                    recValidation.set_StringData(8, "Identifier");
                                    currentColumn.type = "string";
                                    break;
                                case "Property":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "s72";
                                    recValidation.set_StringData(8, "Property");
                                    currentColumn.type = "string";
                                    break;
                                case "Filename":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "s255";
                                    recValidation.set_StringData(8, "Filename");
                                    currentColumn.type = "string";
                                    break;
                                case "WildCardFilename":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "L0";
                                    recValidation.set_StringData(8, "WildCardFilename");
                                    currentColumn.type = "string";
                                    break;
                                case "Path":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "s255";
                                    recValidation.set_StringData(8, "Path");
                                    currentColumn.type = "string";
                                    break;
                                case "Paths":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "s255";
                                    recValidation.set_StringData(8, "Paths");
                                    currentColumn.type = "string";
                                    break;
                                case "AnyPath":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "s255";
                                    recValidation.set_StringData(8, "AnyPath");
                                    currentColumn.type = "string";
                                    break;
                                case "DefaultDir":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "l255";
                                    recValidation.set_StringData(8, "DefaultDir");
                                    currentColumn.type = "string";
                                    break;
                                case "RegPath":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "l255";
                                    recValidation.set_StringData(8, "RegPath");
                                    currentColumn.type = "string";
                                    break;
                                case "Formatted":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "s255";
                                    recValidation.set_StringData(8, "Formatted");
                                    currentColumn.type = "string";
                                    break;
                                case "Template":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "L0";
                                    recValidation.set_StringData(8, "Template");
                                    currentColumn.type = "string";
                                    break;
                                case "Condition":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "s255";
                                    recValidation.set_StringData(8, "Condition");
                                    currentColumn.type = "string";
                                    break;
                                case "GUID":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "s38";
                                    recValidation.set_StringData(8, "GUID");
                                    currentColumn.type = "string";
                                    break;
                                case "Version":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "s32";
                                    recValidation.set_StringData(8, "Version");
                                    currentColumn.type = "string";
                                    break;
                                case "Language":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "s255";
                                    recValidation.set_StringData(8, "Language");
                                    currentColumn.type = "string";
                                    break;
                                case "Binary":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "v0";
                                    recValidation.set_StringData(8, "Binary");
                                    currentColumn.type = "binary";
                                    break;
                                case "CustomSource":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "s72";
                                    recValidation.set_StringData(8, "CustomSource");
                                    currentColumn.type = "string";
                                    break;
                                case "Cabinet":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "S255";
                                    recValidation.set_StringData(8, "Cabinet");
                                    currentColumn.type = "string";
                                    break;
                                case "Shortcut":
                                    if (column.type == null || column.type == "")
                                        tableStructureColumnTypes += "s72";
                                    recValidation.set_StringData(8, "Shortcut");
                                    currentColumn.type = "string";
                                    break;
                                default:
                                    if (column.type == null || column.type == "")
                                    {
                                        if (Verbose)
                                        {
                                            Log(Level.Info, " ");
                                            Log(Level.Info, LogPrefix + "Must specify a valid category or type.  Defaulting to category type: s0");
                                        }
                                        tableStructureColumnTypes += "s0";
                                        currentColumn.type = "string";
                                    }
                                    break;
                            }
                            if (column.type != null)
                            {
                                tableStructureColumnTypes += column.type;
                                if (column.type.ToString().StartsWith("i"))
                                    currentColumn.type = "int";
                                else if(column.type.ToString().StartsWith("v"))
                                    currentColumn.type = "binary";
                                else
                                    currentColumn.type = "string";
                            }

                            recValidation.set_StringData(9, column.set);
                            recValidation.set_StringData(10, column.description);

                            if (column.key)
                                tableStructureKeys += "\t" + column.name;                            

                            validationView.Modify(MsiViewModify.msiViewModifyMerge, recValidation);
                    
                            columnList.Add(currentColumn);

                        }

                        // Create temp file.  Dump table structure contents into the file
                        // Then import the file.  
                        string tableStructureContents = tableStructureColumns + "\n" + tableStructureColumnTypes + "\n" + tableStructureKeys + "\n";
                        string tempFileName = "04527004_BBA8_4cee_B4FF_D54736559260.idt";
                        string fullTempFileName = Path.Combine(Path.Combine(Project.BaseDirectory, msm.sourcedir), tempFileName);
                        FileStream tableStream = null;
                        try 
                        {                                    
                            tableStream = File.Create(fullTempFileName);
                            StreamWriter writer = new StreamWriter(tableStream);
                            writer.Write(tableStructureContents);
                            writer.Flush();
                        }
                        catch (Exception e)
                        {
                            Log(Level.Error, "Error: " + e.ToString());
                        }
                        finally 
                        {
                            tableStream.Close();
                        }

                        validationView.Close();
                        validationView = null;

                        try
                        {
                            Database.Import(Path.GetFullPath(Path.Combine(Project.BaseDirectory, msm.sourcedir)), tempFileName);
                        }
                        catch (Exception ae)
                        {
                            Log(Level.Error, LogPrefix + "ERROR: Temporary table file\n (" + Path.GetFullPath(Path.Combine(Path.Combine(Project.BaseDirectory, msm.sourcedir), tempFileName)) + ") is not valid:\n" + 
                                ae.ToString());
                        }
                        File.Delete(fullTempFileName);

                        if (Verbose)
                        {
                            Log(Level.Info, "Done");
                        }

                        if (table.rows != null)
                            AddTableData(Database, InstallerType, InstallerObject, table.name, table, columnList);

                    }
                }
            }
            return true;

        }


        /// <summary>
        /// Adds table data to the msi database table structure
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <param name="currentTable">The current table name</param>
        /// <param name="table">Xml node representing the current table</param>
        /// <param name="columnList">List of column objects for the current table (Containing: column name, id, type).</param>
        /// <returns>True if successful.</returns>
        private bool AddTableData(Database Database, Type InstallerType, 
            Object InstallerObject, string currentTable, MSMTable table, ArrayList columnList)
        {

            if (Verbose)
            {
                Log(Level.Info, "\t\tAdding table data...");
            }
            View tableView = Database.OpenView("SELECT * FROM `" + currentTable + "`");
            
            foreach (MSMTableRow row in table.rows)
            {
                Record newRec = (Record)InstallerType.InvokeMember(
                    "CreateRecord", 
                    BindingFlags.InvokeMethod, 
                    null, InstallerObject, 
                    new object[] { columnList.Count });                    
                try
                {
                    foreach(MSMTableRowColumnData columnData in row.columns)
                    {

                        // Create the record and add it
                        foreach (MSMRowColumnData columnInfo in columnList)
                        {
                            if (columnInfo.name == columnData.name)
                            {
                                if (columnInfo.type == "int")
                                {
                                    newRec.set_IntegerData((columnInfo.id + 1), Convert.ToInt32(columnData.value));
                                }
                                else if (columnInfo.type == "binary")
                                {
                                    newRec.SetStream((columnInfo.id + 1), columnData.value);
                                }
                                else
                                {
                                    newRec.set_StringData((columnInfo.id + 1), columnData.value);
                                }
                                break;
                            }
                        }
                    }                
                    tableView.Modify(MsiViewModify.msiViewModifyMerge, newRec);
                }
                catch (Exception)
                {
                    Log(Level.Error, LogPrefix + "Incorrect row data format.");
                }
            }
            tableView.Close();
            tableView = null;                    
            
            if (Verbose)
            {
                Log(Level.Info, "Done");
            }
            return true;
        }


        /// <summary>
        /// Sets the sequence number of files to match their 
        /// storage order in the cabinet file, after some 
        /// files have had their filenames changed to go in 
        /// their own component.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="LastSequence">The last file's sequence number.</param>
        /// <returns>True if successful</returns>
        private bool ReorderFiles(Database Database, ref int LastSequence)
        {
            string curPath = Path.Combine(Project.BaseDirectory, msm.sourcedir);
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

                        LastSequence++;
                    }
                    else
                    {
                        Log(Level.Info, LogPrefix + "File " + 
                            Path.GetFileName(curDirFileName) + 
                            " not found during reordering.");

                        curFileView.Close();
                        curFileView = null;

                        return false;
                    }
                }

                curFileView.Close();
                curFileView = null;
            }

            return true;
        }

        /// <summary>
        /// Creates a .cab file with all source files included.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool CreateCabFile(Database Database, Type InstallerType, Object InstallerObject)
        {
            Log(Level.Info, LogPrefix + "Compressing Files...");

            // Create the CabFile
            ProcessStartInfo processInfo = new ProcessStartInfo();

            processInfo.Arguments = "-p -r -P " + 
                Path.Combine(msm.sourcedir, "Temp") + @"\ N " + 
                Path.Combine(Project.BaseDirectory, msm.sourcedir) + @"\MergeModule.CABinet " + 
                Path.Combine(msm.sourcedir, "Temp") + @"\*";

            processInfo.CreateNoWindow = false;
            processInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processInfo.WorkingDirectory = Project.BaseDirectory;
            processInfo.FileName = "cabarc";

            Process process = new Process();
            process.StartInfo = processInfo;
            process.EnableRaisingEvents = true;

            try
            {
                process.Start();
            }
            catch (Exception e) {
                Log(Level.Error, LogPrefix + "ERROR: cabarc.exe is not in your path! \n" + e.ToString());
                return false;
            }

            try
            {
                process.WaitForExit();
            }
            catch (Exception e)
            {
                Log(Level.Error, "" );
                Log(Level.Error, "Error creating cab file: " + e.Message);
                return false;
            }

            if (process.ExitCode != 0)
            {
                Log(Level.Error, "");
                Log(Level.Error, "Error creating cab file, application returned error " + 
                    process.ExitCode + ".");
                return false;
            }

            if (!process.HasExited)
            {
                Log(Level.Info,"" );
                Log(Level.Info, "Killing the cabarc process.");
                process.Kill();
            }
            process = null;
            processInfo = null;

            Log(Level.Info, "Done.");
            
            string cabFile = Path.Combine(Project.BaseDirectory, 
                Path.Combine(msm.sourcedir, @"MergeModule.CABinet"));

            if (File.Exists(cabFile))
            {
                View cabView = Database.OpenView("SELECT * FROM `_Streams`");
                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Storing Cabinet in MSM Database...");
                }

                Record cabRecord = (Record)InstallerType.InvokeMember(
                    "CreateRecord", 
                    BindingFlags.InvokeMethod, 
                    null, InstallerObject, 
                    new object[] { 2 });

                cabRecord.set_StringData(1, Path.GetFileName(cabFile));
                cabRecord.SetStream(2, cabFile);

                cabView.Modify(MsiViewModify.msiViewModifyMerge, cabRecord);
                cabView.Close();
                cabView = null;

            }
            else
            {
                Log(Level.Error, LogPrefix + 
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
                Log(Level.Error, LogPrefix + "ERROR: Directory " + 
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
        /// the MSM directory table.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <param name="Name">The Name of the Folder</param>
        /// <param name="Parent">The Parent of the Folder</param>
        /// <param name="Default">The Relative Filesystem Path of the Folder</param>
        /// <param name="Path">The Path to the Folder from previous calls.</param>
        /// <param name="DirectoryView">The MSM database view.</param>
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

            if (msm.directories != null)
            {
                ArrayList directoryList = new ArrayList();
                foreach(MSMRootDirectory directory in msm.directories)
                {
                    directoryList.Add(directory);
                }

                foreach (property property in msm.properties)
                {
                    if (Name == property.name)
                    {
                        MSMDirectory directory = FindDirectory(Name);
                        if (directory == null)
                        {
                            MSMRootDirectory propDirectory = new MSMRootDirectory();
                            propDirectory.name = Name;
                            propDirectory.root = "TARGETDIR";
                            propDirectory.foldername = ".";

                            directoryList.Add(propDirectory);

                            MSMRootDirectory[] rootDirs = new MSMRootDirectory[directoryList.Count];
                            directoryList.CopyTo(rootDirs);

                            msm.directories = rootDirs;
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
                    MSMDirectory PathInfo = FindDirectory(Parent);

                    if (PathInfo == null)
                    {
                        foreach (property property in msm.properties)
                        {
                            if (Parent == property.name)
                            {
                                MSMRootDirectory directory = new MSMRootDirectory();
                                directory.name = Parent;
                                directory.root = "TARGETDIR";
                                directory.foldername = ".";

                                directoryList.Add(directory);

                                MSMRootDirectory[] rootDirs = new MSMRootDirectory[directoryList.Count];
                                directoryList.CopyTo(rootDirs);

                                msm.directories = rootDirs;

                                // Insert the Directory that is a Property
                                Record recDir = (Record)InstallerType.InvokeMember(
                                    "CreateRecord", 
                                    BindingFlags.InvokeMethod, 
                                    null, InstallerObject, new object[] { 3 });

                                recDir.set_StringData(1, Parent);
                                recDir.set_StringData(2, "TARGETDIR");
                                recDir.set_StringData(3, ".");

                                DirectoryView.Modify(MsiViewModify.msiViewModifyMerge, recDir);

                                PathInfo = directory;

                                break;
                            }
                        }
                    }   

                    string newParent = null;
                    if (PathInfo is MSMRootDirectory)
                    {
                        newParent = ((MSMRootDirectory)PathInfo).root;
                    }
                    else
                    {
                        newParent = FindParent(Parent);
                    }

                    GetRelativePath(Database, InstallerType, InstallerObject, 
                        Parent, newParent, 
                        PathInfo.foldername, Path, DirectoryView);
                }
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
                            attr.Value = Properties.ExpandProperties(attr.Value, Location);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Converts the Byte array in a public key 
        /// token of an assembly to a string MSM expects.
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
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
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

                        typeLibView.Modify(MsiViewModify.msiViewModifyMerge, recTypeLib);

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
                                                object custData = new object();
                                                Guid g = new Guid("0F21F359-AB84-41E8-9A78-36D110E6D2F9");
                                                typeInfo2.GetCustData(ref g, out custData);

                                                if (custData != null)
                                                {
                                                    string className = (string)custData;

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
                                                    RegistryView.Modify(MsiViewModify.msiViewModifyMerge, recRegTlbRec);

                                                    recRegTlbRec.set_StringData(1, 
                                                        "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
                                                    recRegTlbRec.set_StringData(4, "ThreadingModel");
                                                    recRegTlbRec.set_StringData(5, "Both");
                                                    RegistryView.Modify(MsiViewModify.msiViewModifyMerge, recRegTlbRec);

                                                    recRegTlbRec.set_StringData(1, 
                                                        "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
                                                    recRegTlbRec.set_StringData(4, "RuntimeVersion");
                                                    recRegTlbRec.set_StringData(5, System.Environment.Version.ToString(3));
                                                    RegistryView.Modify(MsiViewModify.msiViewModifyMerge, recRegTlbRec);

                                                    recRegTlbRec.set_StringData(1, 
                                                        "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
                                                    recRegTlbRec.set_StringData(4, "Assembly");
                                                    recRegTlbRec.set_StringData(5, tlbRecord.AssemblyName.FullName);
                                                    RegistryView.Modify(MsiViewModify.msiViewModifyMerge, recRegTlbRec);

                                                    recRegTlbRec.set_StringData(1, 
                                                        "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
                                                    recRegTlbRec.set_StringData(3,
                                                        @"CLSID\" + clsid + 
                                                        @"\Implemented Categories");
                                                    recRegTlbRec.set_StringData(4, "+");
                                                    recRegTlbRec.set_StringData(5, null);
                                                    RegistryView.Modify(MsiViewModify.msiViewModifyMerge, recRegTlbRec);

                                                    recRegTlbRec.set_StringData(1, 
                                                        "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
                                                    recRegTlbRec.set_StringData(3,
                                                        @"CLSID\" + clsid + 
                                                        @"\Implemented Categories\{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}");
                                                    recRegTlbRec.set_StringData(4, "+");
                                                    recRegTlbRec.set_StringData(5, null);
                                                    RegistryView.Modify(MsiViewModify.msiViewModifyMerge, recRegTlbRec);
                                                }
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
                                                object custData = new object();
                                                Guid g = new Guid("0F21F359-AB84-41E8-9A78-36D110E6D2F9");
                                                typeInfo2.GetCustData(ref g, out custData);

                                                if (custData != null)
                                                {
                                                    string className = (string)custData;

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
                                                    RegistryView.Modify(MsiViewModify.msiViewModifyMerge, recRegTlbRec);

                                                    recRegTlbRec.set_StringData(1, 
                                                        "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
                                                    recRegTlbRec.set_StringData(3,
                                                        @"Interface\" + iid + @"\TypeLib");
                                                    recRegTlbRec.set_StringData(4, "Version");
                                                    recRegTlbRec.set_StringData(5, "1.0");
                                                    RegistryView.Modify(MsiViewModify.msiViewModifyMerge, recRegTlbRec);

                                                    recRegTlbRec.set_StringData(1, 
                                                        "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
                                                    recRegTlbRec.set_StringData(4, null);
                                                    recRegTlbRec.set_StringData(5, "{"+typeLibAttr.guid.ToString().ToUpper()+"}");
                                                    RegistryView.Modify(MsiViewModify.msiViewModifyMerge, recRegTlbRec);

                                                    recRegTlbRec.set_StringData(1, 
                                                        "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
                                                    recRegTlbRec.set_StringData(3,
                                                        @"Interface\" + iid + @"\ProxyStubClsid32");
                                                    recRegTlbRec.set_StringData(4, null);
                                                    recRegTlbRec.set_StringData(5, "{00020424-0000-0000-C000-000000000046}");
                                                    RegistryView.Modify(MsiViewModify.msiViewModifyMerge, recRegTlbRec);

                                                    recRegTlbRec.set_StringData(1, 
                                                        "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
                                                    recRegTlbRec.set_StringData(3,
                                                        @"Interface\" + iid + @"\ProxyStubClsid");
                                                    recRegTlbRec.set_StringData(4, null);
                                                    recRegTlbRec.set_StringData(5, "{00020424-0000-0000-C000-000000000046}");
                                                    RegistryView.Modify(MsiViewModify.msiViewModifyMerge, recRegTlbRec);
                                                }
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
            typeLibView = null;

            return true;
        }

        /// <summary>
        /// Loads records for the Binary table.  This table stores items 
        /// such as bitmaps, animations, and icons. The binary table is 
        /// also used to store data for custom actions.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadBinary(Database Database, Type InstallerType, Object InstallerObject)
        {
            if (msm.binaries != null)
            {

                // Open the Binary Table
                View binaryView = Database.OpenView("SELECT * FROM `Binary`");

                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Adding Binary Data:");
                }
                
                // Add binary data from Task definition
                foreach (MSMBinary binary in msm.binaries)
                {


                    if (Verbose)
                    {
                        Log(Level.Info, "\t" + Path.Combine(Project.BaseDirectory, binary.value));

                        int nameColSize = 50;

                        if (binary.name.Length > nameColSize)
                        {
                            Log(Level.Error, LogPrefix + 
                                "WARNING: Binary key name longer than " + nameColSize + " characters:\n\tName: " + 
                                binary.name + "\n\tLength: " + binary.name.Length.ToString());

                        }
                    }
                    if (File.Exists(Path.Combine(Project.BaseDirectory, binary.value)))
                    {
                        // Insert the binary data
                        Record recBinary = (Record)InstallerType.InvokeMember(
                            "CreateRecord", 
                            BindingFlags.InvokeMethod, 
                            null, InstallerObject, 
                            new object[] { 2 });

                        recBinary.set_StringData(1, binary.name);
                        recBinary.SetStream(2, Path.Combine(Project.BaseDirectory, binary.value));
                        binaryView.Modify(MsiViewModify.msiViewModifyMerge, recBinary);
                    }
                    else
                    {
                        Log(Level.Error, LogPrefix + 
                            "ERROR: Unable to open file:\n\n\t" + 
                            Path.Combine(Project.BaseDirectory, binary.value) + "\n\n");

                        binaryView.Close();
                        binaryView = null;
                        return false;
                    }
                }

                binaryView.Close();
                binaryView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the Dialog table.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadDialog(Database Database, Type InstallerType, Object InstallerObject)
        {
            if (msm.dialogs != null)
            {

                // Open the Dialog Table
                View dialogView = Database.OpenView("SELECT * FROM `Dialog`");

                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Adding Dialogs:");
                }
                
                foreach (MSMDialog dialog in msm.dialogs)
                {
                    if (Verbose)
                    {
                        Log(Level.Info, "\t" + dialog.name);
                    }

                    // Insert the dialog
                    Record recDialog = (Record)InstallerType.InvokeMember(
                        "CreateRecord", 
                        BindingFlags.InvokeMethod, 
                        null, InstallerObject, 
                        new object[] { 10 });

                    recDialog.set_StringData(1, dialog.name);
                    recDialog.set_IntegerData(2, dialog.hcenter);
                    recDialog.set_IntegerData(3, dialog.vcenter);
                    recDialog.set_IntegerData(4, dialog.width);
                    recDialog.set_IntegerData(5, dialog.height);
                    recDialog.set_IntegerData(6, dialog.attr);
                    recDialog.set_StringData(7, dialog.title);
                    recDialog.set_StringData(8, dialog.firstcontrol);
                    recDialog.set_StringData(9, dialog.defaultcontrol);
                    recDialog.set_StringData(10, dialog.cancelcontrol);
                    
                    dialogView.Modify(MsiViewModify.msiViewModifyMerge, recDialog);
                }

                dialogView.Close();
                dialogView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the Control table.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadControl(Database Database, Type InstallerType, Object InstallerObject)
        {
            if (msm.controls != null)
            {

                // Open the Control Table
                View controlView = Database.OpenView("SELECT * FROM `Control`");

                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Adding Dialog Controls:");
                }
                
                foreach (MSMControl control in msm.controls)
                {
                    if (Verbose)
                    {
                        Log(Level.Info, "\t" + control.name);
                    }

                    // Insert the control
                    Record recControl = (Record)InstallerType.InvokeMember(
                        "CreateRecord", 
                        BindingFlags.InvokeMethod, 
                        null, InstallerObject, 
                        new object[] { 12 });

                    recControl.set_StringData(1, control.dialog);
                    recControl.set_StringData(2, control.name);
                    recControl.set_StringData(3, control.type);
                    recControl.set_IntegerData(4, control.x);
                    recControl.set_IntegerData(5, control.y);
                    recControl.set_IntegerData(6, control.width);
                    recControl.set_IntegerData(7, control.height);
                    recControl.set_IntegerData(8, control.attr);
                    recControl.set_StringData(9, control.property);
                    recControl.set_StringData(10, control.text);
                    recControl.set_StringData(11, control.nextcontrol);
                    recControl.set_StringData(12, control.help);
                    
                    controlView.Modify(MsiViewModify.msiViewModifyMerge, recControl);
                }

                controlView.Close();
                controlView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the ControlCondtion table.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadControlCondition(Database Database, Type InstallerType, Object InstallerObject)
        {
            if (msm.controlconditions != null)
            {

                // Open the ControlCondition Table
                View controlConditionView = Database.OpenView("SELECT * FROM `ControlCondition`");

                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Adding Dialog Control Conditions For:");
                }
                
                foreach (MSMControlCondition controlCondition in msm.controlconditions)
                {
                    if (Verbose)
                    {
                        Log(Level.Info, "\t" + controlCondition.control);
                    }

                    // Insert the condition
                    Record recControlCondition = (Record)InstallerType.InvokeMember(
                        "CreateRecord", 
                        BindingFlags.InvokeMethod, 
                        null, InstallerObject, 
                        new object[] { 4 });

                    recControlCondition.set_StringData(1, controlCondition.dialog);
                    recControlCondition.set_StringData(2, controlCondition.control);
                    recControlCondition.set_StringData(3, controlCondition.action);
                    recControlCondition.set_StringData(4, controlCondition.condition);
                    
                    controlConditionView.Modify(MsiViewModify.msiViewModifyMerge, recControlCondition);
                }

                controlConditionView.Close();
                controlConditionView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the ControlEvent table.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadControlEvent(Database Database, Type InstallerType, Object InstallerObject)
        {
            if (msm.controlevents != null)
            {
                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Modifying Dialog Control Events:");
                }
                
                foreach (MSMControlEvent controlEvent in msm.controlevents)
                {
                    // Open the ControlEvent Table
                    View controlEventView;

                    if (Verbose)
                    {
                        string action = "";
                        if (controlEvent.remove)
                        {
                            action = "\tRemoving";
                        }
                        else
                        {
                            action = "\tAdding";
                        }
                        Log(Level.Info, action + "\tControl: " + controlEvent.control + "\tEvent: " + controlEvent.name);
                    }
                    if (controlEvent.remove)
                    {
                        controlEventView = Database.OpenView("SELECT * FROM `ControlEvent` WHERE `Dialog_`='" + controlEvent.dialog + "' AND `Control_`='" + controlEvent.control + "' AND `Event`='" + controlEvent.name + "' AND `Argument`='" + controlEvent.argument + "' AND `Condition`='" + controlEvent.condition + "'");
                        controlEventView.Execute(null);
                        try
                        {
                            Record recControlEvent = controlEventView.Fetch();
                            controlEventView.Modify(MsiViewModify.msiViewModifyDelete, recControlEvent);                        
                        }
                        catch (IOException)
                        {
                            Log(Level.Error, LogPrefix + 
                                "ERROR: Control Event not found.\n\nSELECT * FROM `ControlEvent` WHERE `Dialog_`='" + controlEvent.dialog + "' AND `Control_`='" + controlEvent.control + "' AND `Event`='" + controlEvent.name + "' AND `Argument`='" + controlEvent.argument + "' AND `Condition`='" + controlEvent.condition + "'");
                            return false;
                        }
                        finally
                        {
                            controlEventView.Close();
                            controlEventView = null;

                        }

                    }
                    else
                    {
                        controlEventView = Database.OpenView("SELECT * FROM `ControlEvent`");
                        // Insert the condition
                        Record recControlEvent = (Record)InstallerType.InvokeMember(
                            "CreateRecord", 
                            BindingFlags.InvokeMethod, 
                            null, InstallerObject, 
                            new object[] { 6 });


                        recControlEvent.set_StringData(1, controlEvent.dialog);
                        recControlEvent.set_StringData(2, controlEvent.control);
                        recControlEvent.set_StringData(3, controlEvent.name);
                        recControlEvent.set_StringData(4, controlEvent.argument);
                        recControlEvent.set_StringData(5, controlEvent.condition);
                        recControlEvent.set_IntegerData(6, controlEvent.order);
                    
                        controlEventView.Modify(MsiViewModify.msiViewModifyMerge, recControlEvent);
                        controlEventView.Close();
                        controlEventView = null;
                    }
                }

            }
            return true;
        }

        /// <summary>
        /// Loads records for the CustomAction table
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadCustomAction(Database Database, Type InstallerType, Object InstallerObject)
        {
            // Add custom actions from Task definition
            if (msm.customactions != null)
            {
                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Adding Custom Actions:");
                }

                View customActionView = Database.OpenView("SELECT * FROM `CustomAction`");

                foreach (MSMCustomAction customAction in msm.customactions)
                {
                    if (Verbose)
                    {
                        Log(Level.Info, "\t" + customAction.action);
                    }

                    // Insert the record into the table
                    Record recCustomAction = (Record)InstallerType.InvokeMember(
                        "CreateRecord", 
                        BindingFlags.InvokeMethod, 
                        null, InstallerObject, 
                        new object[] { 4 });

                    recCustomAction.set_StringData(1, customAction.action);
                    recCustomAction.set_IntegerData(2, customAction.type);
                    recCustomAction.set_StringData(3, customAction.source);
                    recCustomAction.set_StringData(4, customAction.target);
                    
                    customActionView.Modify(MsiViewModify.msiViewModifyMerge, recCustomAction);
                }
                customActionView.Close();
                customActionView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the InstallUISequence, InstallExecuteSequence,
        /// AdminUISequence, AdminExecute, AdvtExecuteSequence tables.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadSequence(Database Database, Type InstallerType, Object InstallerObject)
        {
            // Add custom actions from Task definition
            if (msm.sequences != null)
            {
                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Adding Install/Admin Sequences:");
                }

                // Open the sequence tables
                View installExecuteView = Database.OpenView("SELECT * FROM `InstallExecuteSequence`");
                View installUIView = Database.OpenView("SELECT * FROM `InstallUISequence`");
                View adminExecuteView = Database.OpenView("SELECT * FROM `AdminExecuteSequence`");
                View adminUIView = Database.OpenView("SELECT * FROM `AdminUISequence`");
                View advtExecuteView = Database.OpenView("SELECT * FROM `ModuleAdvtExecuteSequence`");

                // Add binary data from Task definition
                foreach (MSMSequence sequence in msm.sequences)
                {
                    if (Verbose)
                    {
                        Log(Level.Info, "\t" + sequence.action + " to the " + sequence.type.ToString() + "sequence table.");
                    }

                    // Insert the record to the respective table
                    Record recSequence = (Record)InstallerType.InvokeMember(
                        "CreateRecord", 
                        BindingFlags.InvokeMethod, 
                        null, InstallerObject, 
                        new object[] { 3 });

                    recSequence.set_StringData(1, sequence.action);
                    recSequence.set_StringData(2, sequence.condition);
                    recSequence.set_IntegerData(3, sequence.value);
                    switch(sequence.type.ToString())
                    {
                        case "installexecute":
                            installExecuteView.Modify(MsiViewModify.msiViewModifyMerge, recSequence);
                            break;
                        case "installui":
                            installUIView.Modify(MsiViewModify.msiViewModifyMerge, recSequence);
                            break;
                        case "adminexecute":
                            adminExecuteView.Modify(MsiViewModify.msiViewModifyMerge, recSequence);
                            break;
                        case "adminui":
                            adminUIView.Modify(MsiViewModify.msiViewModifyMerge, recSequence);
                            break;
                        case "advtexecute":
                            advtExecuteView.Modify(MsiViewModify.msiViewModifyMerge, recSequence);
                            break;

                    }
                }
                installExecuteView.Close();
                installUIView.Close();
                adminExecuteView.Close();
                adminUIView.Close();
                advtExecuteView.Close();

                installExecuteView = null;
                installUIView = null;
                adminExecuteView = null;
                adminUIView = null;
                advtExecuteView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the _AppMappings table.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadAppMappings(Database Database, Type InstallerType, Object InstallerObject)
        {
            if (msm.appmappings != null)
            {
                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Adding Application Mappings:");
                }

                View appmapView = Database.OpenView("SELECT * FROM `_AppMappings`");

                foreach (MSMAppMapping appmap in msm.appmappings)
                {
                    if (Verbose)
                    {
                        Log(Level.Info, "\t" + appmap.directory);
                    }

                    // Insert the record into the table
                    Record recAppMap = (Record)InstallerType.InvokeMember(
                        "CreateRecord", 
                        BindingFlags.InvokeMethod, 
                        null, InstallerObject, 
                        new object[] { 4 });

                    recAppMap.set_StringData(1, appmap.directory);
                    recAppMap.set_StringData(2, appmap.extension);
                    recAppMap.set_StringData(3, appmap.exepath);
                    recAppMap.set_StringData(4, appmap.verbs);
                    
                    appmapView.Modify(MsiViewModify.msiViewModifyMerge, recAppMap);

                }
                appmapView.Close();
                appmapView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the _UrlToDir table.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadUrlProperties(Database Database, Type InstallerType, Object InstallerObject)
        {
            if (msm.urlproperties != null)
            {
                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Adding URL Properties:");
                }

                View urlpropView = Database.OpenView("SELECT * FROM `_UrlToDir`");

                foreach (MSMURLProperty urlprop in msm.urlproperties)
                {
                    if (Verbose)
                    {
                        Log(Level.Info, "\t" + urlprop.name);
                    }

                    // Insert the record into the table
                    Record recURLProp = (Record)InstallerType.InvokeMember(
                        "CreateRecord", 
                        BindingFlags.InvokeMethod, 
                        null, InstallerObject, 
                        new object[] { 2 });

                    recURLProp.set_StringData(1, urlprop.name);
                    recURLProp.set_StringData(2, urlprop.property);
                    
                    urlpropView.Modify(MsiViewModify.msiViewModifyMerge, recURLProp);

                }
                urlpropView.Close();
                urlpropView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the _VDirToUrl table.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadVDirProperties(Database Database, Type InstallerType, Object InstallerObject)
        {
            if (msm.vdirproperties != null)
            {
                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Adding VDir Properties:");
                }

                View vdirpropView = Database.OpenView("SELECT * FROM `_VDirToUrl`");

                foreach (MSMVDirProperty vdirprop in msm.vdirproperties)
                {
                    if (Verbose)
                    {
                        Log(Level.Info, "\t" + vdirprop.name);
                    }

                    // Insert the record into the table
                    Record recVDirProp = (Record)InstallerType.InvokeMember(
                        "CreateRecord", 
                        BindingFlags.InvokeMethod, 
                        null, InstallerObject, 
                        new object[] { 3 });

                    recVDirProp.set_StringData(1, vdirprop.name);
                    recVDirProp.set_StringData(2, vdirprop.portproperty);
                    recVDirProp.set_StringData(3, vdirprop.urlproperty);

                    vdirpropView.Modify(MsiViewModify.msiViewModifyMerge, recVDirProp);
                }
                vdirpropView.Close();
                vdirpropView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the _AppRootCreate table.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadAppRootCreate(Database Database, Type InstallerType, Object InstallerObject)
        {
            if (msm.approots != null)
            {
                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Adding Application Roots:");
                }

                View approotView = Database.OpenView("SELECT * FROM `_AppRootCreate`");

                foreach (MSMAppRoot appRoot in msm.approots)
                {
                    if (Verbose)
                    {
                        Log(Level.Info, "\t" + appRoot.urlproperty);
                    }

                    // Insert the record into the table
                    Record recAppRootProp = (Record)InstallerType.InvokeMember(
                        "CreateRecord", 
                        BindingFlags.InvokeMethod, 
                        null, InstallerObject, 
                        new object[] { 3 });

                    recAppRootProp.set_StringData(1, appRoot.component);
                    recAppRootProp.set_StringData(2, appRoot.urlproperty);
                    recAppRootProp.set_IntegerData(3, appRoot.inprocflag);

                    approotView.Modify(MsiViewModify.msiViewModifyMerge, recAppRootProp);
                }
                approotView.Close();
                approotView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the _IISProperties table.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadIISProperties(Database Database, Type InstallerType, Object InstallerObject)
        {
            if (msm.iisproperties != null)
            {
                if (Verbose)
                {
                    Log(Level.Info, LogPrefix + "Adding IIS Directory Properties:");
                }

                View iispropView = Database.OpenView("SELECT * FROM `_IISProperties`");


                // Add binary data from Task definition
                foreach (MSMIISProperty iisprop in msm.iisproperties)
                {
                    if (Verbose)
                    {
                        Log(Level.Info, "\t" + iisprop.directory);
                    }

                    // Insert the record into the table
                    Record recIISProp = (Record)InstallerType.InvokeMember(
                        "CreateRecord", 
                        BindingFlags.InvokeMethod, 
                        null, InstallerObject, 
                        new object[] { 3 });

                    recIISProp.set_StringData(1, iisprop.directory);
                    recIISProp.set_IntegerData(2, iisprop.attr);
                    recIISProp.set_StringData(3, iisprop.defaultdoc);
                    
                    iispropView.Modify(MsiViewModify.msiViewModifyMerge, recIISProp);
                }
                iispropView.Close();
                iispropView = null;
            }
            return true;
        }


        /// <summary>
        /// Drops empty tables.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
        /// <returns>True if empy and False if full.</returns>
        private bool DropEmptyTables(Database Database, Type InstallerType, Object InstallerObject)
        {
            if (Verbose)
            {
                Log(Level.Info, LogPrefix + "Dropping unused tables:");
            }            
            // Go through each table listed in _Tables
            View tableView = Database.OpenView("SELECT * FROM `_Tables`");

            tableView.Execute(null);
            Record tableRecord = tableView.Fetch();

            while (tableRecord != null)
            {                
                string tableName = tableRecord.get_StringData(1);

                if (VerifyTableEmpty(Database, tableName))
                {
                    try
                    {
                        // Drop the table
                        View tempView = Database.OpenView("DROP TABLE `" + tableName + "`");
                        tempView.Execute(null);
                        tempView.Close();
                        tempView = null;
                        
                        // Delete entries in _Validation table
                        tempView = Database.OpenView("DELETE FROM `_Validation` WHERE `Table` = '" + tableName + "'");
                        tempView.Execute(null);
                        tempView.Close();
                        tempView = null;

                        if (Verbose)
                        {
                            Log(Level.Info, "\t" + tableName);
                        }
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                
                }
                tableRecord = tableView.Fetch();
            }

            tableView.Close();
            tableView = null;
            return true;
        }

        /// <summary>
        /// Checks to see if the specified table is empty.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="TableName">Name of the table to check existance.</param>
        /// <returns>True if empy and False if full.</returns>
        private bool VerifyTableEmpty(Database Database, string TableName)
        {
            View tableView = Database.OpenView("SELECT * FROM `" + TableName + "`");
            tableView.Execute(null);
            Record tableRecord = tableView.Fetch();

            if (tableRecord != null)
            {
                tableView.Close();
                tableView = null;
                return false;
            }
            else
            {
                tableView.Close();
                tableView = null;
                return true;
            }

        }

        /// <summary>
        /// Checks to see if the specified table exists in the database
        /// already.
        /// </summary>
        /// <param name="Database">The MSM database.</param>
        /// <param name="TableName">Name of the table to check existance.</param>
        /// <returns>True if successful.</returns>
        private bool VerifyTableExistance(Database Database, string TableName)
        {
            View tableView = Database.OpenView("SELECT * FROM `_Tables` WHERE `Name`='" + TableName + "'");
            tableView.Execute(null);
            Record tableRecord = tableView.Fetch();

            if (tableRecord != null)
            {
                tableView.Close();
                tableView = null;
                return true;
            }
            else
            {
                tableView.Close();
                tableView = null;
                return false;
            }

        }

        /// <summary>
        /// Enumerates the registry to see if an assembly has been registered 
        /// for COM interop, and if so adds these registry keys to the Registry 
        /// table, ProgIds to the ProgId table, classes to the Classes table, 
        /// and a TypeLib to the TypeLib table.
        /// </summary>
        /// <param name="FileName">The Assembly filename.</param>
        /// <param name="FileAssembly">The Assembly to check.</param>
        /// <param name="InstallerType">The MSM Installer type.</param>
        /// <param name="InstallerObject">The MSM Installer object.</param>
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
                                    Log(Level.Info, LogPrefix + "Configuring " + typeLibName + " for COM Interop...");

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
                                        ProgIdView.Modify(MsiViewModify.msiViewModifyMerge, recProgId);

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
                                        ClassView.Modify(MsiViewModify.msiViewModifyMerge, recClass);
                                    }
                                    progIdKey.Close();
                                    progIdKey = null;
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
            foreach (MSMDirectory directory in msm.directories)
            {
                string parent = FindParent(DirectoryName, directory);
                if (parent != null)
                {
                    return parent;
                }
            }
            return null;
        }

        private string FindParent(string DirectoryName, MSMDirectory directory)
        {
            if (DirectoryName == directory.name && 
                directory is MSMRootDirectory)
            {
                return ((MSMRootDirectory)directory).root;
            }
            else
            {
                if (directory.directory != null)
                {
                    foreach (MSMDirectory directory2 in directory.directory)
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

        private MSMDirectory FindDirectory(string DirectoryName)
        {
            foreach (MSMDirectory directory in msm.directories)
            {
                MSMDirectory childDirectory = FindDirectory(DirectoryName, directory);
                if (childDirectory != null)
                {
                    return childDirectory;
                }
            }

            return null;
        }

        private MSMDirectory FindDirectory(string DirectoryName, MSMDirectory directory)
        {
            if (directory.name == DirectoryName)
            {
                return directory;
            }

            if (directory.directory != null)
            {
                foreach (MSMDirectory childDirectory in directory.directory)
                {
                    MSMDirectory childDirectory2 = FindDirectory(DirectoryName, childDirectory);
                    if (childDirectory2 != null)
                    {
                        return childDirectory2;
                    }
                }
            }

            return null;
        }

        private string GetDisplayablePath(string path)
        {
            if (path.Length > 40)
            {
                return "..." + path.Substring(path.Length-37, 37);
            }
            return path;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSMRowColumnData
    {
        public string name;
        public int id;
        public string type;
    }

}
