//
// NAntContrib
// Copyright (C) 2001-2004
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
// Matt Harp; Seapine Software, Inc.
//

using System.Text;

using NAnt.Core.Attributes;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks.SurroundSCM {
    /// <summary>
    /// Unlocks frozen branches for a <see href="http://www.seapine.com/surroundscm.html">Surround SCM</see>
    /// repository.
    /// </summary>
    /// <example>
    ///   <para>
    ///   Unfreeze the 'Widget 1.0' branch off of the mainline 'Mainline' on the 
    ///   server at localhost, port 4900 with username 'administrator' and a 
    ///   blank password.
    ///   </para>
    ///   <code>
    /// &lt;sscmunfreeze
    ///     serverconnect=&quot;localhost:4900&quot;
    ///     serverlogin=&quot;administrator:&quot;
    ///     mainline=&quot;Mainline&quot;
    ///     branch=&quot;Widget 1.0&quot;
    /// /&gt;
    ///   </code>
    /// </example>
    [TaskName("sscmunfreeze")]
    public class SSCMUnFreeze : SSCMTask {
        #region Private Instance Fields

        private string _branch;
        private string _mainline;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// Surround SCM branch name.
        /// </summary>
        [TaskAttribute("branch", Required=true)]
        [StringValidator(AllowEmpty=false)]
        public string Branch { 
            get { return _branch; }
            set { _branch = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Surround SCM mainline branch name. The default is pulled from the local working directory.
        /// </summary>
        [TaskAttribute("mainline", Required=false)]
        public string Mainline { 
            get { return _mainline; }
            set { _mainline = StringUtils.ConvertEmptyToNull(value); }
        }

        #endregion Public Instance Properties

        #region Override implementation of SSCMTask

        /// <summary>
        /// Writes the task-specific arguments to the specified 
        /// <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="argBuilder">The <see cref="StringBuilder" /> to write the task-specific arguments to.</param>
        protected override void WriteCommandLineArguments(StringBuilder argBuilder) {
            argBuilder.Append("unfreeze");

            argBuilder.Append(" \"");
            argBuilder.Append(Branch);
            argBuilder.Append("\"");

            // mainline
            if (Mainline != null) {
                argBuilder.Append(" -p\"");
                argBuilder.Append(Mainline );
                argBuilder.Append("\"");
            }
        }

        #endregion Override implementation of SSCMTask
    }
}
