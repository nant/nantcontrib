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

using System;
using System.DirectoryServices; 
using System.Globalization;
using System.Xml;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;
using NAnt.Core.Types;
using NAnt.Core.Util;

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
    ///   <code>
    ///     <![CDATA[
    /// <adsisetprop path="${iis.path}/Root" propname="AuthAnonymous" propvalue="true" />
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <code>
    ///     <![CDATA[
    /// <adsisetprop path="${iis.path}/Root/GWSSample">
    ///     <properties>
    ///         <option name="AuthBasic" value="false" />
    ///         <option name="AuthNTLM" value="true" />
    ///     </properties>
    /// </adsisetprop>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("adsisetprop")]
    public class ADSISetPropertyTask : ADSIBaseTask {
        #region Private Instance Fields

        private string _propertyName;
        private string _propertyValue;
        private OptionCollection _propertyList = new OptionCollection();

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The name of the property to set.
        /// </summary>
        [TaskAttribute("propname")]
        public string PropertyName {
            get { return _propertyName; }
            set { _propertyName = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// The new value of the property.
        /// </summary>
        [TaskAttribute("propvalue")]
        public String PropertyValue {
            get { return _propertyValue; }
            set { _propertyValue = value; }
        }

        [BuildElementCollection("properties", "option")]
        public OptionCollection PropertyList {
            get { return _propertyList; }
        }

        #endregion Public Instance Properties

        #region Override implementation of Element

        protected override void Initialize() {
            base.Initialize ();

            if (this.PropertyName == null && this.PropertyList.Count == 0) {
                throw new BuildException("\"propname\" attribute or at least one <option> element is required.");
            }
        }

        #endregion Override implementation of Element

        #region Override implementation of Task

        /// <summary>
        /// Sets the specified property
        /// </summary>
        protected override void ExecuteTask() {
            string propertyName = null;
            string propertyValue = null;

            try {
                // Get the directory entry for the specified path and set the 
                // property.
                using (DirectoryEntry pathRoot = new DirectoryEntry(Path)) {
                    pathRoot.RefreshCache();
          
                    // if there was a property named in the attributes, set it.
                    if (PropertyName != null) {
                        // store property name and value in local field, to allow
                        // an accurate error message
                        propertyName = PropertyName;
                        propertyValue = PropertyValue;

                        SetProperty(pathRoot, propertyName, propertyValue);
                    }

                    // set the properties named in the child property list.
                    foreach (Option property in PropertyList) {
                        if (property.IfDefined && !property.UnlessDefined) {
                            // store property name and value in local field, to allow
                            // an accurate error message
                            propertyName = property.OptionName;
                            propertyValue = property.Value;

                            SetProperty(pathRoot, propertyName, propertyValue);
                        }
                    }

                    pathRoot.CommitChanges();
                }
            } catch (BuildException) {
                // rethrow any BuildExceptions from further down.
                throw;
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Unable to set property '{0}' to '{1}' on '{2}.", propertyName, 
                    propertyValue, Path), Location, ex);
            }
        }

        #endregion Override implementation of Task

        #region Private Instance Methods

        /// <summary>
        /// Sets the named property on the specified <see cref="DirectoryEntry" /> 
        /// to the given value.
        /// </summary>
        /// <param name="entry">The <see cref="DirectoryEntry" /> we're modifying.</param>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="propertyValue">The value to set the property to.</param>
        /// <remarks>
        /// The following cases are notable:
        /// <list type="bulleted">
        ///   <item>
        ///   If the property does not exist on the item, it is inserted as a 
        ///   string.
        ///   </item>
        ///   <item>
        ///   If the property already exists, this method will attempt to preserve
        ///   the type of the property.  The types this method knows about are 
        ///   <see cref="string" />, <see creef="bool" />, and <see cref="int" />.
        ///   </item>
        ///   <item>
        ///   If the property exists and is an array, the value is added to the 
        ///   array, but only if it's not already present.
        ///   </item>
        /// </list>
        /// </remarks>
        private void SetProperty(DirectoryEntry entry, string propertyName, string propertyValue) {
            if (!entry.Properties.Contains(propertyName)) {
                entry.Properties[propertyName].Insert(0, propertyValue);
            }

            // TODO: can I find out the property type from the entry's schema?
            if (entry.Properties[propertyName].Value.GetType().IsArray) {
                // with arrays, don't add the value if it's already there.
                object objToSet = Convert(entry.Properties[propertyName][0], 
                    propertyValue);
                if (!entry.Properties[propertyName].Contains(objToSet)) {
                    entry.Properties[propertyName].Add(objToSet);
                }
            } else {
                object originalValue = entry.Properties[propertyName].Value;
                object newValue = Convert(originalValue, propertyValue);
                entry.Properties[propertyName][0] = newValue;
            }

            Log(Level.Info, "Property '{0}' set to '{1}' on '{2}.", 
                propertyName, propertyValue, Path);
        }
    
        private object Convert(object existingValue, string newValue) {
            if (existingValue is string) {
                return newValue.ToString();
            } else if (existingValue is bool) {
                return Boolean.Parse(newValue);
            } else if (existingValue is int) {
                return Int32.Parse(newValue);
            } else {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture, 
                    "Don't know how to set property type {0}.", 
                    existingValue.GetType().FullName), Location);
            }
        }

        #endregion Private Instance Methods
    }
}
