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
using System.Globalization;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace NAnt.Contrib.Util { 
    /// <summary>
    /// Helper class to calculate checksums
    /// of files.
    /// </summary>
    internal class ChecksumHelper {
        private HashAlgorithm _provider;

        /// <summary>
        /// Create a new instance
        /// </summary>
        /// <param name="providerName">Name of hash algorithm to use</param>
        /// <exception cref="ArgumentException">The specified hash algorithm does not exist.</exception>
        public ChecksumHelper(string providerName) {
            _provider = HashAlgorithm.Create(providerName);
            if (_provider == null)
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "Hash algorithm '{0}' does not exist.", providerName));
        }

        /// <summary>
        /// Calculates a checksum for a given file
        /// and returns it in a hex string
        /// </summary>
        /// <param name="filename">name of the input file</param>
        /// <returns>hex checksum string</returns>
        public string CalculateChecksum(string filename) {
            byte[] checksum;

            using (FileStream file = File.OpenRead(filename)) {
                checksum = _provider.ComputeHash(file);
            }

            return ChecksumToString(checksum);
        }

        /// <summary>
        /// Converts a checksum value (a byte array)
        /// into a Hex-formatted string.
        /// </summary>
        /// <param name="checksum">Checksum value to convert</param>
        /// <returns>Hexified string value</returns>
        public string ChecksumToString(byte[] checksum) {
            StringBuilder str = new StringBuilder("");
            for ( int i=0; i < checksum.Length; i++ ) {
                str.Append(string.Format("{0:x2}", checksum[i]));
            }
            return str.ToString();
        }
    }
}
