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
using System.IO;
using SourceForge.NAnt;
using SourceForge.NAnt.Attributes;
using InterOpStarTeam = StarTeam;

namespace NAnt.Contrib.Tasks.StarTeam 
{
	/// <summary>
	/// Allows creation of view labels in StarTeam repositories.
	/// </summary>
	/// <remarks>
	/// <para>Often when building projects you wish to label the source control repository.</para>
	/// <para>By default this task creates view labels with the build option turned on.</para>
	/// <para>This task was ported from the Ant task http://jakarta.apache.org/ant/manual/OptionalTasks/starteam.html#stlabel </para>
	/// <para>You need to have the StarTeam SDK installed for this task to function correctly.</para>
	/// </remarks>
	/// <example>
	///   <para>Creates a label in a StarTeam repository.</para>
	///   <code><![CDATA[
	///		<!-- 
	///		constructs a 'url' containing connection information to pass to the task 
	///		alternatively you can set each attribute manually 
	///		-->
	///		<property name="ST.url" value="user:pass@serverhost:49201/projectname/viewname"/>
	///		<stlabel label="3.1 (label title goes here)" description="This is a label description" url="${ST.url}"/>	
	///   ]]></code>
	/// </example>
	[TaskName("stlabel")]
	public class StarTeamLabel:StarTeamTask
	{
		/// <summary> 
		/// The name to be given to the label; required.
		/// </summary>
		[TaskAttribute("label", Required=true)]
		public virtual string Label
		{
			set { _labelName = value; }			
		}

		/// <summary> Should label be marked build : default is true</summary>
		[TaskAttribute("buildlabel", Required=false)]
		[BooleanValidator]         
		public virtual bool BuildLabel
		{
			set { _isBuildLabel = value; }			
		}
				
		/// <summary> Optional description of the label to be stored in the StarTeam project.</summary>
		[TaskAttribute("description", Required=false)]
		public virtual string Description
		{
			set { _description = value; }			
		}
		
		/// <summary> 
		/// Optional: If this property is set the label will be created as of the datetime specified. 
		/// Please provide a datetime format that can be parsed via <see cref="System.DateTime.Parse"/>.
		/// </summary>
		[TaskAttribute("timestamp", Required=false)]
		public virtual string LastBuild
		{
			set 
			{
				try
				{
					_labelAsOfDate = System.DateTime.Parse(value);
					_isAsOfDateSet = true;					
				}
				catch (System.FormatException e)
				{
					throw new BuildException(string.Format("Unable to parse the date {0} : {1}", value ,e.Message),e);
				}
			}
			
		}
		
		/// <summary> The name of the label to be set in Starteam.</summary>
		private string _labelName = null;
		
		/// <summary> The label description to be set in Starteam.</summary>
		private string _description = null;

		/// <summary> Is the label being created a build label.</summary>
		private bool _isBuildLabel = true;
                                                                                            
		/// <summary> If set the datetime to set the label as of.</summary>
		private DateTime _labelAsOfDate;
		
		/// <summary> Kludgy flag to keep track if date has been set. 
		/// Please kill this if you can. Here based on experiences I have had with VB.NET</summary>
		private bool _isAsOfDateSet = false;

		/// <summary> This method does the work of creating the new view and checking it into Starteam.</summary>
		protected override void  ExecuteTask()
		{                         			
			InterOpStarTeam.StLabel newLabel;
			InterOpStarTeam.StView snapshot = openView();
			InterOpStarTeam.StLabelFactoryClass starTeamLabelFactory = new InterOpStarTeam.StLabelFactoryClass();
			// Create the new label and update the repository
			try 
			{
				newLabel = starTeamLabelFactory.CreateViewLabel(snapshot,_labelName, _description, _labelAsOfDate, _isBuildLabel);
				newLabel.update();
				Log.WriteLine("Created Label: {0}",_labelName);
			}
			catch(Exception e)
			{
				throw new BuildException(string.Format("Creating label {0} failed : {1}",_labelName,e.Message));
		    }
		}
		
		/// <summary> 
		/// Override of base-class abstract function creates an appropriately configured view.  
		/// For labels this a view configured as of this.lastBuild.
		/// </summary>
		/// <param name="raw">the unconfigured <code>View</code></param>
		/// <returns>the snapshot <code>View</code> appropriately configured.</returns>
		protected internal override InterOpStarTeam.StView createSnapshotView(InterOpStarTeam.StView raw)
		{
			InterOpStarTeam.StView starTeamView;
			InterOpStarTeam.StViewFactory starTeamViewFactory = new InterOpStarTeam.StViewFactory();
			InterOpStarTeam.StViewConfigurationStaticsClass starTeamViewConfiguration = new InterOpStarTeam.StViewConfigurationStaticsClass();
			if(_isAsOfDateSet)				
			{
				starTeamView = starTeamViewFactory.Create(raw, starTeamViewConfiguration.createFromTime(_labelAsOfDate));
			}
			else 
			{
				starTeamView = starTeamViewFactory.Create(raw, starTeamViewConfiguration.createTip());
			}
			return starTeamView;
		}
	}
}