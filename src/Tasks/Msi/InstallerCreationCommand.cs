//
// NAntContrib
//
// Copyright (C) 2004 Kraen Munck (kmc@innomate.com)
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
// Based on original work by Jayme C. Edwards (jcedwards@users.sourceforge.net)
//

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

using Microsoft.Win32;

using WindowsInstaller;

using NAnt.Core;
using NAnt.Core.Types;

using NAnt.Contrib.Schemas.Msi;

namespace NAnt.Contrib.Tasks.Msi {
    /// <summary>
    /// Base class for <see cref="MsiCreationCommand" /> and <see cref="MsmCreationCommand" />.
    /// </summary>
    public abstract class InstallerCreationCommand {
        private MSIBase msi;
        private NAnt.Core.Task task;
        private Location location;
        private XmlNode node;

        protected InstallerCreationCommand(MSIBase msi, NAnt.Core.Task task, Location location, XmlNode node) {
            this.msi = msi;
            this.task = task;
            this.location = location;
            this.node = node;
            guidCounter = msi.output.GetHashCode();
        }

        public MSIBase MsiBase {
            get { return msi; }
        }

        /*
         * Methods to support moving away from a NAnt.Core.Task subclass
         */

        protected void Log(NAnt.Core.Level messageLevel, string message, params object[] args) {
            task.Log(messageLevel, message, args);
        }

        protected string LogPrefix {
            get { return task.LogPrefix; }
        }

        protected bool Verbose {
            get { return task.Verbose; }
        }

        protected NAnt.Core.Project Project {
            get { return task.Project; }
        }

        protected XmlNamespaceManager NamespaceManager {
            get { return task.NamespaceManager; }
        }

        protected NAnt.Core.Location Location {
            get { return location; }
        }

        protected PropertyDictionary Properties {
            get { return task.Properties; }
        }

        protected XmlNode _xmlNode {
            get { return node; }
        }


        /*
         * Virtual methods for differing code
         */

        protected abstract string CabFileName {
            get;
        }

        protected abstract string TemplateFileName {
            get;
        }

        protected abstract string ErrorTemplateFileName {
            get;
        }

        protected abstract string AdvtExecuteName {
            get;
        }

        protected abstract void AddModuleComponentVirtual(InstallerDatabase database, InstallerTable modComponentTable, string componentName);

        protected abstract void LoadTypeSpecificDataFromTask(InstallerDatabase database, int lastSequence);


        /*
         * Utility methods and properties
         */

        protected string TempFolderPath {
            get {
                return Path.Combine(Project.BaseDirectory, Path.Combine(msi.sourcedir, @"Temp"));
            }
        }

        int guidCounter;

        private string CreateRegistryGuid() {
            if (msi.deterministicguids) {
                guidCounter++;
                return "{00000000-0000-0000-0000-0000"+ String.Format("{0:X8}", guidCounter) +"}";
            } else {
                return "{"+Guid.NewGuid().ToString().ToUpper()+"}";
            }
        }

        private string CreateIdentityGuid() {
            if (msi.deterministicguids) {
                guidCounter++;
                return "_000000000000000000000000"+ String.Format("{0:X8}", guidCounter);
            } else {
                return "_" + Guid.NewGuid().ToString().ToUpper().Replace("-", null).ToUpper();
            }
        }

        /// <summary>
        /// Determines if the supplied version string is valid.  A valid version string should look like:
        /// 1
        /// 1.1
        /// 1.1.1
        /// 1.1.1.1
        /// </summary>
        /// <param name="Version">The version string to verify.</param>
        /// <returns></returns>
        protected static bool IsVersion(ref string Version) {
            // For cases of 5,5,2,2
            Version = Version.Trim().Replace(",", ".");
            Version = Version.Replace(" ", "");
            string[] versionParts = Version.Split('.');
            bool result = true;

            foreach (string versionPart in versionParts) {
                try {
                    int iVersionPart = Convert.ToInt32(versionPart);
                }
                catch (Exception) {
                    result = false;
                    break;
                }
            }
            return result;
        }


        /*
         * Protected properties (TODO: create accessors)
         */

        protected Hashtable featureComponents = new Hashtable();
        protected Hashtable components = new Hashtable();


        /*
         * Common code for the two tasks
         */

        /// <summary>
        /// Sets the sequence number of files to match their
        /// storage order in the cabinet file, after some
        /// files have had their filenames changed to go in
        /// their own component.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        /// <param name="LastSequence">The last file's sequence number.</param>
        protected void ReorderFiles(InstallerDatabase database, ref int LastSequence) {
            string curPath = Path.Combine(Project.BaseDirectory, msi.sourcedir);
            string curTempPath = Path.Combine(curPath, "Temp");

            string[] curFileNames = Directory.GetFiles(curTempPath, "*.*");

            LastSequence = 1;

            foreach (string curDirFileName in curFileNames) {

                using (InstallerRecordReader reader = database.FindRecords("File", 
                           new InstallerSearchClause("File", Comparison.Equals, Path.GetFileName(curDirFileName)))) {

                    if (reader.Read()) {
                        reader.SetValue(7, LastSequence.ToString());
                        reader.Commit();

                        LastSequence++;
                    }
                    else {
                        throw new BuildException("File " +
                            Path.GetFileName(curDirFileName) +
                            " not found during reordering.");
                    }
                }
            }
        }

        private Hashtable files = new Hashtable();
        private ArrayList typeLibRecords = new ArrayList();
        private Hashtable typeLibComponents = new Hashtable();

        private string[] commonFolderNames = new string[] {
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
        /// Cleans the output directory after a build.
        /// </summary>
        /// <param name="cabFile">The path to the cabinet file.</param>
        /// <param name="tempPath">The path to temporary files.</param>
        private void CleanOutput(string cabFile, string tempPath) {
            try {
                File.Delete(cabFile);
            } catch {}

            try {
                Directory.Delete(tempPath, true);
            } catch {}
        }

        /// <summary>
        /// Loads records for the Properties table.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadProperties(InstallerDatabase database) {
            // Select the "Property" Table
            using (InstallerTable propertyTable = database.OpenTable("Property")) {
                Log(Level.Verbose, LogPrefix + "Adding Properties:");

                property productName = null;
                property productCode = null;
                property productVersion = null;
                property manufacturer = null;

                // Add properties from Task definition
                foreach (property property in msi.properties) {
                    // Insert the Property
                    string name = property.name;
                    string sValue = property.value;

                    if (name == "ProductName") {
                        productName = property;
                    } else if (name == "ProductCode") {
                        productCode = property;
                    } else if (name == "ProductVersion") {
                        productVersion = property;
                    } else if (name == "Manufacturer") {
                        manufacturer = property;
                    }

                    if (name == null || name == "") {
                        throw new BuildException("Property with no name attribute detected.", Location);
                    }

                    if (sValue == null || sValue == "") {
                        throw new BuildException(String.Format(CultureInfo.InvariantCulture, "Property {0} has no value.", name), Location);
                    }

                    propertyTable.InsertRecord(name, sValue);

                    Log(Level.Verbose, "\t" + name);
                }

                if ((productName == null) && (this is MsiCreationCommand))
                    throw new BuildException("ProductName property must be specified.  For more information please visit: http://msdn.microsoft.com/library/en-us/msi/setup/productname_property.asp");
                if ((productCode == null) && (this is MsiCreationCommand))
                    throw new BuildException("ProductCode property must be specified.  For more information please visit: http://msdn.microsoft.com/library/en-us/msi/setup/productcode_property.asp");
                if ((productVersion == null) && (this is MsiCreationCommand))
                    throw new BuildException("ProductVersion property must be specified.  For more information please visit: http://msdn.microsoft.com/library/en-us/msi/setup/productversion_property.asp");
                if ((manufacturer == null) && (this is MsiCreationCommand))
                    throw new BuildException("Manufacturer property must be specified.  For more information please visit: http://msdn.microsoft.com/library/en-us/msi/setup/manufacturer_property.asp");
            }
        }

        /// <summary>
        /// Loads records for the Directories table.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadDirectories(InstallerDatabase database) {
            // Open the "Directory" Table
            using (InstallerTable directoryTable = database.OpenTable("Directory")) {

                if (msi.directories != null) {
                    ArrayList directoryList = new ArrayList(msi.directories);

                    AddTargetDir(directoryList);
                    AddCommonDirectories(directoryList);

                    MSIRootDirectory[] directories = new MSIRootDirectory[directoryList.Count];
                    directoryList.CopyTo(directories);
                    msi.directories = directories;

                    int depth = 1;

                    Log(Level.Verbose, LogPrefix + "Adding Directories:");

                    // Add directories from Task definition
                    foreach (MSIRootDirectory directory in msi.directories) {
                        AddDirectory(database,
                            directoryTable, null,
                            directory, depth);
                    }
                }
            }
        }

        private void AddTargetDir(ArrayList directoryList) {
            MSIRootDirectory targetDir = new MSIRootDirectory();
            targetDir.name = "TARGETDIR";
            targetDir.root = "";
            targetDir.foldername = "SourceDir";
            directoryList.Add(targetDir);
        }

        private void AddCommonDirectories(ArrayList directoryList) {
            for (int i = 0; i < commonFolderNames.Length; i++) {
                MSIRootDirectory commonDir = new MSIRootDirectory();
                commonDir.name = commonFolderNames[i];
                commonDir.root = "TARGETDIR";
                commonDir.foldername = ".";
                directoryList.Add(commonDir);
            }
        }


        /// <summary>
        /// Adds a directory record to the directories table.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        /// <param name="directoryTable">The MSI database view.</param>
        /// <param name="ParentDirectory">The parent directory.</param>
        /// <param name="Directory">This directory's Schema object.</param>
        /// <param name="Depth">The tree depth of this directory.</param>
        private void AddDirectory(InstallerDatabase database, InstallerTable directoryTable,
            string ParentDirectory, MSIDirectory Directory, int Depth) {
            string newParent;
            if (Directory is MSIRootDirectory) {
                newParent = ((MSIRootDirectory)Directory).root;
            } else {
                newParent = ParentDirectory;
            }

            StringBuilder relativePath = new StringBuilder();

            GetRelativePath(database,
                Directory.name, ParentDirectory, Directory.foldername,
                relativePath, directoryTable);

            if (relativePath.ToString().Length != 0) {
                string fullPath = Path.Combine(Path.Combine(Project.BaseDirectory, msi.sourcedir), relativePath.ToString());

                bool createTemp = false;
                DirectoryInfo di = new DirectoryInfo(fullPath);
                DirectoryInfo lastExistingDir = di.Parent;
                if (!di.Exists) {
                    while (!lastExistingDir.Exists) {
                        lastExistingDir = lastExistingDir.Parent;
                    }
                    di.Create();
                    createTemp = true;
                }

                string path = GetShortPath(fullPath) + "|" + Directory.foldername;

                if (createTemp) {
                    while (!di.FullName.Equals(lastExistingDir.FullName)) {
                        di.Delete();
                        di = di.Parent;
                    }
                }

                if (Directory.foldername == ".")
                    path = Directory.foldername;

                Log(Level.Verbose, "\t" +  relativePath.ToString());

                // Insert the Directory
                directoryTable.InsertRecord(Directory.name, newParent, path);

                if (Directory.directory != null) {
                    foreach (MSIDirectory childDirectory in Directory.directory) {
                        int newDepth = Depth + 1;

                        AddDirectory(database, directoryTable,
                            Directory.name, childDirectory, newDepth);
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the relative path of a file based on
        /// the component it belongs to and its entry in
        /// the MSI directory table.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        /// <param name="Name">The Name of the Folder</param>
        /// <param name="Parent">The Parent of the Folder</param>
        /// <param name="Default">The Relative Filesystem Path of the Folder</param>
        /// <param name="Path">The Path to the Folder from previous calls.</param>
        /// <param name="directoryTable">The MSI database view.</param>
        private void GetRelativePath(
            InstallerDatabase database,
            string Name,
            string Parent,
            string Default,
            StringBuilder Path,
            InstallerTable directoryTable) {
            if (Name == "TARGETDIR") {
                return;
            }

            for (int i = 0; i < commonFolderNames.Length; i++) {
                if (Name == commonFolderNames[i]) {
                    return;
                }
            }

            if (msi.directories != null) {
                ArrayList directoryList = new ArrayList();
                foreach(MSIRootDirectory directory in msi.directories) {
                    directoryList.Add(directory);
                }

                foreach (property property in msi.properties) {
                    if (Name == property.name) {
                        MSIDirectory directory = FindDirectory(Name);
                        if (directory == null) {
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

                if (Path.Length > 0) {
                    Path.Insert(0, @"\");
                }

                Path.Insert(0, Default);
                if (Parent != null) {
                    MSIDirectory PathInfo = FindDirectory(Parent);

                    if (PathInfo == null) {
                        foreach (property property in msi.properties) {
                            if (Parent == property.name) {
                                MSIRootDirectory directory = new MSIRootDirectory();
                                directory.name = Parent;
                                directory.root = "TARGETDIR";
                                directory.foldername = ".";

                                directoryList.Add(directory);

                                MSIRootDirectory[] rootDirs = new MSIRootDirectory[directoryList.Count];
                                directoryList.CopyTo(rootDirs);

                                msi.directories = rootDirs;

                                // Insert the Directory that is a Property
                                directoryTable.InsertRecord(Parent, "TARGETDIR", ".");

                                PathInfo = directory;

                                break;
                            }
                        }
                    }

                    string newParent = null;
                    if (PathInfo is MSIRootDirectory) {
                        newParent = ((MSIRootDirectory)PathInfo).root;
                    }
                    else {
                        newParent = FindParent(Parent);
                    }

                    GetRelativePath(database,
                        Parent, newParent,
                        PathInfo.foldername, Path, directoryTable);
                }
            }

        }

        private string FindParent(string DirectoryName) {
            foreach (MSIDirectory directory in msi.directories) {
                string parent = FindParent(DirectoryName, directory);
                if (parent != null) {
                    return parent;
                }
            }
            return null;
        }

        private string FindParent(string DirectoryName, MSIDirectory directory) {
            if (DirectoryName == directory.name &&
                directory is MSIRootDirectory) {
                return ((MSIRootDirectory)directory).root;
            }
            else {
                if (directory.directory != null) {
                    foreach (MSIDirectory directory2 in directory.directory) {
                        if (directory2.name == DirectoryName) {
                            return directory.name;
                        }
                        else {
                            string parent = FindParent(DirectoryName, directory2);
                            if (parent != null) {
                                return parent;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private MSIDirectory FindDirectory(string DirectoryName) {
            foreach (MSIDirectory directory in msi.directories) {
                MSIDirectory childDirectory = FindDirectory(DirectoryName, directory);
                if (childDirectory != null) {
                    return childDirectory;
                }
            }

            return null;
        }

        private MSIDirectory FindDirectory(string DirectoryName, MSIDirectory directory) {
            if (directory.name == DirectoryName) {
                return directory;
            }

            if (directory.directory != null) {
                foreach (MSIDirectory childDirectory in directory.directory) {
                    MSIDirectory childDirectory2 = FindDirectory(DirectoryName, childDirectory);
                    if (childDirectory2 != null) {
                        return childDirectory2;
                    }
                }
            }

            return null;
        }

        private string GetDisplayablePath(string path) {
            if (path.Length > 40) {
                return "..." + path.Substring(path.Length-37, 37);
            }
            return path;
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern int GetShortPathName(string LongPath, StringBuilder ShortPath, int BufferSize);

        /// <summary>
        /// Retrieves a DOS 8.3 filename for a file.
        /// </summary>
        /// <param name="LongFile">The file to shorten.</param>
        /// <returns>The new shortened file.</returns>
        private string GetShortFile(string LongFile) {
            if (LongFile.Length <= 8) {
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
        private string GetShortPath(string LongPath) {
            if (LongPath.Length <= 8) {
                return LongPath;
            }

            StringBuilder shortPath = new StringBuilder(255);
            int result = GetShortPathName(LongPath, shortPath, shortPath.Capacity);

            Uri shortPathUri = null;
            try {
                shortPathUri = new Uri("file://" + shortPath.ToString());
            }
            catch (Exception) {
                throw new BuildException(String.Format(CultureInfo.InvariantCulture, "Directory {0} not found.", LongPath), Location);
            }

            string[] shortPathSegments = shortPathUri.Segments;
            if (shortPathSegments.Length == 0) {
                return LongPath;
            }
            if (shortPathSegments.Length == 1) {
                return shortPathSegments[0];
            }
            return shortPathSegments[shortPathSegments.Length-1];
        }

        /// <summary>
        /// Retrieves a DOS 8.3 filename for a complete directory.
        /// </summary>
        /// <param name="LongPath">The path to shorten.</param>
        /// <returns>The new shortened path.</returns>
        private string GetShortDir(string LongPath) {
            if (LongPath.Length <= 8) {
                return LongPath;
            }

            StringBuilder shortPath = new StringBuilder(255);
            int result = GetShortPathName(LongPath, shortPath, shortPath.Capacity);

            Uri shortPathUri = null;
            try {
                shortPathUri = new Uri("file://" + shortPath.ToString());
            }
            catch (Exception) {
                throw new BuildException(String.Format(CultureInfo.InvariantCulture, "Directory {0} not found.", LongPath), Location);
            }

            return shortPath.ToString();
        }

        /// <summary>
        /// Recursively expands properties of all attributes of
        /// a nodelist and their children.
        /// </summary>
        /// <param name="Nodes">The nodes to recurse.</param>
        private void ExpandPropertiesInNodes(XmlNodeList Nodes) {
            foreach (XmlNode node in Nodes) {
                if (node.ChildNodes != null) {
                    ExpandPropertiesInNodes(node.ChildNodes);
                    if (node.Attributes != null) {
                        foreach (XmlAttribute attr in node.Attributes) {
                            attr.Value = Properties.ExpandProperties(attr.Value, Location);
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
        private string ByteArrayToString(Byte[] ByteArray) {
            if ((ByteArray == null) || (ByteArray.Length == 0))
                return "";
            StringBuilder sb = new StringBuilder ();
            sb.Append (ByteArray[0].ToString("x2"));
            for (int i = 1; i < ByteArray.Length; i++) {
                sb.Append(ByteArray[i].ToString("x2"));
            }
            return sb.ToString().ToUpper();
        }

        [DllImport("oleaut32.dll", CharSet=CharSet.Auto)]
        private static extern int LoadTypeLib(string TypeLibFileName, ref IntPtr pTypeLib);

        /// <summary>
        /// Loads TypeLibs for the TypeLib table.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadTypeLibs(InstallerDatabase database) {
            // Open the "TypeLib" Table
            using (InstallerTable typeLibTable = database.OpenTable("TypeLib"),
                       registryTable = database.OpenTable("Registry")) {

                string runtimeVer = Environment.Version.ToString(4);

                for (int i = 0; i < typeLibRecords.Count; i++) {
                    TypeLibRecord tlbRecord = (TypeLibRecord)typeLibRecords[i];

                    IntPtr pTypeLib = new IntPtr(0);
                    int result = LoadTypeLib(tlbRecord.TypeLibFileName, ref pTypeLib);
                    if (result == 0) {
                        UCOMITypeLib typeLib = (UCOMITypeLib)Marshal.GetTypedObjectForIUnknown(
                            pTypeLib, typeof(UCOMITypeLib));
                        if (typeLib != null) {
                            int helpContextId;
                            string name, docString, helpFile;

                            typeLib.GetDocumentation(
                                -1, out name, out docString,
                                out helpContextId, out helpFile);

                            IntPtr pTypeLibAttr = new IntPtr(0);
                            typeLib.GetLibAttr(out pTypeLibAttr);

                            TYPELIBATTR typeLibAttr = (TYPELIBATTR)Marshal.PtrToStructure(pTypeLibAttr, typeof(TYPELIBATTR));

                            string tlbCompName = (string)typeLibComponents[Path.GetFileName(tlbRecord.TypeLibFileName)];

                            typeLibTable.InsertRecord("{"+typeLibAttr.guid.ToString().ToUpper()+"}", Marshal.GetTypeLibLcid(typeLib),
                                tlbCompName, 256, docString == null ? name : docString, null, tlbRecord.FeatureName, 0);

                            typeLib.ReleaseTLibAttr(pTypeLibAttr);

                            // If a .NET type library wrapper for an assembly
                            if (tlbRecord.AssemblyName != null) {
                                // Get all the types defined in the typelibrary
                                // that are not marked "noncreatable"

                                int typeCount = typeLib.GetTypeInfoCount();
                                for (int j = 0; j < typeCount; j++) {
                                    UCOMITypeInfo typeInfo = null;
                                    typeLib.GetTypeInfo(j, out typeInfo);

                                    if (typeInfo != null) {
                                        TYPEATTR typeAttr = GetTypeAttributes(typeInfo);

                                        if (IsCreatableCoClass(typeAttr)) {

                                            if (typeInfo is UCOMITypeInfo2) {
                                                string className = GetClassName(typeInfo);

                                                if (className != null) {
                                                    string clsid = "{" + typeAttr.guid.ToString().ToUpper() + "}";

                                                    AddClassToRegistryTable(registryTable, clsid, className, tlbRecord);
                                                }
                                            }
                                        }
                                        else if (IsIDispatchInterface(typeAttr)) {

                                            string typeName, typeDocString, typeHelpFile;
                                            int typeHelpContextId;

                                            typeInfo.GetDocumentation(-1, out typeName,
                                                out typeDocString, out typeHelpContextId,
                                                out typeHelpFile);

                                            if (typeInfo is UCOMITypeInfo2) {
                                                UCOMITypeInfo2 typeInfo2 = (UCOMITypeInfo2)typeInfo;
                                                if (typeInfo2 != null) {
                                                    object custData;
                                                    Guid g = new Guid("0F21F359-AB84-41E8-9A78-36D110E6D2F9");
                                                    typeInfo2.GetCustData(ref g, out custData);

                                                    if (custData != null) {
                                                        string iid = "{" + typeAttr.guid.ToString().ToUpper() + "}";

                                                        // Insert the Interface

                                                        string typeLibComponent = (string)typeLibComponents[Path.GetFileName(tlbRecord.TypeLibFileName)];

                                                        registryTable.InsertRecord(CreateIdentityGuid(), 0, @"Interface\" + iid,
                                                            null, typeName, typeLibComponent);

                                                        registryTable.InsertRecord(CreateIdentityGuid(), 0, @"Interface\" + iid + @"\TypeLib",
                                                            "Version", "1.0", typeLibComponent);

                                                        registryTable.InsertRecord(CreateIdentityGuid(), 0, @"Interface\" + iid + @"\TypeLib",
                                                            null, "{"+typeLibAttr.guid.ToString().ToUpper()+"}", typeLibComponent);

                                                        registryTable.InsertRecord(CreateIdentityGuid(), 0, @"Interface\" + iid + @"\ProxyStubClsid32",
                                                            null, "{00020424-0000-0000-C000-000000000046}", typeLibComponent);

                                                        registryTable.InsertRecord(CreateIdentityGuid(), 0, @"Interface\" + iid + @"\ProxyStubClsid",
                                                            null, "{00020424-0000-0000-C000-000000000046}", typeLibComponent);
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
            }
        }

        private bool IsIDispatchInterface(TYPEATTR typeAttr) {
            return typeAttr.typekind == TYPEKIND.TKIND_DISPATCH;
        }

        private TYPEATTR GetTypeAttributes(UCOMITypeInfo typeInfo) {
            IntPtr pTypeAttr = new IntPtr(0);
            typeInfo.GetTypeAttr(out pTypeAttr);

            TYPEATTR typeAttr = (TYPEATTR)Marshal.PtrToStructure(pTypeAttr, typeof(TYPEATTR));
            return typeAttr;
        }

        private string GetClassName(UCOMITypeInfo typeInfo) {
            UCOMITypeInfo2 typeInfo2 = (UCOMITypeInfo2)typeInfo;

            object custData = new object();
            Guid g = new Guid("0F21F359-AB84-41E8-9A78-36D110E6D2F9");
            typeInfo2.GetCustData(ref g, out custData);
            return (string)custData;
        }

        private bool IsCreatableCoClass(TYPEATTR typeAttr) {
            return typeAttr.typekind == TYPEKIND.TKIND_COCLASS
                && typeAttr.wTypeFlags == TYPEFLAGS.TYPEFLAG_FCANCREATE;
        }

        private void AddClassToRegistryTable(InstallerTable registryTable, string classGuid, string className, TypeLibRecord tlbRecord) {
            string registryClassIdKeyPart = @"CLSID\" + classGuid + @"\";

            // Insert the Class
            registryTable.InsertRecord(CreateIdentityGuid(), 0, registryClassIdKeyPart + "InprocServer32",
                "Class", className, tlbRecord.AssemblyComponent);

            registryTable.InsertRecord(CreateIdentityGuid(), 0, registryClassIdKeyPart + "InprocServer32",
                "ThreadingModel", "Both", tlbRecord.AssemblyComponent);

            // clr version should have format major.minor.build 
            registryTable.InsertRecord(CreateIdentityGuid(), 0, registryClassIdKeyPart + "InprocServer32",
                "RuntimeVersion", "v"+ Project.TargetFramework.ClrVersion, tlbRecord.AssemblyComponent);

            registryTable.InsertRecord(CreateIdentityGuid(), 0, registryClassIdKeyPart + "InprocServer32",
                "Assembly", tlbRecord.AssemblyName.FullName, tlbRecord.AssemblyComponent);

            registryTable.InsertRecord(CreateIdentityGuid(), 0, registryClassIdKeyPart + "Implemented Categories",
                "+", null, tlbRecord.AssemblyComponent);

            registryTable.InsertRecord(CreateIdentityGuid(), 0, registryClassIdKeyPart + 
                @"Implemented Categories\{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}",
                "+", null, tlbRecord.AssemblyComponent);
        }



        /// <summary>
        /// Loads environment variables for the Environment table.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadEnvironmentVariables(InstallerDatabase database) {

            if (msi.environment != null) {
                // Open the "Environment" Table
                using (InstallerTable envTable = database.OpenTable("Environment")) {
                    foreach (MSIVariable variable in msi.environment) {
                        // Insert the Varible
                        string environmentValue = null;
                        if (variable.append != null && variable.append != "") {
                            environmentValue = "[~];" + variable.append;
                        }
                        envTable.InsertRecord(CreateIdentityGuid(), variable.name, 
                            environmentValue, variable.component);
                    }
                }
            }
        }

        /// <summary>
        /// Loads records for the Registry table.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadRegistry(InstallerDatabase database) {

            if (msi.registry != null) {
                Log(Level.Verbose, LogPrefix + "Adding Registry Values:");

                // Open the "Registry" Table
                using (InstallerTable registryTable = database.OpenTable("Registry")) {
                    foreach (MSIRegistryKey key in msi.registry) {
                        int rootKey = -1;
                        switch (key.root.ToString()) {
                            case "classes":
                                rootKey = 0;
                                break;
                            case "user":
                                rootKey = 1;
                                break;
                            case "machine":
                                rootKey = 2;
                                break;
                            case "users":
                                rootKey = 3;
                                break;
                        }

                        foreach (MSIRegistryKeyValue value in key.value) {
                            if ((value.name == null || value.name == String.Empty) && (value.value == null || value.value == String.Empty))
                                throw new BuildException("Registry value must have a name and/or value specified.");

                            // Insert the Value
                            Log(Level.Verbose, "\t" + GetDisplayablePath(key.path) + @"#" + value.name);

                            string keyValue;
                            if (value.value != null && value.value != "") {
                                keyValue = value.value;
                            } else if (value.dword != null && value.dword != "") {
                                keyValue = "#" + Int32.Parse(value.dword);
                            } else {
                                string val1 = value.Value.Replace(",", null);
                                string val2 = val1.Replace(" ", null);
                                string val3 = val2.Replace("\n", null);
                                string val4 = val3.Replace("\r", null);
                                keyValue = "#x" + val4;
                            }

                            registryTable.InsertRecord((value.id != null ? value.id : CreateIdentityGuid()), 
                                rootKey.ToString(), key.path, value.name, keyValue, key.component);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads records for the RegLocator table
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadRegistryLocators(InstallerDatabase database) {
            // Add properties from Task definition
            if (msi.search != null) {
                Log(Level.Verbose, LogPrefix + "Adding Locators:");

                foreach (searchKey key in msi.search) {
                    switch (key.type.ToString()) {
                        case "registry": {
                            AddRegistryLocaterEntry(database, key);
                            break;
                        }
                        case "file": {
                            break;
                        }
                    }
                }
            }
        }

        private void AddRegistryLocaterEntry(InstallerDatabase database, searchKey key) {
            // Select the "RegLocator" Table
            using (InstallerTable regLocatorTable = database.OpenTable("RegLocator")) {

                int rootKey = -1;
                switch (key.root.ToString()) {
                    case "dependent": {
                        rootKey = -1;
                        break;
                    }
                    case "classes": {
                        rootKey = 0;
                        break;
                    }
                    case "user": {
                        rootKey = 1;
                        break;
                    }
                    case "machine": {
                        rootKey = 2;
                        break;
                    }
                    case "users": {
                        rootKey = 3;
                        break;
                    }
                }

                if (key.value != null) {
                    foreach (searchKeyValue value in key.value) {
                        string signature = "SIG_" + value.setproperty;
                        const int msidbLocatorTypeRawValue = 2;

                        // Insert the signature to the RegLocator Table
                        regLocatorTable.InsertRecord(signature, rootKey.ToString(), key.path,
                            value.name, msidbLocatorTypeRawValue);

                        Log(Level.Verbose, "\t" + GetDisplayablePath(key.path) + @"#" + value.name);
                    }
                }
            }
        }

        /// <summary>
        /// Loads records for the AppSearch table
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadApplicationSearch(InstallerDatabase database) {
            // Add properties from Task definition
            if (msi.search != null) {

                using (InstallerTable appSearchTable = database.OpenTable("AppSearch")) {
                    foreach (searchKey key in msi.search) {
                        switch (key.type.ToString()) {
                            case "registry": {
                                if (key.value != null) {
                                    foreach (searchKeyValue value in key.value) {
                                        appSearchTable.InsertRecord(value.setproperty, "SIG_" + value.setproperty);
                                    }
                                }
                                break;
                            }
                            case "file": {
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads records for the Icon table.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadIconData(InstallerDatabase database) {
            if (msi.icons != null) {
                Log(Level.Verbose, LogPrefix + "Adding Icon Data:");

                // Open the Icon Table
                using (InstallerTable iconTable = database.OpenTable("Icon")) {
                    // Add binary data from Task definition
                    foreach (MSIIcon icon in msi.icons) {
                        string iconPath = Path.Combine(Project.BaseDirectory, icon.value);
                        Log(Level.Verbose, "\t" + iconPath);

                        if (File.Exists(iconPath)) {
                            // Insert the icon data
                            iconTable.InsertRecord(icon.name, new InstallerStream(iconPath));
                        } else {
                            throw new BuildException(String.Format(CultureInfo.InvariantCulture, "Unable to open file:\n\t{0}", iconPath), Location);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads records for the Shortcut table.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadShortcutData(InstallerDatabase database) {
            // Add properties from Task definition
            if (msi.shortcuts != null) {
                Log(Level.Verbose, LogPrefix + "Adding Shortcuts:");

                using (InstallerTable table = database.OpenTable("Shortcut")) {
                    foreach (MSIShortcut shortcut in msi.shortcuts) {
                        Log(Level.Verbose, "\t" + shortcut.name);

                        // Insert the record into the table
                        table.InsertRecord( shortcut.name, shortcut.directory, shortcut.filename, 
                            shortcut.component, shortcut.target, shortcut.arguments,
                            shortcut.description, shortcut.hotkey, shortcut.icon, shortcut.iconindex,
                            shortcut.showcmd, shortcut.wkdir );
                    }
                }
            }
        }

        /// <summary>
        /// Adds custom table(s) to the msi database
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadUserDefinedTables(InstallerDatabase database) {
            // Add properties from Task definition
            if (msi.tables != null) {
                Log(Level.Verbose, LogPrefix + "Adding Tables:");

                foreach (MSITable table in msi.tables) {
                    Log(Level.Verbose, "\t" + table.name);

                    if (!database.VerifyTableExistance(table.name)) {
                        Log(Level.Verbose, "\t\tAdding table structure...");


                        string tableStructureColumns = "";
                        string tableStructureColumnTypes = "";
                        string tableStructureKeys = table.name;
                        bool firstColumn = true;

                        ArrayList columnList = new ArrayList();

                        using (InstallerTable validationTable = database.OpenTable("_Validation")) {
                            foreach (MSITableColumn column in table.columns) { 
                                if (!firstColumn) {
                                    tableStructureColumns += "\t";
                                    tableStructureColumnTypes += "\t";
                                } else {
                                    firstColumn = false;
                                }

                                tableStructureColumns += column.name;

                                tableStructureColumnTypes += GetMsiColumnType(column);
                                if (column.key)
                                    tableStructureKeys += "\t" + column.name;

                                AddToInsertionColumnList(column, columnList);
                                AddColumnValidation(table.name, column, validationTable);
                            }
                        }
                        
                        // Create temp file.  Dump table structure contents into the file
                        // Then import the file.
                        string tableStructureContents = tableStructureColumns + "\n" + tableStructureColumnTypes + "\n" + tableStructureKeys + "\n";
                        try {
                            database.Import(tableStructureContents);
                        } catch (Exception e) {
                            throw new BuildException("Couldn't import tables", Location, e);
                        }

                        Log(Level.Verbose, "Done");

                        if (table.rows != null)
                            AddTableData(database, table.name, table, columnList);

                    }
                }
            }
        }

        private void AddColumnValidation(string tableName, MSITableColumn column, InstallerTable validationTable) {
            string nullability = GetNullability(column);

            string validationCategory = GetValidationCategory(column);

            int minValue;
            bool useMinValue;
            GetMinValue(column, out minValue, out useMinValue);

            int maxValue;
            bool useMaxValue;
            GetMaxValue(column, out maxValue, out useMaxValue);

            validationTable.InsertRecord(tableName, column.name, nullability,
                (useMinValue ? (object) minValue : null),
                (useMaxValue ? (object) maxValue : null), column.keytable,
                (column.keycolumnSpecified ? (object) column.keycolumn : null),
                validationCategory, column.set, column.description);
        }

        private void AddToInsertionColumnList(MSITableColumn column, ArrayList columnList) {
            MSIRowColumnData currentColumn = new MSIRowColumnData();

            currentColumn.name = column.name;
            currentColumn.id = columnList.Count;
            currentColumn.type = GetType(column);
            columnList.Add(currentColumn);
        }

        private string GetMsiColumnType(MSITableColumn column) {
            string columnType;
            if (column.type != null) {
                columnType = column.type;
            } else if (column.categorySpecified) {
                columnType = GetMsiColumnType(column.category, column.nullable);
            } else {
                if (column.nullable) {
                    columnType = "S0";
                }
                else {
                    columnType = "s0";
                }
            }
            return columnType;
        }

        private string GetMsiColumnType(MSITableColumnCategoryType category, bool nullable) {
            if (nullable) {
                return GetNonNullableMsiColumnType(category).ToUpper();
            } else {
                return GetNonNullableMsiColumnType(category);
            }
        }

        private string GetNonNullableMsiColumnType(MSITableColumnCategoryType category) {
            switch (category) {
                case MSITableColumnCategoryType.Text:
                    return "s0";
                case MSITableColumnCategoryType.UpperCase:
                    return "s72";
                case MSITableColumnCategoryType.LowerCase:
                    return "s72";
                case MSITableColumnCategoryType.Integer:
                    return "i2";
                case MSITableColumnCategoryType.DoubleInteger:
                    return "i4";
                case MSITableColumnCategoryType.TimeDate:
                    return "i4";
                case MSITableColumnCategoryType.Identifier:
                    return "s72";
                case MSITableColumnCategoryType.Property:
                    return "s72";
                case MSITableColumnCategoryType.Filename:
                    return "s255";
                case MSITableColumnCategoryType.WildCardFilename:
                    return "l0";
                case MSITableColumnCategoryType.Path:
                    return "s255";
                case MSITableColumnCategoryType.Paths:
                    return "s255";
                case MSITableColumnCategoryType.AnyPath:
                    return "s255";
                case MSITableColumnCategoryType.DefaultDir:
                    return "l255";
                case MSITableColumnCategoryType.RegPath:
                    return "l255";
                case MSITableColumnCategoryType.Formatted:
                    return "s255";
                case MSITableColumnCategoryType.Template:
                    return "l0";
                case MSITableColumnCategoryType.Condition:
                    return "s255";
                case MSITableColumnCategoryType.GUID:
                    return "s38";
                case MSITableColumnCategoryType.Version:
                    return "s32";
                case MSITableColumnCategoryType.Language:
                    return "s255";
                case MSITableColumnCategoryType.Binary:
                    return "v0";
                    // case MSITableColumnCategoryType.CustomSource:
                    // return "s72";
                case MSITableColumnCategoryType.Cabinet:
                    return "s255";
                case MSITableColumnCategoryType.Shortcut:
                    return "s72";
                default:
                    throw new ApplicationException("Unhandled category: "+ category.ToString());
            }
        }

        private string GetType(MSITableColumn column) {
            string currentColumnType;
            if (column.type != null) {
                if (column.type.ToString().ToLower().StartsWith("i"))
                    currentColumnType = "int";
                else if(column.type.ToString().ToLower().StartsWith("v"))
                    currentColumnType = "binary";
                else
                    currentColumnType = "string";
            } else if (column.categorySpecified) {
                currentColumnType = GetType(column.category);
            } else {
                Log(Level.Verbose, LogPrefix + "Must specify a valid category or type.  Defaulting to category type: s0");
                currentColumnType = "string";
            }
            return currentColumnType;
        }

        private string GetType(MSITableColumnCategoryType category) {
            if (IsNumeric(category)) {
                return "int";
            } else if (category == MSITableColumnCategoryType.Binary) {
                return "binary";
            } else {
                return "string";
            }
        }

        private string GetValidationCategory(MSITableColumn column) {
            string columnCategory = null;
            if (!IsNumeric(column.category)) {
                columnCategory = column.category.ToString();
            }
            return columnCategory;
        }

        private void GetMaxValue(MSITableColumn column, out int maxValue, out bool useMaxValue) {
            maxValue = column.maxvalue;
            useMaxValue = column.maxvalueSpecified;
            if (!useMaxValue && IsNumeric(column.category)) {
                useMaxValue = true;
                maxValue = GetMaxValue(column.category);
            }
        }

        private void GetMinValue(MSITableColumn column, out int minValue, out bool useMinValue) {
            minValue = column.minvalue;
            useMinValue = column.minvalueSpecified;
            if (!useMinValue && IsNumeric(column.category)) {
                useMinValue = true;
                minValue = GetMinValue(column.category);
            }
        }

        const string NullableColumn = "Y";
        const string NullableKeyColumn = "@";
        const string NonNullableColumn = "N";

        private string GetNullability(MSITableColumn column) {
            string nullability;
            if (column.nullable) {
                if (column.key) {
                    nullability = NullableKeyColumn;
                }
                else {
                    nullability = NullableColumn;
                }
            }
            else {
                nullability = NonNullableColumn;
            }
            return nullability;
        }

        private bool IsNumeric(MSITableColumnCategoryType category) {
            switch (category) {
                case MSITableColumnCategoryType.Integer:
                case MSITableColumnCategoryType.DoubleInteger:
                case MSITableColumnCategoryType.TimeDate:
                    return true;
                default:
                    return false;
            }
        }

        private int GetMinValue(MSITableColumnCategoryType category) {
            switch (category) {
                case MSITableColumnCategoryType.Integer:
                    return -32767;
                case MSITableColumnCategoryType.DoubleInteger:
                    return -2147483647;
                case MSITableColumnCategoryType.TimeDate:
                    return 0;
                default:
                    throw new ApplicationException("Unhandled category: "+ category.ToString());
            }
        }

        private int GetMaxValue(MSITableColumnCategoryType category) {
            switch (category) {
                case MSITableColumnCategoryType.Integer:
                    return 32767;
                case MSITableColumnCategoryType.DoubleInteger:
                    return 2147483647;
                case MSITableColumnCategoryType.TimeDate:
                    return 2147483647;
                default:
                    throw new ApplicationException("Unhandled category: "+ category.ToString());
            }
        }


        /// <summary>
        /// Adds table data to the msi database table structure
        /// </summary>
        /// <param name="database">The MSI database.</param>
        /// <param name="currentTable">The current table name</param>
        /// <param name="xmlTable">Xml node representing the current table</param>
        /// <param name="columnList">List of column objects for the current table (Containing: column name, id, type).</param>
        private void AddTableData(InstallerDatabase database, string currentTable,
            MSITable xmlTable, ArrayList columnList) {

            Log(Level.Verbose, "\t\tAdding table data...");

            using (InstallerTable table = database.OpenTable(currentTable)) {
                foreach (MSITableRow row in xmlTable.rows) {
                    object[] record = new Object[columnList.Count];

                    // Go through each element defining row data
                    foreach(MSITableRowColumnData columnData in row.columns) {
                        // Create the record and add it
                        // Check to see if the current element equals a specified column.
                        foreach (MSIRowColumnData columnInfo in columnList) {
                            if (columnInfo.name == columnData.name) {
                                if (columnInfo.type == "int") {
                                    record[columnInfo.id] = (object) Convert.ToInt32(columnData.value);
                                }
                                else if (columnInfo.type == "binary") {
                                    record[columnInfo.id] = new InstallerStream(columnData.value);
                                }
                                else {
                                    record[columnInfo.id] = columnData.value;
                                }
                                break;
                            }
                        }
                    }
                    try {
                        table.InsertRecord(record);
                    }
                    catch (Exception e) {
                        Log(Level.Info, LogPrefix + "Incorrect row data format.\n\n" + e.ToString());
                    }
                }
            }

            Log(Level.Verbose, "Done");
        }

        /// <summary>
        /// Loads records for the Binary table.  This table stores items
        /// such as bitmaps, animations, and icons. The binary table is
        /// also used to store data for custom actions.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadBinaryData(InstallerDatabase database) {
            if (msi.binaries != null) {
                Log(Level.Verbose, LogPrefix + "Adding Binary Data:");

                // Open the Binary Table
                using (InstallerTable binaryTable = database.OpenTable("Binary")) {

                    // Add binary data from Task definition
                    foreach (MSIBinary binary in msi.binaries) {
                        string filePath = Path.Combine(Project.BaseDirectory, binary.value);

                        Log(Level.Verbose, "\t" + filePath);

                        int nameColSize = 50;

                        if (binary.name.Length > nameColSize) {
                            Log(Level.Warning, LogPrefix +
                                "WARNING: Binary key name longer than " + nameColSize + " characters:\n\tName: " +
                                binary.name + "\n\tLength: " + binary.name.Length.ToString());

                        }

                        if (File.Exists(filePath)) {
                            binaryTable.InsertRecord(binary.name, new InstallerStream(filePath));
                        }
                        else {
                            throw new BuildException(String.Format(CultureInfo.InvariantCulture, "Unable to open file:\n\t{0}", filePath), Location);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads records for the Dialog table.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadDialogData(InstallerDatabase database) {
            if (msi.dialogs != null) {

                Log(Level.Verbose, LogPrefix + "Adding Dialogs:");

                // Open the Dialog Table
                using (InstallerTable table = database.OpenTable("Dialog")) {
                    foreach (MSIDialog dialog in msi.dialogs) {
                        Log(Level.Verbose, "\t" + dialog.name);

                        // Insert the dialog
                        table.InsertRecord(dialog.name, dialog.hcenter, dialog.vcenter,
                            dialog.width, dialog.height, dialog.attr, dialog.title,
                            dialog.firstcontrol, dialog.defaultcontrol, dialog.cancelcontrol);
                    }
                }
            }
        }


        /// <summary>
        /// Loads records for the Control table.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadDialogControlData(InstallerDatabase database) {
            if (msi.controls != null) {
                Log(Level.Verbose, LogPrefix + "Dialog Controls:");

                using (InstallerTable controlTable = database.OpenTable("Control")) {
                    foreach (MSIControl control in msi.controls) {
                        if (control.remove) {
                            Log(Level.Verbose, "\tRemoving: " + control.name);

                            RemoveDialogControl(database, control);
                        } else {
                            Log(Level.Verbose, "\tAdding:   " + control.name);

                            controlTable.InsertRecord(control.dialog, control.name, control.type, 
                                control.x, control.y, control.width, control.height, control.attr,
                                control.property, control.text, control.nextcontrol, control.help);
                        }
                    }
                }
            }
        }

        private void RemoveDialogControl(InstallerDatabase database, MSIControl control) {
            // Search for a record using all required attributes (even though Dialog_ and Control would suffice)
            using (InstallerRecordReader reader = database.FindRecords("Control", 
                       new InstallerSearchClause("Dialog_", Comparison.Equals, control.dialog),
                       new InstallerSearchClause("Control", Comparison.Equals, control.name),
                       new InstallerSearchClause("Type", Comparison.Equals, control.type),
                       new InstallerSearchClause("X", Comparison.Equals, control.x),
                       new InstallerSearchClause("Y", Comparison.Equals, control.y),
                       new InstallerSearchClause("Width", Comparison.Equals, control.width),
                       new InstallerSearchClause("Height", Comparison.Equals, control.height),
                       new InstallerSearchClause("Attributes", Comparison.Equals, control.attr))) {

                if (reader.Read()) {
                    // If the record is found, delete it
                    reader.DeleteCurrentRecord();
                } else {
                    throw new BuildException(String.Format(CultureInfo.InvariantCulture, 
                        "Control not found: Dialog={0}, Control={1}. One or more of the required attributes do not match.",
                        control.dialog, control.name), Location);
                }
            }
        }



        /// <summary>
        /// Loads records for the ControlCondtion table.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadDialogControlConditionData(InstallerDatabase database) {
            if (msi.controlconditions != null) {
                Log(Level.Verbose, LogPrefix + "Adding Dialog Control Conditions For:");

                using (InstallerTable controlConditionTable = database.OpenTable("ControlCondition")) {
                    foreach (MSIControlCondition controlCondition in msi.controlconditions) {
                        if (controlCondition.remove) {
                            Log(Level.Verbose, "\tRemoving: " + controlCondition.control);

                            RemoveControlCondition(database, controlCondition);
                        } else {
                            Log(Level.Verbose, "\tAdding:   " + controlCondition.control);

                            controlConditionTable.InsertRecord(controlCondition.dialog, controlCondition.control,
                                controlCondition.action, controlCondition.condition);
                        }
                    }
                }
            }
        }

        private void RemoveControlCondition(InstallerDatabase database, MSIControlCondition controlCondition) {
            // Search for a record using all required attributes
            using (InstallerRecordReader reader = database.FindRecords("ControlCondition", 
                       new InstallerSearchClause("Dialog_", Comparison.Equals, controlCondition.dialog),
                       new InstallerSearchClause("Control_", Comparison.Equals, controlCondition.control),
                       new InstallerSearchClause("Action", Comparison.Equals, controlCondition.action),
                       new InstallerSearchClause("Condition", Comparison.Equals, controlCondition.condition))) {

                if (reader.Read()) {
                    // If the record is found, delete it
                    reader.DeleteCurrentRecord();
                } else {
                    throw new BuildException(String.Format(CultureInfo.InvariantCulture, 
                        "ControlEvent not found for removal: Dialog={0}, Control={1}, Action={2}, Condition={3}.",
                        controlCondition.dialog, controlCondition.control, controlCondition.action, controlCondition.condition), Location);
                }
            }
        }



        /// <summary>
        /// Loads records for the ControlEvent table.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadDialogControlEventData(InstallerDatabase database) {
            if (msi.controlevents != null) {
                Log(Level.Verbose, LogPrefix + "Modifying Dialog Control Events:");

                using (InstallerTable controlEventTable = database.OpenTable("ControlEvent")) {
                    foreach (MSIControlEvent controlEvent in msi.controlevents) {
                        Log(Level.Verbose, "\t{0}\tControl: {1}\tEvent: {2}", 
                            (controlEvent.remove ? "Removing" : "Adding"), controlEvent.control, controlEvent.name);

                        if (controlEvent.remove) {
                            RemoveControlEvent(database, controlEvent);
                        } else {
                            controlEventTable.InsertRecord(controlEvent.dialog, controlEvent.control,
                                controlEvent.name, controlEvent.argument, controlEvent.condition, controlEvent.order);
                        }
                    }
                }
            }
        }

        private void RemoveControlEvent(InstallerDatabase database, MSIControlEvent controlEvent) {
            // Search for a record using all required attributes
            using (InstallerRecordReader reader = database.FindRecords("ControlEvent", 
                       new InstallerSearchClause("Dialog_", Comparison.Equals, controlEvent.dialog),
                       new InstallerSearchClause("Control_", Comparison.Equals, controlEvent.control),
                       new InstallerSearchClause("Event", Comparison.Equals, controlEvent.name),
                       new InstallerSearchClause("Argument", Comparison.Equals, controlEvent.argument),
                       new InstallerSearchClause("Condition", Comparison.Equals, controlEvent.condition))) {

                if (reader.Read()) {
                    // If the record is found, delete it
                    reader.DeleteCurrentRecord();
                } else {
                    throw new BuildException(String.Format(CultureInfo.InvariantCulture, 
                        "ControlEvent not found for removal: Dialog={0}, Control={1}, Event={2}, Argument={3}, Condition={4}.",
                        controlEvent.dialog, controlEvent.control, controlEvent.name, controlEvent.argument, controlEvent.condition), Location);
                }
            }
        }



        /// <summary>
        /// Loads records for the CustomAction table
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadCustomAction(InstallerDatabase database) {
            // Add custom actions from Task definition
            if (msi.customactions != null) {
                Log(Level.Verbose, LogPrefix + "Adding Custom Actions:");

                using (InstallerTable customActionTable = database.OpenTable("CustomAction")) {
                    foreach (MSICustomAction customAction in msi.customactions) {
                        Log(Level.Verbose, "\t" + customAction.action);

                        // Insert the record into the table
                        customActionTable.InsertRecord(customAction.action, customAction.type,
                            customAction.source, customAction.target);
                    }
                }
            }
        }

        /// <summary>
        /// Loads records for the ActionText table.  Allows users to specify descriptions/templates for actions.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadActionText(InstallerDatabase database) {
            if (msi.actiontext != null) {
                Log(Level.Verbose, LogPrefix + "Adding ActionText:");

                using (InstallerTable actionTextTable = database.OpenTable("ActionText")) {
                    foreach (MSIActionTextAction action in msi.actiontext) {
                        Log(Level.Verbose, "\t" + action.name);

                        try {
                            actionTextTable.InsertRecord(action.name, action.description, action.template);
                        }
                        catch (Exception) {
                            Log(Level.Warning, LogPrefix + "Warning: Action text for \"" + action.name + "\" already exists in database.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads records for the _AppMappings table.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadAppMappings(InstallerDatabase database) {
            if (msi.appmappings != null) {
                Log(Level.Verbose, LogPrefix + "Adding Application Mappings:");

                using (InstallerTable appmapTable = database.OpenTable("_AppMappings")) {
                    foreach (MSIAppMapping appmap in msi.appmappings) {
                        Log(Level.Verbose, "\t" + appmap.directory);

                        appmapTable.InsertRecord(appmap.directory, appmap.extension, appmap.exepath, appmap.verbs);
                    }
                }
            }
        }

        /// <summary>
        /// Loads records for the _UrlToDir table.
        /// "Load the url properties to convert
        /// url properties to a properties object" ??
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadUrlProperties(InstallerDatabase database) {
            if (msi.urlproperties != null) {
                Log(Level.Verbose, LogPrefix + "Adding URL Properties:");

                using (InstallerTable urlpropTable = database.OpenTable("_UrlToDir")) {
                    foreach (MSIURLProperty urlprop in msi.urlproperties) {
                        Log(Level.Verbose, "\t" + urlprop.name);

                        urlpropTable.InsertRecord(urlprop.name, urlprop.property);
                    }
                }
            }
        }

        /// <summary>
        /// Loads records for the _VDirToUrl table.
        /// Used for converting a vdir to an url
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadVDirProperties(InstallerDatabase database) {
            if (msi.vdirproperties != null) {
                Log(Level.Verbose, LogPrefix + "Adding VDir Properties:");

                using (InstallerTable vdirpropTable = database.OpenTable("_VDirToUrl")) {
                    foreach (MSIVDirProperty vdirprop in msi.vdirproperties) {
                        Log(Level.Verbose, "\t" + vdirprop.name);

                        vdirpropTable.InsertRecord(vdirprop.name, vdirprop.portproperty, vdirprop.urlproperty);
                    }
                }
            }
        }

        /// <summary>
        /// Loads records for the _AppRootCreate table.
        /// Used for making a virtual directory a virtual application
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadAppRootCreate(InstallerDatabase database) {
            if (msi.approots != null) {
                Log(Level.Verbose, LogPrefix + "Adding Application Roots:");

                using (InstallerTable approotTable = database.OpenTable("_AppRootCreate")) {
                    foreach (MSIAppRoot appRoot in msi.approots) {
                        Log(Level.Verbose, "\t" + appRoot.urlproperty);

                        approotTable.InsertRecord(appRoot.component, appRoot.urlproperty, appRoot.inprocflag);
                    }
                }
            }
        }

        /// <summary>
        /// Loads records for the _IISProperties table.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadIISDirectoryProperties(InstallerDatabase database) {
            if (msi.iisproperties != null) {
                Log(Level.Verbose, LogPrefix + "Adding IIS Directory Properties:");

                using (InstallerTable iispropTable = database.OpenTable("_IISProperties")) {
                    foreach (MSIIISProperty iisprop in msi.iisproperties) {
                        Log(Level.Verbose, "\t" + iisprop.directory);

                        iispropTable.InsertRecord(iisprop.directory, iisprop.attr, iisprop.defaultdoc);
                    }
                }
            }
        }

        /// <summary>
        /// Enumerates the registry to see if an assembly has been registered
        /// for COM interop, and if so adds these registry keys to the Registry
        /// table, ProgIds to the ProgId table, classes to the Classes table,
        /// and a TypeLib to the TypeLib table.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        /// <param name="fileName">The Assembly filename.</param>
        /// <param name="fileAssembly">The Assembly to check.</param>
        /// <param name="componentName">The name of the containing component.</param>
        /// <param name="assemblyComponentName">The name of the containing component's assembly GUID.</param>
        /// <param name="classTable">View containing the Class table.</param>
        /// <param name="progIdTable">View containing the ProgId table.</param>
        private void CheckAssemblyForCOMInterop(InstallerDatabase database, string fileName, Assembly fileAssembly,
            string componentName, string assemblyComponentName, InstallerTable classTable, InstallerTable progIdTable) {
            AssemblyName asmName = fileAssembly.GetName();
            string featureName = (string)featureComponents[componentName];
            string typeLibName = Path.GetFileNameWithoutExtension(fileName) + ".tlb";
            string typeLibFileName = Path.Combine(Path.GetDirectoryName(fileName), typeLibName);

            bool foundTypeLib = false;

            // Register the TypeLibrary
            RegistryKey typeLibsKey = Registry.ClassesRoot.OpenSubKey("Typelib", false);

            string[] typeLibs = typeLibsKey.GetSubKeyNames();
            foreach (string typeLib in typeLibs) {
                RegistryKey typeLibKey = typeLibsKey.OpenSubKey(typeLib, false);
                if (typeLibKey != null) {
                    string[] typeLibSubKeys = typeLibKey.GetSubKeyNames();
                    foreach (string typeLibSubKey in typeLibSubKeys) {
                        RegistryKey win32Key = typeLibKey.OpenSubKey(typeLibSubKey + @"\0\win32");
                        if (win32Key != null) {
                            string curTypeLibFileName = (string)win32Key.GetValue(null, null);
                            if (curTypeLibFileName != null) {
                                if (String.Compare(curTypeLibFileName, typeLibFileName, true) == 0) {
                                    Log(Level.Info, LogPrefix + "Configuring " + typeLibName + " for COM Interop...");

                                    TypeLibRecord tlbRecord = new TypeLibRecord(
                                        typeLib, typeLibFileName,
                                        asmName, featureName, assemblyComponentName);

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

                    if (foundTypeLib) {
                        break;
                    }
                }
            }
            typeLibsKey.Close();

            // Register CLSID(s)
            RegistryKey clsidsKey = Registry.ClassesRoot.OpenSubKey("CLSID", false);

            string[] clsids = clsidsKey.GetSubKeyNames();
            foreach (string clsid in clsids) {
                RegistryKey clsidKey = clsidsKey.OpenSubKey(clsid, false);
                if (clsidKey != null) {
                    RegistryKey inprocKey = clsidKey.OpenSubKey("InprocServer32", false);
                    if (inprocKey != null) {
                        string clsidAsmName = (string)inprocKey.GetValue("Assembly", null);
                        if (clsidAsmName != null) {
                            if (asmName.FullName == clsidAsmName) {
                                // Register ProgId(s)
                                RegistryKey progIdKey = clsidKey.OpenSubKey("ProgId", false);
                                if (progIdKey != null) {
                                    string progId = (string)progIdKey.GetValue(null, null);
                                    string className = (string)clsidKey.GetValue(null, null);

                                    if (progId != null) {
                                        progIdTable.InsertRecord(progId, null, clsid, className, null, 0);
                                        classTable.InsertRecord(clsid, "InprocServer32", assemblyComponentName, progId, className, null, null, null, 0, null, null, featureName, 0);
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
        }

        /// <summary>
        /// Loads properties for the Summary Information Stream.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadSummaryInformation(InstallerDatabase database) {
            property productName = null;
            property manufacturer = null;
            property keywords = null;
            property comments = null;

            foreach (property prop in msi.properties) {
                if (prop.name == "ProductName") {
                    productName = prop;
                } else if (prop.name == "Manufacturer") {
                    manufacturer = prop;
                } else if (prop.name == "Keywords") {
                    keywords = prop;
                } else if (prop.name == "Comments") {
                    comments = prop;
                }
            }

            SummaryInfo summaryInfo = database.GetSummaryInformation();
            if (productName != null) {
                summaryInfo.set_Property(2, productName.value);
                summaryInfo.set_Property(3, productName.value);
            }
            if (manufacturer != null) {
                summaryInfo.set_Property(4, manufacturer.value);
            }

            if (keywords != null) {
                summaryInfo.set_Property(5, keywords.value);
            }
            if (comments != null) {
                summaryInfo.set_Property(6, comments.value);
            }

            summaryInfo.set_Property(9, CreateRegistryGuid());

            summaryInfo.set_Property(14, 200);
            summaryInfo.set_Property(15, 2);

            summaryInfo.Persist();
        }

        /// <summary>
        /// Creates a .cab file with all source files included.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void CreateCabFile(InstallerDatabase database) {
            Log(Level.Info, LogPrefix + "Compressing Files...");

            // Create the CabFile
            ProcessStartInfo processInfo = new ProcessStartInfo();

            string shortCabDir = GetShortDir(Path.Combine(Project.BaseDirectory, msi.sourcedir));
            string cabFilePath = shortCabDir + @"\" + CabFileName;
            string tempDir = Path.Combine(msi.sourcedir, "Temp");
            if (tempDir.ToLower().StartsWith(Project.BaseDirectory.ToLower())) {
                tempDir = tempDir.Substring(Project.BaseDirectory.Length+1);
            }

            processInfo.Arguments = "-r -P " + tempDir + @"\ N " + cabFilePath + " " + tempDir + @"\*";

            processInfo.CreateNoWindow = false;
            processInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processInfo.WorkingDirectory = Project.BaseDirectory;
            processInfo.FileName = "cabarc";

            Process process = new Process();
            process.StartInfo = processInfo;
            process.EnableRaisingEvents = true;

            try {
                process.Start();
            }
            catch (Exception e) {
                throw new BuildException("cabarc.exe failed.", Location, e);
            }

            try {
                process.WaitForExit();
            }
            catch (Exception e) {
                throw new BuildException("Error creating cab file.", Location, e);
            }

            if (process.ExitCode != 0) {
                throw new BuildException("Error creating cab file, application returned error " + process.ExitCode, Location);
            }

            if (!process.HasExited) {
                Log(Level.Info,"" );
                Log(Level.Info, "Killing the cabarc process.");
                process.Kill();
            }
            process = null;
            processInfo = null;

            Log(Level.Info, "Done.");

            if (File.Exists(cabFilePath)) {
                Log(Level.Verbose, LogPrefix + "Storing Cabinet in MSI Database...");

                using (InstallerTable cabTable = database.OpenTable("_Streams")) {
                    cabTable.InsertRecord(Path.GetFileName(cabFilePath), new InstallerStream(cabFilePath));
                }
            }
            else {
                throw new BuildException("Unable to open Cabinet file:\n\t" + cabFilePath, Location);
            }
        }

        /// <summary>
        /// Loads records for the sequence tables.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadSequence(InstallerDatabase database) {
            // Add custom actions from Task definition
            if (msi.sequences != null) {
                Log(Level.Verbose, LogPrefix + "Adding Install/Admin Sequences:");

                // Open the sequence tables
                using (InstallerTable
                           installExecuteTable = database.OpenTable("InstallExecuteSequence"),
                           installUITable = database.OpenTable("InstallUISequence"),
                           adminExecuteTable = database.OpenTable("AdminExecuteSequence"),
                           adminUITable = database.OpenTable("AdminUISequence"),
                           advtExecuteTable = database.OpenTable(AdvtExecuteName)) {

                    // Add binary data from Task definition
                    foreach (MSISequence sequence in msi.sequences) {
                        Log(Level.Verbose, "\t" + sequence.action + " to the " + sequence.type.ToString() + "sequence table.");

                        switch(sequence.type.ToString()) {
                            case "installexecute":
                                installExecuteTable.InsertRecord(sequence.action, sequence.condition, sequence.value);
                                break;
                            case "installui":
                                installUITable.InsertRecord(sequence.action, sequence.condition, sequence.value);
                                break;
                            case "adminexecute":
                                adminExecuteTable.InsertRecord(sequence.action, sequence.condition, sequence.value);
                                break;
                            case "adminui":
                                adminUITable.InsertRecord(sequence.action, sequence.condition, sequence.value);
                                break;
                            case "advtexecute":
                                advtExecuteTable.InsertRecord(sequence.action, sequence.condition, sequence.value);
                                break;

                        }
                    }
                }
            }
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
        /// <param name="database">The MSI database.</param>
        /// <param name="directoryTable">The MSI database view.</param>
        /// <param name="Component">The Component's XML Element.</param>
        /// <param name="fileTable">The MSI database view.</param>
        /// <param name="ComponentDirectory">The directory of this file's component.</param>
        /// <param name="ComponentName">The name of this file's component.</param>
        /// <param name="Sequence">The installation sequence number of this file.</param>
        /// <param name="msiAssemblyTable">View containing the MsiAssembly table.</param>
        /// <param name="msiAssemblyNameTable">View containing the MsiAssemblyName table.</param>
        /// <param name="componentTable">View containing the Components table.</param>
        /// <param name="featureComponentTable">View containing the FeatureComponents table.</param>
        /// <param name="classTable">View containing the Class table.</param>
        /// <param name="progIdTable">View containing the ProgId table.</param>
        /// <param name="selfRegTable">View containing the SelfReg table.</param>
        /// <param name="modComponentTable">ModuleComponent table.</param>
        private void AddFiles(InstallerDatabase database, InstallerTable directoryTable, MSIComponent Component,
            InstallerTable fileTable, string ComponentDirectory, string ComponentName, 
            ref int Sequence, InstallerTable msiAssemblyTable, InstallerTable msiAssemblyNameTable,
            InstallerTable componentTable, InstallerTable featureComponentTable, InstallerTable classTable, InstallerTable progIdTable,
            InstallerTable selfRegTable, InstallerTable modComponentTable) {

            XmlElement fileSetElem = (XmlElement)((XmlElement)_xmlNode).SelectSingleNode(
                "components/component[@id='" + Component.id + "']/fileset");

            FileSet componentFiles = new FileSet();
            componentFiles.Project = Project;
            componentFiles.NamespaceManager = NamespaceManager;
            componentFiles.Parent = this;
            componentFiles.Initialize(fileSetElem);

            if (componentFiles.BaseDirectory == null)
                componentFiles.BaseDirectory = new DirectoryInfo(Project.BaseDirectory);

            foreach (string filePath in componentFiles.FileNames) {
                // Insert the File
                string fileName = Path.GetFileName(filePath);

                MSIFileOverride fileOverride = null;

                if (Component.forceid != null) {
                    foreach (MSIFileOverride curOverride in Component.forceid) {
                        if (curOverride.file == fileName) {
                            fileOverride = curOverride;
                            break;
                        }
                    }
                }

                string fileId = fileOverride == null ?
                    CreateIdentityGuid() :
                    fileOverride.id;

                // If the user specifies forceid & specified a file attribute, use it.  Otherwise use the
                // fileattr assigned to the component.
                int fileAttr = ((fileOverride == null) || (fileOverride.attr == 0)) ? Component.fileattr : fileOverride.attr;

                files.Add(Component.directory + "|" + fileName, fileId);

                string fileSize;

                try {
                    fileSize = new FileInfo(filePath).Length.ToString();
                } catch (Exception ex) {
                    throw new BuildException(String.Format(CultureInfo.InvariantCulture, "Could not open file {0}", filePath), Location, ex);
                }

                Log(Level.Verbose, "\t" + filePath);

                // If the file is an assembly, create a new component to contain it,
                // add the new component, map the new component to the old component's
                // feature, and create an entry in the MsiAssembly and MsiAssemblyName
                // table.
                //
                bool isAssembly = false;
                Assembly fileAssembly = null;
                string fileVersion = "";
                try {
                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);
                    fileVersion = fileVersionInfo.FileVersion;
                } catch {}

                try {
                    fileAssembly = Assembly.LoadFrom(filePath);
                    fileVersion = fileAssembly.GetName().Version.ToString();
                    isAssembly = true;
                } catch {}

                string componentFieldValue = null;

                if (isAssembly || filePath.EndsWith(".tlb")) {
                    // The null GUID is authored into any field of a msm database that references a feature.  It gets replaced
                    // with the guid of the feature assigned to the merge module.
                    string feature = "00000000-0000-0000-0000-000000000000";
                    if (featureComponents[ComponentName] != null)
                        feature = (string)featureComponents[ComponentName];

                    string asmCompName = ComponentName;

                    if (componentFiles.FileNames.Count > 1) {
                        asmCompName = "C_" + fileId;
                        componentFieldValue = asmCompName;
                        string newCompId = CreateRegistryGuid();

                        // Add a record for a new Component
                        componentTable.InsertRecord(asmCompName, newCompId, ComponentDirectory, Component.attr, Component.condition, fileId);

                        if (modComponentTable != null) {
                            AddModuleComponentVirtual(database, modComponentTable, asmCompName);
                        }
                        else {
                            // Map the new Component to the existing one's Feature (FeatureComponents is only used in MSI databases)
                            featureComponentTable.InsertRecord(feature, asmCompName);                        
                        }

                    }

                    if (isAssembly) {
                        bool installToGAC = ((fileOverride == null) || (fileOverride.installtogac == false)) ? Component.installassembliestogac : fileOverride.installtogac;
                        // Add a record for a new MsiAssembly
                        if (installToGAC) {
                            msiAssemblyTable.InsertRecord(asmCompName, feature, fileId, null, 0);
                        }
                        else {
                            msiAssemblyTable.InsertRecord(asmCompName, feature, fileId, fileId, 0);
                        }

                        AddAssemblyManifestRecords(fileAssembly, msiAssemblyNameTable, asmCompName);

                        bool checkInterop = Component.checkinterop;

                        if (fileOverride != null) {
                            checkInterop = fileOverride.checkinterop;
                        }

                        if (checkInterop) {
                            CheckAssemblyForCOMInterop(
                                database, filePath, fileAssembly, ComponentName,
                                asmCompName, classTable, progIdTable);
                        }

                        // File can't be a member of both components
                        if (componentFiles.FileNames.Count > 1) {
                            files.Remove(ComponentDirectory + "|" + fileName);
                            files.Add(ComponentDirectory + "|" + fileName, "KeyIsDotNetAssembly");
                        }
                    }
                    else if (filePath.EndsWith(".tlb")) {
                        typeLibComponents.Add(
                            Path.GetFileName(filePath),
                            asmCompName);
                    }
                }

                if (filePath.EndsWith(".dll")) {
                    int hmod = LoadLibrary(filePath);
                    if (hmod != 0) {
                        int regSvr = GetProcAddress(hmod, "DllRegisterServer");
                        if (regSvr != 0) {
                            Log(Level.Info, LogPrefix +
                                "Configuring " +
                                Path.GetFileName(filePath) +
                                " for COM Self Registration...");

                            // Add a record for a new Component
                            selfRegTable.InsertRecord(fileId, null);
                        }
                        FreeLibrary(hmod);
                    }

                    // Register COM .dlls with an embedded
                    // type library for self registration.
                }

                CopyToTempFolder(filePath, fileId);

                if (!isAssembly && !filePath.EndsWith(".tlb")
                    || componentFiles.FileNames.Count == 1) {
                    componentFieldValue = Component.name;
                }

                // Set the file version equal to the override value, if present
                if ((fileOverride != null) && (fileOverride.version != null) && (fileOverride.version != "")) {
                    fileVersion = fileOverride.version;
                }

                if (!IsVersion(ref fileVersion)) {
                    fileVersion = null;
                }

                // propagate language (if available) to File table to avoid 
                // ICE60 verification warnings
                string language = GetLanguage(isAssembly, fileAssembly, filePath);

                Sequence++;

                fileTable.InsertRecord(fileId, componentFieldValue, GetShortFile(filePath) + "|" + fileName, 
                    fileSize, fileVersion, language, fileAttr, Sequence.ToString());
            }
        }

        private void AddAssemblyManifestRecords(Assembly fileAssembly, InstallerTable msiAssemblyNameTable, string asmCompName) {
            AssemblyName asmName = fileAssembly.GetName();

            string version = asmName.Version.ToString(4);

            AssemblyCultureAttribute[] cultureAttrs =
                (AssemblyCultureAttribute[])fileAssembly.GetCustomAttributes(
                typeof(AssemblyCultureAttribute), true);

            string culture = "neutral";
            if (cultureAttrs.Length > 0) {
                culture = cultureAttrs[0].Culture;
            }

            string publicKey = null;
            byte[] keyToken = asmName.GetPublicKeyToken();
            if (keyToken != null) {
                publicKey = ByteArrayToString(keyToken);
            }

            if (asmName.Name != null && asmName.Name != "") {
                msiAssemblyNameTable.InsertRecord(asmCompName, "Name", asmName.Name);
            }

            if (version != null && version != "") {
                msiAssemblyNameTable.InsertRecord(asmCompName, "Version", version);
            }

            if (culture != null && culture != "") {
                msiAssemblyNameTable.InsertRecord(asmCompName, "Culture", culture);
            }

            if (publicKey != null && publicKey != "") {
                msiAssemblyNameTable.InsertRecord(asmCompName, "PublicKeyToken", publicKey);
            }
        }

        private void CopyToTempFolder(string sourceFilePath, string fileId) {
            if (File.Exists(sourceFilePath)) {
                if (!Directory.Exists(TempFolderPath)) {
                    Directory.CreateDirectory(TempFolderPath);
                }

                string newFilePath = Path.Combine(TempFolderPath, fileId);
                File.Copy(sourceFilePath, newFilePath, true);
                // Remove ReadOnly attribute if it exists
                FileAttributes attrs = File.GetAttributes(newFilePath);
                if ((attrs & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                    attrs = attrs ^ FileAttributes.ReadOnly;
                    File.SetAttributes(newFilePath, attrs);
                }
            }
        }

        private string GetLanguage(bool isAssembly, Assembly fileAssembly, string filePath) {
            string language = null;
            try {
                if (isAssembly) {
                    int lcid = fileAssembly.GetName().CultureInfo.LCID;
                    language = (lcid == 0x007F) ? "0" : lcid.ToString(CultureInfo.InvariantCulture);
                } else {
                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);

                    if (!fileVersionInfo.Language.Equals(String.Empty)) {
                        foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.AllCultures)) {
                            if (ci.EnglishName.Equals(fileVersionInfo.Language)) {
                                language = ci.LCID.ToString();
                                break;
                            }
                        }
                    }
                }
            } catch {}
            return language;
        }

        /// <summary>
        /// Loads records for the Components table.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        /// <param name="LastSequence">The sequence number of the last file in the .cab</param>
        private void LoadComponents(InstallerDatabase database, ref int LastSequence) {

            if (msi.components != null) {
                Log(Level.Verbose, LogPrefix + "Add Files:");

                using (InstallerTable
                           msiAssemblyTable = database.OpenTable("MsiAssembly"),
                           msiAssemblyNameTable = database.OpenTable("MsiAssemblyName"),
                           classTable = database.OpenTable("Class"),
                           progIdTable = database.OpenTable("ProgId"),
                           directoryTable = database.OpenTable("Directory"),
                           componentTable = database.OpenTable("Component"),
                           fileTable = database.OpenTable("File"),
                           featureComponentTable = database.OpenTable("FeatureComponents"),
                           selfRegTable = database.OpenTable("SelfReg")) {

                    // Open ModuleComponents table (only exists in MSM archives)
                    InstallerTable modComponentTable = null;
                    if (database.VerifyTableExistance("ModuleComponents")) {
                        modComponentTable = database.OpenTable("ModuleComponents");
                    }

                    try {
                        foreach (MSIComponent component in msi.components) {
                            string keyFileName = component.key.file;

                            if (component.fileset == null) {
                                // Make sure the keyfile maps to a valid registry entry
                                if (((XmlElement)_xmlNode).SelectSingleNode("registry/key[@component='" + component.name + "']/value[@id='" + keyFileName + "']") == null) {
                                    Log(Level.Warning, LogPrefix + "Component '{0}' does not map to a valid registry key.  Skipping...\n", component.name);
                                    continue;
                                }
                            }

                            if (this is MsiCreationCommand)
                                featureComponents.Add(component.name, component.feature);

                            AddModuleComponentVirtual(database, modComponentTable, component.name);

                            if (component.fileset != null) {
                                AddFiles(database, directoryTable, component,
                                    fileTable,
                                    component.directory, component.name, 
                                    ref LastSequence, msiAssemblyTable, msiAssemblyNameTable,
                                    componentTable, featureComponentTable, classTable, progIdTable, selfRegTable, modComponentTable);

                                keyFileName = GetKeyFileName(component);
                            }

                            // Insert the Component
                            componentTable.InsertRecord(component.name, component.id.ToUpper(),
                                component.directory, component.attr, component.condition, keyFileName);
                        }

                        // Add featureComponents from Task definition
                        AddFeatureComponents(featureComponentTable);
                    } finally {
                        if (modComponentTable != null) {
                            modComponentTable.Close();
                        }
                    }
                }
            }
        }

        private string GetKeyFileName(MSIComponent component) {
            string keyFileName;
            if ((component.attr & 4) != 0) {
                keyFileName = component.key.file;
            } else if (files.Contains(component.directory + "|" + component.key.file)) {
                keyFileName = (string)files[component.directory + "|" + component.key.file];
                if (keyFileName == "KeyIsDotNetAssembly") {
                    throw new BuildException("Cannot specify key '" + component.key.file +
                        "' for component '" + component.name + "'. File has been detected as " +
                        "being a COM component or Microsoft.NET assembly and is " +
                        "being registered with its own component. Please specify " +
                        "a different file in the same directory for this component's key.");
                }
            } else {
                throw new BuildException("KeyFile \"" + component.key.file +
                    "\" not found in Component \"" + component.name + "\".");
            }
            return keyFileName;
        }

        private void AddFeatureComponents(InstallerTable featureComponentTable) {
            IEnumerator keyEnum = featureComponents.Keys.GetEnumerator();

            while (keyEnum.MoveNext()) {
                string component = Properties.ExpandProperties((string)keyEnum.Current, Location);
                string feature = Properties.ExpandProperties((string)featureComponents[component], Location);

                if (feature == null) {
                    throw new BuildException("Component " + component +
                        " mapped to nonexistent feature.");
                }

                // Insert the FeatureComponent
                featureComponentTable.InsertRecord(feature, component);
            }
        }

        /// <summary>
        /// Executes the Task.
        /// </summary>
        /// <remarks>None.</remarks>
        public void Execute() {
            string cabFilePath = Path.Combine(Project.BaseDirectory,
                Path.Combine(msi.sourcedir, CabFileName));

            try {
                // Open the Template MSI File
                string source = GetCheckedTemplatePath();
                string errors = GetCheckedErrorTemplatePath();

                CleanOutput(cabFilePath, TempFolderPath);

                string dest = GetDestinationPath();

                // Copy the template MSI file
                CopyFile(source, dest);

                Log(Level.Info, LogPrefix + "Building MSI Database '{0}'.", msi.output);

                // Open the Output Database.
                InstallerDatabase database = new InstallerDatabase(dest);
                database.Open();

                if (msi.debug) {
                    // if debug is true, transform the error strings in
                    database.ApplyTransform(errors);
                }

                int fileSequenceNumber = 0;

                // Load data from the task specification
                LoadCommonDataFromTask(database, ref fileSequenceNumber);
                LoadTypeSpecificDataFromTask(database, fileSequenceNumber);

                // Compress Files
                CreateCabFile(database);

                Log(Level.Info, LogPrefix + "Saving MSI Database...");

                // Commit the MSI Database
                database.Close();
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Unable to build MSI database '{0}'.", msi.output), 
                    Location, ex);
            } finally {
                CleanOutput(cabFilePath, TempFolderPath);
            }
        }

        private void LoadCommonDataFromTask(InstallerDatabase database, ref int fileSequenceNumber) {
            LoadProperties(database);
            LoadRegistryLocators(database);
            LoadApplicationSearch(database);
            LoadUserDefinedTables(database);
            LoadDirectories(database);
            LoadComponents(database, ref fileSequenceNumber);
            LoadDialogData(database);
            LoadDialogControlData(database);
            LoadDialogControlConditionData(database);
            LoadDialogControlEventData(database);
            LoadRegistry(database);
            LoadTypeLibs(database);
            LoadIconData(database);
            LoadShortcutData(database);
            LoadBinaryData(database);
            LoadCustomAction(database);
            LoadSequence(database);
            LoadActionText(database);
            LoadAppMappings(database);
            LoadUrlProperties(database);
            LoadVDirProperties(database);
            LoadAppRootCreate(database);
            LoadIISDirectoryProperties(database);
            LoadEnvironmentVariables(database);
            LoadSummaryInformation(database);
        }

        private void CopyFile(string source, string dest) {
            try {
                File.Copy(source, dest, true);
                File.SetAttributes(dest, System.IO.FileAttributes.Normal);
            } catch (IOException ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture, 
                    "File in use or cannot be copied to output ({0} -> {1}).", 
                    source, dest), Location, ex);
            }
        }

        private string GetDestinationPath() {
            return Path.Combine(Project.BaseDirectory, Path.Combine(msi.sourcedir, msi.output));
        }

        private string GetCheckedErrorTemplatePath() {
            string errors = Path.Combine(TemplateFolder, ErrorTemplateFileName);
            if (msi.errortemplate != null) {
                errors = Path.Combine(Project.BaseDirectory, msi.errortemplate);
            }
            if (!File.Exists(errors)) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Unable to find error template file {0}.", errors), Location);
            }
            return errors;
        }

        private string GetCheckedTemplatePath() {
            string source = Path.Combine(TemplateFolder, TemplateFileName);
            if (msi.template != null) {
                source = Path.Combine(Project.BaseDirectory, msi.template);
            }
            if (!File.Exists(source)) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Unable to find template file {0}.", source), Location);
            }
            return source;
        }

        private string TemplateFolder {
            get {
                // The directory where the Tasks dll resides is used for templates as well
                Module tasksModule = Assembly.GetExecutingAssembly().GetModule("NAnt.Contrib.Tasks.dll");
                return Path.GetDirectoryName(tasksModule.FullyQualifiedName);
            }
        }
    }
}
