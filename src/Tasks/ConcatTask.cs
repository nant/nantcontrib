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
using NAnt.Core.Attributes;
using NAnt.Core;
using NAnt.Core.Types;

namespace NAnt.Contrib.Tasks 
{ 

   /// <summary>
   /// A task that concatenates a set of files.
   /// Loosely based on Ant's Concat task.
   /// </summary>
   /// <remarks>
   /// This task takes a set of input files in a fileset
   /// and concatenates them into a single file. You can 
   /// either replace the output file, or append to it 
   /// by using the append attribute.
   /// 
   /// The order the files are concatenated in is not
   /// especified.
   /// </remarks>
   /// <example>
   ///   <code><![CDATA[
   ///   <concat destfile="${outputdir}\Full.txt" append="true">
   ///      <fileset>
   ///         <includes name="${outputdir}\Test-*.txt" />
   ///      </fileset>
   ///   </concat>
   ///   
   ///   ]]></code>
   /// </example>
   [TaskName("concat")]
   public class ConcatTask : Task
   {
      private string _destination;
      private bool _append = false;
      private FileSet _fileset = new FileSet();

      /// <summary>
      /// Name of Destination file.
      /// </summary>
      [TaskAttribute("destfile", Required=true)]
      public string Destination { 
         get { return _destination; } 
         set { _destination = value; } 
      }
        
      /// <summary>
      /// Whether to append to the destination file (true),
      /// or replace it (false). Default is false.
      /// </summary>
      [TaskAttribute("append"), BooleanValidator()]
      public bool Append { 
         get { return _append; } 
         set { _append = value; } 
      }

      /// <summary>
      /// Set of files to use as input
      /// </summary>
      [BuildElement("fileset")]
      public FileSet FileSet {
        get { return _fileset; }
        set { _fileset = value; }
      }


      ///<summary>
      ///Initializes task and ensures the supplied attributes are valid.
      ///</summary>
      ///<param name="taskNode">Xml node used to define this task instance.</param>
      protected override void InitializeTask(System.Xml.XmlNode taskNode) 
      {
         if (Destination == null) {
            throw new BuildException("Concat attribute \"destfile\" is required.", Location);
         }
         if (FileSet.FileNames.Count == 0) {
            throw new BuildException("Concat fileset cannot be empty!", Location);
         }
      }

      /// <summary>
      /// This is where the work is done
      /// </summary>
      protected override void ExecuteTask() 
      {

         FileStream output = OpenDestinationFile();
         
         try {
            AppendFiles(output);
         } finally {
            output.Close();
         }
      }


      /// <summary>
      /// Opens the destination file according
      /// to the specified flags
      /// </summary>
      /// <returns></returns>
      private FileStream OpenDestinationFile()
      {
         FileMode mode;
         if ( _append ) {
            mode = FileMode.Append | FileMode.OpenOrCreate; 
         } else {
            mode = FileMode.Create;
         }
         try {
            return File.Open(Destination, mode);
         } catch ( IOException e ) {
            string msg = string.Format("File {0} could not be opened", Destination);
            throw new BuildException(msg, e);
         }
      }

      /// <summary>
      /// Appends all specified files
      /// </summary>
      /// <param name="output">File to write to</param>
      private void AppendFiles(FileStream output)
      {
         const int size = 64*1024;
         byte[] buffer = new byte[size];

         foreach ( string file in FileSet.FileNames ) 
         {
            int bytesRead = 0;
            FileStream input = null;
            try {
               input = File.OpenRead(file);
            } catch ( IOException e ) {
               Log(Level.Info, "Concat: File {0} could not be read: {1}", file, e.Message);
               continue;
            }
               
            try {
               while ( (bytesRead = input.Read(buffer, 0, size)) != 0 ) {
                  output.Write(buffer, 0, bytesRead);
               }
            } catch ( IOException e ) {
               throw new BuildException("Concat: Could not read or write from file", e);
            } finally {
               input.Close();
            }
         }
      }

   } // class ConcatTask

} // namespace NAnt.Contrib.Tasks
