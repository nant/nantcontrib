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
// John Lam (jlam@iunknown.com)

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks 
{
    /// <summary>
    /// Increments a four-part version number stored in a text file. The resulting 
    /// version number is written back to the file and exposed using NAnt properties.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   The version number format in the text file is 
    ///   Major.Minor.Build.Revision, e.g. 1.0.5.25.
    ///   </para>
    ///   <list type="table">
    ///     <item>
    ///       <term>Major</term>
    ///       <description>Set in file.</description>
    ///     </item>
    ///     <item>
    ///       <term>Minor</term>
    ///       <description>Set in file.</description>
    ///     </item>
    ///     <item>
    ///       <term>Build</term>
    ///       <description>Can be incremented by setting the <see cref="BuildType" /> parameter.</description>
    ///     </item>
    ///     <item>
    ///       <term>Revision</term>
    ///       <description>Can be incremented by setting the <see cref="RevisionType" /> parameter.</description>
    ///     </item>
    ///   </list>
    ///   <para>The following NAnt properties are created:</para>
    ///   <list type="table"> 
    ///     <item>
    ///       <term><c>prefix</c>.version</term>
    ///       <description>The complete version number, i.e. Major.Minor.Build.Revision</description>
    ///     </item>
    ///     <item>
    ///       <term><c>prefix</c>.major</term>
    ///       <description>The major component of the version number.</description>
    ///     </item>
    ///     <item>
    ///       <term><c>prefix</c>.minor</term>
    ///       <description>The minor component of the version number.</description>
    ///     </item>
    ///     <item>
    ///       <term><c>prefix</c>.build</term>
    ///       <description>The build component of the version number.</description>
    ///     </item>
    ///     <item>
    ///       <term><c>prefix</c>.revision</term>
    ///       <description>The revision component of the version number.</description>
    ///     </item>
    ///   </list>
    /// </remarks>
    [TaskName("version")]
    public class VersionTask : Task {
        /// <summary>
        /// Defines possible algorithms to generate the build number.
        /// </summary>
        public enum BuildNumberAlgorithm {
            /// <summary>
            /// Use the number of months since start of project * 100 + current 
            /// day in month as build number.
            /// </summary>
            MonthDay,

            /// <summary>
            /// Increment an existing build number.
            /// </summary>
            Increment,

            /// <summary>
            /// Use an existing build number (and do not increment it).
            /// </summary>
            NoIncrement
        }

        /// <summary>
        /// Defines possible algorithms to generate the revision number.
        /// </summary>
        public enum RevisionNumberAlgorithm {
            /// <summary>
            /// Use the number of seconds since the start of today / 10.
            /// </summary>
            Automatic,

            /// <summary>
            /// Increment an existing revision number.
            /// </summary>
            Increment
        }

        #region Private Instance Fields

        private string _prefix = "buildnumber";
        private BuildNumberAlgorithm _buildType = BuildNumberAlgorithm.MonthDay;
        private RevisionNumberAlgorithm _revisionType = RevisionNumberAlgorithm.Automatic;
        private FileInfo _path;
        private DateTime _startDate;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The string to prefix the properties with. The default is 
        /// <c>'buildnumber.'</c>.
        /// </summary>
        [TaskAttribute("prefix")]
        [StringValidator(AllowEmpty=false)]
        public string Prefix {
            get { return _prefix; }
            set { _prefix = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Start of project. Date from which to calculate build number. 
        /// Required if <see cref="BuildNumberAlgorithm.MonthDay" /> is used as 
        /// <see cref="BuildType" />.
        /// </summary>
        [TaskAttribute("startdate")]
        public DateTime StartDate {
            get { return _startDate; }
            set { _startDate = value; }
        }

        /// <summary>
        /// Path to the file containing the current version number. The default 
        /// file is <c>'build.number'</c> in the project base directory.
        /// </summary>
        [TaskAttribute("path")]
        public FileInfo Path {
            get { 
                if (_path == null) {
                    _path = new FileInfo(Project.GetFullPath("build.number"));
                }
                return _path;
            }
            set { _path = value; }
        }

        /// <summary>
        /// The algorithm for generating build number. The default is
        /// <see cref="BuildNumberAlgorithm.MonthDay" />.
        /// </summary>
        [TaskAttribute("buildtype")]
        public BuildNumberAlgorithm BuildType {
            get { return _buildType; }
            set { _buildType = value; }
        }

        /// <summary>
        /// The algorithm for generating revision number. The default is
        /// <see cref="RevisionNumberAlgorithm.Automatic" />.
        /// </summary>
        [TaskAttribute("revisiontype")]
        public RevisionNumberAlgorithm RevisionType {
            get { return _revisionType; }
            set { _revisionType = value; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        protected override void ExecuteTask()  {
            Version version = CalculateVersion();

            Project.Properties[Prefix + ".version"] = version.ToString();
            Project.Properties[Prefix + ".major"] = version.Major.ToString();
            Project.Properties[Prefix + ".minor"] = version.Minor.ToString();
            Project.Properties[Prefix + ".build"] = version.Build.ToString();
            Project.Properties[Prefix + ".revision"] = version.Revision.ToString();

            // write version back to file
            WriteVersionToFile(version);

            Log(Level.Info, "Build number '{0}'.", version.ToString());
        }

        #endregion Override implementation of Task

        #region Private Instance Methods

        /// <summary>
        /// Reads a version string from <see cref="Path" /> and returns it as a
        /// <see cref="Version" /> instance.
        /// </summary>
        /// <returns>
        /// A <see cref="Version" /> instance representing the version string in
        /// <see cref="Path" />.
        /// </returns>
        private Version ReadVersionFromFile() {
            string version = null;

            // read the version string
            try {
                using (StreamReader reader = new StreamReader(Path.FullName)) {
                    version = reader.ReadToEnd();
                }
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Unable to read version number from \"{0}\".", Path.FullName), 
                    Location, ex);
            }

            // instantiate a Version instance from the version string
            try {
                return new Version(version);
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Invalid version string \"{0}\" in file \"{1}\".", version,
                    Path), Location, ex);
            }
        }

        /// <summary>
        /// Writes the specified version to <see cref="Path" />.
        /// </summary>
        /// <param name="version">The version to write to <see cref="Path" />.</param>
        private void WriteVersionToFile(Version version) {
            try {
                using (StreamWriter writer = new StreamWriter(Path.FullName)) {
                    writer.Write(version.ToString());
                }
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Unable to write version number to \"{0}\".", Path.FullName), 
                    Location, ex);
            }
        }

        /// <summary>
        /// Calculates the build number based on the number of months since the 
        /// start date.
        /// </summary>
        /// <returns>
        /// The build number based on the number of months since the start date.
        /// </returns>
        private int CalculateMonthDayBuildNumber() {
            // we need to have a start date defined!
            if (StartDate == DateTime.MinValue) {
                throw new BuildException("\"startdate\" must be set when the"
                    + "\"MonthDay\" algorithm is used.", Location);
            }

            DateTime today = DateTime.Now;
            if (StartDate > today) {
                throw new BuildException("Start date cannot be in the future.",
                    Location);
            }

            // Calculate difference in years
            int years = today.Year - StartDate.Year;

            // Calculate difference in months
            int months;
            if (today.Month < StartDate.Month) {
                --years;  // borrow from years
                months = (today.Month + 12) - StartDate.Month;
            } else {
                months = today.Month - StartDate.Month;
            }

            months += years * 12;

            // The days is simply today's day
            int days = today.Day;

            return months * 100 + days;
        }

        /// <summary>
        /// Calculates the number of seconds since midnight. 
        /// start date.
        /// </summary>
        /// <returns>
        /// The number of seconds since midnight.
        /// </returns>
        private int CalculateSecondsSinceMidnight() {
            DateTime today = DateTime.Now;
            return (today.Hour * 3600 + today.Minute * 60 + today.Second) / 10;
        }

        /// <summary>
        /// Calculates the build number of the version number based on 
        /// <see cref="BuildType" />.
        /// </summary>
        /// <returns>
        /// The build number.
        /// </returns>
        private int CalculateBuildNumber(int currentBuildNumber) {
            switch (BuildType) {
                case BuildNumberAlgorithm.MonthDay:
                    return CalculateMonthDayBuildNumber();
                case BuildNumberAlgorithm.Increment:
                    return currentBuildNumber + 1;
                case BuildNumberAlgorithm.NoIncrement:
                    return currentBuildNumber;
                default:
                    throw new InvalidEnumArgumentException("BuildType",
                        (int) BuildType, typeof(BuildNumberAlgorithm));
            }
        }

        /// <summary>
        /// Calculates the complete version.
        /// </summary>
        /// <returns>
        /// The version.
        /// </returns>
        private Version CalculateVersion() {
            Version version = ReadVersionFromFile();

            int newBuildNumber = CalculateBuildNumber(version.Build);
            int newRevisionNumber = CalculateRevisionNumber(version, newBuildNumber);

            return new Version(version.Major, version.Minor, newBuildNumber, 
                newRevisionNumber);
        }

        /// <summary>
        /// Calculates the revision number of the version number based on RevisionType specified
        /// </summary>
        /// <returns>
        /// The revision number.
        /// </returns>
        private int CalculateRevisionNumber(Version version, int newBuildNumber) {
            int newRevsionNumber;

            // modify revision number according to revision type setting
            switch (RevisionType) {
                case RevisionNumberAlgorithm.Automatic:
                    newRevsionNumber = CalculateSecondsSinceMidnight();
                    break;
                case RevisionNumberAlgorithm.Increment:
                    if (newBuildNumber != version.Build) {
                        // reset revision number to zero if the build number
                        // has changed
                        newRevsionNumber = 0;
                    } else {
                        // increment the revision number if this is a revision
                        // of the same build
                        newRevsionNumber = version.Revision + 1;
                    }
                    break;
                default:
                    throw new InvalidEnumArgumentException("RevisionType",
                        (int) RevisionType, typeof(RevisionNumberAlgorithm));

            }

            return newRevsionNumber;
        }

        #endregion Private Instance Methods
    }
}
