// NAnt - A .NET build tool
// Copyright (C) 2001-2006 Gerry Shaw
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
// Gert Driesen (drieseng@users.sourceforge.net)

using System;
using System.ServiceProcess;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;
using NAnt.Core.Util;

namespace NAnt.Contrib.Functions {
    /// <summary>
    /// Allow information on a Windows service to be retrieved.
    /// </summary>
    [FunctionSet("service", "Windows Service")]
    public class ServiceFunctions : FunctionSetBase {
        #region Public Instance Constructors
        
        public ServiceFunctions(Project project, PropertyDictionary properties) : base(project, properties) { }
        
        #endregion Public Instance Constructors

        #region Public Static Methods
        
        /// <summary>
        /// Returns a value indicating whether the specified service is 
        /// installed on a given machine.
        /// </summary>
        /// <param name="service">The short name that identifies the service to the system.</param>
        /// <param name="machineName">The computer on which the service resides.</param>
        /// <returns>
        /// <see langword="true" /> if the service is installed; otherwise,
        /// <see langword="false" />.
        /// </returns>
        /// <remarks>
        /// For the machineName parameter, you can use "." or a zero-length
        /// <see cref="string" /> to represent the local computer.
        /// </remarks>
        /// <example>
        ///   <para>
        ///   The following example starts the "World Wide Web Publishing"
        ///   service if it's installed on the local computer.
        ///   </para>
        ///   <code>
        ///     <![CDATA[
        /// <if test="${service::is-installed('World Wide Web Publishing', '.')}">
        ///     <servicecontroller action="Start" service="World Wide Web Publishing" />
        /// </if>
        ///     ]]>
        ///   </code>
        /// </example>
        [Function("is-installed")]
        public static bool IsInstalled(string service, string machineName) {
            using (ServiceController sc = GetServiceController(service, machineName)) {
                try {
                    bool isStarted = (sc.Status == ServiceControllerStatus.Running);
                    if (!isStarted) {
                        // done to prevent CS0219, variable is assigned but its
                        // value is never used
                    }
                    return true;
                } catch (InvalidOperationException) {
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns a value indicating whether the specified service is running.
        /// </summary>
        /// <param name="service">The short name that identifies the service to the system.</param>
        /// <param name="machineName">The computer on which the service resides.</param>
        /// <returns>
        /// <see langword="true" /> if the service is running; otherwise,
        /// <see langword="false" />.
        /// </returns>
        /// <remarks>
        /// For the machineName parameter, you can use "." or a zero-length
        /// <see cref="string" /> to represent the local computer.
        /// </remarks>
        [Function("is-running")]
        public static bool IsRunning(string service, string machineName) {
            using (ServiceController sc = GetServiceController(service, machineName)) {
                return sc.Status == ServiceControllerStatus.Running;
            }
        }

        /// <summary>
        /// Returns a value indicating whether the specified service is stopped.
        /// </summary>
        /// <param name="service">The short name that identifies the service to the system.</param>
        /// <param name="machineName">The computer on which the service resides.</param>
        /// <returns>
        /// <see langword="true" /> if the service is stopped; otherwise,
        /// <see langword="false" />.
        /// </returns>
        /// <remarks>
        /// For the machineName parameter, you can use "." or a zero-length
        /// <see cref="string" /> to represent the local computer.
        /// </remarks>
        [Function("is-stopped")]
        public static bool IsStopped(string service, string machineName) {
            using (ServiceController sc = GetServiceController(service, machineName)) {
                return sc.Status == ServiceControllerStatus.Stopped;
            }
        }

        /// <summary>
        /// Returns a value indicating whether the specified service is paused.
        /// </summary>
        /// <param name="service">The short name that identifies the service to the system.</param>
        /// <param name="machineName">The computer on which the service resides.</param>
        /// <returns>
        /// <see langword="true" /> if the service is paused; otherwise,
        /// <see langword="false" />.
        /// </returns>
        /// <remarks>
        /// For the machineName parameter, you can use "." or a zero-length
        /// <see cref="string" /> to represent the local computer.
        /// </remarks>
        [Function("is-paused")]
        public static bool IsPaused(string service, string machineName) {
            using (ServiceController sc = GetServiceController(service, machineName)) {
                return sc.Status == ServiceControllerStatus.Paused;
            }
        }

        /// <summary>
        /// Gets the status of the specified service.
        /// </summary>
        /// <param name="service">The short name that identifies the service to the system.</param>
        /// <param name="machineName">The computer on which the service resides.</param>
        /// <returns>
        /// One of the <see cref="ServiceControllerStatus" /> values that
        /// indicates whether the service is running, stopped, or paused, or
        /// whether a start, stop, pause, or continue command is pending.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///   For the machineName parameter, you can use "." or a zero-length
        ///   <see cref="string" /> to represent the local computer.
        ///   </para>
        ///   <para>
        ///   The value returned by <see cref="GetStatus" /> can be compared 
        ///   to either a corresponding enum field name or the underlying
        ///   integral value.
        ///   </para>
        /// </remarks>
        /// <example>
        ///   <para>
        ///   Displays a warning if the <b>Alerter</b> service is stopping
        ///   on <c>SV-ARD-EAI1</c>.
        ///   </para>
        ///   <code>
        ///     <![CDATA[
        /// <if test="${service::get-status('Alerter', 'SV-ARD-EAI1') == 'StopPending'}">
        ///     <echo level="Warning">The Alerter service is stopping.</echo>
        /// </if>
        ///     ]]>
        ///   </code>
        /// </example>
        /// <example>
        ///   <para>
        ///   The &quot;deploy-web-application&quot; target is only executed if
        ///   IIS is running on the local computer.
        ///   </para>
        ///   <code>
        ///     <![CDATA[
        /// <target name="deploy" depends="deploy-sql-scripts, deploy-web-application" />
        /// 
        /// <target name="deploy-sql-scripts">
        ///     ...
        /// </target>
        /// 
        /// <target name="deploy-web-application" if="$(service::get-status('World Wide Web Publishing', '.') == 4)}">
        ///     ...
        /// </target>
        ///     ]]>
        ///   </code>
        /// </example>
        [Function("get-status")]
        public static ServiceControllerStatus GetStatus(string service, string machineName) {
            using (ServiceController sc = GetServiceController(service, machineName)) {
                return sc.Status;
            }
        }

        /// <summary>
        /// Gets the friendly name of the specified service.
        /// </summary>
        /// <param name="service">The short name that identifies the service to the system.</param>
        /// <param name="machineName">The computer on which the service resides.</param>
        /// <returns>
        /// The friendly name of the service, which can be used to identify the service.
        /// </returns>
        [Function("get-display-name")]
        public static string GetDisplayName(string service, string machineName) {
            using (ServiceController sc = GetServiceController(service, machineName)) {
                return sc.DisplayName;
            }
        }

        /// <summary>
        /// Gets the name that identifies the specified service 
        /// </summary>
        /// <param name="service">The short name that identifies the service to the system.</param>
        /// <param name="machineName">The computer on which the service resides.</param>
        /// <returns>
        /// The name that identifies the service.
        /// </returns>
        [Function("get-service-name")]
        public static string GetServiceName(string service, string machineName) {
            using (ServiceController sc = GetServiceController(service, machineName)) {
                return sc.ServiceName;
            }
        }

        #endregion Public Instance Methods

        #region Private Static Methods

        private static ServiceController GetServiceController(string service, string machineName) {
            if (String.IsNullOrEmpty(machineName)) {
                machineName = ".";
            }
            
            return new ServiceController(service, machineName);
        }

        #endregion Private Static Methods
    }
}
