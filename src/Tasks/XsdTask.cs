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
using System.IO;
using System.Xml;
using System.Text;
using System.Resources;
using System.Collections.Specialized;

using NAnt.Core;
using NAnt.Core.Tasks;
using NAnt.DotNet.Tasks;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks
{
    /// <summary>Compiles an XML Schema into a Microsoft.NET Assembly
    /// containing types that can marshal back and forth from XML elements
    /// and the objects that represent them. Also can create a W3C XML
    /// schema from an existing Microsoft.NET Assembly, XML document,
    /// or an old XDR format schema.</summary>
    /// <example>
    ///   <para>Compile a schema.</para>
    ///   <code><![CDATA[<xsd schema="MySchema.xsd" element="MyRootElement"
    ///     language="CS" namespace="MyCompany.MySchema" outputdir="build\bin"
    ///     uri="http://MySchema'sTargetNamespace" />]]></code>
    ///   <para>Generate a schema from an Assembly.</para>
    ///   <code><![CDATA[<xsd assembly="MyAssembly.dll" outputdir="build\Schemas" />]]></code>
    ///   <para>Generate a schema from an XML doc.</para>
    ///   <code><![CDATA[<xsd xmldoc="MyDoc.xml" outputdir="build\Schemas" />]]></code>
    ///   <para>Generate a schema from an XDR schema.</para>
    ///   <code><![CDATA[<xsd xdr="MyOldSchema.xdr" outputdir="build\Schemas" />]]></code>
    /// </example>
    [TaskName("xsd")]
    [ProgramLocation(LocationType.FrameworkSdkDir)]
    public class XsdTask : ExternalProgramBase  {
        private string _args;
        string _schema = null;
        string _target = null;
        string _element = null;
        string _language = null;
        string _namespace = null;
        bool _nologo = false;
        string _outputdir = null;
        string _types = null;
        string _uri = null;
        string _assembly = null;
        string _xmldoc = null;
        string _xdr = null;

        /// <summary>XML Schema (.xsd) filename.</summary>
        [TaskAttribute("schema")]
        public string Schema {
            get { return _schema; }
            set { _schema = value; }
        }

        /// <summary>Target of XML Schema compilation. Can be "classes" or "dataset".</summary>
        [TaskAttribute("target")]
        public string Target
        {
            get { return (_target == "dataset" ? _target : "classes"); }
            set { _target = (value == "dataset" ? value : "classes"); }
        }

        /// <summary>XML element in the Schema to process.</summary>
        [TaskAttribute("element")]
        public string Element {
            get { return _element; }
            set { _element = value; }
        }

        /// <summary>Language of generated code. 'CS', 'VB', 'JS',
        /// or the fully-qualified name of a class implementing
        /// System.CodeDom.Compiler.CodeDomCompiler. </summary>
        [TaskAttribute("language")]
        public string Language {
            get { return _language; }
            set { _language = value; }
        }

        /// <summary>Microsoft.NET namespace of generated classes.</summary>
        [TaskAttribute("namespace")]
        public string Namespace
        {
            get { return _namespace; }
            set { _namespace = value; }
        }

        /// <summary>Suppresses the banner.</summary>
        [TaskAttribute("nologo")]
        [BooleanValidator()]
        public bool NoLogo
        {
            get { return _nologo; }
            set { _nologo = value; }
        }

        /// <summary>Output directory in which to place generated files.</summary>
        [TaskAttribute("outputdir")]
        public string OutputDir
        {
            get { return _outputdir; }
            set { _outputdir = value; }
        }

        /// <summary>Assembly (.dll or .exe) to generate a W3C XML Schema for.</summary>
        [TaskAttribute("assembly")]
        public string Assembly
        {
            get { return _assembly; }
            set { _assembly = value; }
        }

        /// <summary>Types in the assembly for which an XML schema is being created.
        /// If you don't specify this, all types in the assembly will be included.</summary>
        [TaskAttribute("types")]
        public string Types
        {
            get { return _types; }
            set { _types = value; }
        }

        /// <summary>Uri of elements from the Schema to process.</summary>
        [TaskAttribute("uri")]
        public string Uri
        {
            get { return _uri; }
            set { _uri = value; }
        }

        /// <summary>XML document to generate a W3C XML Schema for.</summary>
        [TaskAttribute("xmldoc")]
        public string XmlDoc
        {
            get { return _xmldoc; }
            set { _xmldoc = value; }
        }

        /// <summary>XDR schema to generate a W3C XML Schema for.</summary>
        [TaskAttribute("xdr")]
        public string Xdr
        {
            get { return _xdr; }
            set { _xdr = value; }
        }

        /// <summary>
        /// Arguments of program to execute
        /// </summary>
        public override string ProgramArguments
        {
            get
            {
                return _args;
            }
        }

        ///<summary>
        ///Initializes task and ensures the supplied attributes are valid.
        ///</summary>
        ///<param name="taskNode">Xml node used to define this task instance.</param>
        protected override void InitializeTask(System.Xml.XmlNode taskNode)
        {
        }

        protected override void ExecuteTask()
        {
            StringBuilder arguments = new StringBuilder();

            if (NoLogo)
            {
                arguments.Append("/nologo ");
            }

            if (Xdr != null)
            {
                Log(Level.Info, LogPrefix + "Converting {0} to W3C Schema", Xdr);

                arguments.Append(Xdr);

                if (OutputDir != null)
                {
                    arguments.Append(" /o:");
                    arguments.Append(OutputDir);
                }
            }
            else if (XmlDoc != null)
            {
                Log(Level.Info, LogPrefix + "Generating W3C Schema for XML Document {0}", XmlDoc);

                arguments.Append(XmlDoc);

                if (OutputDir != null)
                {
                    arguments.Append(" /o:");
                    arguments.Append(OutputDir);
                }
            }
            else if (Assembly != null)
            {
                Log(Level.Info, LogPrefix + "Generating W3C Schema for Assembly {0}", Assembly);

                arguments.Append(Assembly);

                if (OutputDir != null)
                {
                    arguments.Append(" /o:");
                    arguments.Append(OutputDir);
                }

                if (Types != null)
                {
                    arguments.Append(" /t:");
                    arguments.Append(Types);
                }
            }
            else if (Schema != null)
            {
                Log(Level.Info, LogPrefix + "Compiling Schema {0} into Microsoft.NET {1}", Schema, Target);

                arguments.Append(Schema);
                arguments.Append(" /");
                arguments.Append(Target);

                if (Element != null)
                {
                    arguments.Append(" /e:");
                    arguments.Append(Element);
                }
                if (Language != null)
                {
                    arguments.Append(" /l:");
                    arguments.Append(Language);
                }
                if (Namespace != null)
                {
                    arguments.Append(" /n:");
                    arguments.Append(Namespace);
                }

                if (OutputDir != null)
                {
                    arguments.Append(" /o:");
                    arguments.Append(OutputDir);
                }

                if (Uri != null)
                {
                    arguments.Append(" /u:");
                    arguments.Append(Uri);
                }
            }

            try
            {
                _args = arguments.ToString();

                base.ExecuteTask();
            }
            catch (Exception e)
            {
                throw new BuildException(LogPrefix + "ERROR: " + e);
            }

            if (Schema != null)
            {
                XmlDocument schemaDoc = new XmlDocument();
                schemaDoc.Load(Schema);

                ResourceWriter schemaWriter = new ResourceWriter(
                    Path.Combine(OutputDir, Namespace + ".resources"));
                schemaWriter.AddResource("schema", schemaDoc.DocumentElement.OuterXml);
                schemaWriter.Close();
            }
        }
    }
}
