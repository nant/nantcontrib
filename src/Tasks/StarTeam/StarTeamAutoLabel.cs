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
using System.Xml;
using SourceForge.NAnt;
using SourceForge.NAnt.Attributes;
using InterOpStarTeam = StarTeam;

namespace NAnt.Contrib.Tasks.StarTeam 
{
	/// <summary>
	/// Task for supporting labeling of repositories with incremented version numbers. 
	/// The version number calculated will be concatenated to the <see cref="LabelTask.Label"/>
	/// </summary>
	/// <remarks>
	/// <para>Instruments root of repository with versionnumber.xml file.</para>
	/// <para>If this file is not present. It is created and checked into StarTeam. The Default Version number is 1.0.0. 
	/// By default the build number is incremented. Properties are present to allow setting and incrementing of Major, Minor, and Build versions.
	/// </para>
	/// <para>When label is created properties are set to expose version information and the new label.
	/// <list type="Bullet"><listheader>Properties Set</listheader><item>label</item><item>Version.text</item><item>Version.major</item><item>Version.minor</item><item>Version.build</item></list>
	/// </para>
	/// <para><i>Note:</i> Incrementing or setting Major or Minor versions does NOT reset the build version.</para>
	/// </remarks>
	/// <example>
	/// <para>Default: increment the build version</para>
	/// <code><stautolabel url="${ST.url}"/></code>
	/// <para>set the major version</para>
	/// <code><stautolabel majorversion="2" url="${ST.url}"/></code>
	/// <para>increment minor version</para>
	/// <code><stautolabel incrementminor="true" url="${ST.url}"/></code>
	/// <para>Example versionnumber.xml file</para>
	/// <code><![CDATA[
	/// <?xml version="1.0"?>
	/// <stautolabel>
	///		<version major="1" minor="0" build="0"></version>
	/// </stautolabel>
	/// ]]></code>
	/// </example>
	[TaskName("stautolabel")]
	public class StarTeamAutoLabel : LabelTask
	{
		private bool _doIncrement = true;
		private bool _incrementMajor= false,_incrementMinor = false;
		private bool _incrementBuild = true;
		private int	_versionMajor = -1, _versionMinor = -1, _versionBuild = -1;			
		private string _versionFile = "versionnumber.xml";

		private InterOpStarTeam.StItem_LockTypeStaticsClass starTeamLockTypeStatics = new InterOpStarTeam.StItem_LockTypeStaticsClass(); 

		/// <summary> 
		/// Allows user to specify the filename where the version xml is stored. Default is <b>versionnumber.xml</b>
		/// </summary>
		[TaskAttribute("versionfile", Required=false)]
		public virtual string VersionFile
		{
			set { _versionFile = value; }			
		}
		
		/// <summary> 
		/// Increment Major version number : Default is <c>false</c> 
		/// If <see cref="MajorVersion"/> is set this property is ignored.
		/// </summary>
		[TaskAttribute("incrementmajor", Required=false)]
		[BooleanValidator]         
		public virtual bool IncrementMajor
		{
			set { _incrementMajor = value; }			
		}
		
		/// <summary> 
		/// Increment Minor version number : Default is <c>false</c> 
		/// If <see cref="MinorVersion"/> is set this property is ignored.
		/// </summary>
		[TaskAttribute("incrementminor", Required=false)]
		[BooleanValidator]         
		public virtual bool IncrementMinor
		{
			set { _incrementMinor = value; }			
		}
		
		/// <summary> 
		/// Increment Build version number : Default is <c>true</c> 
		/// If <see cref="BuildVersion"/> is set this property is ignored.
		/// </summary>
		[TaskAttribute("incrementbuild", Required=false)]
		[BooleanValidator]         
		public virtual bool IncrementBuild
		{
			set { _incrementBuild = value; }			
		}

		/// <summary> 
		/// Major version number used for label. 
		/// If this value is set. <see cref="IncrementMajor"/> is ignored.
		/// </summary>
		[TaskAttribute("majorversion", Required=false)]
		[Int32Validator(0,9999999)]
		public virtual int MajorVersion
		{
			set 
			{	
				_doIncrement = false;
				_versionMajor = value; 
			}			
		}

		/// <summary> 
		/// Minor version number used for label. 
		/// If this value is set. <see cref="IncrementMinor"/> is ignored.
		/// </summary>
		[TaskAttribute("minorversion", Required=false)]
		[Int32Validator(0,9999999)]
		public virtual int MinorVersion
		{
			set 
			{
				_doIncrement = false;
				_versionMinor = value; 
			}			
		}

		/// <summary> 
		/// Build version number used for label. 
		/// If this value is set. <see cref="IncrementBuild"/> is ignored.
		/// </summary>
		[TaskAttribute("buildversion", Required=false)]
		[Int32Validator(0,9999999)]
		public virtual int BuildVersion
		{
			set 
			{
				_doIncrement = false;
				_versionBuild = value; 
			}			
		}

		/// <summary>
		/// Looks for versionnumber.xml at root of repository. 
		/// Updates the xml in this file to correspond with properties set by user and checks in changes. 
		/// A label is then created based on properties set. 
		/// </summary>
		/// <remarks>
		/// Default behavior is to <see cref="IncrementBuild"/> number. 
		/// If user sets <see cref="MajorVersion"/>, <see cref="MinorVersion"/>, or <see cref="BuildVersion"/> no incrementing is done 
		/// and the exact version set and/or read from versionnumber.xml is used.
		/// <para>The title of the Label is the <see cref="LabelTask.Label"/> property concatenated with the version number Major.Minor.Build</para>
		/// </remarks>
		protected override void  ExecuteTask()
		{                         			
			InterOpStarTeam.StView snapshot = openView();
			InterOpStarTeam.StFile stFile = getVersionStFile(snapshot);
			
			try 
			{      
				//load xml document find versions and save incremented version 
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load(stFile.FullName);
				XmlNode nodeVersion = xmlDoc.DocumentElement.SelectSingleNode("version");
				if(_versionMajor < 0) _versionMajor = Convert.ToInt32(nodeVersion.Attributes.GetNamedItem("major").InnerText);
				if(_versionMinor < 0) _versionMinor = Convert.ToInt32(nodeVersion.Attributes.GetNamedItem("minor").InnerText);
				if( _versionBuild < 0) _versionBuild = Convert.ToInt32(nodeVersion.Attributes.GetNamedItem("build").InnerText);
				
				if(_doIncrement == true) 
				{
					if(_incrementMajor == true) _versionMajor++;
					if(_incrementMinor == true) _versionMinor++;
					if(_incrementBuild == true) _versionBuild++;
				}
				nodeVersion.Attributes.GetNamedItem("major").InnerText = _versionMajor.ToString();
				nodeVersion.Attributes.GetNamedItem("minor").InnerText = _versionMinor.ToString();
				nodeVersion.Attributes.GetNamedItem("build").InnerText = _versionBuild.ToString();
				xmlDoc.Save(stFile.FullName);
			}
			catch(XmlException e)
			{
				throw new BuildException("Error parsing / updating version xml",Location,e);
			}


			stFile.checkin("version updated via stautolabel", starTeamLockTypeStatics.UNLOCKED,true, true, true);
			this.Label = string.Format("{0}{1}.{2}.{3}",this.Label,_versionMajor,_versionMinor,_versionBuild);
			this.Properties["label"] = this.Label;
			this.Properties["Version.text"] = string.Format("{1}.{2}.{3}",_versionMajor,_versionMinor,_versionBuild);
			this.Properties["Version.major"] = _versionMajor.ToString();
			this.Properties["Version.minor"] = _versionMinor.ToString();
			this.Properties["Version.build"] = _versionBuild.ToString();
	
			createLabel(snapshot);

		}

		/// <summary>
		/// Locate the versionnumber.xml file in the repository. If it is not present it creates the file. 
		/// The file is checked out exclusively for editting.
		/// </summary>
		/// <param name="snapshot">StarTeam view we are working with.</param>
		/// <returns>StarTeam file handle containing version xml.</returns>
		private InterOpStarTeam.StFile getVersionStFile(InterOpStarTeam.StView snapshot) 
		{
			InterOpStarTeam.StFile stVersionFile = null; 
			//connect to starteam and get root folder 
			InterOpStarTeam.StFolder starTeamRootFolder = snapshot.RootFolder;
			
			//get contents of root folder and look for version file
			//this is weird as I cannot see how to ask StarTeam for an individual file
			foreach(InterOpStarTeam.StFile stFile in starTeamRootFolder.getItems("File"))
			{
				if(stFile.Name == _versionFile)
				{
					stVersionFile = stFile;
					break;
				}
			}

			if(stVersionFile == null) 
			{
				stVersionFile = createVersionStFile(starTeamRootFolder);
			}				
			else 
			{
				stVersionFile.checkout(starTeamLockTypeStatics.EXCLUSIVE,true,true,true);
			}
			return stVersionFile;
		}

		/// <summary>
		/// Creates the versionumber.xml file in the repository.
		/// </summary>
		/// <param name="stFolder">StarTeam folder desired to put the versionnumber.xml files into</param>
		/// <returns>StarTeam File handle to the created file.</returns>
		private InterOpStarTeam.StFile createVersionStFile(InterOpStarTeam.StFolder stFolder)
		{
			//Instantiated here as they are only necessary when adding 
			InterOpStarTeam.StFileFactoryClass starteamFileFactory = new InterOpStarTeam.StFileFactoryClass();
			string versionFilePath = stFolder.getFilePath(_versionFile);

			//create xml and save to local file
			try 
			{
				StreamWriter s = new StreamWriter(versionFilePath,false,System.Text.ASCIIEncoding.ASCII);
				XmlTextWriter xmlWriter = new XmlTextWriter(s);
				xmlWriter.WriteStartDocument(false);
				xmlWriter.WriteStartElement("stautolabel");
				xmlWriter.WriteStartElement("version");
				xmlWriter.WriteAttributeString("major","1");
				xmlWriter.WriteAttributeString("minor","0");
				xmlWriter.WriteAttributeString("build","0");
				xmlWriter.WriteEndElement();
				xmlWriter.WriteEndElement();
				xmlWriter.Close();
			}
			catch(System.Security.SecurityException e)
			{
				throw new BuildException(string.Format("You do not have access to {0}",versionFilePath),Location,e);
			}
			catch(IOException e)
			{
				throw new BuildException(string.Format("Version filepath {0} invalid.",versionFilePath),Location,e);
			}
			
			//add local file to starteam 
			InterOpStarTeam.StFile newFile = starteamFileFactory.Create(stFolder);
			string comment = "version number xml created by stautonumber NAnt task";
			newFile.Add(versionFilePath, _versionFile, comment, comment, starTeamLockTypeStatics.UNLOCKED, true, true);

			return newFile;
		}
	

	}	//class

}
