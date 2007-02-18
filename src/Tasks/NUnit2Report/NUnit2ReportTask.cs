// NUnit2ReportTask.cs
//
//    Loosely based on Tomas Restrepo NUnitReport for NAnt.
//    Loosely based on Erik Hatcher JUnitReport for Ant.
//
// Author:
//    Gilles Bayon (gilles.bayon@laposte.net)
//
// Copyright (C) 2003 Gilles Bayon
//
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
// Gilles Bayon (gilles.bayon@laposte.net)
// Ian Maclean (imaclean@gmail.com)
// Gert Driesen (drieseng@users.sourceforge.net)

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;
using NAnt.Core.Util;

using NAnt.Contrib.Types.NUnit2Report;

namespace NAnt.Contrib.Tasks.NUnit2Report {
    /// <summary>
    /// A task that generates a summary HTML
    /// from a set of NUnit xml report files.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   This task can generate a combined HTML report out of a set of NUnit
    ///   result files generated using the XML Result Formatter.
    ///   </para>
    ///   <para>
    ///   All the properties defined in the current project will be passed
    ///   down to the XSLT file as template parameters, so you can access 
    ///   properties such as nant.project.name, nant.version, etc.
    ///   </para>
    /// </remarks>
    /// <example>
    ///   <code>
    ///     <![CDATA[
    /// <nunit2report todir="${outputdir}">
    ///     <fileset>
    ///         <includes name="${outputdir}\results.xml" />
    ///     </fileset>
    /// </nunit2report>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("nunit2report")]
    public class NUnit2ReportTask : Task  {
        #region Private Static Fields

        private static readonly ArrayList _resFiles = new ArrayList(new string[3]{"toolkit.xsl", "Traductions.xml", "NUnit-Frame.xsl"});  

        #endregion Private Static Fields

        #region Private Instance Fields

        private DirectoryInfo _toDir;
        private FileSet _fileset = new FileSet();
        private XmlDocument _fileSetSummary;
        private FileSet _summaries = new FileSet();
        private string _tempXmlFileSummarie = "";
     
        private FileInfo _xslFile;
        private string _openDescription ="no";
        private string _language = "";
        private ReportFormat _format = ReportFormat.NoFrames;
        private XsltArgumentList _xsltArgs;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The format of the generated report. The default is 
        /// <see cref="ReportFormat.NoFrames" />.
        /// </summary>
        [TaskAttribute("format")]
        public ReportFormat Format {
            get { return _format; }
            set { _format = value; }
        }

        /// <summary>
        /// The output language.
        /// </summary>
        [TaskAttribute("lang")]
        public string Language {
            get { return _language; }
            set { _language = value; }
        }

        /// <summary>
        /// Open all description method. Default to "false".
        /// </summary>
        [TaskAttribute("opendesc")]
        public string OpenDescription {
            get { return _openDescription; }
            set { _openDescription = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The directory where the files resulting from the transformation
        /// should be written to. The default is the project's base directory.
        /// </summary>
        [TaskAttribute("todir")]
        public DirectoryInfo ToDir {
            get {
                if (_toDir == null) {
                    return new DirectoryInfo(Project.BaseDirectory);
                }
                return _toDir;
            }
            set { _toDir = value; }
        }

        /// <summary>
        /// Set of XML files to use as input
        /// </summary>
        [BuildElement("fileset")]
        public FileSet XmlFileSet {
            get { return _fileset; }
        }

        /// <summary>
        /// Set of summary XML files to use as input.
        /// </summary>
        //[BuildElement("summaries")]
        public FileSet XmlSummaries {
            get { return _summaries; }
        }

        /// <summary>
        /// XSLT file used to generate the report if <see cref="Format" /> is 
        /// <see cref="ReportFormat.NoFrames" />.
        /// </summary>
        [TaskAttribute("xslfile")]
        public FileInfo XslFile {
            get { return _xslFile; }
            set { _xslFile = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        /// <summary>
        /// Initializes task and ensures the supplied attributes are valid.
        /// </summary>
        protected override void Initialize() {
            if (Format == ReportFormat.NoFrames) {
                if (XslFile != null && !XslFile.Exists) {
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                        "The XSLT file \"{0}\" could not be found.",
                        XslFile.FullName), Location);
                }
            }

            if (XmlFileSet.FileNames.Count == 0) {
                throw new BuildException("NUnitReport fileset cannot be empty!", Location);
            }

            foreach (string file in XmlSummaries.FileNames) {
                _tempXmlFileSummarie = file;
            }

            // Get the NAnt, OS parameters
            _xsltArgs = GetPropertyList();
        }

        /// <summary>
        /// This is where the work is done
        /// </summary>
        protected override void ExecuteTask() {
            _fileSetSummary = CreateSummaryXmlDoc();
    
            foreach (string file in XmlFileSet.FileNames) {
                XmlDocument source = new XmlDocument();
                source.Load(file);
                XmlNode node = _fileSetSummary.ImportNode(source.DocumentElement, true);
                _fileSetSummary.DocumentElement.AppendChild(node);
            }

            Log(Level.Info, "Generating report...");

            try {
                // ensure destination directory exists
                if (!ToDir.Exists) {
                    ToDir.Create();
                    ToDir.Refresh();
                }

                if (Format == ReportFormat.NoFrames) {
                    XslTransform xslTransform = new XslTransform();
                    XmlResolver resolver = new LocalResXmlResolver();

                    if (XslFile != null) {
                        xslTransform.Load(LoadStyleSheet(XslFile), resolver);
                    } else {
                        xslTransform.Load(LoadStyleSheet("NUnit-NoFrame.xsl"), resolver);
                    }

                    // xmlReader hold the first transformation
                    XmlReader xmlReader = xslTransform.Transform(_fileSetSummary, _xsltArgs);

                    // i18n
                    XsltArgumentList xsltI18nArgs = new XsltArgumentList();
                    xsltI18nArgs.AddParam("lang", "",Language);

                    // Load the i18n stylesheet
                    XslTransform xslt = new XslTransform();
                    xslt.Load(LoadStyleSheet("i18n.xsl"), resolver);

                    XPathDocument xmlDoc;
                    xmlDoc = new XPathDocument(xmlReader);

                    XmlTextWriter writerFinal = new XmlTextWriter(
                        Path.Combine(ToDir.FullName, "index.html"), 
                        Encoding.GetEncoding("ISO-8859-1"));

                    // Apply the second transform to xmlReader to final ouput
                    xslt.Transform(xmlDoc, xsltI18nArgs, writerFinal);

                    xmlReader.Close();
                    writerFinal.Close();
                } else {
                    XmlTextReader reader = null;

                    try {
                        // create the index.html
                        StringReader stream = new StringReader("<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='1.0' >" +
                            "<xsl:output method='html' indent='yes' encoding='ISO-8859-1'/>" +
                            "<xsl:include href=\"NUnit-Frame.xsl\"/>" +
                            "<xsl:template match=\"test-results\">" +
                            "   <xsl:call-template name=\"index.html\"/>" +
                            " </xsl:template>" +
                            " </xsl:stylesheet>");
                        Write (stream, Path.Combine(ToDir.FullName, "index.html"));

                        // create the stylesheet.css
                        stream = new StringReader("<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='1.0' >" +
                            "<xsl:output method='html' indent='yes' encoding='ISO-8859-1'/>" +
                            "<xsl:include href=\"NUnit-Frame.xsl\"/>" +
                            "<xsl:template match=\"test-results\">" +
                            "   <xsl:call-template name=\"stylesheet.css\"/>" +
                            " </xsl:template>" +
                            " </xsl:stylesheet>");
                        Write (stream, Path.Combine(ToDir.FullName, "stylesheet.css"));

                        // create the overview-summary.html at the root
                        stream = new StringReader("<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='1.0' >" +
                            "<xsl:output method='html' indent='yes' encoding='ISO-8859-1'/>" +
                            "<xsl:include href=\"NUnit-Frame.xsl\"/>" +
                            "<xsl:template match=\"test-results\">" +
                            "    <xsl:call-template name=\"overview.packages\"/>" +
                            " </xsl:template>" +
                            " </xsl:stylesheet>");
                        Write (stream, Path.Combine(ToDir.FullName, "overview-summary.html"));

                        // create the allclasses-frame.html at the root
                        stream = new StringReader("<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='1.0' >" +
                            "<xsl:output method='html' indent='yes' encoding='ISO-8859-1'/>" +
                            "<xsl:include href=\"NUnit-Frame.xsl\"/>" +
                            "<xsl:template match=\"test-results\">" +
                            "    <xsl:call-template name=\"all.classes\"/>" +
                            " </xsl:template>" +
                            " </xsl:stylesheet>");
                        Write (stream, Path.Combine(ToDir.FullName, "allclasses-frame.html"));

                        // create the overview-frame.html at the root
                        stream = new StringReader("<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='1.0' >" +
                            "<xsl:output method='html' indent='yes' encoding='ISO-8859-1'/>" +
                            "<xsl:include href=\"NUnit-Frame.xsl\"/>" +
                            "<xsl:template match=\"test-results\">" +
                            "    <xsl:call-template name=\"all.packages\"/>" +
                            " </xsl:template>" +
                            " </xsl:stylesheet>");
                        Write (stream, Path.Combine(ToDir.FullName, "overview-frame.html"));

                        XPathNavigator xpathNavigator = _fileSetSummary.CreateNavigator();

                        // Get All the test suite containing test-case.
                        XPathExpression expr = xpathNavigator.Compile("//test-suite[(child::results/test-case)]");

                        XPathNodeIterator iterator = xpathNavigator.Select(expr);
                        while (iterator.MoveNext()) {
                            // output directory
                            string path = "";

                            XPathNavigator xpathNavigator2 = iterator.Current;
                            string testSuiteName = iterator.Current.GetAttribute("name", "");
                               
                            // Get get the path for the current test-suite.
                            XPathNodeIterator iterator2 = xpathNavigator2.SelectAncestors("", "", true);
                            string parent = "";
                            int parentIndex = -1;

                            while (iterator2.MoveNext()) {
                                string directory = iterator2.Current.GetAttribute("name","");
                                if (directory != "" && directory.IndexOf(".dll") < 0) {
                                    path = directory + "/" + path;
                                }
                                if (parentIndex == 1) {
                                    parent = directory;
                                }
                                parentIndex++;
                            }

                            // resolve to absolute path
                            path = Path.Combine(ToDir.FullName, path);

                            // ensure directory exists
                            if (!Directory.Exists(path)) {
                                Directory.CreateDirectory(path);
                            }

                            // Build the "testSuiteName".html file
                            // Correct MockError duplicate testName !
                            // test-suite[@name='MockTestFixture' and ancestor::test-suite[@name='Assemblies'][position()=last()]]

                            stream = new StringReader("<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='1.0' >" +
                                "<xsl:output method='html' indent='yes' encoding='ISO-8859-1'/>" +
                                "<xsl:include href=\"NUnit-Frame.xsl\"/>" +
                                "<xsl:template match=\"/\">" +
                                "    <xsl:for-each select=\"//test-suite[@name='"+testSuiteName+"' and ancestor::test-suite[@name='"+parent+"'][position()=last()]]\">" +
                                "        <xsl:call-template name=\"test-case\">" +
                                "            <xsl:with-param name=\"dir.test\">"+String.Join(".", path.Split('/'))+"</xsl:with-param>" +
                                "        </xsl:call-template>" +
                                "    </xsl:for-each>" +
                                " </xsl:template>" +
                                " </xsl:stylesheet>");
                            Write(stream, Path.Combine(path, testSuiteName + ".html"));

                            Log(Level.Debug,"dir={0} Generating {1}.html", path, testSuiteName);
                        }
                    } finally {
                        Log(Level.Debug, "Processing of stream complete.");

                        // Finished with XmlTextReader
                        if (reader != null) {
                            reader.Close();
                        }
                    }
                }
            } catch (Exception ex) {
                throw new BuildException("Failure generating report.", Location, ex);
            }
        }

        #endregion Override implementation of Task

        #region Private Instance Methods

        /// <summary>
        /// Load a stylesheet from the assemblies resource stream.
        /// </summary>
        ///<param name="xslFileName">File name of the file to extract.</param>
        protected XPathDocument LoadStyleSheet(string xslFileName) {
            Stream xsltStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                string.Format("xslt.{0}", xslFileName) );
            if (xsltStream == null) {
                throw new BuildException(string.Format("Missing '{0}' resource stream",
                    xslFileName), Location);
            }

            XmlTextReader xtr = new XmlTextReader(xsltStream, XmlNodeType.Document, null);
            return new XPathDocument(xtr);
        }

        /// <summary>
        /// Load a stylesheet from the file system.
        /// </summary>
        ///<param name="xslFile">The XSLT file to load.</param>
        protected XPathDocument LoadStyleSheet(FileInfo xslFile) {
            Stream stream = new FileStream(xslFile.FullName, FileMode.Open,
                FileAccess.Read);

            XmlTextReader xtr = new XmlTextReader(stream);
            return new XPathDocument(xtr);
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
        /// <returns></returns>
        private XsltArgumentList GetPropertyList() {
            XsltArgumentList args = new XsltArgumentList();

            Log(Level.Verbose, "Processing XsltArgumentList");
            foreach (DictionaryEntry entry in Project.Properties) {
                string value = entry.Value as string;
                if (value != null) {
                    args.AddParam((string) entry.Key, "", value);
                }
            }

            // Add argument to the C# XML comment file
            args.AddParam("summary.xml", "", _tempXmlFileSummarie);
            // Add open.description argument
            args.AddParam("open.description", "", OpenDescription);
   
            return args;
        }

        /// <summary>
        ///  Run the transform and output to filename
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        private void Write(StringReader stream, string fileName) {
            XmlTextReader reader = null;

            // Load the XmlTextReader from the stream
            reader = new XmlTextReader(stream);
            XslTransform xslTransform = new XslTransform();
            XmlResolver resolver = new LocalResXmlResolver();

            //Load the stylesheet from the stream.
            xslTransform.Load(reader, resolver);

            XPathDocument xmlDoc;
            
            // xmlReader hold the first transformation
            XmlReader xmlReader = xslTransform.Transform(_fileSetSummary, _xsltArgs);

            // i18n
            XsltArgumentList xsltI18nArgs = new XsltArgumentList();
            xsltI18nArgs.AddParam("lang", "", Language);

            XslTransform xslt = new XslTransform();

            // Load the stylesheet.
            xslt.Load(LoadStyleSheet("i18n.xsl"), resolver);

            xmlDoc = new XPathDocument(xmlReader);

            XmlTextWriter writerFinal = new XmlTextWriter(fileName, Encoding.GetEncoding("ISO-8859-1"));
            // Apply the second transform to xmlReader to final ouput
            xslt.Transform(xmlDoc, xsltI18nArgs, writerFinal);

            xmlReader.Close();
            writerFinal.Close();
        }

        #endregion Private Instance Methods

        /// <summary>
        /// Custom XmlResolver used to load the 
        /// XSLT files out of this assembly resources.
        /// </summary>
        internal class LocalResXmlResolver : XmlUrlResolver {
            const string SCHEME_MRES = "mres";

            /// <summary>
            /// Loads the specified file from our internal resources if its there
            /// </summary>
            /// <param name="absoluteUri"></param>
            /// <param name="role"></param>
            /// <param name="objToReturn"></param>
            /// <returns></returns>
            public override object GetEntity(Uri absoluteUri, string role, Type objToReturn) {
                string filename = absoluteUri.Segments[absoluteUri.Segments.Length-1];

                if (absoluteUri.Scheme == SCHEME_MRES || 
                    (absoluteUri.Scheme == "file" 
                        && ! File.Exists(absoluteUri.AbsolutePath)
                        && _resFiles.Contains(filename))) {
                    Assembly thisAssm = Assembly.GetExecutingAssembly();
                    //string filename = absoluteUri.Segments[absoluteUri.Segments.Length-1];
                    return thisAssm.GetManifestResourceStream("xslt." + filename);
                } else {
                    // we don't know how to handle this URI scheme....
                    return base.GetEntity(absoluteUri, role, objToReturn);
                }
            }
        }
    }
}
