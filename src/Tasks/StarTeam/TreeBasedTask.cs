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
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using InterOpStarTeam = StarTeam;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.StarTeam {
    /// <summary>
    /// Base for tree based star team tasks.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Abstracts tree-walking behavior common to many subtasks.
    /// </para>
    /// <para>
    /// This class provides tree iteration functionality. Derived classes will implement their specific task 
    /// functionally using the visitor pattern, specifically by implementing the method <see cref="visit"/>
    /// </para>
    /// <para>This class ported from the Ant task http://jakarta.apache.org/ant/manual/OptionalTasks/starteam.html </para>
    /// <para>You need to have the StarTeam SDK installed for StarTeam tasks to function correctly.</para>
    /// </remarks>
    public abstract class TreeBasedTask : StarTeamTask {
        protected string _rootStarTeamFolder = "/";
        protected string _rootLocalFolder = null;
        protected int _filesAffected = 0;
        protected string _label = null;
        protected bool _recursive = true;
        protected bool _forced = false;
        protected StringCollection _exclude = new StringCollection();
        protected StringCollection _include = new StringCollection();
        protected StringCollection _excludePatterns = new StringCollection();
        protected StringCollection _includePatterns = new StringCollection();

        /// <summary>
        /// Root StarTeam folder to begin operations on. Defaults to the root of the view.
        /// </summary>
        [TaskAttribute("rootstarteamfolder", Required=true)]
        public virtual string rootstarteamfolder {
            set { _rootStarTeamFolder = value;}
        }

        /// <summary>
        /// Root Local folder where files are to be checkout/in or manipulated. Defaults to the StarTeam default folder. 
        /// </summary>
        public virtual string RootLocalFolder {
            set { _rootLocalFolder = value; }
        }

        /// <summary>
        /// Accepts comma de-limited list of expressions to include in tree operations. 
        /// If nothing is set ALL filespecs are included.
        /// </summary>
        /// <example>
        /// <para>Match all C# files in the directory</para>
        /// <code>*.cs</code>
        /// </example>
        /// <remarks>
        /// Expressions should be just for filename matching. 
        /// Technically relative directory information is accepted but will never match.
        /// </remarks>
        [TaskAttribute("includes")]
        public virtual string Includes {
            set {
                _include.Clear();
                _include.AddRange(value.Split(','));
                populatePatterns(_include,out _includePatterns);
            }
        }

        /// <summary>
        /// Accepts comma de-limited list of expressions to exclude from tree operations. 
        /// If nothing is specified. NO filespecs are excluded.
        /// </summary>
        /// <example>
        /// <para>Match <b>No</b> C# files in the directory</para>
        /// <code>*.cs</code>
        /// </example>
        /// <remarks>
        /// <para>
        /// If a excludes pattern is set with no <see cref="Includes"/> patterns present includes defaults to "*"
        /// </para>
        /// Expressions should be just for filename matching. 
        /// Technically relative directory information is accepted but will never match.
        /// </remarks>
        [TaskAttribute("excludes")]
        public virtual string Excludes {
            set {
                _exclude.Clear();
                _exclude.AddRange(value.Split(','));
                populatePatterns(_include,out _includePatterns);
            }
        }

        /// <summary>
        ///Default : true - should tasks recurse through tree.
        /// </summary>
        [TaskAttribute("recursive")]
        [BooleanValidator]
        public virtual bool recursive {
            get { return _recursive;}
            set { _recursive = value; }
        }

        /// <summary>
        /// Default : false - force check in/out actions regardless of the status that StarTeam is maintaining for the file. 
        /// </summary>
        /// <remarks>
        /// <para>If <see cref="RootLocalFolder"/> is set then this property should be set <c>true</c>. 
        /// Otherwise the checkout will be based on how the repository compares to local target folder.
        /// </para>
        /// <para>Note that if forced is not on. Files with status Modified and Merge will not be processed.</para>
        /// </remarks>
        [TaskAttribute("forced")]
        [BooleanValidator]
        public virtual bool Forced {
            get { return _forced; }
            set { _forced = value;}
        }

        /// <summary> 
        /// Label used for checkout. If no label is set latest state of repository is checked out.
        /// </summary>
        /// <remarks>
        /// The label must exist in starteam or an exception will be thrown. 
        /// </remarks>
        [TaskAttribute("label", Required=false)]
        public virtual string Label {
            get { return _label; }
            set {
                //trim up label
                if (null != value) {
                    value = value.Trim();
                    if (value.Length > 0) {
                        _label = value;
                    }
                }
            }
        }

        /// <summary>
        /// Does the work of opening the supplied Starteam view and calling 
        /// the <see cref="visit"/> method setting the pattern in motion to perform the task.
        /// </summary>
        protected override void ExecuteTask() {
            _filesAffected = 0;
            testPreconditions();

            InterOpStarTeam.StView snapshot = openView();
            try {
                InterOpStarTeam.StStarTeamFinderStatics starTeamFinder = new InterOpStarTeam.StStarTeamFinderStatics();
                InterOpStarTeam.StFolder starTeamRootFolder = starTeamFinder.findFolder(snapshot.RootFolder, _rootStarTeamFolder);

                // set the local folder.
                FileInfo localrootfolder;

                if (null == starTeamRootFolder) {
                    throw new BuildException("Unable to find root folder in repository.",Location);
                }
                if (null == _rootLocalFolder) {
                    // use Star Team's default
                    try {
                        localrootfolder = new FileInfo(starTeamRootFolder.Path);
                    }
                    catch(Exception e) {
                        throw new BuildException(string.Format("Could not get handle to root folder ({0}) found.",starTeamRootFolder.Path),Location,e);
                    }
                } else {
                    // force StarTeam to use our folder
                    try {
                        Log(Level.Info, "Overriding local folder to '{0}'", _rootLocalFolder);
                        localrootfolder = new FileInfo(_rootLocalFolder);
                    } catch(Exception e) {
                        throw new BuildException(string.Format("Could not get handle to root folder '{0}'.",
                            starTeamRootFolder.Path), Location, e);
                    }
                }

                // Inspect everything in the root folder and then recursively
                visit(starTeamRootFolder, localrootfolder);
                Log(Level.Info, "{0} files affected", _filesAffected.ToString());
            } catch (System.Exception e) {
                throw new BuildException(e.Message, Location, e);
            }
        }

        /// <summary> 
        /// Helper method calls on the StarTeam API to retrieve an ID number for the specified view, corresponding to this.label.
        /// </summary>
        /// <returns>The Label identifier or <c>-1</c> if no label was provided.</returns>
        protected virtual InterOpStarTeam.IStLabel getLabelID(InterOpStarTeam.StView stView) {
            if (null != _label) {
                foreach(InterOpStarTeam.IStLabel stLabel in stView.Labels) {
                    System.Diagnostics.Trace.WriteLine(stLabel.Name);
                    if (stLabel.Name == _label) {
                        return stLabel;
                    }
                }
                throw new BuildException("Error: label " + _label + " does not exist in view",Location);
            }
            return null;
        }

        /// <summary> Derived classes must override this class to define actual processing to be performed on each folder in the tree defined for the task</summary>
        /// <param name="rootStarteamFolder">the StarTeam folderto be visited</param>
        /// <param name="rootLocalFolder">the local mapping of rootStarteamFolder</param>
        protected abstract void visit(InterOpStarTeam.StFolder rootStarteamFolder, FileInfo rootLocalFolder);

        /// <summary> 
        /// Derived classes must override this method to define tests for any preconditons required by the task.  
        /// This method is called at the beginning of the ExecuteTask method. 
        /// </summary>
        /// <seealso cref="ExecuteTask"/>
        protected abstract void testPreconditions();

        /// <summary> 
        /// Gets the collection of the local file names in the supplied directory.
        /// We need to check this collection against what we find in Starteam to
        /// understand what we need to do in order to synch with the repository.
        /// </summary>
        /// <remarks>
        /// The goal is to keep track of which local files are not controlled by StarTeam.
        /// </remarks>
        /// <param name="localFolder">Local folder to scan</param>
        /// <returns>hashtable whose keys represent a file or directory in localFolder.</returns>
        protected static System.Collections.Hashtable listLocalFiles(FileInfo localFolder) {
            System.Collections.Hashtable localFileList = new System.Collections.Hashtable();
            // we can't use java 2 collections so we will use an identity
            // Hashtable to  hold the file names.  We only care about the keys,
            // not the values (which will all be "").

            bool bExists;
            if (File.Exists(localFolder.FullName))
                bExists = true;
            else
                bExists = Directory.Exists(localFolder.FullName);
            if (bExists) {
                foreach(string fileName in Directory.GetFileSystemEntries(localFolder.FullName)) {
                    localFileList.Add(Path.Combine(localFolder.ToString(),fileName),"");
                    //SupportClass.PutElement(localFileList, localFolder.ToString() + Path.DirectorySeparatorChar + fileName, "");
                }
            }
            return localFileList;
        }

        /// <summary> 
        /// Removes file being worked with from the <see cref="listLocalFiles" /> generated hashtable.
        /// </summary>
        /// <remarks>
        /// The goal is to keep track of which local files are not controlled by StarTeam.
        /// </remarks>
        /// <param name="localFiles">Hashtable of the current directory's file|dire</param>
        /// <param name="thisfile">file to remove from list.</param>
        protected virtual void delistLocalFile(System.Collections.Hashtable localFiles, FileInfo thisfile) {
            string key = thisfile.ToString();
            localFiles.Remove(key);
        }

        /// <summary>
        /// Evaluates defined <see cref="Includes"/> and <see cref="Excludes"/> patterns against a filename. 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        protected virtual bool IsIncluded(string filePath) {   
            //check to see if any actual includes are set.
            if(_includePatterns.Count < 1 && _excludePatterns.Count < 1) return true;
            //make sure includes matches all if excludes is set.
            if(_excludePatterns.Count > 1 && _includePatterns.Count == 0) {
                _includePatterns.Add(ToRegexPattern("*"));
            }

            bool included = false;

            // check path against includes
            foreach (string pattern in _includePatterns) {
                Match m = Regex.Match(filePath, pattern);
                if (m.Success) {
                    included = true;
                    break;
                }
            }

            // check path against excludes
            if (included) {
                foreach (string pattern in _excludePatterns) {
                    Match m = Regex.Match(filePath, pattern);
                    if (m.Success) {
                        included = false;
                        break;
                    }
                }
            }

            return included;
        }

        /// <summary>
        ///     Lifted/Modified from <see cref="NAnt.Core.DirectoryScanner"/> to convert patterns to match filenames to regularexpressions.
        /// </summary>
        /// <param name="nantPattern">Search pattern - meant to be just a filename with no path info</param>
        /// <remarks>The directory seperation code in here most likely is overkill.</remarks>
        /// <returns>Regular expresssion for searching matching file names</returns>
        static string ToRegexPattern(string nantPattern) {                                                                          
            StringBuilder pattern = new StringBuilder(nantPattern);

            // NAnt patterns can use either / \ as a directory seperator.
            // We must replace both of these characters with Path.DirectorySeperatorChar
            pattern.Replace('/',  Path.DirectorySeparatorChar);
            pattern.Replace('\\', Path.DirectorySeparatorChar);

            // The '\' character is a special character in regular expressions
            // and must be escaped before doing anything else.
            pattern.Replace(@"\", @"\\");

            // Escape the rest of the regular expression special characters.
            // NOTE: Characters other than . $ ^ { [ ( | ) * + ? \ match themselves.
            // TODO: Decide if ] and } are missing from this list, the above
            // list of characters was taking from the .NET SDK docs.
            pattern.Replace(".", @"\.");
            pattern.Replace("$", @"\$");
            pattern.Replace("^", @"\^");
            pattern.Replace("{", @"\{");
            pattern.Replace("[", @"\[");
            pattern.Replace("(", @"\(");
            pattern.Replace(")", @"\)");
            pattern.Replace("+", @"\+");

            // Special case directory seperator string under Windows.
            string seperator = Path.DirectorySeparatorChar.ToString();
            if (seperator == @"\") {
                seperator = @"\\";
            }

            // Convert NAnt pattern characters to regular expression patterns.

            // SPECIAL CASE: to match subdirectory OR current directory.  If
            // we don't do this then we can write something like 'src/**/*.cs'
            // to match all the files ending in .cs in the src directory OR
            // subdirectories of src.
            pattern.Replace(seperator + "**", "(" + seperator + ".|)|");

            // | is a place holder for * to prevent it from being replaced in next line
            pattern.Replace("**", ".|");
            pattern.Replace("*", "[^" + seperator + "]*");
            pattern.Replace("?", "[^" + seperator + "]?");
            pattern.Replace('|', '*'); // replace place holder string

            // Help speed up the search
            //pattern.Insert(0, '^'); // start of line
            //pattern.Append('$'); // end of line

            return pattern.ToString();
        }
 
        /// <summary>
        /// Convert path patterns to regularexpression patterns. Stored in the given string collection.
        /// </summary>
        /// <param name="paths">collection of paths to expand into regular expressions</param>
        /// <param name="patterns">collection to store the given regularexpression patterns</param>
        protected void populatePatterns(StringCollection paths, out StringCollection patterns) {
            patterns = new StringCollection();
            foreach(string path in paths) {
                patterns.Add(ToRegexPattern(path));
            }
        }
    }
}