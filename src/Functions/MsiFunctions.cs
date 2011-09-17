// NAnt - A .NET build tool
// Copyright (C) 2001-2006 Gerry Shaw
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
// Gert Driesen (drieseng@users.sourceforge.net)

using System;
using System.Globalization;
using System.IO;
using System.Reflection;

using WindowsInstaller;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;
using NAnt.Core.Util;

namespace NAnt.Contrib.Functions {
    /// <summary>
    /// Allows information on Windows Installer databases and products to be
    /// retrieved.
    /// </summary>
    [FunctionSet("msi", "Windows Installer")]
    public class MsiFunctions : FunctionSetBase {
        #region Public Instance Constructors

        public MsiFunctions(Project project, PropertyDictionary properties) : base(project, properties) { }

        #endregion Public Instance Constructors

        #region Public Static Methods

        /// <summary>
        /// Returns the product code of the specified Windows Installer
        /// database.
        /// </summary>
        /// <param name="databasePath">The path of the Windows Installer database.</param>
        /// <returns>
        /// The product code of the specified Windows Installer database.
        /// </returns>
        /// <exception cref="FileNotFoundException">The specified installer database does not exist.</exception>
        /// <exception cref="ArgumentException">
        ///   <para>
        ///   The installer database could not be opened.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///   The product code property is not available in the installer
        ///   database.
        ///   </para>
        /// </exception>
        /// <example>
        ///   <para>
        ///   Retrieves the product code of a given installer database, and
        ///   fails the build if the product is not installed.
        ///   </para>
        ///   <code>
        ///     <![CDATA[
        /// <property name="office.productcode" value=${msi::get-product-code('Office.msi')}" />
        /// <fail unless="${msi::is-installed(office.productcode)}">Please install Office first.</fail>
        ///     ]]>
        ///   </code>
        /// </example>
        [Function("get-product-code")]
        public string GetProductCode(string databasePath) {
            string fullDBPath = Project.GetFullPath(databasePath);

            if (!File.Exists(fullDBPath))
                throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture,
                    "Database '{0}' does not exist.", fullDBPath));

            Installer installer = CreateInstaller();
            Database database;
            
            try {
                database = installer.OpenDatabase(fullDBPath, MsiOpenDatabaseMode.msiOpenDatabaseModeDirect);
            } catch (Exception ex) {
                throw new ArgumentException (string.Format(CultureInfo.InvariantCulture,
                    "Database '{0}' could not be opened.", fullDBPath), ex);
            }

            View view = null;
            try {
                view = database.OpenView("SELECT `Value` FROM `Property` WHERE `Property` = 'ProductCode'");
                view.Execute((Record) null);
                Record record = view.Fetch();
                if (record == null)
                    throw new ArgumentException("The product code is not set.");

                // Workaround for Mono bug #323644
                // return record.get_StringData (1);
                PropertyInfo p = typeof (Record).GetProperty ("StringData");
                return (string) p.GetValue (record, new object [] { 1 });
            } finally {
                if (view != null)
                    view.Close ();
                view = null;
                database = null;

                // force garbage collection to ensure database is released
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        /// <summary>
        /// Returns a value indicating whether a product with the specified
        /// <c>product code</c> is installed.
        /// </summary>
        /// <param name="productCode">The product code of the product to check.</param>
        /// <returns>
        /// <see langword="true" /> if a product with the specified product
        /// code is installed; otherwise, <see langword="false" />.
        /// </returns>
        /// <example>
        ///   <para>
        ///   Retrieves the product code of a given installer database, and
        ///   fails the build if the product is not installed.
        ///   </para>
        ///   <code>
        ///     <![CDATA[
        /// <property name="office.productcode" value=${msi::get-product-code('Office.msi')}" />
        /// <fail unless="${msi::is-installed(office.productcode)}">Please install Office first.</fail>
        ///     ]]>
        ///   </code>
        /// </example>
        [Function("is-installed")]
        public static bool IsInstalled(string productCode) {
            if (productCode == null)
                throw new ArgumentNullException("productCode");

            Installer installer = CreateInstaller();

            bool found = false;
            foreach (string product in installer.Products) {
                if (product == productCode) {
                    found = true;
                    break;
                }
            }

            return found;
        }

        #endregion Public Static Methods

        #region Private Static Methods

        private static Installer CreateInstaller() {
            Type type = Type.GetTypeFromProgID("WindowsInstaller.Installer");
            if (type == null)
                throw new InvalidOperationException("Windows Installer is not available.");

            return (Installer) Activator.CreateInstance(type);
        }

        #endregion Private Static Methods
    }
}
