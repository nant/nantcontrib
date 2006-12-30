//
// NAntContrib
// Copyright (C) 2001-2005 Gerry Shaw
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

// Tim Armbruster http://www.tfarmbruster.com

using System.Runtime.InteropServices;

namespace NAnt.Contrib.Util {
    public class IniFile {
        # region API functions
        // API functions
        [ DllImport("Kernel32.dll", EntryPoint = "GetPrivateProfileStringA",
              CharSet=System.Runtime.InteropServices.CharSet.Ansi, SetLastError=true) ]
        private static extern int GetPrivateProfileString(
            string lpApplicationName, 
            string lpKeyName, 
            string lpDefault, 
            System.Text.StringBuilder lpReturnedString, 
            int nSize, 
            string lpFileName);

        [ DllImport("Kernel32.dll", EntryPoint = "WritePrivateProfileStringA",
              CharSet=System.Runtime.InteropServices.CharSet.Ansi, SetLastError=true) ]
        private static extern int WritePrivateProfileString(
            string lpApplicationName, 
            string lpKeyName, 
            string lpString, 
            string lpFileName);

        #endregion

        private string strFilename;

        public IniFile(string FileName) {
            strFilename = FileName;
        }

        public string FileName {
            get { return (strFilename); }
        }

        /// <summary>
        /// Returns a string from your INI file
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        public string GetString(string Section, 
            string Key, string Default) {
            int intCharCount;
            System.Text.StringBuilder objResult = new System.Text.StringBuilder(256);
            intCharCount = GetPrivateProfileString(Section, Key, 
                Default, objResult, objResult.Capacity, strFilename);
            if (intCharCount > 0) {
                return objResult.ToString();
            } 
            else {
                return string.Empty;
            }
        }

        /// <summary>
        /// ' Writes a string to your INI file
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="String"></param>
        public void WriteString(string Section, 
            string Key, string String) {
            WritePrivateProfileString(Section, Key, String, strFilename);
        }
    }
}
