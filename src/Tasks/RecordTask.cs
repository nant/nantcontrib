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
using NAnt.Core.Util;

using NAnt.Contrib.Util; 

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// A task that records the build's output to a file. Loosely based on Ant's 
    /// <a href="http://ant.apache.org/manual/CoreTasks/recorder.html">Record</a>
    /// task.
    /// </summary>
    /// <remarks>
    /// This task allows you to record the build's output, or parts of it to a 
    /// file. You can start and stop recording at any place in the build process.
    /// </remarks>
    /// <example>
    ///   <code>
    ///     <![CDATA[
    /// <record name="${outputdir}\Buildlog.txt" level="Info" action="Start"/>
    /// <record name="${outputdir}\Buildlog.txt" action="Close"/>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("record")]
    public class RecordTask : Task {
        //
        // valid actions
        //
        public enum ActionType {
            Start = 0,
            Stop = 1,
            Close = 2,
            Flush = 3
        }

        #region Private Instance Fields

        private FileInfo _logname;
        private ActionType _actionType;
        private bool _autoFlush = false;
        private Level _thresholdLevel = Level.Info;

        #endregion Private Instance Fields

        #region Private Static Fields

        private static RecorderCollection _recorders = new RecorderCollection();

        #endregion Private Static Fields

        #region Public Instance Properties

        /// <summary>
        /// Name of destination file.
        /// </summary>
        [TaskAttribute("name", Required=true)]
        public FileInfo LogName {
            get { return _logname; }
            set { _logname = value; }
        }    

        /// <summary>
        /// Action to apply to this log instance - either <see cref="ActionType.Start" />,
        /// <see cref="ActionType.Stop" />, <see cref="ActionType.Close" /> or
        /// <see cref="ActionType.Flush" />.
        /// </summary>
        [TaskAttribute("action", Required=true)]
        public ActionType Action {
            get { return _actionType; }
            set {
                if (!Enum.IsDefined(typeof(ActionType), value)) {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, 
                        "An invalid action {0} was specified.", value)); 
                } else {
                    this._actionType = value;
                }
            } 
        }

        /// <summary>
        /// Determines whether the recorder will flush it's buffer after every 
        /// write to it. The default is <see langword="false" />.
        /// </summary>
        /// <remarks>
        /// Effective only with the <see cref="ActionType.Start" /> action.
        /// </remarks>
        [TaskAttribute("autoflush", Required=false)]
        [BooleanValidator()]
        public bool AutoFlush {
            get { return _autoFlush; }
            set { _autoFlush = value; }
        }

        /// <summary>
        /// Determine the level of logging - either <see cref="Level.Debug" />, 
        /// <see cref="Level.Verbose" />, <see cref="Level.Info" />, 
        /// <see cref="Level.Warning" /> or <see cref="Level.Error" />. 
        /// The default is <see cref="Level.Info" />.
        /// </summary>
        /// <remarks>
        /// Effective only with the <see cref="ActionType.Start" /> action.
        /// </remarks>
        [TaskAttribute("level", Required=false)]
        public Level ThresholdLevel {
            get { return this._thresholdLevel; }
            set {
                if (!Enum.IsDefined(typeof(Level), value)) {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, 
                        "An invalid level {0} was specified.", value)); 
                } else {
                    this._thresholdLevel = value;
                }
            }
        }

        #endregion Public Instance Properties

        #region Private Static Properties

        private static RecorderCollection Recorders {
            get { return _recorders; }
        }

        #endregion Private Static Properties

        #region Override implementation of Task

        /// <summary>
        /// This is where the work is done.
        /// </summary>
        protected override void ExecuteTask() {
            IRecorder recorder = _recorders[LogName.FullName];

            switch (Action) {
                case ActionType.Start:
                    if (recorder == null) {
                        recorder = new FileLogListener(LogName.FullName);
                        Recorders.Add(recorder);
                    }
                    recorder.AutoFlush = AutoFlush;
                    recorder.Logger.Threshold = ThresholdLevel;
                    recorder.Start();
                    AttachRecorder(recorder);
                    break;
                case ActionType.Stop:
                    if (recorder == null) {
                        throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                            "Tried to stop non-existent recorder '{0}'", LogName.FullName), 
                            Location);
                    }
                    recorder.Stop();
                    break;
                case ActionType.Close:
                    if (recorder == null) {
                        throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                            "Tried to close non-existent recorder '{0}'", LogName.FullName),
                            Location);
                    }
                    DetachRecorder(recorder);
                    recorder.Close();
                    Recorders.Remove(recorder.Name);
                    break;
                case ActionType.Flush:
                    if (recorder == null) {
                        throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                            "Tried to flush non-existent recorder '{0}'", LogName.FullName),
                            Location);
                    }
                    recorder.Flush();
                    break;
            }
        }

        #endregion Override implementation of Task

        #region Private Instance Methods

        private void DetachRecorder(IRecorder recorder) {
            IBuildLogger logger = recorder.Logger;

            // unhook up to build events
            Project.BuildStarted -= new BuildEventHandler(logger.BuildStarted);
            Project.BuildFinished -= new BuildEventHandler(logger.BuildFinished);
            Project.TargetStarted -= new BuildEventHandler(logger.TargetStarted);
            Project.TargetFinished -= new BuildEventHandler(logger.TargetFinished);
            Project.TaskStarted -= new BuildEventHandler(logger.TaskStarted);
            Project.TaskFinished -= new BuildEventHandler(logger.TaskFinished);
            Project.MessageLogged -= new BuildEventHandler(logger.MessageLogged);

            if (Project.BuildListeners.Contains(logger)) {
                Project.BuildListeners.Remove(logger);
            }
        }

        private void AttachRecorder(IRecorder recorder) {
            DetachRecorder(recorder);

            IBuildLogger logger = recorder.Logger;

            // hook up to build events
            Project.BuildStarted += new BuildEventHandler(logger.BuildStarted);
            Project.BuildFinished += new BuildEventHandler(logger.BuildFinished);
            Project.TargetStarted += new BuildEventHandler(logger.TargetStarted);
            Project.TargetFinished += new BuildEventHandler(logger.TargetFinished);
            Project.TaskStarted += new BuildEventHandler(logger.TaskStarted);
            Project.TaskFinished += new BuildEventHandler(logger.TaskFinished);
            Project.MessageLogged += new BuildEventHandler(logger.MessageLogged);

            Project.BuildListeners.Add(logger);
        }

        #endregion Private Instance Methods
    }
}
