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

using InterOpStarTeam = StarTeam;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.StarTeam {
    /// <summary>
    /// Base star team task.
    /// </summary>
    /// <remarks>
    /// <para> 
    /// Common super class for all StarTeam tasks. At this level of the hierarchy we are concerned only with obtaining a
    /// connection to the StarTeam server.  The subclass <see cref="TreeBasedTask"/>, abstracts tree-walking 
    /// behavior common to many subtasks.
    /// </para>
    /// <para>This class ported from the Ant task http://jakarta.apache.org/ant/manual/OptionalTasks/starteam.html </para>
    /// <para>You need to have the StarTeam SDK installed for StarTeam tasks to function correctly.</para>
    /// </remarks>
    /// <seealso cref="TreeBasedTask"/>
    /// <author> <a href="mailto:jcyip@thoughtworks.com">Jason Yip</a></author>
    /// <author> <a href="mailto:stevec@ignitesports.com">Steve Cohen</a></author>
    public abstract class StarTeamTask : NAnt.Core.Task {
        /// <summary>
        /// Name of StarTeamServer.
        /// </summary>
        /// <remarks>
        /// Required if <see cref="url"/> is not set. If you wish to set all 
        /// connection parameters at once set <see cref="url"/>.
        /// </remarks>
        [TaskAttribute("servername", Required=false)]
        public virtual string servername {
            get  { return _servername; }
            set  { _servername = value; }
        }

        /// <summary>
        /// Port number of the StarTeam connection.
        /// </summary>
        /// <remarks>
        /// Required if <see cref="url" /> is not set. If you wish to set all 
        /// connection parameters at once set <see cref="url" />.
        /// </remarks>
        [TaskAttribute("serverport", Required=false)]
        public virtual string serverport {
            get { return _serverport; }
            set { _serverport = value; }
        }

        /// <summary>
        /// The name of the StarTeam project to be acted on
        /// </summary>
        /// <remarks>
        /// Required if <see cref="url"/> is not set. If you wish to set all 
        /// connection parameters at once set <see cref="url"/>.
        /// </remarks>
        [TaskAttribute("projectname", Required=false)]
        public virtual string projectname {
            get { return _projectname; }
            set { _projectname = value; }
        }

        /// <summary>
        /// The name of the StarTeam view to be acted on.
        /// </summary>
        /// <remarks>
        /// Required if <see cref="url"/> is not set. If you wish to set all 
        /// connection parameters at once set <see cref="url"/>.
        /// </remarks>
        [TaskAttribute("viewname", Required=false)]
        public virtual string viewname {
            get { return _viewname; }
            set { _viewname = value; }
        }

        /// <summary>
        /// The StarTeam user name used for login.
        /// </summary>
        /// <remarks>
        /// Required if <see cref="url"/> is not set. If you wish to set all 
        /// connection parameters at once set <see cref="url"/>.
        /// </remarks>
        [TaskAttribute("username", Required=false)]
        public virtual string username {
            get { return _username; }
            set { _username = value; }
        }

        /// <summary> 
        /// The password used for login.
        /// </summary>
        /// <remarks>
        /// Required if <see cref="url"/> is not set. If you wish to set all 
        /// connection parameters at once set <see cref="url"/>.
        /// </remarks>
        [TaskAttribute("password", Required=false)]
        public virtual string password {
            get { return _password; }
            set { _password = value; }
        }

        /// <summary> 
        /// One stop to set all parameters needed to connect to a StarTeam server.
        /// </summary>
        /// <remarks>
        /// <para>If you do not wish to specify a url you can set each parameter individually. 
        /// You must set all connection parameters for the task to be able to connect to the StarTeam server.</para>
        /// </remarks>
        /// <example>
        /// <para>Here is how to configure the url string.</para>
        /// <code>servername:portnum/project/view</code>
        /// <para>You can optionally specify a username and password.</para>
        /// <code>username:password@servername:portnum/project/view</code>
        /// </example>
        /// <seealso cref="servername"/>
        /// <seealso cref="serverport"/>
        /// <seealso cref="projectname"/>
        /// <seealso cref="viewname"/>
        /// <seealso cref="username"/>
        /// <seealso cref="password"/>
        [TaskAttribute("url", Required=false)]
        public virtual string url {
            get {
                return string.Format("{0}:{1}@{2}:{3}/{4}/{5}", _username, _password,
                    _servername, _serverport, _projectname, _viewname);
            }
            set {
                //TODO: Convert this to a regular expression
                string[] path = value.Split('@');
                //see if url string looks like:
                //username:password@host:port/projectname/viewname
                if(path.Length > 1) {
                    string tmp = path[0];
                    value = path[1];
                    path = tmp.Split(':');
                    _username = path[0];
                    _password = path[1];
                }
                //else it can just look like:
                //host:port/projectname/viewname
                //use a uribuilder object to make life easier
                path = value.Split('/');
                _projectname = path[1];
                _viewname = path[2];
                path = path[0].Split(':');
                _servername = path[0];
                _serverport = path[1];
            }
        }

        /// <value> The username of the connection</value>
        private string _username;

        /// <value> The username of the connection</value>
        private string _password;

        /// <value> name of Starteam server to connect to</value>
        private string _servername;

        /// <value> port of Starteam server to connect to</value>
        private string _serverport;

        /// <value> name of Starteam project to connect to</value>
        private string _projectname;

        /// <value> name of Starteam view to connect to</value>
        private string _viewname;

        /// <value>The starteam server through which all activities will be done.</value>
        protected InterOpStarTeam.StServer _server = null;

        /// <summary>
        /// Derived classes must override this method to instantiate a view configured appropriately to its task.
        /// </summary>
        /// <param name="rawview">the unconfigured <code>View</code></param>
        /// <returns>the view appropriately configured.</returns>
        protected internal abstract InterOpStarTeam.StView createSnapshotView(InterOpStarTeam.StView rawview);

        /// <summary>
        /// All tasks will call on this method to connect to StarTeam and open the view for processing.  
        /// </summary>
        /// <returns>the a view to be used for processing.</returns>
        /// <seealso cref="createSnapshotView"/>
        protected internal virtual InterOpStarTeam.StView openView() {
            InterOpStarTeam.StStarTeamFinderStatics starTeamFinder = new InterOpStarTeam.StStarTeamFinderStatics();
            InterOpStarTeam.StView view = starTeamFinder.openView(this.url);

            if (null == view) {
                throw new BuildException("Cannot find view" + this.url + " in repository()",Location);
            }

            InterOpStarTeam.StView snapshot = createSnapshotView(view);
            _server = snapshot.Server;
            return snapshot;
        }

        /// <summary> Returns the name of the user or a blank string if the user is not found.</summary>
        /// <param name="userID">a user's ID</param>
        /// <returns>the name of the user</returns>
        protected internal virtual string getUserName(int userID) {
            InterOpStarTeam.StUser u = _server.getUser(userID);
            if (null == u) {
                return "";
            }
            return u.Name;
        }
    }
}