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

namespace NAnt.Contrib.Util
{ 

   /// <summary>
   /// Recorder interface user with the Record task
   /// </summary>
   public interface IRecorder
   {
      /// <summary>
      /// Name of this recorder (possibly a file name)
      /// </summary>
      string Name {
         get; 
      }

     /* /// <summary>
      /// Underlying LogListener instance
      /// </summary>
      //LogListener Listener {
      //   get;
      //} */

      /// <summary>Start Recording</summary>
      void Start();
      /// <summary>Stop Recording</summary>
      void Stop();
      /// <summary>Close the recorder</summary>
      void Close();

   } // interface IRecorder


   /// <summary>
   /// Keeps track of used recorders
   /// </summary>
   internal class RecorderCollection
   {
      private Hashtable _list;

      public RecorderCollection()
      {
         _list = new Hashtable();
      }

      public void AddRecorder(IRecorder recorder)
      {
         _list.Add(recorder.Name, recorder);
      }
      public IRecorder GetRecorder(string name)
      {
         if ( _list.ContainsKey(name) ) 
            return (IRecorder)_list[name];
         else
            return null;
      }
      public void RemoveRecorder(string name)
      {
         if ( _list.ContainsKey(name) ) 
            _list.Remove(name);
      }

   } // class RecorderCollection

   /// <summary>
   /// Implementation of LogListener that
   /// writes information to a file.
   /// </summary>
   internal class FileLogListener : IRecorder //LogListener, 
   {
      private StreamWriter _writer;
      private bool _stopped = false;
      private string _name;


      /// <summary>
      /// Create a new instance
      /// </summary>
      /// <param name="name">Name of Recorder (file to write to)</param>
      public FileLogListener(string name)
      {
         _name = name;
         _stopped = true;
         _writer = null;
      }

      #region IRecorder Implementation
      
      public string Name {
         get { return _name; }
      }
      //public LogListener Listener {
      //   get { return this; }
      //}
      public void Start()
      {
         _stopped = false;
         _writer = new StreamWriter(File.OpenWrite(_name));
      }
      public void Stop()
      {
         _stopped = true;
      }
      public void Close()
      {
         Stop();
         _writer.Close();
         _writer = null;
      }
      #endregion // IRecorder Implementation

      #region LogListener Implementation

      // commented to get building. Needs investigation to see what this was doing
      /*   public override void Write(string message)
      {
         if ( _writer == null )
            throw new BuildException("Tried to write to an invalid FileLogListener instance!");
         if ( !_stopped )
            _writer.Write(message);
      }

      public override void WriteLine(string message)
      {
         if ( _writer == null )
            throw new BuildException("Tried to write to an invalid FileLogListener instance!");
         if ( !_stopped )
            _writer.WriteLine(message);
      }

      public override void Flush() 
      {
         if ( _writer == null )
            throw new BuildException("Tried to flush an invalid FileLogListener instance!");
         _writer.Flush();
      }
*/
      #endregion // LogListener Implementation

   } // class FileLogListener

} // namespace NAnt.Contrib.Util
