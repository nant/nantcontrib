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
using System.IO;

using InterOpStarTeam = StarTeam;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.StarTeam {
    /// <summary>
    /// Allows creation of view labels in StarTeam repositories.
    /// </summary>
    /// <remarks>
    /// <para>Often when building projects you wish to label the source control repository.</para>
    /// <para>By default this task creates view labels with the build option turned on.</para>
    /// <para>This task was ported from the Ant task http://jakarta.apache.org/ant/manual/OptionalTasks/starteam.html#stlabel </para>
    /// <para>You need to have the StarTeam SDK installed for this task to function correctly.</para>
    /// </remarks>
    /// <example>
    ///   <para>Creates a label in a StarTeam repository.</para>
    ///   <code>
    ///     <![CDATA[
    /// <!-- 
    ///   constructs a 'url' containing connection information to pass to the task 
    ///   alternatively you can set each attribute manually 
    /// -->
    /// <property name="ST.url" value="user:pass@serverhost:49201/projectname/viewname" />
    /// <stlabel label="3.1 (label title goes here)" description="This is a label description" url="${ST.url}" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("stlabel")]
    public class StarTeamLabel : LabelTask {
        #region Override implementation of LabelTask

        /// <summary>
        /// This method does the work of creating the new view and checking it 
        /// into Starteam.
        /// </summary>
        protected override void  ExecuteTask() {   
            InterOpStarTeam.StView snapshot = openView();
            createLabel(snapshot);
        }

        #endregion Override implementation of LabelTask
    }
}
