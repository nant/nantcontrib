//
// NAntContrib
//
// Copyright (C) 2004 James Geurts (jgeurts@users.sourceforge.net)
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
using System.Xml;

using NAnt.Core.Attributes;

using NAnt.Contrib.Types;
using NAnt.Contrib.Schemas.Msi;

namespace NAnt.Contrib.Tasks.Msi {
    /// <summary>
    /// Base class for msi/msm installer tasks
    /// </summary>
    public abstract class InstallerTaskBase : SchemaValidatedTask {
        #region Attributes

        /// <summary>
        /// The name of the file that will be generated when the task completes 
        /// execution (eg. MyInstall.msi or MyMergeModule.msm).
        /// </summary>
        [TaskAttribute("output", Required=true, ProcessXml=false)]
        public string MsiOutput {
            get { return null; }
        }

        /// <summary>
        /// A directory relative to the NAnt script in which the msi task resides 
        /// from which to retrieve files  that will be installed by the msi 
        /// database. All files that will be included in your installation need 
        /// to be located directly within or in subdirectories of this directory. 
        /// </summary>
        [TaskAttribute("sourcedir", Required=true, ProcessXml=false)]
        public string MsiSourceDir {
            get { return null; }
        }        

        /// <summary>
        /// A installer file to use as the starting database in which all files 
        /// and entries will be made, and then copied to the filename specified 
        /// by the output parameter. Install templates are included with the 
        /// install tasks, you only need to supply this value if you want to 
        /// override the default template. 
        /// </summary>
        [TaskAttribute("template", Required=false, ProcessXml=false)]
        public string MsiTemplate {
            get { return null; }
        }

        /// <summary>
        /// A .mst file to use as the starting database containing strings 
        /// displayed to the user when errors occur during installation. 
        /// A .mst template is included with the msi task, you only need to 
        /// supply this value if you want to override the default error 
        /// template and cannot perform something through the features of the 
        /// msi task.
        /// </summary>
        [TaskAttribute("errortemplate", Required=false, ProcessXml=false)]
        public string MsiErrorTemplate {
            get { return null; }
        }

        /// <summary>
        /// Causes the generated msi database file to contain debug messages for 
        /// errors created by inconsistencies in creation of the database. This 
        /// makes the file slightly larger and should be avoided when generating 
        /// a production release of your software.
        /// </summary>
        [TaskAttribute("debug", Required=false, ProcessXml=false)]
        public bool MsiDebug {
            get { return false; }
        }

        #endregion Attributes

        #region Nested build elements

        /// <summary>
        /// <para>
        /// Sets various properties in the SummaryInformation stream 
        /// (http://msdn.microsoft.com/library/default.asp?url=/library/en-us/msi/setup/summary_information_stream.asp)
        /// </para>
        /// <para>
        ///    All of the sub-elements are optional.
        /// </para>
        /// <h3>Nested Elements:</h3>
        /// <h4>&lt;title&gt;</h4>
        /// <ul>
        /// Briefly describes the type of installer package. Phrases such as "Installation Database" or 
        /// "Transform" or "Patch" may be used for this property.
        /// <br />Default value: Value of the <c>ProductName</c> property, if defined.
        /// </ul>
        /// <h4>&lt;/title&gt;</h4>
        /// <h4>&lt;subject&gt;</h4>
        /// <ul>
        /// A short description of the product to be installed.  This value is typically set from the installer 
        /// property <c>ProductName</c>
        /// <br />Default value: Value of the <c>ProductName</c> property, if defined.
        /// </ul>
        /// <h4>&lt;/subject&gt;</h4>
        /// <h4>&lt;author&gt;</h4>
        /// <ul>
        /// The manufacturer of the installation database. This value is typically set from the installer 
        /// property <c>Manufacturer</c>.
        /// <br />Default value: Value of the <c>Manufacturer</c> property, if defined.
        /// </ul>
        /// <h4>&lt;/author&gt;</h4>
        /// <h4>&lt;keywords&gt;</h4>
        /// <ul>
        /// Used by file browsers to hold keywords that permit the database file to be found in a keyword search. 
        /// The set of keywords typically includes "Installer" as well as product-specific keywords, and may be 
        /// localized.
        /// <br />Default value: Value of the <c>Keywords</c> property, if defined.
        /// </ul>
        /// <h4>&lt;/keywords&gt;</h4>
        /// <h4>&lt;comments&gt;</h4>
        /// <ul>
        /// A general description/purpose of the installer database.
        /// <br />Default value: Value of the <c>Comments</c> property, if defined.
        /// </ul>
        /// <h4>&lt;/comments&gt;</h4>
        /// <h4>&lt;template&gt;</h4>
        /// <ul>
        /// <para>
        /// Indicates the platform and language versions that are supported by the database. The Template Summary 
        /// Property of a patch package is a semicolon-delimited list of the product codes that can accept the 
        /// patch.
        /// </para>
        /// <para>
        ///    See http://msdn.microsoft.com/library/default.asp?url=/library/en-us/msi/setup/template_summary_property.asp for more information.
        /// </para>
        /// </ul>
        /// <h4>&lt;/template&gt;</h4>
        /// <h4>&lt;revisionnumber&gt;</h4>
        /// <ul>
        /// Contains the package code (GUID) for the installer package. The package code is a unique identifier 
        /// of the installer package.  Note: Default behavior - a new GUID is generated every time
        /// </ul>
        /// <h4>&lt;/revisionnumber&gt;</h4>
        /// <h4>&lt;creatingapplication&gt;</h4>
        /// <ul>
        /// The name of the application used to author the database.  Note: Default value is NAnt.
        /// </ul>
        /// <h4>&lt;/creatingapplication&gt;</h4>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Define specific summary information.</para>
        ///     <code>
        /// &lt;summaryinformation&gt;
        ///     &lt;title&gt;Installation Database&lt;/title&gt;
        ///     &lt;subject&gt;${install.productname}&lt;/subject&gt;
        ///     &lt;author&gt;${install.manufacturer}&lt;/author&gt;
        ///     &lt;keywords&gt;MSI, database, NAnt, Installer&lt;/keywords&gt;
        ///     &lt;comments&gt;This installer database contains the logic and data required to install NAnt&lt;/comments&gt;
        ///     &lt;template&gt;;1033&lt;/template&gt;
        ///     &lt;creatingapplication&gt;NAnt - http://nant.sf.net &lt;/creatingapplication&gt;
        /// &lt;/summaryinformation&gt; 
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("summaryinformation", ProcessXml=false)]
        public SchemaElement[] InstallerSummaryInformationElement {
            get { return null; }
            set {}
        }     
        
        /// <summary>
        /// <para>
        /// Name/value pairs which will be set in the PROPERTY table of the 
        /// installer database.
        /// </para>
        /// <para>
        /// The properties element contains within it one to any number of property elements.<br/>
        /// <see href="http://msdn.microsoft.com/library/en-us/msi/setup/protected_properties.asp">Public property</see> names cannot contain lowercase letters.<br/>
        /// <see href="http://msdn.microsoft.com/library/en-us/msi/setup/protected_properties.asp">Private property</see> names must contain some lowercase letters.<br/>
        /// Property names prefixed with % represent system and user environment variables. These are 
        /// never entered into the <see href="http://msdn.microsoft.com/library/en-us/msi/setup/property_table.asp">Property 
        /// table</see>. The permanent settings of environment variables can only be modified using the <see href="http://msdn.microsoft.com/library/en-us/msi/setup/environment_table.asp">Environment Table</see>. 
        /// More information is available <see href="http://msdn.microsoft.com/library/default.asp?url=/library/en-us/msi/setup/properties.asp">here</see>.
        /// </para>
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///            <term>Type</term>
        ///            <term>Description</term>
        ///            <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///            <term>name</term>
        ///            <term>string</term>
        ///            <term>A name used to refer to the property.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>value</term>
        ///            <term>string</term>
        ///            <term>The value of the property. This value can contain references to other, predefined properties to build a compound property.</term>
        ///            <term>True</term>
        ///        </item>
        ///    </list>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Define the required properties.</para>
        ///     <code>
        /// &lt;properties&gt;
        ///     &lt;property name="ProductName" value="My Product" /&gt;
        ///     &lt;property name="ProductVersion" value="1.0.0" /&gt;
        ///     &lt;property name="Manufacturer" value="ACME Inc." /&gt;
        ///     &lt;property name="ProductCode" value="{29D8F096-3371-4cba-87E1-A8C6511F7B4C}" /&gt;
        ///     &lt;property name="UpgradeCode" value="{69E66919-0DE1-4280-B4C1-94049F76BA1A}" /&gt;
        /// &lt;/properties&gt; 
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("properties", ProcessXml=false)]
        public SchemaElement[] InstallerPropertiesElement {
            get { return null; }
            set {}
        }        

        /// <summary>
        /// <para>
        /// Contains within it one to any number of app, registry, ini, or dirfile elements. 
        /// These elements are used to search for an existing filesystem directory, file, or 
        /// Windows Registry setting.  A property in the installer database is 
        /// then set with the value obtained from the search.
        /// </para>
        /// <h3>&lt;app&gt;</h3>
        /// <para>
        /// More information on these attributes can be found at: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/msi/setup/complocator_table.asp
        /// </para>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///         <term>Type</term>
        ///         <term>Description</term>
        ///         <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///         <term>componentid</term>
        ///         <term>string</term>
        ///         <term>The component ID of the component whose key path is to be used for the search.</term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>type</term>
        ///         <term>msi:MSILocatorTypeDirFile</term>
        ///         <term>Valid input: <c>file</c> or <c>directory</c></term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>setproperty</term>
        ///         <term>string</term>
        ///         <term>A name used to refer to the property within the Msi database.  Set at install time.</term>
        ///         <term>True</term>
        ///     </item>
        /// </list>
        /// <h3>&lt;/app&gt;</h3>
        /// <h3>&lt;registry&gt;</h3>
        /// <para>
        /// More information on these attributes can be found at: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/msi/setup/reglocator_table.asp
        /// </para>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///         <term>Type</term>
        ///         <term>Description</term>
        ///         <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///         <term>type</term>
        ///         <term>msi:MSILocatorTypeDirFileReg64</term>
        ///         <term>Valid input: <c>registry</c>, <c>file</c>, <c>directory</c>, <c>64bit</c></term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>path</term>
        ///         <term>string</term>
        ///         <term>Depending on the <c>type</c> specified: 
        ///         <list type="bullet">
        ///             <item>Path is a directory.</item>
        ///             <item>Path is a registry key.</item>
        ///         </list>
        ///         </term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>root</term>
        ///         <term>msi:MSIRegistryKeyRoot</term>
        ///         <term>Valid input: 
        ///         <list type="bullet">
        ///             <item><c>dependent</c> - If this is a per-user installation, the registry value is written under HKEY_CURRENT_USER.  If this is a per-machine installation, the registry value is written under HKEY_LOCAL_MACHINE. Note that a per-machine installation is specified by setting the ALLUSERS property to 1.</item>
        ///             <item><c>machine</c> represents HKEY_LOCAL_MACHINE</item>
        ///             <item><c>classes</c> represents HKEY_CLASSES_ROOT</item>
        ///             <item><c>user</c> represents HKEY_CURRENT_USER</item>
        ///             <item><c>users</c> represents HKEY_USERS</item>
        ///         </list>
        ///         </term>
        ///         <term>True</term>
        ///     </item>
        /// </list>
        /// <ul>
        /// <h3>Nested Elements:</h3>
        /// <h4>&lt;value&gt;</h4>
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///         <term>Type</term>
        ///         <term>Description</term>
        ///         <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///         <term>name</term>
        ///         <term>string</term>
        ///         <term>Depending on the <c>type</c> specified: 
        ///         <list type="bullet">
        ///             <item>Key path is a file name.</item>
        ///             <item>Key path is a registry value.</item>
        ///         </list>
        ///         </term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>setproperty</term>
        ///         <term>string</term>
        ///         <term>A name used to refer to the property within the Msi database.  Set at install time.</term>
        ///         <term>True</term>
        ///     </item>
        /// </list>
        /// <h4>&lt;/value&gt;</h4>
        /// </ul>
        /// <h3>&lt;/registry&gt;</h3>
        /// <h3>&lt;ini&gt;</h3>
        /// <para>
        /// More information on these attributes can be found at: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/msi/setup/inilocator_table.asp
        /// </para>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///         <term>Type</term>
        ///         <term>Description</term>
        ///         <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///         <term>filename</term>
        ///         <term>string</term>
        ///         <term>The .ini file name. (The .ini file must be present in the default Microsoft Windows directory.)
        ///         </term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>section</term>
        ///         <term>string</term>
        ///         <term>Section name within the .ini file.
        ///         </term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>key</term>
        ///         <term>string</term>
        ///         <term>Key value within the section.
        ///         </term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>field</term>
        ///         <term>msi:nonNegativeInt</term>
        ///         <term>The field in the .ini line. If Field is Null or 0, then the entire line is read. 
        ///         This must be a non-negative number.
        ///         </term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>type</term>
        ///         <term>msi:MSILocatorTypeDirFileRaw</term>
        ///         <term>Valid input: <c>file</c> ,<c>directory</c>, or <c>raw</c></term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>setproperty</term>
        ///         <term>string</term>
        ///         <term>A name used to refer to the property within the Msi database.  Set at install time.</term>
        ///         <term>True</term>
        ///     </item>
        /// </list>
        /// <h3>&lt;/ini&gt;</h3>
        /// <h3>&lt;dirfile&gt;</h3>
        /// <para>
        /// More information on these attributes can be found at: 
        /// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/msi/setup/drlocator_table.asp
        /// and
        /// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/msi/setup/signature_table.asp
        /// </para>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///         <term>Type</term>
        ///         <term>Description</term>
        ///         <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///         <term>parent</term>
        ///         <term>string</term>
        ///         <term>An identifier to RegLocator, IniLocator, or CompLocator tables.  
        ///         If it does not expand to a full path, then all the fixed drives of the user's system are searched by using the Path.
        ///         <br/>In order to determine what the key is for a table, prefix the property name assigned
        ///         to that locator with SIG_
        ///         </term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>path</term>
        ///         <term>string</term>
        ///         <term>the path on the user's system. This is a either a full path or a relative subpath 
        ///         below the directory specified in the Parent column.
        ///         </term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>depth</term>
        ///         <term>msi:nonNegativeInt</term>
        ///         <term>The depth below the path that the installer searches for the file or directory.
        ///         </term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>setproperty</term>
        ///         <term>string</term>
        ///         <term>A name used to refer to the property within the Msi database.  Set at install time.</term>
        ///         <term>True</term>
        ///     </item>
        /// </list>
        /// <ul>
        /// <h3>Nested Elements:</h3>
        /// <h4>&lt;file&gt;</h4>
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///         <term>Type</term>
        ///         <term>Description</term>
        ///         <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///         <term>name</term>
        ///         <term>string</term>
        ///         <term>The name of the file.</term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>minversion</term>
        ///         <term>string</term>
        ///         <term>The minimum version of the file, with a language comparison. If this field is 
        ///         specified, then the file must have a version that is at least equal to MinVersion. 
        ///         If the file has an equal version to the MinVersion field value but the language 
        ///         specified in the Languages column differs, the file does not satisfy the signature 
        ///         filter criteria.</term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>maxversion</term>
        ///         <term>string</term>
        ///         <term>The maximum version of the file. If this field is specified, then the file 
        ///         must have a version that is at most equal to MaxVersion.</term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>minsize</term>
        ///         <term>msi:nonNegativeInt</term>
        ///         <term>The minimum size of the file. If this field is specified, then the file 
        ///         under inspection must have a size that is at least equal to MinSize. This must 
        ///         be a non-negative number.</term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>maxsize</term>
        ///         <term>msi:nonNegativeInt</term>
        ///         <term>The maximum size of the file. If this field is specified, then the file 
        ///         under inspection must have a size that is at most equal to MaxSize. This must 
        ///         be a non-negative number.</term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>mindate</term>
        ///         <term>msi:nonNegativeInt</term>
        ///         <term>The minimum modification date and time of the file. If this field is 
        ///         specified, then the file under inspection must have a modification date and time 
        ///         that is at least equal to MinDate. This must be a non-negative number.</term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>maxdate</term>
        ///         <term>msi:nonNegativeInt</term>
        ///         <term>The maximum creation date of the file. If this field is specified, then the 
        ///         file under inspection must have a creation date that is at most equal to MaxDate. 
        ///         This must be a non-negative number.</term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>languages</term>
        ///         <term>string</term>
        ///         <term>The languages supported by the file.</term>
        ///         <term>False</term>
        ///     </item>
        /// </list>
        /// <h4>&lt;/file&gt;</h4>
        /// </ul>
        /// <h3>&lt;/dirfile&gt;</h3>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Get the path of the web directory and the version of IIS.  Create new properties in the Msi file with those values.</para>
        ///     <code>
        /// &lt;search&gt;
        ///     &lt;registry type="registry" path="Software\Microsoft\InetStp" root="machine" &gt;
        ///         &lt;value name="PathWWWRoot" setproperty="IISWWWROOT" /&gt;
        ///     &lt;/registry&gt;
        ///     &lt;registry type="registry" path="SYSTEM\CurrentControlSet\Services\W3SVC\Parameters" root="machine" &gt;
        ///             &lt;value name="MajorVersion" setproperty="IISVERSION" /&gt;
        ///     &lt;/registry&gt;
        /// &lt;/search&gt; 
        ///     </code>
        /// </example>
        /// <example>
        ///     <para>Shows two ways to get the default key value for the specified key.  Create new properties in the Msi file with those values.</para>
        ///     <code>
        /// &lt;search&gt;
        ///     &lt;registry type="registry" path="Software\Microsoft\MessengerService" root="machine" &gt;
        ///         &lt;value setproperty="MSGSRVNAME" /&gt;
        ///         &lt;value name="" setproperty="MSGSRVNAME2" /&gt;
        ///     &lt;/registry&gt;
        /// &lt;/search&gt; 
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("search", ProcessXml=false)]
        public SchemaElement[] InstallerSearchElement {
            get { return null; }
            set {}
        }

        /// <summary>
        /// <para>
        /// Contains within it one to any number of launchcondition elements.  
        /// Launch conditions are conditions that all must be satisfied for the 
        /// installation to begin.
        /// </para>
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///         <term>Type</term>
        ///         <term>Description</term>
        ///         <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///         <term>name</term>
        ///         <term>string</term>
        ///         <term>A name used to identify the launch condition.</term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>condition</term>
        ///         <term>string</term>
        ///         <term>Expression that must evaluate to True for installation to begin.</term>
        ///         <term>True</term>
        ///     </item>
        /// </list>
        /// <h3>Nested Elements:</h3>
        /// <h4>&lt;description&gt;</h4>
        /// <ul>
        /// Localizable text to display when the condition fails and the installation must be terminated.
        /// </ul>
        /// <h4>&lt;/description&gt;</h4>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Create a check to make sure that IIS 5.x is installed.</para>
        ///     <code>
        /// &lt;launchconditions&gt;
        ///     &lt;launchcondition name="CheckIIS" condition="(IISVERSION = &amp;quot;#5&amp;quot;)" &gt;
        ///         &lt;description&gt;
        ///             This setup requires Internet information Server 5.x.  Please install Internet Information Server and run this setup again.
        ///         &lt;/description&gt;
        ///     &lt;/launchcondition&gt;
        /// &lt;/launchconditions&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("launchconditions", ProcessXml=false)]
        public SchemaElement[] InstallerLaunchConditionsElement {
            get { return null; }
            set {}
        }

        /// <summary>
        /// <para>
        /// Creates custom tables not directly managed by default features of 
        /// the installer task.
        /// </para>
        /// <h3>Parameters</h3>
        /// <list type="table">
        /// <listheader>
        ///     <term>Attribute</term>
        ///     <term>Type</term>
        ///     <term>Description</term>
        ///     <term>Required</term>
        /// </listheader>
        /// <item>
        ///     <term>name</term>
        ///     <term>string</term>
        ///     <term>A unique name used to identify the table.</term>
        ///     <term>True</term>
        /// </item>
        /// </list>
        /// <h3>Nested Elements:</h3>
        /// <h4>&lt;columns&gt;</h4>
        ///     <ul>
        ///         <h4>&lt;column&gt;</h4>
        ///             <ul>
        ///                 Defines the columns making up the table
        ///                 <h3>Parameters</h3>
        ///                 <list type="table">
        ///                       <listheader>
        ///                            <term>Attribute</term>
        ///                            <term>Type</term>
        ///                            <term>Description</term>
        ///                            <term>Required</term>
        ///                       </listheader>
        ///                       <item>
        ///                            <term>name</term>
        ///                            <term>string</term>
        ///                            <term>A unique name used to define the column.</term>
        ///                            <term>True</term>
        ///                       </item>
        ///                       <item>
        ///                            <term>nullable</term>
        ///                            <term>bool</term>
        ///                            <term>When set to <c>true</c>, allows the column to accept null values; <c>false</c> does not allow null values.</term>
        ///                            <term>True</term>
        ///                       </item>
        ///                       <item>
        ///                            <term>category</term>
        ///                            <term>msi:MSITableColumnCategoryType</term>
        ///                            <term>Valid input:
        ///                            <list type="bullet">
        ///                                <item><c>Text</c></item>
        ///                                <item><c>UpperCase</c></item>
        ///                                <item><c>LowerCase</c></item>
        ///                                <item><c>Integer</c></item>
        ///                                <item><c>DoubleInteger</c></item>
        ///                                <item><c>Time/Date</c></item>
        ///                                <item><c>Identifier</c></item>
        ///                                <item><c>Property</c></item>
        ///                                <item><c>Filename</c></item>
        ///                                <item><c>WildCardFilename</c></item>
        ///                                <item><c>Path</c></item>
        ///                                <item><c>Paths</c></item>
        ///                                <item><c>AnyPath</c></item>
        ///                                <item><c>DefaultDir</c></item>
        ///                                <item><c>RegPath</c></item>
        ///                                <item><c>Formatted</c></item>
        ///                                <item><c>Template</c></item>
        ///                                <item><c>Condition</c></item>
        ///                                <item><c>GUID</c></item>
        ///                                <item><c>Version</c></item>
        ///                                <item><c>Language</c></item>
        ///                                <item><c>Binary</c></item>
        ///                                <item><c>Cabinet</c></item>
        ///                                <item><c>Shortcut</c></item>
        ///                            </list>
        ///                            More information here: <a href="http://msdn.microsoft.com/library/en-us/msi/setup/column_data_types.asp">http://msdn.microsoft.com/library/en-us/msi/setup/column_data_types.asp</a>
        ///                            </term>
        ///                            <term>False</term>
        ///                        </item>
        ///                        <item>
        ///                            <term>type</term>
        ///                            <term>string</term>
        ///                            <term>Overrides the <c>category</c> specification.  An example of valid input would be: <c>S255</c></term>
        ///                            <term>False</term>
        ///                        </item>
        ///                        <item>
        ///                            <term>key</term>
        ///                            <term>bool</term>
        ///                            <term>When set to <c>true</c>, the column is used to form the primary key for the table; <c>false</c> specifies that the column is not used to form the primary key.</term>
        ///                            <term>False</term>
        ///                        </item>
        ///                        <item>
        ///                            <term>minvalue</term>
        ///                            <term>int</term>
        ///                            <term>This field applies to columns having numeric value. The field contains the minimum permissible value. This can be the minimum value for an integer or the minimum value for a date or version string.</term>
        ///                            <term>False</term>
        ///                        </item>
        ///                        <item>
        ///                            <term>maxvalue</term>
        ///                            <term>int</term>
        ///                            <term>This field applies to columns having numeric value. The field is the maximum permissible value. This may be the maximum value for an integer or the maximum value for a date or version string. </term>
        ///                            <term>False</term>
        ///                        </item>
        ///                        <item>
        ///                            <term>keytable</term>
        ///                            <term>string</term>
        ///                            <term>This field applies to columns that are external keys. The field identified in Column must link to the column number specified by KeyColumn in the table named in KeyTable. This can be a list of tables separated by semicolons.</term>
        ///                            <term>False</term>
        ///                        </item>
        ///                        <item>
        ///                            <term>keycolumn</term>
        ///                            <term>int</term>
        ///                            <term>This field applies to table columns that are external keys. The field identified in Column must link to the column number specified by KeyColumn in the table named in KeyTable. The permissible range of the KeyColumn field is 1-32.</term>
        ///                            <term>False</term>
        ///                        </item>
        ///                        <item>
        ///                            <term>set</term>
        ///                            <term>string</term>
        ///                            <term>This is a list of permissible values for this field separated by semicolons. This field is usually used for enums.</term>
        ///                            <term>False</term>
        ///                        </item>
        ///                        <item>
        ///                            <term>description</term>
        ///                            <term>string</term>
        ///                            <term>A description of the data that is stored in the column. </term>
        ///                            <term>False</term>
        ///                        </item>
        ///                   </list>
        ///              </ul>
        ///          <h4>&lt;/column&gt;</h4>
        ///      </ul>
        /// <h4>&lt;/columns&gt;</h4>
        /// <h4>&lt;rows&gt;</h4>
        ///     <ul>
        ///         <h4>&lt;row&gt;</h4>
        ///         <ul>
        ///             Defines the data for a row in the table
        ///             <h4>&lt;columns&gt;</h4>
        ///             <ul>
        ///                 <h4>&lt;column&gt;</h4>
        ///                 <ul>
        ///                     Defines data for a specific cell in the row
        ///                     <h3>Parameters</h3>
        ///                     <list type="table">
        ///                         <listheader>
        ///                             <term>Attribute</term>
        ///                             <term>Type</term>
        ///                             <term>Description</term>
        ///                             <term>Required</term>
        ///                         </listheader>
        ///                         <item>
        ///                            <term>name</term>
        ///                            <term>string</term>
        ///                            <term>Name of the column to populate.</term>
        ///                            <term>True</term>
        ///                        </item>
        ///                        <item>
        ///                            <term>value</term>
        ///                            <term>string</term>
        ///                            <term>Value to populate the cell with.</term>
        ///                            <term>True</term>
        ///                        </item>
        ///                     </list>
        ///                 </ul>
        ///                 <h4>&lt;/column&gt;</h4>
        ///             </ul>
        ///             <h4>&lt;/columns&gt;</h4>
        ///         </ul>
        ///         <h4>&lt;/row&gt;</h4>
        ///     </ul>
        /// <h4>&lt;/rows&gt;</h4>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>
        ///         Build the IniFile table.  Since the WriteIniValues and RemoveIniValues actions exist in the template, they will use this table.
        ///     </para>
        ///     <code>
        /// &lt;tables&gt;
        ///     &lt;table name="IniFile"&gt;
        ///         &lt;columns&gt;
        ///             &lt;column name="IniFile" nullable="false" category="Identifier" key="true" description="The key for this table." /&gt;
        ///             &lt;column name="FileName" nullable="false" category="Text" description="The localizable name of the .ini file in which to write the information. " /&gt;
        ///             &lt;column name="DirProperty" nullable="true" category="Identifier" description="Name of a property having a value that resolves to the full path of the folder containing the .ini file. " /&gt; 
        ///             &lt;column name="Section" nullable="false" category="Formatted" description="The localizable .ini file section." /&gt;
        ///             &lt;column name="Key" nullable="false" category="Formatted" description="The localizable .ini file key within the section" /&gt;
        ///             &lt;column name="Value" nullable="false" category="Formatted" description="The localizable value to be written. " /&gt;
        ///             &lt;column name="Action" nullable="false" category="Integer" description="The type of modification to be made. " /&gt;
        ///             &lt;column name="Component_" nullable="false" category="Identifier" description="External key into the first column of the Component table referencing the component that controls the installation of the .ini value. " /&gt;
        ///         &lt;/columns&gt;
        ///         &lt;rows&gt;
        ///             &lt;row&gt;
        ///                 &lt;columns&gt;
        ///                     &lt;column name="IniFile" value="MyInternetShortcut" /&gt;
        ///                     &lt;column name="FileName" value="MyInternetAddr.url" /&gt;
        ///                     &lt;column name="DirProperty" value="D__MYDIR" /&gt;
        ///                     &lt;column name="Section" value="InternetShortcut" /&gt;
        ///                     &lt;column name="Key" value="URL" /&gt;
        ///                     &lt;column name="Value" value="[TARGETURL]" /&gt;
        ///                     &lt;column name="Action" value="0" /&gt;
        ///                     &lt;column name="Component_" value="C__Documentation" /&gt;
        ///                 &lt;/columns&gt;
        ///             &lt;/row&gt;
        ///         &lt;/rows&gt;
        ///     &lt;/table&gt;
        /// &lt;/tables&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("tables", ProcessXml=false)]
        public SchemaElement[] InstallerTablesElement {
            get { return null; }
            set {}
        }
        
        /// <summary>
        /// <para>
        /// Specifies the directory layout for the product.
        /// </para>
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///         <term>Type</term>
        ///         <term>Description</term>
        ///         <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///         <term>name</term>
        ///         <term>string</term>
        ///         <term>A name used to refer to the directory.</term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>foldername</term>
        ///         <term>string</term>
        ///         <term>The directory's name (localizable)under the parent directory.</term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>root</term>
        ///         <term>string</term>
        ///         <term>A reference to the directory's parent directory.  This can be a property name or one of the predefined directories included with the default template:
        ///         <list type="bullet">
        ///             <item><c>AdminToolsFolder</c></item>
        ///             <item><c>AppDataFolder</c></item>
        ///             <item><c>CommonAppDataFolder</c></item>
        ///             <item><c>CommonFiles64Folder</c></item>
        ///             <item><c>CommonFilesFolder</c></item>
        ///             <item><c>DesktopFolder</c></item>
        ///             <item><c>FavoritesFolder</c></item>
        ///             <item><c>FontsFolder</c></item>
        ///             <item><c>LocalAppDataFolder</c></item>
        ///             <item><c>MyPicturesFolder</c></item>
        ///             <item><c>PersonalFolder</c></item>
        ///             <item><c>ProgramFilesFolder</c></item>
        ///             <item><c>ProgramMenuFolder</c></item>
        ///             <item><c>ProgramFiles64Folder</c></item>
        ///             <item><c>SendToFolder</c></item>
        ///             <item><c>StartMenuFolder</c></item>
        ///             <item><c>StartupFolder</c></item>
        ///             <item><c>System16Folder</c></item>
        ///             <item><c>System64Folder</c></item>
        ///             <item><c>SystemFolder</c></item>
        ///             <item><c>TARGETDIR</c></item>
        ///             <item><c>TempFolder</c></item>
        ///             <item><c>TemplateFolder</c></item>
        ///             <item><c>WindowsFolder</c></item>
        ///             <item><c>WindowsVolume</c></item>
        ///            </list>
        ///            </term>
        ///            <term>True</term>
        ///        </item>
        /// </list>
        /// <h3>Nested Elements:</h3>
        /// <h4>&lt;directory&gt;</h4>
        /// <ul>
        ///     Sub directories.  Note, this element can contain nested &lt;directory/&gt; sub elements.
        ///     <h3>Parameters</h3>
        ///     <list type="table">
        ///         <listheader>
        ///             <term>Attribute</term>
        ///             <term>Type</term>
        ///             <term>Description</term>
        ///             <term>Required</term>
        ///         </listheader>
        ///         <item>
        ///             <term>name</term>
        ///             <term>string</term>
        ///             <term>A name used to refer to the directory.</term>
        ///             <term>True</term>
        ///         </item>
        ///         <item>
        ///             <term>foldername</term>
        ///             <term>string</term>
        ///             <term>The directory's name (localizable)under the parent directory.</term>
        ///             <term>True</term>
        ///         </item>
        ///     </list>
        /// </ul>
        /// <h4>&lt;/directory&gt;</h4>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Define a sample directory structure.</para>
        ///     <code>
        /// &lt;directories&gt;
        ///     &lt;directory name="D__ACME" foldername="ACME" root="TARGETDIR" &gt;
        ///         &lt;directory name="D__ACME_MyProduct" foldername="My Product" /&gt;
        ///     &lt;/directory&gt;
        /// &lt;/directories&gt; 
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("directories", ProcessXml=false)]
        public SchemaElement[] InstallerDirectoriesElement {
            get { return null; }
            set {}
        }

        /// <summary>
        /// <para>
        /// Used to modify the environment variables of the target computer at 
        /// runtime.
        /// </para>
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///            <term>Attribute</term>
        ///            <term>Type</term>
        ///            <term>Description</term>
        ///            <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///         <term>name</term>
        ///         <term>string</term>
        ///         <term>The localizable name of the environment variable. The key values are written or removed depending upon 
        ///         which of the characters in the following table are prefixed to the name. There is no effect in the ordering of 
        ///         the symbols used in a prefix.
        ///         <list type="table">
        ///             <listheader>
        ///                 <term>Prefix</term>
        ///                 <description>Description</description>
        ///             </listheader>
        ///             <item>
        ///                 <term>=</term>
        ///                 <description>Create the environment variable if it does not exist, and then set it during installation. If the environment variable exists, set it during the installation.</description>
        ///             </item>
        ///             <item>
        ///                 <term>+</term>
        ///                 <description>Create the environment variable if it does not exist, then set it during installation. This has no effect on the value of the environment variable if it already exists.</description>
        ///             </item>
        ///             <item>
        ///                 <term>-</term>
        ///                 <description>Remove the environment variable when the component is removed. This symbol can be combined with any prefix.</description>
        ///             </item>
        ///             <item>
        ///                 <term>!</term>
        ///                 <description>Remove the environment variable during an installation. The installer only removes an environment variable during an installation if the name and value of the variable match the entries in the Name and Value fields of the Environment table. If you want to remove an environment variable, regardless of its value, use the '!' syntax, and leave the Value field empty.</description>
        ///             </item>
        ///             <item>
        ///                 <term>*</term>
        ///                 <description>This prefix is used with Microsoft® Windows® NT/Windows® 2000 to indicate that the name refers to a system environment variable. If no asterisk is present, the installer writes the variable to the user's environment. Microsoft Windows 95/98 ignores the asterisk and add the environment variable to autoexec.bat. This symbol can be combined with any prefix. A package that is used for per-machine installations should write environment variables to the machine's environment by including * in the Name column. For more information, see <a href="http://msdn.microsoft.com/library/en-us/msi/setup/environment_table.asp">http://msdn.microsoft.com/library/en-us/msi/setup/environment_table.asp</a></description>
        ///             </item>
        ///             <item>
        ///                 <term>=-</term>
        ///                 <description>The environment variable is set on install and removed on uninstall. This is the usual behavior.</description>
        ///             </item>
        ///             <item>
        ///                 <term>!-</term>
        ///                 <description>Removes an environment variable during an install or uninstall.</description>
        ///             </item>
        ///         </list>
        ///         More information can be found here: <a href="http://msdn.microsoft.com/library/en-us/msi/setup/environment_table.asp">http://msdn.microsoft.com/library/en-us/msi/setup/environment_table.asp</a>
        ///         </term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>append</term>
        ///         <term>string</term>
        ///         <term>Localizable value that is to be set as a <a href="http://msdn.microsoft.com/library/en-us/msi/setup/formatted.asp">formatted</a> string</term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>component</term>
        ///         <term>string</term>
        ///         <term>Refrence to a component.  Allows the variabled to be modified when the component is un/installed.</term>
        ///         <term>True</term>
        ///     </item>
        /// </list>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Append the installation path to the user PATH variable.</para>
        ///     <code>
        /// &lt;environment&gt;
        ///     &lt;variable name="PATH" append="'[TARGETDIR]'" component="C__MainFiles" /&gt;
        /// &lt;/environment&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("environment", ProcessXml=false)]
        public SchemaElement[] InstallerEnvironmentElement {
            get { return null; }
            set {}
        }

        /// <summary>
        /// <para>
        /// Groups sets of files into named sets, these can be used to install 
        /// and perform operations on a set of files as one entity. 
        /// </para>
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///         <term>Type</term>
        ///         <term>Description</term>
        ///         <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///         <term>name</term>
        ///         <term>string</term>
        ///         <term>A name used to refer to the component.</term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>id</term>
        ///         <term>string</term>
        ///         <term>A string GUID unique to this component, version, and language.<br/>Note that the letters of these GUIDs must be 
        ///         uppercase. Utilities such as GUIDGEN can generate GUIDs containing lowercase letters. The lowercase letters must be 
        ///         changed to uppercase to make these valid component code GUIDs.
        ///         </term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>attr</term>
        ///         <term>int</term>
        ///         <term>This column contains a bit flag that specifies options for remote execution. Add the indicated bit to the total value in the column to include an option. 
        ///         <list type="table">
        ///             <listheader>
        ///                 <term>Value</term>
        ///                 <description>Description</description>
        ///             </listheader>
        ///             <item>
        ///                 <term>0</term>
        ///                 <description>Component cannot be run from source. <br/>
        ///                 Set this bit for all components belonging to a feature to prevent the feature from being run-from-network or 
        ///                 run-from-source. Note that if a feature has no components, the feature always shows run-from-source and 
        ///                 run-from-my-computer as valid options.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <term>1</term>
        ///                 <description>Component can only be run from source. <br/>
        ///                 Set this bit for all components belonging to a feature to prevent the feature from being run-from-my-computer. 
        ///                 Note that if a feature has no components, the feature always shows run-from-source and run-from-my-computer as 
        ///                 valid options.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <term>2</term>
        ///                 <description>Component can run locally or from source.</description>
        ///             </item>
        ///             <item>
        ///                 <term>4</term>
        ///                 <description>If this bit is set, the value in the key element is used as a key into the Registry table.<br/>
        ///                 If the Value field of the corresponding record in the Registry table is null, the Name field in that record must 
        ///                 not contain "+", "-", or "*". For more information, see the description of the Name field in Registry table.<br/>
        ///                 Setting this bit is recommended for registry entries written to the HKCU hive. This ensures the installer writes 
        ///                 the necessary HKCU registry entries when there are multiple users on the same machine.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <term>16</term>
        ///                 <description>If this bit is set, the installer does not remove the component during an uninstall. The installer registers an extra system client for the component in the Windows Installer registry settings.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <term>32</term>
        ///                 <description>If this bit is set, the value in the KeyPath column is a key into the ODBCDataSource table.</description>
        ///             </item>
        ///             <item>
        ///                 <term>64</term>
        ///                 <description>If this bit is set, the installer reevaluates the value of the statement in the Condition column 
        ///                 upon a reinstall. If the value was previously False and has changed to True, the installer installs the component. 
        ///                 If the value was previously True and has changed to False, the installer removes the component even if the component 
        ///                 has other products as clients. <br/>This bit should only be set for transitive components. See Using Transitive 
        ///                 Components.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <term>128</term>
        ///                 <description>If this bit is set, the installer does not install or reinstall the component if a key path file or 
        ///                 a key path registry entry for the component already exists. The application does register itself as a client of 
        ///                 the component. <br/>
        ///                 Use this flag only for components that are being registered by the Registry table.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <term>256</term>
        ///                 <description>Set this bit to mark this as a 64-bit component. This attribute facilitates the installation of 
        ///                 packages that include both 32-bit and 64-bit components. If this bit is not set, the component is registered 
        ///                 as a 32-bit component.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///         </term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>directory</term>
        ///         <term>string</term>
        ///         <term>Refrence to a directory.  Defines the directory location for where the files assigned to the component are to be placed.</term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>feature</term>
        ///         <term>string</term>
        ///         <term>Refrence to a feature.  Maps a feature to the component.  Used to determine if the component is to be installed or not.</term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>condition</term>
        ///         <term>string</term>
        ///         <term>A conditional statement that can control whether a component is installed. If the condition is null or evaluates to 
        ///         true, then the component is enabled. If the condition evaluates to False, then the component is disabled and is not 
        ///         installed.</term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>fileattr</term>
        ///         <term>int</term>
        ///         <term>Integer containing bit flags representing file attributes.<br/> 
        ///         The following table shows the definition of the bit field.
        ///         <list type="table">
        ///             <listheader>
        ///                 <term>Value</term>
        ///                 <description>Description</description>
        ///             </listheader>
        ///             <item>
        ///                 <term>1</term>
        ///                 <description>Read-Only</description>
        ///             </item>
        ///             <item>
        ///                 <term>2</term>
        ///                 <description>Hidden</description>
        ///             </item>
        ///             <item>
        ///                 <term>4</term>
        ///                 <description>System</description>
        ///             </item>
        ///             <item>
        ///                 <term>512</term>
        ///                 <description>The file is vital for the proper operation of the component to which it belongs</description>
        ///             </item>
        ///             <item>
        ///                 <term>1024</term>
        ///                 <description>The file contains a valid checksum. A checksum is required to repair a file that has become corrupted.</description>
        ///             </item>
        ///             <item>
        ///                 <term>4096</term>
        ///                 <description>This bit must only be added by a patch and if the file is being added by the patch.</description>
        ///             </item>
        ///             <item>
        ///                 <term>8192</term>
        ///                 <description>The file's source type is uncompressed. If set, ignore the Word Count Summary Property. 
        ///                 If neither msidbFileAttributesNoncompressed or msidbFileAttributesCompressed are set, the compression 
        ///                 state of the file is specified by the Word Count Summary Property. Do not set both msidbFileAttributesNoncompressed 
        ///                 and msidbFileAttributesCompressed.</description>
        ///             </item>
        ///             <item>
        ///                 <term>16384</term>
        ///                 <description>The file's source type is compressed. If set, ignore the Word Count Summary Property. 
        ///                 If neither msidbFileAttributesNoncompressed or msidbFileAttributesCompressed are set, the compression state of 
        ///                 the file is specified by the Word Count Summary Property. Do not set both msidbFileAttributesNoncompressed and 
        ///                 msidbFileAttributesCompressed.</description>
        ///             </item>
        ///         </list>
        ///         </term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>checkinterop</term>
        ///         <term>bool</term>
        ///         <term>Used to determine if file(s) in the fileset are interop file(s).  If <c>true</c>, extra information will be added in the install
        ///         package to register each interop file. If <c>false</c>, the file(s) will not be not be checked and the extra registration information
        ///         will not be added to the msi. </term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>installassembliestogac</term>
        ///         <term>bool</term>
        ///         <term>Used to determine if assemblies should be installed to the Global Assembly Cache.  
        ///         If <c>true</c>, all assemblies in the fileset will be added to the GAC. If <c>false</c>, the assemblies will be installed
        ///         to the specified directory (as a normal file would).  Note: If an assembly is specified to be installed into the GAC, it will not
        ///         also be installed to the directory specified.</term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>keepsubdirs</term>
        ///         <term>bool</term>
        ///         <term>Used to determine if directories in the fileset should be built.
        ///         If <c>true</c>, all subdirectories of the fileset basedir will be built. If <c>false</c> the directories structure will be
        ///         flattened.  The default is <c>false</c>.</term>
        ///         <term>False</term>
        ///     </item>
        /// </list>
        /// <h3>Nested Elements:</h3>
        /// <h4>&lt;keyfile&gt;</h4>
        ///     <ul>
        ///         This value points to a file or folder belonging to the component that the installer uses to detect the component. Two components cannot share the same key path value.
        ///         <h3>Parameters</h3>
        ///         <list type="table">
        ///             <listheader>
        ///                 <term>Attribute</term>
        ///                 <term>Type</term>
        ///                 <term>Description</term>
        ///                 <term>Required</term>
        ///             </listheader>
        ///             <item>
        ///                 <term>file</term>
        ///                 <term>string</term>
        ///                 <term>Name of the key (file) to use.  Also, this could be an id of a registry key value.</term>
        ///                 <term>True</term>
        ///             </item>
        ///         </list>
        ///     </ul>
        /// <h4>&lt;/keyfile&gt;</h4>
        /// <h4>&lt;fileset&gt;</h4>
        ///     <ul>
        ///         Specifies the files to include with the component
        ///     </ul>
        /// <h4>&lt;/fileset&gt;</h4>
        /// <h4>&lt;forceid&gt;</h4>
        ///     <ul>
        ///         Used to force specific attributes on a per file basis
        ///         <h3>Parameters</h3>
        ///         <list type="table">
        ///             <listheader>
        ///                 <term>Attribute</term>
        ///                 <term>Type</term>
        ///                 <term>Description</term>
        ///                 <term>Required</term>
        ///             </listheader>
        ///             <item>
        ///                 <term>file</term>
        ///                 <term>string</term>
        ///                 <term>Name of the file, in the fileset, to override.</term>
        ///                 <term>True</term>
        ///             </item>
        ///             <item>
        ///                 <term>id</term>
        ///                 <term>string</term>
        ///                 <term>Unique GUID to assign to the file.</term>
        ///                 <term>True</term>
        ///             </item>
        ///             <item>
        ///                 <term>attr</term>
        ///                 <term>int</term>
        ///                 <term>Integer containing bit flags representing file attributes.<br/> 
        ///         The following table shows the definition of the bit field.
        ///         <list type="table">
        ///             <listheader>
        ///                 <term>Value</term>
        ///                 <description>Description</description>
        ///             </listheader>
        ///             <item>
        ///                 <term>1</term>
        ///                 <description>Read-Only</description>
        ///             </item>
        ///             <item>
        ///                 <term>2</term>
        ///                 <description>Hidden</description>
        ///             </item>
        ///             <item>
        ///                 <term>4</term>
        ///                 <description>System</description>
        ///             </item>
        ///             <item>
        ///                 <term>512</term>
        ///                 <description>The file is vital for the proper operation of the component to which it belongs</description>
        ///             </item>
        ///             <item>
        ///                 <term>1024</term>
        ///                 <description>The file contains a valid checksum. A checksum is required to repair a file that has become corrupted.</description>
        ///             </item>
        ///             <item>
        ///                 <term>4096</term>
        ///                 <description>This bit must only be added by a patch and if the file is being added by the patch.</description>
        ///             </item>
        ///             <item>
        ///                 <term>8192</term>
        ///                 <description>The file's source type is uncompressed. If set, ignore the Word Count Summary Property. 
        ///                 If neither msidbFileAttributesNoncompressed or msidbFileAttributesCompressed are set, the compression 
        ///                 state of the file is specified by the Word Count Summary Property. Do not set both msidbFileAttributesNoncompressed 
        ///                 and msidbFileAttributesCompressed.</description>
        ///             </item>
        ///             <item>
        ///                 <term>16384</term>
        ///                 <description>The file's source type is compressed. If set, ignore the Word Count Summary Property. 
        ///                 If neither msidbFileAttributesNoncompressed or msidbFileAttributesCompressed are set, the compression state of 
        ///                 the file is specified by the Word Count Summary Property. Do not set both msidbFileAttributesNoncompressed and 
        ///                 msidbFileAttributesCompressed.</description>
        ///             </item>
        ///         </list>
        ///                 </term>
        ///                 <term>False</term>
        ///             </item>
        ///             <item>
        ///                 <term>version</term>
        ///                 <term>string</term>
        ///                 <term>This field is the version string for a versioned file. This field is blank for non-versioned files.</term>
        ///                 <term>False</term>
        ///             </item>
        ///             <item>
        ///                 <term>language</term>
        ///                 <term>string</term>
        ///                 <term>A list of decimal language IDs separated by commas.</term>
        ///                 <term>False</term>
        ///             </item>
        ///             <item>
        ///                 <term>checkinterop</term>
        ///                 <term>bool</term>
        ///                 <term>Used to determine if file is an interop file.  If <c>true</c>, extra information will be added in the install
        ///         package to register the interop file. If <c>false</c>, the file will not be not be checked and the extra registration information
        ///         will not be added to the msi.</term>
        ///                 <term>False</term>
        ///             </item>
        ///             <item>
        ///                 <term>installtogac</term>
        ///                 <term>bool</term>
        ///                 <term>If <c>true</c>, and if the file is an assembly, it will be installed to the GAC. If <c>false</c>, the file 
        ///                 will be installed to the directory specified by the component.  Note: If an assembly is specified to 
        ///                 be installed into the GAC, it will not also be installed to the directory specified.</term>
        ///                 <term>False</term>
        ///             </item>
        ///         </list>
        ///     </ul>
        /// <h4>&lt;/forceid&gt;</h4>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Define a sample component structure.</para>
        ///     <code>
        /// &lt;components&gt;
        ///     &lt;component name="C__MainFiles" id="{26AA7144-E683-441D-9843-3C79AEC1C636}" attr="2" directory="TARGETDIR" feature="F__MainFiles" &gt;
        ///         &lt;key file="default.aspx" /&gt;
        ///         &lt;fileset basedir="${install.dir}"&gt;
        ///             &lt;include name="*.*" /&gt;
        ///         &lt;/fileset&gt;
        ///     &lt;/component&gt;
        /// &lt;/components&gt; 
        ///     </code>
        /// </example>
        /// <example>
        ///     <para>Install files to TARGETDIR and assemblies to the GAC (Global Assembly Cache).  Do not install MyOtherAssembly.dll to the GAC, but rather install it with the other files (to TARGETDIR)</para>
        ///     <code>
        /// &lt;components&gt;
        ///     &lt;component name="C__MainFiles" id="{26AA7144-E683-441D-9843-3C79AEC1C636}" attr="2" directory="TARGETDIR" feature="F__MainFiles" installassembliestogac="true" &gt;
        ///         &lt;key file="MyAssemblyName.xml" /&gt;
        ///         &lt;fileset basedir="${install.dir}"&gt;
        ///             &lt;include name="*.*" /&gt;
        ///         &lt;/fileset&gt;
        ///         &lt;forceid file="MyOtherAssembly.dll" id="_4EB7CCB23D394958988ED817DA00B9D1" installtogac="false" /&gt;
        ///     &lt;/component&gt;
        /// &lt;/components&gt; 
        ///     </code>
        /// </example>
        /// <example>
        ///     <para>Assign a registry entry to a specific component.</para>
        ///     <code>
        /// &lt;components&gt;
        ///     &lt;component name="C__RegistryEntry" id="{06C654AA-273D-4E39-885C-3E5225D9F336}" attr="4" directory="TARGETDIR" feature="F__DefaultFeature" &gt;
        ///         &lt;key file="R__822EC365A8754FACBF6C713BFE4E57F0" /&gt;
        ///     &lt;/component&gt;
        /// &lt;/components&gt; 
        /// .
        /// .
        /// .
        /// &lt;registry&gt;
        ///     &lt;key path="SOFTWARE\MyCompany\MyProduct\" root="machine" component="C__RegistryEntry"&gt;
        ///          &lt;value id="R__822EC365A8754FACBF6C713BFE4E57F0" name="MyKeyName" value="MyKeyValue" /&gt;
        ///     &lt;/key&gt;
        /// &lt;/registry&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("components", ProcessXml=false)]
        public SchemaElement[] InstallerComponentsElement {
            get { return null; }
            set {}
        }

        /// <summary>
        /// <para>
        /// Creates custom dialogs that can gather information not handled by 
        /// the default installer template.
        /// </para>
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///            <term>Type</term>
        ///            <term>Description</term>
        ///            <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///            <term>name</term>
        ///            <term>string</term>
        ///            <term>A name used to refer to the dialog.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>hcenter</term>
        ///            <term>int</term>
        ///            <term>Horizontal position of the dialog box. The range is 0 to 100, with 0 at the left edge of the screen and 100 at the right edge.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>vcenter</term>
        ///            <term>int</term>
        ///            <term>Vertical position of the dialog box. The range is 0 to 100, with 0 at the top edge of the screen and 100 at the bottom edge.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>width</term>
        ///            <term>int</term>
        ///            <term>Width of the rectangular boundary of the dialog box. This number must be non-negative.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>height</term>
        ///            <term>int</term>
        ///            <term>Height of the rectangular boundary of the dialog box. This number must be non-negative.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>attr</term>
        ///            <term>int</term>
        ///            <term>A 32-bit word that specifies the attribute flags to be applied to this dialog box. This number must be non-negative.
        ///            <list type="table">
        ///                <listheader>
        ///                    <term>Value</term>
        ///                    <description>Description</description>
        ///                </listheader>
        ///                <item>
        ///                    <term>1</term>
        ///                    <description>Visible</description>
        ///                </item>
        ///                <item>
        ///                    <term>2</term>
        ///                    <description>Modal</description>
        ///                </item>
        ///                <item>
        ///                    <term>4</term>
        ///                    <description>Minimize</description>
        ///                </item>
        ///                <item>
        ///                    <term>8</term>
        ///                    <description>SysModal</description>
        ///                </item>
        ///                <item>
        ///                    <term>16</term>
        ///                    <description>KeepModeless</description>
        ///                </item>
        ///                <item>
        ///                    <term>32</term>
        ///                    <description>TrackDiskSpace</description>
        ///                </item>
        ///                <item>
        ///                    <term>64</term>
        ///                    <description>UseCustomPalette</description>
        ///                </item>
        ///                <item>
        ///                    <term>128</term>
        ///                    <description>RTLRO</description>
        ///                </item>
        ///                <item>
        ///                    <term>256</term>
        ///                    <description>RightAligned</description>
        ///                </item>
        ///                <item>
        ///                    <term>512</term>
        ///                    <description>LeftScroll</description>
        ///                </item>
        ///                <item>
        ///                    <term>896</term>
        ///                    <description>BiDi</description>
        ///                </item>
        ///                <item>
        ///                    <term>65536</term>
        ///                    <description>Error</description>
        ///                </item>
        ///            </list>
        ///            More information here: <a href="http://msdn.microsoft.com/library/en-us/msi/setup/dialog_style_bits.asp">http://msdn.microsoft.com/library/en-us/msi/setup/dialog_style_bits.asp</a>
        ///            </term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>title</term>
        ///            <term>string</term>
        ///            <term>A localizable text string specifying the title to be displayed in the title bar of the dialog box.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>firstcontrol</term>
        ///            <term>string</term>
        ///            <term>An external key to the second column of the Control table. Combining this field with the Dialog field identifies a 
        ///            unique control in the Control table. This defines the control that takes the focus when the dialog box is created. This 
        ///            column is ignored in an Error dialog box. <br/>
        ///            Because static text cannot take the focus, a Text control that describes an Edit, PathEdit, ListView, ComboBox or 
        ///            VolumeSelectCombo control must be made the first control in the dialog box to ensure compatibility with screen readers.
        ///            </term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>defaultcontrol</term>
        ///            <term>string</term>
        ///            <term>An external key to the second column of the Control table. Combining this field with the Dialog field results in 
        ///            a primary key into the Control table that defines the default control. Hitting the Return key is equivalent to clicking 
        ///            on the default control. If this column is left blank, then there is no default control. This column is ignored in the 
        ///            Error dialog box. 
        ///            </term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>cancelcontrol</term>
        ///            <term>string</term>
        ///            <term>An external key to the second column of the Control table. Combining this field with the Dialog field results in 
        ///            a primary key of the Control table that defines the cancel control. Hitting the ESC key or clicking the Close button in 
        ///            the dialog box is equivalent to clicking on the cancel control. This column is ignored in an Error dialog box. <br />
        ///            The cancel control is hidden during rollback or the removal of backed up files. The protected UI handler hides the control 
        ///            upon receiving a INSTALLMESSAGE_COMMONDATA message.
        ///            </term>
        ///            <term>True</term>
        ///        </item>
        ///    </list>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Add a web folder dialog:</para>
        ///     <code>
        /// &lt;dialogs&gt;
        ///     &lt;dialog name="WebFolderDlg" hcenter="50" vcenter="50" width="370" height="270" attr="39" title="[ProductName] [Setup]" firstcontrol="Next" defaultcontrol="Next" cancelcontrol="Cancel" /&gt;
        /// &lt;/dialogs&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("dialogs", ProcessXml=false)]
        public SchemaElement[] InstallerDialogsElement {
            get { return null; }
            set {}
        }

        /// <summary>
        /// <para>
        /// Creates user interface controls displayed on custom dialogs.
        /// </para>
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///            <term>Type</term>
        ///            <term>Description</term>
        ///            <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///            <term>name</term>
        ///            <term>string</term>
        ///            <term>Name of the control. This name must be unique within a dialog box but can be repeated on different dialog boxes.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>dialog</term>
        ///            <term>string</term>
        ///            <term>Refrence to a dialog.  Used to associate the control with the dialog.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>type</term>
        ///            <term>string</term>
        ///            <term>The type of the control.
        ///            <list type="table">
        ///                <listheader>
        ///                    <term>Control name</term>
        ///                    <description>Description</description>
        ///                </listheader>
        ///                <item>
        ///                    <term>Billboard</term>
        ///                    <description>Displays billboards based on progress messages.</description>
        ///                </item>
        ///                <item>
        ///                    <term>Bitmap</term>
        ///                    <description>Displays a static picture of a bitmap.</description>
        ///                </item>
        ///                <item>
        ///                    <term>CheckBox</term>
        ///                    <description>A two-state check box.</description>
        ///                </item>
        ///                <item>
        ///                    <term>ComboBox</term>
        ///                    <description>A drop-down list with an edit field.</description>
        ///                </item>
        ///                <item>
        ///                    <term>DirectoryCombo</term>
        ///                    <description>Select all except the last segment of the path.</description>
        ///                </item>
        ///                <item>
        ///                    <term>DirectoryList</term>
        ///                    <description>Displays folders below the main part of path.</description>
        ///                </item>
        ///                <item>
        ///                    <term>Edit</term>
        ///                    <description>A regular edit field for any string or integer.</description>
        ///                </item>
        ///                <item>
        ///                    <term>GroupBox</term>
        ///                    <description>Displays a rectangle that groups other controls together.</description>
        ///                </item>
        ///                <item>
        ///                    <term>Icon</term>
        ///                    <description>Displays a static picture of an icon.</description>
        ///                </item>
        ///                <item>
        ///                    <term>Line</term>
        ///                    <description>Displays a horizontal line.</description>
        ///                </item>
        ///                <item>
        ///                    <term>ListBox</term>
        ///                    <description>A drop-down list without an edit field.</description>
        ///                </item>
        ///                <item>
        ///                    <term>ListView</term>
        ///                    <description>Displays a column of values with icons for selection.</description>
        ///                </item>
        ///                <item>
        ///                    <term>MaskedEdit</term>
        ///                    <description>An edit field with a mask in the text field.</description>
        ///                </item>
        ///                <item>
        ///                    <term>PathEdit</term>
        ///                    <description>Displays folder name or entire path in an edit field.</description>
        ///                </item>
        ///                <item>
        ///                    <term>ProgressBar</term>
        ///                    <description>Bar graph that changes length as it receives progress messages.</description>
        ///                </item>
        ///                <item>
        ///                    <term>PushButton</term>
        ///                    <description>Displays a basic push button.</description>
        ///                </item>
        ///                <item>
        ///                    <term>RadioButtonGroup</term>
        ///                    <description>A group of radio buttons.</description>
        ///                </item>
        ///                <item>
        ///                    <term>ScrollableText</term>
        ///                    <description>Displays a long string of text.</description>
        ///                </item>
        ///                <item>
        ///                    <term>SelectionTree</term>
        ///                    <description>Displays information from the Feature table and enables the user to change their selection state.</description>
        ///                </item>
        ///                <item>
        ///                    <term>Text</term>
        ///                    <description>Displays static text.</description>
        ///                </item>
        ///                <item>
        ///                    <term>VolumeCostList</term>
        ///                    <description>Displays costing information on different volumes.</description>
        ///                </item>
        ///                <item>
        ///                    <term>VolumeSelectCombo</term>
        ///                    <description>Selects volume from an alphabetical list.</description>
        ///                </item>
        ///            </list>
        ///            More information found here: <a href="http://msdn.microsoft.com/library/en-us/msi/setup/controls.asp">http://msdn.microsoft.com/library/en-us/msi/setup/controls.asp</a></term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>x</term>
        ///            <term>int</term>
        ///            <term>Horizontal coordinate of the upper-left corner of the rectangular boundary of the control. This must be a non-negative number.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>y</term>
        ///            <term>int</term>
        ///            <term>Vertical coordinate of the upper-left corner of the rectangular boundary of the control. This must be a non-negative number.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>width</term>
        ///            <term>int</term>
        ///            <term>Width of the rectangular boundary of the control. This must be a non-negative number.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>height</term>
        ///            <term>int</term>
        ///            <term>Height of the rectangular boundary of the control. This must be a non-negative number.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>attr</term>
        ///            <term>int</term>
        ///            <term>A 32-bit word that specifies the bit flags to be applied to this control. This must be a non-negative number, and the allowed values depend upon the type of control.For a list of all control attributes, and the value to enter in this field, see <a href="http://msdn.microsoft.com/library/en-us/msi/setup/control_attributes.asp">Control Attributes</a>.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>property</term>
        ///            <term>string</term>
        ///            <term>The name of a defined property to be linked to this control. Radio button, list box, and combo box values are tied into a group by being linked to the same property. This column is required for active controls and is ignored by static controls.</term>
        ///            <term>False</term>
        ///        </item>
        ///     <item>
        ///            <term>text</term>
        ///            <term>string</term>
        ///            <term>A localizable string used to set the initial text contained in a control. The string can also contain embedded properties.</term>
        ///            <term>False</term>
        ///        </item>
        ///     <item>
        ///            <term>nextcontrol</term>
        ///            <term>string</term>
        ///            <term>The name of another control on the same dialog box. If the focus in the dialog box is on the control in the Control column, hitting the tab key moves the focus to the control listed here. Therefore this is used to specify the tab order of the controls on the dialog box. The links between the controls must form a closed cycle. Some controls, such as static text controls, can be left out of the cycle. In that case, this field may be left blank. </term>
        ///            <term>False</term>
        ///        </item>
        ///     <item>
        ///            <term>help</term>
        ///            <term>string</term>
        ///            <term>Optional, localizable text strings that are used with the Help button. The string is divided into two parts by a separator character (|). The first part of the string is used as ToolTip text. This text is used by screen readers for controls that contain a picture. The second part of the string is reserved for future use. The separator character is required even if only one of the two kinds of text is present.</term>
        ///            <term>False</term>
        ///        </item>
        ///     <item>
        ///            <term>remove</term>
        ///            <term>bool</term>
        ///            <term>If <c>true</c>, the control is removed.  If <c>false</c>, the control is added.</term>
        ///            <term>False</term>
        ///        </item>
        ///    </list>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Remove the Browse button from the customize dialog and add controls for a web dialog</para>
        ///     <code>
        /// &lt;controls&gt;
        ///     &lt;!-- Remove the Browse button from customize dialog --&gt;
        ///     &lt;control dialog="CustomizeDlg" name="Browse" type="PushButton"
        ///         x="304" y="200" width="56" height="17" attr="3" remove="true" /&gt;
        ///     &lt;control dialog="CustomizeDlg" name="Tree" type="SelectionTree"
        ///         x="25" y="85" width="175" height="95" attr="7" remove="true" /&gt;
        ///
        ///     &lt;!-- Re add the tree control with the proper next control --&gt;
        ///     &lt;control dialog="CustomizeDlg" name="Tree" type="SelectionTree"
        ///         x="25" y="85" width="175" height="95" attr="7" 
        ///         property="_BrowseProperty" text="Tree of selections" nextcontrol="Reset" /&gt;
        ///
        ///     &lt;!-- Adds the controls associated with the webfolder dialog --&gt;
        ///     &lt;control dialog="WebFolderDlg" name="BannerBitmap" type="Bitmap" 
        ///         x="0" y="0" width="374" height="44" attr="1" 
        ///         text="[BannerBitmap]" nextcontrol="VDirLabel" /&gt;
        ///     &lt;control dialog="WebFolderDlg" name="Title" type="Text" 
        ///         x="15" y="6" width="200" height="15" attr="196611" 
        ///         text="[DlgTitleFont]Virtual Directory Information" /&gt;
        ///     &lt;control dialog="WebFolderDlg" name="Description" type="Text" 
        ///         x="25" y="23" width="280" height="15" attr="196611" 
        ///         text="Please enter your virtual directory and port information." /&gt;
        ///     &lt;control dialog="WebFolderDlg" name="BannerLine" type="Line" 
        ///         x="0" y="44" width="374" height="0" attr="1" /&gt;
        ///     &lt;control dialog="WebFolderDlg" name="VDirLabel" type="Text" 
        ///         x="18" y="73" width="348" height="15" attr="3" 
        ///         text="&amp;Virtual directory:" 
        ///         nextcontrol="Edit_VDir" /&gt;            
        ///     &lt;control dialog="WebFolderDlg" name="Edit_VDir" type="Edit" 
        ///         x="18" y="85" width="252" height="18" attr="7" 
        ///         property="TARGETVDIR" 
        ///         text="[TARGETVDIR]" 
        ///         nextcontrol="PortLabel" /&gt;
        ///     &lt;control dialog="WebFolderDlg" name="PortLabel" type="Text" 
        ///         x="18" y="110" width="348" height="15" attr="3" 
        ///         text="&amp;Port:" 
        ///         nextcontrol="Edit_Port" /&gt;            
        ///     &lt;control dialog="WebFolderDlg" name="Edit_Port" type="Edit" 
        ///         x="18" y="122" width="48" height="18" attr="7" 
        ///         property="TARGETPORT" 
        ///         text="[TARGETPORT]" 
        ///         nextcontrol="Back" /&gt;
        ///     &lt;control dialog="WebFolderDlg" name="BottomLine" type="Line" 
        ///         x="0" y="234" width="374" height="0" attr="1" /&gt;                
        ///     &lt;control dialog="WebFolderDlg" name="Back" type="PushButton" 
        ///         x="180" y="243" width="56" height="17" attr="3" 
        ///         text="[ButtonText_Back]" nextcontrol="Next" /&gt;
        ///     &lt;control dialog="WebFolderDlg" name="Next" type="PushButton" 
        ///         x="236" y="243" width="56" height="17" attr="3" 
        ///         text="[ButtonText_Next]" nextcontrol="Cancel" /&gt;
        ///     &lt;control dialog="WebFolderDlg" name="Cancel" type="PushButton" 
        ///         x="304" y="243" width="56" height="17" attr="3" 
        ///         text="[ButtonText_Cancel]" nextcontrol="BannerBitmap" /&gt;
        /// &lt;/controls&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("controls", ProcessXml=false)]
        public SchemaElement[] InstallerControlsElement {
            get { return null; }
            set {}
        }

        /// <summary>
        /// <para>
        /// Used to validate and perform operations as the result of information 
        /// entered by the user into controls on custom dialogs.
        /// </para>
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///            <term>Type</term>
        ///            <term>Description</term>
        ///            <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///            <term>dialog</term>
        ///            <term>string</term>
        ///            <term>Refrence to a dialog.  Used to associate the control with the dialog.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>control</term>
        ///            <term>string</term>
        ///            <term>Refrence to a control.  Maps to a control for the specified dialog.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>action</term>
        ///            <term>string</term>
        ///            <term>The action that is to be taken on the control. The possible actions are shown in the following table.
        ///            <list type="table">
        ///                <listheader>
        ///                    <term>Value</term>
        ///                    <description>Description</description>
        ///                </listheader>
        ///                <item>
        ///                    <term>Default</term>
        ///                    <description>Set control as the default.</description>
        ///                </item>
        ///                <item>
        ///                    <term>Disable</term>
        ///                    <description>Disable the control.</description>
        ///                </item>
        ///                <item>
        ///                    <term>Enable</term>
        ///                    <description>Enable the control.</description>
        ///                </item>
        ///                <item>
        ///                    <term>Hide</term>
        ///                    <description>Hide the control.</description>
        ///                </item>
        ///                <item>
        ///                    <term>Show</term>
        ///                    <description>Display the control.</description>
        ///                </item>
        ///            </list>
        ///            </term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>condition</term>
        ///            <term>string</term>
        ///            <term>A conditional statement that specifies under which conditions the action should be triggered. If this statement does not evaluate to TRUE, the action does not take place. If it is set to 1, the action is always applied. </term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>remove</term>
        ///            <term>bool</term>
        ///            <term>If <c>true</c>, the control condition is removed.  If <c>false</c>, the control condition is added.</term>
        ///            <term>False</term>
        ///        </item>
        ///    </list>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Remove the control condition for the Browse button from the customize dialog and add control conditions for a web dialog</para>
        ///     <code>
        /// &lt;controlconditions&gt;
        ///     &lt;!-- Remove control condition for Browse button on customizeDlg --&gt;
        ///     &lt;controlcondition dialog="CustomizeDlg" control="Browse" action="Hide"
        ///         condition="Installed" remove="true" /&gt;
        ///     &lt;!-- Add control conditions for the web folder dialog --&gt;
        ///     &lt;controlcondition dialog="WebFolderDlg" control="Back" action="Disable"
        ///         condition="ShowUserRegistrationDlg=&quot;&quot;" /&gt;
        ///     &lt;controlcondition dialog="WebFolderDlg" control="Back" action="Enable"
        ///         condition="ShowUserRegistrationDlg&lt;&gt;&quot;&quot;" /&gt;
        /// &lt;/controlconditions&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("controlconditions", ProcessXml=false)]
        public SchemaElement[] InstallerControlConditionsElement {
            get { return null; }
            set {}
        }

        /// <summary>
        /// <para>
        /// Used to route the flow of the installation process as the result of 
        /// events raised by the user interacting with controls on dialogs.
        /// </para>
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///            <term>Type</term>
        ///            <term>Description</term>
        ///            <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///            <term>dialog</term>
        ///            <term>string</term>
        ///            <term>Refrence to a dialog.  Used to associate the control with the dialog.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>control</term>
        ///            <term>string</term>
        ///            <term>Refrence to a control.  Maps to a control for the specified dialog.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>name</term>
        ///            <term>string</term>
        ///            <term>An identifier that specifies the type of event that should take place when the user interacts with the control 
        ///            specified by Dialog_ and Control_. For a list of possible values see <a href="http://msdn.microsoft.com/library/en-us/msi/setup/controlevent_overview.asp">ControlEvent Overview</a>. <br/>
        ///            To set a property with a control, put [Property_Name] in this field and the new value in the argument field. Put { } 
        ///            into the argument field to enter the null value.
        ///            </term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>argument</term>
        ///            <term>string</term>
        ///            <term>A value used as a modifier when triggering a particular event.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>condition</term>
        ///            <term>string</term>
        ///            <term>A conditional statement that determines whether the installer activates the event in the Event column. The installer 
        ///            triggers the event if the conditional statement in the Condition field evaluates to True. Therefore put a 1 in this column 
        ///            to ensure that the installer triggers the event. The installer does not trigger the event if the Condition field contains 
        ///            a statement that evaluates to False. The installer does not trigger an event with a blank in the Condition field unless no 
        ///            other events of the control evaluate to True. If none of the Condition fields for the control named in the Control_ field 
        ///            evaluate to True, the installer triggers the one event having a blank Condition field, and if more than one Condition field 
        ///            is blank it triggers the one event of these with the largest value in the Ordering field.</term>
        ///            <term>False</term>
        ///        </item>
        ///     <item>
        ///            <term>order</term>
        ///            <term>int</term>
        ///            <term>An integer used to order several events tied to the same control. This must be a non-negative number.</term>
        ///            <term>False</term>
        ///        </item>
        ///     <item>
        ///            <term>remove</term>
        ///            <term>bool</term>
        ///            <term>If <c>true</c>, the control condition is removed.  If <c>false</c>, the control condition is added.</term>
        ///            <term>False</term>
        ///        </item>
        ///    </list>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Remove the control events for the Browse button from the customize dialog and add events conditions for a web dialog</para>
        ///     <code>
        /// <!-- Make sure the dialog is put into the execute order -->
        /// &lt;controlevents&gt;
        ///     &lt;!-- Remove the old control events --&gt;
        ///     &lt;controlevent dialog="UserRegistrationDlg" control="Next" name="NewDialog" 
        ///         argument="SetupTypeDlg" condition="ProductID" remove="true" /&gt;
        ///     &lt;controlevent dialog="SetupTypeDlg" control="Back" name="NewDialog" 
        ///         argument="LicenseAgreementDlg" condition="ShowUserRegistrationDlg &lt;&gt; 1" remove="true" /&gt;
        ///     &lt;controlevent dialog="SetupTypeDlg" control="Back" name="NewDialog" 
        ///         argument="UserRegistrationDlg" condition="ShowUserRegistrationDlg = 1" remove="true" /&gt;
        ///     &lt;!-- Remove control events for Browse button on CustomizeDlg --&gt;
        ///     &lt;controlevent dialog="CustomizeDlg" control="Browse" name="SelectionBrowse" 
        ///         argument="BrowseDlg" condition="1" remove="true" /&gt;                
        ///
        ///     &lt;!-- Add new control events for the web dialog --&gt;
        ///     &lt;controlevent dialog="UserRegistrationDlg" control="Next" name="NewDialog" 
        ///         argument="WebFolderDlg" condition="ProductID" /&gt;                        
        ///     &lt;controlevent dialog="SetupTypeDlg" control="Back" name="NewDialog" 
        ///         argument="WebFolderDlg" condition="ShowWebFolderDlg &lt;&gt; 1" /&gt;
        ///     &lt;controlevent dialog="SetupTypeDlg" control="Back" name="NewDialog" 
        ///         argument="WebFolderDlg" condition="ShowWebFolderDlg = 1" /&gt;
        ///     &lt;controlevent dialog="WebFolderDlg" control="Cancel" name="SpawnDialog" 
        ///         argument="CancelDlg" order="0" /&gt;
        ///     &lt;controlevent dialog="WebFolderDlg" control="Back" name="NewDialog" 
        ///         argument="LicenseAgreementDlg" condition="ShowUserRegistrationDlg&lt;&gt;1" 
        ///         order="0" /&gt;
        ///     &lt;controlevent dialog="WebFolderDlg" control="Back" name="NewDialog" 
        ///         argument="UserRegistrationDlg" condition="ShowUserRegistrationDlg=1" 
        ///         order="0" /&gt;
        ///     &lt;!-- Virtual Directory Control Events --&gt;
        ///     &lt;controlevent dialog="WebFolderDlg" control="Next" name="DoAction" 
        ///         argument="WEBCA_CreateURLs" condition="1" order="0" /&gt;
        ///     &lt;controlevent dialog="WebFolderDlg" control="Next" name="DoAction" 
        ///         argument="WEBCA_EvaluateURLsMB" condition="1" order="1" /&gt;
        ///     &lt;controlevent dialog="WebFolderDlg" control="Next" name="SetTargetPath" 
        ///         argument="TARGETDIR" condition="1" order="2" /&gt;
        ///     &lt;controlevent dialog="WebFolderDlg" control="Next" name="NewDialog" 
        ///         argument="SetupTypeDlg" condition="1" order="3" /&gt;
        /// &lt;/controlevents&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("controlevents", ProcessXml=false)]
        public SchemaElement[] InstallerControlEventsElement {
            get { return null; }
            set {}
        }
                
        /// <summary>
        /// <para>
        /// Makes modifications to the Windows Registry of the target computer 
        /// at runtime.
        /// </para>
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///            <term>Type</term>
        ///            <term>Description</term>
        ///            <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///            <term>component</term>
        ///            <term>string</term>
        ///            <term>Refrence to a component.  The component that controls the installation of the registry value.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>root</term>
        ///            <term>msi:MSIRegistryKeyRoot</term>
        ///            <term>Valid input: 
        ///                <list type="bullet">
        ///                       <item><c>dependent</c> - If this is a per-user installation, the registry value is written under HKEY_CURRENT_USER.  If this is a per-machine installation, the registry value is written under HKEY_LOCAL_MACHINE. Note that a per-machine installation is specified by setting the ALLUSERS property to 1.</item>
        ///                    <item><c>machine</c> represents HKEY_LOCAL_MACHINE</item>
        ///                    <item><c>classes</c> represents HKEY_CLASSES_ROOT</item>
        ///                    <item><c>user</c> represents HKEY_CURRENT_USER</item>
        ///                    <item><c>users</c> represents HKEY_USERS</item>
        ///                </list>
        ///            </term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>path</term>
        ///            <term>string</term>
        ///            <term>Registry key.</term>
        ///            <term>True</term>
        ///        </item>
        ///    </list>
        ///    <h3>Nested Elements:</h3>
        ///    <h4>&lt;value&gt;</h4>
        ///        <ul>
        ///            Specifies the registry value to add to the target machine.
        ///            <list type="table">
        ///                <listheader>
        ///                    <term>Attribute</term>
        ///                    <term>Type</term>
        ///                    <term>Description</term>
        ///                    <term>Required</term>
        ///                </listheader>
        ///                <item>
        ///                    <term>name</term>
        ///                    <term>string</term>
        ///                    <term>The registry value name (localizable). If this is Null, then the data entered into the Value column are 
        ///                    written to the default registry key. <br/>
        ///                    If the Value column is Null, then the strings shown in the following table in the Name column have special 
        ///                    significance.
        ///                    <list type="table">
        ///                        <listheader>
        ///                            <term>String</term>
        ///                            <description>Description</description>
        ///                        </listheader>
        ///                        <item>
        ///                            <term>+</term>
        ///                            <description>The key is to be created, if absent, when the component is installed.</description>
        ///                        </item>
        ///                        <item>
        ///                            <term>-</term>
        ///                            <description>The key is to be deleted, if present, with all of its values and subkeys, when the component is uninstalled.</description>
        ///                        </item>
        ///                        <item>
        ///                            <term>*</term>
        ///                            <description>The key is to be created, if absent, when the component is installed. Additionally, the key is to be deleted, if present, with all of its values and subkeys, when the component is uninstalled.</description>
        ///                        </item>
        ///                    </list>
        ///                    </term>
        ///                    <term>False</term>
        ///                </item>
        ///                <item>
        ///                    <term>value</term>
        ///                    <term>string</term>
        ///                    <term>The localizable registry value. The field is <a href="http://msdn.microsoft.com/library/en-us/msi/setup/formatted.asp">Formatted</a>. If the value is attached to one of the following prefixes (i.e. #%value) then the value is interpreted as described in the table. Note that each prefix begins with a number sign (#). If the value begins with two or more consecutive number signs (#), the first # is ignored and value is interpreted and stored as a string. 
        ///                    <list type="table">
        ///                        <listheader>
        ///                            <term>Prefix</term>
        ///                            <description>Description</description>
        ///                        </listheader>
        ///                        <item>
        ///                            <term>#x</term>
        ///                            <description>The value is interpreted and stored as a hexadecimal value (REG_BINARY).</description>
        ///                        </item>
        ///                        <item>
        ///                            <term>#%</term>
        ///                            <description>The value is interpreted and stored as an expandable string (REG_EXPAND_SZ).</description>
        ///                        </item>
        ///                        <item>
        ///                            <term>#</term>
        ///                            <description>The value is interpreted and stored as an integer (REG_DWORD).</description>
        ///                        </item>
        ///                    </list>
        ///                    <list type="bullet">
        ///                        <item>If the value contains the sequence tilde [~], then the value is interpreted as a Null-delimited list of strings (REG_MULTI_SZ). For example, to specify a list containing the three strings a, b and c, use "a[~]b[~]c." </item>
        ///                        <item>The sequence [~] within the value separates the individual strings and is interpreted and stored as a Null character.</item>
        ///                        <item>If a [~] precedes the string list, the strings are to be appended to any existing registry value strings. If an appending string already occurs in the registry value, the original occurrence of the string is removed.</item>
        ///                        <item>If a [~] follows the end of the string list, the strings are to be prepended to any existing registry value strings. If a prepending string already occurs in the registry value, the original occurrence of the string is removed.</item>
        ///                        <item>If a [~] is at both the beginning and the end or at neither the beginning nor the end of the string list, the strings are to replace any existing registry value strings.</item>
        ///                        <item>Otherwise, the value is interpreted and stored as a string (REG_SZ). </item>
        ///                    </list>
        ///                    </term>
        ///                    <term>False</term>
        ///                </item>
        ///                <item>
        ///                    <term>dword</term>
        ///                    <term>string</term>
        ///                    <term>A dword value to input, if the value attribute is null.  This removes the requirement of adding "#" before the value.</term>
        ///                    <term>False</term>
        ///                </item>
        ///                <item>
        ///                    <term>id</term>
        ///                    <term>string</term>
        ///                    <term>Primary key used to identify a registry record.</term>
        ///                    <term>False</term>
        ///                </item>
        ///            </list>
        ///        </ul>
        ///    <h4>&lt;/value&gt;</h4>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Add the a couple registry entries on the target machine.</para>
        ///     <code>
        /// &lt;registry&gt;
        ///     &lt;key component="C__MainFiles" root="machine" path="SOFTWARE\ACME\My Product\" &gt;
        ///         &lt;value name="ProductVersion" value="1.0.0" /&gt;
        ///         &lt;value name="ProductDir" value="[TARGETDIR]" /&gt;
        ///         &lt;value name="VirtualDir" value="[TARGETVDIR]" /&gt;
        ///     &lt;/key&gt;
        /// &lt;/registry&gt;
        ///     </code>
        /// </example>
        /// <example>
        ///     <para>Add a default key value to the specified registry key path</para>
        ///     <code>
        /// &lt;registry&gt;
        ///     &lt;key component="C__MainFiles" root="machine" path="SOFTWARE\ACME\My Product\" &gt;
        ///         &lt;value value="1.0.0" /&gt;
        ///     &lt;/key&gt;
        /// &lt;/registry&gt;
        ///     </code>
        /// </example>
        /// <example>
        ///     <para>Another way to add a default key value to the specified registry key path</para>
        ///     <code>
        /// &lt;registry&gt;
        ///     &lt;key component="C__MainFiles" root="machine" path="SOFTWARE\ACME\My Product\" &gt;
        ///         &lt;value name="" value="1.0.0" /&gt;
        ///     &lt;/key&gt;
        /// &lt;/registry&gt;
        ///     </code>
        /// </example>
        /// <example>
        ///     <para>Specify hexadecimal value (REG_BINARY) for the default key</para>
        ///     <code>
        /// &lt;registry&gt;
        ///     &lt;key component="C__MainFiles" root="machine" path="SOFTWARE\ACME\My Product\" &gt;
        ///         &lt;value&gt;
        /// 1a,81,0a,03,01,00,06,00,00,00,d3,15,fd,00,01,00,00,00,00,00,01,
        /// 00,00,00,00,00,00,00,00,00,00,00,b0,90,ce,09,01,00,00,00,00,00,ff,ff,ff,00,
        /// 00,00,00,00,00,00,00,00,6d,7a,0a,03,01,00,00,00,00,00,00,00,38,40,00,00,00,
        /// 00,00,00,00,00,00,00,00,00,90,01,00,00,00,00,00,01,00,00,00,00,0f,00,00,00,
        /// f0,ff,ff,ff,54,69,6d,65,73,20,4e,65,77,20,52,6f,6d,61,6e,f4,6f,d4,08,02,00
        ///         &lt;/value&gt;
        ///     &lt;/key&gt;
        /// &lt;/registry&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("registry", ProcessXml=false)]
        public SchemaElement[] InstallerRegistryElement {
            get { return null; }
            set {}
        }
        
        /// <summary>
        /// <para>
        /// Stores icons to be used with shortcuts, file extensions, CLSIDs or 
        /// similar uses.
        /// </para>
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///            <term>Type</term>
        ///            <term>Description</term>
        ///            <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///            <term>name</term>
        ///            <term>string</term>
        ///            <term>Name of the icon file.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>value</term>
        ///            <term>string</term>
        ///            <term>The binary icon data in PE (.dll or .exe) or icon (.ico) format.</term>
        ///            <term>True</term>
        ///        </item>
        ///    </list>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Add a compiled help icon to the msi database; To be used with a shortcut.</para>
        ///     <code>
        /// &lt;icons&gt;
        ///     &lt;icon name="CHMICON" value="${resource.dir}\chm.ico" /&gt;
        /// &lt;/icons&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("icons", ProcessXml=false)]
        public SchemaElement[] InstallerIconsElement {
            get { return null; }
            set {}
        }

        /// <summary>
        /// <para>
        /// Creates shortcuts on the target computer.
        /// </para>
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///            <term>Type</term>
        ///            <term>Description</term>
        ///            <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///            <term>name</term>
        ///            <term>string</term>
        ///            <term>Unique name identifying the shortcut.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>directory</term>
        ///            <term>string</term>
        ///            <term>Reference to a directory.  The location of where the shortcut should be created.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>filename</term>
        ///            <term>string</term>
        ///            <term>The localizable name of the shortcut to be created.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>component</term>
        ///            <term>string</term>
        ///            <term>Reference to a component.  The installer uses the installation state of this specified component to determine whether the shortcut is created or deleted. This component must have a valid key path for the shortcut to be installed. If the Target column contains the name of a feature, the file launched by the shortcut is the key file of the component listed in this column. </term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>target</term>
        ///            <term>string</term>
        ///            <term>The shortcut target.  The installer evaluates this field as a Formatted string. The field should contains a property identifier enclosed by square brackets ([ ]), that is expanded into the file or a folder pointed to by the shortcut.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>arguments</term>
        ///            <term>string</term>
        ///            <term>The command-line arguments for the shortcut. <br/>Note that the resolution of properties in the Arguments field is limited. A property formatted as [Property] in this field can only be resolved if the property already has the intended value when the component owning the shortcut is installed. For example, for the argument "[#MyDoc.doc]" to resolve to the correct value, the same process must be installing the file MyDoc.doc and the component that owns the shortcut.
        ///            </term>
        ///            <term>False</term>
        ///        </item>
        ///     <item>
        ///            <term>hotkey</term>
        ///            <term>string</term>
        ///            <term>The hotkey for the shortcut. The low-order byte contains the virtual-key code for the key, and the high-order byte contains modifier flags. This must be a non-negative number. Authors of installation packages are generally recommend not to set this option, because this can add duplicate hotkeys to a users desktop. In addition, the practice of assigning hotkeys to shortcuts can be problematic for users using hotkeys for accessibility.</term>
        ///            <term>False</term>
        ///        </item>
        ///     <item>
        ///            <term>icon</term>
        ///            <term>string</term>
        ///            <term>Reference to an icon.  </term>
        ///            <term>False</term>
        ///        </item>
        ///     <item>
        ///            <term>iconindex</term>
        ///            <term>int</term>
        ///            <term>The icon index for the shortcut. This must be a non-negative number.</term>
        ///            <term>False</term>
        ///        </item>
        ///     <item>
        ///            <term>showcmd</term>
        ///            <term>int</term>
        ///            <term>The Show command for the application window. <br/>The following values may be used. The values are as defined for the Windows API function ShowWindow.
        ///            <list type="table">
        ///                <listheader>
        ///                    <term>Value</term>
        ///                    <description>Description</description>
        ///                </listheader>
        ///                <item>
        ///                    <term>1</term>
        ///                    <description>SW_SHOWNORMAL</description>
        ///                </item>
        ///                <item>
        ///                    <term>3</term>
        ///                    <description>SW_SHOWMAXIMIZED</description>
        ///                </item>
        ///                <item>
        ///                    <term>7</term>
        ///                    <description>SW_SHOWMINNOACTIVE</description>
        ///                </item>
        ///            </list>
        ///            </term>
        ///            <term>False</term>
        ///        </item>
        ///     <item>
        ///            <term>wkdir</term>
        ///            <term>string</term>
        ///            <term>The name of the property that has the path of the working directory for the shortcut.</term>
        ///            <term>False</term>
        ///        </item>
        ///    </list>
        ///    <h3>Nested Elements:</h3>
        ///    <h4>&lt;description&gt;</h4>
        ///    <ul>
        ///    The localizable description of the shortcut. 
        ///    </ul>
        ///    <h4>&lt;/description&gt;</h4>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Add a compiled help icon to the msi database; To be used with a shortcut.</para>
        ///     <code>
        /// &lt;shortcuts&gt;
        ///     &lt;shortcut name="HelpFiles" directory="D__PROGRAMMENU_ACME_MYPRODUCT" filename="Help File" component="C__MainFiles" target="[$C__MainFiles]\Help.chm" icon="CHMICON" iconindex="0" showcmd="3" &gt;
        ///         &lt;description&gt;My Product help documentation&lt;/description&gt;
        ///     &lt;/shortcut&gt;
        /// &lt;/shortcuts&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("shortcuts", ProcessXml=false)]
        public SchemaElement[] InstallerShortcutsElement {
            get { return null; }
            set {}
        }

        /// <summary>
        /// <para>
        /// Stores the binary data for items such as bitmaps, animations, and 
        /// icons. The binary table is also used to store data for custom 
        /// actions.
        /// </para>
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///            <term>Type</term>
        ///            <term>Description</term>
        ///            <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///            <term>name</term>
        ///            <term>string</term>
        ///            <term>A unique key that identifies the particular binary data. If the binary data is for a control, the key appears in the Text column of the associated control in the Control table. This key must be unique among all controls requiring binary data.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>value</term>
        ///            <term>string</term>
        ///            <term>The binary file to add.</term>
        ///            <term>True</term>
        ///        </item>
        ///    </list>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Add the custom action dll to create/modify virtual directories</para>
        ///     <code>
        /// &lt;binaries&gt;
        ///     &lt;binary name="MSVBDPCADLL" value="${resource.dir}\MSVBDPCA.DLL" /&gt;
        /// &lt;/binaries&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("binaries", ProcessXml=false)]
        public SchemaElement[] InstallerBinariesElement {
            get { return null; }
            set {}
        }

        /// <summary>
        /// <para>
        /// Used to configure executables that may be run during steps in the 
        /// installation process to do things outside the bounds of MSI 
        /// technology's feature set. This is the main spot you can extend MSI 
        /// technology to perform custom processes via compiled code.
        /// </para>
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///            <term>Type</term>
        ///            <term>Description</term>
        ///            <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///            <term>action</term>
        ///            <term>string</term>
        ///            <term>Name of the action. The action normally appears in a sequence table unless it is called by another custom action. If the name matches any built-in action, then the custom action is never called. </term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>type</term>
        ///            <term>string</term>
        ///            <term>A field of flag bits specifying the basic type of custom action and options. See <a href="http://msdn.microsoft.com/library/en-us/msi/setup/summary_list_of_all_custom_action_types.asp">Summary List of All Custom Action Types</a> for a list of the basic types. See <a href="http://msdn.microsoft.com/library/en-us/msi/setup/custom_action_return_processing_options.asp">Custom Action Return Processing Options</a>, <a href="http://msdn.microsoft.com/library/en-us/msi/setup/custom_action_execution_scheduling_options.asp">Custom Action Execution Scheduling Options</a>, <a href="http://msdn.microsoft.com/library/en-us/msi/setup/custom_action_hidden_target_option.asp">Custom Action Hidden Target Option</a>, and <a href="http://msdn.microsoft.com/library/en-us/msi/setup/custom_action_in_script_execution_options.asp">Custom Action In-Script Execution Options</a>. </term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>source</term>
        ///            <term>string</term>
        ///            <term>A property name or external key into another table. For a discussion of the possible custom action sources, see <a href="http://msdn.microsoft.com/library/en-us/msi/setup/custom_action_sources.asp">Custom Action Sources</a> and the <a href="http://msdn.microsoft.com/library/en-us/msi/setup/summary_list_of_all_custom_action_types.asp">Summary List of All Custom Action Types</a>. For example, the Source column may contain an external key into the first column of one of the following tables containing the source of the custom action code. <br/>
        ///            Directory table for calling existing executables.<br/>
        ///            File table for calling executables and DLLs that have just been installed.<br/>
        ///            Binary table for calling executables, DLLs, and data stored in the database.<br/>
        ///            Property table for calling executables whose paths are held by a property.
        ///            </term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>target</term>
        ///            <term>string</term>
        ///            <term>An execution parameter that depends on the basic type of custom action. See the Summary List of All Custom Action Types for a description of what should be entered in this field for each type of custom action. For example, this field may contain the following depending on the custom action. 
        ///            <list type="table">
        ///                <listheader>
        ///                    <term>Target</term>
        ///                    <term>Custom Action</term>
        ///                </listheader>
        ///                <item>
        ///                    <term>Entry point (required)</term>
        ///                    <term>Calling a DLL.</term>
        ///                </item>
        ///                <item>
        ///                    <term>Executable name with arguments (required)</term>
        ///                    <term>Calling an existing executable.</term>
        ///                </item>
        ///                <item>
        ///                    <term>Command line arguments (optional)</term>
        ///                    <term>Calling an executable just installed.</term>
        ///                </item>
        ///                <item>
        ///                    <term>Target file name (required)</term>
        ///                    <term>Creating a file from custom data.</term>
        ///                </item>
        ///                <item>
        ///                    <term>Null</term>
        ///                    <term>Executing script code.</term>
        ///                </item>
        ///            </list>
        ///            </term>
        ///            <term>True</term>
        ///        </item>
        ///    </list>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Add some custom actions related to the virtual directory dialog and custom action.</para>
        ///     <code>
        /// &lt;customactions&gt;
        ///     &lt;!-- Custom actions creating entry points into the custom action dll specified in the binary table --&gt;
        ///     &lt;customaction action="WEBCA_GatherWebFolderProperties" type="1" source="MSVBDPCADLL" target="GatherWebFolderProperties" /&gt;
        ///     &lt;customaction action="WEBCA_ApplyWebFolderProperties" type="1537" source="MSVBDPCADLL" target="ApplyWebFolderProperties" /&gt;
        ///     &lt;customaction action="WEBCA_RollbackApplyWebFolderProperties" type="1281" source="MSVBDPCADLL" target="RollbackApplyWebFolderProperties" /&gt;
        ///     &lt;customaction action="WEBCA_CreateURLs" type="1" source="MSVBDPCADLL" target="CreateURLs" /&gt;
        ///     &lt;customaction action="WEBCA_EvaluateURLs" type="1" source="MSVBDPCADLL" target="EvaluateURLs" /&gt;
        ///     &lt;customaction action="WEBCA_EvaluateURLsNoFail" type="1" source="MSVBDPCADLL" target="EvaluateURLsNoFail" /&gt;
        ///     &lt;customaction action="WEBCA_EvaluateURLsMB" type="1" source="MSVBDPCADLL" target="EvaluateURLsMB" /&gt;
        ///     &lt;customaction action="WEBCA_CreateAppRoots" type="1" source="MSVBDPCADLL" target="CreateAppRoots" /&gt;
        ///     
        ///     &lt;!-- Custom actions to set default control values in the webfolder dialog --&gt;
        ///     &lt;customaction action="WEBCA_TARGETVDIR" type="307" source="TARGETVDIR" target="Default VDir" /&gt;
        ///     &lt;customaction action="WEBCA_TARGETPORT" type="307" source="TARGETPORT" target="80" /&gt;
        /// &lt;/customactions&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("customactions", ProcessXml=false)]
        public SchemaElement[] InstallerCustomActionsElement {
            get { return null; }
            set {}
        }
        
        /// <summary>
        /// <para>
        /// Used to modify the sequence of tasks/events that execute during the 
        /// overall installation process.
        /// </para>
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///            <term>Type</term>
        ///            <term>Description</term>
        ///            <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///            <term>type</term>
        ///            <term>msi:MSISequenceTable</term>
        ///            <term>Valid inputs:
        ///                <list type="bullet">
        ///                    <item><c>installexecute</c> represents <a href="http://msdn.microsoft.com/library/en-us/msi/setup/installexecutesequence_table.asp">InstallExecuteSequence Table</a>.</item>
        ///                    <item><c>installui</c> represents <a href="http://msdn.microsoft.com/library/en-us/msi/setup/installuisequence_table.asp">InstallUISequence Table</a></item>
        ///                    <item><c>adminexecute</c> represents <a href="http://msdn.microsoft.com/library/en-us/msi/setup/adminexecutesequence_table.asp">AdminExecuteSequence Table</a></item>
        ///                    <item><c>adminui</c> represents <a href="http://msdn.microsoft.com/library/en-us/msi/setup/adminuisequence_table.asp">AdminUISequence Table</a></item>
        ///                    <item><c>advtexecute</c> represents <a href="http://msdn.microsoft.com/library/en-us/msi/setup/advtuisequence_table.asp">AdvtUISequence Table</a></item>
        ///                </list>
        ///            </term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>action</term>
        ///            <term>string</term>
        ///            <term>Name of the action to execute. This is either a built-in action or a custom action.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>value</term>
        ///            <term>int</term>
        ///            <term>Number that determines the sequence position in which this action is to be executed. <br/>
        ///            A positive value represents the sequence position. A Null value indicates that the action is not executed. The following 
        ///            negative values indicate that this action is to be executed if the installer returns the associated termination flag. No 
        ///            more than one action may have a negative value entered in the Sequence field.
        ///            <list type="table">
        ///                <listheader>
        ///                    <term>Value</term>
        ///                    <description>Description</description>
        ///                </listheader>
        ///                <item>
        ///                    <term>-1</term>
        ///                    <description>Successful completion.</description>
        ///                </item>
        ///                <item>
        ///                    <term>-2</term>
        ///                    <description>User terminates install.</description>
        ///                </item>
        ///                <item>
        ///                    <term>-3</term>
        ///                    <description>Fatal exit terminates.</description>
        ///                </item>
        ///                <item>
        ///                    <term>-4</term>
        ///                    <description>Install is suspended.</description>
        ///                </item>
        ///            </list>
        ///            </term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>condition</term>
        ///            <term>string</term>
        ///            <term>This field contains a conditional expression. If the expression evaluates to False, then the action is skipped. If the expression syntax is invalid, then the sequence terminates, returning iesBadActionData. </term>
        ///            <term>False</term>
        ///        </item>
        ///    </list>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Add the sequences to support virtual directories</para>
        ///     <code>
        /// &lt;sequences&gt;
        ///     &lt;sequence type="installexecute" action="WEBCA_TARGETVDIR" value="750" condition="TARGETVDIR=&amp;quot;&amp;quot;" /&gt;
        ///     &lt;sequence type="installexecute" action="WEBCA_TARGETPORT" value="750" condition="TARGETPORT=&amp;quot;&amp;quot;" /&gt;                                    
        ///     &lt;sequence type="installexecute" action="WEBCA_CreateURLs" value="752" condition="NOT Installed" /&gt;
        ///     &lt;sequence type="installexecute" action="WEBCA_EvaluateURLs" value="753" condition="NOT Installed" /&gt;
        ///     &lt;sequence type="installexecute" action="WEBCA_GatherWebFolderProperties" value="3701" condition="NOT Installed" /&gt;
        ///     &lt;sequence type="installexecute" action="WEBCA_ApplyWebFolderProperties" value="3701" condition="NOT Installed" /&gt;
        ///     &lt;sequence type="installexecute" action="WEBCA_RollbackApplyWebFolderProperties" value="3701" condition="NOT Installed" /&gt;
        ///     &lt;sequence type="installexecute" action="WEBCA_CreateAppRoots" value="3701" condition="NOT Installed" /&gt;
        ///     &lt;sequence type="installui" action="WEBCA_TARGETVDIR" value="750" condition="TARGETVDIR=&amp;quot;&amp;quot;" /&gt;
        ///     &lt;sequence type="installui" action="WEBCA_TARGETPORT" value="750" condition="TARGETPORT=&amp;quot;&amp;quot;" /&gt;
        ///     &lt;sequence type="installui" action="WEBCA_CreateURLs" value="752" condition="NOT Installed" /&gt;
        ///     &lt;sequence type="installui" action="WEBCA_EvaluateURLsNoFail" value="753" condition="NOT Installed" /&gt;
        ///     &lt;sequence type="adminexecute" action="WEBCA_TARGETVDIR" value="750" condition="TARGETVDIR=&amp;quot;&amp;quot;" /&gt;
        ///     &lt;sequence type="adminexecute" action="WEBCA_TARGETPORT" value="750" condition="TARGETPORT=&amp;quot;&amp;quot;" /&gt;
        ///     &lt;sequence type="adminexecute" action="WEBCA_CreateURLs" value="752" condition="NOT Installed" /&gt;
        ///     &lt;sequence type="adminexecute" action="WEBCA_EvaluateURLs" value="753" condition="NOT Installed" /&gt;
        ///     &lt;sequence type="adminui" action="WEBCA_TARGETVDIR" value="750" condition="TARGETVDIR=&amp;quot;&amp;quot;" /&gt;
        ///     &lt;sequence type="adminui" action="WEBCA_TARGETPORT" value="750" condition="TARGETPORT=&amp;quot;&amp;quot;" /&gt;
        ///     &lt;sequence type="adminui" action="WEBCA_CreateURLs" value="752" condition="NOT Installed" /&gt;
        ///     &lt;sequence type="adminui" action="WEBCA_EvaluateURLsNoFail" value="753" condition="NOT Installed" /&gt;                        
        /// &lt;/sequences&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("sequences", ProcessXml=false)]
        public SchemaElement[] InstallerSequencesElement {
            get { return null; }
            set {}
        }
        
        /// <summary>
        /// <para>
        /// Creates text to be displayed in a progress dialog box and written 
        /// to the log for actions that take a long time to execute. The text 
        /// displayed consists of the action description and optionally formatted 
        /// data from the action.  The entries in the ActionText table typically 
        /// refer to actions in sequence tables.
        /// </para>
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///            <term>Type</term>
        ///            <term>Description</term>
        ///            <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///            <term>name</term>
        ///            <term>string</term>
        ///            <term>Unique name identifying the action.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>template</term>
        ///            <term>string</term>
        ///            <term>A localized format template is used to format action data records for display during action execution. If no template is supplied, then the action data will not be displayed.</term>
        ///            <term>False</term>
        ///        </item>
        ///    </list>
        ///    <h3>Nested Elements:</h3>
        ///    <h4>&lt;description&gt;</h4>
        ///    <ul>
        ///    Localized description displayed in the progress dialog box or written to the log when the action is executing. 
        ///    </ul>
        ///    <h4>&lt;/description&gt;</h4>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Add the related action text for the web folder actions.</para>
        ///     <code>
        /// &lt;actiontext&gt;
        ///     &lt;action name="WEBCA_GatherWebFolderProperties" &gt;
        ///         &lt;description&gt;Gathering web folder properties&lt;/description&gt;
        ///     &lt;/action&gt;
        ///     &lt;action name="WEBCA_ApplyWebFolderProperties" &gt;
        ///         &lt;description&gt;Applying web folder properties&lt;/description&gt;
        ///     &lt;/action&gt;
        ///     &lt;action name="WEBCA_RollbackApplyWebFolderProperties" &gt;
        ///         &lt;description&gt;Removing web folder properties&lt;/description&gt;
        ///     &lt;/action&gt;
        ///     &lt;action name="WEBCA_CreateURLs" &gt;
        ///         &lt;description&gt;Creating URLs&lt;/description&gt;
        ///     &lt;/action&gt;
        ///     &lt;action name="WEBCA_EvaluateURLs" &gt;
        ///         &lt;description&gt;Evaluating URLs&lt;/description&gt;
        ///     &lt;/action&gt;
        ///     &lt;action name="WEBCA_EvaluateURLsNoFail" &gt;
        ///         &lt;description&gt;Evaluating URLs and do not fail if URL is invalid&lt;/description&gt;
        ///     &lt;/action&gt;
        ///     &lt;action name="WEBCA_EvaluateURLsMB" &gt;
        ///         &lt;description&gt;Evaluating URLs&lt;/description&gt;
        ///     &lt;/action&gt;
        ///     &lt;action name="WEBCA_CreateAppRoots" &gt;
        ///         &lt;description&gt;Creating application roots&lt;/description&gt;
        ///     &lt;/action&gt;
        ///     &lt;action name="WEBCA_TARGETVDIR" &gt;
        ///         &lt;description&gt;Set TARGETVDIR property to the specified virtual dir&lt;/description&gt;
        ///     &lt;/action&gt;
        ///     &lt;action name="WEBCA_TARGETPORT" &gt;
        ///         &lt;description&gt;Set TARGETPORT property to the specified virtual dir port&lt;/description&gt;
        ///     &lt;/action&gt;
        /// &lt;/actiontext&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("actiontext", ProcessXml=false)]
        public SchemaElement[] InstallerActionTextElement {
            get { return null; }
            set {}
        }
        
        /// <summary>
        /// <para>
        /// Adds Verbs and a handler for the specified file type.
        /// </para>
        /// <note>This not an officially Microsoft supported table.</note>
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///            <term>Type</term>
        ///            <term>Description</term>
        ///            <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///            <term>directory</term>
        ///            <term>string</term>
        ///            <term>Refrence to a directory.  The directory to add the specific verb/handler to IIS for the specified file type.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>extension</term>
        ///            <term>string</term>
        ///            <term>File name extension to specifically handle</term>
        ///            <term>False</term>
        ///        </item>
        ///     <item>
        ///            <term>exepath</term>
        ///            <term>string</term>
        ///            <term>Path to the Internet Server API (ISAPI) or Common Gateway Interface (CGI) program to run to process a request.</term>
        ///            <term>False</term>
        ///        </item>
        ///     <item>
        ///            <term>verbs</term>
        ///            <term>string</term>
        ///            <term>Internet Information Services verbs that are allowed for the executable file.  Only verbs entered in this field will be allowed.</term>
        ///            <term>False</term>
        ///        </item>
        ///    </list>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Add the aspx app mapping</para>
        ///     <code>
        /// &lt;appmappings&gt;
        ///     &lt;appmapping directory="D__ACME_MyProduct" extension=".aspx" exepath="[DOTNETFOLDER]aspnet_isapi.dll" verbs="GET,HEAD,POST,DEBUG" /&gt;
        /// &lt;/appmappings&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("appmappings", ProcessXml=false)]
        public SchemaElement[] InstallerAppMappingsElement {
            get { return null; }
            set {}
        }
        
        /// <summary>
        /// <para>
        /// Determines the local path equivalent for a url and stores this 
        /// information in a property.
        /// </para>
        /// <note>This not an officially Microsoft supported table.</note>
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///            <term>Type</term>
        ///            <term>Description</term>
        ///            <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///            <term>name</term>
        ///            <term>string</term>
        ///            <term>The name of the URLProperty to convert</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>property</term>
        ///            <term>string</term>
        ///            <term>The name of the property to store the directory information.</term>
        ///            <term>True</term>
        ///        </item>
        ///    </list>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Convert the TARGETURL property to a directory and store that information in TARGETDIR</para>
        ///     <code>
        /// &lt;urlproperties&gt;
        ///     &lt;urlproperty name="TARGETURL" property="TARGETDIR" /&gt;
        /// &lt;/urlproperties&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("urlproperties", ProcessXml=false)]
        public SchemaElement[] InstallerUrlPropertiesElement {
            get { return null; }
            set {}
        }

        /// <summary>
        /// <para>
        /// Creates a URLProperty representing the virtual directory and port.
        /// </para>
        /// <note>This not an officially Microsoft supported table.</note>
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///            <term>Type</term>
        ///            <term>Description</term>
        ///            <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///            <term>name</term>
        ///            <term>string</term>
        ///            <term>Property containing the virtual directory</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>portproperty</term>
        ///            <term>string</term>
        ///            <term>Property containing the network port number to use.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>urlproperty</term>
        ///            <term>string</term>
        ///            <term>URLProperty to store the url in</term>
        ///            <term>True</term>
        ///        </item>
        ///    </list>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Convert the virtual directory and port to a url and store the value in a property.</para>
        ///     <code>
        /// &lt;vdirproperties&gt;
        ///     &lt;vdirproperty name="TARGETVDIR" portproperty="TARGETPORT" urlproperty="TARGETURL" /&gt;
        /// &lt;/vdirproperties&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("vdirproperties", ProcessXml=false)]
        public SchemaElement[] InstallerVDirPropertiesElement {
            get { return null; }
            set {}
        }
        
        /// <summary>
        /// <para>
        /// Create a Web application definition and marks it as running in-process 
        /// or out-of-process. If an application already exists at the specified 
        /// path, you can use this method to reconfigure the application from 
        /// in-process to out-of-process, or the reverse.
        /// </para>
        /// <note>This not an officially Microsoft supported table.</note>
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///            <term>Type</term>
        ///            <term>Description</term>
        ///            <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///            <term>component</term>
        ///            <term>string</term>
        ///            <term>Reference to a component.  Determines when the approot will be created.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>urlproperty</term>
        ///            <term>string</term>
        ///            <term>URLProperty with stored url</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>inprocflag</term>
        ///            <term>int</term>
        ///            <term>Specifies whether the application being created is to run in-process (0), out-of-process (1), or in a pooled process (2). If the application already exists and is running, changing the value of this flag will cause the application definition to be deleted and a new application created to run in the specified process space.</term>
        ///            <term>True</term>
        ///        </item>
        ///    </list>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Convert the virtual directory and port to a url and store the value in a property.</para>
        ///     <code>
        /// &lt;approots&gt;
        ///     &lt;approot component="C__MainFiles" urlproperty="TARGETURL" inprocflag="2" /&gt;
        /// &lt;/approots&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("approots", ProcessXml=false)]
        public SchemaElement[] InstallerAppRootsElement {
            get { return null; }
            set {}
        }
        
        /// <summary>
        /// <para>
        /// Specifies directory security in IIS.  Can also configure the default 
        /// documents supported by each directory.
        /// </para>
        /// <note>This not an officially Microsoft supported table.</note>
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///            <term>Type</term>
        ///            <term>Description</term>
        ///            <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///            <term>directory</term>
        ///            <term>string</term>
        ///            <term>Reference to a directory.  This is the directory that gets modified with the specific attributes.</term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>attr</term>
        ///            <term>int</term>
        ///            <term>Attributes to set for the directory.
        ///            <list type="table">
        ///                <listheader>
        ///                    <term>Value</term>
        ///                    <term>Flag Name</term>
        ///                    <description>Description</description>
        ///                </listheader>
        ///                <item>
        ///                    <term>1</term>
        ///                    <term>AccessRead</term>
        ///                    <description>The file or the contents of the folder may be read through Microsoft Internet Explorer.</description>
        ///                </item>
        ///                <item>
        ///                    <term>2</term>
        ///                    <term>AccessWrite</term>
        ///                    <description>Users are allowed to upload files and their associated properties to the enabled directory on your server or to change content in a Write-enabled file. Write can be implemented only with a browser that supports the PUT feature of the HTTP 1.1 protocol standard.</description>
        ///                </item>
        ///                <item>
        ///                    <term>4</term>
        ///                    <term>AccessExecute</term>
        ///                    <description>The file or the contents of the folder may be executed, regardless of file type.</description>
        ///                </item>
        ///                <item>
        ///                    <term>8</term>
        ///                    <term>AccessSSL</term>
        ///                    <description>File access requires SSL file permission processing, with or without a client certificate.</description>
        ///                </item>
        ///                <item>
        ///                    <term>16</term>
        ///                    <term>AccessSource</term>
        ///                    <description>Users are allowed to access source code if either Read or Write permissions are set. Source code includes scripts in Microsoft ® Active Server Pages (ASP) applications.</description>
        ///                </item>
        ///                <item>
        ///                    <term>32</term>
        ///                    <term>AccessSSLNegotiateCert</term>
        ///                    <description>SSL file access processing requests a certificate from the client. A value of false indicates that access continues if the client does not have a certificate. Some versions of Internet Explorer will close the connection if the server requests a certificate and a certificate is not available (even if AccessSSLRequireCert is also set to true).</description>
        ///                </item>
        ///                <item>
        ///                    <term>64</term>
        ///                    <term>AccessSSLRequireCert</term>
        ///                    <description>SSL file access processing requests a certificate from the client. If the client provides no certificate, the connection is closed. AccessSSLNegotiateCert must also be set to true when using AccessSSLRequireCert.</description>
        ///                </item>
        ///                <item>
        ///                    <term>128</term>
        ///                    <term>AccessSSLMapCert</term>
        ///                    <description>SSL file permission processing maps a client certificate to a Microsoft Windows ® operating system user-account. The AccessSSLNegotiateCert property must also be set to true for the mapping to occur.</description>
        ///                </item>
        ///                <item>
        ///                    <term>256</term>
        ///                    <term>AccessSSL128</term>
        ///                    <description>File access requires SSL file permission processing with a minimum key size of 128 bits, with or without a client certificate.</description>
        ///                </item>
        ///                <item>
        ///                    <term>512</term>
        ///                    <term>AccessScript</term>
        ///                    <description>The file or the contents of the folder may be executed if they are script files or static content. A value of false only allows static files, such as HTML files, to be served.</description>
        ///                </item>
        ///                <item>
        ///                    <term>1024</term>
        ///                    <term>AccessNoRemoteWrite</term>
        ///                    <description>Remote requests to create or change files are denied; only requests from the same computer as the IIS server succeed if the AccessWrite property is set to true. You cannot set AccessNoRemoteWrite to false to enable remote requests, and set AccessWrite to false to disable local requests.</description>
        ///                </item>
        ///                <item>
        ///                    <term>4096</term>
        ///                    <term>AccessNoRemoteRead</term>
        ///                    <description>Remote requests to view files are denied; only requests from the same computer as the IIS server succeed if the AccessRead property is set to true. You cannot set AccessNoRemoteRead to false to enable remote requests, and set AccessRead to false to disable local requests.</description>
        ///                </item>
        ///                <item>
        ///                    <term>8192</term>
        ///                    <term>AccessNoRemoteExecute</term>
        ///                    <description>Remote requests to execute applications are denied; only requests from the same computer as the IIS server succeed if the AccessExecute property is set to true. You cannot set AccessNoRemoteExecute to false to enable remote requests, and set AccessExecute to false to disable local requests.</description>
        ///                </item>
        ///                <item>
        ///                    <term>16384</term>
        ///                    <term>AccessNoRemoteScript</term>
        ///                    <description>Requests to view dynamic content are denied; only requests from the same computer as the IIS server succeed if the AccessScript property is set to true. You cannot set AccessNoRemoteScript to false to enable remote requests, and set AccessScript to false to disable local requests.</description>
        ///                </item>
        ///                <item>
        ///                    <term>32768</term>
        ///                    <term>AccessNoPhysicalDir</term>
        ///                    <description>Access to the physical path is not allowed.</description>
        ///                </item>
        ///            </list>
        ///            </term>
        ///            <term>True</term>
        ///        </item>
        ///     <item>
        ///            <term>defaultdoc</term>
        ///            <term>string</term>
        ///            <term>Adds a filename to the <a href="http://msdn.microsoft.com/library/en-us/iissdk/iis/ref_mb_defaultdoc.asp">Default Documents</a> to process.  Add multiple separated with a comma (Eg. "Default.aspx,Default.htm")</term>
        ///            <term>False</term>
        ///        </item>
        ///    </list>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Specify permissions for the directory structure.</para>
        ///     <code>
        /// &lt;iisproperties&gt;
        ///     &lt;iisproperty directory="TARGETDIR" attr="626" defaultdoc="Default.aspx" /&gt;
        ///     &lt;iisproperty directory="D__BIN" attr="112" /&gt;
        ///     &lt;iisproperty directory="D__SomeSubDir" attr="114" /&gt;
        /// &lt;/iisproperties&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("iisproperties", ProcessXml=false)]
        public SchemaElement[] InstallerIISPropertiesElement {
            get { return null; }
            set {}
        }

        #endregion Nested build elements
    }
}
