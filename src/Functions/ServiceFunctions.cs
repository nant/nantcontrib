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
// Gert Driesen (gert.driesen@ardatis.com)

using System;
using System.ServiceProcess;

using NAnt.Core;
using NAnt.Core.Types;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Functions {
    /// <summary>
    /// Allow information on a Windows service to be retrieved.
    /// </summary>
    [FunctionSet("service", "Services")]
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
        /// <param name="machine">The computer on which the service resides.</param>
        /// <returns>
        /// <see langword="true" /> if the service is installed; otherwise,
        /// <see langword="false" />.
        /// </returns>
        [Function("is-installed")]
        public static bool IsInstalled(string service, string machine) {
            using (ServiceController sc = new ServiceController(service, machine)) {
                try {
                    bool isStarted = (sc.Status == ServiceControllerStatus.Running);
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
        /// <param name="machine">The computer on which the service resides.</param>
        /// <returns>
        /// <see langword="true" /> if the service is running; otherwise,
        /// <see langword="false" />.
        /// </returns>
        [Function("is-running")]
        public static bool IsRunning(string service, string machine){
            using (ServiceController sc = new ServiceController(service, machine)) {
                return sc.Status == ServiceControllerStatus.Running;
            }
        }

        /// <summary>
        /// Returns a value indicating whether the specified service is stopped.
        /// </summary>
        /// <param name="service">The short name that identifies the service to the system.</param>
        /// <param name="machine">The computer on which the service resides.</param>
        /// <returns>
        /// <see langword="true" /> if the service is stopped; otherwise,
        /// <see langword="false" />.
        /// </returns>
        [Function("is-stopped")]
        public static bool IsStopped(string service, string machine){
            using (ServiceController sc = new ServiceController(service, machine)) {
                return sc.Status == ServiceControllerStatus.Stopped;
            }
        }

        /// <summary>
        /// Returns a value indicating whether the specified service is paused.
        /// </summary>
        /// <param name="service">The short name that identifies the service to the system.</param>
        /// <param name="machine">The computer on which the service resides.</param>
        /// <returns>
        /// <see langword="true" /> if the service is paused; otherwise,
        /// <see langword="false" />.
        /// </returns>
        [Function("is-paused")]
        public static bool IsPaused(string service, string machine){
            using (ServiceController sc = new ServiceController(service, machine)) {
                return sc.Status == ServiceControllerStatus.Paused;
            }
        }

        #endregion Public Instance Methods
    }
}
