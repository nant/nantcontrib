//
// NAntContrib
//
// Copyright (C) 2004 Kraen Munck (kmc@innomate.com)
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
// Based on original work by Jayme C. Edwards (jcedwards@users.sourceforge.net)
//

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NAnt.Contrib.Tasks.Msi {
    /// <summary>
    /// Maintains a forward reference to a .tlb file
    /// in the same directory as an assembly .dll
    /// that has been registered for COM interop.
    /// </summary>
    internal class TypeLibRecord {
        private AssemblyName assemblyName;
        private string libId, typeLibFileName,
            featureName, assemblyComponent;

        /// <summary>
        /// Creates a new <see cref="TypeLibRecord"/>.
        /// </summary>
        /// <param name="LibId">The typelibrary id.</param>
        /// <param name="TypeLibFileName">The typelibrary filename.</param>
        /// <param name="AssemblyName">The name of the assembly.</param>
        /// <param name="FeatureName">The feature containing the typelibrary's file.</param>
        /// <param name="AssemblyComponent">The name of the Assembly's component.</param>
        public TypeLibRecord(
            string LibId, string TypeLibFileName,
            AssemblyName AssemblyName, string FeatureName,
            string AssemblyComponent) {
            libId = LibId;
            typeLibFileName = TypeLibFileName;
            assemblyName = AssemblyName;
            featureName = FeatureName;
            assemblyComponent = AssemblyComponent;
        }

        /// <summary>
        /// Retrieves the name of the Assembly's component.
        /// </summary>
        /// <value>The Assembly's component Name.</value>
        public string AssemblyComponent {
            get { return assemblyComponent; }
        }

        /// <summary>
        /// Retrieves the typelibrary filename.
        /// </summary>
        /// <value>The typelibrary filename.</value>
        public string TypeLibFileName {
            get { return typeLibFileName; }
        }

        /// <summary>
        /// Retrieves the typelibrary id.
        /// </summary>
        /// <value>The typelibrary id.</value>
        public string LibId {
            get { return libId; }
        }

        /// <summary>
        /// Retrieves the name of the assembly.
        /// </summary>
        /// <value>The name of the assembly.</value>
        public AssemblyName AssemblyName {
            get { return assemblyName; }
        }

        /// <summary>
        /// Retrieves the feature containing the typelibrary's file.
        /// </summary>
        /// <value>The feature containing the typelibrary's file.</value>
        public string FeatureName {
            get { return featureName; }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSIRowColumnData {
        public string name;
        public int id;
        public string type;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct CUSTDATAITEM {
        public Guid guid;
        public object varValue;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CUSTDATA {
        public int cCustData;
        public CUSTDATAITEM[] prgCustData;
    }

    [ComImport]
    [Guid("00020412-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface UCOMITypeInfo2 {
        #region Implementation of UCOMITypeInfo
        void GetContainingTypeLib(out System.Runtime.InteropServices.UCOMITypeLib ppTLB, out int pIndex);
        void GetIDsOfNames(string[] rgszNames, int cNames, int[] pMemId);
        void GetRefTypeInfo(int hRef, out System.Runtime.InteropServices.UCOMITypeInfo ppTI);
        void GetMops(int memid, out string pBstrMops);
        void ReleaseVarDesc(System.IntPtr pVarDesc);
        void ReleaseTypeAttr(System.IntPtr pTypeAttr);
        void GetDllEntry(int memid, System.Runtime.InteropServices.INVOKEKIND invKind, out string pBstrDllName, out string pBstrName, out short pwOrdinal);
        void GetRefTypeOfImplType(int index, out int href);
        void GetTypeComp(out System.Runtime.InteropServices.UCOMITypeComp ppTComp);
        void GetTypeAttr(out System.IntPtr ppTypeAttr);
        void GetDocumentation(int index, out string strName, out string strDocString, out int dwHelpContext, out string strHelpFile);
        void AddressOfMember(int memid, System.Runtime.InteropServices.INVOKEKIND invKind, out System.IntPtr ppv);
        void GetNames(int memid, string[] rgBstrNames, int cMaxNames, out int pcNames);
        void CreateInstance(object pUnkOuter, ref System.Guid riid, out object ppvObj);
        void Invoke(object pvInstance, int memid, short wFlags, ref System.Runtime.InteropServices.DISPPARAMS pDispParams, out object pVarResult, out System.Runtime.InteropServices.EXCEPINFO pExcepInfo, out int puArgErr);
        void GetVarDesc(int index, out System.IntPtr ppVarDesc);
        void ReleaseFuncDesc(System.IntPtr pFuncDesc);
        void GetFuncDesc(int index, out System.IntPtr ppFuncDesc);
        void GetImplTypeFlags(int index, out int pImplTypeFlags);
        #endregion

        void GetTypeKind([Out] out TYPEKIND pTypeKind);
        void GetTypeFlags([Out] out int pTypeFlags);
        void GetFuncIndexOfMemId(int memid, INVOKEKIND invKind, [Out] out int pFuncIndex);
        void GetVarIndexOfMemId(int memid, [Out] out int pVarIndex);
        void GetCustData([In] ref Guid guid, [Out] out object pCustData);
        void GetFuncCustData(int index, [In] ref Guid guid, [Out] out object pVarVal);
        void GetParamCustData(int indexFunc, int indexParam, [In] ref Guid guid, [Out] out object pVarVal);
        void GetVarCustData(int index, [In] ref Guid guid, [Out] out object pVarVal);
        void GetImplTypeCustData(int index, [In] ref Guid guid, [Out] out object pVarVal);
        void GetDocumentation2(int memid, int lcid, [Out] out string pbstrHelpString, [Out] out int pdwHelpStringContext, [Out] out string pbstrHelpStringDll);
        void GetAllCustData([In,Out] ref IntPtr pCustData);
        void GetAllFuncCustData(int index, [Out] out CUSTDATA pCustData);
        void GetAllParamCustData(int indexFunc, int indexParam, [Out] out CUSTDATA pCustData);
        void GetAllVarCustData(int index, [Out] out CUSTDATA pCustData);
        void GetAllImplTypeCustData(int index, [Out] out CUSTDATA pCustData);
    }
}
