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

        protected override string TemplateResourceName {
            get { return "MSITaskTemplate.msi"; }
        }

        protected override string ErrorsResourceName {
            get { return "MSITaskErrors.mst"; }
        }

        protected override string CabFileName {
            get {
                return Path.GetFileNameWithoutExtension(msi.output) + ".cab";
            }
        }

        protected override string AdvtExecuteName {
            get { return "AdvtExecuteSequence"; }
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
                    Log(Level.Verbose, "Storing banner '{0}'.", bannerFile);

                    using (InstallerRecordReader reader = database.FindRecords("Binary", new InstallerSearchClause("Name", Comparison.Equals, "bannrbmp"))) {
                        if (reader.Read()) {
                            // Write the Banner file to the MSI database
                            reader.SetValue(1, new InstallerStream(bannerFile));
                            reader.Commit();
                        } else {
                            throw new BuildException("Banner Binary record not found in template database.",
                                Location);
                        }
                    }
                } else {
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture, 
                        "Unable to open banner image '{0}'.", bannerFile), Location);
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
                    Log(Level.Info, "Storing background '{0}'.", bgFile);

                    using (InstallerRecordReader reader = database.FindRecords("Binary", new InstallerSearchClause("Name", Comparison.Equals, "dlgbmp"))) {
                        if (reader.Read()) {
                            // Write the Background file to the MSI database
                            reader.SetValue(1, new InstallerStream(bgFile));
                            reader.Commit();
                        } else {
                            throw new BuildException("Background Binary record not found in template database.",
                                Location);
                        }
                    }
                } else {
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture, 
                        "Unable to open background image '{0}'.", bgFile), Location);
                }
            }
        }

        /// <summary>
        /// Loads the license file.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadLicense(InstallerDatabase database) {
            if (msi.license != null) {
                // Find the License control
                using (InstallerRecordReader recordReader = database.FindRecords("Control", new InstallerSearchClause("Control", Comparison.Equals, "AgreementText"))) {
                    if (recordReader.Read()) {
                        string licFile = Path.Combine(Project.BaseDirectory, msi.license);
                        Log(Level.Verbose, "Storing license '{0}'.", licFile);

                        // make sure license exists
                        if (!File.Exists(licFile)) {
                            throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                                "License file '{0}' does not exist.", licFile),
                                Location);
                        }

                        StreamReader licenseFileReader = null;
                        try {
                            licenseFileReader = File.OpenText(licFile);
                        } catch (IOException ex) {
                            throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                                "Unable to open license file '{0}'.", licFile), 
                                Location, ex);
                        }

                        try {
                            recordReader.SetValue(9, licenseFileReader.ReadToEnd());
                            recordReader.Commit();
                        } finally {
                            licenseFileReader.Close();
                        }
                    } else {
                        throw new BuildException("Couldn't find AgreementText Control in template database.", 
                            Location);
                    }
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
                mediaTable.InsertRecord("1", LastSequence.ToString(), null, 
                    "#" + Path.GetFileNameWithoutExtension(msi.output) + ".cab", null, null);
            }
        }

        /// <summary>
        /// Loads records for the Features table.
        /// </summary>
        /// <param name="database">The MSI database.</param>
        private void LoadFeatures(InstallerDatabase database) {
            if (msi.features == null) {
                return;
            }

            Log(Level.Verbose, "Adding Features:");

            using (InstallerTable featureTable = database.OpenTable("Feature"), conditionTable = database.OpenTable("Condition")) {
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

            int featureInstallLevel = Feature.typical ? TypicalInstallLevel
                : NonTypicalInstallLevel;

            // Insert the Feature
            featureTable.InsertRecord(Feature.name, ParentFeature, Feature.title,
                Feature.description, Feature.display, featureInstallLevel,
                GetFeatureDirectory(Feature), Feature.attr);

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

                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Feature '{0}' needs to be assigned a component or directory.", 
                    Feature.name), Location);
            }
        }

        private void AddFeatureConditions(MSIFeature Feature, InstallerTable conditionTable) {
            if (Feature.conditions != null) {
                Log(Level.Verbose, "\t\tAdding Feature Conditions...");

                foreach (MSIFeatureCondition featureCondition in Feature.conditions) {
                    try {
                        // Insert the feature's condition
                        conditionTable.InsertRecord(Feature.name, featureCondition.level, featureCondition.expression);
                    } catch (Exception ex) {
                        Log(Level.Info, "Error adding feature condition: " + ex.Message);
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
                Log(Level.Verbose, "Adding Launch Conditions:");

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
                MsmMerge2Class mergeClass = new MsmMerge2Class();

                int index = 1;

                Log(Level.Verbose, "Storing Merge Modules:");

                if (!Directory.Exists(TempPath))
                    Directory.CreateDirectory(TempPath);

                // Merge module(s) assigned to a specific feature
                foreach (MSIMerge merge in msi.mergemodules) {
                    // Get each merge module file name assigned to this feature
                    //NAntFileSet modules = merge.modules;

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
                    } catch (FileLoadException fle) {
                        throw new BuildException("Error while opening the database for merging.", 
                            Location, fle);
                    }

                    // Iterate each module assigned to this feature
                    foreach (string mergeModule in mergeSet.FileNames) {
                        Log(Level.Verbose, "\t" + Path.GetFileName(mergeModule));

                        // Open the merge module (by Filename)
                        try {
                            mergeClass.OpenModule(mergeModule, 1033);
                        } catch (Exception ex) {
                            throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                                "File '{0}' cannot be opened.", mergeModule), Location, ex);
                        }

                        // Once the merge is complete, components in the module are attached to the
                        // feature identified by Feature. This feature is not created and must be
                        // an existing feature. Note that the Merge method gets all the feature
                        // references in the module and substitutes the feature reference for all
                        // occurrences of the null GUID in the module database.

                        if (merge.configurationitems != null) {
                            Log(Level.Verbose, "\t\tConfigurable item(s):");
                            Hashtable configurations = new Hashtable();
                            foreach (MSIConfigurationItem configItem in merge.configurationitems) {
                                if ((configItem.module == null || configItem.module.Equals(String.Empty)) || configItem.module.ToLower().Equals(mergeModule.ToLower()) || configItem.module.ToLower().Equals(Path.GetFileName(mergeModule.ToLower()))) {
                                    if (configItem.module == null || configItem.module.Equals(String.Empty)) {
                                        if (configurations[configItem.name] == null) {
                                            configurations[configItem.name] = configItem.value;
                                            Log(Level.Verbose, "\t\t\t{0}\tValue: {1}", 
                                                configItem.name, configItem.value);
                                        }
                                    } else {
                                        configurations[configItem.name] = configItem.value;
                                        Log(Level.Verbose, "\t\t\t{0}\tValue: {1}", 
                                            configItem.name, configItem.value);
                                    }
                                }
                            }
                            mergeClass.MergeEx(merge.feature, null, new MsmConfigureModule(configurations));
                        } else {
                            mergeClass.Merge(merge.feature, null);
                        }

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

            // paths to cabarc need to be quoted, however the destination path
            // must end in a \ so the last quote is left of (this seems to be ok)
            // cabarc does not work (nor does it raise an error) if you end the 
            // destination path with a quote.
            processInfo.Arguments = "-o X \"" +
                moduleCab + "\" \"" + TempFolderPath+ @"\";

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
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Error extracting merge module cab file '{0}'.", moduleCab), 
                    Location, ex);
            }

            if (process.ExitCode != 0) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Error extracting merge module cab file '{0}'.\nExit code: {1}.", 
                    moduleCab, process.ExitCode), Location);
            }
        }
    }

    internal class MsmConfigureModule : IMsmConfigureModule {
        #region Private Instance Fields

        private Hashtable _configurations;

        #endregion Private Instance Fields

        #region Internal Instance Constructors

        internal MsmConfigureModule(Hashtable configurations) {
            _configurations = configurations;
        }

        #endregion Internal Instance Constructors

        #region Implementation of IMsmConfigureModule 

        string MsmMergeTypeLib.IMsmConfigureModule.ProvideTextData(string name) {
            if (_configurations[name] != null) {
                return (string) _configurations[name];
            }
            return null;
        }

        int MsmMergeTypeLib.IMsmConfigureModule.ProvideIntegerData(string name) {
            if (_configurations[name] != null) {
                return int.Parse((string)_configurations[name], 
                    CultureInfo.InvariantCulture);
            }
            return 0;
        }

        #endregion Implementation of IMsmConfigureModule 
    }
}
