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
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;

using SourceForge.NAnt.Attributes;
using SourceForge.NAnt;

using NAnt.Contrib.Util;

namespace NAnt.Contrib.Tasks 
{ 

   /// <summary>
   /// A task that Validates a set of XML files based on a
   /// set of Schemas (XSD)
   /// </summary>
   /// <remarks>
   /// This task takes a set of input xml files in a fileset
   /// and a set of schemas into an OptionSet, and validates
   /// them. Right now, there's no way to specify more than one
   /// schema to use the targetNamespace property (soon to come).
   /// 
   /// Note that if the name attribite of a schema is an empty
   /// string, then the system will use the targetNamespace attribute
   /// of the underlying schema to associate it with a namespace.
   /// 
   /// You can use the failonerror attribute of the task to control
   /// whether a validation failure will stop the build or not.
   /// </remarks>
   /// <example>
   ///   <code><![CDATA[
   ///      <validatexml>
   ///         <schemas>
   ///            <schemaref source="rcf-schema.xsd"/>
   ///            <schemaref namespace="urn:schemas-company-com:base" source="base-schema.xsd"/>
   ///          </schemas>
   ///         <files>
   ///            <includes name="*.xml"/>
   ///         </files>
   ///      </validatexml>
   ///   ]]></code>
   /// </example>
   [TaskName("validatexml")]
   public class ValidateXmlTask : Task
   {
      private FileSet _xmlFiles = new FileSet();
      private int _numErrors = 0;
      private SchemaSet _schemas = new SchemaSet();

      /// <summary>
      /// Set of XML files to use as input
      /// </summary>
      [FileSet("files", Required=true)]
      public FileSet XmlFiles {
         get { return _xmlFiles; }
      }

      [SchemaSetAttribute("schemas", Required=true)]
      public SchemaSet Schemas {
         get { return _schemas; }
      }



      ///<summary>
      ///Initializes task and ensures the supplied attributes are valid.
      ///</summary>
      ///<param name="taskNode">Xml node used to define this task instance.</param>
      protected override void InitializeTask(System.Xml.XmlNode taskNode) 
      {
         if (XmlFiles.FileNames.Count == 0) {
            throw new BuildException("ValidateXml fileset cannot be empty!", Location);
         }

         if ( _schemas.Schemas.Count == 0 ) {
            throw new BuildException("ValidateXml at least one schema must be specified", Location);
         }

      }

      /// <summary>
      /// This is where the work is done
      /// </summary>
      protected override void ExecuteTask() 
      {
         foreach ( string file in XmlFiles.FileNames ) {
            Log.WriteLine(LogPrefix + "Validating " + file);
            Log.Indent();
            try {
               ValidateFile(file);
            } catch ( XmlException ex ) {
               throw new BuildException("Invalid XML file: " + ex.Message, Location);
            }
            Log.Unindent();

            if ( _numErrors == 0 ) {
               Log.WriteLine(LogPrefix + "Document is valid");
            } else {
               if ( !FailOnError ) {
                  Log.WriteLine(LogPrefix + _numErrors + " Errors in document");
               } else  {
                  string msg = string.Format("Invalid XML Document '{0}'", file);
                  throw new BuildException(msg, Location);
               }
            }
         }
      }

      private void ValidateFile(string file)
      {
         XmlReader xmlReader = new XmlTextReader(file);
         XmlValidatingReader valReader = new XmlValidatingReader(xmlReader);

         valReader.Schemas.Add(_schemas.Schemas);

         valReader.ValidationEventHandler += 
            new ValidationEventHandler(OnValidationError);

         while ( valReader.Read() ) 
         { 
         }

         valReader.Close();
      }

      private void OnValidationError(object sender, ValidationEventArgs args)
      {
         _numErrors++;
         Log.WriteLine(LogPrefix + "Validation Error: {0}", args.Message);
      }


   } // class ValidateXmlTask



    /// <summary>Indicates that field should be treated as a xml schema set for the task.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited=true)]
    public class SchemaSetAttribute : BuildElementAttribute 
    {     

        public SchemaSetAttribute(string name) : base(name) 
        {        
        }      
    }

   /// <summary>
   /// Represents the schema collection element
   /// </summary>
   public class SchemaSet : Element
   {
      private XmlSchemaCollection _schemas = null;
      
      /// <summary>
      /// Schemas in this element
      /// </summary>
      public XmlSchemaCollection Schemas {
         get { return _schemas; }
      }

      /// <summary>
      /// Initialize this element node
      /// </summary>
      /// <param name="elementNode"></param>
      protected override void InitializeElement(XmlNode elementNode)  
      {
         try
         {
            //
            // Check out whatever <schemaref> elements there are
            //
            _schemas = new XmlSchemaCollection();
            foreach ( XmlNode node in elementNode ) 
            {
               if ( node.Name.Equals("schemaref") ) {
                  SchemaRefElement v = new SchemaRefElement();
                  v.Project = Project;
                  v.Initialize(node);
                  _schemas.Add(v.Namespace, v.Source);
               }
            }
         } catch ( XmlSchemaException xse ) {
            throw new BuildException("Invalid Schema: " + xse.Message, Location);
         } 
      }

   } // class SchemaSet


   /// <summary>
   /// Allows the specification of a namespace/schema location pair
   /// </summary>
   [ElementName("schemaref")]
   public class SchemaRefElement : Element
   {
      private string _namespace = null;
      private string _source = null;

      /// <summary>
      /// Namespace URI associated with this schema. 
      /// If not present, it is assumed that the 
      /// schema's targetNamespace value is to be used.
      /// </summary>
      [TaskAttribute("namespace")]
      public string Namespace {
         get { return _namespace; }
         set { _namespace = value; }
      }

      /// <summary>
      /// Location of this schema. Could be a 
      /// local file path or an http URL.
      /// </summary>
      [TaskAttribute("source", Required=true)]
      public string Source {
         get { return _source; }
         set { _source = value; }
      }

   } // class SchemaRefElement


} // namespace NAnt.Contrib.Tasks