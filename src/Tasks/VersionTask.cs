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

// John Lam (jlam@iunknown.com)

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks {

    /// <summary>Increments a version number counter from a text file. Resulting version string is written back
    ///  to the file and entered in a NAnt property defined by <c>prefix</c> + &quot;version&quot;.</summary>
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
    public class VersionTask : Task 
    {
        struct VersionNumber {
            public int Major;
            public int Minor;
            public int Build;
            public int Revision;

            public VersionNumber( int major, int minor, int build, int revision ) {
                Major     = major;
                Minor     = minor;
                Build     = build;
                Revision  = revision;
            }
        }

        private string        revisionType_     = "automatic";
        private string        prefix_           = "sys.";
        private string        path_             = "build.number";
        private string        buildType_        = "monthday";
        private const string  mask_             = @"([0-9]+)\.([0-9]+)\.([0-9]+)\.([0-9]+)";
        private const int     maskMatchCount_   = 5;
        private DateTime      startDate_        = DateTime.MinValue;

        /// <summary>The string to prefix the property names with.  Default is "sys."</summary>
        [TaskAttribute("prefix", Required=false)]
        public string Prefix {
          get { return prefix_; }
          set { prefix_ = value; }
        }

        /// <summary>
        /// Start of project. Date from which to calculate build number. Required if &quot;monthday&quot; is used as <c>buildtype</c>.
        /// </summary>
        [ TaskAttribute( "startDate" ) ]
        public string StartDate {
          set { startDate_ = Convert.ToDateTime( value ); }
        }

        /// <summary>
        /// Path to the file containing the current version number. Defaults to 'build.number'.
        /// </summary>
        [ TaskAttribute( "path" ) ]
        public string Path 
        {
          get { return path_; }
          set { path_ = value; }
        }

        /// <summary>
        /// Algorithm for generating build number. Valid values are &quot;monthday&quot;,
        /// &quot;increment&quot; and &quot;noincrement&quot;. Defaults to &quot;monthday&quot;.
        /// </summary>
        [ TaskAttribute( "buildtype" ) ]
        public string BuildType { 
            set {
                if( "monthday" == value || "increment" == value || "noincrement" == value )
                    buildType_ = value;
            else
              throw new Exception( String.Format( "Invalid VersionType specified: {0} must be 'monthday', 'increment', or 'noincrement'", value ) );
          }
          get { return buildType_; }
        }

        /// <summary>
        /// Algorithm for generating revision number. Valid values are &quot;automatic&quot; and
        /// &quot;increment&quot;.
        /// </summary>
        [ TaskAttribute( "revisiontype" ) ]
        public string RevisionType {
          set 
          {
            if( "automatic" == value || "increment" == value )
              revisionType_ = value;
            else
              throw new Exception( String.Format( "Invalid RevisionType specified: {0} must be 'automatic' or 'increment'", value ) );
          }
          get { return revisionType_; }
    }


    private string ReadVersionString()
    {
      string result;
      StreamReader reader = new StreamReader( Path );

      if( null != reader )
      {
        using( reader )
        {
          result = reader.ReadToEnd();
        }
      }
      else
      {
        throw new Exception( String.Format( "Cannot open: {0}", Path ) );
      }
      return result;
    }

    private void WriteVersionString( string buildString )
    {
      StreamWriter writer = new StreamWriter( Path );

      if( null != writer )
      {
        using( writer )
        {
          writer.Write( buildString );
        }
      }
      else
        throw new Exception( String.Format( "Cannot open: {0} for write!", Path ) );
    }

    private VersionNumber ParseVersionString( string versionString )
    {
      Regex regex = new Regex( mask_ );

      if( null != regex )
      {
        Match matches = regex.Match( versionString );
        if( null == matches )
          throw new Exception( "Invalid build number string!" );

        if( maskMatchCount_ != matches.Groups.Count )
          throw new Exception( "Invalid build number string!" );

        return new VersionNumber( Convert.ToInt32( matches.Groups[ 1 ].Value ),
                                  Convert.ToInt32( matches.Groups[ 2 ].Value ),
                                  Convert.ToInt32( matches.Groups[ 3 ].Value ),
                                  Convert.ToInt32( matches.Groups[ 4 ].Value ) );
      }
      else
        throw new Exception( String.Format( "Failed to create a regex object using mask: {0}", mask_ ) );
    }

    // This method calculates the build number based on the number of months since the start date

    private int CalculateMonthDayBuildNumber()
    {
      // We need to have a start date defined!
      if( DateTime.MinValue == startDate_ )
        throw new Exception( "Start date must be defined if using the month+day algorithm" );

      DateTime today = DateTime.Now;
      if( startDate_ > today )
        throw new Exception( "Start date cannot be after today" );

      // Calculate difference in years
      int years = today.Year - startDate_.Year;

      // Calculate difference in months
      int months;
      if( today.Month < startDate_.Month )
      {
        --years;  // borrow from years
        months = (today.Month + 12) - startDate_.Month;
      }
      else
        months = today.Month - startDate_.Month;

      months += years * 12;

      // The days is simply today's day
      int days = today.Day;

      return months * 100 + days;
    }

    private int CalculateSecondsSinceMidnight()
    {
      DateTime today = DateTime.Now;
      return (today.Hour * 3600 + today.Minute * 60 + today.Second) / 10;
    }

    private int CalculateRevisionNumber( VersionNumber versionNumber )
    {
      if( "automatic" == revisionType_ )      
        return CalculateSecondsSinceMidnight();
      else
        return versionNumber.Revision + 1;
    }

    private int CalculateBuildNumber( int currentBuildNumber )
    {
      switch( BuildType )
      {
        case "monthday":
          return CalculateMonthDayBuildNumber();
        case "increment":
          return currentBuildNumber + 1;
        case "noincrement":
          return currentBuildNumber;
        default:
          throw new Exception( String.Format( "Invalid BuildType: {0}", BuildType ) );
      }
    }

    private string CalculateVersionNumber()
    {
      string        versionString   = ReadVersionString();
      VersionNumber versionNumber = ParseVersionString( versionString );

      int newBuildNumber = CalculateBuildNumber( versionNumber.Build );

      if( "automatic" == RevisionType )
        versionNumber.Revision = CalculateSecondsSinceMidnight();
      else
      {
        if( newBuildNumber != versionNumber.Build )
          versionNumber.Revision = 0;
        else
          versionNumber.Revision += 1;
      }

      versionNumber.Build = newBuildNumber;

      string newVersionString = String.Format( "{0}.{1}.{2}.{3}", versionNumber.Major,
                                                                  versionNumber.Minor,
                                                                  versionNumber.Build,
                                                                  versionNumber.Revision );
      WriteVersionString( newVersionString );
      return newVersionString;
    }

    protected override void ExecuteTask() 
    {
      string buildNumber = CalculateVersionNumber();
      Project.Properties[ Prefix + "version" ] = buildNumber;
      Log(Level.Info,  String.Format( "{0} Build number: {1}", LogPrefix, buildNumber ) );
    }


    }
    }
