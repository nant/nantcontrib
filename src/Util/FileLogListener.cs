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

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Util {
    #region IRecorder interface

    /// <summary>
    /// Recorder interface user with the Record task
    /// </summary>
    public interface IRecorder  {
        /// <summary>
        /// Gets the name of this recorder (possibly a file name).
        /// </summary>
        string Name {
            get;
        }

        /// <summary>
        /// Gets The underlying <see cref="IBuildLogger" /> instance.
        /// </summary>
        IBuildLogger Logger {
            get;
        }

        /// <summary>
        /// Defines whether the underlying writer is automatically flushes or 
        /// not.
        /// </summary>
        bool AutoFlush {
            get;
            set;
        }

        /// <summary>
        /// Starts recording.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops recording.
        /// </summary>
        void Stop();

        /// <summary>
        /// Closes the recorder.
        /// </summary>
        void Close();

        /// <summary>
        /// Flushes the recorder.
        /// </summary>
        void Flush();
    }

    #endregion IRecorder interface

    #region RecorderCollection class

    /// <summary>
    /// Keeps track of used recorders
    /// </summary>
    internal class RecorderCollection {
        private Hashtable _list;

        public RecorderCollection() {
            _list = new Hashtable();
        }

        public void Add(IRecorder recorder) {
            _list.Add(recorder.Name, recorder);
        }
        
        [System.Runtime.CompilerServices.IndexerName("Item")]
        public IRecorder this[string name] {
            get {
                if (_list.ContainsKey(name)) {
                    return (IRecorder) _list[name];
                } else {
                    return null;
                }
            }
        }

        public void Remove(string name) {
            if (_list.ContainsKey(name)) {
                _list.Remove(name);
            }
        }
    }

    #endregion RecorderCollection class

    [Serializable()]
    internal class FileLogListener : IBuildLogger, IRecorder {
        #region Public Instance Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLogListener" />
        /// class.
        /// </summary>
        public FileLogListener(string name) {
            _name = name;
            _stopped = true;
            _threshold = Level.Info;
            _outputWriter = null;
            _autoFlush = false;
        }

        #endregion Public Instance Constructors

        #region Implementation of IBuildLogger

        /// <summary>
        /// Gets or sets the highest level of message this logger should respond
        /// to.
        /// </summary>
        /// <value>
        /// The highest level of message this logger should respond to.
        /// </value>
        /// <remarks>
        /// Only messages with a message level higher than or equal to the given
        /// level should be written to the log.
        /// </remarks>
        public virtual Level Threshold {
            get { return _threshold; }
            set { _threshold = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to produce emacs (and other
        /// editor) friendly output.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if output is to be unadorned so that emacs 
        /// and other editors can parse files names, etc. The default is
        /// <see langword="false" />.
        /// </value>
        public virtual bool EmacsMode {
            get { return _emacsMode; }
            set { _emacsMode = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="TextWriter" /> to which the logger is
        /// to send its output.
        /// </summary>
        public virtual TextWriter OutputWriter {
            get { return _outputWriter; }
            set { _outputWriter = value; }
        }

        /// <summary>
        /// Flushes buffered build events or messages to the underlying storage.
        /// </summary>
        public virtual void Flush() {
            if (OutputWriter == null) {
                return;
            }
            OutputWriter.Flush();
        }

        #endregion Implementation of IBuildLogger

        #region Implementation of IBuildListener

        /// <summary>
        /// Signals that a build has started.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        /// <remarks>
        /// This event is fired before any targets have started.
        /// </remarks>
        public virtual void BuildStarted(object sender, BuildEventArgs e) {
        }

        /// <summary>
        /// Signals that the last target has finished.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        /// <remarks>
        /// This event will still be fired if an error occurred during the build.
        /// </remarks>
        public virtual void BuildFinished(object sender, BuildEventArgs e) {
            Exception error = e.Exception;
            int indentationLevel = 0;

            if (e.Project != null) {
                indentationLevel = e.Project.IndentationLevel * e.Project.IndentationSize;
            }

            if (error == null) {
                OutputMessage(Level.Info, "", indentationLevel);
                OutputMessage(Level.Info, "BUILD SUCCEEDED", indentationLevel);
                OutputMessage(Level.Info, "", indentationLevel);
            } else {
                OutputMessage(Level.Error, "", indentationLevel);
                OutputMessage(Level.Error, "BUILD FAILED", indentationLevel);
                OutputMessage(Level.Error, "", indentationLevel);

                if (error is BuildException) {
                    if (Threshold <= Level.Verbose) {
                        OutputMessage(Level.Error, error.ToString(), indentationLevel);
                    } else {
                        if (error.Message != null) {
                            OutputMessage(Level.Error, error.Message, indentationLevel);
                        }
                        if (error.InnerException != null && error.InnerException.Message != null) {
                            OutputMessage(Level.Error, error.InnerException.Message, indentationLevel);
                        }
                    }
                } else {
                    OutputMessage(Level.Error, "INTERNAL ERROR", indentationLevel);
                    OutputMessage(Level.Error, "", indentationLevel);
                    OutputMessage(Level.Error, error.ToString(), indentationLevel);
                    OutputMessage(Level.Error, "", indentationLevel);
                    OutputMessage(Level.Error, "Please send bug report to nant-developers@lists.sourceforge.net.", indentationLevel);
                }

                OutputMessage(Level.Info, "", indentationLevel);
            }

            // make sure all messages are written to the underlying storage
            Flush();
        }

        /// <summary>
        /// Signals that a target has started.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        public virtual void TargetStarted(object sender, BuildEventArgs e) {
            int indentationLevel = 0;

            if (e.Project != null) {
                indentationLevel = e.Project.IndentationLevel * e.Project.IndentationSize;
            }

            if (e.Target != null) {
                OutputMessage(Level.Info, string.Empty, indentationLevel);
                OutputMessage(Level.Info, string.Format(CultureInfo.InvariantCulture, 
                    "{0}:", e.Target.Name), indentationLevel);
                OutputMessage(Level.Info, string.Empty, indentationLevel);
            }
        }

        /// <summary>
        /// Signals that a task has finished.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        /// <remarks>
        /// This event will still be fired if an error occurred during the build.
        /// </remarks>
        public virtual void TargetFinished(object sender, BuildEventArgs e) {
        }

        /// <summary>
        /// Signals that a task has started.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        public virtual void TaskStarted(object sender, BuildEventArgs e) {
        }

        /// <summary>
        /// Signals that a task has finished.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        /// <remarks>
        /// This event will still be fired if an error occurred during the build.
        /// </remarks>
        public virtual void TaskFinished(object sender, BuildEventArgs e) {
        }

        /// <summary>
        /// Signals that a message has been logged.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        /// <remarks>
        /// Only messages with a priority higher or equal to the threshold of
        /// the logger will actually be output in the build log.
        /// </remarks>
        public virtual void MessageLogged(object sender, BuildEventArgs e) {
            // output the message
            OutputMessage(e);
        }

        #endregion Implementation of IBuildListener

        #region Implementation of IRecorder

        public string Name {
            get { return _name; }
        }

        public IBuildLogger Logger {
            get { return this; }
        }

        public bool AutoFlush {
            get { return _autoFlush; }
            set {
                _autoFlush = value;
                if (OutputWriter == null) {
                    return;
                }

                ((StreamWriter) OutputWriter).AutoFlush = value;
            }
        }

        public void Start() {
            _stopped = false;
            if (OutputWriter == null) {
                OutputWriter = new StreamWriter(new FileStream(Name, 
                    FileMode.Create, FileAccess.Write, FileShare.Read));
            }
            ((StreamWriter) OutputWriter).AutoFlush = AutoFlush;
        }

        public void Stop() {
            _stopped = true;
        }

        public void Close() {
            Stop();
            OutputWriter.Close();
            OutputWriter = null;
        }

        #endregion Implementation of IRecorder

        #region Protected Instance Methods

        /// <summary>
        /// Empty implementation which allows derived classes to receive the
        /// output that is generated in this logger.
        /// </summary>
        /// <param name="message">The message being logged.</param>
        protected virtual void Log(string message) {
        }

        #endregion Protected Instance Methods

        #region Private Instance Methods

        /// <summary>
        /// Outputs an indented message to the build log if its priority is 
        /// greather than or equal to the <see cref="Threshold" /> of the 
        /// logger.
        /// </summary>
        /// <param name="messageLevel">The priority of the message to output.</param>
        /// <param name="message">The message to output.</param>
        /// <param name="indentationLength">The number of characters that the message should be indented.</param>
        private void OutputMessage(Level messageLevel, string message, int indentationLength) {
            OutputMessage(CreateBuildEvent(messageLevel, message), indentationLength);
        }

        /// <summary>
        /// Outputs an indented message to the build log if its priority is 
        /// greather than or equal to the <see cref="Threshold" /> of the 
        /// logger.
        /// </summary>
        /// <param name="e">The event to output.</param>
        private void OutputMessage(BuildEventArgs e) {
            int indentationLength = 0;
            
            if (e.Project != null) {
                indentationLength = e.Project.IndentationLevel * e.Project.IndentationSize;
            }

            OutputMessage(e, indentationLength);
        }

        /// <summary>
        /// Outputs an indented message to the build log if its priority is 
        /// greather than or equal to the <see cref="Threshold" /> of the 
        /// logger.
        /// </summary>
        /// <param name="e">The event to output.</param>
        /// <param name="indentationLength">TODO</param>
        private void OutputMessage(BuildEventArgs e, int indentationLength) {
            if (!_stopped && (e.MessageLevel >= Threshold)) {
                if (OutputWriter == null) {
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                        "Tried to write to an invalid FileLogListener instance '{0}'!",
                        Name), Location.UnknownLocation);
                }

                string txt = e.Message;

                // beautify the message a bit
                txt = txt.Replace("\t", " "); // replace tabs with spaces
                txt = txt.Replace("\r", ""); // get rid of carriage returns

                // split the message by lines - the separator is "\n" since we've eliminated
                // \r characters
                string[] lines = txt.Split('\n');
                string label = String.Empty;

                if (e.Task != null && !EmacsMode) {
                    label = "[" + e.Task.Name + "] ";
                    label = label.PadLeft(e.Project.IndentationSize);
                }

                if (indentationLength > 0) {
                    label = new String(' ', indentationLength) + label;
                }

                foreach (string line in lines) {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(label);
                    sb.Append(line);

                    string indentedMessage = sb.ToString();

                    OutputWriter.WriteLine(indentedMessage);

                    Log(indentedMessage);
                }
            }
        }

        #endregion Private Instance Methods

        #region Private Static Methods

        private static BuildEventArgs CreateBuildEvent(Level messageLevel, string message) {
            BuildEventArgs buildEvent = new BuildEventArgs();
            buildEvent.MessageLevel = messageLevel;
            buildEvent.Message = message;
            return buildEvent;
        }

        #endregion Private Static Methods

        #region Private Instance Fields

        private Level _threshold = Level.Info;
        private bool _emacsMode;
        private TextWriter _outputWriter;
        private string _name = string.Empty;
        private bool _stopped = true;
        private bool _autoFlush;

        #endregion Private Instance Fields
    }
}
