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

using NAnt.Contrib.Util;

namespace NAnt.Contrib.Tasks 
{ 

   /// <summary>
   /// A task that records the build's output
   /// to a file. Loosely based on Ant's Record task.
   /// </summary>
   /// <remarks>
   /// This task allows you to record the build's output, 
   /// or parts of it to a file. You can start and stop
   /// recording at any place in the build process.
   /// </remarks>
   /// <example>
   ///   <code><![CDATA[
   ///      <record name="${outputdir}\Buildlog.txt" action="Start"/>
   ///      <record name="${outputdir}\Buildlog.txt" action="Close"/>
   ///   ]]></code>
   /// </example>
   [TaskName("record")]
   public class RecordTask : Task
   {
      //
      // valid actions
      //
      enum Actions {
         Start = 0,
         Stop = 1,
         Close = 2,
      }
      private string _logname;
      private Actions _action;
      static RecorderCollection _recorders = new RecorderCollection();

      /// <summary>
      /// Name of Destination file.
      /// </summary>
      [TaskAttribute("name", Required=true)]
      public string LogName { 
         get { return _logname; } 
         set { _logname = value; } 
      }
        
/*      /// <summary>
      /// Whether to append to the destination file (true),
      /// or replace it (false). Default is false.
      /// </summary>
      [TaskAttribute("append")]
      public string Append { 
         get { return _append.ToString(); } 
         set { _append = bool.Parse(value); } 
      }
*/
      /// <summary>
      /// Action to apply to this log instance
      /// </summary>
      /// <remarks>It can take on of the following values: start, stop, close</remarks>
      [TaskAttribute("action", Required=true)]
      public string Action {
         get { return _action.ToString(); }
         set { _action = (Actions)Enum.Parse(typeof(Actions), value); }
      }


      ///<summary>
      ///Initializes task and ensures the supplied attributes are valid.
      ///</summary>
      ///<param name="taskNode">Xml node used to define this task instance.</param>
      protected override void InitializeTask(System.Xml.XmlNode taskNode) 
      {
         if (LogName == null) {
            throw new BuildException("Record attribute \"name\" is required.", Location);
         }
         if (Action == null) {
            throw new BuildException("Record attribute \"action\" is required.", Location);
         }
      }

      /// <summary>
      /// This is where the work is done
      /// </summary>
      protected override void ExecuteTask() 
      {
         IRecorder recorder = _recorders.GetRecorder(LogName);

         switch ( _action )
         {
         case Actions.Start:
            if ( recorder == null ) {
               recorder = new FileLogListener(LogName);
               _recorders.AddRecorder(recorder);
               //Log.Listeners.Add(recorder);
            }
            recorder.Start();
            break;
         case Actions.Stop:
            if ( recorder == null )
               throw new BuildException("Tried to stop non-existent recorder");
            recorder.Stop();
            break;
         case Actions.Close:
            if ( recorder == null )
               throw new BuildException("Tried to close non-existent recorder");
            recorder.Close();
            //Log.Listeners.Remove(recorder);
            _recorders.RemoveRecorder(recorder.Name);
            break;
         }

      }


   } // class RecordTask

} // namespace NAnt.Contrib.Tasks
