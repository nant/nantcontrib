#region GNU General Public License
//
// NAntContrib
//
// Copyright (C) 2002 Jayme C. Edwards (jedwards@wi.rr.com)
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
// Additions: jgeurts@users.sourceforge.net
//
#endregion

using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

using Microsoft.Win32;

using MsmMergeTypeLib;
using WindowsInstaller;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;
using NAnt.Core.Types;

using NAnt.Contrib.Schemas.MSI;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Creates a Windows Installer (also known as Microsoft Installer, or MSI) setup database for installing software on the Windows Platform. 
    /// <br />See the <a href="http://msdn.microsoft.com/library/en-us/msi/setup/roadmap_to_windows_installer_documentation.asp?frame=true" >Roadmap to Windows Installer Documentation</a> at Microsoft's MSDN website for more information.
    /// </summary>
    /// <remarks>
    /// Requires <c>cabarc.exe</c> in the path.  This tool is included in the 
    /// Microsoft Cabinet SDK. (<a href="http://msdn.microsoft.com/library/en-us/dncabsdk/html/cabdl.asp">http://msdn.microsoft.com/library/en-us/dncabsdk/html/cabdl.asp</a>)
    /// </remarks>
    [TaskName("msi")]
    [SchemaValidator(typeof(msi))]
    public class MSITask : SchemaValidatedTask {
        msi msi;

        Hashtable files = new Hashtable();
        Hashtable featureComponents = new Hashtable();
        Hashtable components = new Hashtable();
        ArrayList typeLibRecords = new ArrayList();
        Hashtable typeLibComponents = new Hashtable();

        string[] commonFolderNames = new string[] {
                                                      "AdminToolsFolder", "AppDataFolder",
                                                      "CommonAppDataFolder", "CommonFiles64Folder",
                                                      "CommonFilesFolder", "DesktopFolder",
                                                      "FavoritesFolder", "FontsFolder",
                                                      "LocalAppDataFolder", "MyPicturesFolder",
                                                      "PersonalFolder", "ProgramFiles64Folder",
                                                      "ProgramFilesFolder", "ProgramMenuFolder",
                                                      "SendToFolder", "StartMenuFolder",
                                                      "StartupFolder", "System16Folder",
                                                      "System64Folder", "SystemFolder",
                                                      "TempFolder", "TemplateFolder",
                                                      "WindowsFolder", "WindowsVolume"
                                                  };

        /// <summary>
        /// The name of the .msi file that will be generated when the task completes execution.
        /// </summary>
        [TaskAttribute("output", Required=true)]
        public string MsiOutput
        {
            get { return msi.output; }
        }        

        /// <summary>
        /// A directory relative to the NAnt script in which the msi task resides from which to retrieve files 
        /// that will be installed by the msi database. All files that will be included in your installation need 
        /// to be located directly within or in subdirectories of this directory. 
        /// </summary>
        [TaskAttribute("sourcedir", Required=true)]
        public string MsiSourceDir
        {
            get { return msi.sourcedir; }
        }        

        /// <summary>
        /// An .rtf (rich text format) file containing the license agreement for your software. 
        /// The contents of this file will be displayed to the user when setup runs and must be accepted to continue.
        /// </summary>
        [TaskAttribute("license", Required=true)]
        public string MsiLicense
        {
            get { return msi.license; }
        }

        /// <summary>
        /// A .bmp (bitmap) file 495x60 pixels in size that will be displayed as the banner (top) image of the installation user interface.
        /// </summary>
        [TaskAttribute("banner", Required=false)]
        public string MsiBanner
        {
            get { return msi.banner; }
        }

        /// <summary>
        /// A .bmp (bitmap) file 495x315 pixels in size that will be displayed as the background image of the installation user interface.
        /// </summary>
        [TaskAttribute("background", Required=false)]
        public string MsiBackground
        {
            get { return msi.background; }
        }

        /// <summary>
        /// A .msi file to use as the starting database in which all files and entries will be made, and then copied to the filename 
        /// specified by the output parameter. A .msi template is included with the msi task, you only need to supply this value if you 
        /// want to override the default template and cannot perform something through the features of the msi task. 
        /// </summary>
        [TaskAttribute("template", Required=false)]
        public string MsiTemplate
        {
            get { return msi.template; }
        }

        /// <summary>
        /// A .mst file to use as the starting database containing strings displayed to the user when errors occur during installation. 
        /// A .mst template is included with the msi task, you only need to supply this value if you want to override the default error 
        /// template and cannot perform something through the features of the msi task.
        /// </summary>
        [TaskAttribute("errortemplate", Required=false)]
        public string MsiErrorTemplate
        {
            get { return msi.errortemplate; }
        }

        /// <summary>
        /// Causes the generated msi database file to contain debug messages for errors created by inconsistencies in creation of the 
        /// database. This makes the file slightly larger and should be avoided when generating a production release of your software.
        /// </summary>
        [TaskAttribute("debug", Required=false)]
        public bool MsiDebug
        {
            get { return msi.debug; }
        }

        /// <summary>
        /// Name/value pairs which will be set in the PROPERTY table of the msi database.
        /// <para>
        /// The properties element contains within it one to any number of property elements.<br/>
        /// <a href="http://msdn.microsoft.com/library/en-us/msi/setup/public_properties.asp">Public property</a> names cannot contain lowercase letters.<br/>
        /// <a href="http://msdn.microsoft.com/library/en-us/msi/setup/private_properties.asp">Private property</a> names must contain some lowercase letters.<br/>
        /// Property names prefixed with % represent system and user environment variables. These are 
        /// never entered into the <a href="http://msdn.microsoft.com/library/en-us/msi/setup/property_table.asp">Property 
        /// table</a>. The permanent settings of environment variables can only be modified using the <a href="http://msdn.microsoft.com/library/en-us/msi/setup/environment_table.asp">Environment Table</a>. 
        /// More information available: <a href="http://msdn.microsoft.com/library/default.asp?url=/library/en-us/msi/setup/properties.asp">http://msdn.microsoft.com/library/default.asp?url=/library/en-us/msi/setup/properties.asp</a>
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
        [BuildElementCollection("properties", "property", ElementType=typeof(property))]
        public property[] MsiPropertiesElement
        {
            get { return msi.properties; }
        }        

        /// <summary>
        /// The search element contains within it one to any number of key elements. Key elements are used to search for an existing filesystem directory, file, or Windows Registry setting.  A property in the Msi file is then set with the value obtained from that registry value.
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///         <term>Type</term>
        ///         <term>Description</term>
        ///         <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///         <term>type</term>
        ///         <term>msi:MSISearchKeyType</term>
        ///         <term>Valid input: <c>registry</c> or <c>file</c></term>
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
        ///             <item><c>machine</c> represents HKEY_LOCAL_MACHINE</item>
        ///             <item><c>root</c> represents HKEY_CLASSES_ROOT</item>
        ///             <item><c>user</c> represents HKEY_CURRENT_USER</item>
        ///             <item><c>users</c> represents HKEY_USERS</item>
        ///         </list>
        ///         </term>
        ///         <term>True</term>
        ///     </item>
        ///    </list>
        ///    <h3>Nested Elements:</h3>
        ///    <h4>&lt;value&gt;</h4>
        ///    <ul>
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
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>setproperty</term>
        ///         <term>string</term>
        ///         <term>A name used to refer to the property within the Msi database.  Set at install time.</term>
        ///         <term>True</term>
        ///     </item>
        /// </list>
        /// </ul>
        /// <h4>&lt;/value&gt;</h4>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Get the path of the web directory and the version of IIS.  Create new properties in the Msi file with those values.</para>
        ///     <code>
        /// &lt;search&gt;
        ///     &lt;key type="registry" path="Software\Microsoft\InetStp" root="machine" &gt;
        ///         &lt;value name="PathWWWRoot" setproperty="IISWWWROOT" /&gt;
        ///     &lt;/key&gt;
        ///     &lt;key type="registry" path="SYSTEM\CurrentControlSet\Services\W3SVC\Parameters" root="machine" &gt;
        ///             &lt;value name="MajorVersion" setproperty="IISVERSION" /&gt;
        ///     &lt;/key&gt;
        /// &lt;/search&gt; 
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElementCollection("search", "key", ElementType=typeof(searchKey))]
        public searchKey[] MsiSearchElement
        {
            get { return msi.search; }
        }

        /// <summary>
        /// The launchconditions element contains within it one to any number of launchcondition elements.  Launch conditions are conditions that all must be satisfied for the installation to begin.
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
        [BuildElementCollection("launchconditions", "launchcondition", ElementType=typeof(MSILaunchCondition))]
        public MSILaunchCondition[] MsiLaunchConditionsElement
        {
            get { return msi.launchconditions; }
        }

        /// <summary>
        /// Creates custom tables not directly managed by default features of the msi task.
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
        [BuildElementCollection("tables", "table", ElementType=typeof(MSITable))]
        public MSITable[] MsiTablesElement
        {
            get { return msi.tables; }
        }
        
        /// <summary>
        /// Specifies the directory layout for the product.
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
        [BuildElementCollection("directories", "directory", ElementType=typeof(MSIDirectory))]
        public MSIDirectory[] MsiDirectoriesElement
        {
            get { return msi.directories; }
        }

        /// <summary>
        /// Used to modify the environment variables of the target computer at runtime.
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
        [BuildElementCollection("environment", "variable", ElementType=typeof(MSIVariable))]
        public MSIVariable[] MsiEnvironmentElement
        {
            get { return msi.environment; }
        }

        /// <summary>
        /// Groups sets of components into named sets, these can be used to layout the tree control that allows users to select and deselect features of your software product when a custom installation is selected at runtime.
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
        ///         <term>A name used to refer to the feature.</term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>display</term>
        ///         <term>int</term>
        ///         <term>The number in this field specifies the order in which the feature is to be displayed in the user interface. 
        ///         The value also determines if the feature is initially displayed expanded or collapsed.<br/>
        ///         If the value is null or zero, the record is not displayed. If the value is odd, the feature node is expanded initially. 
        ///         If the value is even, the feature node is collapsed initially.
        ///         </term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>title</term>
        ///         <term>string</term>
        ///         <term>Short string of text identifying the feature. This string is listed as an item by the SelectionTree control of the Selection Dialog.</term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>typical</term>
        ///         <term>bool</term>
        ///         <term>Determines if the feature should be included in a "typical" install.  This is useful for when the user selects to just install the typical features.</term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>directory</term>
        ///         <term>string</term>
        ///         <term>Refrence to a directory.  Specify a corresponding directory to go with the feature.</term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>attr</term>
        ///         <term>int</term>
        ///         <term>Any combination of the following: 
        ///             <list type="table">
        ///                 <listheader>
        ///                     <term>Value</term>
        ///                     <description>Description</description>
        ///                 </listheader>
        ///                 <item>
        ///                     <term>0</term>
        ///                     <description>Components of this feature that are not marked for installation from source are installed locally.</description>
        ///                 </item>
        ///                 <item>
        ///                     <term>1</term>
        ///                     <description>Components of this feature not marked for local installation are installed to run from the source CD-ROM or server.</description>
        ///                 </item>
        ///                 <item>
        ///                     <term>2</term>
        ///                     <description>Set this attribute and the state of the feature is the same as the state of the feature's parent.</description>
        ///                 </item>
        ///                 <item>
        ///                     <term>4</term>
        ///                     <description>Set this attribute and the feature state is Advertise.</description>
        ///                 </item>
        ///                 <item>
        ///                     <term>8</term>
        ///                     <description>Note that this bit works only with features that are listed by the ADVERTISE property. <br/>Set this attribute to prevent the feature from being advertised.</description>
        ///                 </item>
        ///                 <item>
        ///                     <term>16</term>
        ///                     <description>Set this attribute and the user interface does not display an option to change the feature state to Absent. Setting this attribute forces the feature to the installation state, whether or not the feature is visible in the UI.</description>
        ///                 </item>
        ///                 <item>
        ///                     <term>32</term>
        ///                     <description>Set this attribute and advertising is disabled for the feature if the operating system shell does not support Windows Installer descriptors.</description>
        ///                 </item>
        ///             </list>
        ///             More information found here: <a href="http://msdn.microsoft.com/library/en-us/msi/setup/feature_table.asp">http://msdn.microsoft.com/library/en-us/msi/setup/feature_table.asp</a>
        ///         </term>
        ///         <term>False</term>
        ///     </item>
        /// </list>
        /// <h3>Nested Elements:</h3>
        /// <h4>&lt;feature&gt;</h4>
        /// <ul>
        ///     Nested feature elements are supported.
        /// </ul>
        /// <h4>&lt;/feature&gt;</h4>
        /// <h4>&lt;description&gt;</h4>
        /// <ul>
        ///     Longer string of text describing the feature. This localizable string is displayed by the Text control of the Selection Dialog. 
        /// </ul>
        /// <h4>&lt;/description&gt;</h4>
        /// <h4>&lt;conditions&gt;</h4>
        /// <ul>
        ///     <h4>&lt;condition&gt;</h4>
        ///     <ul>
        ///         <list type="table">
        ///             <listheader>
        ///                 <term>Attribute</term>
        ///                 <term>Type</term>
        ///                 <term>Description</term>
        ///                 <term>Required</term>
        ///             </listheader>
        ///             <item>
        ///                 <term>expression</term>
        ///                 <term>string</term>
        ///                 <term>If this conditional expression evaluates to TRUE, then the Level column in the Feature table is set to the 
        ///                 conditional install level. <br/>
        ///                 The expression in the Condition column should not contain reference to the installed state of any feature or component. 
        ///                 This is because the expressions in the Condition column are evaluated before the installer evaluates the installed 
        ///                 states of features and components. Any expression in the Condition table that attempts to check the installed state 
        ///                 of a feature or component always evaluates to false.<br/>
        ///                 For information on the syntax of conditional statements, see <a href="http://msdn.microsoft.com/library/en-us/msi/setup/conditional_statement_syntax.asp">Conditional Statement Syntax</a>.
        ///                 </term>
        ///                 <term>True</term>
        ///             </item>
        ///             <item>
        ///                 <term>level</term>
        ///                 <term>int</term>
        ///                 <term>The installer sets the install level of this feature to the level specified in this column if the expression in 
        ///                 the Condition column evaluates to TRUE.  Set this value to 0 to have the component not install if the condition is not met.<br/>
        ///                 For any installation, there is a defined install level, which is an integral value from 1 to 32,767. The initial value 
        ///                 is determined by the InstallLevel property, which is set in the Property table.<br/>
        ///                 A feature is installed only if the feature level value is less than or equal to the current install level. The user 
        ///                 interface can be authored such that once the installation is initialized, the installer allows the user to modify the 
        ///                 install level of any feature in the Feature table. For example, an author can define install level values that represent 
        ///                 specific installation options, such as Complete, Typical, or Minimum, and then create a dialog box that uses 
        ///                 SetInstallLevel ControlEvents to enable the user to select one of these states. Depending on the state the user selects, 
        ///                 the dialog box sets the install level property to the corresponding value. If the author assigns Typical a level of 100 
        ///                 and the user selects Typical, only those features with a level of 100 or less are installed. In addition, the Custom 
        ///                 option could lead to another dialog box containing a Selection Tree control. The Selection Tree would then allow the user 
        ///                 to individually change whether each feature is installed.</term>
        ///                 <term>True</term>
        ///             </item>
        ///         </list>
        ///     </ul>
        ///     <h4>&lt;/condition&gt;</h4>
        /// </ul>
        /// <h4>&lt;/conditions&gt;</h4>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Define a sample features structure.</para>
        ///     <code>
        /// &lt;features&gt;
        ///     &lt;feature name="F__Default" title="My Product" display="1" typical="true" directory="TARGETDIR"&gt;
        ///         &lt;description&gt;My Product from ACME, Inc. &lt;/description&gt;
        ///         &lt;feature name="F__MainFiles" display="0" typical="true" /&gt;
        ///     &lt;/feature&gt;
        ///     &lt;feature name="F__Help" title="My Product Help Files" display="1" typical="false" directory="D__ACME_MyProduct_Help" /&gt;
        /// &lt;/features&gt; 
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElementCollection("features", "feature", ElementType=typeof(MSIFeature))]
        public MSIFeature[] MsiFeaturesElement
        {
            get { return msi.features; }
        }

        /// <summary>
        /// Groups sets of files into named sets, these can be used to install and perform operations on a set of files as one entity. 
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
        ///                 <term>Name of the key (file) to use.  Also, this could be a registry key.</term>
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
        ///             &lt;includes name="*.*" /&gt;
        ///         &lt;/fileset&gt;
        ///     &lt;/component&gt;
        /// &lt;/components&gt; 
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElementCollection("components", "component", ElementType=typeof(MSIComponent))]
        public MSIComponent[] MsiComponentsElement
        {
            get { return msi.components; }
        }

        /// <summary>
        /// Creates custom dialogs that can gather information not handled by the default msi template.
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
        ///            The cancel control is hidden during rollback or the removal of backed up files. The internal UI handler hides the control 
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
        [BuildElementCollection("dialogs", "dialog", ElementType=typeof(MSIDialog))]
        public MSIDialog[] MsiDialogsElement
        {
            get { return msi.dialogs; }
        }

        /// <summary>
        /// Creates user interface controls displayed on custom dialogs.
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
        [BuildElementCollection("controls", "control", ElementType=typeof(MSIControl))]
        public MSIControl[] MsiControlsElement
        {
            get { return msi.controls; }
        }

        /// <summary>
        /// Used to validate and perform operations as the result of information entered by the user into controls on custom dialogs.
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
        [BuildElementCollection("controlconditions", "controlcondition", ElementType=typeof(MSIControlCondition))]
        public MSIControlCondition[] MsiControlConditionsElement
        {
            get { return msi.controlconditions; }
        }

        /// <summary>
        /// Used to route the flow of the installation process as the result of events raised by the user interacting with controls on dialogs.
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
        [BuildElementCollection("controlevents", "controlevent", ElementType=typeof(MSIControlEvent))]
        public MSIControlEvent[] MsiControlEventsElement
        {
            get { return msi.controlevents; }
        }
        
        /// <summary>
        /// Includes pre-packaged installation components (.msm files) as part of the msi database. This feature allows reuse of installation components that use MSI technology from other setup vendors or as created by the <see cref="MSMTask"/> task. 
        /// <h3>Parameters</h3>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Attribute</term>
        ///            <term>Type</term>
        ///            <term>Description</term>
        ///            <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///            <term>feature</term>
        ///            <term>string</term>
        ///            <term>Refrence to a feature.  Used to associate the merge module with the feature (and the feature's directory) for when to install the components in the merge module.</term>
        ///            <term>True</term>
        ///        </item>
        ///    </list>
        ///    <h3>Nested Elements:</h3>
        ///    <h4>&lt;modules&gt;</h4>
        ///        <ul>
        ///            Specifies the merge module(s) to include with the specified feature.
        ///        </ul>
        ///    <h4>&lt;/modules&gt;</h4>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Add the NAnt merge module to the install.</para>
        ///     <code>
        /// &lt;mergemodules&gt;
        ///     &lt;merge feature="F__NAntMSM"&gt;
        ///         &lt;modules&gt;
        ///             &lt;includes name="${nant.dir}\Install\NAnt.msm" /&gt;
        ///         &lt;/modules&gt;
        ///     &lt;/merge&gt;
        /// &lt;/mergemodules&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElementCollection("mergemodules", "merge", ElementType=typeof(MSIMerge))]
        public MSIMerge[] MsiMergeModulesElement
        {
            get { return msi.mergemodules; }
        }
        
        /// <summary>
        /// Makes modifications to the Windows Registry of the target computer at runtime.
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
        ///                    <item><c>machine</c> represents HKEY_LOCAL_MACHINE</item>
        ///                    <item><c>root</c> represents HKEY_CLASSES_ROOT</item>
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
        ///                    <term>True</term>
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
        /// </summary>
        [BuildElementCollection("registry", "key", ElementType=typeof(MSIRegistryKey))]
        public MSIRegistryKey[] MsiRegistryElement
        {
            get { return msi.registry; }
        }
        
        /// <summary>
        /// Stores icons to be used with shortcuts, file extensions, CLSIDs or similar uses.
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
        [BuildElementCollection("icons", "icon", ElementType=typeof(MSIIcon))]
        public MSIIcon[] MsiIconsElement
        {
            get { return msi.icons; }
        }

        /// <summary>
        /// Creates shortcuts on the target computer.
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
        ///             &lt;description&gt;My Product help documentation&lt;/description&gt;
        ///     &lt;/shortcut&gt;
        /// &lt;/shortcuts&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElementCollection("shortcuts", "shortcut", ElementType=typeof(MSIShortcut))]
        public MSIShortcut[] MsiShortcutsElement
        {
            get { return msi.shortcuts; }
        }

        /// <summary>
        /// Stores the binary data for items such as bitmaps, animations, and icons. The binary table is also used to store data for custom actions. 
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
        [BuildElementCollection("binaries", "binary", ElementType=typeof(MSIBinary))]
        public MSIBinary[] MsiBinariesElement
        {
            get { return msi.binaries; }
        }

        /// <summary>
        /// Used to configure executables that may be run during steps in the installation process to do things outside the bounds of MSI technology's feature set. This is the main spot you can extend MSI technology to perform custom processes via compiled code. Used to configure executables that may be run during steps in the installation process to do things outside the bounds of MSI technology's feature set. This is the main spot you can extend MSI technology to perform custom processes via compiled code. 
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
        [BuildElementCollection("customactions", "customaction", ElementType=typeof(MSICustomAction))]
        public MSICustomAction[] MsiCustomActionsElement
        {
            get { return msi.customactions; }
        }
        
        /// <summary>
        /// Used to modify the sequence of tasks/events that execute during the overall installation process.
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
        [BuildElementCollection("sequences", "sequence", ElementType=typeof(MSISequence))]
        public MSISequence[] MsiSequencesElement
        {
            get { return msi.sequences; }
        }
        
        /// <summary>
        /// Creates text to be displayed in a progress dialog box and written to the log for actions that take a long time to execute. The text displayed consists of the action description and optionally formatted data from the action.  The entries in the ActionText table typically refer to actions in sequence tables.
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
        [BuildElementCollection("actiontext", "action", ElementType=typeof(MSIActionTextAction))]
        public MSIActionTextAction[] MsiActionTextElement
        {
            get { return msi.actiontext; }
        }
        
        /// <summary>
        /// Attn!  Not an officially Microsoft supported table.  Used by the virtual directory custom action.<br/>
        ///    Adds Verbs and a handler for the specified file type.
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
        [BuildElementCollection("appmappings", "appmapping", ElementType=typeof(MSIAppMapping))]
        public MSIAppMapping[] MsiAppMappingsElement
        {
            get { return msi.appmappings; }
        }
        
        /// <summary>
        /// Attn!  Not an officially Microsoft supported table.  Used by the virtual directory custom action.<br/>
        ///    Determines the local path equivalent for a url and stores this information in a property.
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
        [BuildElementCollection("urlproperties", "urlproperty", ElementType=typeof(MSIURLProperty))]
        public MSIURLProperty[] MsiUrlPropertiesElement
        {
            get { return msi.urlproperties; }
        }

        /// <summary>
        /// Attn!  Not an officially Microsoft supported table.  Used by the virtual directory custom action.<br/>
        ///    Creates a URLProperty representing the virtual directory and port.
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
        [BuildElementCollection("vdirproperties", "vdirproperty", ElementType=typeof(MSIVDirProperty))]
        public MSIVDirProperty[] MsiVDirPropertiesElement
        {
            get { return msi.vdirproperties; }
        }
        
        /// <summary>
        /// Attn!  Not an officially Microsoft supported table.  Used by the virtual directory custom action.<br/>
        /// Create a Web application definition and mark it as running in-process or out-of-process. If an application already exists at the specified path, you can use this method to reconfigure the application from in-process to out-of-process, or the reverse.
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
        [BuildElementCollection("approots", "approot", ElementType=typeof(MSIAppRoot))]
        public MSIAppRoot[] MsiAppRootsElement
        {
            get { return msi.approots; }
        }
        
        /// <summary>
        /// Attn!  Not an officially Microsoft supported table.  Used by the virtual directory custom action.<br/>
        /// Specifies directory security in IIS.  Also can configure the default documents supported by each directory.
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
        [BuildElementCollection("iisproperties", "iisproperty", ElementType=typeof(MSIAppRoot))]
        public MSIIISProperty[] MsiIISPropertiesElement
        {
            get { return msi.iisproperties; }
        }
        
        /// <summary>
        /// Initialize taks and verify parameters.
        /// </summary>
        /// <param name="TaskNode">Node that contains the XML fragment
        /// used to define this task instance.</param>
        /// <remarks>None.</remarks>
        protected override void InitializeTask(XmlNode TaskNode) 
        {
            base.InitializeTask(TaskNode);

            msi = (msi)SchemaObject;
        }

        /// <summary>
        /// Executes the Task.
        /// </summary>
        /// <remarks>None.</remarks>
        protected override void ExecuteTask() {
            string tempPath = Path.Combine(Project.BaseDirectory,
                Path.Combine(msi.sourcedir, @"Temp"));

            string cabFile = Path.Combine(Project.BaseDirectory,
                Path.Combine(msi.sourcedir,
                Path.GetFileNameWithoutExtension(msi.output) + @".cab"));

            try {
                // Create WindowsInstaller.Installer
                Type msiType = Type.GetTypeFromProgID("WindowsInstaller.Installer");
                Object obj = Activator.CreateInstance(msiType);

                // Open the Template MSI File
                Module tasksModule = Assembly.GetExecutingAssembly().GetModule("NAnt.Contrib.Tasks.dll");

                string source = Path.Combine(Path.GetDirectoryName(tasksModule.FullyQualifiedName), "MSITaskTemplate.msi");
                if (msi.template != null) {
                    source = Path.Combine(Project.BaseDirectory, msi.template);
                }
                if (!File.Exists(source)) {
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                        "Unable to find template file {0}.", source), Location);
                }

                string dest = Path.Combine(Project.BaseDirectory, Path.Combine(msi.sourcedir, msi.output));

                string errors = Path.Combine(Path.GetDirectoryName(tasksModule.FullyQualifiedName), "MSITaskErrors.mst");
                if (msi.errortemplate != null) {
                    errors = Path.Combine(Project.BaseDirectory, msi.errortemplate);
                }
                if (!File.Exists(errors)) {
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                        "Unable to find error template file {0}.", errors), Location);
                }

                CleanOutput(cabFile, tempPath);

                // copy the template MSI file
                try {
                    File.Copy(source, dest, true);
                    File.SetAttributes(dest, System.IO.FileAttributes.Normal);
                } catch (IOException ex) {
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture, 
                        "File in use or cannot be copied to output ({0} -> {1}).", 
                        source, dest), Location, ex);
                }

                // Open the Output Database.
                Database d = null;

                d = (Database) msiType.InvokeMember(
                    "OpenDatabase",
                    BindingFlags.InvokeMethod,
                    null, obj,
                    new Object[] {
                                     dest,
                                     MsiOpenDatabaseMode.msiOpenDatabaseModeDirect
                                 });

                if (msi.debug) {
                    // if debug is true, transform the error strings in
                    d.ApplyTransform(errors, MsiTransformError.msiTransformErrorNone);
                }

                Log(Level.Info, LogPrefix + "Building MSI Database '{0}'.", msi.output);

                // load the banner image
                if (!LoadBanner(d)) {
                    throw new BuildException();
                }

                // load the background image
                if (!LoadBackground(d)) {
                    throw new BuildException();
                }

                // load the license file
                if (!LoadLicense(d)) {
                    throw new BuildException();
                }

                // load properties
                if (!LoadProperties(d, msiType, obj)) {
                    throw new BuildException();
                }

                // load registry locators
                if (!LoadRegLocator(d, msiType, obj)) {
                    CleanOutput(cabFile, tempPath);
                    throw new BuildException();
                }

                // load application search
                if (!LoadAppSearch(d, msiType, obj)) {
                    throw new BuildException();
                }

                // load launch conditions
                if (!LoadLaunchCondition(d, msiType, obj)) {
                    throw new BuildException();
                }

                // add user defined table(s) to the database
                if (!AddTables(d, msiType, obj)) {
                    throw new BuildException();
                }

                // commit the MSI database
                d.Commit();

                GC.Collect();
                GC.WaitForPendingFinalizers();

                View directoryView, asmView, asmNameView, classView, progIdView;

                // load directories
                if (!LoadDirectories(d, msiType, obj, out directoryView)) {
                    throw new BuildException();
                }

                // load assemblies
                if (!LoadAssemblies(d, msiType, obj, out asmView,
                    out asmNameView, out classView, out progIdView)) {
                    throw new BuildException();
                }

                int lastSequence = 0;

                // load components
                if (!LoadComponents(d, msiType, obj, ref lastSequence,
                    asmView, asmNameView, directoryView, classView, progIdView)) {
                    throw new BuildException();
                }

                directoryView.Close();
                asmView.Close();
                asmNameView.Close();
                classView.Close();
                progIdView.Close();

                directoryView = null;
                asmView = null;
                asmNameView = null;
                classView = null;
                progIdView = null;

                // commit the MSI database
                d.Commit();

                GC.Collect();
                GC.WaitForPendingFinalizers();

                // load features
                if (!LoadFeatures(d, msiType, obj)) {
                    throw new BuildException();
                }

                // load dialog data
                if (!LoadDialog(d, msiType, obj)) {
                    throw new BuildException();
                }

                // load dialog control data
                if (!LoadControl(d, msiType, obj)) {
                    throw new BuildException();
                }

                // load dialog control condition data
                if (!LoadControlCondition(d, msiType, obj)) {
                    throw new BuildException();
                }

                // load dialog control event data
                if (!LoadControlEvent(d, msiType, obj)) {
                    throw new BuildException();
                }

                View registryView;

                // load the registry
                if (!LoadRegistry(d, msiType, obj, out registryView)) {
                    throw new BuildException();
                }

                // load typeLibs
                if (!LoadTypeLibs(d, msiType, obj, registryView)) {
                    throw new BuildException();
                }

                registryView.Close();
                registryView = null;

                // commit the MSI database
                d.Commit();

                GC.Collect();
                GC.WaitForPendingFinalizers();

                // load icon data
                if (!LoadIcon(d, msiType, obj)) {
                    throw new BuildException();
                }

                // load shortcut data
                if (!LoadShortcut(d, msiType, obj)) {
                    throw new BuildException();
                }

                // load binary data
                if (!LoadBinary(d, msiType, obj)) {
                    throw new BuildException();
                }

                // load custom actions
                if (!LoadCustomAction(d, msiType, obj)) {
                    throw new BuildException();
                }

                // load sequences
                if (!LoadSequence(d, msiType, obj)) {
                    throw new BuildException();
                }

                // load action text
                if (!LoadActionText(d, msiType, obj)) {
                    throw new BuildException();
                }

                // load application mappings
                if (!LoadAppMappings(d, msiType, obj)) {
                    throw new BuildException();
                }

                // load the url properties to convert
                // url properties to a properties object
                if (!LoadUrlProperties(d, msiType, obj)) {
                    throw new BuildException();
                }

                // load the vdir properties to convert
                // a vdir to an url
                if (!LoadVDirProperties(d, msiType, obj)) {
                    throw new BuildException();
                }

                // load the application root properties
                // to make a virtual directory an virtual
                // application
                if (!LoadAppRootCreate(d, msiType, obj)) {
                    throw new BuildException();
                }

                // load IIS directory properties
                if (!LoadIISProperties(d, msiType, obj)) {
                    throw new BuildException();
                }

                // load summary information
                if (!LoadSummaryInfo(d)) {
                    throw new BuildException();
                }

                // load environment variables
                if (!LoadEnvironment(d, msiType, obj)) {
                    throw new BuildException();
                }

                // Commit the MSI Database
                d.Commit();
                d = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();

                // load merge modules
                if (!LoadMergeModules(dest, tempPath)) {
                    throw new BuildException();
                }

                d = (Database) msiType.InvokeMember(
                    "OpenDatabase",
                    BindingFlags.InvokeMethod,
                    null, obj,
                    new Object[] {
                                     dest,
                                     MsiOpenDatabaseMode.msiOpenDatabaseModeDirect
                                 });

                // reorder files
                if (!ReorderFiles(d, ref lastSequence)) {
                    throw new BuildException();
                }

                // load media
                if (!LoadMedia(d, msiType, obj, lastSequence)) {
                    throw new BuildException();
                }

                // compress files
                if (!CreateCabFile(d, msiType, obj)) {
                    throw new BuildException();
                }

                Log(Level.Info, LogPrefix + "Saving MSI Database...");

                // Commit the MSI Database
                d.Commit();
                d = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Unable to build MSI database '{0}'.", msi.output), 
                    Location, ex);
            } finally {
                CleanOutput(cabFile, tempPath);
            }
        }

        /// <summary>
        /// Cleans the output directory after a build.
        /// </summary>
        /// <param name="cabFile">The path to the cabinet file.</param>
        /// <param name="tempPath">The path to temporary files.</param>
        private void CleanOutput(string cabFile, string tempPath) {
            try {
                File.Delete(cabFile);
            } catch {}

            try {
                Directory.Delete(tempPath, true);
            } catch {}
        }

        /// <summary>
        /// Loads the banner image.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <returns>True if successful.</returns>
        private bool LoadBanner(Database Database) {
            // Try to open the Banner
            if (msi.banner != null) {
                string bannerFile = Path.Combine(Project.BaseDirectory, msi.banner);
                if (File.Exists(bannerFile)) {
                    View bannerView = Database.OpenView("SELECT * FROM `Binary` WHERE `Name`='bannrbmp'");
                    bannerView.Execute(null);
                    Record bannerRecord = bannerView.Fetch();
                    if (Verbose) {
                        Log(Level.Info, LogPrefix + "Storing Banner:\n\t" + bannerFile);
                    }

                    // Write the Banner file to the MSI database
                    bannerRecord.SetStream(2, bannerFile);
                    bannerView.Modify(MsiViewModify.msiViewModifyUpdate, bannerRecord);
                    bannerView.Close();
                    bannerView = null;
                }
                else {
                    Log(Level.Error, LogPrefix +
                        "Unable to open Banner Image:\n\n\t" +
                        bannerFile + "\n\n");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Loads the background image.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <returns>True if successful.</returns>
        private bool LoadBackground(Database Database) {
            // Try to open the Background
            if (msi.background != null) {
                string bgFile = Path.Combine(Project.BaseDirectory, msi.background);
                if (File.Exists(bgFile)) {
                    View bgView = Database.OpenView("SELECT * FROM `Binary` WHERE `Name`='dlgbmp'");
                    bgView.Execute(null);
                    Record bgRecord = bgView.Fetch();
                    if (Verbose) {
                        Log(Level.Info, LogPrefix + "Storing Background:\n\t" + bgFile);
                    }

                    // Write the Background file to the MSI database
                    bgRecord.SetStream(2, bgFile);
                    bgView.Modify(MsiViewModify.msiViewModifyUpdate, bgRecord);
                    bgView.Close();
                    bgView = null;
                }
                else {
                    Log(Level.Error, LogPrefix +
                        "ERROR: Unable to open Background Image:\n\n\t" +
                        bgFile + "\n\n");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Loads the license file.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <returns>True if successful.</returns>
        private bool LoadLicense(Database Database) {
            // Try to open the License
            if (msi.license != null) {
                string licFile = Path.Combine(Project.BaseDirectory, msi.license);
                if (File.Exists(licFile)) {
                    View licView = Database.OpenView("SELECT * FROM `Control` WHERE `Control`='AgreementText'");
                    licView.Execute(null);
                    Record licRecord = licView.Fetch();
                    if (Verbose) {
                        Log(Level.Info, LogPrefix + "Storing License:\n\t" + licFile);
                    }
                    StreamReader licReader = null;
                    try {
                        licReader = File.OpenText(licFile);
                        licRecord.set_StringData(10, licReader.ReadToEnd());
                        licView.Modify(MsiViewModify.msiViewModifyUpdate, licRecord);
                    } catch (IOException) {
                        Log(Level.Error, LogPrefix +
                            "ERROR: Unable to open License File:\n\n\t" +
                            licFile + "\n\n");
                        return false;
                    } finally {
                        licView.Close();
                        licView = null;
                        if (licReader != null) {
                            licReader.Close();
                            licReader = null;
                        }
                    }
                } else {
                    Log(Level.Error, LogPrefix +
                        "ERROR: Unable to open License File:\n\n\t" +
                        licFile + "\n\n");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Loads records for the Properties table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadProperties(Database Database, Type InstallerType, Object InstallerObject) {
            // Select the "Property" Table
            View propView = Database.OpenView("SELECT * FROM `Property`");

            if (Verbose) {
                Log(Level.Info, LogPrefix + "Adding Properties:");
            }

            // Add properties from Task definition
            foreach (property property in msi.properties) {
                // Insert the Property
                Record recProp = (Record)InstallerType.InvokeMember(
                    "CreateRecord",
                    BindingFlags.InvokeMethod,
                    null, InstallerObject,
                    new object[] { 2 });

                string name = property.name;
                string sValue = property.value;

                if (name == null || name == "") {
                    Log(Level.Error, LogPrefix +
                        "ERROR: Property with no name attribute detected.");
                    return false;
                }

                if (sValue == null || sValue == "") {
                    Log(Level.Error, LogPrefix +
                        "ERROR: Property " + name +
                        " has no value.");
                    return false;
                }

                recProp.set_StringData(1, name);
                recProp.set_StringData(2, sValue);
                propView.Modify(MsiViewModify.msiViewModifyMerge, recProp);

                if (Verbose) {
                    Log(Level.Info, "\t" + name);
                }
            }
            propView.Close();
            propView = null;
            return true;
        }

        /// <summary>
        /// Loads records for the Components table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <param name="LastSequence">The sequence number of the last file in the .cab</param>
        /// <param name="MsiAssemblyView">View containing the MsiAssembly table.</param>
        /// <param name="MsiAssemblyNameView">View containing the MsiAssemblyName table.</param>
        /// <param name="DirectoryView">The MSI database view.</param>
        /// <param name="ClassView">View containing the Class table.</param>
        /// <param name="ProgIdView">View containing the ProgId table.</param>
        /// <returns>True if successful.</returns>
        private bool LoadComponents(Database Database, Type InstallerType, Object InstallerObject,
            ref int LastSequence, View MsiAssemblyView, View MsiAssemblyNameView,
            View DirectoryView, View ClassView, View ProgIdView) {
            // Open the "Component" Table
            View compView = Database.OpenView("SELECT * FROM `Component`");

            // Open the "File" Table
            View fileView = Database.OpenView("SELECT * FROM `File`");

            // Open the "FeatureComponents" Table
            View featCompView = Database.OpenView("SELECT * FROM `FeatureComponents`");

            // Open the "SelfReg" Table
            View selfRegView = Database.OpenView("SELECT * FROM `SelfReg`");

            // Add components from Task definition
            int componentIndex = 0;

            if (Verbose) {
                Log(Level.Info, LogPrefix + "Add Files:");
            }

            if (msi.components != null) {
                foreach (MSIComponent component in msi.components) {
                    // Insert the Component
                    Record recComp = (Record)InstallerType.InvokeMember(
                        "CreateRecord",
                        BindingFlags.InvokeMethod,
                        null, InstallerObject,
                        new object[] { 6 });

                    recComp.set_StringData(1, component.name);
                    recComp.set_StringData(2, component.id.ToUpper());
                    recComp.set_StringData(3, component.directory);
                    recComp.set_IntegerData(4, component.attr);
                    recComp.set_StringData(5, component.condition);

                    featureComponents.Add(component.name, component.feature);

                    componentIndex++;

                    bool success = AddFiles(Database, DirectoryView, component,
                        fileView, InstallerType, InstallerObject,
                        component.directory, component.name, ref componentIndex,
                        ref LastSequence, MsiAssemblyView, MsiAssemblyNameView,
                        compView, featCompView, ClassView, ProgIdView, selfRegView);

                    if (!success) {
                        return success;
                    }

                    if ((component.attr & 4) != 0) {
                        recComp.set_StringData(6, component.key.file);
                        compView.Modify(MsiViewModify.msiViewModifyMerge, recComp);
                    } else if (files.Contains(component.directory + "|" + component.key.file)) {
                        string keyFileName = (string)files[component.directory + "|" + component.key.file];
                        if (keyFileName == "KeyIsDotNetAssembly") {
                            Log(Level.Error, LogPrefix + "ERROR: Cannot specify key '" + component.key.file +
                                "' for component '" + component.name + "'. File has been detected as " +
                                "being a COM component or Microsoft.NET assembly and is " +
                                "being registered with its own component. Please specify " +
                                "a different file in the same directory for this component's key.");
                            return false;
                        } else {
                            recComp.set_StringData(6, keyFileName);
                            compView.Modify(MsiViewModify.msiViewModifyMerge, recComp);
                        }
                    } else {
                        Log(Level.Error,
                            LogPrefix + "ERROR: KeyFile \"" + component.key.file +
                            "\" not found in Component \"" + component.name + "\".");
                        return false;
                    }
                }

                // Add featureComponents from Task definition
                IEnumerator keyEnum = featureComponents.Keys.GetEnumerator();

                while (keyEnum.MoveNext()) {
                    string component = Properties.ExpandProperties((string)keyEnum.Current, Location);
                    string feature = Properties.ExpandProperties((string)featureComponents[component], Location);

                    if (feature == null) {
                        Log(Level.Error, LogPrefix +
                            "ERROR: Component " + component +
                            " mapped to nonexistent feature.");
                        return false;
                    }

                    // Insert the FeatureComponent
                    Record recFeatComps = (Record) InstallerType.InvokeMember(
                        "CreateRecord",
                        BindingFlags.InvokeMethod,
                        null, InstallerObject,
                        new object[] { 2 });

                    recFeatComps.set_StringData(1, feature);
                    recFeatComps.set_StringData(2, component);
                    featCompView.Modify(MsiViewModify.msiViewModifyMerge, recFeatComps);
                }
            }

            compView.Close();
            fileView.Close();
            featCompView.Close();
            selfRegView.Close();

            compView = null;
            fileView = null;
            featCompView = null;
            selfRegView = null;

            return true;
        }

        /// <summary>
        /// Loads records for the Directories table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <param name="DirectoryView">The MSI database view.</param>
        /// <returns>True if successful.</returns>
        private bool LoadDirectories(Database Database, Type InstallerType,
            Object InstallerObject, out View DirectoryView) {
            // Open the "Directory" Table
            DirectoryView = Database.OpenView("SELECT * FROM `Directory`");

            ArrayList directoryList = new ArrayList(msi.directories);

            MSIRootDirectory targetDir = new MSIRootDirectory();
            targetDir.name = "TARGETDIR";
            targetDir.root = "";
            targetDir.foldername = "SourceDir";
            directoryList.Add(targetDir);

            // Insert the Common Directories
            for (int i = 0; i < commonFolderNames.Length; i++) {
                MSIRootDirectory commonDir = new MSIRootDirectory();
                commonDir.name = commonFolderNames[i];
                commonDir.root = "TARGETDIR";
                commonDir.foldername = ".";
                directoryList.Add(commonDir);
            }

            MSIRootDirectory[] directories = new MSIRootDirectory[directoryList.Count];
            directoryList.CopyTo(directories);
            msi.directories = directories;

            int depth = 1;

            if (Verbose) {
                Log(Level.Info, LogPrefix + "Adding Directories:");
            }

            // Add directories from Task definition
            foreach (MSIRootDirectory directory in msi.directories) {
                bool result = AddDirectory(Database,
                    DirectoryView, null, InstallerType,
                    InstallerObject, directory, depth);

                if (!result) {
                    DirectoryView.Close();
                    DirectoryView = null;
                    return result;
                }
            }

            return true;
        }

        /// <summary>
        /// Adds a directory record to the directories table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="DirectoryView">The MSI database view.</param>
        /// <param name="ParentDirectory">The parent directory.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <param name="Directory">This directory's Schema object.</param>
        /// <param name="Depth">The tree depth of this directory.</param>
        /// <returns></returns>
        private bool AddDirectory(Database Database, View DirectoryView,
            string ParentDirectory,
            Type InstallerType, object InstallerObject,
            MSIDirectory Directory, int Depth) {
            string newParent = ParentDirectory;
            if (Directory is MSIRootDirectory) {
                newParent = ((MSIRootDirectory)Directory).root;
            }

            // Insert the Directory
            Record recDir = (Record)InstallerType.InvokeMember(
                "CreateRecord",
                BindingFlags.InvokeMethod,
                null, InstallerObject, new object[] { 3 });

            recDir.set_StringData(1, Directory.name);
            recDir.set_StringData(2, newParent);

            StringBuilder relativePath = new StringBuilder();

            GetRelativePath(Database, InstallerType, InstallerObject,
                Directory.name, ParentDirectory, Directory.foldername,
                relativePath, DirectoryView);

            string basePath = Path.Combine(Project.BaseDirectory, msi.sourcedir);
            string fullPath = Path.Combine(basePath, relativePath.ToString());
            string path = GetShortPath(fullPath) + "|" + Directory.foldername;
            if (Directory.foldername == ".")
                path = Directory.foldername;

            if (relativePath.ToString() == "") {
                return true;
            }

            if (path == "MsiTaskPathNotFound") {
                return false;
            }

            if (Verbose) {
                Log(Level.Info, "\t" +
                    Path.Combine(Project.BaseDirectory, Path.Combine(msi.sourcedir, relativePath.ToString())));
            }

            recDir.set_StringData(3, path);

            DirectoryView.Modify(MsiViewModify.msiViewModifyMerge, recDir);

            if (Directory.directory != null) {
                foreach (MSIDirectory childDirectory in Directory.directory) {
                    int newDepth = Depth + 1;

                    bool result = AddDirectory(Database, DirectoryView,
                        Directory.name, InstallerType,
                        InstallerObject, childDirectory, newDepth);

                    if (!result) {
                        return result;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Loads records for the Media table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <param name="LastSequence">The sequence number of the last file in the .cab.</param>
        /// <returns>True if successful.</returns>
        private bool LoadMedia(Database Database, Type InstallerType, Object InstallerObject, int LastSequence) {
            // Open the "Media" Table
            View mediaView = Database.OpenView("SELECT * FROM `Media`");

            // Insert the Disk
            Record recMedia = (Record)InstallerType.InvokeMember(
                "CreateRecord",
                BindingFlags.InvokeMethod,
                null, InstallerObject,
                new object[] { 6 });

            recMedia.set_StringData(1, "1");
            recMedia.set_StringData(2, LastSequence.ToString());
            recMedia.set_StringData(4, "#" + Path.GetFileNameWithoutExtension(msi.output) + ".cab");
            mediaView.Modify(MsiViewModify.msiViewModifyMerge, recMedia);

            mediaView.Close();

            mediaView = null;

            return true;
        }

        /// <summary>
        /// Loads properties for the Summary Information Stream.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <returns>True if successful.</returns>
        private bool LoadSummaryInfo(Database Database) {
            property productName = null;
            property manufacturer = null;
            property keywords = null;
            property comments = null;

            foreach (property prop in msi.properties) {
                if (prop.name == "ProductName") {
                    productName = prop;
                } else if (prop.name == "Manufacturer") {
                    manufacturer = prop;
                } else if (prop.name == "Keywords") {
                    keywords = prop;
                } else if (prop.name == "Comments") {
                    comments = prop;
                }
            }

            SummaryInfo summaryInfo = Database.get_SummaryInformation(200);
            summaryInfo.set_Property(2, productName.value);
            summaryInfo.set_Property(3, productName.value);
            summaryInfo.set_Property(4, manufacturer.value);

            if (keywords != null) {
                summaryInfo.set_Property(5, keywords.value);
            }
            if (comments != null) {
                summaryInfo.set_Property(6, comments.value);
            }

            summaryInfo.set_Property(9, "{"+Guid.NewGuid().ToString().ToUpper()+"}");
            summaryInfo.set_Property(14, 200);
            summaryInfo.set_Property(15, 2);

            summaryInfo.Persist();

            return true;
        }

        /// <summary>
        /// Loads environment variables for the Environment table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadEnvironment(Database Database, Type InstallerType, Object InstallerObject) {
            // Open the "Environment" Table
            View envView = Database.OpenView("SELECT * FROM `Environment`");

            if (msi.environment != null) {
                foreach (MSIVariable variable in msi.environment) {
                    // Insert the Varible
                    Record recVar = (Record)InstallerType.InvokeMember(
                        "CreateRecord",
                        BindingFlags.InvokeMethod,
                        null, InstallerObject,
                        new object[] { 4 });

                    recVar.set_StringData(1, "_" + Guid.NewGuid().ToString().ToUpper().Replace("-", null));
                    recVar.set_StringData(2, variable.name);

                    if (variable.append != null && variable.append != "") {
                        recVar.set_StringData(3, "[~];" + variable.append);
                    }

                    recVar.set_StringData(4, variable.component);

                    envView.Modify(MsiViewModify.msiViewModifyMerge, recVar);
                }
            }
            envView.Close();
            envView = null;

            return true;
        }

        /// <summary>
        /// Loads records for the Features table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadFeatures(Database Database, Type InstallerType, Object InstallerObject) {
            // Open the "Feature" Table
            View featView = Database.OpenView("SELECT * FROM `Feature`");
            // Open the "Condition" Table
            View conditionView = Database.OpenView("SELECT * FROM `Condition`");

            // Add features from Task definition
            int order = 1;
            int depth = 1;

            if (Verbose) {
                Log(Level.Info, LogPrefix + "Adding Features:");
            }

            foreach (MSIFeature feature in msi.features) {
                bool result = AddFeature(featView, conditionView, null, InstallerType,
                    InstallerObject, feature, depth, order);

                if (!result) {
                    featView.Close();
                    return result;
                }
                order++;
            }

            featView.Close();
            conditionView.Close();

            featView = null;
            conditionView = null;
            return true;
        }

        /// <summary>
        /// Adds a feature record to the Features table.
        /// </summary>
        /// <param name="FeatureView">The MSI database view for Feature table.</param>
        /// <param name="ConditionView">The MSI database view for Condition table.</param>
        /// <param name="ParentFeature">The name of this feature's parent.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI INstaller object.</param>
        /// <param name="Feature">This Feature's Schema element.</param>
        /// <param name="Depth">The tree depth of this feature.</param>
        /// <param name="Order">The tree order of this feature.</param>
        private bool AddFeature(View FeatureView, View ConditionView, string ParentFeature,
            Type InstallerType, Object InstallerObject,
            MSIFeature Feature, int Depth, int Order) {
            string directory = null;
            if (Feature.directory != null) {
                directory = Feature.directory;
            } else {
                bool foundComponent = false;

                IEnumerator featComps = featureComponents.Keys.GetEnumerator();

                while (featComps.MoveNext()) {
                    string componentName = (string)featComps.Current;
                    string featureName = (string)featureComponents[componentName];

                    if (featureName == Feature.name) {
                        directory = (string)components[componentName];
                        foundComponent = true;
                    }
                }

                if (!foundComponent) {
                    Log(Level.Error,
                        LogPrefix + "ERROR: Feature " + Feature.name +
                        " needs to be assigned a component or directory.");
                    return false;
                }
            }

            // Insert the Feature
            Record recFeat = (Record)InstallerType.InvokeMember(
                "CreateRecord",
                BindingFlags.InvokeMethod,
                null, InstallerObject,
                new object[] { 8 });

            recFeat.set_StringData(1, Feature.name);
            recFeat.set_StringData(2, ParentFeature);
            recFeat.set_StringData(3, Feature.title);
            recFeat.set_StringData(4, Feature.description);
            recFeat.set_IntegerData(5, Feature.display);

            if (!Feature.typical) {
                recFeat.set_StringData(6, "4");
            } else {
                recFeat.set_StringData(6, "3");
            }

            recFeat.set_StringData(7, directory);
            recFeat.set_IntegerData(8, Feature.attr);

            FeatureView.Modify(MsiViewModify.msiViewModifyMerge, recFeat);

            if (Verbose) {
                Log(Level.Info, "\t" + Feature.name);
            }

            // Add feature conditions
            if (Feature.conditions != null) {
                if (Verbose) {
                    Log(Level.Info, "\t\tAdding Feature Conditions...");
                }

                foreach (MSIFeatureCondition featureCondition in Feature.conditions) {
                    try {
                        // Insert the feature's condition
                        Record recCondition = (Record)InstallerType.InvokeMember(
                            "CreateRecord",
                            BindingFlags.InvokeMethod,
                            null, InstallerObject,
                            new object[] { 3 });

                        recCondition.set_StringData(1, Feature.name);
                        recCondition.set_IntegerData(2, featureCondition.level);
                        recCondition.set_StringData(3, featureCondition.expression);
                        ConditionView.Modify(MsiViewModify.msiViewModifyMerge, recCondition);
                    }
                    catch (Exception e) {
                        Log(Level.Info, "\nError adding feature condition: " + e.ToString());
                    }
                }

                if (Verbose) {
                    Log(Level.Info, "Done");
                }
            }

            if (Feature.feature != null) {
                foreach (MSIFeature childFeature in Feature.feature) {
                    int newDepth = Depth + 1;
                    int newOrder = 1;

                    bool result = AddFeature(FeatureView, ConditionView, Feature.name, InstallerType,
                        InstallerObject, childFeature, newDepth, newOrder);

                    if (!result) {
                        return result;
                    }
                    newOrder++;
                }
            }
            return true;
        }

        [DllImport("kernel32")]
        private extern static int LoadLibrary(string lpLibFileName);
        [DllImport("kernel32")]
        private extern static bool FreeLibrary(int hLibModule);
        [DllImport("kernel32", CharSet=CharSet.Ansi)]
        private extern static int GetProcAddress(int hModule, string lpProcName);

        /// <summary>
        /// Adds a file record to the Files table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="DirectoryView">The MSI database view.</param>
        /// <param name="Component">The Component's XML Element.</param>
        /// <param name="FileView">The MSI database view.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <param name="ComponentDirectory">The directory of this file's component.</param>
        /// <param name="ComponentName">The name of this file's component.</param>
        /// <param name="ComponentCount">The index in the number of components of this file's component.</param>
        /// <param name="Sequence">The installation sequence number of this file.</param>
        /// <param name="MsiAssemblyView">View containing the MsiAssembly table.</param>
        /// <param name="MsiAssemblyNameView">View containing the MsiAssemblyName table.</param>
        /// <param name="ComponentView">View containing the Components table.</param>
        /// <param name="FeatureComponentView">View containing the FeatureComponents table.</param>
        /// <param name="ClassView">View containing the Class table.</param>
        /// <param name="ProgIdView">View containing the ProgId table.</param>
        /// <param name="SelfRegView">View containing the SelfReg table.</param>
        /// <returns>True if successful.</returns>
        private bool AddFiles(Database Database, View DirectoryView, MSIComponent Component,
            View FileView, Type InstallerType, Object InstallerObject,
            string ComponentDirectory, string ComponentName, ref int ComponentCount,
            ref int Sequence, View MsiAssemblyView, View MsiAssemblyNameView,
            View ComponentView, View FeatureComponentView, View ClassView, View ProgIdView,
            View SelfRegView) {
            XmlElement fileSetElem = (XmlElement)((XmlElement)_xmlNode).SelectSingleNode(
                "components/component[@id='" + Component.id + "']/fileset");

            FileSet componentFiles = new FileSet();
            componentFiles.Project = Project;
            componentFiles.Parent = this;
            componentFiles.Initialize(fileSetElem);

            MSIDirectory componentDirInfo = FindDirectory(ComponentDirectory);

            StringBuilder relativePath = new StringBuilder();

            string newParent = null;
            if (componentDirInfo is MSIRootDirectory) {
                newParent = ((MSIRootDirectory)componentDirInfo).root;
            } else {
                newParent = FindParent(ComponentDirectory);
            }

            GetRelativePath(Database, InstallerType,
                InstallerObject,
                ComponentDirectory,
                newParent,
                componentDirInfo.foldername,
                relativePath, DirectoryView);

            string basePath = Path.Combine(Project.BaseDirectory, msi.sourcedir);
            string fullPath = Path.Combine(basePath, relativePath.ToString());

            for (int i = 0; i < componentFiles.FileNames.Count; i++) {
                // Insert the File
                Record recFile = (Record)InstallerType.InvokeMember(
                    "CreateRecord",
                    BindingFlags.InvokeMethod,
                    null, InstallerObject,
                    new object[] { 8 });

                string fileName = Path.GetFileName(componentFiles.FileNames[i]);
                string filePath = Path.Combine(fullPath, fileName);

                MSIFileOverride fileOverride = null;

                if (Component.forceid != null) {
                    foreach (MSIFileOverride curOverride in Component.forceid) {
                        if (curOverride.file == fileName) {
                            fileOverride = curOverride;
                            break;
                        }
                    }
                }

                string fileId = fileOverride == null ?
                    "_" + Guid.NewGuid().ToString().ToUpper().Replace("-", null) :
                    fileOverride.id;

                // If the user specifies forceid & specified a file attribute, use it.  Otherwise use the
                // fileattr assigned to the component.
                int fileAttr = ((fileOverride == null) || (fileOverride.attr == 0)) ? Component.fileattr : fileOverride.attr;

                files.Add(Component.directory + "|" + fileName, fileId);
                recFile.set_StringData(1, fileId);

                if (File.Exists(filePath)) {
                    try {
                        recFile.set_StringData(4, new FileInfo(filePath).Length.ToString());
                    } catch (Exception) {
                        Log(Level.Error, LogPrefix +
                            "ERROR: Could not open file " + filePath);
                        return false;
                    }
                } else {
                    Log(Level.Error, LogPrefix +
                        "ERROR: Could not open file " + filePath);
                    return false;
                }

                if (Verbose) {
                    Log(Level.Info, "\t" +
                        Path.Combine(Project.BaseDirectory, Path.Combine(Path.Combine(msi.sourcedir,
                        relativePath.ToString()), fileName)));
                }

                // If the file is an assembly, create a new component to contain it,
                // add the new component, map the new component to the old component's
                // feature, and create an entry in the MsiAssembly and MsiAssemblyName
                // table.
                //
                bool isAssembly = false;
                Assembly fileAssembly = null;
                string fileVersion = "";
                try {
                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);
                    fileVersion = fileVersionInfo.FileVersion;
                } catch {}

                try {
                    fileAssembly = Assembly.LoadFrom(filePath);
                    fileVersion = fileAssembly.GetName().Version.ToString();
                    isAssembly = true;
                } catch {}

                if (isAssembly || filePath.EndsWith(".tlb")) {
                    string feature = (string)featureComponents[ComponentName];

                    string asmCompName = ComponentName;

                    if (componentFiles.FileNames.Count > 1) {
                        asmCompName = "C_" + fileId;
                        string newCompId = "{" + Guid.NewGuid().ToString().ToUpper() + "}";

                        recFile.set_StringData(2, asmCompName);

                        // Add a record for a new Component
                        Record recComp = (Record)InstallerType.InvokeMember(
                            "CreateRecord",
                            BindingFlags.InvokeMethod,
                            null, InstallerObject,
                            new object[] { 6 });

                        recComp.set_StringData(1, asmCompName);
                        recComp.set_StringData(2, newCompId);
                        recComp.set_StringData(3, ComponentDirectory);
                        recComp.set_IntegerData(4, Component.attr);
                        recComp.set_StringData(5, Component.condition);
                        recComp.set_StringData(6, fileId);
                        ComponentView.Modify(MsiViewModify.msiViewModifyMerge, recComp);

                        // Map the new Component to the existing one's Feature
                        Record featComp = (Record)InstallerType.InvokeMember(
                            "CreateRecord",
                            BindingFlags.InvokeMethod,
                            null, InstallerObject,
                            new object[] { 2 });

                        featComp.set_StringData(1, (string)featureComponents[ComponentName]);
                        featComp.set_StringData(2, asmCompName);
                        FeatureComponentView.Modify(MsiViewModify.msiViewModifyMerge, featComp);
                    }

                    if (isAssembly) {
                        // Add a record for a new MsiAssembly
                        Record recAsm = (Record)InstallerType.InvokeMember(
                            "CreateRecord",
                            BindingFlags.InvokeMethod,
                            null, InstallerObject,
                            new object[] { 5 });

                        recAsm.set_StringData(1, asmCompName);
                        recAsm.set_StringData(2, (string)featureComponents[ComponentName]);
                        recAsm.set_StringData(3, fileId);
                        recAsm.set_StringData(4, fileId);
                        recAsm.set_IntegerData(5, 0);
                        MsiAssemblyView.Modify(MsiViewModify.msiViewModifyMerge, recAsm);

                        //
                        // Add records for the Assembly Manifest
                        //

                        AssemblyName asmName = fileAssembly.GetName();

                        string name = asmName.Name;
                        string version = asmName.Version.ToString(4);

                        AssemblyCultureAttribute[] cultureAttrs =
                            (AssemblyCultureAttribute[])fileAssembly.GetCustomAttributes(
                            typeof(AssemblyCultureAttribute), true);

                        string culture = "neutral";
                        if (cultureAttrs.Length > 0) {
                            culture = cultureAttrs[0].Culture;
                        }

                        string publicKey = null;
                        byte[] keyToken = asmName.GetPublicKeyToken();
                        if (keyToken != null) {
                            publicKey = ByteArrayToString(keyToken);
                        }

                        if (name != null && name != "") {
                            Record recAsmName = (Record)InstallerType.InvokeMember(
                                "CreateRecord",
                                BindingFlags.InvokeMethod,
                                null, InstallerObject,
                                new object[] { 3 });

                            recAsmName.set_StringData(1, asmCompName);
                            recAsmName.set_StringData(2, "Name");
                            recAsmName.set_StringData(3, name);
                            MsiAssemblyNameView.Modify(MsiViewModify.msiViewModifyMerge, recAsmName);
                        }

                        if (version != null && version != "") {
                            Record recAsmVersion = (Record)InstallerType.InvokeMember(
                                "CreateRecord",
                                BindingFlags.InvokeMethod,
                                null, InstallerObject, new object[] { 3 });

                            recAsmVersion.set_StringData(1, asmCompName);
                            recAsmVersion.set_StringData(2, "Version");
                            recAsmVersion.set_StringData(3, version);
                            MsiAssemblyNameView.Modify(MsiViewModify.msiViewModifyMerge, recAsmVersion);
                        }

                        if (culture != null && culture != "") {
                            Record recAsmLocale = (Record)InstallerType.InvokeMember(
                                "CreateRecord",
                                BindingFlags.InvokeMethod,
                                null, InstallerObject,
                                new object[] { 3 });

                            recAsmLocale.set_StringData(1, asmCompName);
                            recAsmLocale.set_StringData(2, "Culture");
                            recAsmLocale.set_StringData(3, culture);
                            MsiAssemblyNameView.Modify(MsiViewModify.msiViewModifyMerge, recAsmLocale);
                        }

                        if (publicKey != null && publicKey != "") {
                            Record recPublicKey = (Record)InstallerType.InvokeMember(
                                "CreateRecord",
                                BindingFlags.InvokeMethod,
                                null, InstallerObject,
                                new object[] { 3 });

                            recPublicKey.set_StringData(1, asmCompName);
                            recPublicKey.set_StringData(2, "PublicKeyToken");
                            recPublicKey.set_StringData(3, publicKey);
                            MsiAssemblyNameView.Modify(MsiViewModify.msiViewModifyMerge, recPublicKey);
                        }

                        bool checkInterop = Component.checkinterop;

                        if (fileOverride != null) {
                            checkInterop = fileOverride.checkinterop;
                        }

                        if (checkInterop) {
                            bool success = CheckAssemblyForCOMInterop(
                                filePath, fileAssembly, InstallerType,
                                InstallerObject, ComponentName,
                                asmCompName, ClassView, ProgIdView);

                            if (!success) {
                                return success;
                            }
                        }

                        // File can't be a member of both components
                        if (componentFiles.FileNames.Count > 1) {
                            files.Remove(ComponentDirectory + "|" + fileName);
                            files.Add(ComponentDirectory + "|" + fileName, "KeyIsDotNetAssembly");
                        }
                    }
                    else if (filePath.EndsWith(".tlb")) {
                        typeLibComponents.Add(
                            Path.GetFileName(filePath),
                            asmCompName);
                    }
                }

                if (filePath.EndsWith(".dll")) {
                    int hmod = LoadLibrary(filePath);
                    if (hmod != 0) {
                        int regSvr = GetProcAddress(hmod, "DllRegisterServer");
                        if (regSvr != 0) {
                            Log(Level.Info, LogPrefix +
                                "Configuring " +
                                Path.GetFileName(filePath) +
                                " for COM Self Registration...");

                            // Add a record for a new Component
                            Record recSelfReg = (Record)InstallerType.InvokeMember(
                                "CreateRecord",
                                BindingFlags.InvokeMethod,
                                null, InstallerObject,
                                new object[] { 2 });

                            recSelfReg.set_StringData(1, fileId);
                            SelfRegView.Modify(MsiViewModify.msiViewModifyMerge, recSelfReg);
                        }
                        FreeLibrary(hmod);
                    }

                    // Register COM .dlls with an embedded
                    // type library for self registration.
                }

                if (File.Exists(filePath)) {
                    string cabDir = Path.Combine(
                        Project.BaseDirectory,
                        Path.Combine(msi.sourcedir, "Temp"));

                    if (!Directory.Exists(cabDir)) {
                        Directory.CreateDirectory(cabDir);
                    }

                    string cabPath = Path.Combine(cabDir, fileId);
                    File.Copy(filePath, cabPath, true);
                }

                if (!isAssembly && !filePath.EndsWith(".tlb")
                    || componentFiles.FileNames.Count == 1) {
                    recFile.set_StringData(2, Component.name);
                }

                // Set the file version equal to the override value, if present
                if ((fileOverride != null) && (fileOverride.version != null) && (fileOverride.version != "")) {
                    fileVersion = fileOverride.version;
                }

                if (!IsVersion(ref fileVersion)) {
                    fileVersion = null;
                }

                // propagate language (if available) to File table to avoid 
                // ICE60 verification warnings
                string language = null;
                try {
                    if (isAssembly) {
                        int lcid = fileAssembly.GetName().CultureInfo.LCID;
                        language = (lcid == 0x007F) ? "0" : lcid.ToString(CultureInfo.InvariantCulture);
                    } else {
                        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);
                        language = fileVersionInfo.Language;
                    }
                } catch {}

                recFile.set_StringData(3, GetShortFile(filePath) + "|" + fileName);
                recFile.set_StringData(5, fileVersion);
                recFile.set_StringData(6, language);
                recFile.set_IntegerData(7, fileAttr);

                Sequence++;

                recFile.set_StringData(8, Sequence.ToString());
                FileView.Modify(MsiViewModify.msiViewModifyMerge, recFile);
            }
            return true;
        }

        /// <summary>
        /// Determines if the supplied version string is valid.  A valid version string should look like:
        /// 1
        /// 1.1
        /// 1.1.1
        /// 1.1.1.1
        /// </summary>
        /// <param name="Version">The version string to verify.</param>
        /// <returns></returns>
        private bool IsVersion(ref string Version) {
            // For cases of 5,5,2,2
            Version = Version.Trim().Replace(",", ".");
            Version = Version.Replace(" ", "");
            string[] versionParts = Version.Split('.');
            bool result = true;

            foreach (string versionPart in versionParts) {
                try {
                    int iVersionPart = Convert.ToInt32(versionPart);
                }
                catch (Exception) {
                    result = false;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Loads records for the Registry table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <param name="RegistryView">View containing the Registry table.</param>
        /// <returns>True if successful.</returns>
        private bool LoadRegistry(Database Database, Type InstallerType,
            Object InstallerObject, out View RegistryView) {
            // Open the "Registry" Table
            RegistryView = Database.OpenView("SELECT * FROM `Registry`");

            if (msi.registry != null) {
                if (Verbose) {
                    Log(Level.Info, LogPrefix + "Adding Registry Values:");
                }

                foreach (MSIRegistryKey key in msi.registry) {
                    int rootKey = -1;
                    switch (key.root.ToString()) {
                        case "classes":
                            rootKey = 0;
                            break;
                        case "user":
                            rootKey = 1;
                            break;
                        case "machine":
                            rootKey = 2;
                            break;
                        case "users":
                            rootKey = 3;
                            break;
                    }

                    foreach (MSIRegistryKeyValue value in key.value) {
                        // Insert the Value
                        Record recVal = (Record)InstallerType.InvokeMember(
                            "CreateRecord",
                            BindingFlags.InvokeMethod,
                            null, InstallerObject,
                            new object[] { 6 });

                        recVal.set_StringData(1, (value.id != null ? value.id : "_" +
                            Guid.NewGuid().ToString().ToUpper().Replace("-", null)));
                        recVal.set_StringData(2, rootKey.ToString());
                        recVal.set_StringData(3, key.path);
                        recVal.set_StringData(4, value.name);

                        if (Verbose) {
                            string keypath = GetDisplayablePath(key.path);
                            Log(Level.Info, "\t" + keypath + @"#" + value.name);
                        }

                        if (value.value != null && value.value != "") {
                            recVal.set_StringData(5, value.value);
                        } else if (value.dword != null && value.dword != "") {
                            string sDwordMsi = "#" + Int32.Parse(value.dword);
                            recVal.set_StringData(5, sDwordMsi);
                        } else {
                            string val1 = value.Value.Replace(",", null);
                            string val2 = val1.Replace(" ", null);
                            string val3 = val2.Replace("\n", null);
                            string val4 = val3.Replace("\r", null);
                            recVal.set_StringData(5, "#x" + val4);
                        }

                        recVal.set_StringData(6, key.component);
                        RegistryView.Modify(MsiViewModify.msiViewModifyMerge, recVal);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Creates the assembly and assembly name tables.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <param name="MsiAssemblyView">View containing the MsiAssembly table.</param>
        /// <param name="MsiAssemblyNameView">View containing the MsiAssemblyName table.</param>
        /// <param name="ClassView">View containing the Class table.</param>
        /// <param name="ProgIdView">View containing the ProgId table.</param>
        /// <returns></returns>
        private bool LoadAssemblies(Database Database, Type InstallerType,
            Object InstallerObject, out View MsiAssemblyView,
            out View MsiAssemblyNameView, out View ClassView,
            out View ProgIdView) {
            MsiAssemblyView = Database.OpenView("SELECT * FROM `MsiAssembly`");
            MsiAssemblyNameView = Database.OpenView("SELECT * FROM `MsiAssemblyName`");
            ClassView = Database.OpenView("SELECT * FROM `Class`");
            ProgIdView = Database.OpenView("SELECT * FROM `ProgId`");

            return true;
        }

        /// <summary>
        /// Loads records for the RegLocator table
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadRegLocator(Database Database, Type InstallerType,
            Object InstallerObject) {
            // Add properties from Task definition
            if (msi.search != null) {
                if (Verbose) {
                    Log(Level.Info, LogPrefix + "Adding Locators:");
                }

                foreach (searchKey key in msi.search) {
                    switch (key.type.ToString()) {
                        case "registry":
                            // Select the "RegLocator" Table
                            View regLocatorView = Database.OpenView("SELECT * FROM `RegLocator`");

                            int rootKey = -1;
                            switch (key.root.ToString()) {
                                case "classes":
                                    rootKey = 0;
                                    break;
                                case "user":
                                    rootKey = 1;
                                    break;
                                case "machine":
                                    rootKey = 2;
                                    break;
                                case "users":
                                    rootKey = 3;
                                    break;
                            }

                            if (key.value != null) {
                                foreach (searchKeyValue value in key.value) {
                                    string signature = "SIG_" + value.setproperty;

                                    // Insert the signature to the RegLocator Table
                                    Record recRegLoc = (Record)InstallerType.InvokeMember(
                                        "CreateRecord",
                                        BindingFlags.InvokeMethod,
                                        null, InstallerObject,
                                        new object[] { 5 });

                                    recRegLoc.set_StringData(1, signature);
                                    recRegLoc.set_StringData(2, rootKey.ToString());
                                    recRegLoc.set_StringData(3, key.path);
                                    recRegLoc.set_StringData(4, value.name);
                                    // 2 represents msidbLocatorTypeRawValue
                                    recRegLoc.set_IntegerData(5, 2);
                                    regLocatorView.Modify(MsiViewModify.msiViewModifyMerge, recRegLoc);

                                    if (Verbose) {
                                        string path = GetDisplayablePath(key.path);
                                        Log(Level.Info, "\t" + key.path + @"#" + value.name);
                                    }
                                }
                            }
                            regLocatorView.Close();
                            regLocatorView = null;

                            break;
                        case "file":
                            break;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Loads records for the RegLocator table
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadAppSearch(Database Database, Type InstallerType,
            Object InstallerObject) {
            // Add properties from Task definition
            if (msi.search != null) {
                foreach (searchKey key in msi.search) {
                    switch (key.type.ToString()) {
                        case "registry":
                            // Select the "AppSearch" Table
                            View appSearchView = Database.OpenView("SELECT * FROM `AppSearch`");

                            if (key.value != null) {
                                foreach (searchKeyValue value in key.value) {
                                    string signature = "SIG_" + value.setproperty;

                                    // Insert the Property/Signature into AppSearch Table
                                    Record recAppSearch = (Record)InstallerType.InvokeMember(
                                        "CreateRecord",
                                        BindingFlags.InvokeMethod,
                                        null, InstallerObject,
                                        new object[] { 2 });

                                    recAppSearch.set_StringData(1, value.setproperty);
                                    recAppSearch.set_StringData(2, signature);

                                    appSearchView.Modify(MsiViewModify.msiViewModifyMerge, recAppSearch);
                                }
                            }
                            appSearchView.Close();
                            appSearchView = null;

                            break;
                        case "file":
                            break;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Loads records for the LaunchCondition table
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadLaunchCondition(Database Database, Type InstallerType,
            Object InstallerObject) {
            // Add properties from Task definition
            if (msi.launchconditions != null) {
                if (Verbose) {
                    Log(Level.Info, LogPrefix + "Adding Launch Conditions:");
                }

                // Open the Launch Condition Table
                View lcView = Database.OpenView("SELECT * FROM `LaunchCondition`");

                // Add binary data from Task definition
                foreach (MSILaunchCondition launchCondition in msi.launchconditions) {
                    if (Verbose) {
                        Log(Level.Info, "\t" + launchCondition.name);
                    }

                    // Insert the icon data
                    Record recLC = (Record)InstallerType.InvokeMember(
                        "CreateRecord",
                        BindingFlags.InvokeMethod,
                        null, InstallerObject,
                        new object[] { 2 });

                    recLC.set_StringData(1, launchCondition.condition);
                    recLC.set_StringData(2, launchCondition.description);
                    lcView.Modify(MsiViewModify.msiViewModifyMerge, recLC);
                }

                lcView.Close();
                lcView = null;
            }
            return true;
        }


        /// <summary>
        /// Loads records for the Icon table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadIcon(Database Database, Type InstallerType,
            Object InstallerObject) {
            if (msi.icons != null) {

                // Open the Icon Table
                View iconView = Database.OpenView("SELECT * FROM `Icon`");

                if (Verbose) {
                    Log(Level.Info, LogPrefix + "Adding Icon Data:");
                }

                // Add binary data from Task definition
                foreach (MSIIcon icon in msi.icons) {
                    if (Verbose) {
                        Log(Level.Info, "\t" + Path.Combine(Project.BaseDirectory, icon.value));
                    }

                    if (File.Exists(Path.Combine(Project.BaseDirectory, icon.value))) {
                        // Insert the icon data
                        Record recIcon = (Record)InstallerType.InvokeMember(
                            "CreateRecord",
                            BindingFlags.InvokeMethod,
                            null, InstallerObject,
                            new object[] { 2 });

                        recIcon.set_StringData(1, icon.name);
                        recIcon.SetStream(2, Path.Combine(Project.BaseDirectory, icon.value));
                        iconView.Modify(MsiViewModify.msiViewModifyMerge, recIcon);
                    } else {
                        Log(Level.Error, LogPrefix +
                            "ERROR: Unable to open file:\n\n\t" +
                            Path.Combine(Project.BaseDirectory, icon.value) + "\n\n");

                        iconView.Close();
                        iconView = null;
                        return false;
                    }
                }

                iconView.Close();
                iconView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the Shortcut table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadShortcut(Database Database, Type InstallerType,
            Object InstallerObject) {
            // Add properties from Task definition
            if (msi.shortcuts != null) {
                if (Verbose) {
                    Log(Level.Info, LogPrefix + "Adding Shortcuts:");
                }

                View shortcutView = Database.OpenView("SELECT * FROM `Shortcut`");

                foreach (MSIShortcut shortcut in msi.shortcuts) {
                    if (Verbose) {
                        Log(Level.Info, "\t" + shortcut.name);
                    }

                    // Insert the record into the table
                    Record shortcutRec = (Record)InstallerType.InvokeMember(
                        "CreateRecord",
                        BindingFlags.InvokeMethod,
                        null, InstallerObject,
                        new object[] { 12 });

                    shortcutRec.set_StringData(1, shortcut.name);
                    shortcutRec.set_StringData(2, shortcut.directory);
                    shortcutRec.set_StringData(3, shortcut.filename);
                    shortcutRec.set_StringData(4, shortcut.component);
                    shortcutRec.set_StringData(5, shortcut.target);
                    shortcutRec.set_StringData(6, shortcut.arguments);
                    shortcutRec.set_StringData(7, shortcut.description);
                    shortcutRec.set_StringData(8, shortcut.hotkey);
                    shortcutRec.set_StringData(9, shortcut.icon);
                    shortcutRec.set_IntegerData(10, shortcut.iconindex);
                    shortcutRec.set_IntegerData(11, shortcut.showcmd);
                    shortcutRec.set_StringData(12, shortcut.wkdir);

                    shortcutView.Modify(MsiViewModify.msiViewModifyMerge, shortcutRec);
                }
                shortcutView.Close();
                shortcutView = null;
            }
            return true;

        }

        /// <summary>
        /// Adds custom table(s) to the msi database
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool AddTables(Database Database, Type InstallerType,
            Object InstallerObject) {
            // Add properties from Task definition
            if (msi.tables != null) {
                if (Verbose) {
                    Log(Level.Info, LogPrefix + "Adding Tables:");
                }

                foreach (MSITable table in msi.tables) {
                    if (Verbose) {
                        Log(Level.Info, "\t" + table.name);
                    }

                    bool tableExists = true;
                    try {
                        View tableView = Database.OpenView("SELECT * FROM `" + table.name + "`");

                        if (Verbose) {
                            Log(Level.Info, "\t\tTable exists.. skipping");
                        }
                        tableExists = true;
                        tableView.Close();
                        tableView = null;
                    }
                    catch (Exception) {
                        tableExists = false;
                    }

                    if (!tableExists) {
                        if (Verbose) {
                            Log(Level.Info, "\t\tAdding table structure...");
                        }

                        View validationView = Database.OpenView("SELECT * FROM `_Validation`");

                        string tableStructureColumns = "";
                        string tableStructureColumnTypes = "";
                        string tableStructureKeys = table.name;
                        bool firstColumn = true;

                        ArrayList columnList = new ArrayList();

                        foreach (MSITableColumn column in table.columns) {
                            // Add this column to the column list
                            MSIRowColumnData currentColumn = new MSIRowColumnData();

                            currentColumn.name = column.name;
                            currentColumn.id = columnList.Count;

                            Record recValidation = (Record)InstallerType.InvokeMember(
                                "CreateRecord",
                                BindingFlags.InvokeMethod,
                                null, InstallerObject,
                                new object[] { 10 });

                            recValidation.set_StringData(1, table.name);
                            recValidation.set_StringData(2, column.name);
                            if (column.nullable) {
                                if (column.key) {
                                    recValidation.set_StringData(3, "@");
                                }
                                else {
                                    recValidation.set_StringData(3, "Y");
                                }
                            }
                            else {
                                recValidation.set_StringData(3, "N");
                            }
                            if (column.minvalueSpecified)
                                recValidation.set_IntegerData(4, column.minvalue);
                            if (column.maxvalueSpecified)
                                recValidation.set_IntegerData(5, column.maxvalue);
                            recValidation.set_StringData(6, column.keytable);
                            if (column.keycolumnSpecified)
                                recValidation.set_IntegerData(7, column.keycolumn);


                            if (!firstColumn) {
                                tableStructureColumns += "\t";
                                tableStructureColumnTypes += "\t";
                            }
                            else
                                firstColumn = false;

                            tableStructureColumns += column.name;

                            switch(column.category.ToString()) {
                                case "Text":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "S0";
                                        }
                                        else {
                                            tableStructureColumnTypes += "s0";
                                        }
                                    }
                                    recValidation.set_StringData(8, "Text");
                                    currentColumn.type = "string";
                                    break;
                                case "UpperCase":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "S72";
                                        }
                                        else {
                                            tableStructureColumnTypes += "s72";
                                        }
                                    }
                                    recValidation.set_StringData(8, "UpperCase");
                                    currentColumn.type = "string";
                                    break;
                                case "LowerCase":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "S72";
                                        }
                                        else {
                                            tableStructureColumnTypes += "s72";
                                        }
                                    }
                                    recValidation.set_StringData(8, "LowerCase");
                                    currentColumn.type = "string";
                                    break;
                                case "Integer":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "I2";
                                        }
                                        else {
                                            tableStructureColumnTypes += "i2";
                                        }
                                    }
                                    if (!column.minvalueSpecified)
                                        recValidation.set_IntegerData(4, -32767);
                                    if (!column.maxvalueSpecified)
                                        recValidation.set_IntegerData(5, 32767);
                                    recValidation.set_StringData(8, null);
                                    currentColumn.type = "int";
                                    break;
                                case "DoubleInteger":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "I4";
                                        }
                                        else {
                                            tableStructureColumnTypes += "i4";
                                        }
                                    }
                                    if (!column.minvalueSpecified)
                                        recValidation.set_IntegerData(4, -2147483647);
                                    if (!column.maxvalueSpecified)
                                        recValidation.set_IntegerData(5, 2147483647);
                                    recValidation.set_StringData(8, null);
                                    currentColumn.type = "int";
                                    break;
                                case "Time/Date":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "I4";
                                        }
                                        else {
                                            tableStructureColumnTypes += "i4";
                                        }
                                    }
                                    if (!column.minvalueSpecified)
                                        recValidation.set_IntegerData(4, 0);
                                    if (!column.maxvalueSpecified)
                                        recValidation.set_IntegerData(5, 2147483647);
                                    recValidation.set_StringData(8, null);
                                    currentColumn.type = "int";
                                    break;
                                case "Identifier":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "S72";
                                        }
                                        else {
                                            tableStructureColumnTypes += "s72";
                                        }
                                    }
                                    recValidation.set_StringData(8, "Identifier");
                                    currentColumn.type = "string";
                                    break;
                                case "Property":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "S72";
                                        }
                                        else {
                                            tableStructureColumnTypes += "s72";
                                        }
                                    }
                                    recValidation.set_StringData(8, "Property");
                                    currentColumn.type = "string";
                                    break;
                                case "Filename":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "S255";
                                        }
                                        else {
                                            tableStructureColumnTypes += "s255";
                                        }
                                    }
                                    recValidation.set_StringData(8, "Filename");
                                    currentColumn.type = "string";
                                    break;
                                case "WildCardFilename":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "L0";
                                        }
                                        else {
                                            tableStructureColumnTypes += "l0";
                                        }
                                    }
                                    recValidation.set_StringData(8, "WildCardFilename");
                                    currentColumn.type = "string";
                                    break;
                                case "Path":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "S255";
                                        }
                                        else {
                                            tableStructureColumnTypes += "s255";
                                        }
                                    }
                                    recValidation.set_StringData(8, "Path");
                                    currentColumn.type = "string";
                                    break;
                                case "Paths":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "S255";
                                        }
                                        else {
                                            tableStructureColumnTypes += "s255";
                                        }
                                    }
                                    recValidation.set_StringData(8, "Paths");
                                    currentColumn.type = "string";
                                    break;
                                case "AnyPath":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "S255";
                                        }
                                        else {
                                            tableStructureColumnTypes += "s255";
                                        }
                                    }
                                    recValidation.set_StringData(8, "AnyPath");
                                    currentColumn.type = "string";
                                    break;
                                case "DefaultDir":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "L255";
                                        }
                                        else {
                                            tableStructureColumnTypes += "l255";
                                        }
                                    }
                                    recValidation.set_StringData(8, "DefaultDir");
                                    currentColumn.type = "string";
                                    break;
                                case "RegPath":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "L255";
                                        }
                                        else {
                                            tableStructureColumnTypes += "l255";
                                        }
                                    }
                                    recValidation.set_StringData(8, "RegPath");
                                    currentColumn.type = "string";
                                    break;
                                case "Formatted":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "S255";
                                        }
                                        else {
                                            tableStructureColumnTypes += "s255";
                                        }
                                    }
                                    recValidation.set_StringData(8, "Formatted");
                                    currentColumn.type = "string";
                                    break;
                                case "Template":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "L0";
                                        }
                                        else {
                                            tableStructureColumnTypes += "l0";
                                        }
                                    }
                                    recValidation.set_StringData(8, "Template");
                                    currentColumn.type = "string";
                                    break;
                                case "Condition":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "S255";
                                        }
                                        else {
                                            tableStructureColumnTypes += "s255";
                                        }
                                    }
                                    recValidation.set_StringData(8, "Condition");
                                    currentColumn.type = "string";
                                    break;
                                case "GUID":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "S38";
                                        }
                                        else {
                                            tableStructureColumnTypes += "s38";
                                        }
                                    }
                                    recValidation.set_StringData(8, "GUID");
                                    currentColumn.type = "string";
                                    break;
                                case "Version":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "S32";
                                        }
                                        else {
                                            tableStructureColumnTypes += "s32";
                                        }
                                    }
                                    recValidation.set_StringData(8, "Version");
                                    currentColumn.type = "string";
                                    break;
                                case "Language":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "S255";
                                        }
                                        else {
                                            tableStructureColumnTypes += "s255";
                                        }
                                    }
                                    recValidation.set_StringData(8, "Language");
                                    currentColumn.type = "string";
                                    break;
                                case "Binary":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "V0";
                                        }
                                        else {
                                            tableStructureColumnTypes += "v0";
                                        }
                                    }
                                    recValidation.set_StringData(8, "Binary");
                                    currentColumn.type = "binary";
                                    break;
                                case "CustomSource":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "S72";
                                        }
                                        else {
                                            tableStructureColumnTypes += "s72";
                                        }
                                    }
                                    recValidation.set_StringData(8, "CustomSource");
                                    currentColumn.type = "string";
                                    break;
                                case "Cabinet":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "S255";
                                        }
                                        else {
                                            tableStructureColumnTypes += "s255";
                                        }
                                    }
                                    recValidation.set_StringData(8, "Cabinet");
                                    currentColumn.type = "string";
                                    break;
                                case "Shortcut":
                                    if (column.type == null || column.type == "") {
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "S72";
                                        }
                                        else {
                                            tableStructureColumnTypes += "s72";
                                        }
                                    }
                                    recValidation.set_StringData(8, "Shortcut");
                                    currentColumn.type = "string";
                                    break;
                                default:
                                    if (column.type == null || column.type == "") {
                                        if (Verbose) {
                                            Log(Level.Info, " ");
                                            Log(Level.Info, LogPrefix + "Must specify a valid category or type.  Defaulting to category type: s0");
                                        }
                                        if (column.nullable) {
                                            tableStructureColumnTypes += "S0";
                                        }
                                        else {
                                            tableStructureColumnTypes += "s0";
                                        }
                                        currentColumn.type = "string";
                                    }
                                    break;
                            }
                            if (column.type != null) {
                                tableStructureColumnTypes += column.type;
                                if (column.type.ToString().ToLower().StartsWith("i"))
                                    currentColumn.type = "int";
                                else if(column.type.ToString().ToLower().StartsWith("v"))
                                    currentColumn.type = "binary";
                                else
                                    currentColumn.type = "string";
                            }

                            recValidation.set_StringData(9, column.set);
                            recValidation.set_StringData(10, column.description);

                            if (column.key)
                                tableStructureKeys += "\t" + column.name;

                            validationView.Modify(MsiViewModify.msiViewModifyMerge, recValidation);

                            columnList.Add(currentColumn);

                        }

                        validationView.Close();
                        validationView = null;
                        
                        // Create temp file.  Dump table structure contents into the file
                        // Then import the file.
                        string tableStructureContents = tableStructureColumns + "\n" + tableStructureColumnTypes + "\n" + tableStructureKeys + "\n";
                        string tempFileName = "85E99F65_1B01_4add_8835_EB2C9DA4E8BF.idt";
                        string fullTempFileName = Path.Combine(Path.Combine(Project.BaseDirectory, msi.sourcedir), tempFileName);
                        FileStream tableStream = null;
                        try {
                            tableStream = File.Create(fullTempFileName);
                            StreamWriter writer = new StreamWriter(tableStream);
                            writer.Write(tableStructureContents);
                            writer.Flush();
                        }
                        finally {
                            tableStream.Close();
                        }

                        try {
                            Database.Import(Path.GetFullPath(Path.Combine(Project.BaseDirectory, msi.sourcedir)), tempFileName);
                        }
                        catch (Exception ae) {
                            Log(Level.Error, LogPrefix + "ERROR: Temporary table file\n (" + Path.GetFullPath(Path.Combine(Path.Combine(Project.BaseDirectory, msi.sourcedir), tempFileName)) + ") is not valid:\n" +
                                ae.ToString());
                        }
                        File.Delete(fullTempFileName);

                        if (Verbose) {
                            Log(Level.Info, "Done");
                        }

                        if (table.rows != null)
                            AddTableData(Database, InstallerType, InstallerObject, table.name, table, columnList);

                    }
                }
            }
            return true;

        }


        /// <summary>
        /// Adds table data to the msi database table structure
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <param name="currentTable">The current table name</param>
        /// <param name="table">Xml node representing the current table</param>
        /// <param name="columnList">List of column objects for the current table (Containing: column name, id, type).</param>
        /// <returns>True if successful.</returns>
        private bool AddTableData(Database Database, Type InstallerType,
            Object InstallerObject, string currentTable, MSITable table, ArrayList columnList) {

            if (Verbose) {
                Log(Level.Info, "\t\tAdding table data...");
            }
            View tableView = Database.OpenView("SELECT * FROM `" + currentTable + "`");

            foreach (MSITableRow row in table.rows) {
                Record newRec = (Record)InstallerType.InvokeMember(
                    "CreateRecord",
                    BindingFlags.InvokeMethod,
                    null, InstallerObject,
                    new object[] { columnList.Count });
                try {
                    // Go through each element defining row data
                    foreach(MSITableRowColumnData columnData in row.columns) {
                        // Create the record and add it
                        // Check to see if the current element equals a specified column.
                        foreach (MSIRowColumnData columnInfo in columnList) {
                            if (columnInfo.name == columnData.name) {
                                if (columnInfo.type == "int") {
                                    newRec.set_IntegerData((columnInfo.id + 1), Convert.ToInt32(columnData.value));
                                }
                                else if (columnInfo.type == "binary") {
                                    newRec.SetStream((columnInfo.id + 1), columnData.value);
                                }
                                else if (columnInfo.type == "guid") {
                                    // Guids must have all uppercase letters
                                    newRec.set_StringData((columnInfo.id + 1), columnData.value.ToUpper());
                                }
                                else {
                                    newRec.set_StringData((columnInfo.id + 1), columnData.value);
                                }
                                break;
                            }
                        }
                    }
                    tableView.Modify(MsiViewModify.msiViewModifyMerge, newRec);
                }
                catch (Exception e) {
                    Log(Level.Info, LogPrefix + "Incorrect row data format.\n\n" + e.ToString());
                }
            }
            tableView.Close();
            tableView = null;

            if (Verbose) {
                Log(Level.Info, "Done");
            }
            return true;
        }


        /// <summary>
        /// Sets the sequence number of files to match their
        /// storage order in the cabinet file, after some
        /// files have had their filenames changed to go in
        /// their own component.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="LastSequence">The last file's sequence number.</param>
        /// <returns>True if successful</returns>
        private bool ReorderFiles(Database Database, ref int LastSequence) {
            string curPath = Path.Combine(Project.BaseDirectory, msi.sourcedir);
            string curTempPath = Path.Combine(curPath, "Temp");

            try {
                string[] curFileNames = Directory.GetFiles(curTempPath, "*.*");
                LastSequence = 1;

                foreach (string curDirFileName in curFileNames) {
                    View curFileView = Database.OpenView(
                        "SELECT * FROM `File` WHERE `File`='" +
                        Path.GetFileName(curDirFileName) + "'");

                    if (curFileView != null) {
                        curFileView.Execute(null);
                        Record recCurFile = curFileView.Fetch();

                        if (recCurFile != null) {
                            recCurFile.set_StringData(8, LastSequence.ToString());
                            curFileView.Modify(MsiViewModify.msiViewModifyUpdate, recCurFile);

                            LastSequence++;
                        }
                        else {
                            Log(Level.Info, LogPrefix + "File " +
                                Path.GetFileName(curDirFileName) +
                                " not found during reordering.");

                            curFileView.Close();
                            curFileView = null;

                            return false;
                        }
                    }

                    curFileView.Close();
                    curFileView = null;
                }
            }
            catch (Exception e) {
                // There are no files added to the msi.  (Msi containing only merge modules?)
                Log(Level.Info, LogPrefix + "NOTE: No files found to add to MSI (Does not include MSM files).\n" + e.Message + "\n" + e.StackTrace);
            }
            return true;
        }

        /// <summary>
        /// Creates a .cab file with all source files included.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool CreateCabFile(Database Database, Type InstallerType, Object InstallerObject) {
            Log(Level.Info, LogPrefix + "Compressing Files...");

            // Create the CabFile
            ProcessStartInfo processInfo = new ProcessStartInfo();

            string shortCabDir = GetShortDir(Path.Combine(Project.BaseDirectory, msi.sourcedir));
            string cabFile = shortCabDir + @"\" + Path.GetFileNameWithoutExtension(msi.output) + ".cab";
            string tempDir = Path.Combine(msi.sourcedir, "Temp");
            if (tempDir.ToLower().StartsWith(Project.BaseDirectory.ToLower())) {
                tempDir = tempDir.Substring(Project.BaseDirectory.Length+1);
            }
            processInfo.Arguments = "-p -r -P " + tempDir + @"\ N " + cabFile + " " + tempDir + @"\*";

            processInfo.CreateNoWindow = false;
            processInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processInfo.WorkingDirectory = Project.BaseDirectory;
            processInfo.FileName = "cabarc";

            Process process = new Process();
            process.StartInfo = processInfo;
            process.EnableRaisingEvents = true;

            try {
                process.Start();
            }
            catch (Exception e) {
                Log(Level.Error, LogPrefix + "ERROR: cabarc.exe is not in your path! \n" + e.ToString());
                return false;
            }

            try {
                process.WaitForExit();
            }
            catch (Exception e) {
                Log(Level.Error, "");
                Log(Level.Error, "Error creating cab file: " + e.Message);
                return false;
            }

            if (process.ExitCode != 0) {
                Log(Level.Error, "");
                Log(Level.Error, "Error creating cab file, application returned error " +
                    process.ExitCode + ".");
                return false;
            }

            if (!process.HasExited) {
                Log(Level.Info,"" );
                Log(Level.Info, "Killing the cabarc process.");
                process.Kill();
            }
            process = null;
            processInfo = null;

            Log(Level.Info, "Done.");

            if (File.Exists(cabFile)) {
                View cabView = Database.OpenView("SELECT * FROM `_Streams`");
                if (Verbose) {
                    Log(Level.Info, LogPrefix + "Storing Cabinet in MSI Database...");
                }

                Record cabRecord = (Record)InstallerType.InvokeMember(
                    "CreateRecord",
                    BindingFlags.InvokeMethod,
                    null, InstallerObject,
                    new object[] { 2 });

                cabRecord.set_StringData(1, Path.GetFileName(cabFile));
                cabRecord.SetStream(2, cabFile);

                cabView.Modify(MsiViewModify.msiViewModifyMerge, cabRecord);
                cabView.Close();
                cabView = null;

            }
            else {
                Log(Level.Error, LogPrefix +
                    "ERROR: Unable to open Cabinet file:\n\n\t" +
                    cabFile + "\n\n");
                return false;
            }
            return true;
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern int GetShortPathName(string LongPath, StringBuilder ShortPath, int BufferSize);

        /// <summary>
        /// Retrieves a DOS 8.3 filename for a file.
        /// </summary>
        /// <param name="LongFile">The file to shorten.</param>
        /// <returns>The new shortened file.</returns>
        private string GetShortFile(string LongFile) {
            if (LongFile.Length <= 8) {
                return LongFile;
            }

            StringBuilder shortPath = new StringBuilder(255);
            int result = GetShortPathName(LongFile, shortPath, shortPath.Capacity);
            return Path.GetFileName(shortPath.ToString());
        }

        /// <summary>
        /// Retrieves a DOS 8.3 filename for a directory.
        /// </summary>
        /// <param name="LongPath">The path to shorten.</param>
        /// <returns>The new shortened path.</returns>
        private string GetShortPath(string LongPath) {
            if (LongPath.Length <= 8) {
                return LongPath;
            }

            StringBuilder shortPath = new StringBuilder(255);
            int result = GetShortPathName(LongPath, shortPath, shortPath.Capacity);

            Uri shortPathUri = null;
            try {
                shortPathUri = new Uri("file://" + shortPath.ToString());
            }
            catch (Exception) {
                Log(Level.Error, LogPrefix + "ERROR: Directory " +
                    LongPath + " not found.");
                return "MsiTaskPathNotFound";
            }

            string[] shortPathSegments = shortPathUri.Segments;
            if (shortPathSegments.Length == 0) {
                return LongPath;
            }
            if (shortPathSegments.Length == 1) {
                return shortPathSegments[0];
            }
            return shortPathSegments[shortPathSegments.Length-1];
        }

        /// <summary>
        /// Retrieves a DOS 8.3 filename for a complete directory.
        /// </summary>
        /// <param name="LongPath">The path to shorten.</param>
        /// <returns>The new shortened path.</returns>
        private string GetShortDir(string LongPath) {
            if (LongPath.Length <= 8) {
                return LongPath;
            }

            StringBuilder shortPath = new StringBuilder(255);
            int result = GetShortPathName(LongPath, shortPath, shortPath.Capacity);

            Uri shortPathUri = null;
            try {
                shortPathUri = new Uri("file://" + shortPath.ToString());
            }
            catch (Exception) {
                Log(Level.Error, LogPrefix + "ERROR: Directory " +
                    LongPath + " not found.");
                return "MsiTaskPathNotFound";
            }

            return shortPath.ToString();
        }

        /// <summary>
        /// Retrieves the relative path of a file based on
        /// the component it belongs to and its entry in
        /// the MSI directory table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <param name="Name">The Name of the Folder</param>
        /// <param name="Parent">The Parent of the Folder</param>
        /// <param name="Default">The Relative Filesystem Path of the Folder</param>
        /// <param name="Path">The Path to the Folder from previous calls.</param>
        /// <param name="DirectoryView">The MSI database view.</param>
        private void GetRelativePath(
            Database Database,
            Type InstallerType,
            Object InstallerObject,
            string Name,
            string Parent,
            string Default,
            StringBuilder Path,
            View DirectoryView) {
            if (Name == "TARGETDIR") {
                return;
            }

            for (int i = 0; i < commonFolderNames.Length; i++) {
                if (Name == commonFolderNames[i]) {
                    return;
                }
            }

            ArrayList directoryList = new ArrayList();
            foreach(MSIRootDirectory directory in msi.directories) {
                directoryList.Add(directory);
            }

            foreach (property property in msi.properties) {
                if (Name == property.name) {
                    MSIDirectory directory = FindDirectory(Name);
                    if (directory == null) {
                        MSIRootDirectory propDirectory = new MSIRootDirectory();
                        propDirectory.name = Name;
                        propDirectory.root = "TARGETDIR";
                        propDirectory.foldername = ".";

                        directoryList.Add(propDirectory);

                        MSIRootDirectory[] rootDirs = new MSIRootDirectory[directoryList.Count];
                        directoryList.CopyTo(rootDirs);

                        msi.directories = rootDirs;
                    }

                    return;
                }
            }

            if (Path.Length > 0) {
                Path.Insert(0, @"\");
            }

            Path.Insert(0, Default);
            if (Parent != null) {
                MSIDirectory PathInfo = FindDirectory(Parent);

                if (PathInfo == null) {
                    foreach (property property in msi.properties) {
                        if (Parent == property.name) {
                            MSIRootDirectory directory = new MSIRootDirectory();
                            directory.name = Parent;
                            directory.root = "TARGETDIR";
                            directory.foldername = ".";

                            directoryList.Add(directory);

                            MSIRootDirectory[] rootDirs = new MSIRootDirectory[directoryList.Count];
                            directoryList.CopyTo(rootDirs);

                            msi.directories = rootDirs;

                            // Insert the Directory that is a Property
                            Record recDir = (Record)InstallerType.InvokeMember(
                                "CreateRecord",
                                BindingFlags.InvokeMethod,
                                null, InstallerObject, new object[] { 3 });

                            recDir.set_StringData(1, Parent);
                            recDir.set_StringData(2, "TARGETDIR");
                            recDir.set_StringData(3, ".");

                            DirectoryView.Modify(MsiViewModify.msiViewModifyMerge, recDir);

                            PathInfo = directory;

                            break;
                        }
                    }
                }

                string newParent = null;
                if (PathInfo is MSIRootDirectory) {
                    newParent = ((MSIRootDirectory)PathInfo).root;
                }
                else {
                    newParent = FindParent(Parent);
                }

                GetRelativePath(Database, InstallerType, InstallerObject,
                    Parent, newParent,
                    PathInfo.foldername, Path, DirectoryView);
            }
        }

        /// <summary>
        /// Recursively expands properties of all attributes of
        /// a nodelist and their children.
        /// </summary>
        /// <param name="Nodes">The nodes to recurse.</param>
        void ExpandPropertiesInNodes(XmlNodeList Nodes) {
            foreach (XmlNode node in Nodes) {
                if (node.ChildNodes != null) {
                    ExpandPropertiesInNodes(node.ChildNodes);
                    if (node.Attributes != null) {
                        foreach (XmlAttribute attr in node.Attributes) {
                            attr.Value = Properties.ExpandProperties(attr.Value, Location);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Converts the Byte array in a public key
        /// token of an assembly to a string MSI expects.
        /// </summary>
        /// <param name="ByteArray">The array of bytes.</param>
        /// <returns>The string containing the array.</returns>
        private string ByteArrayToString(Byte[] ByteArray) {
            if ((ByteArray == null) || (ByteArray.Length == 0))
                return "";
            StringBuilder sb = new StringBuilder ();
            sb.Append (ByteArray[0].ToString("x2"));
            for (int i = 1; i < ByteArray.Length; i++) {
                sb.Append(ByteArray[i].ToString("x2"));
            }
            return sb.ToString().ToUpper();
        }

        [DllImport("oleaut32.dll", CharSet=CharSet.Auto)]
        private static extern int LoadTypeLib(string TypeLibFileName, ref IntPtr pTypeLib);

        /// <summary>
        /// Loads TypeLibs for the TypeLib table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <param name="RegistryView">View containing the Registry Table.</param>
        /// <returns>True if successful.</returns>
        private bool LoadTypeLibs(Database Database, Type InstallerType, object InstallerObject, View RegistryView) {
            // Open the "TypeLib" Table
            View typeLibView = Database.OpenView("SELECT * FROM `TypeLib`");

            string runtimeVer = Environment.Version.ToString(4);

            for (int i = 0; i < typeLibRecords.Count; i++) {
                TypeLibRecord tlbRecord = (TypeLibRecord)typeLibRecords[i];

                IntPtr pTypeLib = new IntPtr(0);
                int result = LoadTypeLib(tlbRecord.TypeLibFileName, ref pTypeLib);
                if (result == 0) {
                    UCOMITypeLib typeLib = (UCOMITypeLib)Marshal.GetTypedObjectForIUnknown(
                        pTypeLib, typeof(UCOMITypeLib));
                    if (typeLib != null) {
                        int helpContextId;
                        string name, docString, helpFile;

                        typeLib.GetDocumentation(
                            -1, out name, out docString,
                            out helpContextId, out helpFile);

                        IntPtr pTypeLibAttr = new IntPtr(0);
                        typeLib.GetLibAttr(out pTypeLibAttr);

                        TYPELIBATTR typeLibAttr = (TYPELIBATTR)Marshal.PtrToStructure(pTypeLibAttr, typeof(TYPELIBATTR));

                        string tlbCompName = (string)typeLibComponents[Path.GetFileName(tlbRecord.TypeLibFileName)];

                        Record recTypeLib = (Record)InstallerType.InvokeMember(
                            "CreateRecord",
                            BindingFlags.InvokeMethod,
                            null, InstallerObject,
                            new object[] { 8 });

                        recTypeLib.set_StringData(1, "{"+typeLibAttr.guid.ToString().ToUpper()+"}");
                        recTypeLib.set_IntegerData(2, Marshal.GetTypeLibLcid(typeLib));
                        recTypeLib.set_StringData(3, tlbCompName);
                        recTypeLib.set_IntegerData(4, 256);
                        recTypeLib.set_StringData(5, docString == null ? name : docString);
                        recTypeLib.set_StringData(7, tlbRecord.FeatureName);
                        recTypeLib.set_IntegerData(8, 0);

                        typeLib.ReleaseTLibAttr(pTypeLibAttr);

                        typeLibView.Modify(MsiViewModify.msiViewModifyMerge, recTypeLib);

                        // If a .NET type library wrapper for an assembly
                        if (tlbRecord.AssemblyName != null) {
                            // Get all the types defined in the typelibrary
                            // that are not marked "noncreatable"

                            int typeCount = typeLib.GetTypeInfoCount();
                            for (int j = 0; j < typeCount; j++) {
                                UCOMITypeInfo typeInfo = null;
                                typeLib.GetTypeInfo(j, out typeInfo);

                                if (typeInfo != null) {
                                    IntPtr pTypeAttr = new IntPtr(0);
                                    typeInfo.GetTypeAttr(out pTypeAttr);

                                    TYPEATTR typeAttr = (TYPEATTR)Marshal.PtrToStructure(pTypeAttr, typeof(TYPEATTR));

                                    if (typeAttr.typekind == TYPEKIND.TKIND_COCLASS
                                        && typeAttr.wTypeFlags == TYPEFLAGS.TYPEFLAG_FCANCREATE) {
                                        string clsid = "{" + typeAttr.guid.ToString().ToUpper() + "}";

                                        if (typeInfo is UCOMITypeInfo2) {
                                            UCOMITypeInfo2 typeInfo2 = (UCOMITypeInfo2)typeInfo;
                                            if (typeInfo2 != null) {
                                                object custData = new object();
                                                Guid g = new Guid("0F21F359-AB84-41E8-9A78-36D110E6D2F9");
                                                typeInfo2.GetCustData(ref g, out custData);

                                                if (custData != null) {
                                                    string className = (string)custData;

                                                    // Insert the Class
                                                    Record recRegTlbRec = (Record)InstallerType.InvokeMember(
                                                        "CreateRecord",
                                                        BindingFlags.InvokeMethod,
                                                        null, InstallerObject,
                                                        new object[] { 6 });

                                                    recRegTlbRec.set_StringData(1,
                                                        "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
                                                    recRegTlbRec.set_IntegerData(2, 0);
                                                    recRegTlbRec.set_StringData(3,
                                                        @"CLSID\" + clsid +
                                                        @"\InprocServer32");
                                                    recRegTlbRec.set_StringData(4, "Class");
                                                    recRegTlbRec.set_StringData(5, className);
                                                    recRegTlbRec.set_StringData(6, tlbRecord.AssemblyComponent);

                                                    RegistryView.Modify(MsiViewModify.msiViewModifyMerge, recRegTlbRec);

                                                    recRegTlbRec.set_StringData(1,
                                                        "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
                                                    recRegTlbRec.set_StringData(4, "ThreadingModel");
                                                    recRegTlbRec.set_StringData(5, "Both");
                                                    RegistryView.Modify(MsiViewModify.msiViewModifyMerge, recRegTlbRec);

                                                    recRegTlbRec.set_StringData(1,
                                                        "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
                                                    recRegTlbRec.set_StringData(4, "RuntimeVersion");
                                                    recRegTlbRec.set_StringData(5, System.Environment.Version.ToString(3));
                                                    RegistryView.Modify(MsiViewModify.msiViewModifyMerge, recRegTlbRec);

                                                    recRegTlbRec.set_StringData(1,
                                                        "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
                                                    recRegTlbRec.set_StringData(4, "Assembly");
                                                    recRegTlbRec.set_StringData(5, tlbRecord.AssemblyName.FullName);
                                                    RegistryView.Modify(MsiViewModify.msiViewModifyMerge, recRegTlbRec);

                                                    recRegTlbRec.set_StringData(1,
                                                        "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
                                                    recRegTlbRec.set_StringData(3,
                                                        @"CLSID\" + clsid +
                                                        @"\Implemented Categories");
                                                    recRegTlbRec.set_StringData(4, "+");
                                                    recRegTlbRec.set_StringData(5, null);
                                                    RegistryView.Modify(MsiViewModify.msiViewModifyMerge, recRegTlbRec);

                                                    recRegTlbRec.set_StringData(1,
                                                        "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
                                                    recRegTlbRec.set_StringData(3,
                                                        @"CLSID\" + clsid +
                                                        @"\Implemented Categories\{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}");
                                                    recRegTlbRec.set_StringData(4, "+");
                                                    recRegTlbRec.set_StringData(5, null);
                                                    RegistryView.Modify(MsiViewModify.msiViewModifyMerge, recRegTlbRec);
                                                }
                                            }
                                        }
                                    }
                                    else if (typeAttr.typekind == TYPEKIND.TKIND_DISPATCH) {
                                        string iid = "{" + typeAttr.guid.ToString().ToUpper() + "}";

                                        string typeName, typeDocString, typeHelpFile;
                                        int typeHelpContextId;

                                        typeInfo.GetDocumentation(-1, out typeName,
                                            out typeDocString, out typeHelpContextId,
                                            out typeHelpFile);

                                        if (typeInfo is UCOMITypeInfo2) {
                                            UCOMITypeInfo2 typeInfo2 = (UCOMITypeInfo2)typeInfo;
                                            if (typeInfo2 != null) {
                                                object custData = new object();
                                                Guid g = new Guid("0F21F359-AB84-41E8-9A78-36D110E6D2F9");
                                                typeInfo2.GetCustData(ref g, out custData);

                                                if (custData != null) {
                                                    string className = (string)custData;

                                                    // Insert the Interface
                                                    Record recRegTlbRec = (Record)InstallerType.InvokeMember(
                                                        "CreateRecord",
                                                        BindingFlags.InvokeMethod,
                                                        null, InstallerObject,
                                                        new object[] { 6 });

                                                    string typeLibComponent = (string)typeLibComponents[Path.GetFileName(tlbRecord.TypeLibFileName)];

                                                    recRegTlbRec.set_StringData(1,
                                                        "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
                                                    recRegTlbRec.set_IntegerData(2, 0);
                                                    recRegTlbRec.set_StringData(3,
                                                        @"Interface\" + iid);
                                                    recRegTlbRec.set_StringData(4, null);
                                                    recRegTlbRec.set_StringData(5, typeName);
                                                    recRegTlbRec.set_StringData(6, typeLibComponent);
                                                    RegistryView.Modify(MsiViewModify.msiViewModifyMerge, recRegTlbRec);

                                                    recRegTlbRec.set_StringData(1,
                                                        "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
                                                    recRegTlbRec.set_StringData(3,
                                                        @"Interface\" + iid + @"\TypeLib");
                                                    recRegTlbRec.set_StringData(4, "Version");
                                                    recRegTlbRec.set_StringData(5, "1.0");
                                                    RegistryView.Modify(MsiViewModify.msiViewModifyMerge, recRegTlbRec);

                                                    recRegTlbRec.set_StringData(1,
                                                        "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
                                                    recRegTlbRec.set_StringData(4, null);
                                                    recRegTlbRec.set_StringData(5, "{"+typeLibAttr.guid.ToString().ToUpper()+"}");
                                                    RegistryView.Modify(MsiViewModify.msiViewModifyMerge, recRegTlbRec);

                                                    recRegTlbRec.set_StringData(1,
                                                        "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
                                                    recRegTlbRec.set_StringData(3,
                                                        @"Interface\" + iid + @"\ProxyStubClsid32");
                                                    recRegTlbRec.set_StringData(4, null);
                                                    recRegTlbRec.set_StringData(5, "{00020424-0000-0000-C000-000000000046}");
                                                    RegistryView.Modify(MsiViewModify.msiViewModifyMerge, recRegTlbRec);

                                                    recRegTlbRec.set_StringData(1,
                                                        "_" + Guid.NewGuid().ToString().Replace("-", null).ToUpper());
                                                    recRegTlbRec.set_StringData(3,
                                                        @"Interface\" + iid + @"\ProxyStubClsid");
                                                    recRegTlbRec.set_StringData(4, null);
                                                    recRegTlbRec.set_StringData(5, "{00020424-0000-0000-C000-000000000046}");
                                                    RegistryView.Modify(MsiViewModify.msiViewModifyMerge, recRegTlbRec);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            typeLibView.Close();
            typeLibView = null;

            return true;
        }

        /// <summary>
        /// Merges Merge Modules into the MSI Database.
        /// </summary>
        /// <param name="Database">The MSI Database.</param>
        /// <param name="TempPath">The path to temporary files.</param>
        /// <returns>True if successful.</returns>
        private bool LoadMergeModules(string Database, string TempPath) {
            // If <mergemodules>...</mergemodules> exists in the nant msi task

            if (msi.mergemodules != null) {
                MsmMergeClass mergeClass = new MsmMergeClass();

                int index = 1;

                if (Verbose) {
                    Log(Level.Info, LogPrefix + "Storing Merge Modules:");
                }

                if (!Directory.Exists(TempPath))
                    Directory.CreateDirectory(TempPath);

                // Merge module(s) assigned to a specific feature
                foreach (MSIMerge merge in msi.mergemodules) {
                    // Get each merge module file name assigned to this feature
                    NAntFileSet modules = merge.modules;

                    FileSet mergeSet = new FileSet();
                    mergeSet.Parent = this;
                    mergeSet.Project = Project;

                    XmlElement modulesElem = (XmlElement)((XmlElement)_xmlNode).SelectSingleNode(
                        "mergemodules/merge[@feature='" + merge.feature + "']/modules");

                    mergeSet.Initialize(modulesElem);

                    // Iterate each module assigned to this feature
                    foreach (string mergeModule in mergeSet.FileNames) {
                        if (Verbose) {
                            Log(Level.Info, "\t" + Path.GetFileName(mergeModule));
                        }

                        try {
                            // Open the merge module (by Filename)
                            mergeClass.OpenModule(mergeModule, 1033);

                            try {
                                mergeClass.OpenDatabase(Database);
                            }
                            catch (FileLoadException fle) {
                                Log(Level.Info, fle.Message + " " + fle.FileName + " " + fle.StackTrace);
                                return false;
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

                            Process process = new Process();

                            if (File.Exists(moduleCab)) {
                                // Extract the cabfile contents to a Temp directory
                                ProcessStartInfo processInfo = new ProcessStartInfo();

                                processInfo.Arguments = "-o X " +
                                    moduleCab + " " + Path.Combine(Project.BaseDirectory, Path.Combine(msi.sourcedir, @"Temp\"));

                                processInfo.CreateNoWindow = false;
                                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                processInfo.WorkingDirectory = msi.output;
                                processInfo.FileName = "cabarc";


                                process.StartInfo = processInfo;
                                process.EnableRaisingEvents = true;

                                process.Start();

                                try {
                                    process.WaitForExit();
                                }
                                catch (Exception e) {
                                    Log(Level.Info, "" );
                                    Log(Level.Info, "Error extracting merge module cab file: " + moduleCab);
                                    Log(Level.Info, "Error was: " + e.Message);

                                    File.Delete(moduleCab);
                                    return false;
                                }

                                if (process.ExitCode != 0) {
                                    Log(Level.Error, "");
                                    Log(Level.Error, "Error extracting merge module cab file: " + moduleCab);
                                    Log(Level.Error, "Application returned ERROR: " + process.ExitCode);

                                    File.Delete(moduleCab);
                                    return false;
                                }

                                File.Delete(moduleCab);
                            }
                        }
                        catch (Exception) {
                            Log(Level.Error, LogPrefix + "ERROR: cabarc.exe is not in your path");
                            Log(Level.Error, LogPrefix + "or file " + mergeModule + " is not found.");
                            return false;
                        }
                    }
                    mergeClass.CloseModule();
                    // Close and save the database
                    mergeClass.CloseDatabase(true);

                }
            }

            return true;
        }

        /// <summary>
        /// Loads records for the Binary table.  This table stores items
        /// such as bitmaps, animations, and icons. The binary table is
        /// also used to store data for custom actions.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadBinary(Database Database, Type InstallerType, Object InstallerObject) {
            if (msi.binaries != null) {

                // Open the Binary Table
                View binaryView = Database.OpenView("SELECT * FROM `Binary`");

                if (Verbose) {
                    Log(Level.Info, LogPrefix + "Adding Binary Data:");
                }

                // Add binary data from Task definition
                foreach (MSIBinary binary in msi.binaries) {
                    if (Verbose) {
                        Log(Level.Info, "\t" + Path.Combine(Project.BaseDirectory, binary.value));

                        int nameColSize = 50;

                        if (binary.name.Length > nameColSize) {
                            Log(Level.Warning, LogPrefix +
                                "WARNING: Binary key name longer than " + nameColSize + " characters:\n\tName: " +
                                binary.name + "\n\tLength: " + binary.name.Length.ToString());

                        }
                    }

                    if (File.Exists(Path.Combine(Project.BaseDirectory, binary.value))) {
                        // Insert the binary data
                        Record recBinary = (Record)InstallerType.InvokeMember(
                            "CreateRecord",
                            BindingFlags.InvokeMethod,
                            null, InstallerObject,
                            new object[] { 2 });

                        recBinary.set_StringData(1, binary.name);
                        recBinary.SetStream(2, Path.Combine(Project.BaseDirectory, binary.value));
                        binaryView.Modify(MsiViewModify.msiViewModifyMerge, recBinary);
                    }
                    else {
                        Log(Level.Error, LogPrefix +
                            "ERROR: Unable to open file:\n\n\t" +
                            Path.Combine(Project.BaseDirectory, binary.value) + "\n\n");

                        binaryView.Close();
                        binaryView = null;
                        return false;
                    }
                }

                binaryView.Close();
                binaryView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the Dialog table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadDialog(Database Database, Type InstallerType, Object InstallerObject) {
            if (msi.dialogs != null) {

                // Open the Dialog Table
                View dialogView = Database.OpenView("SELECT * FROM `Dialog`");

                if (Verbose) {
                    Log(Level.Info, LogPrefix + "Adding Dialogs:");
                }

                foreach (MSIDialog dialog in msi.dialogs) {
                    if (Verbose) {
                        Log(Level.Info, "\t" + dialog.name);
                    }

                    // Insert the dialog
                    Record recDialog = (Record)InstallerType.InvokeMember(
                        "CreateRecord",
                        BindingFlags.InvokeMethod,
                        null, InstallerObject,
                        new object[] { 10 });

                    recDialog.set_StringData(1, dialog.name);
                    recDialog.set_IntegerData(2, dialog.hcenter);
                    recDialog.set_IntegerData(3, dialog.vcenter);
                    recDialog.set_IntegerData(4, dialog.width);
                    recDialog.set_IntegerData(5, dialog.height);
                    recDialog.set_IntegerData(6, dialog.attr);
                    recDialog.set_StringData(7, dialog.title);
                    recDialog.set_StringData(8, dialog.firstcontrol);
                    recDialog.set_StringData(9, dialog.defaultcontrol);
                    recDialog.set_StringData(10, dialog.cancelcontrol);

                    dialogView.Modify(MsiViewModify.msiViewModifyMerge, recDialog);
                }

                dialogView.Close();
                dialogView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the Control table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadControl(Database Database, Type InstallerType, Object InstallerObject) {
            if (msi.controls != null) {
                if (Verbose) {
                    Log(Level.Info, LogPrefix + "Dialog Controls:");
                }

                View controlView;

                foreach (MSIControl control in msi.controls) {
                    if (control.remove) {
                        if (Verbose) {
                            Log(Level.Info, "\tRemoving: " + control.name);
                        }

                        // Open Control table
                        controlView = Database.OpenView("SELECT * FROM `Control` WHERE `Dialog_`='" + control.dialog + "' AND `Control`='" + control.name + "' AND `Type`='" + control.type + "' AND `X`=" + control.x + " AND `Y`=" + control.y + " AND `Width`=" + control.width + " AND `Height`=" + control.height + " AND `Attributes`=" + control.attr);
                        controlView.Execute(null);

                        try {
                            Record recControl = controlView.Fetch();
                            controlView.Modify(MsiViewModify.msiViewModifyDelete, recControl);
                        }
                        catch (Exception) {
                            Log(Level.Error, LogPrefix +
                                "ERROR: Control not found.\n\nSELECT * FROM `Control` WHERE `Dialog_`='" + control.dialog + "' AND `Control`='" + control.name + "' AND `Type`='" + control.type + "' AND `X`='" + control.x + "' AND `Y`='" + control.y + "' AND `Width`='" + control.width + "' AND `Height`='" + control.height + "' AND `Attributes`='" + control.attr + "'");
                            return false;
                        }
                        finally {
                            controlView.Close();
                            controlView = null;
                        }
                    }
                    else {
                        if (Verbose) {
                            Log(Level.Info, "\tAdding:   " + control.name);
                        }
                        // Open the Control Table
                        controlView = Database.OpenView("SELECT * FROM `Control`");

                        // Insert the control
                        Record recControl = (Record)InstallerType.InvokeMember(
                            "CreateRecord",
                            BindingFlags.InvokeMethod,
                            null, InstallerObject,
                            new object[] { 12 });

                        recControl.set_StringData(1, control.dialog);
                        recControl.set_StringData(2, control.name);
                        recControl.set_StringData(3, control.type);
                        recControl.set_IntegerData(4, control.x);
                        recControl.set_IntegerData(5, control.y);
                        recControl.set_IntegerData(6, control.width);
                        recControl.set_IntegerData(7, control.height);
                        recControl.set_IntegerData(8, control.attr);
                        recControl.set_StringData(9, control.property);
                        recControl.set_StringData(10, control.text);
                        recControl.set_StringData(11, control.nextcontrol);
                        recControl.set_StringData(12, control.help);

                        controlView.Modify(MsiViewModify.msiViewModifyMerge, recControl);

                        controlView.Close();
                        controlView = null;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Loads records for the ControlCondtion table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadControlCondition(Database Database, Type InstallerType, Object InstallerObject) {
            if (msi.controlconditions != null) {
                View controlConditionView;

                if (Verbose) {
                    Log(Level.Info, LogPrefix + "Adding Dialog Control Conditions For:");
                }

                foreach (MSIControlCondition controlCondition in msi.controlconditions) {
                    if (controlCondition.remove) {
                        if (Verbose) {
                            Log(Level.Info, "\tRemoving: " + controlCondition.control);
                        }

                        controlConditionView = Database.OpenView("SELECT * FROM `ControlCondition` WHERE `Dialog_`='" + controlCondition.dialog + "' AND `Control_`='" + controlCondition.control + "' AND `Action`='" + controlCondition.action + "' AND `Condition`='" + controlCondition.condition + "'");
                        controlConditionView.Execute(null);

                        try {
                            Record recControlCondition = controlConditionView.Fetch();
                            controlConditionView.Modify(MsiViewModify.msiViewModifyDelete, recControlCondition);
                        }
                        catch (Exception) {
                            Log(Level.Error, LogPrefix +
                                "ERROR: Control Condition not found.\n\nSELECT * FROM `ControlCondition` WHERE `Dialog_`='" + controlCondition.dialog + "' AND `Control_`='" + controlCondition.control + "' AND `Action`='" + controlCondition.action + "' AND `Condition`='" + controlCondition.condition + "'");
                            return false;
                        }
                        finally {
                            controlConditionView.Close();
                            controlConditionView = null;
                        }
                    }
                    else {
                        if (Verbose) {
                            Log(Level.Info, "\tAdding:   " + controlCondition.control);
                        }

                        controlConditionView = Database.OpenView("SELECT * FROM `ControlCondition`");

                        // Insert the condition
                        Record recControlCondition = (Record)InstallerType.InvokeMember(
                            "CreateRecord",
                            BindingFlags.InvokeMethod,
                            null, InstallerObject,
                            new object[] { 4 });

                        recControlCondition.set_StringData(1, controlCondition.dialog);
                        recControlCondition.set_StringData(2, controlCondition.control);
                        recControlCondition.set_StringData(3, controlCondition.action);
                        recControlCondition.set_StringData(4, controlCondition.condition);

                        controlConditionView.Modify(MsiViewModify.msiViewModifyMerge, recControlCondition);

                        controlConditionView.Close();
                        controlConditionView = null;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Loads records for the ControlEvent table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadControlEvent(Database Database, Type InstallerType, Object InstallerObject) {
            if (msi.controlevents != null) {
                if (Verbose) {
                    Log(Level.Info, LogPrefix + "Modifying Dialog Control Events:");
                }

                foreach (MSIControlEvent controlEvent in msi.controlevents) {
                    // Open the ControlEvent Table
                    View controlEventView;

                    if (Verbose) {
                        string action = "";
                        if (controlEvent.remove) {
                            action = "\tRemoving";
                        }
                        else {
                            action = "\tAdding";
                        }
                        Log(Level.Info, action + "\tControl: " + controlEvent.control + "\tEvent: " + controlEvent.name);
                    }
                    if (controlEvent.remove) {
                        controlEventView = Database.OpenView("SELECT * FROM `ControlEvent` WHERE `Dialog_`='" + controlEvent.dialog + "' AND `Control_`='" + controlEvent.control + "' AND `Event`='" + controlEvent.name + "' AND `Argument`='" + controlEvent.argument + "' AND `Condition`='" + controlEvent.condition + "'");
                        controlEventView.Execute(null);
                        try {
                            Record recControlEvent = controlEventView.Fetch();
                            controlEventView.Modify(MsiViewModify.msiViewModifyDelete, recControlEvent);
                        }
                        catch (IOException) {
                            Log(Level.Error, LogPrefix +
                                "ERROR: Control Event not found.\n\nSELECT * FROM `ControlEvent` WHERE `Dialog_`='" + controlEvent.dialog + "' AND `Control_`='" + controlEvent.control + "' AND `Event`='" + controlEvent.name + "' AND `Argument`='" + controlEvent.argument + "' AND `Condition`='" + controlEvent.condition + "'");
                            return false;
                        }
                        finally {
                            controlEventView.Close();
                            controlEventView = null;

                        }

                    }
                    else {
                        controlEventView = Database.OpenView("SELECT * FROM `ControlEvent`");
                        // Insert the condition
                        Record recControlEvent = (Record)InstallerType.InvokeMember(
                            "CreateRecord",
                            BindingFlags.InvokeMethod,
                            null, InstallerObject,
                            new object[] { 6 });


                        recControlEvent.set_StringData(1, controlEvent.dialog);
                        recControlEvent.set_StringData(2, controlEvent.control);
                        recControlEvent.set_StringData(3, controlEvent.name);
                        recControlEvent.set_StringData(4, controlEvent.argument);
                        recControlEvent.set_StringData(5, controlEvent.condition);
                        recControlEvent.set_IntegerData(6, controlEvent.order);

                        controlEventView.Modify(MsiViewModify.msiViewModifyMerge, recControlEvent);
                        controlEventView.Close();
                        controlEventView = null;
                    }
                }

            }
            return true;
        }

        /// <summary>
        /// Loads records for the CustomAction table
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadCustomAction(Database Database, Type InstallerType, Object InstallerObject) {
            // Add custom actions from Task definition
            if (msi.customactions != null) {
                if (Verbose) {
                    Log(Level.Info, LogPrefix + "Adding Custom Actions:");
                }

                View customActionView = Database.OpenView("SELECT * FROM `CustomAction`");

                foreach (MSICustomAction customAction in msi.customactions) {
                    if (Verbose) {
                        Log(Level.Info, "\t" + customAction.action);
                    }

                    // Insert the record into the table
                    Record recCustomAction = (Record)InstallerType.InvokeMember(
                        "CreateRecord",
                        BindingFlags.InvokeMethod,
                        null, InstallerObject,
                        new object[] { 4 });

                    recCustomAction.set_StringData(1, customAction.action);
                    recCustomAction.set_IntegerData(2, customAction.type);
                    recCustomAction.set_StringData(3, customAction.source);
                    recCustomAction.set_StringData(4, customAction.target);

                    customActionView.Modify(MsiViewModify.msiViewModifyMerge, recCustomAction);
                }
                customActionView.Close();
                customActionView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the InstallUISequence, InstallExecuteSequence,
        /// AdminUISequence, and AdminExecute tables.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadSequence(Database Database, Type InstallerType, Object InstallerObject) {
            // Add custom actions from Task definition
            if (msi.sequences != null) {
                if (Verbose) {
                    Log(Level.Info, LogPrefix + "Adding Install/Admin Sequences:");
                }

                // Open the sequence tables
                View installExecuteView = Database.OpenView("SELECT * FROM `InstallExecuteSequence`");
                View installUIView = Database.OpenView("SELECT * FROM `InstallUISequence`");
                View adminExecuteView = Database.OpenView("SELECT * FROM `AdminExecuteSequence`");
                View adminUIView = Database.OpenView("SELECT * FROM `AdminUISequence`");
                View advtExecuteView = Database.OpenView("SELECT * FROM `AdvtExecuteSequence`");

                // Add binary data from Task definition
                foreach (MSISequence sequence in msi.sequences) {
                    if (Verbose) {
                        Log(Level.Info, "\t" + sequence.action + " to the " + sequence.type.ToString() + "sequence table.");
                    }

                    // Insert the record to the respective table
                    Record recSequence = (Record)InstallerType.InvokeMember(
                        "CreateRecord",
                        BindingFlags.InvokeMethod,
                        null, InstallerObject,
                        new object[] { 3 });

                    recSequence.set_StringData(1, sequence.action);
                    recSequence.set_StringData(2, sequence.condition);
                    recSequence.set_IntegerData(3, sequence.value);
                    switch(sequence.type.ToString()) {
                        case "installexecute":
                            installExecuteView.Modify(MsiViewModify.msiViewModifyMerge, recSequence);
                            break;
                        case "installui":
                            installUIView.Modify(MsiViewModify.msiViewModifyMerge, recSequence);
                            break;
                        case "adminexecute":
                            adminExecuteView.Modify(MsiViewModify.msiViewModifyMerge, recSequence);
                            break;
                        case "adminui":
                            adminUIView.Modify(MsiViewModify.msiViewModifyMerge, recSequence);
                            break;
                        case "advtexecute":
                            advtExecuteView.Modify(MsiViewModify.msiViewModifyMerge, recSequence);
                            break;

                    }
                }
                installExecuteView.Close();
                installUIView.Close();
                adminExecuteView.Close();
                adminUIView.Close();
                advtExecuteView.Close();

                installExecuteView = null;
                installUIView = null;
                adminExecuteView = null;
                adminUIView = null;
                advtExecuteView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the ActionText table.  Allows users to specify descriptions/templates for actions.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadActionText(Database Database, Type InstallerType, Object InstallerObject) {
            if (msi.actiontext != null) {
                if (Verbose) {
                    Log(Level.Info, LogPrefix + "Adding ActionText:");
                }

                // Open the actiontext table
                View actionTextView = Database.OpenView("SELECT * FROM `ActionText`");

                foreach (MSIActionTextAction action in msi.actiontext) {
                    if (Verbose) {
                        Log(Level.Info, "\t" + action.name);
                    }

                    try {
                        // Insert the record to the respective table
                        Record recAction = (Record)InstallerType.InvokeMember(
                            "CreateRecord",
                            BindingFlags.InvokeMethod,
                            null, InstallerObject,
                            new object[] { 3 });

                        recAction.set_StringData(1, action.name);
                        recAction.set_StringData(2, action.description);
                        recAction.set_StringData(3, action.template);
                        actionTextView.Modify(MsiViewModify.msiViewModifyMerge, recAction);
                    }
                    catch (Exception e) {
                        Log(Level.Warning, LogPrefix + "Warning: Action text for \"" + action.name + "\" already exists in database.");
                        if (Verbose) {
                            Log(Level.Error, LogPrefix + e.ToString());
                        }
                    }
                }
                actionTextView.Close();
                actionTextView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the _AppMappings table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadAppMappings(Database Database, Type InstallerType, Object InstallerObject) {
            if (msi.appmappings != null) {
                if (Verbose) {
                    Log(Level.Info, LogPrefix + "Adding Application Mappings:");
                }

                View appmapView = Database.OpenView("SELECT * FROM `_AppMappings`");

                foreach (MSIAppMapping appmap in msi.appmappings) {
                    if (Verbose) {
                        Log(Level.Info, "\t" + appmap.directory);
                    }

                    // Insert the record into the table
                    Record recAppMap = (Record)InstallerType.InvokeMember(
                        "CreateRecord",
                        BindingFlags.InvokeMethod,
                        null, InstallerObject,
                        new object[] { 4 });

                    recAppMap.set_StringData(1, appmap.directory);
                    recAppMap.set_StringData(2, appmap.extension);
                    recAppMap.set_StringData(3, appmap.exepath);
                    recAppMap.set_StringData(4, appmap.verbs);

                    appmapView.Modify(MsiViewModify.msiViewModifyMerge, recAppMap);
                }
                appmapView.Close();
                appmapView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the _UrlToDir table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadUrlProperties(Database Database, Type InstallerType, Object InstallerObject) {
            if (msi.urlproperties != null) {
                if (Verbose) {
                    Log(Level.Info, LogPrefix + "Adding URL Properties:");
                }

                View urlpropView = Database.OpenView("SELECT * FROM `_UrlToDir`");

                foreach (MSIURLProperty urlprop in msi.urlproperties) {
                    if (Verbose) {
                        Log(Level.Info, "\t" + urlprop.name);
                    }

                    // Insert the record into the table
                    Record recURLProp = (Record)InstallerType.InvokeMember(
                        "CreateRecord",
                        BindingFlags.InvokeMethod,
                        null, InstallerObject,
                        new object[] { 2 });

                    recURLProp.set_StringData(1, urlprop.name);
                    recURLProp.set_StringData(2, urlprop.property);

                    urlpropView.Modify(MsiViewModify.msiViewModifyMerge, recURLProp);
                }
                urlpropView.Close();
                urlpropView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the _VDirToUrl table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadVDirProperties(Database Database, Type InstallerType, Object InstallerObject) {
            if (msi.vdirproperties != null) {
                if (Verbose) {
                    Log(Level.Info, LogPrefix + "Adding VDir Properties:");
                }

                View vdirpropView = Database.OpenView("SELECT * FROM `_VDirToUrl`");

                foreach (MSIVDirProperty vdirprop in msi.vdirproperties) {
                    if (Verbose) {
                        Log(Level.Info, "\t" + vdirprop.name);
                    }

                    // Insert the record into the table
                    Record recVDirProp = (Record)InstallerType.InvokeMember(
                        "CreateRecord",
                        BindingFlags.InvokeMethod,
                        null, InstallerObject,
                        new object[] { 3 });

                    recVDirProp.set_StringData(1, vdirprop.name);
                    recVDirProp.set_StringData(2, vdirprop.portproperty);
                    recVDirProp.set_StringData(3, vdirprop.urlproperty);

                    vdirpropView.Modify(MsiViewModify.msiViewModifyMerge, recVDirProp);
                }
                vdirpropView.Close();
                vdirpropView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the _AppRootCreate table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadAppRootCreate(Database Database, Type InstallerType, Object InstallerObject) {
            if (msi.approots != null) {
                if (Verbose) {
                    Log(Level.Info, LogPrefix + "Adding Application Roots:");
                }

                View approotView = Database.OpenView("SELECT * FROM `_AppRootCreate`");

                foreach (MSIAppRoot appRoot in msi.approots) {
                    if (Verbose) {
                        Log(Level.Info, "\t" + appRoot.urlproperty);
                    }

                    // Insert the record into the table
                    Record recAppRootProp = (Record)InstallerType.InvokeMember(
                        "CreateRecord",
                        BindingFlags.InvokeMethod,
                        null, InstallerObject,
                        new object[] { 3 });

                    recAppRootProp.set_StringData(1, appRoot.component);
                    recAppRootProp.set_StringData(2, appRoot.urlproperty);
                    recAppRootProp.set_IntegerData(3, appRoot.inprocflag);

                    approotView.Modify(MsiViewModify.msiViewModifyMerge, recAppRootProp);
                }
                approotView.Close();
                approotView = null;
            }
            return true;
        }

        /// <summary>
        /// Loads records for the _IISProperties table.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <returns>True if successful.</returns>
        private bool LoadIISProperties(Database Database, Type InstallerType, Object InstallerObject) {
            if (msi.iisproperties != null) {
                if (Verbose) {
                    Log(Level.Info, LogPrefix + "Adding IIS Directory Properties:");
                }

                View iispropView = Database.OpenView("SELECT * FROM `_IISProperties`");


                // Add binary data from Task definition
                foreach (MSIIISProperty iisprop in msi.iisproperties) {
                    if (Verbose) {
                        Log(Level.Info, "\t" + iisprop.directory);
                    }

                    // Insert the record into the table
                    Record recIISProp = (Record)InstallerType.InvokeMember(
                        "CreateRecord",
                        BindingFlags.InvokeMethod,
                        null, InstallerObject,
                        new object[] { 3 });

                    recIISProp.set_StringData(1, iisprop.directory);
                    recIISProp.set_IntegerData(2, iisprop.attr);
                    recIISProp.set_StringData(3, iisprop.defaultdoc);

                    iispropView.Modify(MsiViewModify.msiViewModifyMerge, recIISProp);
                }
                iispropView.Close();
                iispropView = null;
            }
            return true;
        }


        /// <summary>
        /// Checks to see if the specified table exists in the database
        /// already.
        /// </summary>
        /// <param name="Database">The MSI database.</param>
        /// <param name="TableName">Name of the table to check existance.</param>
        /// <returns>True if successful.</returns>
        private bool VerifyTableExistance(Database Database, string TableName) {
            View tableView = Database.OpenView("SELECT * FROM `_Tables` WHERE `Name`='" + TableName + "'");
            tableView.Execute(null);
            Record tableRecord = tableView.Fetch();

            if (tableRecord != null) {
                tableView.Close();
                tableView = null;
                return true;
            }
            else {
                tableView.Close();
                tableView = null;
                return false;
            }

        }

        /// <summary>
        /// Enumerates the registry to see if an assembly has been registered
        /// for COM interop, and if so adds these registry keys to the Registry
        /// table, ProgIds to the ProgId table, classes to the Classes table,
        /// and a TypeLib to the TypeLib table.
        /// </summary>
        /// <param name="FileName">The Assembly filename.</param>
        /// <param name="FileAssembly">The Assembly to check.</param>
        /// <param name="InstallerType">The MSI Installer type.</param>
        /// <param name="InstallerObject">The MSI Installer object.</param>
        /// <param name="ComponentName">The name of the containing component.</param>
        /// <param name="AssemblyComponentName">The name of the containing component's assembly GUID.</param>
        /// <param name="ClassView">View containing the Class table.</param>
        /// <param name="ProgIdView">View containing the ProgId table.</param>
        /// <returns>True if successful.</returns>
        private bool CheckAssemblyForCOMInterop(string FileName, Assembly FileAssembly, Type InstallerType,
            object InstallerObject, string ComponentName, string AssemblyComponentName, View ClassView, View ProgIdView) {
            AssemblyName asmName = FileAssembly.GetName();
            string featureName = (string)featureComponents[ComponentName];
            string typeLibName = Path.GetFileNameWithoutExtension(FileName) + ".tlb";
            string typeLibFileName = Path.Combine(Path.GetDirectoryName(FileName), typeLibName);

            bool foundTypeLib = false;

            // Register the TypeLibrary
            RegistryKey typeLibsKey = Registry.ClassesRoot.OpenSubKey("Typelib", false);

            string[] typeLibs = typeLibsKey.GetSubKeyNames();
            foreach (string typeLib in typeLibs) {
                RegistryKey typeLibKey = typeLibsKey.OpenSubKey(typeLib, false);
                if (typeLibKey != null) {
                    string[] typeLibSubKeys = typeLibKey.GetSubKeyNames();
                    foreach (string typeLibSubKey in typeLibSubKeys) {
                        RegistryKey win32Key = typeLibKey.OpenSubKey(typeLibSubKey + @"\0\win32");
                        if (win32Key != null) {
                            string curTypeLibFileName = (string)win32Key.GetValue(null, null);
                            if (curTypeLibFileName != null) {
                                if (String.Compare(curTypeLibFileName, typeLibFileName, true) == 0) {
                                    Log(Level.Info, LogPrefix + "Configuring " + typeLibName + " for COM Interop...");

                                    Record recTypeLib = (Record)InstallerType.InvokeMember(
                                        "CreateRecord",
                                        BindingFlags.InvokeMethod,
                                        null, InstallerObject,
                                        new object[] { 8 });

                                    TypeLibRecord tlbRecord = new TypeLibRecord(
                                        typeLib, typeLibFileName,
                                        asmName, featureName, AssemblyComponentName);

                                    typeLibRecords.Add(tlbRecord);

                                    foundTypeLib = true;
                                    win32Key.Close();
                                    break;
                                }
                            }
                            win32Key.Close();
                        }
                    }
                    typeLibKey.Close();

                    if (foundTypeLib) {
                        break;
                    }
                }
            }
            typeLibsKey.Close();

            // Register CLSID(s)
            RegistryKey clsidsKey = Registry.ClassesRoot.OpenSubKey("CLSID", false);

            string[] clsids = clsidsKey.GetSubKeyNames();
            foreach (string clsid in clsids) {
                RegistryKey clsidKey = clsidsKey.OpenSubKey(clsid, false);
                if (clsidKey != null) {
                    RegistryKey inprocKey = clsidKey.OpenSubKey("InprocServer32", false);
                    if (inprocKey != null) {
                        string clsidAsmName = (string)inprocKey.GetValue("Assembly", null);
                        if (clsidAsmName != null) {
                            if (asmName.FullName == clsidAsmName) {
                                // Register ProgId(s)
                                RegistryKey progIdKey = clsidKey.OpenSubKey("ProgId", false);
                                if (progIdKey != null) {
                                    string progId = (string)progIdKey.GetValue(null, null);
                                    string className = (string)clsidKey.GetValue(null, null);

                                    if (progId != null) {
                                        Record recProgId = (Record)InstallerType.InvokeMember(
                                            "CreateRecord",
                                            BindingFlags.InvokeMethod,
                                            null, InstallerObject,
                                            new object[] { 6 });

                                        recProgId.set_StringData(1, progId);
                                        recProgId.set_StringData(3, clsid);
                                        recProgId.set_StringData(4, className);
                                        recProgId.set_IntegerData(6, 0);
                                        ProgIdView.Modify(MsiViewModify.msiViewModifyMerge, recProgId);

                                        Record recClass = (Record)InstallerType.InvokeMember(
                                            "CreateRecord",
                                            BindingFlags.InvokeMethod,
                                            null, InstallerObject,
                                            new object[] { 13 });

                                        recClass.set_StringData(1, clsid);
                                        recClass.set_StringData(2, "InprocServer32");
                                        recClass.set_StringData(3, AssemblyComponentName);
                                        recClass.set_StringData(4, progId);
                                        recClass.set_StringData(5, className);
                                        //recClass.set_StringData(6, appId);
                                        recClass.set_IntegerData(9, 0);
                                        recClass.set_StringData(12, featureName);
                                        recClass.set_IntegerData(13, 0);
                                        ClassView.Modify(MsiViewModify.msiViewModifyMerge, recClass);
                                    }
                                    progIdKey.Close();
                                    progIdKey = null;
                                }
                            }
                        }
                        inprocKey.Close();
                    }
                    clsidKey.Close();
                }
            }
            clsidsKey.Close();

            return true;
        }

        private string FindParent(string DirectoryName) {
            foreach (MSIDirectory directory in msi.directories) {
                string parent = FindParent(DirectoryName, directory);
                if (parent != null) {
                    return parent;
                }
            }
            return null;
        }

        private string FindParent(string DirectoryName, MSIDirectory directory) {
            if (DirectoryName == directory.name &&
                directory is MSIRootDirectory) {
                return ((MSIRootDirectory)directory).root;
            }
            else {
                if (directory.directory != null) {
                    foreach (MSIDirectory directory2 in directory.directory) {
                        if (directory2.name == DirectoryName) {
                            return directory.name;
                        }
                        else {
                            string parent = FindParent(DirectoryName, directory2);
                            if (parent != null) {
                                return parent;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private MSIDirectory FindDirectory(string DirectoryName) {
            foreach (MSIDirectory directory in msi.directories) {
                MSIDirectory childDirectory = FindDirectory(DirectoryName, directory);
                if (childDirectory != null) {
                    return childDirectory;
                }
            }

            return null;
        }

        private MSIDirectory FindDirectory(string DirectoryName, MSIDirectory directory) {
            if (directory.name == DirectoryName) {
                return directory;
            }

            if (directory.directory != null) {
                foreach (MSIDirectory childDirectory in directory.directory) {
                    MSIDirectory childDirectory2 = FindDirectory(DirectoryName, childDirectory);
                    if (childDirectory2 != null) {
                        return childDirectory2;
                    }
                }
            }

            return null;
        }

        private string GetDisplayablePath(string path) {
            if (path.Length > 40) {
                return "..." + path.Substring(path.Length-37, 37);
            }
            return path;
        }
    }

    /// <summary>
    /// Maintains a forward reference to a .tlb file
    /// in the same directory as an assembly .dll
    /// that has been registered for COM interop.
    /// </summary>
    internal class TypeLibRecord {
        private AssemblyName assemblyName;
        private string libId, typeLibFileName,
            featureName, assemblyComponent;

        /// <summary>
        /// Creates a new <see cref="TypeLibRecord"/>.
        /// </summary>
        /// <param name="LibId">The typelibrary id.</param>
        /// <param name="TypeLibFileName">The typelibrary filename.</param>
        /// <param name="AssemblyName">The name of the assembly.</param>
        /// <param name="FeatureName">The feature containing the typelibrary's file.</param>
        /// <param name="AssemblyComponent">The name of the Assembly's component.</param>
        public TypeLibRecord(
            string LibId, string TypeLibFileName,
            AssemblyName AssemblyName, string FeatureName,
            string AssemblyComponent) {
            libId = LibId;
            typeLibFileName = TypeLibFileName;
            assemblyName = AssemblyName;
            featureName = FeatureName;
            assemblyComponent = AssemblyComponent;
        }

        /// <summary>
        /// Retrieves the name of the Assembly's component.
        /// </summary>
        /// <value>The Assembly's component Name.</value>
        /// <remarks>None.</remarks>
        public string AssemblyComponent {
            get { return assemblyComponent; }
        }

        /// <summary>
        /// Retrieves the typelibrary filename.
        /// </summary>
        /// <value>The typelibrary filename.</value>
        /// <remarks>None.</remarks>
        public string TypeLibFileName {
            get { return typeLibFileName; }
        }

        /// <summary>
        /// Retrieves the typelibrary id.
        /// </summary>
        /// <value>The typelibrary id.</value>
        /// <remarks>None.</remarks>
        public string LibId {
            get { return libId; }
        }

        /// <summary>
        /// Retrieves the name of the assembly.
        /// </summary>
        /// <value>The name of the assembly.</value>
        /// <remarks>None.</remarks>
        public AssemblyName AssemblyName {
            get { return assemblyName; }
        }

        /// <summary>
        /// Retrieves the feature containing the typelibrary's file.
        /// </summary>
        /// <value>The feature containing the typelibrary's file.</value>
        /// <remarks>None.</remarks>
        public string FeatureName {
            get { return featureName; }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSIRowColumnData {
        public string name;
        public int id;
        public string type;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct CUSTDATAITEM {
        public Guid guid;
        public object varValue;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CUSTDATA {
        public int cCustData;
        public CUSTDATAITEM[] prgCustData;
    }

    [ComImport]
    [Guid("00020412-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface UCOMITypeInfo2 {
        #region Implementation of UCOMITypeInfo
        void GetContainingTypeLib(out System.Runtime.InteropServices.UCOMITypeLib ppTLB, out int pIndex);
        void GetIDsOfNames(string[] rgszNames, int cNames, int[] pMemId);
        void GetRefTypeInfo(int hRef, out System.Runtime.InteropServices.UCOMITypeInfo ppTI);
        void GetMops(int memid, out string pBstrMops);
        void ReleaseVarDesc(System.IntPtr pVarDesc);
        void ReleaseTypeAttr(System.IntPtr pTypeAttr);
        void GetDllEntry(int memid, System.Runtime.InteropServices.INVOKEKIND invKind, out string pBstrDllName, out string pBstrName, out short pwOrdinal);
        void GetRefTypeOfImplType(int index, out int href);
        void GetTypeComp(out System.Runtime.InteropServices.UCOMITypeComp ppTComp);
        void GetTypeAttr(out System.IntPtr ppTypeAttr);
        void GetDocumentation(int index, out string strName, out string strDocString, out int dwHelpContext, out string strHelpFile);
        void AddressOfMember(int memid, System.Runtime.InteropServices.INVOKEKIND invKind, out System.IntPtr ppv);
        void GetNames(int memid, string[] rgBstrNames, int cMaxNames, out int pcNames);
        void CreateInstance(object pUnkOuter, ref System.Guid riid, out object ppvObj);
        void Invoke(object pvInstance, int memid, short wFlags, ref System.Runtime.InteropServices.DISPPARAMS pDispParams, out object pVarResult, out System.Runtime.InteropServices.EXCEPINFO pExcepInfo, out int puArgErr);
        void GetVarDesc(int index, out System.IntPtr ppVarDesc);
        void ReleaseFuncDesc(System.IntPtr pFuncDesc);
        void GetFuncDesc(int index, out System.IntPtr ppFuncDesc);
        void GetImplTypeFlags(int index, out int pImplTypeFlags);
        #endregion

        void GetTypeKind([Out] out TYPEKIND pTypeKind);
        void GetTypeFlags([Out] out int pTypeFlags);
        void GetFuncIndexOfMemId(int memid, INVOKEKIND invKind, [Out] out int pFuncIndex);
        void GetVarIndexOfMemId(int memid, [Out] out int pVarIndex);
        void GetCustData([In] ref Guid guid, [Out] out object pCustData);
        void GetFuncCustData(int index, [In] ref Guid guid, [Out] out object pVarVal);
        void GetParamCustData(int indexFunc, int indexParam, [In] ref Guid guid, [Out] out object pVarVal);
        void GetVarCustData(int index, [In] ref Guid guid, [Out] out object pVarVal);
        void GetImplTypeCustData(int index, [In] ref Guid guid, [Out] out object pVarVal);
        void GetDocumentation2(int memid, int lcid, [Out] out string pbstrHelpString, [Out] out int pdwHelpStringContext, [Out] out string pbstrHelpStringDll);
        void GetAllCustData([In,Out] ref IntPtr pCustData);
        void GetAllFuncCustData(int index, [Out] out CUSTDATA pCustData);
        void GetAllParamCustData(int indexFunc, int indexParam, [Out] out CUSTDATA pCustData);
        void GetAllVarCustData(int index, [Out] out CUSTDATA pCustData);
        void GetAllImplTypeCustData(int index, [Out] out CUSTDATA pCustData);
    }
}
