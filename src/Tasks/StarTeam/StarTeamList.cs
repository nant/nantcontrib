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
    /// List items in StarTeam repositories.
    /// </summary>
    /// <remarks>
    /// <para>This task was ported from the Ant task http://jakarta.apache.org/ant/manual/OptionalTasks/starteam.html#stlist </para>
    /// <para>You need to have the StarTeam SDK installed for this task to function correctly.</para>
    /// </remarks>
    /// <example>
    ///   <para>Lists all files in a StarTeam repository.</para>
    ///   <code>
    ///     <![CDATA[
    /// <!-- 
    ///   constructs a 'url' containing connection information to pass to the task 
    ///   alternatively you can set each attribute manually 
    /// -->
    /// <property name="ST.url" value="user:pass@serverhost:49201/projectname/viewname" />
    /// <stlist rootstarteamfolder="/" recursive="true" url="${ST.url}" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("stlist")]
    public class StarTeamList : TreeBasedTask {
        /// <summary>
        /// Override of base-class abstract function creates an appropriately configured view for checkoutlists. 
        /// The current view or a view of the label specified <see cref="TreeBasedTask.Label" />.
        /// </summary>
        /// <param name="raw">the unconfigured <c>View</c></param>
        /// <returns>the snapshot <c>View</c> appropriately configured.</returns>
        protected override internal InterOpStarTeam.StView createSnapshotView(InterOpStarTeam.StView raw) {
            InterOpStarTeam.StViewConfigurationStaticsClass starTeamViewConfiguration = new InterOpStarTeam.StViewConfigurationStaticsClass();
            InterOpStarTeam.StViewFactory starTeamViewFactory = new InterOpStarTeam.StViewFactory();
            InterOpStarTeam.IStLabel stLabel = getLabelID(raw);

            // if a label has been supplied, use it to configure the view
            // otherwise use current view
            if (stLabel != null) {
                return starTeamViewFactory.Create(raw, starTeamViewConfiguration.createFromLabel(stLabel.ID));
            }
            else {
                return starTeamViewFactory.Create(raw, starTeamViewConfiguration.createTip());
            }
        }

        /// <summary>Required base-class abstract function implementation is a no-op here.</summary>
        protected override void testPreconditions() {
            //intentionally do nothing.
        }

        /// <summary> Implements base-class abstract function to perform the checkout
        /// operation on the files in each folder of the tree.</summary>
        /// <param name="starteamFolder">the StarTeam folder from which files to be checked out</param>
        /// <param name="targetFolder">the local mapping of rootStarteamFolder</param>
        protected override void visit(InterOpStarTeam.StFolder starteamFolder, FileInfo targetFolder) {
            try {
                if (null == _rootLocalFolder) {
                    Log(Level.Info, "Folder: {0} (Default folder: {1})", 
                        starteamFolder.Name, targetFolder);
                } else {
                    Log(Level.Info, "Folder: {0} (Local folder: {1})", 
                        starteamFolder.Name, targetFolder);
                }
                System.Collections.Hashtable localFiles = listLocalFiles(targetFolder);

                // For all Files in this folder, we need to check
                // if there have been modifications.
                foreach(InterOpStarTeam.StFile stFile in starteamFolder.getItems("File")) {
                    string filename = stFile.Name;
                    FileInfo localFile = new FileInfo(Path.Combine(targetFolder.FullName,filename));

                    delistLocalFile(localFiles, localFile);

                    // If the file doesn't pass the include/exclude tests, skip it.
                    if (!IsIncluded(filename)) {
                        continue;
                    }

                    list(stFile, localFile);
                }

                // Now we recursively call this method on all sub folders in this
                // folder unless recursive attribute is off.
                foreach(InterOpStarTeam.StFolder stFolder in starteamFolder.SubFolders) {
                    FileInfo targetSubfolder = new FileInfo(stFolder.Path);
                    delistLocalFile(localFiles, targetSubfolder);
                    if (this.recursive) {
                        visit(stFolder, targetSubfolder);
                    }
                }
            } catch (IOException ex) {
                throw new BuildException(ex.Message, Location, ex);
            }
        }

        protected internal virtual void list(InterOpStarTeam.StFile reposFile, FileInfo localFile) {
            System.Text.StringBuilder b = new System.Text.StringBuilder();
            if (null == _rootLocalFolder) {
                InterOpStarTeam.StStatusStaticsClass starTeamStatus = new InterOpStarTeam.StStatusStaticsClass();
                // status is irrelevant to us if we have specified a
                // root local folder.
                b.Append(pad(starTeamStatus.Name(reposFile.Status), 12) + " ");
            }
            b.Append( pad(getUserName(reposFile.Locker), 20) + " " + reposFile.ModifiedTime.ToShortDateString() + rpad(reposFile.LocalSize.ToString(), 9) + " " + reposFile.Name);
            Log(Level.Info, b.ToString());
        }

        private const string blankstr = "                              ";

        protected internal static string pad(string s, int padlen) {
            return (s + blankstr).Substring(0, (padlen) - (0));
        }

        protected internal static string rpad(string s, int padlen) {
            s = blankstr + s;
            return s.Substring(s.Length - padlen);
        }
    }
}