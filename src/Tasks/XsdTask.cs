//
// NAntContrib
// Copyright (C) 2001-2002 Gerry Shaw
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

// Jayme C. Edwards (jedwards@wi.rr.com)

using System;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Text;
using System.Xml;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;
using NAnt.Core.Types;
using NAnt.Core.Util;

using NAnt.DotNet.Tasks;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// The <see cref="XsdTask" /> generates XML schema or common language runtime 
    /// classes from XDR, XML, and XSD files, or from classes in a runtime assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The following operations can be performed :
    /// </para>
    /// <list type="table">
    ///     <listheader>
    ///         <term>Operation</term>
    ///         <description>Description</description>
    ///     </listheader>
    ///     <item>
    ///         <term>XDR to XSD</term>
    ///         <description>
    ///             Generates an XML schema from an XML-Data-Reduced schema file. 
    ///             XDR is an early XML-based schema format. 
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>XML to XSD</term>
    ///         <description>
    ///             Generates an XML schema from an XML file.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>XSD to DataSet</term>
    ///         <description>
    ///             Generates common language runtime <see cref="DataSet" /> 
    ///             classes from an XSD schema file. The generated classes 
    ///             provide a rich object model for regular XML data. 
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>XSD to Classes</term>
    ///         <description>
    ///             Generates runtime classes from an XSD schema file. The 
    ///             generated classes can be used in conjunction with 
    ///             <see cref="System.Xml.Serialization.XmlSerializer" /> to 
    ///             read and write XML code that follows the schema. 
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>Classes to XSD</term>
    ///         <description>
    ///             Generates an XML schema from a type or types in a runtime 
    ///             assembly file. The generated schema defines the XML format 
    ///             used by <see cref="System.Xml.Serialization.XmlSerializer" />. 
    ///         </description>
    ///     </item>
    /// </list>
    /// </remarks>
    /// <example>
    ///   <para>Compile a XML Schema.</para>
    ///   <code>
    ///     <![CDATA[
    /// <xsd 
    ///     schema="MySchema.xsd" 
    ///     element="MyRootElement" 
    ///     language="CS" 
    ///     namespace="MyCompany.MySchema" 
    ///     outputdir="build\bin"
    ///     uri="http://MySchema'sTargetNamespace" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>Generate an XML Schema from an assembly.</para>
    ///   <code>
    ///     <![CDATA[
    /// <xsd assembly="MyAssembly.dll" outputdir="build\Schemas" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>Generate an XML Schema from an XML document.</para>
    ///   <code>
    ///     <![CDATA[
    /// <xsd xmldoc="MyDoc.xml" outputdir="build\Schemas" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>Generate an XML Schema from an XDR Schema.</para>
    ///   <code>
    ///     <![CDATA[
    /// <xsd xdr="MyOldSchema.xdr" outputdir="build\Schemas" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("xsd")]
    [ProgramLocation(LocationType.FrameworkSdkDir)]
    public class XsdTask : ExternalProgramBase {
        #region Private Instance Fields

        private FileInfo _schema;
        private string _target;
        private string _element;
        private string _language;
        private string _namespace;
        private DirectoryInfo _outputdir;
        private string _types;
        private string _uri;
        private FileInfo _assembly;
        private FileInfo _xmldoc;
        private FileInfo _xdr;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// XML Schema (.xsd) filename.
        /// </summary>
        [TaskAttribute("schema")]
        public FileInfo Schema {
            get { return _schema; }
            set { _schema = value; }
        }

        /// <summary>
        /// Target of XML Schema compilation - either <c>classes</c> or 
        /// <c>dataset</c>. The default is <c>classes</c>.
        /// </summary>
        [TaskAttribute("target")]
        public string Target {
            get { return (_target == "dataset" ? _target : "classes"); }
            set { _target = (value == "dataset" ? value : "classes"); }
        }

        /// <summary>
        /// XML element in the Schema to process.
        /// </summary>
        /// <remarks>
        /// TO-DO : turn this into collection of elements !
        /// </remarks>
        [TaskAttribute("element")]
        public string Element {
            get { return _element; }
            set { _element = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The language to use for the generated code - either <c>CS</c>, 
        /// <c>VB</c>, <c>JS</c>, <c>VJC</c> or the fully-qualified name of a 
        /// class implementing <see cref="System.CodeDom.Compiler.CodeDomProvider" />.
        /// </summary>
        [TaskAttribute("language")]
        public string Language {
            get { return _language; }
            set { _language = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Specifies the runtime namespace for the generated types. The default 
        /// namespace is <c>Schemas</c>.
        /// </summary>
        [TaskAttribute("namespace")]
        public string Namespace {
            get { return _namespace; }
            set { _namespace = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The output directory in which to place generated files.
        /// </summary>
        [TaskAttribute("outputdir")]
        public DirectoryInfo OutputDir {
            get { return _outputdir; }
            set { _outputdir = value; }
        }

        /// <summary>
        /// Assembly (.dll or .exe) to generate an XML Schema for.
        /// </summary>
        [TaskAttribute("assembly")]
        public FileInfo Assembly {
            get { return _assembly; }
            set { _assembly = value; }
        }

        /// <summary>
        /// Types in the assembly for which an XML schema is being created.
        /// By default all types in the assembly will be included.
        /// </summary>
        /// <remarks>
        /// TO-DO : turn this into collection of types !
        /// </remarks>
        [TaskAttribute("types")]
        public string Types {
            get { return _types; }
            set { _types = value; }
        }

        /// <summary>
        /// Specifies the URI for the elements in the <see cref="Schema" /> to 
        /// generate code for. 
        /// </summary>
        [TaskAttribute("uri")]
        public string Uri {
            get { return _uri; }
            set { _uri = value; }
        }

        /// <summary>
        /// XML document to generate an XML Schema for.
        /// </summary>
        [TaskAttribute("xmldoc")]
        public FileInfo XmlDoc {
            get { return _xmldoc; }
            set { _xmldoc = value; }
        }

        /// <summary>
        /// XDR Schema to generate an XML Schema for.
        /// </summary>
        [TaskAttribute("xdr")]
        public FileInfo Xdr {
            get { return _xdr; }
            set { _xdr = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Element

        /// <summary>
        /// Validates the <see cref="Task" />.
        /// </summary>
        protected override void Initialize() {
            if (Xdr == null && XmlDoc == null && Assembly == null && Schema == null) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Either the 'xdr', 'xmldoc', 'assembly' or 'schema' attribute"
                    + " of <{0} ... /> should be specified.", this.Name), Location);
            }
        }

        #endregion Override implementation of Element

        #region Override implementation of ExternalProgramBase

        /// <summary>
        /// Gets a value indiciating whether the external program is a managed
        /// application which should be executed using a runtime engine, if 
        /// configured. 
        /// </summary>
        /// <value>
        /// <see langword="ManagedExecutionMode.Auto" />.
        /// </value>
        /// <remarks>
        /// Modifying this property has no effect.
        /// </remarks>
        public override ManagedExecution Managed {
            get { return ManagedExecution.Auto; }
            set { }
        }

        /// <summary>
        /// Gets the command-line arguments for the external program.
        /// </summary>
        /// <value>
        /// The command-line arguments for the external program.
        /// </value>
        public override string ProgramArguments { 
            get { return string.Empty; }
        }

        protected override void ExecuteTask() {
            if (Xdr != null) {
                Log(Level.Info, "Generating XML Schema from XDR Schema '{0}'.", 
                    Xdr.FullName);

                Arguments.Add(new Argument(Xdr));

                if (OutputDir != null) {
                    Arguments.Add(new Argument(string.Format(
                        CultureInfo.InvariantCulture, "/o:\"{0}\"", 
                        OutputDir.FullName)));
                }
            } else if (XmlDoc != null) {
                Log(Level.Info, "Generating XML Schema for XML document '{0}'.", 
                    XmlDoc.FullName);

                Arguments.Add(new Argument(XmlDoc));

                if (OutputDir != null) {
                    Arguments.Add(new Argument(string.Format(
                        CultureInfo.InvariantCulture, "/o:\"{0}\"", 
                        OutputDir.FullName)));
                }
            } else if (Assembly != null) {
                Log(Level.Info, "Generating XML Schema for assembly '{0}'.", 
                    Assembly.FullName);

                Arguments.Add(new Argument(Assembly));

                if (OutputDir != null) {
                    Arguments.Add(new Argument(string.Format(
                        CultureInfo.InvariantCulture, "/o:\"{0}\"", 
                        OutputDir.FullName)));
                }

                if (Types != null) {
                    Arguments.Add(new Argument(string.Format(
                        CultureInfo.InvariantCulture, "/t:\"{0}\"", 
                        Types)));
                }
            } else if (Schema != null) {
                Log(Level.Info, "Compiling XML Schema '{0}' into Microsoft.NET {1}.", 
                    Schema.FullName, Target);

                Arguments.Add(new Argument(Schema));

                // set target (classes or dataset)
                Arguments.Add(new Argument("/" + Target));

                if (Element != null) {
                    Arguments.Add(new Argument(string.Format(
                        CultureInfo.InvariantCulture, "/e:\"{0}\"", 
                        Element)));
                }
                if (Language != null) {
                    Arguments.Add(new Argument(string.Format(
                        CultureInfo.InvariantCulture, "/l:\"{0}\"", 
                        Language)));
                }
                if (Namespace != null) {
                    Arguments.Add(new Argument(string.Format(
                        CultureInfo.InvariantCulture, "/n:\"{0}\"", 
                        Namespace)));
                }

                if (OutputDir != null) {
                    Arguments.Add(new Argument(string.Format(
                        CultureInfo.InvariantCulture, "/o:\"{0}\"", 
                        OutputDir.FullName)));
                }

                if (Uri != null) {
                    Arguments.Add(new Argument(string.Format(
                        CultureInfo.InvariantCulture, "/u:\"{0}\"", 
                        Uri)));
                }
            }

            // suppress logo-banner
            Arguments.Add(new Argument("/nologo"));

            try {
                base.ExecuteTask();
            } catch (Exception ex) {
                if (Xdr != null || XmlDoc != null || Assembly != null) {
                    throw new BuildException("Error generating XML Schema.", 
                        Location, ex);
                } else {
                    throw new BuildException("Error compiling XML Schema.", 
                        Location, ex);
                }
            }
        }

        #endregion Override implementation of ExternalProgramBase
    }
}
