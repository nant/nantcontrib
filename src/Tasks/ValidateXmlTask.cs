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
   /// set of Schemas (XSD, XDR and DTD)
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
   /// </remarks>
   /// <example>
   ///   <code><![CDATA[
   ///      <validatexml>
   ///         <schemas>
   ///            <option name="" value="rcf-schema.xsd"/>
   ///          </schemas>
   ///         <xmlfiles>
   ///            <includes name="rcf_example.xml"/>
   ///         </xmlfiles>
   ///      </validatexml>
   ///   ]]></code>
   /// </example>
   [TaskName("validatexml")]
   public class ValidateXmlTask : Task
   {
      private FileSet _xmlFiles = new FileSet();
      private OptionSet _schemas = new OptionSet();
      private int _numErrors = 0;

      /// <summary>
      /// Set of XML files to use as input
      /// </summary>
      [FileSet("xmlfiles")]
      public FileSet XmlFiles {
         get { return _xmlFiles; }
      }

      [OptionSetAttribute("schemas")]
      public OptionSet Schemas {
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
      }

      /// <summary>
      /// This is where the work is done
      /// </summary>
      protected override void ExecuteTask() 
      {
         foreach ( string file in XmlFiles.FileNames ) {
            Log.WriteLine(LogPrefix + "Validating " + file);
            Log.Indent();
               ValidateFile(file);
            Log.Unindent();

            if ( _numErrors == 0 ) {
               Log.WriteLine(LogPrefix + "Document is valid");
            } else {
               Log.WriteLine(LogPrefix + _numErrors + " Errors in document");
            }
         }
      }

      private void ValidateFile(string file)
      {
         XmlReader xmlReader = new XmlTextReader(file);
         XmlValidatingReader valReader = new XmlValidatingReader(xmlReader);
         
         foreach ( OptionValue schema in Schemas ) {
            if ( schema.Name == "" ) 
               valReader.Schemas.Add(null, schema.Value);
            else
               valReader.Schemas.Add(schema.Name, schema.Value);
         }

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

} // namespace NAnt.Contrib.Tasks