//
// NAntContrib
// Copyright (C) 2002 Tomas Restrepo (tomasr@mvps.org)
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307 USA
//

using System;
using System.IO;

using InterOpStarTeam = StarTeam;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.StarTeam {
    /// <summary>
    /// Task to check in files to StarTeam repositories. 
    /// </summary>
    /// <remarks>
    /// <para>You add files to the repository that are not controlled by setting <see cref="adduncontrolled" />.</para>
    /// <para>This task was ported from the Ant task http://jakarta.apache.org/ant/manual/OptionalTasks/starteam.html#stcheckin </para>
    /// <para>You need to have the StarTeam SDK installed for this task to function correctly.</para>
    /// </remarks>
    /// <example>
    ///   <para>Recursively checks in all files in the project.</para>
    ///   <code>
    ///     <![CDATA[
    /// <!-- 
    ///   constructs a 'url' containing connection information to pass to the task 
    ///   alternatively you can set each attribute manually 
    /// -->
    /// <property name="ST.url" value="user:pass@serverhost:49201/projectname/viewname" />
    /// <stcheckin forced="true" rootstarteamfolder="/" recursive="true" url="${ST.url}" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("stcheckin")]
    public class StarTeamCheckin : TreeBasedTask {
        /// <summary>classes used to access static values</summary>
        private InterOpStarTeam.StItem_LockTypeStaticsClass starTeamLockTypeStatics = new InterOpStarTeam.StItem_LockTypeStaticsClass(); 
        private InterOpStarTeam.StStatusStaticsClass starTeamStatusStatics = new InterOpStarTeam.StStatusStaticsClass();

        /// <summary>Facotry classes used when files and folders are added to the repository. Populated when adduncontrolled is enabled.</summary>
        private InterOpStarTeam.StFolderFactoryClass starteamFolderFactory;
        private InterOpStarTeam.StFileFactoryClass starteamFileFactory;

        public StarTeamCheckin() {
            _lockStatus = starTeamLockTypeStatics.UNLOCKED; 
            this.recursive = false;
        }

        [TaskAttribute("comment")]
        public virtual string comment {
            get { return _comment; }
            set { _comment = value; }
        }

        /// <summary> 
        /// if true, any files or folders NOT in StarTeam will be added to the repository.  Defaults to "false".
        /// </summary>
        [TaskAttribute("adduncontrolled")]
        public virtual bool adduncontrolled {
            get { return _addUncontrolled; }
            set { 
                _addUncontrolled = value; 
                if(_addUncontrolled) {
                    //Instantiated here as they are only necessary when adding 
                    starteamFolderFactory = new InterOpStarTeam.StFolderFactoryClass();
                    starteamFileFactory = new InterOpStarTeam.StFileFactoryClass();
                }
            }
        }

        /// <summary> 
        /// Set to do an unlocked checkout; optional, default is false; 
        /// If true, file will be unlocked so that other users may change it.  If false, lock status will not change. 
        /// </summary>
        [TaskAttribute("unlocked")]
        public virtual bool unlocked {
            set {
                if (value) {
                    _lockStatus = starTeamLockTypeStatics.UNLOCKED;
                }
                else {
                    _lockStatus = starTeamLockTypeStatics.UNCHANGED;
                }
            }
        }

        /// <value> will folders be created for new items found with the checkin.</value>
        private bool _createFolders = false;

        /// <value> The comment which will be stored with the checkin.</value>
        private string _comment = null;

        /// <value> holder for the add Uncontrolled attribute.  If true, all local files not in StarTeam will be added to the repository.</value>
        private bool _addUncontrolled = false;
        private InterOpStarTeam.IStLabel _stLabel = null;

        /// <value> This attribute tells whether unlocked files on checkin (so that other users may access them) checkout or to 
        /// leave the checkout status alone (default).
        /// </value>
        private int _lockStatus;

        /// <summary>
        /// Override of base-class abstract function creates an appropriately configured view.  For checkins this is
        /// always the current or "tip" view.
        /// </summary>
        /// <param name="raw">the unconfigured <code>View</code></param>
        /// <returns>the snapshot <code>View</code> appropriately configured.</returns>
        protected internal override InterOpStarTeam.StView createSnapshotView(InterOpStarTeam.StView raw) {
            InterOpStarTeam.StView snapshot;
            InterOpStarTeam.StViewConfigurationStaticsClass starTeamViewConfiguration = new InterOpStarTeam.StViewConfigurationStaticsClass();
            InterOpStarTeam.StViewFactory starTeamViewFactory = new InterOpStarTeam.StViewFactory();
            snapshot = starTeamViewFactory.Create(raw, starTeamViewConfiguration.createTip());
            if(_label != string.Empty) {
                _stLabel = this.getLabelID(snapshot);
            }
            return snapshot;
        }

        /// <summary> Implements base-class abstract function to define tests for any preconditons required by the task</summary>
        protected override void testPreconditions() {
            if (null != _rootLocalFolder && this.Forced) {
                Log(Level.Warning, "rootLocalFolder specified, but forcing off.");
            }
        }

        /// <summary> Implements base-class abstract function to perform the checkin operation on the files in each folder of the tree.</summary>
        /// <param name="starteamFolder">the StarTeam folder to which files will be checked in</param>
        /// <param name="targetFolder">local folder from which files will be checked in</param>
        protected override void visit(InterOpStarTeam.StFolder starteamFolder, FileInfo targetFolder) {
            int notProcessed = 0;
            int notMatched = 0;

            try {
                System.Collections.Hashtable localFiles = listLocalFiles(targetFolder);

                // If we have been told to create the working folders
                // For all Files in this folder, we need to check
                // if there have been modifications.

                foreach(InterOpStarTeam.StFile stFile in starteamFolder.getItems("File")) {
                    string filename = stFile.Name;
                    FileInfo localFile = new FileInfo(Path.Combine(targetFolder.FullName, filename));

                    delistLocalFile(localFiles, localFile);

                    // If the file doesn't pass the include/exclude tests, skip it.
                    if (!IsIncluded(filename)) {
                        if(this.Verbose) {
                            Log(Level.Info, "Skipping : {0}",localFile.ToString());
                        }
                        notMatched++;
                        continue;
                    }

                    // If forced is not set then we may save ourselves some work by looking at the status flag.
                    // Otherwise, we care nothing about these statuses.

                    if (!this.Forced) {
                        int fileStatus = (stFile.Status);

                        // We try to update the status once to give StarTeam another chance.
                        if (fileStatus == starTeamStatusStatics.merge || fileStatus == starTeamStatusStatics.UNKNOWN) {
                            stFile.updateStatus(true, true);
                            fileStatus = (stFile.Status);
                        }
                        if(fileStatus == starTeamStatusStatics.merge) {
                            Log(Level.Info, "Not processing {0} as it needs Merging and Forced is not on.",stFile.toString());
                            continue;
                        }
                        if (fileStatus == starTeamStatusStatics.CURRENT) {
                            //count files not processed so we can inform later
                            notProcessed++;
                            continue;
                        }
                    }

                    //may want to offer this to be surpressed but usually it is a good idea to have
                    //in the build log what changed for that build.
                    Log(Level.Info, "Checking In: {0}", localFile.ToString());

                    //check in anything else
                    stFile.checkinFrom(localFile.FullName, _comment, _lockStatus, true, true, true);

                    _updateLabel(stFile);

                    //track files affected for non-verbose output
                    _filesAffected++;
                }

                //if we are being verbose emit count of files not processed 
                if(this.Verbose) {
                    if(notProcessed > 0) 
                        Log(Level.Info, "{0} : {1} files not processed because they were current.",
                            targetFolder.FullName, notProcessed.ToString());
                    if(notMatched > 0) 
                        Log(Level.Info, "{0} : {1} files not processed because they did not match includes/excludes.",
                            targetFolder.FullName, notMatched.ToString());
                }

                // Now we recursively call this method on all sub folders in this
                // folder unless recursive attribute is off.
                foreach (InterOpStarTeam.StFolder stFolder in starteamFolder.SubFolders) {
                    FileInfo targetSubfolder = new FileInfo(stFolder.Path);
                    delistLocalFile(localFiles, targetSubfolder);

                    if (this.recursive) {
                        visit(stFolder, targetSubfolder);
                    }
                }
                if (_addUncontrolled) {
                    addUncontrolledItems(localFiles, starteamFolder);
                }
            } catch (IOException ex) {
                throw new BuildException(ex.Message, Location, ex);
            }
        }

        /// <summary> Adds to the StarTeam repository everything on the local machine that is not currently in the repository.</summary>
        /// <param name="localFiles">Hasttable containing files missing in the repository for the current folder</param>
        /// <param name="folder">StarTeam folder to which these items are to be added.</param>
        private void  addUncontrolledItems(System.Collections.Hashtable localFiles, InterOpStarTeam.StFolder folder) {
            try {
                foreach(string fileName in localFiles.Keys) {
                    FileInfo file = new FileInfo(fileName);
                    add(folder, file);
                }
            }
                //TODO: Move security catch into add()
            catch (System.Security.SecurityException e) {
                Log(Level.Error, "Error adding file: {0}", e.Message);
            }
        }

        /// <summary> Adds the file or directpry to the repository.</summary>
        /// <param name="parentFolder">StarTeam folder underwhich items will be added.</param>
        /// <param name="file">the file or directory to add</param>
        /// <returns>true if the file was successfully added otherwise false.</returns>
        private bool add(InterOpStarTeam.StFolder parentFolder, FileInfo file) {
            // If the current file is a Directory, we need to process all of its children as well.
            if (Directory.Exists(file.FullName)) {
                if(!_createFolders) {
                    Log(Level.Info, "Could not add new folder as createfolders is disabled: {0}",
                        file.FullName);
                    return false;
                }

                Log(Level.Info, "Adding new folder to repository: {0}", file.FullName);
                InterOpStarTeam.StFolder newFolder = starteamFolderFactory.Create(parentFolder);
                newFolder.Name = file.Name;
                newFolder.update();

                // now visit this new folder to take care of adding any files or subfolders within it.
                if (this.recursive) {
                    visit(newFolder, file);
                }
            } else {
                Log(Level.Info, "Adding new file to repository: {0}", file.FullName);
                InterOpStarTeam.StFile newFile = starteamFileFactory.Create(parentFolder);
                newFile.Add(file.FullName, file.Name, null, _comment, starTeamLockTypeStatics.UNLOCKED, true, true);

                _updateLabel(newFile);
            }

            return true;
        }

        private void _updateLabel(InterOpStarTeam.StFile stFile) {
            //if user defined a label attach the item checked to that label
            if (_stLabel != null) {
                _stLabel.moveLabelToItem((InterOpStarTeam.StItem)stFile);
                _stLabel.update();
            }
        }
    }
}