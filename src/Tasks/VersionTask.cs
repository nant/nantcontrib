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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Increments a version number counter from a text file. The resulting 
    /// version string is written back to the file and entered in a NAnt property 
    /// defined by <c>prefix</c> + &quot;version&quot;.
    /// </summary>
    /// <remarks>
    ///   <para><c>buildtype</c> determines how the build number is generated:
    ///   <list type="bullet">
    ///    <item><term>monthday</term><description>use the # months since start of project * 100 + current day in month as build number</description></item>
    ///    <item><term>increment</term><description>increment a build number stored in the build.number file in current directory</description></item>
    ///    <item><term>noincrement</term><description>do not increment the build number - we use this if we need to update an existing build</description></item>
    ///   </list></para>
    ///   <para><c>revisiontype</c> determines how the revision number is generated:
    ///   <list type="bullet">
    ///    <item><term>automatic</term><description>use the # seconds since the start of today / 10</description></item>
    ///    <item><term>increment</term><description>use the file version's revision number spec'd by the revisionbin attribute</description></item>
    ///   </list></para>
    /// </remarks>
    [TaskName("version")]
    public class VersionTask : Task {
        private struct VersionNumber {
            public int Major;
            public int Minor;
            public int Build;
            public int Revision;

            public VersionNumber(int major, int minor, int build, int revision) {
                Major = major;
                Minor = minor;
                Build = build;
                Revision = revision;
            }
        }

        #region Private Instance Fields

        private string _revisionType = "automatic";
        private string _prefix = "sys.";
        private string _path = "build.number";
        private string _buildType = "monthday";
        private DateTime _startDate;

        #endregion Private Instance Fields

        #region Private Static Fields

        private const string _mask = @"([0-9]+)\.([0-9]+)\.([0-9]+)\.([0-9]+)";
        private const int _maskMatchCount = 5;

        #endregion Private Static Fields

        #region Public Instance Properties

        /// <summary>
        /// The string to prefix the property name with. The default is 
        /// <c>'sys.'</c>.
        /// </summary>
        [TaskAttribute("prefix")]
        public string Prefix {
            get { return _prefix; }
            set { _prefix = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Start of project. Date from which to calculate build number. 
        /// Required if &quot;monthday&quot; is used as <c>buildtype</c>.
        /// </summary>
        [TaskAttribute("startDate")]
        public string StartDate {
            get { return _startDate.ToString("G", DateTimeFormatInfo.InvariantInfo); }
            set {
                try {
                    _startDate = Convert.ToDateTime(value);
                } catch (FormatException ex) {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, 
                        "Invalid string representation {0} of a DateTime value.", value), 
                        "StartDate", ex);
                }
            } 
        }

        /// <summary>
        /// Path to the file containing the current version number. The default 
        /// file is <c>'build.number'</c> in the project base directory.
        /// </summary>
        [TaskAttribute("path")]
        [StringValidator(AllowEmpty=false)]
        public string Path {
            get { return Project.GetFullPath(_path); }
            set { _path = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Algorithm for generating build number. Valid values are &quot;monthday&quot;,
        /// &quot;increment&quot; and &quot;noincrement&quot;. The default is &quot;monthday&quot;.
        /// </summary>
        [TaskAttribute("buildtype")]
        public string BuildType {
            get { return _buildType; }
            set {
                if (value == "monthday" || value == "increment" || value == "noincrement") {
                    _buildType = value;
                } else {
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                        "Invalid VersionType specified: {0} must be 'monthday', 'increment', or 'noincrement'", value),
                        Location);
                }
            }
        }

        /// <summary>
        /// Algorithm for generating revision number. Valid values are &quot;automatic&quot; and
        /// &quot;increment&quot;.
        /// </summary>
        [TaskAttribute("revisiontype")]
        public string RevisionType {
            get { return _revisionType; }
            set {
                if (value == "automatic" || value == "increment") {
                    _revisionType = value;
                } else {
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                        "Invalid RevisionType specified: {0} must be 'automatic' or 'increment'", value),
                        Location);
                }
            }
        }

        #endregion Public Instance Properties

        #region Override implementation of Task

        protected override void ExecuteTask() {
            // calculate new version number
            string buildNumber = CalculateVersionNumber();

            // expose version number as build property
            Project.Properties[Prefix + "version"] = buildNumber;

            // output new version number in build log
            Log(Level.Info, "Build number '{0}'.", buildNumber);
        }

        #endregion Override implementation of Task

        #region Private Instance Methods

        private string ReadVersionString() {
            try {
                using (StreamReader reader = new StreamReader(Path)) {
                    return reader.ReadToEnd();
                }
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Unable to read version number from {0}.", Path), Location, 
                    ex);
            }
        }

        private void WriteVersionString(string buildString) {
            try {
                using (StreamWriter writer = new StreamWriter(Path)) {
                    writer.Write(buildString);
                }
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Unable to write version number to {0}.", Path), Location, 
                    ex);
            }
        }

        private VersionNumber ParseVersionString(string versionString) {
            Regex regex = new Regex(_mask);

            if (regex != null) {
                Match matches = regex.Match(versionString);
                if (matches == null) {
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                        "Invalid build number string {0}.", versionString),
                        Location);
                }

                if (_maskMatchCount != matches.Groups.Count) {
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture, 
                        "Invalid build number string {0}.", versionString),
                        Location);
                }

                return new VersionNumber(Convert.ToInt32(matches.Groups[1].Value),
                    Convert.ToInt32(matches.Groups[2].Value),
                    Convert.ToInt32(matches.Groups[3].Value),
                    Convert.ToInt32(matches.Groups[4].Value));
            } else {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Failed to create a regex object using mask {0}.", _mask),
                    Location);
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
            if (_startDate == DateTime.MinValue) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Start date must be defined if using the month+day algorithm."),
                    Location);
            }

            DateTime today = DateTime.Now;
            if (_startDate > today) {
                throw new BuildException("Start date cannot be in the future.",
                    Location);
            }

            // Calculate difference in years
            int years = today.Year - _startDate.Year;

            // Calculate difference in months
            int months;
            if (today.Month < _startDate.Month) {
                --years;  // borrow from years
                months = (today.Month + 12) - _startDate.Month;
            } else {
                months = today.Month - _startDate.Month;
            }

            months += years * 12;

            // The days is simply today's day
            int days = today.Day;

            return months * 100 + days;
        }

        private int CalculateSecondsSinceMidnight() {
            DateTime today = DateTime.Now;
            return (today.Hour * 3600 + today.Minute * 60 + today.Second) / 10;
        }

        private int CalculateBuildNumber(int currentBuildNumber) {
            switch (BuildType) {
                case "monthday":
                    return CalculateMonthDayBuildNumber();
                case "increment":
                    return currentBuildNumber + 1;
                case "noincrement":
                    return currentBuildNumber;
                default:
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                        "Invalid build type {0}.", BuildType), Location);
            }
        }

        private string CalculateVersionNumber() {
            // read the current version string from file
            string versionString = ReadVersionString();

            // parse the version string 
            VersionNumber versionNumber = ParseVersionString(versionString);

            // calculate the new build number
            int newBuildNumber = CalculateBuildNumber(versionNumber.Build);

            // modify revision number according to revision type setting
            if (RevisionType == "automatic") {
                versionNumber.Revision = CalculateSecondsSinceMidnight();
            } else {
                if (newBuildNumber != versionNumber.Build) {
                    // reset revision number to zero if the build number has 
                    // changed
                    versionNumber.Revision = 0;
                } else {
                    // increment the revision number if this is a revision of
                    // the same build
                    versionNumber.Revision += 1;
                }
            }

            // set build number
            versionNumber.Build = newBuildNumber;

            // generate new version string
            string newVersionString = string.Format(CultureInfo.InvariantCulture, 
                "{0}.{1}.{2}.{3}", versionNumber.Major, versionNumber.Minor, 
                versionNumber.Build, versionNumber.Revision);

            // write new version string back to file
            WriteVersionString(newVersionString);

            // return the new version string
            return newVersionString;
        }

        #endregion Private Instance Methods
    }
}
