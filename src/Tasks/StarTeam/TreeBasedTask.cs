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
using SourceForge.NAnt;
using SourceForge.NAnt.Attributes;
using InterOpStarTeam = StarTeam;

namespace NAnt.Contrib.Tasks.StarTeam 
{

	/// <summary>
	/// Base for tree based star team tasks.
	/// </summary>
	/// <remarks>
	/// <para>Abstracts tree-walking behavior common to many subtasks.</para>
	/// <para>
	/// This class provides tree iteration functionality. Derived classes will implement their specific task 
	/// functionally using the visitor pattern, specifically by implementing the method <see cref="visit"/>
	/// </para>
	/// <para>This class ported from the Ant task http://jakarta.apache.org/ant/manual/OptionalTasks/starteam.html </para>
	/// <para>You need to have the StarTeam SDK installed for StarTeam tasks to function correctly.</para>
	/// </remarks>
	public abstract class TreeBasedTask : StarTeamTask
	{
		/// <summary>
		/// Root StarTeam folder to begin operations on. Defaults to the root of the view.
		/// </summary>
		[TaskAttribute("rootstarteamfolder",Required=true)]
		public virtual string rootstarteamfolder
		{
			set { _rootStarTeamFolder = value;}
		}

		/// <summary>
		/// Root Local folder where files are to be checkout/in or manipulated. Defaults to the StarTeam default folder. 
		/// </summary>
		public virtual string RootLocalFolder
		{
			set { _rootLocalFolder = value; }
		}
	
		//TODO: add Include / Exclude capability utilizing filesets

		//	public virtual string Includes
		//	{
		//		/// <summary> Gets the patterns from the include filter. Rather that duplicate the details of AntStarTeamCheckOut's filtering here, refer to these links: *</summary>
		//		/// <returns>A string of filter patterns separated by spaces.</returns>
		//		/// <seealso cref=" #setIncludes(String includes)"/>
		//		/// <seealso cref=" #setExcludes(String excludes)"/>
		//		/// <seealso cref=" #getExcludes()"/>
		//		get
		//		{
		//			return includes;
		//		}
		//		/// <summary> Declare files to include using standard <tt>includes</tt> patterns; optional. </summary>
		//		/// <param name="includes">A string of filter patterns to include. Separate the patterns by spaces.</param>
		//		/// <seealso cref=" #getIncludes()"/>
		//		/// <seealso cref=" #setExcludes(String excludes)"/>
		//		/// <seealso cref=" #getExcludes()"/>
		//		set
		//		{
		//			this.includes = value;
		//		}
		//	}
		//
		//	public virtual string Excludes
		//	{
		//		/// <summary> Gets the patterns from the exclude filter. Rather that duplicate the details of AntStarTeanCheckOut's filtering here, refer to these
		//		/// links:
		//		/// *
		//		/// </summary>
		//		/// <returns>A string of filter patterns separated by spaces.</returns>
		//		/// <seealso cref=" #setExcludes(String excludes)"/>
		//		/// <seealso cref=" #setIncludes(String includes)"/>
		//		/// <seealso cref=" #getIncludes()"/>
		//		get
		//		{
		//			return excludes;
		//		}
		//		/// <summary> Declare files to exclude using standard <tt>excludes</tt> patterns; optional. 
		//		/// When filtering files, AntStarTeamCheckOut
		//		/// uses an unmodified version of <CODE>DirectoryScanner</CODE>'s
		//		/// <CODE>match</CODE> method, so here are the patterns straight from the
		//		/// Ant source code:
		//		/// <BR><BR>
		//		/// Matches a string against a pattern. The pattern contains two special
		//		/// characters:
		//		/// <BR>'*' which means zero or more characters,
		//		/// <BR>'?' which means one and only one character.
		//		/// <BR><BR>
		//		/// For example, if you want to check out all files except .XML and
		//		/// .HTML files, you would put the following line in your program:
		//		/// <CODE>setExcludes("*.XML,*.HTML");</CODE>
		//		/// Finally, note that filters have no effect on the <B>directories</B>
		//		/// that are scanned; you could not skip over all files in directories
		//		/// whose names begin with "project," for instance.
		//		/// <BR><BR>
		//		/// Treatment of overlapping inlcudes and excludes: To give a simplistic
		//		/// example suppose that you set your include filter to "*.htm *.html"
		//		/// and your exclude filter to "index.*". What happens to index.html?
		//		/// AntStarTeamCheckOut will not check out index.html, as it matches an
		//		/// exclude filter ("index.*"), even though it matches the include
		//		/// filter, as well.
		//		/// <BR><BR>
		//		/// Please also read the following sections before using filters:
		//		/// *
		//		/// </summary>
		//		/// <param name="excludes">A string of filter patterns to exclude. Separate the patterns by spaces.</param>
		//		/// <seealso cref=" #setIncludes(String includes)"/>
		//		/// <seealso cref=" #getIncludes()"/>
		//		/// <seealso cref=" #getExcludes()"/>
		//		set
		//		{
		//			this.excludes = value;
		//		}
		//	}

		/// <summary>
		///Default : true - should tasks recurse through tree.
		/// </summary>
		[TaskAttribute("recursive")]
		[BooleanValidator]         
		public virtual bool recursive
		{
			get { return _recursive;}
			set { _recursive = value; }		
		}	

		/// <summary>
		/// Default : false - force check in/out actions regardless of the status that StarTeam is maintaining for the file. 
		/// </summary>
		/// <remarks>
		/// If <see cref="RootLocalFolder"/> is set then this property should be set <c>true</c>. 
		/// Otherwise the checkout will be based on statuses which do not relate to the target folder.  
		/// </remarks>
		[TaskAttribute("forced")]
		[BooleanValidator]
		public virtual bool forced
		{
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
		public virtual string label
		{
			get { return _label; }			
			set 
			{	//trim up label
				if (null != value)
				{
					value = value.Trim();
					if (value.Length > 0)
					{
						_label = value;
					}
				}
			}			
		}

		///////////////////////////////////////////////////////////////
		// default values for attributes.
		///////////////////////////////////////////////////////////////
		// <summary> This constant sets the filter to include all files. This default has the same result as <CODE>setIncludes("*")</CODE>.</summary>
		//public const string DEFAULT_INCLUDESETTING = "*";
	
		// <summary> This disables the exclude filter by default. In other words, no files are excluded. This setting is equivalent to
		// <CODE>setExcludes(null)</CODE>.
		// </summary>
		//public const string DEFAULT_EXCLUDESETTING = null;
	
		//ATTRIBUTES settable from ant.
	
		// <value> The root folder of the operation in InterOpStarTeam.</value>
		protected string _rootStarTeamFolder = "/";
	
		// <value> The local folder corresponding to starteamFolder.  If not specified the Star Team defalt folder will be used.</value>
		protected string _rootLocalFolder = null;

		// <value>Keeps a tally of the number of files affected by the operation</value>
		protected int _filesAffected = 0;
	
		// <value> All files that fit this pattern are checked out.</value>
		//private string includes;
	
		// <value> All files fitting this pattern are ignored.</value>
		//private string excludes;
	
		/// <value> StarTeam label on which to perform task.</value>
		protected string _label = null;
	
		/// <value> Set recursion to false to check out files in only the given folder and not in its subfolders.</value>
		private bool _recursive = true;
	
		/// <value> If forced set to true, files in the target directory will be processed regardless of status in the repository.
		/// Usually this should be  true if rootlocalfolder is set because status will be relative to the default folder, not
		/// to the one being processed. </value>
		private bool _forced = false;
	                    	

	
		///////////////////////////////////////////////////////////////
		// INCLUDE-EXCLUDE processing
		///////////////////////////////////////////////////////////////
	
		// <summary> Look if the file should be processed by the task. Don't process it if it fits no include filters or if it fits an exclude filter.</summary>
		// <param name="pName">the item name to look for being included.</param>
		// <returns>whether the file should be checked out or not.</returns>
		//	protected internal virtual bool shouldProcess(string pName)
		//	{
		//		bool includeIt = matchPatterns(Includes, pName);
		//		bool excludeIt = matchPatterns(Excludes, pName);
		//		return (includeIt && !excludeIt);
		//	}

		// <summary> Convenience method to see if a string match a one pattern in given set of space-separated patterns.</summary>
		// <param name="patterns">the space-separated list of patterns.</param>
		// <param name="pName">the name to look for matching.</param>
		// <returns>whether the name match at least one pattern.</returns>
		//	protected internal virtual bool matchPatterns(string patterns, string pName)
		//	{
		//		if (patterns == null) 
		//		{
		//			return false;
		//		}
		//		SupportClass.Tokenizer exStr = new SupportClass.Tokenizer(patterns, ",");
		//		while (exStr.HasMoreTokens())
		//		{
		//				if (DirectoryScanner.match(exStr.NextToken(), pName))
		//				{
		//					return true;
		//				}
		//		}
		//		return false;
		//	}
	
		/// <summary>
		/// Does the work of opening the supplied Starteam view and calling 
		/// the <see cref="visit"/> method setting the pattern in motion to perform the task.
		/// </summary>
		protected override void ExecuteTask() 
		{
			try
			{
				_filesAffected = 0;
				testPreconditions();
			
				InterOpStarTeam.StView snapshot = openView();
				InterOpStarTeam.StStarTeamFinderStatics starTeamFinder = new InterOpStarTeam.StStarTeamFinderStatics();
				InterOpStarTeam.StFolder starTeamRootFolder = starTeamFinder.findFolder(snapshot.RootFolder, _rootStarTeamFolder);
			    
				// set the local folder.
				FileInfo localrootfolder;

				if (null == starTeamRootFolder)
				{
					throw new BuildException("Unable to find root folder in repository.");
				}
				if (null == _rootLocalFolder)
				{
					// use Star Team's default
					try 
					{
						localrootfolder = new FileInfo(starTeamRootFolder.Path);
					}
					catch(Exception e) 
					{
						throw new BuildException(string.Format("Could not get handle to root folder ({0}) found.",starTeamRootFolder.Path),e);
					}
				}
				else
				{
					// force StarTeam to use our folder
					try 
					{
						Log.WriteLine("Overriding local folder to {0}",_rootLocalFolder);
						localrootfolder = new FileInfo(_rootLocalFolder);
					}
					catch(Exception e) 
					{
						throw new BuildException(string.Format("Could not get handle to root folder ({0}) found.",starTeamRootFolder.Path),e);
					}
				}
				
				// Inspect everything in the root folder and then recursively
				visit(starTeamRootFolder, localrootfolder);
				Log.WriteLine("{0} Files Affected",_filesAffected.ToString());
			}
			catch (System.Exception e)
			{
				throw new BuildException(e.Message);
			}
		}
	
		/// <summary> 
		/// Helper method calls on the StarTeam API to retrieve an ID number for the specified view, corresponding to this.label.
		/// </summary>
		/// <returns>The Label identifier or <c>-1</c> if no label was provided.</returns>
		protected internal virtual InterOpStarTeam.IStLabel getLabelID(InterOpStarTeam.StView stView)
		{
			if (null != _label)
			{
				foreach(InterOpStarTeam.IStLabel stLabel in stView.Labels) 
				{
					System.Diagnostics.Trace.WriteLine(stLabel.Name);
					if (stLabel.Name == _label)
					{
						return stLabel;
					}
				}
				throw new BuildException("Error: label " + _label + " does not exist in view");
			}
			return null;
		}
	
		/// <summary> Derived classes must override this class to define actual processing to be performed on each folder in the tree defined for the task</summary>
		/// <param name="rootStarteamFolder">the StarTeam folderto be visited</param>
		/// <param name="rootLocalFolder">the local mapping of rootStarteamFolder</param>
		protected internal abstract void  visit(InterOpStarTeam.StFolder rootStarteamFolder, FileInfo rootLocalFolder);
	
		/// <summary> 
		/// Derived classes must override this method to define tests for any preconditons required by the task.  
		/// This method is called at the beginning of the ExecuteTask method. 
		/// </summary>
		/// <seealso cref="ExecuteTask"/>
		protected internal abstract void  testPreconditions();
	
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
		protected internal static System.Collections.Hashtable listLocalFiles(FileInfo localFolder)
		{   		
			System.Collections.Hashtable localFileList = new System.Collections.Hashtable();
			// we can't use java 2 collections so we will use an identity
			// Hashtable to  hold the file names.  We only care about the keys,
			// not the values (which will all be "").
		
			bool bExists;
			if (File.Exists(localFolder.FullName))
				bExists = true;
			else
				bExists = Directory.Exists(localFolder.FullName);
			if (bExists)
			{
				foreach(string fileName in Directory.GetFileSystemEntries(localFolder.FullName))
				{
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
		protected internal virtual void delistLocalFile(System.Collections.Hashtable localFiles, FileInfo thisfile)
		{
			string key = thisfile.ToString();
			localFiles.Remove(key);
		}

	}
}