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
using System.Globalization;
using System.IO;
using System.Xml;

using InterOpStarTeam = StarTeam;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.StarTeam {
    /// <summary>
    /// Task for supporting labeling of repositories with incremented version 
    /// numbers. The version number calculated will be concatenated to the 
    /// <see cref="LabelTask.Label" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Instruments root of repository with <c>versionnumber.xml</c> file.
    /// </para>
    /// <para>
    /// If this file is not present, it is created and checked into StarTeam. 
    /// The default version number is 1.0.0. By default the build number is 
    /// incremented. Properties are present to allow setting and incrementing 
    /// of major, minor, and build versions.
    /// </para>
    /// <para>
    /// When label is created, properties are set to expose version information 
    /// and the new label :
    /// </para>
    /// <list type="bullet">
    ///     <item>
    ///         <description>label</description>
    ///     </item>
    ///     <item>
    ///         <description>Version.text</description>
    ///     </item>
    ///     <item>
    ///         <description>Version.major</description>
    ///     </item>
    ///     <item>
    ///         <description>Version.minor</description>
    ///     </item>
    ///     <item>
    ///         <description>Version.build</description>
    ///     </item>
    /// </list>
    /// <note>
    /// Incrementing or setting major or minor versions does NOT reset the 
    /// build version.
    /// </note>
    /// </remarks>
    /// <example>
    ///   <para>Increment the build version.</para>
    ///   <code>
    ///     <![CDATA[
    /// <stautolabel url="${ST.url}" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>Set the major version.</para>
    ///   <code>
    ///     <![CDATA[
    /// <stautolabel majorversion="2" url="${ST.url}" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>Increment the minor version.</para>
    ///   <code>
    ///     <![CDATA[
    /// <stautolabel incrementminor="true" url="${ST.url}" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>Example <c>versionnumber.xml</c> file.</para>
    ///   <code>
    ///     <![CDATA[
    /// <?xml version="1.0"?>
    /// <stautolabel>
    ///     <version major="1" minor="0" build="0" />
    /// </stautolabel>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("stautolabel")]
    public class StarTeamAutoLabel : LabelTask {
        #region Private Instance Fields

        private bool _doIncrement = true;
        private bool _incrementMajor= false;
        private bool _incrementMinor = false;
        private bool _incrementBuild = true;
        private int _versionMajor = -1;
        private int _versionMinor = -1;
        private int _versionBuild = -1;
        private string _versionFile = "versionnumber.xml";
        private InterOpStarTeam.StItem_LockTypeStaticsClass starTeamLockTypeStatics = new InterOpStarTeam.StItem_LockTypeStaticsClass(); 

        #endregion Private Instance Fields

        /// <summary> 
        /// Allows user to specify the filename where the version xml is stored. 
        /// The default is <c>versionnumber.xml</c>.
        /// </summary>
        [TaskAttribute("versionfile", Required=false)]
        public virtual string VersionFile {
            get { return _versionFile; }
            set { _versionFile = value; }
        }

        /// <summary> 
        /// Increment major version number. The default is <see langword="false" />.
        /// If <see cref="MajorVersion"/> is set, this property is ignored.
        /// </summary>
        [TaskAttribute("incrementmajor", Required=false)]
        [BooleanValidator]
        public virtual bool IncrementMajor {
            get { return _incrementMajor; }
            set { _incrementMajor = value; }
        }

        /// <summary> 
        /// Increment minor version number. The default is <see langword="false" />. 
        /// If <see cref="MinorVersion"/> is set, this property is ignored.
        /// </summary>
        [TaskAttribute("incrementminor", Required=false)]
        [BooleanValidator]
        public virtual bool IncrementMinor {
            get { return _incrementMinor; }
            set { _incrementMinor = value; }
        }

        /// <summary> 
        /// Increment build version number. The default is <see langword="true" />.
        /// If <see cref="BuildVersion"/> is set, this property is ignored.
        /// </summary>
        [TaskAttribute("incrementbuild", Required=false)]
        [BooleanValidator]
        public virtual bool IncrementBuild {
            get { return _incrementBuild; }
            set { _incrementBuild = value; }
        }

        /// <summary> 
        /// Major version number used for label.  If this value is set, 
        /// <see cref="IncrementMajor"/> is ignored.
        /// </summary>
        [TaskAttribute("majorversion", Required=false)]
        [Int32Validator(0,9999999)]
        public virtual int MajorVersion {
            get { return _versionMajor; }
            set {
                _doIncrement = false;
                _versionMajor = value;
            }
        }

        /// <summary> 
        /// Minor version number used for label. If this value is set, 
        /// <see cref="IncrementMinor"/> is ignored.
        /// </summary>
        [TaskAttribute("minorversion", Required=false)]
        [Int32Validator(0,9999999)]
        public virtual int MinorVersion {
            get { return _versionMinor; }
            set {
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
        public virtual int BuildVersion {
            get { return _versionBuild; }
            set {
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
        protected override void  ExecuteTask() {
            InterOpStarTeam.StView snapshot = openView();
            InterOpStarTeam.StFile stFile = getVersionStFile(snapshot);

            try {
                //load xml document find versions and save incremented version 
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(stFile.FullName);
                XmlNode nodeVersion = xmlDoc.DocumentElement.SelectSingleNode("version");
                if (_versionMajor < 0) {
                    _versionMajor = Convert.ToInt32(nodeVersion.Attributes.GetNamedItem("major").InnerText);
                }
                if (_versionMinor < 0) {
                    _versionMinor = Convert.ToInt32(nodeVersion.Attributes.GetNamedItem("minor").InnerText);
                }
                if ( _versionBuild < 0) {
                    _versionBuild = Convert.ToInt32(nodeVersion.Attributes.GetNamedItem("build").InnerText);
                }

                if (_doIncrement == true) {
                    if (_incrementMajor == true) {
                        _versionMajor++;
                    }
                    if (_incrementMinor == true) {
                        _versionMinor++;
                    }
                    if (_incrementBuild == true) {
                        _versionBuild++;
                    }
                }
                nodeVersion.Attributes.GetNamedItem("major").InnerText = 
                    _versionMajor.ToString(CultureInfo.InvariantCulture);
                nodeVersion.Attributes.GetNamedItem("minor").InnerText = 
                    _versionMinor.ToString(CultureInfo.InvariantCulture);
                nodeVersion.Attributes.GetNamedItem("build").InnerText = 
                    _versionBuild.ToString(CultureInfo.InvariantCulture);
                xmlDoc.Save(stFile.FullName);
            } catch(XmlException ex) {
                throw new BuildException("Error parsing / updating version xml", 
                    Location, ex);
            }

            stFile.checkin("version updated via stautolabel", starTeamLockTypeStatics.UNLOCKED, 
                true, true, true);
            this.Label = string.Format(CultureInfo.InvariantCulture, 
                "{0}{1}.{2}.{3}", this.Label, _versionMajor, _versionMinor, 
                _versionBuild);
            this.Properties["label"] = this.Label;
            this.Properties["Version.text"] = _versionMajor.ToString(CultureInfo.InvariantCulture) + "." 
                + _versionMinor.ToString(CultureInfo.InvariantCulture) + "." 
                + _versionBuild.ToString(CultureInfo.InvariantCulture);
            this.Properties["Version.major"] = _versionMajor.ToString(CultureInfo.InvariantCulture);
            this.Properties["Version.minor"] = _versionMinor.ToString(CultureInfo.InvariantCulture);
            this.Properties["Version.build"] = _versionBuild.ToString(CultureInfo.InvariantCulture);

            createLabel(snapshot);
        }

        /// <summary>
        /// Locate the <c>versionnumber.xml</c> file in the repository. If it 
        /// is not present, the file is created. The file is checked out 
        /// exclusively for editing.
        /// </summary>
        /// <param name="snapshot">StarTeam view we are working with.</param>
        /// <returns>
        /// StarTeam file handle containing version xml.
        /// </returns>
        private InterOpStarTeam.StFile getVersionStFile(InterOpStarTeam.StView snapshot) {
            InterOpStarTeam.StFile stVersionFile = null; 
            //connect to starteam and get root folder 
            InterOpStarTeam.StFolder starTeamRootFolder = snapshot.RootFolder;

            //get contents of root folder and look for version file
            //this is weird as I cannot see how to ask StarTeam for an individual file
            foreach(InterOpStarTeam.StFile stFile in starTeamRootFolder.getItems("File")) {
                if (stFile.Name == _versionFile) {
                    stVersionFile = stFile;
                    break;
                }
            }

            if (stVersionFile == null) {
                stVersionFile = createVersionStFile(starTeamRootFolder);
            } else {
                stVersionFile.checkout(starTeamLockTypeStatics.EXCLUSIVE, true, 
                    true, true);
            }
            return stVersionFile;
        }

        /// <summary>
        /// Creates the versionumber.xml file in the repository.
        /// </summary>
        /// <param name="stFolder">StarTeam folder desired to put the versionnumber.xml files into</param>
        /// <returns>StarTeam File handle to the created file.</returns>
        private InterOpStarTeam.StFile createVersionStFile(InterOpStarTeam.StFolder stFolder) {
            // instantiated here as they are only necessary when adding 
            InterOpStarTeam.StFileFactoryClass starteamFileFactory = new InterOpStarTeam.StFileFactoryClass();
            string versionFilePath = stFolder.getFilePath(_versionFile);

            // create xml and save to local file
            try {
                StreamWriter s = new StreamWriter(versionFilePath, false, System.Text.ASCIIEncoding.ASCII);
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
            } catch (System.Security.SecurityException ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture, 
                    "You do not have access to '{0}'.", versionFilePath), Location, ex);
            } catch (IOException ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture, 
                    "Version filepath '{0}' is invalid.", versionFilePath), Location, ex);
            }

            //add local file to starteam 
            InterOpStarTeam.StFile newFile = starteamFileFactory.Create(stFolder);
            string comment = "version number xml created by stautonumber NAnt task";
            newFile.Add(versionFilePath, _versionFile, comment, comment, 
                starTeamLockTypeStatics.UNLOCKED, true, true);

            return newFile;
        }
    }
}
