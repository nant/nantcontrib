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
using System;
using System.IO;
using System.DirectoryServices;
using SourceForge.NAnt;
using SourceForge.NAnt.Attributes;

namespace NAnt.Contrib.Tasks
{   /// <summary>
   /// A task to Create or Modify a IIS Virtual Directory for the Default Web Site.
   /// </summary>
   /// <remarks>
   /// If the virtual directory does not exist it is created.
   /// If the virtual directory alread exists it is modified.
   /// Required Parameters are dirpath and vdirname.
   /// For a list of optional parameters see <a href="ms-help://MS.VSCC/MS.MSDNVS/iisref/html/psdk/asp/aore8v5e.htm">IIsWebVirtualDir</a>.
   /// </remarks>
   /// <example>
   ///   <para>Creates a Temp IIS Virtual Directory pointing to <c>c:\temp</c> on the local machine.</para>
   ///   <code><![CDATA[
   ///      <mkiisdir 
   ///         dirpath="c:\temp"
   ///         vdirname="TEMP"
   ///      />
   ///   ]]></code>
   /// </example>
   /// <example>
   ///   <para>Creates a Temp IIS Virtual Directory pointing to <c>c:\temp</c> on the machine <c>staging</c>.</para>
   ///   <code><![CDATA[
   ///      <mkiisdir 
   ///         iisserver="staging"
   ///         dirpath="c:\temp"
   ///         vdirname="TEMP"
   ///      />
   ///   ]]></code>
   /// </example>
   [TaskName("mkiisdir")]
   public class MkIISDirTask : Task{

      private string _iisserver = "localhost";
      private const string _apppath = "/LM/W3SVC/1/Root/";
      private string _dirpath = null;
      private string _vdirname = null;
      private bool _accessexecute = false;
      private bool _accessnoremoteexecute = false;
      private bool _accessnoremoteread = false;
      private bool _accessnoremotescript = false;
      private bool _accessnoremotewrite = false;
      private bool _accessread = true;
      private bool _accesssource = false;
      private bool _accessscript = true;
      private bool _accessssl = false;
      private bool _accessssl128 = false;
      private bool _accesssslmapcert = false;
      private bool _accesssslnegotiatecert = false;
      private bool _accesssslrequirecert = false;
      private bool _accesswrite = false;
      private bool _anonymouspasswordsync = true;
      private bool _appallowclientdebug = false;
      private bool _appallowdebugging = false;
      private bool _aspallowsessionstate = true;
      private bool _aspbufferingon = true;
      private bool _aspenableapplicationrestart = true;
      private bool _aspenableasphtmlfallback = false;
      private bool _aspenablechunkedencoding = true;
      private bool _asperrorstontlog = false;
      private bool _aspenableparentpaths = true;
      private bool _aspenabletypelibcache = true;
      private bool _aspexceptioncatchenable = true;
      private bool _asplogerrorrequests = true;
      private bool _aspscripterrorsenttobrowser = true;
      private bool _aspthreadgateenabled = false;
      private bool _asptrackthreadingmodel = false;
      private bool _authanonymous = true;
      private bool _authbasic = false;
      private bool _authntlm = false;
      private bool _authpersistsinglerequest = false;
      private bool _authpersistsinglerequestifproxy = true;
      private bool _authpersistsinglerequestalwaysifproxy = false;
      private bool _cachecontrolnocache = false;
      private bool _cacheisapi = true;
      private bool _contentindexed = true;
      private bool _cpuappenabled = true;
      private bool _cpucgienabled = true;
      private bool _createcgiwithnewconsole = false;
      private bool _createprocessasuser = true;
      private bool _dirbrowseshowdate = true;
      private bool _dirbrowseshowextension = true;
      private bool _dirbrowseshowlongdate = true;
      private bool _dirbrowseshowsize = true;
      private bool _dirbrowseshowtime = true;
      private bool _dontlog = false;
      private bool _enabledefaultdoc = true;
      private bool _enabledirbrowsing = false;
      private bool _enabledocfooter = false;
      private bool _enablereversedns = false;
      private bool _ssiexecdisable = false;
      private bool _uncauthenticationpassthrough = false;
      private string _aspscripterrormessage = "An error occurred on the server when processing the URL.  Please contact the system administrator.";
      private string _defaultdoc = "Default.htm,Default.asp,index.htm,iisstart.asp,Default.aspx";
      // Required
      /// <summary>The file system path</summary>
      [TaskAttribute("dirpath", Required=true)]
      public string dirpath{
         get { return _dirpath; }
         set { _dirpath = value; }
      }

      /// <summary>Name of the IIS Virtual Directory</summary>
      [TaskAttribute("vdirname", Required=true)]
      public string vdirname{
         get { return _vdirname; }
         set { _vdirname = value; }
      }

      // Optional
      /// <summary>The IIS server.  Defaults to localhost.</summary>
      [TaskAttribute("iisserver")]
      public string iisserver{
         get { return _iisserver; }
         set { _iisserver = value; }
      }

      [TaskAttribute("accessexecute")]
      public bool accessexecute{
         get { return _accessexecute; }
         set { _accessexecute = value; }
      }
	
      [TaskAttribute("accessnoremoteexecute")]
      public bool accessnoremoteexecute{
         get { return _accessnoremoteexecute; }
         set { _accessnoremoteexecute = value; }
      }

      [TaskAttribute("accessnoremoteread")]
      public bool accessnoremoteread{
         get { return _accessnoremoteread; }
         set { _accessnoremoteread = value; }
      }

      [TaskAttribute("accessnoremotescript")]
      public bool accessnoremotescript{
         get { return _accessnoremotescript; }
         set { _accessnoremotescript = value; }
      }

      [TaskAttribute("accessnoremotewrite")]
      public bool accessnoremotewrite{
         get { return _accessnoremotewrite; }
         set { _accessnoremotewrite = value; }
      }

      [TaskAttribute("accessread")]
      public bool accessread{
         get { return _accessread; }
         set { _accessread = value; }
      }

      [TaskAttribute("accesssource")]
      public bool accesssource{
         get { return _accesssource; }
         set { _accesssource = value; }
      }

      [TaskAttribute("accessscript")]
      public bool accessscript{
         get { return _accessscript; }
         set { _accessscript = value; }
      }

      [TaskAttribute("accessssl")]
      public bool accessssl{
         get { return _accessssl; }
         set { _accessssl = value; }
      }

      [TaskAttribute("accessssl128")]
      public bool accessssl128{
         get { return _accessssl128; }
         set { _accessssl128 = value; }
      }

      [TaskAttribute("accesssslmapcert")]
      public bool accesssslmapcert{
         get { return _accesssslmapcert; }
         set { _accesssslmapcert = value; }
      }

      [TaskAttribute("accesssslnegotiatecert")]
      public bool accesssslnegotiatecert{
         get { return _accesssslnegotiatecert; }
         set { _accesssslnegotiatecert = value; }
      }

      [TaskAttribute("accesssslrequirecert")]
      public bool accesssslrequirecert{
         get { return _accesssslrequirecert; }
         set { _accesssslrequirecert = value; }
      }

      [TaskAttribute("accesswrite")]
      public bool accesswrite{
         get { return _accesswrite; }
         set { _accesswrite = value; }
      }

      [TaskAttribute("anonymouspasswordsync")]
      public bool anonymouspasswordsync{
         get { return _anonymouspasswordsync; }
         set { _anonymouspasswordsync = value; }
      }
      
      [TaskAttribute("appallowclientdebug")]
      public bool appallowclientdebug{
         get { return _appallowclientdebug; }
         set { _appallowclientdebug = value; }
      }

      [TaskAttribute("appallowdebugging")]
      public bool appallowdebugging{
         get { return _appallowdebugging; }
         set { _appallowdebugging = value; }
      }
      
      [TaskAttribute("aspallowsessionstate")]
      public bool aspallowsessionstate{
         get { return _aspallowsessionstate; }
         set { _aspallowsessionstate = value; }
      }

      [TaskAttribute("aspbufferingon")]
      public bool aspbufferingon{
         get { return _aspbufferingon; }
         set { _aspbufferingon = value; }
      }
      
      [TaskAttribute("aspenableapplicationrestart")]
      public bool aspenableapplicationrestart{
         get { return _aspenableapplicationrestart; }
         set { _aspenableapplicationrestart = value; }
      }

      [TaskAttribute("aspenableasphtmlfallback")]
      public bool aspenableasphtmlfallback{
         get { return _aspenableasphtmlfallback; }
         set { _aspenableasphtmlfallback = value; }
      }
      
      [TaskAttribute("aspenablechunkedencoding")]
      public bool aspenablechunkedencoding{
         get { return _aspenablechunkedencoding; }
         set { _aspenablechunkedencoding = value; }
      }

      [TaskAttribute("asperrorstontlog")]
      public bool asperrorstontlog{
         get { return _asperrorstontlog; }
         set { _asperrorstontlog = value; }
      }

      [TaskAttribute("aspenableparentpaths")]
      public bool aspenableparentpaths{
         get { return _aspenableparentpaths; }
         set { _aspenableparentpaths = value; }
      }
      
      [TaskAttribute("aspenabletypelibcache")]
      public bool aspenabletypelibcache{
         get { return _aspenabletypelibcache; }
         set { _aspenabletypelibcache = value; }
      }

      [TaskAttribute("aspexceptioncatchenable")]
      public bool aspexceptioncatchenable{
         get { return _aspexceptioncatchenable; }
         set { _aspexceptioncatchenable = value; }
      }
      
      [TaskAttribute("asplogerrorrequests")]
      public bool asplogerrorrequests{
         get { return _asplogerrorrequests; }
         set { _asplogerrorrequests = value; }
      }

      [TaskAttribute("aspscripterrorsenttobrowser")]
      public bool aspscripterrorsenttobrowser{
         get { return _aspscripterrorsenttobrowser; }
         set { _aspscripterrorsenttobrowser = value; }
      }
      
      [TaskAttribute("aspthreadgateenabled")]
      public bool aspthreadgateenabled{
         get { return _aspthreadgateenabled; }
         set { _aspthreadgateenabled = value; }
      }
      
      [TaskAttribute("asptrackthreadingmodel")]
      public bool asptrackthreadingmodel{
         get { return _asptrackthreadingmodel; }
         set { _asptrackthreadingmodel = value; }
      }

      [TaskAttribute("authanonymous")]
      public bool authanonymous{
         get { return _authanonymous; }
         set { _authanonymous = value; }
      }

      [TaskAttribute("authbasic")]
      public bool authbasic{
         get { return _authbasic; }
         set { _authbasic = value; }
      }
      
      [TaskAttribute("authntlm")]
      public bool authntlm{
         get { return _authntlm; }
         set { _authntlm = value; }
      }

      [TaskAttribute("authpersistsinglerequest")]
      public bool authpersistsinglerequest{
         get { return _authpersistsinglerequest; }
         set { _authpersistsinglerequest = value; }
      }

      [TaskAttribute("authpersistsinglerequestifproxy")]
      public bool authpersistsinglerequestifproxy{
         get { return _authpersistsinglerequestifproxy; }
         set { _authpersistsinglerequestifproxy = value; }
      }

      [TaskAttribute("authpersistsinglerequestalwaysifproxy")]
      public bool authpersistsinglerequestalwaysifproxy{
         get { return _authpersistsinglerequestalwaysifproxy; }
         set { _authpersistsinglerequestalwaysifproxy = value; }
      }
            
      [TaskAttribute("cachecontrolnocache")]
      public bool cachecontrolnocache{
         get { return _cachecontrolnocache; }
         set { _cachecontrolnocache = value; }
      }

      [TaskAttribute("cacheisapi")]
      public bool cacheisapi{
         get { return _cacheisapi; }
         set { _cacheisapi = value; }
      }

      [TaskAttribute("contentindexed")]
      public bool contentindexed{
         get { return _contentindexed; }
         set { _contentindexed = value; }
      }
      
      [TaskAttribute("cpuappenabled")]
      public bool cpuappenabled{
         get { return _cpuappenabled; }
         set { _cpuappenabled = value; }
      }
      
      [TaskAttribute("cpucgienabled")]
      public bool cpucgienabled{
         get { return _cpucgienabled; }
         set { _cpucgienabled = value; }
      }
      
      [TaskAttribute("createcgiwithnewconsole")]
      public bool createcgiwithnewconsole{
         get { return _createcgiwithnewconsole; }
         set { _createcgiwithnewconsole = value; }
      }
      
      [TaskAttribute("createprocessasuser")]
      public bool createprocessasuser{
         get { return _createprocessasuser; }
         set { _createprocessasuser = value; }
      }
      
      [TaskAttribute("dirbrowseshowdate")]
      public bool dirbrowseshowdate{
         get { return _dirbrowseshowdate; }
         set { _dirbrowseshowdate = value; }
      }
      
      [TaskAttribute("dirbrowseshowextension")]
      public bool dirbrowseshowextension{
         get { return _dirbrowseshowextension; }
         set { _dirbrowseshowextension = value; }
      }

      [TaskAttribute("dirbrowseshowlongdate")]
      public bool dirbrowseshowlongdate{
         get { return _dirbrowseshowlongdate; }
         set { _dirbrowseshowlongdate = value; }
      }
      
      [TaskAttribute("dirbrowseshowsize")]
      public bool dirbrowseshowsize{
         get { return _dirbrowseshowsize; }
         set { _dirbrowseshowsize = value; }
      }
 
      [TaskAttribute("dirbrowseshowtime")]
      public bool dirbrowseshowtime{
         get { return _dirbrowseshowtime; }
         set { _dirbrowseshowtime = value; }
      }

      [TaskAttribute("dontlog")]
      public bool dontlog{
         get { return _dontlog; }
         set { _dontlog = value; }
      }

      [TaskAttribute("enabledefaultdoc")]
      public bool enabledefaultdoc{
         get { return _enabledefaultdoc; }
         set { _enabledefaultdoc = value; }
      }

      [TaskAttribute("enabledirbrowsing")]
      public bool enabledirbrowsing{
         get { return _enabledirbrowsing; }
         set { _enabledirbrowsing = value; }
      }

      [TaskAttribute("enabledocfooter")]
      public bool enabledocfooter{
         get { return _enabledocfooter; }
         set { _enabledocfooter = value; }
      }

      [TaskAttribute("enablereversedns")]
      public bool enablereversedns{
         get { return _enablereversedns; }
         set { _enablereversedns = value; }
      }

      [TaskAttribute("ssiexecdisable")]
      public bool ssiexecdisable{
         get { return _ssiexecdisable; }
         set { _ssiexecdisable = value; }
      }

      [TaskAttribute("uncauthenticationpassthrough")]
      public bool uncauthenticationpassthrough{
         get { return _uncauthenticationpassthrough; }
         set { _uncauthenticationpassthrough = value; }
      }

      [TaskAttribute("aspscripterrormessage")]
      public string aspscripterrormessage{
         get { return _aspscripterrormessage; }
         set { _aspscripterrormessage = value; }
      }

      [TaskAttribute("defaultdoc")]
      public string defaultdoc{
         get { return _defaultdoc; }
         set { _defaultdoc = value; }
      }

      private string iispath {
        get { return "IIS://" + iisserver + "/W3SVC/1/Root" ;}
      }

      protected override void ExecuteTask()
      {
         try{
            DirectoryEntry folderRoot = new DirectoryEntry(iispath);
            folderRoot.RefreshCache();
            DirectoryEntry newVirDir;

            try{
               // Try to find the directory
               DirectoryEntry tempVirDir = folderRoot.Children.Find(_vdirname,folderRoot.SchemaClassName);
               newVirDir = tempVirDir;
            }
            catch{
               // If the directory doesn't exist create it.
               newVirDir = folderRoot.Children.Add(_vdirname,folderRoot.SchemaClassName);
               newVirDir.CommitChanges();
            }

            string fullPath = Path.GetFullPath(_dirpath);

            // Set Required Properties
            newVirDir.Properties["Path"].Value = fullPath;
            newVirDir.Properties["AppFriendlyName"].Value = _vdirname;
            newVirDir.Properties["AppRoot"].Value = _apppath + _vdirname;

            // Set Optional Properties
            newVirDir.Properties["AccessExecute"][0] = _accessexecute;
            newVirDir.Properties["AccessNoRemoteExecute"][0] = _accessnoremoteexecute;
            newVirDir.Properties["AccessNoRemoteRead"][0] = _accessnoremoteread;
            newVirDir.Properties["AccessNoRemoteScript"][0] = _accessnoremotescript;
            newVirDir.Properties["AccessNoRemoteWrite"][0] = _accessnoremotewrite;
            newVirDir.Properties["AccessRead"][0] = _accessread;
            newVirDir.Properties["AccessSource"][0] = _accesssource;
            newVirDir.Properties["AccessScript"][0] = _accessscript;
            newVirDir.Properties["AccessSSL"][0] = _accessssl;
            newVirDir.Properties["AccessSSL128"][0] = _accessssl128;
            newVirDir.Properties["AccessSSLMapCert"][0] = _accesssslmapcert;
            newVirDir.Properties["AccessSSLNegotiateCert"][0] = _accesssslnegotiatecert;
            newVirDir.Properties["AccessSSLRequireCert"][0] = _accesssslrequirecert;
            newVirDir.Properties["AccessWrite"][0] = _accesswrite;
            newVirDir.Properties["AnonymousPasswordSync"][0] = _anonymouspasswordsync;
            newVirDir.Properties["AppAllowClientDebug"][0] = _appallowclientdebug;
            newVirDir.Properties["AppAllowDebugging"][0] = _appallowdebugging;
            newVirDir.Properties["AspBufferingOn"][0] = _aspbufferingon;
            newVirDir.Properties["AspEnableApplicationRestart"][0] = _aspenableapplicationrestart;
            newVirDir.Properties["AspEnableAspHtmlFallback"][0] = _aspenableasphtmlfallback;
            newVirDir.Properties["AspEnableChunkedEncoding"][0] = _aspenablechunkedencoding;
            newVirDir.Properties["AspErrorsToNTLog"][0] = _asperrorstontlog;
            newVirDir.Properties["AspEnableParentPaths"][0] = _aspenableparentpaths;
            newVirDir.Properties["AspEnableTypelibCache"][0] = _aspenabletypelibcache;
            newVirDir.Properties["AspExceptionCatchEnable"][0] = _aspexceptioncatchenable;
            newVirDir.Properties["AspLogErrorRequests"][0] = _asplogerrorrequests;
            newVirDir.Properties["AspScriptErrorSentToBrowser"][0] = _aspscripterrorsenttobrowser;
            newVirDir.Properties["AspThreadGateEnabled"][0] = _aspthreadgateenabled;
            newVirDir.Properties["AspTrackThreadingModel"][0] = _asptrackthreadingmodel;
            newVirDir.Properties["AuthAnonymous"][0] = _authanonymous;
            newVirDir.Properties["AuthBasic"][0] = _authbasic;
            newVirDir.Properties["AuthNTLM"][0] = _authntlm;
            newVirDir.Properties["AuthPersistSingleRequest"][0] = _authpersistsinglerequest;
            newVirDir.Properties["AuthPersistSingleRequestIfProxy"][0] = _authpersistsinglerequestifproxy;
            newVirDir.Properties["AuthPersistSingleRequestAlwaysIfProxy"][0] = _authpersistsinglerequestalwaysifproxy;
            newVirDir.Properties["CacheControlNoCache"][0] = _cachecontrolnocache;
            newVirDir.Properties["CacheISAPI"][0] = _cacheisapi;
            newVirDir.Properties["ContentIndexed"][0] = _contentindexed;
            newVirDir.Properties["CpuAppEnabled"][0] = _cpuappenabled;
            newVirDir.Properties["CpuCgiEnabled"][0] = _cpucgienabled;
            newVirDir.Properties["CreateCGIWithNewConsole"][0] = _createcgiwithnewconsole;
            newVirDir.Properties["CreateProcessAsUser"][0] = _createprocessasuser;
            newVirDir.Properties["DirBrowseShowDate"][0] = _dirbrowseshowdate;
            newVirDir.Properties["DirBrowseShowExtension"][0] = _dirbrowseshowextension;
            newVirDir.Properties["DirBrowseShowLongDate"][0] = _dirbrowseshowlongdate;
            newVirDir.Properties["DirBrowseShowSize"][0] = _dirbrowseshowsize;
            newVirDir.Properties["DirBrowseShowTime"][0] = _dirbrowseshowtime;
            newVirDir.Properties["DontLog"][0] = _dontlog;
            newVirDir.Properties["EnableDefaultDoc"][0] = _enabledefaultdoc;
            newVirDir.Properties["EnableDirBrowsing"][0] = _enabledirbrowsing;
            newVirDir.Properties["EnableDocFooter"][0] = _enabledocfooter;
            newVirDir.Properties["EnableReverseDns"][0] = _enablereversedns;
            newVirDir.Properties["SSIExecDisable"][0] = _ssiexecdisable;
            newVirDir.Properties["UNCAuthenticationPassthrough"][0] = _uncauthenticationpassthrough;
            newVirDir.Properties["AspScriptErrorMessage"][0] = _aspscripterrormessage;
	    newVirDir.Properties["DefaultDoc"][0] = _defaultdoc;


            // Save Changes
            newVirDir.CommitChanges();
            folderRoot.CommitChanges();
            newVirDir.Close();
            folderRoot.Close();
         }
         catch (Exception e){
            throw new BuildException(this.GetType().ToString() + ": Error creating virtual directory, see build log for details.", Location, e);
         }
      }
   }


   /// <summary>
   /// A task to delete a specified IIS Virtual Directory in the Default Web Site.
   /// </summary>
   /// <example>
   ///   <para>Delete the TEMP IIS Virtual Directory.</para>
   ///   <code><![CDATA[
   ///      <deliisdir 
   ///         vdirname="TEMP"
   ///      />
   ///   ]]></code>
   /// </example>
   [TaskName("deliisdir")]
   public class DelIISDirTask : Task{

      private const string _iispath = "IIS://localhost/W3SVC/1/Root";
      string _vdirname = null;

      /// <summary>Name of the IIS Virtual Directory</summary>
      [TaskAttribute("vdirname", Required=true)]
      public string vdirname{
         get { return _vdirname; }
         set { _vdirname = value; }
      }
	
      protected override void ExecuteTask(){
         try{
	    DirectoryEntry folderRoot = new DirectoryEntry(_iispath);
            DirectoryEntries rootEntries = folderRoot.Children;
	    folderRoot.RefreshCache();
	    DirectoryEntry childVirDir = folderRoot.Children.Find(_vdirname,folderRoot.SchemaClassName);
	      
            rootEntries.Remove(childVirDir);
	  
	    childVirDir.Close();
            folderRoot.Close();
         }
         catch (Exception e){
            throw new BuildException(this.GetType().ToString() + ": Error deleting virtual directory, see build log for details.", Location, e);
         }
      }
   }

   /// <summary>
   /// A task to List the configuration settings of a specified IIS Virtual Directory in the Default Web Site.
   /// </summary>
   /// <example>
   ///   <para>List the settings of the TEMP IIS Virtual Directory.</para>
   ///   <code><![CDATA[
   ///      <iisdirinfo 
   ///         vdirname="TEMP"
   ///      />
   ///   ]]></code>
   /// </example>
   [TaskName("iisdirinfo")]
   public class IISDirInfoTask : Task{

      private const string _iispath = "IIS://localhost/W3SVC/1/Root";
      string _vdirname = null;

      /// <summary>Name of the IIS Virtual Directory</summary>
      [TaskAttribute("vdirname", Required=true)]
      public string vdirname{
         get { return _vdirname; }
         set { _vdirname = value; }
      }
	
      protected override void ExecuteTask(){
         try{
            DirectoryEntry folderRoot = new DirectoryEntry(_iispath);
            folderRoot.RefreshCache();
            DirectoryEntry newVirDir = folderRoot.Children.Find(_vdirname,folderRoot.SchemaClassName);

            Log.WriteLine("AccessExecute: " + newVirDir.Properties["AccessExecute"].Value.ToString());
            Log.WriteLine("AccessFlags: " + newVirDir.Properties["AccessFlags"].Value.ToString());
            Log.WriteLine("AccessNoRemoteExecute: " + newVirDir.Properties["AccessNoRemoteExecute"].Value.ToString());
            Log.WriteLine("AccessNoRemoteRead: " + newVirDir.Properties["AccessNoRemoteRead"].Value.ToString());
            Log.WriteLine("AccessNoRemoteScript: " + newVirDir.Properties["AccessNoRemoteScript"].Value.ToString());
            Log.WriteLine("AccessNoRemoteWrite: " + newVirDir.Properties["AccessNoRemoteWrite"].Value.ToString());
            Log.WriteLine("AccessRead: " + newVirDir.Properties["AccessRead"].Value.ToString());
            Log.WriteLine("AccessSource: " + newVirDir.Properties["AccessSource"].Value.ToString());
            Log.WriteLine("AccessScript: " + newVirDir.Properties["AccessScript"].Value.ToString());
            Log.WriteLine("AccessSSL: " + newVirDir.Properties["AccessSSL"].Value.ToString());
            Log.WriteLine("AccessSSL128: " + newVirDir.Properties["AccessSSL128"].Value.ToString());
            Log.WriteLine("AccessSSLFlags: " + newVirDir.Properties["AccessSSLFlags"].Value.ToString());
            Log.WriteLine("AccessSSLMapCert: " + newVirDir.Properties["AccessSSLMapCert"].Value.ToString());
            Log.WriteLine("AccessSSLNegotiateCert: " + newVirDir.Properties["AccessSSLNegotiateCert"].Value.ToString());
            Log.WriteLine("AccessSSLRequireCert: " + newVirDir.Properties["AccessSSLRequireCert"].Value.ToString());
            Log.WriteLine("AccessWrite: " + newVirDir.Properties["AccessWrite"].Value.ToString());
            Log.WriteLine("AnonymousPasswordSync: " + newVirDir.Properties["AnonymousPasswordSync"].Value.ToString());
            Log.WriteLine("AnonymousUserName: " + newVirDir.Properties["AnonymousUserName"].Value.ToString());
            Log.WriteLine("AnonymousUserPass: " + newVirDir.Properties["AnonymousUserPass"].Value.ToString());
            Log.WriteLine("AppAllowClientDebug: " + newVirDir.Properties["AppAllowClientDebug"].Value.ToString());
            Log.WriteLine("AppAllowDebugging: " + newVirDir.Properties["AppAllowDebugging"].Value.ToString());
            Log.WriteLine("AppFriendlyName: " + newVirDir.Properties["AppFriendlyName"].Value.ToString());
            Log.WriteLine("AppIsolated: " + newVirDir.Properties["AppIsolated"].Value.ToString());
            Log.WriteLine("AppOopRecoverLimit: " + newVirDir.Properties["AppOopRecoverLimit"].Value.ToString());
            Log.WriteLine("AppPackageID: " + newVirDir.Properties["AppPackageID"].Value.ToString());
            Log.WriteLine("AppPackageName: " + newVirDir.Properties["AppPackageName"].Value.ToString());
            Log.WriteLine("AppRoot: " + newVirDir.Properties["AppRoot"].Value.ToString());
            Log.WriteLine("AppWamClsID: " + newVirDir.Properties["AppWamClsID"].Value.ToString());
            Log.WriteLine("AspAllowOutOfProcComponents: " + newVirDir.Properties["AspAllowOutOfProcComponents"].Value.ToString());
            Log.WriteLine("AspAllowSessionState: " + newVirDir.Properties["AspAllowSessionState"].Value.ToString());
            Log.WriteLine("AspBufferingOn: " + newVirDir.Properties["AspBufferingOn"].Value.ToString());
            Log.WriteLine("AspCodepage: " + newVirDir.Properties["AspCodepage"].Value.ToString());
            Log.WriteLine("AspEnableApplicationRestart: " + newVirDir.Properties["AspEnableApplicationRestart"].Value.ToString());
            Log.WriteLine("AspEnableAspHtmlFallback: " + newVirDir.Properties["AspEnableAspHtmlFallback"].Value.ToString());
            Log.WriteLine("AspEnableChunkedEncoding: " + newVirDir.Properties["AspEnableChunkedEncoding"].Value.ToString());
            Log.WriteLine("AspEnableParentPaths: " + newVirDir.Properties["AspEnableParentPaths"].Value.ToString());
            Log.WriteLine("AspEnableTypelibCache: " + newVirDir.Properties["AspEnableTypelibCache"].Value.ToString());
            Log.WriteLine("AspErrorsToNTLog: " + newVirDir.Properties["AspErrorsToNTLog"].Value.ToString());
            Log.WriteLine("AspExceptionCatchEnable: " + newVirDir.Properties["AspExceptionCatchEnable"].Value.ToString());
            Log.WriteLine("AspLogErrorRequests: " + newVirDir.Properties["AspLogErrorRequests"].Value.ToString());
            Log.WriteLine("AspProcessorThreadMax: " + newVirDir.Properties["AspProcessorThreadMax"].Value.ToString());
            Log.WriteLine("AspQueueConnectionTestTime: " + newVirDir.Properties["AspQueueConnectionTestTime"].Value.ToString());
            Log.WriteLine("AspQueueTimeout: " + newVirDir.Properties["AspQueueTimeout"].Value.ToString());
            Log.WriteLine("AspRequestQueueMax: " + newVirDir.Properties["AspRequestQueueMax"].Value.ToString());
            Log.WriteLine("AspScriptEngineCacheMax: " + newVirDir.Properties["AspScriptEngineCacheMax"].Value.ToString());
            Log.WriteLine("AspScriptErrorMessage: " + newVirDir.Properties["AspScriptErrorMessage"].Value.ToString());
            Log.WriteLine("AspScriptErrorSentToBrowser: " + newVirDir.Properties["AspScriptErrorSentToBrowser"].Value.ToString());
            Log.WriteLine("AspScriptFileCacheSize: " + newVirDir.Properties["AspScriptFileCacheSize"].Value.ToString());
            Log.WriteLine("AspScriptLanguage: " + newVirDir.Properties["AspScriptLanguage"].Value.ToString());
            Log.WriteLine("AspScriptTimeout: " + newVirDir.Properties["AspScriptTimeout"].Value.ToString());
            Log.WriteLine("AspSessionMax: " + newVirDir.Properties["AspSessionMax"].Value.ToString());
            Log.WriteLine("AspSessionTimeout: " + newVirDir.Properties["AspSessionTimeout"].Value.ToString());
            Log.WriteLine("AspThreadGateEnabled: " + newVirDir.Properties["AspThreadGateEnabled"].Value.ToString());
            Log.WriteLine("AspThreadGateLoadHigh: " + newVirDir.Properties["AspThreadGateLoadHigh"].Value.ToString());
            Log.WriteLine("AspThreadGateLoadLow: " + newVirDir.Properties["AspThreadGateLoadLow"].Value.ToString());
            Log.WriteLine("AspThreadGateSleepDelay: " + newVirDir.Properties["AspThreadGateSleepDelay"].Value.ToString());
            Log.WriteLine("AspThreadGateSleepMax: " + newVirDir.Properties["AspThreadGateSleepMax"].Value.ToString());
            Log.WriteLine("AspThreadGateTimeSlice: " + newVirDir.Properties["AspThreadGateTimeSlice"].Value.ToString());
            Log.WriteLine("AspTrackThreadingModel: " + newVirDir.Properties["AspTrackThreadingModel"].Value.ToString());
            Log.WriteLine("AuthAnonymous: " + newVirDir.Properties["AuthAnonymous"].Value.ToString());
            Log.WriteLine("AuthBasic: " + newVirDir.Properties["AuthBasic"].Value.ToString());
            Log.WriteLine("AuthFlags: " + newVirDir.Properties["AuthFlags"].Value.ToString());
            Log.WriteLine("AuthNTLM: " + newVirDir.Properties["AuthNTLM"].Value.ToString());
            Log.WriteLine("AuthPersistence: " + newVirDir.Properties["AuthPersistence"].Value.ToString());
            Log.WriteLine("AuthPersistSingleRequest: " + newVirDir.Properties["AuthPersistSingleRequest"].Value.ToString());
            Log.WriteLine("AuthPersistSingleRequestIfProxy: " + newVirDir.Properties["AuthPersistSingleRequestIfProxy"].Value.ToString());
            Log.WriteLine("AuthPersistSingleRequestAlwaysIfProxy: " + newVirDir.Properties["AuthPersistSingleRequestAlwaysIfProxy"].Value.ToString());
            Log.WriteLine("CacheControlCustom: " + newVirDir.Properties["CacheControlCustom"].Value.ToString());
            Log.WriteLine("CacheControlMaxAge: " + newVirDir.Properties["CacheControlMaxAge"].Value.ToString());
            Log.WriteLine("CacheControlNoCache: " + newVirDir.Properties["CacheControlNoCache"].Value.ToString());
            Log.WriteLine("CacheISAPI: " + newVirDir.Properties["CacheISAPI"].Value.ToString());
            Log.WriteLine("ContentIndexed: " + newVirDir.Properties["ContentIndexed"].Value.ToString());
            Log.WriteLine("CpuAppEnabled: " + newVirDir.Properties["CpuAppEnabled"].Value.ToString());
            Log.WriteLine("CpuCgiEnabled: " + newVirDir.Properties["CpuCgiEnabled"].Value.ToString());
            Log.WriteLine("CreateCGIWithNewConsole: " + newVirDir.Properties["CreateCGIWithNewConsole"].Value.ToString());
            Log.WriteLine("CreateProcessAsUser: " + newVirDir.Properties["CreateProcessAsUser"].Value.ToString());
            Log.WriteLine("DefaultDoc: " + newVirDir.Properties["DefaultDoc"].Value.ToString());
            Log.WriteLine("DefaultDocFooter: " + newVirDir.Properties["DefaultDocFooter"].Value.ToString());
            Log.WriteLine("DefaultLogonDomain: " + newVirDir.Properties["DefaultLogonDomain"].Value.ToString());
            Log.WriteLine("DirBrowseFlags: " + newVirDir.Properties["DirBrowseFlags"].Value.ToString());
            Log.WriteLine("DirBrowseShowDate: " + newVirDir.Properties["DirBrowseShowDate"].Value.ToString());
            Log.WriteLine("DirBrowseShowExtension: " + newVirDir.Properties["DirBrowseShowExtension"].Value.ToString());
            Log.WriteLine("DirBrowseShowLongDate: " + newVirDir.Properties["DirBrowseShowLongDate"].Value.ToString());
            Log.WriteLine("DirBrowseShowSize: " + newVirDir.Properties["DirBrowseShowSize"].Value.ToString());
            Log.WriteLine("DirBrowseShowTime: " + newVirDir.Properties["DirBrowseShowTime"].Value.ToString());
            Log.WriteLine("DontLog: " + newVirDir.Properties["DontLog"].Value.ToString());
            Log.WriteLine("EnableDefaultDoc: " + newVirDir.Properties["EnableDefaultDoc"].Value.ToString());
            Log.WriteLine("EnableDirBrowsing: " + newVirDir.Properties["EnableDirBrowsing"].Value.ToString());
            Log.WriteLine("EnableDocFooter: " + newVirDir.Properties["EnableDocFooter"].Value.ToString());
            Log.WriteLine("EnableReverseDns: " + newVirDir.Properties["EnableReverseDns"].Value.ToString());
            //Log.WriteLine(newVirDir.Properties["HttpCustomHeaders"].Value.ToString());
            //Log.WriteLine("HttpErrors: " + newVirDir.Properties["HttpErrors"].Value.ToSring());
            Log.WriteLine("HttpExpires: " + newVirDir.Properties["HttpExpires"].Value.ToString());
            //Log.WriteLine(newVirDir.Properties["HttpPics"].Value.ToString());
            Log.WriteLine("HttpRedirect: " + newVirDir.Properties["HttpRedirect"].Value.ToString());
            //Log.WriteLine(newVirDir.Properties["IPSecurity"].Value.ToString());
            Log.WriteLine("LogonMethod: " + newVirDir.Properties["LogonMethod"].Value.ToString());
            //Log.WriteLine(newVirDir.Properties["MimeMap"].Value.ToString());
            Log.WriteLine("Path: " + newVirDir.Properties["Path"].Value.ToString());
            Log.WriteLine("PoolIDCTimeout: " + newVirDir.Properties["PoolIDCTimeout"].Value.ToString());
            Log.WriteLine("PutReadSize: " + newVirDir.Properties["PutReadSize"].Value.ToString());
            Log.WriteLine("Realm: " + newVirDir.Properties["Realm"].Value.ToString());
            Log.WriteLine("RedirectHeaders: " + newVirDir.Properties["RedirectHeaders"].Value.ToString());
            //Log.WriteLine("ScriptMaps: " + newVirDir.Properties["ScriptMaps"].Value.ToString());
            Log.WriteLine("SSIExecDisable: " + newVirDir.Properties["SSIExecDisable"].Value.ToString());
            Log.WriteLine("UNCAuthenticationPassthrough: " + newVirDir.Properties["UNCAuthenticationPassthrough"].Value.ToString());
            Log.WriteLine("UNCPassword: " + newVirDir.Properties["UNCPassword"].Value.ToString());
            Log.WriteLine("UNCUserName: " + newVirDir.Properties["UNCUserName"].Value.ToString());
            Log.WriteLine("UploadReadAheadSize: " + newVirDir.Properties["UploadReadAheadSize"].Value.ToString());

            newVirDir.Close();
            folderRoot.Close();
         }
         catch (Exception e){
            throw new BuildException(this.GetType().ToString() + ": Error retrieving virtual directory info, see build log for details.", Location, e);
         }
      }
   }
}
