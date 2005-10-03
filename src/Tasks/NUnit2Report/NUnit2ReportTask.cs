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
//Gilles Bayon (gilles.bayon@laposte.net)
//Ian Maclean (imaclean@gmail.com)

using System;
using System.Collections;
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

namespace NAntContrib.NUnit2ReportTasks {

    /// <summary>
    /// A task that generates a summary HTML
    /// from a set of NUnit xml report files.
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
    ///   <nunit2report
    ///         out="${outputdir}\TestSummary.html"
    ///         >
    ///      <fileset>
    ///         <includes name="${outputdir}\results.xml" />
    ///      </fileset>
    ///   </nunit2report>
    ///
    ///   ]]></code>
    /// </example>
    [TaskName("nunit2report")]
    public class NUnit2ReportTask : Task  {
        
        #region static declarations
        private  static readonly ArrayList resFiles = new ArrayList(new string[3]{"toolkit.xsl", "Traductions.xml", "NUnit-Frame.xsl"});  
        #endregion
        
        #region Private Static Fields

        private const string XSL_DEF_FILE_NOFRAME = "NUnit-NoFrame.xsl";
        private const string XSL_I18N_FILE = "i18n.xsl"; 
        
        #endregion Private Static Fields

        #region Private Instance Fields

        private string _outFilename="index.htm";
        private string _todir="";
     
        private FileSet _fileset = new FileSet();
        private XmlDocument _fileSetSummary;
     
        private FileSet _summaries = new FileSet();
        private string _tempXmlFileSummarie = "";
     
        private string _xslFile = null;
        private string _i18nXsl = null;
        private string _openDescription ="no";
        private string _language = "";
        private string _format = "noframes";
        private XsltArgumentList _xsltArgs;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The format of the generated report.
        /// Must be "noframes" or "frames".
        /// Default to "noframes".
        /// </summary>
        [TaskAttribute("format", Required=false)]
        public string Format {
            get { return _format; }
            set { _format = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The output language.
        /// </summary>
        [TaskAttribute("lang", Required=false)]
        public string Language {
            get { return _language; }
            set { _language = value; }
        }
    
        /// <summary>
        /// Open all description method. Default to "false".
        /// </summary>
        [TaskAttribute("opendesc", Required=false)]
        public string OpenDescription {
            get { return _openDescription; }
            set { _openDescription = StringUtils.ConvertEmptyToNull(value); }
        }
    
        /// <summary>
        /// The directory where the files resulting from the transformation should be written to.
        /// </summary>
        [TaskAttribute("todir", Required=false)]
        public string Todir {
            get { return _todir; }
            set { _todir = value; }
        }
    
        /// <summary>
        /// Index of the Output HTML file(s).
        /// Default to "index.htm".
        /// </summary>
        [TaskAttribute("out", Required=false)]
        public string OutFilename {
            get { return _outFilename; }
            set { _outFilename = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Set of XML files to use as input
        /// </summary>
        [BuildElement("fileset")]
        public FileSet XmlFileSet {
            get { return _fileset; }
        }
    
        /// <summary>
        /// Set of Summary XML files to use as input
        /// </summary>
        [BuildElement("summaries")]
        public FileSet XmlSummaries {
            get { return _summaries; }
        }
        /// <summary>
        /// XSLT file used to generate the report.
        /// </summary>
        [TaskAttribute("xslfile", Required=false)]
        public string XslFile { 
            get { return _xslFile; } 
            set { _xslFile = StringUtils.ConvertEmptyToNull(value); } 
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        ///<summary>
        ///Initializes task and ensures the supplied attributes are valid.
        ///</summary>
        ///<param name="taskNode">Xml node used to define this task instance.</param>
        protected override void InitializeTask(XmlNode taskNode) {

            if (Format=="noframes") {
                // Use the default iff the user didn't set an xslt file.
                if ( _xslFile == null )    {
                    _xslFile = XSL_DEF_FILE_NOFRAME;
                }
            }
            _i18nXsl = XSL_I18N_FILE;

            if (XmlFileSet.FileNames.Count == 0) {
                throw new BuildException("NUnitReport fileset cannot be empty!", Location);
            }

            foreach ( string file in XmlSummaries.FileNames ) {
                _tempXmlFileSummarie = file;
            }

            // Get the Nant, OS parameters
            _xsltArgs = GetPropertyList();

            //Create directory if ...
            if (Todir!="") {
                Directory.CreateDirectory(Todir);
            }

        }

        /// <summary>
        /// This is where the work is done
        /// </summary>
        protected override void ExecuteTask() {
            _fileSetSummary = CreateSummaryXmlDoc();
    
            foreach ( string file in XmlFileSet.FileNames ) {
                XmlDocument source = new XmlDocument();
                source.Load(file);
                XmlNode node = _fileSetSummary.ImportNode(source.DocumentElement, true);
                _fileSetSummary.DocumentElement.AppendChild(node);
            }

            // prepare properties and transforms
            try {
                if ( Format=="noframes") {
         
                    XslTransform xslTransform = new XslTransform();
                   
                    XmlResolver resolver = new LocalResXmlResolver();
                    xslTransform.Load(LoadStyleSheet(_xslFile), resolver);            
         
                    // xmlReader hold the first transformation
                    XmlReader xmlReader = xslTransform.Transform(_fileSetSummary, _xsltArgs);
         
                    //  i18n 
                    XsltArgumentList xsltI18nArgs = new XsltArgumentList();
                    xsltI18nArgs.AddParam("lang", "",Language);
         
                    XslTransform xslt = new XslTransform();
                    //Load the i18n stylesheet.
                    xslt.Load(LoadStyleSheet(_i18nXsl),resolver);
         
                    XPathDocument xmlDoc;
                    xmlDoc = new XPathDocument(xmlReader);
         
                    XmlTextWriter writerFinal = new XmlTextWriter(Path.Combine(Todir, OutFilename), 
                        Encoding.GetEncoding("ISO-8859-1"));
                    
                    // Apply the second transform to xmlReader to final ouput
                    xslt.Transform(xmlDoc, xsltI18nArgs, writerFinal);
         
                    xmlReader.Close();
                    writerFinal.Close();
                }
                else {
                    StringReader stream;
                    XmlTextReader reader = null;
    
                    try {
                        // create the index.html
                        stream = new StringReader("<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='1.0' >" +
                            "<xsl:output method='html' indent='yes' encoding='ISO-8859-1'/>" +
                            "<xsl:include href=\"NUnit-Frame.xsl\"/>" +
                            "<xsl:template match=\"test-results\">" +
                            "   <xsl:call-template name=\"index.html\"/>" +
                            " </xsl:template>" +
                            " </xsl:stylesheet>");
                        Write (stream,Path.Combine(Todir,OutFilename));
    
                        // create the stylesheet.css
                        stream = new StringReader("<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='1.0' >" +
                            "<xsl:output method='html' indent='yes' encoding='ISO-8859-1'/>" +
                            "<xsl:include href=\"NUnit-Frame.xsl\"/>" +
                            "<xsl:template match=\"test-results\">" +
                            "   <xsl:call-template name=\"stylesheet.css\"/>" +
                            " </xsl:template>" +
                            " </xsl:stylesheet>");
                        Write (stream,Path.Combine(Todir,"stylesheet.css"));
    
                        // create the overview-summary.html at the root
                        stream = new StringReader("<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='1.0' >" +
                            "<xsl:output method='html' indent='yes' encoding='ISO-8859-1'/>" +
                            "<xsl:include href=\"NUnit-Frame.xsl\"/>" +
                            "<xsl:template match=\"test-results\">" +
                            "    <xsl:call-template name=\"overview.packages\"/>" +
                            " </xsl:template>" +
                            " </xsl:stylesheet>");
                        Write (stream,Path.Combine(Todir,"overview-summary.html"));
    
    
                        // create the allclasses-frame.html at the root
                        stream = new StringReader("<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='1.0' >" +
                            "<xsl:output method='html' indent='yes' encoding='ISO-8859-1'/>" +
                            "<xsl:include href=\"NUnit-Frame.xsl\"/>" +
                            "<xsl:template match=\"test-results\">" +
                            "    <xsl:call-template name=\"all.classes\"/>" +
                            " </xsl:template>" +
                            " </xsl:stylesheet>");
                        Write (stream,Path.Combine(Todir,"allclasses-frame.html"));
    
                        // create the overview-frame.html at the root
                        stream = new StringReader("<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='1.0' >" +
                            "<xsl:output method='html' indent='yes' encoding='ISO-8859-1'/>" +
                            "<xsl:include href=\"NUnit-Frame.xsl\"/>" +
                            "<xsl:template match=\"test-results\">" +
                            "    <xsl:call-template name=\"all.packages\"/>" +
                            " </xsl:template>" +
                            " </xsl:stylesheet>");
                        Write (stream,Path.Combine(Todir,"overview-frame.html"));
    
                        // Create directory
                        string path ="";
    
                        XPathNavigator xpathNavigator = _fileSetSummary.CreateNavigator(); //doc.CreateNavigator();
    
                        // Get All the test suite containing test-case.
                        XPathExpression expr = xpathNavigator.Compile("//test-suite[(child::results/test-case)]");
    
                        XPathNodeIterator iterator = xpathNavigator.Select(expr);
                        string directory="";
                        string testSuiteName = "";
    
                        while (iterator.MoveNext()) {
                            XPathNavigator xpathNavigator2 = iterator.Current;
                            testSuiteName = iterator.Current.GetAttribute("name","");
                               
                            Log(Level.Debug,"Test case : "+ testSuiteName);

                            // Get get the path for the current test-suite.
                            XPathNodeIterator iterator2 = xpathNavigator2.SelectAncestors("", "", true);
                            path = "";
                            string parent = "";
                            int parentIndex = -1;
    
                            while (iterator2.MoveNext()) {
                                directory = iterator2.Current.GetAttribute("name","");
                                if (directory!="" && directory.IndexOf(".dll")<0) {
                                    path = directory+"/"+path;
                                }
                                if (parentIndex==1) {
                                    parent = directory;
                                }
                                parentIndex++;
                            }
                            Directory.CreateDirectory(Path.Combine(Todir,path));// path = xx/yy/zz                                                        
                            
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
                            Write (stream, Path.Combine( Path.Combine(Todir,path) ,testSuiteName+".html") );
    
                            Log(Level.Debug,"dir={0} Generating {1}.html", Todir+path, testSuiteName );
                            
                        }
                        Log(Level.Info,"Processing ...");

    
                    }
    
                    catch (Exception e) {
                        Console.WriteLine ("Exception: {0}", e.ToString());
                    }
    
                    finally {
                        Log(Level.Debug,"Processing of stream complete.");

                        // Finished with XmlTextReader
                        if (reader != null) {
                            reader.Close();
                        }
                    }
                }
            }
            catch (Exception e) {
                throw new BuildException(e.Message, Location);
            }
        }

        #endregion Override implementation of Task

        #region Private Instance Methods

        /// <summary>
        /// Load a stylesheet from the assemblies resource stream.
        /// </summary>
        ///<param name="xslFileName">File name of the file to extract.</param>
        protected XmlDocument LoadStyleSheet(string xslFileName) {
            Stream xsltStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                string.Format("xslt.{0}", xslFileName) );
            if (xsltStream == null) {
                throw new Exception(string.Format("Missing '{0}' Resource Stream", xslFileName));
            }

            XmlTextReader reader = new XmlTextReader(xsltStream, XmlNodeType.Document,null);

            //first load in an XmlDocument so we can set the appropriate nant-namespace
            XmlDocument xsltDoc = new XmlDocument();
            xsltDoc.Load(reader);
            return xsltDoc;
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
            foreach ( DictionaryEntry entry in Project.Properties ) {
    
                if ((string)entry.Value!=null) {
                
                    try {
                        args.AddParam((string)entry.Key, "", (string)entry.Value);
                    }
                    catch(ArgumentException aex) {
                        Console.WriteLine("Invalid Xslt parameter {0}", aex);
                    }
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
            XmlReader xmlReader = xslTransform.Transform(_fileSetSummary, _xsltArgs);//(xmlDoc, _xsltArgs);
    
            //  i18n
            XsltArgumentList xsltI18nArgs = new XsltArgumentList();
            xsltI18nArgs.AddParam("lang", "", Language);
    
            XslTransform xslt = new XslTransform();
    
            //Load the stylesheet.
            xslt.Load(LoadStyleSheet(_i18nXsl), resolver);
    
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
                        && resFiles.Contains(filename) ))  {
                    Assembly thisAssm = Assembly.GetExecutingAssembly();
                    //string filename = absoluteUri.Segments[absoluteUri.Segments.Length-1];
                    return thisAssm.GetManifestResourceStream("xslt." + filename);
                }
                else {
                    // we don't know how to handle this URI scheme....
                    return base.GetEntity(absoluteUri, role, objToReturn);
                }
            }
        }
    } 
}