//
// NAntContrib
// Copyright (C) 2001-2002 Gerry Shaw
//
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

// Ian MacLean ( ian@maclean.ms)
// Gerry Shaw (gerry_shaw@yahoo.com)
// Based on the CollectionGen collection generator by Chris Sells ( http://www.sellsbrothers.com/tools/#collectionGen )

using System;
using System.Collections.Specialized;
using System.IO;

using NAnt.Core;
using NAnt.Core.Types;
using NAnt.Core.Attributes;
using CollectionGenerator;

namespace NAnt.Contrib.Tasks {


    /// <summary>Generates collection classes based on a given XML specification file. Code generation is in the specified language.</summary>
    ///<remarks>
    ///   <para>See the <a href="http://www.sellsbrothers.com/tools/">CollectionGen tool page</a> for more information.</para>
    ///</remarks>
    /// <example>   
    ///   <code>
    /// <![CDATA[
    /// <typedcollection language="CSharp">
    ///    <fileset>
    ///        <include name="collections.xml" />
    ///    </fileset>
    ///</typedcollection>
    /// ]]>
    ///   </code>
    /// </example>
    [TaskName("typedcollection")]
    public class TypedCollectionTask : Task {
        
        string _language = null;
        string _fileName = null;
        FileSet _fileset = new FileSet();

        /// <summary>The language to generate collection classes for.  Valid values are "CSharp" or "VB".</summary>
        [TaskAttribute("language", Required=true )]
        public string Language {
            get { return _language; }
            set { _language = value; }
        }

        /// <summary>The name of the template file for collection generation. This is provided as an alternate to using the task's fileset.</summary>
        [TaskAttribute("file")]
        public string FileName {
            get { return _fileName; }
            set { _fileName = value; }
        }

        /// <summary>
        /// All files in this fileset will be run thru the collection generator.
        /// </summary>
        [BuildElement("fileset")]
        public FileSet TypedCollFileSet {
            get { return _fileset; }
            set { _fileset = value; }
        }

        protected override void ExecuteTask() {
            // add the shortcut filename to the file set
            if (FileName != null) {
                try {
                    string path = Project.GetFullPath(FileName);
                    TypedCollFileSet.Includes.Add(path);
                } catch (Exception e) {
                    string msg = String.Format("Could not find file '{0}'", FileName);
                    throw new BuildException(msg, Location, e);
                }
            }

            // gather the information needed to perform the operation
            StringCollection fileNames = TypedCollFileSet.FileNames;

            // display build log message
            Log(Level.Info, "Building typesafe collection classes for {0} files"
                + " in the {1} language", fileNames.Count, Language);

            // perform operation
            foreach (string path in fileNames) {
                GenerateCollectionClasses(path, Language);
            }
        }
        /// <summary>
        /// The actual generation work is done here.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="language"></param>
        private void GenerateCollectionClasses(string path, string language){
            string langExt = "";
            try {
                if (File.Exists(path)) {
                    Log(Level.Info, path);
                    CollectionGenerator.CollectionGenerator generator = null;
                    // load the file
                    switch ( language ) {
                        case "CSharp" :
                            langExt = ".cs";
                            generator = new CSharpCollectionGenerator();
                            break;
                        case "VB" :
                            generator = new VBCollectionGenerator();
                            langExt = ".vb";
                            break;
                    }
                    StreamReader reader = new StreamReader( path );
                    string collectionXML = reader.ReadToEnd();
                    reader.Close();

                    string generatedCode = generator.GenerateCodeFromXml( collectionXML );
                    string outputFile = Path.GetDirectoryName(path) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension( path ) + langExt;
                    // create new .lang file file 
                    StreamWriter writer = new StreamWriter( outputFile );
                                     
                    writer.Write( generatedCode );                                    
                    writer.Close();
                } else {
                    throw new FileNotFoundException();
                }
            } 
            catch( Exception e ){
                string msg = String.Format("Failed to generate collection classes for file : '{0}'", path);
                throw new BuildException(msg, Location, e);
            }            
        }
    }
}
