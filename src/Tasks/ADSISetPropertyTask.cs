// NAnt - A .NET build tool
// Copyright (C) 2002 Galileo International
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
// Gordon Weakliem (gordon.weakliem@galileo.com)
// 
using System;
using System.DirectoryServices; 
using NAnt.Core;
using NAnt.Core.Types;
using NAnt.Core.Tasks;
using NAnt.Core.Attributes;

namespace NAnt.Contrib.Tasks {
  /// <summary>
  /// Sets a property on an ADSI object.
  /// </summary>
  /// <remarks>
  /// This task uses a heuristic to determine the type of the property in ADSI.  The following cases are notable:
  /// <list type="bulleted">
  ///   <item>If the property does not exist on the item, it is inserted as a string.</item>
  ///   <item>If the property already exists, this method will attempt to preserve
  ///   the type of the property.  The types this method knows about are String,
  ///   Boolean, and Int32.</item>
  ///   <item>If the property exists and is an array, the value is added to 
  ///   the array, but only if it is not already present.</item>
  /// </list>
  /// </remarks>
  /// <example>
  /// &lt;adsisetprop path='${iis.path}/Root' propname='AuthAnonymous' propvalue='true'/&gt;
  /// 
  /// &lt;adsisetprop path='${iis.path}/Root/GWSSample'&gt;
  /// &lt;properties&gt;
  /// &lt;option name='AuthBasic' value='false'/&gt;
  /// &lt;option name='AuthNTLM' value='true'/&gt;
  /// &lt;/properties&gt;
  /// &lt;/adsisetprop&gt;
  /// </example>
  [TaskName("adsisetprop")]
  public class ADSISetPropertyTask : ADSIBaseTask
  {
    public ADSISetPropertyTask()
    {
      _propertyList = new OptionCollection();
    }

    private string _propName;
    private string _propValue;

    /// <summary>
    /// The name of the property to set
    /// </summary>
    [TaskAttribute("propname",Required=false)]
    public String PropName
    {
      get { return _propName; }
      set { _propName = value; }
    }

    /// <summary>
    /// The new value of the property
    /// </summary>
    [TaskAttribute("propvalue",Required=false)]
    public String PropValue
    {
      get { return _propValue; }
      set { _propValue = value; }
    }

    /// <summary>
    /// A group of properties to set with this task.
    /// </summary>
    private OptionCollection _propertyList = new OptionCollection();

    [BuildElementCollection("properties", "option", Required=false)]
    public OptionCollection PropertyList
    {
      get { return _propertyList; }
    }

    /// <summary>
    /// Sets the specified property
    /// </summary>
    protected override void ExecuteTask() 
    {
      if ( (this.PropName == null || this.PropValue == null) && this.PropertyList.Count == 0 )
      {
          throw new BuildException("propname and propvalue attributes or <properties> option set is required");
      }
      try
      {
        // Get the directory entry for the specified path and set the 
        // property.
        using (DirectoryEntry pathRoot = new DirectoryEntry(Path))
        {
          pathRoot.RefreshCache();
          
          // if there was a property named in the attributes, set it.
          if (PropName != null)
          {
            SetProperty(pathRoot, PropName, PropValue);
          }
          // set the properties named in the child property list.
          foreach (Option ov in PropertyList)
          {
            SetProperty(pathRoot, ov.Name, ov.Value);
          }

          pathRoot.CommitChanges();
        }
      }
      catch (BuildException)
      {
        // rethrow any BuildExceptions from farther down.
        throw;
      }
      catch (Exception e)
      {
        // log any other exception and wrap it in a BuildException.
        Log(Level.Error, LogPrefix + "Error setting property {0}: {1}", 
          PropName,e.Message);
        throw new NAnt.Core.BuildException(String.Format("Unable to set property {0} to value {1}: {2}",PropName,PropValue,e.Message),e);
      }
    }

    /// <summary>
    /// Sets the named property on the DirectoryEntry passed to the method to the given value.
    /// </summary>
    /// <param name="entry">The DirectoryEntry we're modifying</param>
    /// <param name="propName">The name of the property to set</param>
    /// <param name="propValue">The value to set the property to.</param>
    /// <remarks>
    /// The following cases are notable:
    /// <list type="bulleted">
    ///   <item>If the property does not exist on the item, it is inserted as a string.</item>
    ///   <item>If the property already exists, this method will attempt to preserve
    ///   the type of the property.  The types this method knows about are String,
    ///   Boolean, and Int32.</item>
    ///   <item>If the property exists and is an array, the value is added to 
    ///   the array, but only if it is not already present.</item>
    /// </list>
    /// </remarks>
    private void SetProperty(DirectoryEntry entry, String propName, String propValue)
    {
      if (!entry.Properties.Contains(propName))
      {
        entry.Properties[propName].Insert(0,propValue);
      }
      // TODO: can I find out the property type from the entry's schema?
      if (entry.Properties[propName].Value.GetType().IsArray)
      {
        // with arrays, don't add the value if it's already there.
        object objToSet = Convert(entry.Properties[propName][0],propValue);
        if (!entry.Properties[propName].Contains(objToSet))
        {
          entry.Properties[propName].Add(objToSet);
        }
      }
      else
      {
        //entry.Properties[propName].Clear();
        object originalValue = entry.Properties[propName].Value;
        object newValue= Convert(originalValue,propValue);
        entry.Properties[propName][0] = newValue;
      }
      Log(Level.Info, LogPrefix + "{2}: Property {0} set to {1}", 
        propName, propValue, Path);
    }
    
    private object Convert(object existingValue, String newValue)
    {
      if (existingValue is String)
      {
        return newValue.ToString();
      }
      else if (existingValue is Boolean)
      {
        return Boolean.Parse(newValue);
      }
      else if (existingValue is Int32)
      {
        return Int32.Parse(newValue);
      }
      else
      {
        throw new BuildException(String.Format("Don't know how to set property type {0}",existingValue.GetType().FullName));
      }
    }
  }
}
