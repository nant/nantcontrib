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

using NAnt.Core.Attributes;

using NAnt.Contrib.Types;
using NAnt.Contrib.Schemas.Msi;

namespace NAnt.Contrib.Tasks.Msi {
    /// <summary>
    /// Builds a Windows Installer Merge Module (MSM) database.
    /// </summary>
    /// <para>
    /// Requires <c>cabarc.exe</c> in the path.  This tool is part of the 
    /// <see href="http://msdn2.microsoft.com/en-us/library/ms974336.aspx">Microsoft Cabinet SDK</see>.
    /// </para>
    [TaskName("msm")]
    [SchemaValidator(typeof(msm), "NAnt.Contrib.Tasks.Msi.MsiTask")]
    public class MsmTask : InstallerTaskBase {
        #region Private Instance Fields

        private MsmCreationCommand _taskCommand;

        #endregion Private Instance Fields

        #region Attributes

        /// <summary>
        /// Stores a unique identifier for a merge module.  To be used as the merge module's ModuleSignature
        /// </summary>
        [TaskAttribute("id", Required=true, ProcessXml=false)]
        public string MsmId {
            get { return ((msm)_taskCommand.MsiBase).id; }
        }

        /// <summary>
        /// Specifies the numeric language ID or IDs for a merge module.
        /// </summary>
        [TaskAttribute("language", Required=false, ProcessXml=false)]
        public string MsmLanguage {
            get { return ((msm)_taskCommand.MsiBase).language; }
        }

        /// <summary>
        /// Stores the version number of a merge module.
        /// </summary>
        [TaskAttribute("version", Required=false, ProcessXml=false)]
        public string MsmVersion {
            get { return ((msm)_taskCommand.MsiBase).version; }
        }

        #endregion Attributes
        
        #region Nested build elements

        /// <summary>
        /// <para>
        /// Lists other merge modules that are required for this merge module 
        /// to operate properly.
        /// </para>
        /// <para>
        /// Contains any number of dependency elements.
        /// </para>
        /// <para>
        /// More information is available <see href="http://msdn.microsoft.com/library/en-us/msi/setup/moduledependency_table.asp">here</see>.
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
        ///         <term>id</term>
        ///         <term>string</term>
        ///         <term>Identifier of the merge module required</term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>language</term>
        ///         <term>string</term>
        ///         <term>Numeric language ID of the dependent merge module. Can specify the language ID for a single language, such as 1033 for U.S. English, or specify the language ID for a language group, such as 9 for any English. If the field contains a group language ID, any merge module with having a language code in that group satisfies the dependency. If the RequiredLanguage is set to 0, any merge module filling the other requirements satisfies the dependency.</term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>version</term>
        ///         <term>string</term>
        ///         <term>Version of the dependent merge module. If ommited, any version fills the dependency.</term>
        ///         <term>False</term>
        ///     </item>
        /// </list>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Make sure that the NAnt merge module is included</para>
        ///     <code>
        /// &lt;moduledependencies&gt;
        ///     &lt;dependency id="NAnt_MergeModule.2D2FB50C_DADF_4813_8932_8EF1E8CB8E80" language="0" /&gt;
        /// &lt;/moduledependencies&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("moduledependencies", ProcessXml=false)]
        public SchemaElement[] MsmModuleDependenciesElement {
            get { return null; }
            set {}
        }        

        /// <summary>
        /// <para>
        /// Lists other merge modules that are incompatible in the same 
        /// installer database.
        /// </para>
        /// <para>
        /// Contains any number of exclusion elements.
        /// </para>
        /// <para>
        /// More information is available <see href="http://msdn.microsoft.com/library/en-us/msi/setup/moduleexclusion_table.asp">here</see>.
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
        ///         <term>id</term>
        ///         <term>string</term>
        ///         <term>Identifier of the merge module required</term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>language</term>
        ///         <term>string</term>
        ///         <term>Numeric language ID of the merge module in ExcludedID. The ExcludedLanguage column can specify the language ID for a single language, such as 1033 for U.S. English, or specify the language ID for a language group, such as 9 for any English. The ExcludedLanguage column can accept negative language IDs. The meaning of the value in the ExcludedLanguage column is as follows.
        ///             <list type="table">
        ///                 <listheader>
        ///                     <term>ExcludedLanguage</term>
        ///                     <description>Description</description>
        ///                 </listheader>
        ///                 <item>
        ///                     <term>&gt; 0</term>
        ///                     <description>Exclude the language IDs specified by ExcludedLanguage.</description>
        ///                 </item>
        ///                 <item>
        ///                     <term>= 0</term>
        ///                     <description>Exclude no language IDs.</description>
        ///                 </item>
        ///                 <item>
        ///                     <term>&lt; 0</term>
        ///                     <description>Exclude all language IDs except those specified by ExcludedLanguage.</description>
        ///                 </item>
        ///             </list>
        ///         </term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>minversion</term>
        ///         <term>string</term>
        ///         <term>Minimum version excluded from a range. If ommitted, all versions before maxversion are excluded. If both minversion and maxversion are ommitted there is no exclusion based on version.</term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>maxversion</term>
        ///         <term>string</term>
        ///         <term>Maximum version excluded from a range. If ommitted, all versions after minversion are excluded. If both minversion and maxversion are ommitted there is no exclusion based on version.</term>
        ///         <term>False</term>
        ///     </item>
        /// </list>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Exclude the all NAnt merge modules created before version 0.85.0</para>
        ///     <code>
        /// &lt;moduleexclusions&gt;
        ///     &lt;exclusion id="NAnt_MergeModule.2D2FB50C_DADF_4813_8932_8EF1E8CB8E80" language="0" maxversion="0.85.0" /&gt;
        /// &lt;/moduleexclusions&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("moduleexclusions", ProcessXml=false)]
        public SchemaElement[] MsmModuleExclusionsElement {
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
        ///         <term>Type</term>
        ///         <term>Description</term>
        ///         <term>Required</term>
        ///     </listheader>
        ///     <item>
        ///         <term>type</term>
        ///         <term>msi:MSISequenceTable</term>
        ///         <term>Valid inputs:
        ///             <list type="bullet">
        ///                 <item><c>installexecute</c> represents <a href="http://msdn.microsoft.com/library/en-us/msi/setup/moduleinstallexecutesequence_table.asp">ModuleInstallExecuteSequence Table</a>.</item>
        ///                 <item><c>installui</c> represents <a href="http://msdn.microsoft.com/library/en-us/msi/setup/moduleinstalluisequence_table.asp">ModuleInstallUISequence Table</a></item>
        ///                 <item><c>adminexecute</c> represents <a href="http://msdn.microsoft.com/library/en-us/msi/setup/moduleadminexecutesequence_table.asp">ModuleAdminExecuteSequence Table</a></item>
        ///                 <item><c>adminui</c> represents <a href="http://msdn.microsoft.com/library/en-us/msi/setup/moduleadminuisequence_table.asp">ModuleAdminUISequence Table</a></item>
        ///                 <item><c>advtexecute</c> represents <a href="http://msdn.microsoft.com/library/en-us/msi/setup/moduleadvtuisequence_table.asp">ModuleAdvtUISequence Table</a></item>
        ///             </list>
        ///         </term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>action</term>
        ///         <term>string</term>
        ///         <term>Action to insert into sequence. Refers to one of the installer standard actions, or an entry in the merge module's CustomAction table or Dialog table.<br/>If a standard action is used in the Action column of a merge module sequence table, the BaseAction and After attributes must be ommitted.</term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>sequence</term>
        ///         <term>int</term>
        ///         <term>The sequence number of a standard action. If a custom action or dialog is entered into the Action column of this row, this attribute must be ommitted <br/>When using standard actions in merge module sequence tables, the value in the Sequence column should be the recommended action sequence number. If the sequence number in the merge module differs from that for the same action in the .msi file sequence table, the merge tool uses the sequence number from the .msi file. See the suggested sequences in Using a Sequence Table for the recommended sequence numbers of standard actions.</term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>baseaction</term>
        ///         <term>string</term>
        ///         <term>Can contain a standard action, a custom action specified in the merge module's custom action table, or a dialog specified in the module's dialog table. Is a key into the Action column of this table. It cannot be a foreign key into another merge table or table in the .msi file. This means that every standard action, custom action, or dialog listed in the BaseAction column must also be listed in the Action column of another record in this table.</term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>after</term>
        ///         <term>bool</term>
        ///         <term>Boolean for whether Action comes before or after BaseAction
        ///         <list type="table">
        ///             <listheader>
        ///                 <term>Value</term>
        ///                 <description>Description</description>
        ///             </listheader>
        ///             <item>
        ///                 <term>True</term>
        ///                 <description>Action to come after BaseAction</description>
        ///             </item>
        ///             <item>
        ///                 <term>False</term>
        ///                 <description>Action to come before BaseAction</description>
        ///             </item>
        ///         </list>
        ///         </term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>condition</term>
        ///         <term>string</term>
        ///         <term>A conditional statement that indicates if the action is be executed.</term>
        ///         <term>False</term>
        ///     </item>
        /// </list>
        /// </summary>
        [BuildElement("modulesequences", ProcessXml=false)]
        public SchemaElement[] MsmModuleSequencesElement {
            get { return null; }
            set {}
        }  

        /// <summary>
        /// <para>
        /// If a table in the merge module is listed in the ModuleIgnoreTable 
        /// table, it is not merged into the .msi file. If the table already 
        /// exists in the .msi file, it is not modified by the merge. The tables 
        /// in the ModuleIgnoreTable can therefore contain data that is unneeded 
        /// after the merge.
        /// </para>
        /// <para>
        /// More information is available <see href="http://msdn.microsoft.com/library/en-us/msi/setup/moduleignoretable_table.asp?frame=true">here</see>.
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
        ///         <term>table</term>
        ///         <term>string</term>
        ///         <term>Name of the table in the merge module that is not to be merged into the .msi file.</term>
        ///         <term>True</term>
        ///     </item>
        /// </list>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Ensure the module is compatible for users who have versions of Mergemod.dll earlier than 2.0</para>
        ///     <code>
        /// &lt;moduleignoretables&gt;
        ///     &lt;table name="ModuleConfiguration" /&gt;
        ///     &lt;table name="ModuleSubstitution" /&gt;
        ///     &lt;table name="_ModuleConfigurationGroup" /&gt;
        /// &lt;/moduleignoretables&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("moduleignoretables", ProcessXml=false)]
        public SchemaElement[] MsmModuleIgnoreTablesElement {
            get { return null; }
            set {}
        }

        /// <summary>
        /// <para>
        /// The ModuleSubstitution table specifies the configurable fields of a 
        /// module database and provides a template for the configuration of each 
        /// field. The user or merge tool may query this table to determine what 
        /// configuration operations are to take place. This table is not merged 
        /// into the target database.
        /// </para>
        /// <para>
        /// More information is available <see href="http://msdn.microsoft.com/library/en-us/msi/setup/modulesubstitution_table.asp">here</see>.
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
        ///         <term>table</term>
        ///         <term>string</term>
        ///         <term>Name of the table being modified in the module database.</term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>row</term>
        ///         <term>string</term>
        ///         <term>Specifies the primary keys of the target row in the table named in the Table column. Multiple primary keys are separated by semicolons. Target rows are selected for modification before any changes are made to the target table. If one record in the ModuleSubstitution table changes the primary key field of a target row, other records in the ModuleSubstitution table are applied based on the original primary key data, not the resulting of primary key substitutions. The order of row substitution is undefined.<br/>Values in this column are always in CMSM special format. A literal semicolon (';') or equal sign ('=') can be added by prefixing the character with a backslash. '\'. A null value for a key is signified by a null, a leading semicolon, two consecutive semicolons, or a trailing semicolon, depending on whether the null value is a sole, first, middle, or final key column value.</term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>column</term>
        ///         <term>string</term>
        ///         <term>Specifies the target column in the row named in the Row column. If multiple rows in the ModuleSubstitution table change different columns of the same target row, all the column substitutions are performed before the modified row is inserted into the database. The order of column substitution is undefined.</term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>value</term>
        ///         <term>string</term>
        ///         <term>Contains a string that provides a formatting template for the data being substituted into the target field specified by Table, Row, and Column. When a substitution string of the form [=ItemA] is encountered, the string, including the bracket characters, is replaced by the value for the configurable "ItemA." The configurable item "ItemA" is specified in the Name column of the ModuleConfiguration table and its value is provided by the merge tool. If the merge tool declines to provide a value for any item in a replacement string, the default value specified in the DefaultValue column of the ModuleConfiguration Table is substituted. If a string references an item not in the ModuleConfiguration table, the merge fails.
        ///             <list type="bullet">
        ///                 <item>
        ///                 This column uses CMSM special format. A literal semicolon (';') or equals sign ('=') can be added to the table by prefixing the character with a backslash. '\'.
        ///                 </item>
        ///                 <item>
        ///                 The Value field may contain multiple substitution strings. For example, the configuration of items "Food1" and "Food2" in the string: "[=Food1] is good, but [=Food2] is better because [=Food2] is more nutritious."
        ///                 </item>
        ///                 <item>
        ///                 Replacement strings must not be nested. The template "[=AB[=CDE]]" is invalid.
        ///                 </item>
        ///                 <item>
        ///                 If the Value field evaluates to null, and the target field is not nullable, the merge fails and an error object of type msmErrorBadNullSubstitution is created and added to the error list. For details, see the error types described in get_Type Function.
        ///                 </item>
        ///                 <item>
        ///                 If the Value field evaluates to the null GUID: {00000000-0000-0000-0000-000000000000}, the null GUID is replaced by the name of the feature before the row is merged into the module. For details, see Referencing Features in Merge Modules.
        ///                 </item>
        ///                 <item>
        ///                 The template in the Value field is evaluated before being inserted into the target field. Substitution into a row is done before replacing any features.
        ///                 </item>
        ///                 <item>
        ///                 If the Value column evaluates to a string of only integer characters (with an optional + or -), the string is converted into an integer before being substituted into an target field of the Integer Format Type. If the template evaluates to a string that does not consist only of integer characters (and an optional + or -) the result cannot be substituted into an integer target field. Attempting to insert a non-integer into an integer field causes the merge to fail and adds a msmErrorBadSubstitutionType error object to the error list.
        ///                 </item>
        ///                 <item>
        ///                 If the target column specified in the Table and Column fields is a Text Format Type, and evaluation of the Value field results in an Integer Format Type, a decimal representation of the number is inserted into the target text field.
        ///                 </item>
        ///                 <item>
        ///                 If the target field is an Integer Format Type, and the Value field consists of a non-delimited list of items in Bitfield Format, the value in the target field is combined using the bitwise AND operator with the inverse of the bitwise OR of all of the mask values from the items, then combined using the bitwise OR operator with each of the integer or bitfield items when masked by their corresponding mask values. Essentially, this explicitly sets the bits from the properties to the provided values but leaves all other bits in the cell alone.
        ///                 </item>
        ///                 <item>
        ///                 If the Value field evaluates to a Key Format Type, and is a key into a table that uses multiple primary keys, the item name may be followed by a semicolon and an integer value that indicates the 1-based index into the set of values that together make a primary key. If no integer is specified, the value 1 is used. For example, the Control table has two primary key columns, Dialog_ and Control. The value of an item "Item1" that is a key into the Control table will be of the form "DialogName;ControlName", where DialogName is the value in the Dialog_ table and ControlName is the value in the Control column. To substitute just ControlName, the substitution string [=Item1;2] should be used.
        ///                 </item>
        ///             </list>
        ///         </term>
        ///         <term>False</term>
        ///     </item>
        /// </list>
        /// </summary>
        [BuildElement("modulesubstitutions", ProcessXml=false)]
        public SchemaElement[] MsmModuleSubstitutionsElement {
            get { return null; }
            set {}
        }

        /// <summary>
        /// <para>
        /// Identifies the configurable attributes of the module. This table is 
        /// not merged into the database.
        /// </para>
        /// <para>
        /// More information is available <see href="http://msdn.microsoft.com/library/en-us/msi/setup/moduleconfiguration_table.asp">here</see>.
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
        ///         <term>Name of the configurable item. This name is referenced in the formatting template in the Value column of the ModuleSubstitution table.</term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>format</term>
        ///         <term>msi:MSMModuleConfigurationFormat</term>
        ///         <term>Specifies the format of the data being changed
        ///             <list type="bullet">
        ///                 <item>text</item>
        ///                 <item>key</item>
        ///                 <item>integer</item>
        ///                 <item>bitfield</item>
        ///             </list>
        ///         </term>
        ///         <term>True</term>
        ///     </item>
        ///     <item>
        ///         <term>type</term>
        ///         <term>string</term>
        ///         <term>Specifies the type for the data being changed. This type is used to provide a context for any user-interface and is not used in the merge process. The valid values for this depend on the value in the Format attribute.</term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>contextdata</term>
        ///         <term>string</term>
        ///         <term>Specifies a semantic context for the requested data. The type is used to provide a context for any user-interface and is not used in the merge process. The valid values for this column depend on the values in the Format and Type attributes.</term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>defaultvalue</term>
        ///         <term>string</term>
        ///         <term>Specifies a default value for the item in this record if the merge tool declines to provide a value. This value must have the format, type, and context of the item. If this is a "Key" format item, the foreign key must be a valid key into the tables of the module. Null may be a valid value for this column depending on the item. For "Key" format items, this value is in CMSM special format. For all other types, the value is treated literally.<br/>Module authors must ensure that the module is valid in its default state. This ensures that versions of Mergemod.dll earlier than version 2.0 can still use the module in its default state.</term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>attr</term>
        ///         <term>int</term>
        ///         <term>Bit field containing attributes for this configurable item. Null is equivalent to 0.
        ///             <list type="table">
        ///                 <listheader>
        ///                     <term>Value</term>
        ///                     <description>Description</description>
        ///                 </listheader>
        ///                 <item>
        ///                     <term>1</term>
        ///                     <description>This attribute only applies to records that list a foreign key to a module table in their DefaultValue field.</description>
        ///                 </item>
        ///                 <item>
        ///                     <term>2</term>
        ///                     <description>When this attribute is set, null is not a valid response for this item. This attribute has no effect for Integer Format Types or Bitfield Format Types.</description>
        ///                 </item>
        ///             </list>
        ///         </term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>displayname</term>
        ///         <term>string</term>
        ///         <term>Provides a short description of this item that the authoring tool may use in the user interface. This column may not be localized. Set this column to null to have the module is request that the authoring tool not expose this property in the UI.</term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>description</term>
        ///         <term>string</term>
        ///         <term>Provides a description of this item that the authoring tool may use in UI elements. This string may be localized by the module's language transform.</term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>helplocation</term>
        ///         <term>string</term>
        ///         <term>Provides either the name of a help file (without the .chm extension) or a semicolon delimited list of help namespaces. This can be ommitted if no help is available.</term>
        ///         <term>False</term>
        ///     </item>
        ///     <item>
        ///         <term>helpkeyword</term>
        ///         <term>string</term>
        ///         <term>Provides a keyword into the help file or namespace from the HelpLocation column. The interpretation of this keyword depends on the HelpLocation attribute.</term>
        ///         <term>False</term>
        ///     </item>
        /// </list>
        /// </summary>
        [BuildElement("moduleconfigurations", ProcessXml=false)]
        public SchemaElement[] MsmModuleConfigurationsElement {
            get { return null; }
            set {}
        }

        #endregion

        #region Override implementation of SchemaValidatedTask

        /// <summary>
        /// Initializes task and verifies parameters.
        /// </summary>
        protected override void Initialize() {
            base.Initialize();

            _taskCommand = new MsmCreationCommand((msm) SchemaObject, this, 
                this.Location, this.XmlNode);
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        protected override void ExecuteTask() {
            _taskCommand.Execute();
        }

        #endregion Override implementation of SchemaValidatedTask
    }
}
