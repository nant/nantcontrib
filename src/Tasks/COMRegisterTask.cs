// NAnt - A .NET build tool
// Copyright (C) 2001-2002 Gerry Shaw
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
// Ian MacLean (ian@maclean.ms)

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using SYSKIND = System.Runtime.InteropServices.ComTypes.SYSKIND;
using TYPELIBATTR = System.Runtime.InteropServices.ComTypes.TYPELIBATTR;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Register COM servers or type libraries.
    /// </summary>
    /// <remarks>
    /// <para>
    /// COM register task will try and register any type of COM related file 
    /// that needs registering.
    /// </para>
    /// <para>Executable files (.exe) will be registered as exe servers, type 
    /// libaries (.tlb) registered with RegisterTypeLib and for all other 
    /// filetypes it will attempt to register them as dll servers.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>Register a single dll server.</para>
    ///   <code>
    ///     <![CDATA[
    /// <comregister file="myComServer.dll" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>Register a single exe server </para>
    ///   <code>
    ///     <![CDATA[
    /// <comregister file="myComServer.exe" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>Register a set of COM files at once.</para>
    ///   <code>
    ///     <![CDATA[
    /// <comregister unregister="false">
    ///     <fileset>
    ///         <include name="an_ExeServer.exe" />
    ///         <include name="a_TypeLibrary.tlb" />
    ///         <include name="a_DllServer.dll" />
    ///         <include name="an_OcxServer.ocx" />
    ///     </fileset>
    /// </comregister>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("comregister")]
    public class COMRegisterTask : Task {
        //-----------------------------------------------------------------------------
        // Typelib Imports
        //-----------------------------------------------------------------------------
        [DllImport("oleaut32.dll", EntryPoint="LoadTypeLib", CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        private static extern int LoadTypeLib(string filename, ref IntPtr pTypeLib);

        [DllImport("oleaut32.dll", EntryPoint="RegisterTypeLib", CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        private static extern int RegisterTypeLib(IntPtr pTypeLib, string fullpath, string helpdir);

        [DllImport("oleaut32.dll", EntryPoint="UnRegisterTypeLib", CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        private static extern int UnRegisterTypeLib (
            ref Guid libID, 
            short wVerMajor,
            short wVerMinor, 
            int lCID, 
            SYSKIND tSysKind);


        //-----------------------------------------------------------------------------
        // Kernel 32 imports
        //-----------------------------------------------------------------------------
        [DllImport("kernel32.dll", SetLastError=true)]
        private static extern uint SetErrorMode(uint uMode);
 
        [DllImport("Kernel32.dll", EntryPoint="LoadLibrary", CharSet=System.Runtime.InteropServices.CharSet.Unicode, SetLastError=true)]
        private static extern IntPtr LoadLibrary(string fullpath);
        
        private const uint SEM_FAILCRITICALERRORS = 0x0001;
        private const uint SEM_NOGPFAULTERRORBOX = 0x0002;
        private const uint SEM_NOOPENFILEERRORBOX = 0x8000;
        
        [DllImport("Kernel32.dll", SetLastError=true)]
        private static extern int FreeLibrary(IntPtr hModule);
        
        [DllImport("Kernel32.dll", SetLastError=true)]
        private static extern IntPtr GetProcAddress(IntPtr handle, string lpprocname);
        
        [DllImport("Kernel32.dll")]
        public static extern int FormatMessage(int flags, IntPtr source, 
            int messageId, int languageId, StringBuilder buffer, int size, 
            IntPtr arguments);

        private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;

        private const int WIN32ERROR_ProcNotFound = 127;
        private const int WIN32ERROR_FileNotFound = 2;

        private FileInfo _file; 
        private bool _unregister;
        private FileSet _fileset = new FileSet();
        
        /// <summary>
        /// The name of the file to register. This is provided as an alternate 
        /// to using the task's fileset.
        /// </summary>
        [TaskAttribute("file")]
        public FileInfo File {
            get { return _file; }
            set { _file = value; }
        }

        /// <summary>Unregistering this time. ( /u paramater )Default is "false".</summary>
        [TaskAttribute("unregister")]
        [BooleanValidator()]
        public bool Unregister {
            get { return _unregister; }
            set { _unregister = value; }
        }

        /// <summary>
        /// The set of files to register.
        /// </summary>
        [BuildElement("fileset")]
        public FileSet COMRegisterFileSet {
            get { return _fileset; } 
            set { _fileset = value; }
        }

        #region Override implementation of Task

        protected override void ExecuteTask() {
            // add the filename to the file set
            if (File != null) {
                COMRegisterFileSet.Includes.Add(File.FullName);
            }

            // gather the information needed to perform the operation
            StringCollection fileNames = COMRegisterFileSet.FileNames;
                      
            // display build log message
            if (Unregister) {
                Log(Level.Info, "Unregistering {0} files", fileNames.Count);
            } else {
                Log(Level.Info, "Registering {0} files", fileNames.Count);
            }

            // perform operation
            foreach (string path in fileNames) {
                if (!System.IO.File.Exists(path)) {
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                        "File '{0}' does not exist", path), Location);
                }

                // store current directory
                string originalCurrentDirectory = Directory.GetCurrentDirectory();

                try {
                    // change current directory to directory containing
                    // the file to register
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(path));
                   
                    Log(Level.Verbose, "Registering '{0}'.", path);

                    switch(Path.GetExtension(path).ToLower(CultureInfo.InvariantCulture)) {
                        case ".tlb":
                            RegisterTypelib(path);
                            break;
                        case ".exe":
                            RegisterExeServer(path);
                            break;
                        case ".dll":
                        case ".ocx" :
                            RegisterDllServer(path);
                            break;
                        default:
                            RegisterDllServer(path);
                            break;
                    }
                } finally {
                    // restore original current directory
                    Directory.SetCurrentDirectory(originalCurrentDirectory);
                }
            }
        }

        #endregion Override implementation of Task

        #region Private Instance Methods

        /// <summary>
        /// Register an inproc COM server, usually a .dll or .ocx
        /// </summary>
        /// <param name="path"></param>
        private void RegisterDllServer(string path){
            IntPtr handle = new IntPtr();
            
            // set the error mode to prevent failure message boxes from being displayed. 
            uint oldErrorMode = SetErrorMode(SEM_NOOPENFILEERRORBOX | SEM_FAILCRITICALERRORS );
            
            handle = LoadLibrary(path);
            int error = Marshal.GetLastWin32Error();
            if (handle.ToInt32() == 0 && error != 0){
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Error loading dll '{0}'.", path), Location);
            }
            
            string entryPoint = "DllRegisterServer";
            string action = "register";
            if (Unregister) {
                entryPoint = "DllUnregisterServer";
                action = "unregister";
            }
            IntPtr address = GetProcAddress(handle, entryPoint);
            error = Marshal.GetLastWin32Error();
            
            if (address.ToInt32() == 0 && error != 0){
                string message = string.Format(CultureInfo.InvariantCulture,
                    "Error {0}ing dll. '{1}' has no {2} function.", action,
                    path, entryPoint);
                FreeLibrary(handle);
                throw new BuildException(message, Location);
            }
            try {
                // Do the actual registration here
                DynamicPInvoke.DynamicDllFuncInvoke(path, entryPoint);
            } catch (TargetInvocationException ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Error while {0}ing '{1}'", action, path), Location, 
                    ex.InnerException);
            } finally {
                if (handle.ToInt32() != 0) {
                    // need to call FreeLibrary a second time as the DynPInvoke added an extra ref
                    FreeLibrary(handle);
                }
            }
            // unload the library
            FreeLibrary(handle);
            SetErrorMode(oldErrorMode);
        }
        

        /// <summary>
        /// Register a COM type library
        /// </summary>
        /// <param name="path"></param>
        private void RegisterTypelib(string path) {
            IntPtr Typelib = new IntPtr(0);
            int error = 0;

            // Load typelib
            int result = LoadTypeLib(path, ref Typelib);
            error = Marshal.GetLastWin32Error();

            if (error != 0 || result != 0) {
                int win32error = (error != 0) ? error : result;
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Error loading typelib '{0}' ({1}: {2}).", path, win32error,
                    GetWin32ErrorMessage(win32error)), Location);
            }

            try {
                if (Unregister) {
#if NET_2_0
                    ITypeLib typeLib = null;
#else
                    UCOMITypeLib typeLib = null;
#endif

                    try {
#if NET_2_0
                        typeLib = (ITypeLib) Marshal.GetTypedObjectForIUnknown(
                            Typelib, typeof(ITypeLib));

#else
                        typeLib = (UCOMITypeLib) Marshal.GetTypedObjectForIUnknown(
                            Typelib, typeof(UCOMITypeLib));
#endif
                        // check for for win32 error
                        error = Marshal.GetLastWin32Error();
                        if (error != 0) {
                            throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                                "Error retrieving information from typelib '{0}' ({1}: {2}).", 
                                path, error, GetWin32ErrorMessage(error)), Location);
                        }

                        IntPtr libAttrPtr = new IntPtr(0);
                        typeLib.GetLibAttr(out libAttrPtr);
                        TYPELIBATTR typeLibAttr = (TYPELIBATTR) 
                            Marshal.PtrToStructure(libAttrPtr, typeof(TYPELIBATTR));

                        // unregister type library
                        UnRegisterTypeLib(ref typeLibAttr.guid, typeLibAttr.wMajorVerNum, 
                            typeLibAttr.wMinorVerNum, typeLibAttr.lcid, typeLibAttr.syskind);
                        // check for for win32 error
                        error = Marshal.GetLastWin32Error();
                        // release the TYPELIBATTR
                        typeLib.ReleaseTLibAttr(libAttrPtr);
                        if (error != 0) {
                            // signal error
                            throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                                "Typelib '{0}' could not be unregistered ({1}: {2}).", 
                                path, error, GetWin32ErrorMessage(error)), Location);
                        }
                    } finally {
                        if (typeLib != null) {
                            Marshal.ReleaseComObject(typeLib);
                        }
                    }
                } else {
                    //Perform registration
                    RegisterTypeLib(Typelib, path, null);
                    error = Marshal.GetLastWin32Error();

                    if (error != 0) {
                        throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                            "Error registering typelib '{0}' ({1}: {2}).", path, 
                            error, GetWin32ErrorMessage(error)), Location);
                    }
                }
            } finally {
                Marshal.Release(Typelib);
            }
        } 

        /// <summary>
        /// Register exe servers.
        /// </summary>
        /// <param name="path"></param>
        private void RegisterExeServer(string path){
            // Create process with the /regserver flag
            Process process = new Process();

            process.StartInfo.FileName = path;
            if (this.Unregister) {
                process.StartInfo.Arguments = path + " /unregserver";
            } else {
                process.StartInfo.Arguments = path + " /regserver";
            }
            
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(path);
            
            try {
                process.Start();
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Error registering '{0}'.", path), Location, ex);
            }
            
            bool exited = process.WaitForExit(5000);
            
            // kill if it doesn't terminate after 5s
            if (!exited || !process.HasExited){
                process.Kill();
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "'{0}' is not a COM server.", path), Location);
            }
            
            // check for error output. COM exe servers should not ouput to stdio on register
            StreamReader stdErr = process.StandardError;
            string errors = stdErr.ReadToEnd();
            if (errors.Length > 0) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "'{0}' doesn't support the /regserver option.", path), Location);
            }

            StreamReader stdOut = process.StandardOutput;
            string output = stdOut.ReadToEnd();
            if (output.Length > 0) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "'{0}' doesn't support the /regserver option.", path), Location);
            }
        }

        #endregion Private Instance Methods

        #region Private Static Methods

        private static string GetWin32ErrorMessage(int error) {
            StringBuilder sb = new StringBuilder(1024);

            int retVal = FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM, IntPtr.Zero, 
                error, 0, sb, sb.Capacity, IntPtr.Zero);
            if (retVal != 0) {
                string message = sb.ToString();
                return message.TrimEnd('\0', '\n', '\r');
            } else {
                return string.Empty;
            }
        }

        #endregion Private Static Methods

        /// <summary>
        /// Helper class to synamically build an assembly with the correct 
        /// P/Invoke signature
        /// </summary>
        private class DynamicPInvoke {
            /// <summary>
            /// Register a given dll.
            /// </summary>
            /// <param name="dll"></param>
            /// <param name="entrypoint"></param>
            /// <returns></returns>
            public static object DynamicDllFuncInvoke(string dll, string entrypoint) {
                Type returnType = typeof(int);
                Type [] parameterTypes = null;
                object[] parameterValues = null;
                string entryPoint = entrypoint;
                
                // Create a dynamic assembly and a dynamic module
                AssemblyName asmName = new AssemblyName();
                asmName.Name = "dllRegAssembly";
                AssemblyBuilder dynamicAsm = 
                AppDomain.CurrentDomain.DefineDynamicAssembly(asmName,
                    AssemblyBuilderAccess.Run);
                ModuleBuilder dynamicMod = dynamicAsm.DefineDynamicModule(
                    "tempModule");

                // Dynamically construct a global PInvoke signature 
                // using the input information
                dynamicMod.DefinePInvokeMethod(entryPoint, dll, 
                    MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.PinvokeImpl,
                    CallingConventions.Standard, returnType, parameterTypes, 
                    CallingConvention.Winapi, CharSet.Ansi);

                // This global method is now complete
                dynamicMod.CreateGlobalFunctions();

                // Get a MethodInfo for the PInvoke method
                MethodInfo mi = dynamicMod.GetMethod(entryPoint);

                // Invoke the static method and return whatever it returns
                return mi.Invoke(null, parameterValues);
            }
        }
    }
}
