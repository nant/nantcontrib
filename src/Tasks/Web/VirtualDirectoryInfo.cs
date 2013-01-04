// NAntContrib
// Copyright (C) 2002 Brian Nantz bcnantz@juno.com
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
// Brian Nantz (bcnantz@juno.com)
// Gert Driesen (drieseng@users.sourceforge.net)

using System;
using System.Collections;
using System.DirectoryServices;
using System.Globalization;
using System.IO;

using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks.Web {
    /// <summary>
    /// Lists the configuration settings of a specified virtual directory in a
    /// web site hosted on Internet Information Server.
    /// </summary>
    /// <example>
    ///   <para>
    ///   List the settings of a virtual directory named <c>Temp</c>.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <iisdirinfo vdirname="Temp" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("iisdirinfo")]
    public class VirtualDirectoryInfo : WebBase {
        protected override void ExecuteTask() {
            Log(Level.Info, "Retrieving settings of virtual directory '{0}'"
                + " on '{1}' (website: {2}).", this.VirtualDirectory, 
                this.Server, this.Website);

            // ensure IIS is available on specified host and port
            this.CheckIISSettings();

            try {
                DirectoryEntry folderRoot = new DirectoryEntry(this.ServerPath);
                folderRoot.RefreshCache();
                DirectoryEntry newVirDir = folderRoot.Children.Find(
                    this.VirtualDirectory, folderRoot.SchemaClassName);

                bool supportsPropertyEnumeration;

                try {
                    supportsPropertyEnumeration = newVirDir.Properties.PropertyNames.Count >= 0;
                } catch {
                    supportsPropertyEnumeration = false;
                }

                // the IIS ADSI provider only supports enumeration of properties
                // on IIS6 (and Windows XP SP2, but well ...) or higher
                if (supportsPropertyEnumeration) {
                    foreach (string propertyName in newVirDir.Properties.PropertyNames) {
                        object propertyValue = newVirDir.Properties[propertyName].Value;

                        if (propertyValue == null)
                        {
                            Log(Level.Info, propertyName + ":  Null");
                        }
                        else
                        {
                            if (propertyValue.GetType().IsArray)
                            {
                                Log(Level.Info, propertyName + ":");

                                Array propertyValues = (Array)propertyValue;
                                foreach (object value in propertyValues)
                                {
                                    Log(Level.Info, '\t' + value.ToString());
                                }
                            }
                            else
                            {
                                Log(Level.Info, propertyName + ": "
                                    + newVirDir.Properties[propertyName].Value.ToString());
                            }
                        }
                        
                    }
                } else {
                    Log(Level.Info, "AccessExecute: " + newVirDir.Properties["AccessExecute"].Value.ToString());
                    Log(Level.Info, "AccessFlags: " + newVirDir.Properties["AccessFlags"].Value.ToString());
                    Log(Level.Info, "AccessNoRemoteExecute: " + newVirDir.Properties["AccessNoRemoteExecute"].Value.ToString());
                    Log(Level.Info, "AccessNoRemoteRead: " + newVirDir.Properties["AccessNoRemoteRead"].Value.ToString());
                    Log(Level.Info, "AccessNoRemoteScript: " + newVirDir.Properties["AccessNoRemoteScript"].Value.ToString());
                    Log(Level.Info, "AccessNoRemoteWrite: " + newVirDir.Properties["AccessNoRemoteWrite"].Value.ToString());
                    Log(Level.Info, "AccessRead: " + newVirDir.Properties["AccessRead"].Value.ToString());
                    Log(Level.Info, "AccessSource: " + newVirDir.Properties["AccessSource"].Value.ToString());
                    Log(Level.Info, "AccessScript: " + newVirDir.Properties["AccessScript"].Value.ToString());
                    Log(Level.Info, "AccessSSL: " + newVirDir.Properties["AccessSSL"].Value.ToString());
                    Log(Level.Info, "AccessSSL128: " + newVirDir.Properties["AccessSSL128"].Value.ToString());
                    Log(Level.Info, "AccessSSLFlags: " + newVirDir.Properties["AccessSSLFlags"].Value.ToString());
                    Log(Level.Info, "AccessSSLMapCert: " + newVirDir.Properties["AccessSSLMapCert"].Value.ToString());
                    Log(Level.Info, "AccessSSLNegotiateCert: " + newVirDir.Properties["AccessSSLNegotiateCert"].Value.ToString());
                    Log(Level.Info, "AccessSSLRequireCert: " + newVirDir.Properties["AccessSSLRequireCert"].Value.ToString());
                    Log(Level.Info, "AccessWrite: " + newVirDir.Properties["AccessWrite"].Value.ToString());
                    Log(Level.Info, "AnonymousPasswordSync: " + newVirDir.Properties["AnonymousPasswordSync"].Value.ToString());
                    Log(Level.Info, "AnonymousUserName: " + newVirDir.Properties["AnonymousUserName"].Value.ToString());
                    Log(Level.Info, "AnonymousUserPass: " + newVirDir.Properties["AnonymousUserPass"].Value.ToString());
                    Log(Level.Info, "AppAllowClientDebug: " + newVirDir.Properties["AppAllowClientDebug"].Value.ToString());
                    Log(Level.Info, "AppAllowDebugging: " + newVirDir.Properties["AppAllowDebugging"].Value.ToString());
                    Log(Level.Info, "AppFriendlyName: " + newVirDir.Properties["AppFriendlyName"].Value.ToString());
                    Log(Level.Info, "AppIsolated: " + newVirDir.Properties["AppIsolated"].Value.ToString());
                    Log(Level.Info, "AppOopRecoverLimit: " + newVirDir.Properties["AppOopRecoverLimit"].Value.ToString());
                    Log(Level.Info, "AppPackageID: " + newVirDir.Properties["AppPackageID"].Value.ToString());
                    Log(Level.Info, "AppPackageName: " + newVirDir.Properties["AppPackageName"].Value.ToString());
                    Log(Level.Info, "AppRoot: " + newVirDir.Properties["AppRoot"].Value.ToString());
                    Log(Level.Info, "AppWamClsID: " + newVirDir.Properties["AppWamClsID"].Value.ToString());
                    Log(Level.Info, "AspAllowOutOfProcComponents: " + newVirDir.Properties["AspAllowOutOfProcComponents"].Value.ToString());
                    Log(Level.Info, "AspAllowSessionState: " + newVirDir.Properties["AspAllowSessionState"].Value.ToString());
                    Log(Level.Info, "AspBufferingOn: " + newVirDir.Properties["AspBufferingOn"].Value.ToString());
                    Log(Level.Info, "AspCodepage: " + newVirDir.Properties["AspCodepage"].Value.ToString());
                    Log(Level.Info, "AspEnableApplicationRestart: " + newVirDir.Properties["AspEnableApplicationRestart"].Value.ToString());
                    Log(Level.Info, "AspEnableAspHtmlFallback: " + newVirDir.Properties["AspEnableAspHtmlFallback"].Value.ToString());
                    Log(Level.Info, "AspEnableChunkedEncoding: " + newVirDir.Properties["AspEnableChunkedEncoding"].Value.ToString());
                    Log(Level.Info, "AspEnableParentPaths: " + newVirDir.Properties["AspEnableParentPaths"].Value.ToString());
                    Log(Level.Info, "AspEnableTypelibCache: " + newVirDir.Properties["AspEnableTypelibCache"].Value.ToString());
                    Log(Level.Info, "AspErrorsToNTLog: " + newVirDir.Properties["AspErrorsToNTLog"].Value.ToString());
                    Log(Level.Info, "AspExceptionCatchEnable: " + newVirDir.Properties["AspExceptionCatchEnable"].Value.ToString());
                    Log(Level.Info, "AspLogErrorRequests: " + newVirDir.Properties["AspLogErrorRequests"].Value.ToString());
                    Log(Level.Info, "AspProcessorThreadMax: " + newVirDir.Properties["AspProcessorThreadMax"].Value.ToString());
                    Log(Level.Info, "AspQueueConnectionTestTime: " + newVirDir.Properties["AspQueueConnectionTestTime"].Value.ToString());
                    Log(Level.Info, "AspQueueTimeout: " + newVirDir.Properties["AspQueueTimeout"].Value.ToString());
                    Log(Level.Info, "AspRequestQueueMax: " + newVirDir.Properties["AspRequestQueueMax"].Value.ToString());
                    Log(Level.Info, "AspScriptEngineCacheMax: " + newVirDir.Properties["AspScriptEngineCacheMax"].Value.ToString());
                    Log(Level.Info, "AspScriptErrorMessage: " + newVirDir.Properties["AspScriptErrorMessage"].Value.ToString());
                    Log(Level.Info, "AspScriptErrorSentToBrowser: " + newVirDir.Properties["AspScriptErrorSentToBrowser"].Value.ToString());
                    Log(Level.Info, "AspScriptFileCacheSize: " + newVirDir.Properties["AspScriptFileCacheSize"].Value.ToString());
                    Log(Level.Info, "AspScriptLanguage: " + newVirDir.Properties["AspScriptLanguage"].Value.ToString());
                    Log(Level.Info, "AspScriptTimeout: " + newVirDir.Properties["AspScriptTimeout"].Value.ToString());
                    Log(Level.Info, "AspSessionMax: " + newVirDir.Properties["AspSessionMax"].Value.ToString());
                    Log(Level.Info, "AspSessionTimeout: " + newVirDir.Properties["AspSessionTimeout"].Value.ToString());
                    Log(Level.Info, "AuthAnonymous: " + newVirDir.Properties["AuthAnonymous"].Value.ToString());
                    Log(Level.Info, "AuthBasic: " + newVirDir.Properties["AuthBasic"].Value.ToString());
                    Log(Level.Info, "AuthFlags: " + newVirDir.Properties["AuthFlags"].Value.ToString());
                    Log(Level.Info, "AuthNTLM: " + newVirDir.Properties["AuthNTLM"].Value.ToString());
                    Log(Level.Info, "AuthPersistSingleRequest: " + newVirDir.Properties["AuthPersistSingleRequest"].Value.ToString());
                    Log(Level.Info, "AuthPersistence: " + newVirDir.Properties["AuthPersistence"].Value.ToString());				
                    Log(Level.Info, "CacheControlCustom: " + newVirDir.Properties["CacheControlCustom"].Value.ToString());
                    Log(Level.Info, "CacheControlMaxAge: " + newVirDir.Properties["CacheControlMaxAge"].Value.ToString());
                    Log(Level.Info, "CacheControlNoCache: " + newVirDir.Properties["CacheControlNoCache"].Value.ToString());
                    Log(Level.Info, "CacheISAPI: " + newVirDir.Properties["CacheISAPI"].Value.ToString());
                    Log(Level.Info, "ContentIndexed: " + newVirDir.Properties["ContentIndexed"].Value.ToString());				
                    Log(Level.Info, "CreateCGIWithNewConsole: " + newVirDir.Properties["CreateCGIWithNewConsole"].Value.ToString());
                    Log(Level.Info, "CreateProcessAsUser: " + newVirDir.Properties["CreateProcessAsUser"].Value.ToString());
                    Log(Level.Info, "DefaultDoc: " + newVirDir.Properties["DefaultDoc"].Value.ToString());
                    Log(Level.Info, "DefaultDocFooter: " + newVirDir.Properties["DefaultDocFooter"].Value.ToString());
                    Log(Level.Info, "DefaultLogonDomain: " + newVirDir.Properties["DefaultLogonDomain"].Value.ToString());
                    Log(Level.Info, "DirBrowseFlags: " + newVirDir.Properties["DirBrowseFlags"].Value.ToString());
                    Log(Level.Info, "DirBrowseShowDate: " + newVirDir.Properties["DirBrowseShowDate"].Value.ToString());
                    Log(Level.Info, "DirBrowseShowExtension: " + newVirDir.Properties["DirBrowseShowExtension"].Value.ToString());
                    Log(Level.Info, "DirBrowseShowLongDate: " + newVirDir.Properties["DirBrowseShowLongDate"].Value.ToString());
                    Log(Level.Info, "DirBrowseShowSize: " + newVirDir.Properties["DirBrowseShowSize"].Value.ToString());
                    Log(Level.Info, "DirBrowseShowTime: " + newVirDir.Properties["DirBrowseShowTime"].Value.ToString());
                    Log(Level.Info, "DontLog: " + newVirDir.Properties["DontLog"].Value.ToString());
                    Log(Level.Info, "EnableDefaultDoc: " + newVirDir.Properties["EnableDefaultDoc"].Value.ToString());
                    Log(Level.Info, "EnableDirBrowsing: " + newVirDir.Properties["EnableDirBrowsing"].Value.ToString());
                    Log(Level.Info, "EnableDocFooter: " + newVirDir.Properties["EnableDocFooter"].Value.ToString());
                    Log(Level.Info, "EnableReverseDns: " + newVirDir.Properties["EnableReverseDns"].Value.ToString());
                    Log(Level.Info, "HttpExpires: " + newVirDir.Properties["HttpExpires"].Value.ToString());
                    Log(Level.Info, "HttpRedirect: " + newVirDir.Properties["HttpRedirect"].Value.ToString());
                    Log(Level.Info, "LogonMethod: " + newVirDir.Properties["LogonMethod"].Value.ToString());
                    Log(Level.Info, "Path: " + newVirDir.Properties["Path"].Value.ToString());				
                    Log(Level.Info, "Realm: " + newVirDir.Properties["Realm"].Value.ToString());
                    Log(Level.Info, "UNCPassword: " + newVirDir.Properties["UNCPassword"].Value.ToString());
                    Log(Level.Info, "UNCUserName: " + newVirDir.Properties["UNCUserName"].Value.ToString());
                    Log(Level.Info, "UploadReadAheadSize: " + newVirDir.Properties["UploadReadAheadSize"].Value.ToString());
                    Log(Level.Info, "AspTrackThreadingModel: " + newVirDir.Properties["AspTrackThreadingModel"].Value.ToString());
                    Log(Level.Info, "AuthPersistSingleRequestIfProxy: " + newVirDir.Properties["AuthPersistSingleRequestIfProxy"].Value.ToString());
                    Log(Level.Info, "AuthPersistSingleRequestAlwaysIfProxy: " + newVirDir.Properties["AuthPersistSingleRequestAlwaysIfProxy"].Value.ToString());
                    Log(Level.Info, "PoolIDCTimeout: " + newVirDir.Properties["PoolIDCTimeout"].Value.ToString());
                    Log(Level.Info, "PutReadSize: " + newVirDir.Properties["PutReadSize"].Value.ToString());
                    Log(Level.Info, "RedirectHeaders: " + newVirDir.Properties["RedirectHeaders"].Value.ToString());
                    Log(Level.Info, "SSIExecDisable: " + newVirDir.Properties["SSIExecDisable"].Value.ToString());
                    Log(Level.Info, "UNCAuthenticationPassthrough: " + newVirDir.Properties["UNCAuthenticationPassthrough"].Value.ToString());
                    
                    // these seem to cause problems on Windows XP SP2

                    /*
                    Log(Level.Info, "AspThreadGateEnabled: " + newVirDir.Properties["AspThreadGateEnabled"].Value.ToString());
                    Log(Level.Info, "AspThreadGateLoadHigh: " + newVirDir.Properties["AspThreadGateLoadHigh"].Value.ToString());
                    Log(Level.Info, "AspThreadGateLoadLow: " + newVirDir.Properties["AspThreadGateLoadLow"].Value.ToString());
                    Log(Level.Info, "AspThreadGateSleepDelay: " + newVirDir.Properties["AspThreadGateSleepDelay"].Value.ToString());
                    Log(Level.Info, "AspThreadGateSleepMax: " + newVirDir.Properties["AspThreadGateSleepMax"].Value.ToString());
                    Log(Level.Info, "AspThreadGateTimeSlice: " + newVirDir.Properties["AspThreadGateTimeSlice"].Value.ToString());
                    Log(Level.Info, "CpuAppEnabled: " + newVirDir.Properties["CpuAppEnabled"].Value.ToString());
                    Log(Level.Info, "CpuCgiEnabled: " + newVirDir.Properties["CpuCgiEnabled"].Value.ToString());
                    */
                }

                newVirDir.Close();
                folderRoot.Close();
            } catch (BuildException) {
                // re-throw exception
                throw;
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Error retrieving info for virtual directory '{0}' on '{1}' (website: {2}).", 
                    this.VirtualDirectory, this.Server, this.Website), Location, ex);
            }
        }
    }
}
