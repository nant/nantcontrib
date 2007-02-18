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
using System.Web.Mail;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace NAnt.Contrib.Tasks { 
    /// <summary>
    /// A task that generates a summary HTML
    /// from a set of NUnit xml report files.
    /// Loosely based on Erik Hatcher JUnitReport for Ant.
    /// </summary>
    /// <remarks>
    /// This task can generate a combined HTML report out of a
    /// set of NUnit result files generated using the 
    /// XML Result Formatter.
    /// 
    /// By default, NUnitReport will generate the combined
    /// report using the NUnitSummary.xsl file located at the
    /// assembly's location, but you can specify a different
    /// XSLT template to use with the <code>xslfile</code>
    /// attribute.
    /// 
    /// Also, all the properties defined in the current
    /// project will be passed down to the XSLT file as 
    /// template parameters, so you can access properties
    /// such as nant.project.name, nant.version, etc.
    /// </remarks>
    /// <example>
    ///   <code><![CDATA[
    ///   <nunitreport 
    ///         out="${outputdir}\TestSummary.html"
    ///         >
    ///      <fileset>
    ///         <include name="${outputdir}\Test-*.xml" />
    ///      </fileset>
    ///   </nunitreport>
    ///   
    ///   ]]></code>
    /// </example>
    [TaskName("nunitreport")]
    public class NUnitReportTask : Task {
        private const string XSL_DEF_FILE = "mres://./NUnitSummary.xsl";
        private string _outFilename;
        private FileSet _fileset = new FileSet();
        private string _xsl;

        /// <summary>
        /// Name of Output HTML file.
        /// </summary>
        [TaskAttribute("out", Required=true)]
        public string OutFilename { 
            get { return _outFilename; } 
            set { _outFilename = value; } 
        }
        
        /// <summary>
        /// XSLT file used to generate the report.
        /// </summary>
        [TaskAttribute("xslfile", Required=false)]
        public string XslFile { 
            get { return _xsl; } 
            set { _xsl = value; } 
        }

        /// <summary>
        /// Set of XML files to use as input
        /// </summary>
        [BuildElement("fileset")]
        public FileSet XmlFileSet {
            get { return _fileset; }
            set { _fileset = value; }
        }

        /// <summary>
        /// Initializes task and ensures the supplied attributes are valid.
        /// </summary>
        protected override void Initialize() {
            if (OutFilename == null) {
                throw new BuildException("NUnitReport attribute \"out\" is required.", Location);
            }

            if (XmlFileSet.FileNames.Count == 0) {
                throw new BuildException("NUnitReport fileset cannot be empty!", Location);
            }

        }

        /// <summary>
        /// This is where the work is done
        /// </summary>
        protected override void ExecuteTask() {
            XmlDocument summary = CreateSummaryXmlDoc();

            foreach ( string file in XmlFileSet.FileNames ) {
                XmlDocument source = new XmlDocument();
                source.Load(file);
                XmlNode node = summary.ImportNode(source.DocumentElement, true);
                summary.DocumentElement.AppendChild(node);
            }

            //
            // prepare properties and transform
            //
            XsltArgumentList args = GetPropertyList();
            XslTransform xslt = LoadTransform();
            XmlTextWriter writer = new XmlTextWriter(OutFilename, null);
            xslt.Transform(summary, args, writer);
            writer.Close();
        }


        /// <summary>
        /// Initializes the XmlDocument instance
        /// used to summarize the test results
        /// </summary>
        /// <returns></returns>
        private XmlDocument CreateSummaryXmlDoc() {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("testsummary");
            root.SetAttribute("created", DateTime.Now.ToString());
            doc.AppendChild(root);
            return doc;
        }

        /// <summary>
        /// Builds an XsltArgumentList with all
        /// the properties defined in the 
        /// current project as XSLT parameters.
        /// </summary>
        /// <returns>Property List</returns>
        private XsltArgumentList GetPropertyList() {
            XsltArgumentList args = new XsltArgumentList();

            foreach ( DictionaryEntry entry in this.Project.Properties ) {
                string value = (entry.Value!=null) ? (string)entry.Value : "";
                args.AddParam((string)entry.Key, "", value);
            }
            return args;
        }

        /// <summary>
        /// Loads the XSLT Transform
        /// </summary>
        /// <remarks>
        /// This method will load the file specified
        /// through the the xslfile attribute, or
        /// the default transformation included
        /// as a managed resource.
        /// </remarks>
        /// <returns>The Transformation to use</returns>
        private XslTransform LoadTransform() {
            XslTransform xslt = new XslTransform();
            if ( XslFile != null ) {
                xslt.Load(XslFile);
            } else {
                XmlResolver resolver = new LocalResXmlResolver();
                Stream stream = 
                    (Stream)resolver.GetEntity(new Uri(XSL_DEF_FILE), null, null);
                XmlTextReader reader = new XmlTextReader(XSL_DEF_FILE, stream);
                xslt.Load(reader, resolver);
            }
            return xslt;
        }


        /// <summary>
        /// Custom XmlResolver used to load the 
        /// XSLT files out of this assembly resources.
        /// </summary>
        internal class LocalResXmlResolver : XmlUrlResolver {
            const string SCHEME_MRES = "mres";

            /// <summary>
            /// Loads the XSLT file
            /// </summary>
            /// <param name="absoluteUri"></param>
            /// <param name="role"></param>
            /// <param name="objToReturn"></param>
            /// <returns></returns>
            public override object GetEntity(Uri absoluteUri, string role, Type objToReturn) {
                if ( absoluteUri.Scheme != SCHEME_MRES ) {
                    // we don't know how to handle this URI scheme....
                    return base.GetEntity(absoluteUri, role, objToReturn);
                }
                Assembly thisAssm = Assembly.GetExecutingAssembly();
                string filename = absoluteUri.Segments[absoluteUri.Segments.Length-1];
                return thisAssm.GetManifestResourceStream(filename);
            }
        }
    }
}
