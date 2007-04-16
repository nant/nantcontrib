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
// Based on original work by Jayme C. Edwards (jcedwards@users.sourceforge.net)
//

using System;
using System.Xml;

using NAnt.Core.Attributes;

using NAnt.Contrib.Types;
using NAnt.Contrib.Schemas.Msi;

namespace NAnt.Contrib.Tasks.Msi {
    /// <summary>
    /// Creates a Windows Installer (also known as Microsoft Installer, or MSI)
    /// setup database for installing software on the Windows Platform. 
    /// </summary>
    /// <remarks>
    /// <para>
    /// Requires <c>cabarc.exe</c> in the path.  This tool is part of the 
    /// <see href="http://msdn2.microsoft.com/en-us/library/ms974336.aspx">Microsoft Cabinet SDK</see>.
    /// </para>
    /// <para>
    /// Requires <c>mergemod.dll</c> version 2.0 or greater.  This dll is part of the 
    /// <see href="http://www.microsoft.com/msdownload/platformsdk/instmsi.htm">Microsoft Platform SDK</see>.
    /// </para>
    /// <para>
    /// See the <see href="http://msdn.microsoft.com/library/en-us/msi/setup/roadmap_to_windows_installer_documentation.asp?frame=true" >Roadmap to Windows Installer Documentation</see> 
    /// at Microsoft's MSDN website for more information.
    /// </para>
    /// </remarks>
    [TaskName("msi")]
    [SchemaValidator(typeof(msi))]
    public class MsiTask : InstallerTaskBase {
        #region Private Instance Fields

        private MsiCreationCommand _taskCommand;

        #endregion Private Instance Fields

        #region XmlDoc Documenation Support

        #region Attributes

        /// <summary>
        /// An .rtf (rich text format) file containing the license agreement 
        /// for your software. The contents of this file will be displayed to 
        /// the user when setup runs and must be accepted to continue.
        /// </summary>
        [TaskAttribute("license", Required=false)]
        public string MsiLicense {
            get { return ((msi)_taskCommand.MsiBase).license; }
        }

        /// <summary>
        /// A .bmp (bitmap) file 495x60 pixels in size that will be displayed 
        /// as the banner (top) image of the installation user interface.
        /// </summary>
        [TaskAttribute("banner", Required=false)]
        public string MsiBanner {
            get { return ((msi)_taskCommand.MsiBase).banner; }
        }

        /// <summary>
        /// A .bmp (bitmap) file 495x315 pixels in size that will be displayed 
        /// as the background image of the installation user interface.
        /// </summary>
        [TaskAttribute("background", Required=false)]
        public string MsiBackground {
            get { return ((msi)_taskCommand.MsiBase).background; }
        }

        #endregion

        #region Sub-Elements

        /// <summary>
        /// <para>
        /// Groups sets of components into named sets, these can be used to 
        /// layout the tree control that allows users to select and deselect 
        /// features of your software product when a custom installation is 
        /// selected at runtime.
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
        [BuildElement("features", ProcessXml=false)]
        public SchemaElement[] MsiFeaturesElement {
            get { return null; }
            set {}
        }

        /// <summary>
        /// <para>
        /// Includes pre-packaged installation components (.msm files) as part 
        /// of the msi database. This feature allows reuse of installation 
        /// components that use MSI technology from other setup vendors or as 
        /// created by the <see cref="MsmTask" />.
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
        ///            <term>feature</term>
        ///            <term>string</term>
        ///            <term>Refrence to a feature.  Used to associate the merge module with the feature (and the feature's directory) for when to install the components in the merge module.</term>
        ///            <term>True</term>
        ///        </item>
        ///    </list>
        /// <h3>Nested Elements:</h3>
        /// <h4>&lt;modules&gt;</h4>
        ///   <ul>
        ///     Specifies the merge module(s) to include with the specified feature.
        ///   </ul>
        /// <h4>&lt;/modules&gt;</h4>
        /// <h4>&lt;configurationitems&gt;</h4>
        ///   <ul>
        ///     <h4>&lt;configurationitem&gt;</h4>
        ///       <ul>
        ///         Specifies the value for a configurable item
        ///         <h3>Parameters</h3>
        ///         <list type="table">
        ///             <listheader>
        ///                 <term>Attribute</term>
        ///                 <term>Type</term>
        ///                 <term>Description</term>
        ///                 <term>Required</term>
        ///             </listheader>
        ///             <item>
        ///                 <term>module</term>
        ///                 <term>string</term>
        ///                 <term>Merge module filename to limit the configuration item to.  If this is not set, the configuration item will be applied to all merge modules in the fileset.</term>
        ///                 <term>True</term>
        ///             </item>
        ///             <item>
        ///                 <term>name</term>
        ///                 <term>string</term>
        ///                 <term>Name of item for which data is to be set</term>
        ///                 <term>True</term>
        ///             </item>
        ///             <item>
        ///                 <term>value</term>
        ///                 <term>string</term>
        ///                 <term>Value of the configurable item</term>
        ///                 <term>True</term>
        ///             </item>
        ///         </list>
        ///     </ul>
        ///     <h4>&lt;/configurationitem&gt;</h4>
        ///   </ul>
        /// <h4>&lt;/configurationitems&gt;</h4>
        /// <h3>Examples</h3>
        /// <example>
        ///     <para>Add the NAnt merge module to the install.</para>
        ///     <code>
        /// &lt;mergemodules&gt;
        ///     &lt;merge feature="F__NAntMSM"&gt;
        ///         &lt;modules&gt;
        ///             &lt;include name="${nant.dir}\Install\NAnt.msm" /&gt;
        ///         &lt;/modules&gt;
        ///     &lt;/merge&gt;
        /// &lt;/mergemodules&gt;
        ///     </code>
        /// </example>
        /// <example>
        ///     <para>Add a Visual Studio .wid package (merge module) and specify some configurable items.</para>
        ///     <code>
        /// &lt;mergemodules&gt;
        ///   &lt;merge feature="F__DefaultFeature"&gt;
        ///     &lt;modules&gt;
        ///       &lt;include name="VsdReadmeDlg.wid" /&gt;
        ///     &lt;/modules&gt;
        ///     &lt;configurationitems&gt;
        ///       &lt;configurationitem name="BannerBitmap" value="CONFIGURED_BANNERBITMAP" /&gt;
        ///       &lt;configurationitem module="VsdReadmeDlg.wid" name="ReadmeText" value="CONFIGURED_READMETEXT" /&gt;
        ///     &lt;/configurationitems&gt;
        ///   &lt;/merge&gt;
        /// &lt;/mergemodules&gt;
        ///     </code>
        /// </example>
        /// </summary>
        [BuildElement("mergemodules", ProcessXml=false)]
        public SchemaElement[] MsiMergeModulesElement {
            get { return null; }
            set {}
        }

        #endregion

        #endregion

        #region Override implementation of SchemaValidatedTask

        /// <summary>
        /// Initializes task and verifies parameters.
        /// </summary>
        protected override void Initialize() {
            base.Initialize();

            _taskCommand = new MsiCreationCommand((msi) SchemaObject, this, 
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
