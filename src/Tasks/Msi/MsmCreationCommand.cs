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
// Based on original work by James Geurts (jgeurts@users.sourceforge.net)
//

using System;
using System.Xml;

using NAnt.Core;

using NAnt.Contrib.Schemas.Msi;

namespace NAnt.Contrib.Tasks.Msi {
    /// <summary>
    /// Summary description for MsmTaskInfo.
    /// </summary>
    public class MsmCreationCommand : InstallerCreationCommand {
        public MsmCreationCommand(msm msi, Task task, Location location, XmlNode node)
            : base(msi, task, location, node) {
        }

        private msm msi {
            get { return (msm) this.MsiBase; }
        }

        protected override string TemplateResourceName {
            get {
                return "MSMTaskTemplate.msm";
            }
        }

        protected override string ErrorsResourceName {
            get {
                return "MSMTaskErrors.mst";
            }
        }

        protected override void LoadTypeSpecificDataFromTask(InstallerDatabase database, int lastSequence) {
            LoadModuleSignature(database);
            LoadModuleDependency(database);
            LoadModuleExclusion(database);
            LoadModuleSequence(database);
            LoadModuleIgnoreTable(database);
            LoadModuleSubstitution(database);
            LoadModuleConfiguration(database);

            // Commit the MSI Database
            database.Commit();

            ReorderFiles(database, ref lastSequence);

            // Delete unused tables
            Log(Level.Verbose, "Dropping unused tables");
            database.DropEmptyTables(true);
        }

        /// <summary>
        /// Loads records for the ModuleSignature table.
        /// </summary>
        /// <param name="database">The MSM database.</param>
        private void LoadModuleSignature(InstallerDatabase database) {
            if (msi.id != null) {
                Log(Level.Verbose, "Storing Module Signature:\n\tId:\t\t" + msi.id + "\n\tVersion:\t" + msi.version + "\n\tLanguage:\t" + Convert.ToInt32(msi.language));
                
                using (InstallerTable modsigTable = database.OpenTable("ModuleSignature")) {
                    modsigTable.InsertRecord( msi.id, Convert.ToInt32(msi.language), msi.version );
                }
            }
        }


        /// <summary>
        /// Loads records for the ModuleDependency table.
        /// </summary>
        /// <param name="database">The MSM database.</param>
        private void LoadModuleDependency(InstallerDatabase database) {
            if (msi.moduledependencies != null) {
                Log(Level.Verbose, "Adding Module Dependencies:");

                using (InstallerTable modDepTable = database.OpenTable("ModuleDependency")) {

                    foreach (MSMModuleDependency dependency in msi.moduledependencies) {
                        if (dependency.id == null || dependency.id == "") {
                            throw new BuildException("Dependency with no id attribute detected.",
                                Location);
                        }

                        modDepTable.InsertRecord(msi.id, Convert.ToInt32(msi.language), dependency.id, 
                            Convert.ToInt32(dependency.language), dependency.version);

                        Log(Level.Verbose, " - " + dependency.id);
                    }
                }
            }
        }

        /// <summary>
        /// Loads records for the ModuleExclusion table.
        /// </summary>
        /// <param name="database">The MSM database.</param>
        private void LoadModuleExclusion(InstallerDatabase database) {
            if (msi.moduleexclusions != null) {
                Log(Level.Verbose, "Adding Module Exclusions:");

                using (InstallerTable modExTable = database.OpenTable("ModuleExclusion")) {

                    foreach (MSMModuleExclusion exclusion in msi.moduleexclusions) {
                        // Insert the Property
                        if (exclusion.id == null || exclusion.id == "") {
                            throw new BuildException("Exclusion with no id attribute detected.",
                                Location);
                        }

                        modExTable.InsertRecord(msi.id, Convert.ToInt32(msi.language), exclusion.id,
                            Convert.ToInt32(exclusion.language), exclusion.minversion, exclusion.maxversion);

                        Log(Level.Verbose, " - " + exclusion.id);
                    }
                }
            }
        }

        /// <summary>
        /// Loads records for the ModuleInstallUISequence, ModuleInstallExecuteSequence,
        /// ModuleAdminUISequence, ModuleAdminExecute, and ModuleAdvtExecuteSequence tables.
        /// </summary>
        /// <param name="database">The MSM database.</param>
        private void LoadModuleSequence(InstallerDatabase database) {
            // Add custom actions from Task definition
            if (msi.modulesequences != null) {
                Log(Level.Verbose, "Adding Module Install/Admin Sequences:");

                // Open the sequence tables
                using (InstallerTable
                           installExecuteTable = database.OpenTable("ModuleInstallExecuteSequence"),
                           installUITable = database.OpenTable("ModuleInstallUISequence"),
                           adminExecuteTable = database.OpenTable("ModuleAdminExecuteSequence"),
                           adminUITable = database.OpenTable("ModuleAdminUISequence"),
                           advtExecuteTable = database.OpenTable("ModuleAdvtExecuteSequence")) {

                    // Add binary data from Task definition
                    foreach (MSMModuleSequence sequence in msi.modulesequences) {
                        Log(Level.Verbose, "\t" + sequence.action + " to the module" + sequence.type.ToString() + "sequence table.");

                        // Insert the record to the respective table

                        InstallerTable currentTable = null;

                        switch(sequence.type.ToString()) {
                            case "installexecute":
                                currentTable = installExecuteTable;
                                break;
                            case "installui":
                                currentTable = installUITable;
                                break;
                            case "adminexecute":
                                currentTable = adminExecuteTable;
                                break;
                            case "adminui":
                                currentTable = adminUITable;
                                break;
                            case "advtexecute":
                                currentTable = advtExecuteTable;
                                break;
                        }

                        if (currentTable != null) {
                            currentTable.InsertRecord(sequence.action, Convert.ToInt32(sequence.sequence), 
                                sequence.baseaction, Convert.ToInt32(sequence.after), sequence.condition);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads records for the ModuleIgnoreTable table.
        /// </summary>
        /// <param name="database">The MSM database.</param>
        private void LoadModuleIgnoreTable(InstallerDatabase database) {
            if (msi.moduleignoretables != null) {
                Log(Level.Verbose, "Adding Tables To Ignore:");

                using (InstallerTable modIgnoreTableTable = database.OpenTable("ModuleIgnoreTable")) {
                    foreach (MSMModuleIgnoreTable table in msi.moduleignoretables) {
                        if (table.name == null || table.name == "") {
                            throw new BuildException("Table with no name attribute detected.",
                                Location);
                        }

                        modIgnoreTableTable.InsertRecord(table.name);

                        Log(Level.Verbose, "\t" + table.name);
                    }
                }
            }
        }

        /// <summary>
        /// Loads records for the ModuleSubstitution table.
        /// </summary>
        /// <param name="database">The MSM database.</param>
        private void LoadModuleSubstitution(InstallerDatabase database) {
            if (msi.modulesubstitutions != null) {
                Log(Level.Verbose, "Adding Module Substitutions:");

                using (InstallerTable modSubstitutionTable = database.OpenTable("ModuleSubstitution")) {

                    foreach (MSMModuleSubstitution substitution in msi.modulesubstitutions) {
                        if (substitution.table == null || substitution.table == "") {
                            throw new BuildException("Substitution with no table attribute detected.",
                                Location);
                        }

                        modSubstitutionTable.InsertRecord(substitution.table, substitution.row, substitution.column, substitution.value);

                        Log(Level.Verbose, "\tRow: " + substitution.row + "\tColumn: " + substitution.column);
                    }
                }
            }
        }

        /// <summary>
        /// Loads records for the ModuleConfiguration table.
        /// </summary>
        /// <param name="database">The MSM database.</param>
        private void LoadModuleConfiguration(InstallerDatabase database) {
            if (msi.moduleconfigurations != null) {
                Log(Level.Verbose, "Adding Module Configurations:");

                using (InstallerTable modConfigurationTable = database.OpenTable("ModuleConfiguration")) {
                    // Add properties from Task definition
                    foreach (MSMModuleConfiguration configuration in msi.moduleconfigurations) {
                        // Insert the Property

                        int format = 0;

                        switch (configuration.format.ToString()) {
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

                        if (configuration.name == null || configuration.name == "") {
                            throw new BuildException("Configuration with no name attribute detected.",
                                Location);
                        }

                        modConfigurationTable.InsertRecord(configuration.name, format, configuration.type,
                            configuration.contextdata, configuration.defaultvalue, Convert.ToInt32(configuration.attr),
                            configuration.displayname, configuration.description, configuration.helplocation,
                            configuration.helpkeyword);

                        Log(Level.Verbose, "\t" + configuration.name);

                    }
                }
            }
        }

        protected override void AddModuleComponentVirtual(InstallerDatabase database, InstallerTable modComponentTable, string componentName) {
            // Add the new component to the modulecomponents table
            modComponentTable.InsertRecord(componentName, msi.id, Convert.ToInt32(msi.language));
        }


        protected override string CabFileName {
            get {
                return "MergeModule.CABinet";
            }
        }

        protected override string AdvtExecuteName {
            get {
                return "ModuleAdvtExecuteSequence";
            }
        }
    }
}
