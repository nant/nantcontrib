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
// Jeff Hemry ( ?? )

using System;
using System.IO;

using NAnt.Core;

namespace NAnt.Contrib.Tasks.Perforce {
    /// <summary>
    /// Static helper class for Perforce tasks
    /// </summary>
    public class Perforce {
        public static string GetUserName() {
            return GetP4Info("User name:");
        }

        public static string GetClient() {
            return GetP4Info("Client name:");
        }

        public static string GetChangelistNumber(string User, string Client, string Changelist, bool CreateIfMissing) {
            string result = GetChangelistNumber(User,Client,Changelist);
            if ( result == null && CreateIfMissing) {
                result = CreateChangelist(User,Client,Changelist);
            }
            return result;
        }
        public static string GetChangelistNumber(string User, string Client, string ChangeList) {
            string result = null;
            string CurrentChangelists = getProcessOutput("p4", string.Format("-u {0} -c {1} changes -s pending -u {0}", User, Client ), null );
            string[] lines = CurrentChangelists.Split( '\n' );
            foreach( string line in lines ) {
                if ( line.IndexOf( ChangeList ) > -1 ) {
                    string[] s2 = line.Split( ' ' );   // poor manz regex
                    result = s2[ 1 ];
                    break;
                }
            }
            return result;
        }
        private static string CreateChangelist(string User, string Client, string ChangeList) {
            // create new changelist
            string changeListDef = string.Concat(
                "Change: new\n",
                "Client: ", Client, "\n", 
                "User: ", User, "\n", 
                "Status: new\nDescription:\n ",
                ChangeList + "\nFiles:\n\n" );
            
            string output = getProcessOutput( "p4", string.Format("-u {0} -c {1} change -i" , User, Client) , changeListDef );

            string[] s = output.Split( ' ' );   // poor manz regex
            if ( ( s.Length == 3) && ( s[0] == "Change" ) && ( s[2].StartsWith( "created." ) ) ) {
                return s[ 1 ];
            } else {
                throw new BuildException( string.Format( "Unexpected P4 output = {0}", output));
            }
        }

        private static string GetP4Info(string SearchPattern) {
            string result = null;
            string output = getProcessOutput("p4","info",null);    
            string[] lines = output.Split('\n' );
            foreach( string line in lines ) {
                if ( line.IndexOf( SearchPattern ) > -1 ) {
                    string[] s2 = line.Split( ' ' );   // poor manz regex
                    if(s2.Length > 2) {
                        result = s2[2].Trim('\r');
                    }
                    break;
                }
            }
            return result;
        }

        public static string getProcessOutput( string exe, string prms, string input ) {
            string output = null;
            int exitCode = RunProcess( exe, prms, input, ref output );
            return output;
        }
        public static int RunProcess( string exe, string prms, string input, ref string output ) {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo si = new System.Diagnostics.ProcessStartInfo( exe, prms );
            si.UseShellExecute = false;
            si.RedirectStandardOutput = true;
            si.RedirectStandardInput = ( input != null );
            p.StartInfo = si;
            p.Start();
            if ( input != null ) 
            {
                StreamWriter sw = p.StandardInput;
                sw.Write( input );
                sw.Close();
            }          
            output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return p.ExitCode;
        }
        public static int RunProcess( string exe, string prms, string input ) {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo si = new System.Diagnostics.ProcessStartInfo( exe, prms );
            si.UseShellExecute = false;
            si.RedirectStandardOutput = false;
            si.RedirectStandardInput = ( input != null );
            p.StartInfo = si;
            p.Start();
            if ( input != null ) 
            {
                StreamWriter sw = p.StandardInput;
                sw.Write( input );
                sw.Close();
            }          
            p.WaitForExit();
            return p.ExitCode;
        }
    }
}
