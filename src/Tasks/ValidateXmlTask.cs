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

using System.Globalization;
using System.Xml;
using System.Xml.Schema;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

using NAnt.Contrib.Types;

namespace NAnt.Contrib.Tasks { 
    /// <summary>
    /// Validates a set of XML files based on a set of XML Schemas (XSD).
    /// </summary>
    /// <example>
    ///   <code>
    ///     <![CDATA[
    /// <validatexml>
    ///     <schemas>
    ///         <schema source="rcf-schema.xsd" />
    ///         <schema namespace="urn:schemas-company-com:base" source="base-schema.xsd" />
    ///     </schemas>
    ///     <files>
    ///         <include name="*.xml" />
    ///     </files>
    /// </validatexml>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("validatexml")]
    public class ValidateXmlTask : Task {
        #region Private Instance Fields

        private FileSet _xmlFiles = new FileSet();
        private int _numErrors = 0;
        private XmlSchemaReferenceCollection _schemas = new XmlSchemaReferenceCollection();

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The XML files that must be validated.
        /// </summary>
        [BuildElement("files", Required=true)]
        public FileSet XmlFiles {
            get { return _xmlFiles; }
            set { _xmlFiles = value; }
        }

        /// <summary>
        /// The XML Schemas (XSD) to use for validation.
        /// </summary>
        [BuildElementCollection("schemas", "schema")]
        public XmlSchemaReferenceCollection Schemas {
            get { return _schemas; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        /// <summary>
        /// This is where the work is done.
        /// </summary>
        protected override void ExecuteTask() {
            XmlSchemaCollection schemaCollection = new XmlSchemaCollection();

            foreach (XmlSchemaReference schema in Schemas) {
                try {
                    schemaCollection.Add(schema.Namespace, schema.Source);
                } catch (XmlSchemaException ex) {
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                        "Invalid XSD schema '{0}.", schema.Source), Location, ex);
                }
            }

            foreach (string file in XmlFiles.FileNames) {
                // initialize error counter
                _numErrors = 0;

                // output name of xml file that will be validated
                Log(Level.Info, "Validating '{0}'.", file);

                try {
                    ValidateFile(file, schemaCollection);
                } catch (XmlException ex) {
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                        "Invalid XML file '{0}'.", file), Location, ex);
                }

                if (_numErrors == 0) {
                    Log(Level.Info, "Document is valid.");
                } else {
                    if (!FailOnError) {
                        Log(Level.Info, "{0} validation errors in document.", _numErrors);
                    } else  {
                        throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                            "XML validation failed for document '{0}'.", file), Location);
                    }
                }
            }
        }

        #endregion Override implementation of Task

        #region Private Instance Methods

        private void ValidateFile(string file, XmlSchemaCollection schemas) {
            // load xml file
            XmlReader xmlReader = new XmlTextReader(file);

            // create validating reader
            XmlValidatingReader valReader = new XmlValidatingReader(xmlReader);

            // add user-specified schemas to validating reader
            valReader.Schemas.Add(schemas);

            // link validation event handler
            valReader.ValidationEventHandler += new ValidationEventHandler(OnValidationError);

            // read xml file
            while (valReader.Read()) {
            }

            // close reader
            valReader.Close();
        }

        private void OnValidationError(object sender, ValidationEventArgs args) {
            switch (args.Severity) {
                case XmlSeverityType.Error:
                    // increment error count
                    _numErrors++;
                    // output error message
                    Log(Level.Info, "Validation error: {0}", args.Message);
                    break;
                case XmlSeverityType.Warning:
                    // output error message
                    Log(Level.Info, "Validation warning: {0}", args.Message);
                    break;
            }
        }

        #endregion Private Instance Methods
    }
}
