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
using System.Xml;

using MsmMergeTypeLib;

using NAnt.Core;
using NAnt.Core.Types;

using NAnt.Contrib.Schemas.Msi;

namespace NAnt.Contrib.Tasks.Msi {
    /// <summary>
    /// Summary description for MsiTaskInfo.
    /// </summary>
    public class MsiCreationCommand : InstallerCreationCommand {
        public MsiCreationCommand(msi msi, Task task, Location location, XmlNode node)
            : base(msi, task, location, node) {
        }

        private msi msi {
            get { return (msi) this.MsiBase; }
        }

        protected override string TemplateFileName {
            get {
                return "MSITaskTemplate.msi";
            }
        }

        protected override string ErrorTemplateFileName {
            get {
                return "MSITaskErrors.mst";
            }
        }

        protected override string CabFileName {
            get {
                return Path.GetFileNameWithoutExtension(msi.output) + ".cab";
            }
        }

        protected override string AdvtExecuteName {
            get {
                return "AdvtExecuteSequence";
            }
        }

        protected override void AddModuleComponentVirtual(InstallerDatabase database, InstallerTable modComponentTable, string componentName) {
        }


        protected override void LoadTypeSpecificDataFromTask(InstallerDatabase database, int lastSequence) {
            LoadLaunchCondition(database);
            LoadFeatures(database);

            // The database file must be closed for merging to succeed
            database.Close();

            LoadMergeModules(database.ArchivePath, TempFolderPath);

            // Reopen database after merging
            database.Open();

            ReorderFiles(database, ref lastSequence);
            LoadMedia(database, lastSequence);
            LoadBannerImage(database);
            LoadBackgroundImage(database);
            LoadLicense(database);
        }

        /// <summary>
        /// Loads the banner image.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadBannerImage(InstallerDatabase database) {
            // Try to open the Banner
            if (msi.banner != null) {
                string bannerFile = Path.Combine(Project.BaseDirectory, msi.banner);
                if (File.Exists(bannerFile)) {
                    Log(Level.Verbose, LogPrefix + "Storing Banner:\n\t" + bannerFile);

                    using (InstallerRecordReader reader = database.FindRecords("Binary",
                               new InstallerSearchClause("Name", Comparison.Equals, "bannrbmp"))) {
                        if (reader.Read()) {
                            // Write the Banner file to the MSI database
                            reader.SetValue(1, new InstallerStream(bannerFile));
                            reader.Commit();
                        } else {
                            throw new BuildException("Banner Binary record not found in template database.");
                        }
                    }
                }
                else {
                    throw new BuildException(String.Format(CultureInfo.InvariantCulture, "Unable to open Banner Image:\n\t{0}", bannerFile), Location);
                }
            }
        }

        /// <summary>
        /// Loads the background image.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadBackgroundImage(InstallerDatabase database) {
            // Try to open the Background
            if (msi.background != null) {
                string bgFile = Path.Combine(Project.BaseDirectory, msi.background);
                if (File.Exists(bgFile)) {
                    Log(Level.Info, LogPrefix + "Storing Background:\n\t" + bgFile);

                    using (InstallerRecordReader reader = database.FindRecords("Binary",
                               new InstallerSearchClause("Name", Comparison.Equals, "dlgbmp"))) {
                        if (reader.Read()) {
                            // Write the Background file to the MSI database
                            reader.SetValue(1, new InstallerStream(bgFile));
                            reader.Commit();
                        } else {
                            throw new BuildException("Background Binary record not found in template database.");
                        }
                    }
                }
                else {
                    throw new BuildException(String.Format(CultureInfo.InvariantCulture, "Unable to open Background Image:\n\t{0}", bgFile), Location);
                }
            }
        }

        /// <summary>
        /// Loads the license file.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadLicense(InstallerDatabase database) {

            // Find the License control
            using (InstallerRecordReader recordReader = database.FindRecords("Control",
                       new InstallerSearchClause("Control", Comparison.Equals, "AgreementText"))) {

                if (recordReader.Read()) {
                    if (msi.license != null) {
                        string licFile = Path.Combine(Project.BaseDirectory, msi.license);
                        Log(Level.Info, LogPrefix + "Storing license '{0}'.", licFile);
                        StreamReader licenseFileReader = null;
                        try {
                            licenseFileReader = File.OpenText(licFile);
                        } catch (IOException ex) {
                            throw new BuildException(String.Format(CultureInfo.InvariantCulture,
                                "Unable to open License File:\n\t{0}", licFile), Location, ex);
                        }

                        try {
                            recordReader.SetValue(9, licenseFileReader.ReadToEnd());
                            recordReader.Commit();
                        } finally {
                            licenseFileReader.Close();
                        }
                    } else {
                        // Delete the license control
                        recordReader.DeleteCurrentRecord();
                    }
                } else {
                    throw new BuildException("Couldn't find AgreementText Control in template database.", Location);
                }
            }
        }

        /// <summary>
        /// Loads records for the Media table.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        /// <param name="LastSequence">The sequence number of the last file in the .cab.</param>
        private void LoadMedia(InstallerDatabase database, int LastSequence) {
            // Open the "Media" Table
            using (InstallerTable mediaTable = database.OpenTable("Media")) {
                mediaTable.InsertRecord( "1", LastSequence.ToString(), null, 
                    "#" + Path.GetFileNameWithoutExtension(msi.output) + ".cab", null, null );
            }
        }

        /// <summary>
        /// Loads records for the Features table.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadFeatures(InstallerDatabase database) {
            Log(Level.Verbose, LogPrefix + "Adding Features:");

            using (InstallerTable
                       featureTable = database.OpenTable("Feature"),
                       conditionTable = database.OpenTable("Condition")) {
                // Add features from Task definition
                int order = 1;
                int depth = 1;

                foreach (MSIFeature feature in msi.features) {
                    AddFeature(featureTable, conditionTable, null, database, feature, depth, order);
                    order++;
                }
            }
        }



        /// <summary>
        /// Adds a feature record to the Features table.
        /// </summary>
        /// <param name="featureTable">The MSI database Feature table.</param>
        /// <param name="conditionTable">The MSI database Condition table.</param>
        /// <param name="ParentFeature">The name of this feature's parent.</param>
        /// <param name="database">The MSI database.</param>
        /// <param name="Feature">This Feature's Schema element.</param>
        /// <param name="Depth">The tree depth of this feature.</param>
        /// <param name="Order">The tree order of this feature.</param>
        private void AddFeature(InstallerTable featureTable, InstallerTable conditionTable, string ParentFeature,
            InstallerDatabase database, MSIFeature Feature, int Depth, int Order) {
            const int TypicalInstallLevel = 3;
            const int NonTypicalInstallLevel = 4;

            int featureInstallLevel = ( Feature.typical ? TypicalInstallLevel : NonTypicalInstallLevel );

            // Insert the Feature
            featureTable.InsertRecord( Feature.name, ParentFeature, Feature.title, Feature.description, Feature.display, 
                ( Feature.typical ? TypicalInstallLevel : NonTypicalInstallLevel ), GetFeatureDirectory(Feature), Feature.attr );

            Log(Level.Verbose, "\t" + Feature.name);

            AddFeatureConditions(Feature, conditionTable);

            if (Feature.feature != null) {
                foreach (MSIFeature childFeature in Feature.feature) {
                    int newDepth = Depth + 1;
                    int newOrder = 1;

                    AddFeature(featureTable, conditionTable, Feature.name,
                        database, childFeature, newDepth, newOrder);
                    newOrder++;
                }
            }
        }

        private string GetFeatureDirectory(MSIFeature Feature) {
            if (Feature.directory != null) {
                return Feature.directory;
            } else {
                IEnumerator featComps = featureComponents.Keys.GetEnumerator();

                while (featComps.MoveNext()) {
                    string componentName = (string)featComps.Current;
                    string featureName = (string)featureComponents[componentName];

                    if (featureName == Feature.name) {
                        return (string)components[componentName];
                    }
                }

                throw new BuildException("Feature " + Feature.name +
                    " needs to be assigned a component or directory.", Location);
            }
        }

        private void AddFeatureConditions(MSIFeature Feature, InstallerTable conditionTable) {
            if (Feature.conditions != null) {
                Log(Level.Verbose, "\t\tAdding Feature Conditions...");

                foreach (MSIFeatureCondition featureCondition in Feature.conditions) {
                    try {
                        // Insert the feature's condition
                        conditionTable.InsertRecord(Feature.name, featureCondition.level, featureCondition.expression);
                    }
                    catch (Exception e) {
                        Log(Level.Info, "\nError adding feature condition: " + e.ToString());
                    }
                }

                Log(Level.Verbose, "Done");
            }
        }



        /// <summary>
        /// Loads records for the LaunchCondition table
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadLaunchCondition(InstallerDatabase database) {
            // Add properties from Task definition
            if (msi.launchconditions != null) {
                Log(Level.Verbose, LogPrefix + "Adding Launch Conditions:");

                // Open the Launch Condition Table
                using (InstallerTable table = database.OpenTable("LaunchCondition")) {
                    // Add binary data from Task definition
                    foreach (MSILaunchCondition launchCondition in msi.launchconditions) {
                        Log(Level.Verbose, "\t" + launchCondition.name);

                        table.InsertRecord(launchCondition.condition, launchCondition.description);
                    }
                }
            }
        }



        /// <summary>
        /// Merges Merge Modules into the MSI Database.
        /// </summary>
        /// <param name="Database">The MSI Database.</param>
        /// <param name="TempPath">The path to temporary files.</param>
        private void LoadMergeModules(string Database, string TempPath) {
            // If <mergemodules>...</mergemodules> exists in the nant msi task

            if (msi.mergemodules != null) {
                MsmMergeClass mergeClass = new MsmMergeClass();

                int index = 1;

                Log(Level.Verbose, LogPrefix + "Storing Merge Modules:");

                if (!Directory.Exists(TempPath))
                    Directory.CreateDirectory(TempPath);

                // Merge module(s) assigned to a specific feature
                foreach (MSIMerge merge in msi.mergemodules) {
                    // Get each merge module file name assigned to this feature
                    NAntFileSet modules = merge.modules;

                    FileSet mergeSet = new FileSet();
                    mergeSet.Parent = this;
                    mergeSet.Project = Project;
					mergeSet.NamespaceManager = NamespaceManager;

                    XmlElement modulesElem = (XmlElement)((XmlElement)_xmlNode).SelectSingleNode(
                        "nant:mergemodules/nant:merge[@feature='" + merge.feature + "']/nant:modules", 
						NamespaceManager);

                    mergeSet.Initialize(modulesElem);

                    try {
                        mergeClass.OpenDatabase(Database);
                    }
                    catch (FileLoadException fle) {
                        throw new BuildException("Error while opening the database for merging.", Location, fle);
                    }

                    // Iterate each module assigned to this feature
                    foreach (string mergeModule in mergeSet.FileNames) {
                        Log(Level.Verbose, "\t" + Path.GetFileName(mergeModule));

                        // Open the merge module (by Filename)
                        try {
                            mergeClass.OpenModule(mergeModule, 1033);
                        } catch {
                            throw new BuildException("File " + mergeModule + " is not found.", Location);
                        }

                        // Once the merge is complete, components in the module are attached to the
                        // feature identified by Feature. This feature is not created and must be
                        // an existing feature. Note that the Merge method gets all the feature
                        // references in the module and substitutes the feature reference for all
                        // occurrences of the null GUID in the module database.
                        mergeClass.Merge(merge.feature, null);

                        string moduleCab = Path.Combine(Path.GetDirectoryName(Database),
                            "mergemodule" + index + ".cab");

                        index++;

                        mergeClass.ExtractCAB(moduleCab);

                        if (File.Exists(moduleCab)) {
                            // Extract the cabfile contents to a Temp directory
                            try {
                                ExtractCabFileToTempDirectory(moduleCab);
                            } finally {
                                File.Delete(moduleCab);
                            }
                        }
                        mergeClass.CloseModule();
                    }
                    // Close and save the database
                    mergeClass.CloseDatabase(true);

                }
            }
        }

        private void ExtractCabFileToTempDirectory(string moduleCab) {
            Process process = new Process();
            ProcessStartInfo processInfo = new ProcessStartInfo();

            processInfo.Arguments = "-o X " +
                moduleCab + " " + Path.Combine(Project.BaseDirectory, Path.Combine(msi.sourcedir, @"Temp\"));

            processInfo.CreateNoWindow = false;
            processInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processInfo.WorkingDirectory = msi.output;
            processInfo.FileName = "cabarc";

            process.StartInfo = processInfo;
            process.EnableRaisingEvents = true;

            try {
                process.Start();
            } catch (Exception ex) {
                throw new BuildException("cabarc.exe failed.", Location, ex);
            }

            try {
                process.WaitForExit();
            }
            catch (Exception e) {
                throw new BuildException("Error extracting merge module cab file: " + moduleCab, Location, e);
            }

            if (process.ExitCode != 0) {
                throw new BuildException("Error extracting merge module cab file: " + moduleCab + "\nApplication returned ERROR: " + process.ExitCode, Location);
            }
        }
    }
}
