// Copyright (C) 2002 Gordon Weakliem
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

// Gordon Weakliem (gweakliem@oddpost.com)
using System;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Text;
using NAnt.Core;
using NAnt.Core.Types;
using NAnt.Core.Tasks;
using NAnt.Core.Attributes;


namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Generates an AssemblyInfo.cs file using the attributes given.
    /// </summary>
    [TaskName("asminfo")]
    public class AssemblyInfoTask : Task {
        
        private string _asmVer = "1.0.*.*";
        private string _projectName = String.Empty ;
        private string _description = String.Empty;
        private string _config = String.Empty;
        private string _companyName = String.Empty;
        private string _productName = String.Empty;
        private string _copyright = String.Empty;
        private string _trademark = String.Empty;
        private string _culture = String.Empty;
        private bool _delaySign = false;
        private string _keyFile = String.Empty;
        private string _keyName = String.Empty;
        private string _path = System.IO.Directory.GetCurrentDirectory() + "AssemblyInfo.cs";

        /// <summary>
        /// Options for the assembly info file.  These are specified as attribute/initializer pairs.
        /// </summary>
        /// <example>
        /// &lt;asminfo&gt;
        ///     &lt;attributes&gt;
        ///         &lt;option name="AssemblyVersion" value="1.0.0.0" /&gt;
        ///         &lt;option name="AssemblyTitle" value="My fun assembly" /&gt;
        ///         &lt;option name="AssemblyDescription" value="More fun than a barrel of monkeys" /&gt;
        ///         &lt;option name="AssemblyCopyright" value="Copyright (c) 2002, Monkeyboy, Inc." /&gt;
        ///     &lt;/attributes&gt;
        /// &lt;/asminfo&gt;
        /// </example>
        OptionCollection _attributes = new OptionCollection();
        [BuildElementCollection("attributes")]
        public OptionCollection AssemblyAttributes { get { return _attributes; } }

        /// <summary>
        /// Path where the generated assemblyinfo.cs gets stored.
        /// </summary>
        [TaskAttribute("path")]
        public string Path {
            get { return _path; }
            set { _path = value; }
        }

        [TaskAttribute("version")]
        public string AssemblyVersion {
            get { return _asmVer; }
            set { _asmVer = value; }
        }

        [TaskAttribute("name")]
        public string ProjectName  {
            get { return _projectName; }
            set { _projectName= value; }
        }

        [TaskAttribute("description")]
        public string ProjectDescription {
            get { return _description; }
            set { _description = value; }
        }

        [TaskAttribute("config")]
        public string AssemblyConfiguration {
            get { return _config; }
            set { _config = value; }
        }

        [TaskAttribute("companyname")]
        public string CompanyName {
            get { return _companyName; }
            set { _companyName= value; }
        }

        [TaskAttribute("productname")]
        public string ProductName {
            get { return _productName; }
            set { _productName = value; }
        }

        [TaskAttribute("copyright")]
        public string Copyright {
            get { return _copyright; }
            set { _copyright = value; }
        }

        [TaskAttribute("trademark")]
        public string Trademark {
            get { return _trademark; }
            set { _trademark = value; }
        }

        [TaskAttribute("culture")]
        public string Culture {
            get { return _culture; }
            set { _culture = value; }
        }
        
        [TaskAttribute("delaysign")]
        public bool DelaySign {
            get { return _delaySign; }
            set { _delaySign = value; }
        }

        [TaskAttribute("keyfile")]
        public string KeyFile {
            get { return _keyFile; }
            set { _keyFile= value; }
        }

        [TaskAttribute("keyname")]
        public string KeyName {
            get { return _keyName; }
            set { _keyName= value; }
        }

        protected override void ExecuteTask()  {
            try
            {
                CodeCompileUnit ccu = new CodeCompileUnit();
                
                CodeAttributeDeclaration att = new CodeAttributeDeclaration("System.Reflection.AssemblyVersion");
                att.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(AssemblyVersion)));
                ccu.AssemblyCustomAttributes.Add(att);

                att = new CodeAttributeDeclaration("System.Reflection.AssemblyTitle");
                att.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(ProjectName)));
                ccu.AssemblyCustomAttributes.Add(att);

                att = new CodeAttributeDeclaration("System.Reflection.AssemblyDescription");
                att.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(ProjectDescription)));
                ccu.AssemblyCustomAttributes.Add(att);

                att = new CodeAttributeDeclaration("System.Reflection.AssemblyConfiguration");
                att.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(AssemblyConfiguration)));
                ccu.AssemblyCustomAttributes.Add(att);

                att = new CodeAttributeDeclaration("System.Reflection.AssemblyCompany");
                att.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(CompanyName)));
                ccu.AssemblyCustomAttributes.Add(att);

                att = new CodeAttributeDeclaration("System.Reflection.AssemblyProduct");
                att.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(ProductName)));
                ccu.AssemblyCustomAttributes.Add(att);

                if (Copyright.Equals(String.Empty))
                {
                    Copyright = String.Format("Copyright (c) {1} {0}.  All Rights Reserved.",DateTime.Now.Year,CompanyName);
                }
                att = new CodeAttributeDeclaration("System.Reflection.AssemblyCopyright");
                att.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(Copyright)));
                ccu.AssemblyCustomAttributes.Add(att);

                att = new CodeAttributeDeclaration("System.Reflection.AssemblyTrademark");
                att.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(Trademark)));
                ccu.AssemblyCustomAttributes.Add(att);

                att = new CodeAttributeDeclaration("System.Reflection.AssemblyCulture");
                att.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(Culture)));
                ccu.AssemblyCustomAttributes.Add(att);

                att = new CodeAttributeDeclaration("System.Reflection.AssemblyDelaySign");
                att.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(DelaySign)));
                ccu.AssemblyCustomAttributes.Add(att);

                att = new CodeAttributeDeclaration("System.Reflection.AssemblyKeyFile");
                att.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(KeyFile)));
                ccu.AssemblyCustomAttributes.Add(att);

                att = new CodeAttributeDeclaration("System.Reflection.AssemblyKeyName");
                att.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(KeyName)));
                ccu.AssemblyCustomAttributes.Add(att);

                att = new CodeAttributeDeclaration("log4net.Config.Domain");
                att.Arguments.Add(new CodeAttributeArgument("UseDefaultDomain",new CodePrimitiveExpression(true)));
                ccu.AssemblyCustomAttributes.Add(att);

                CSharpCodeProvider ccp = new CSharpCodeProvider();
                ICodeGenerator gen = ccp.CreateGenerator();

                System.IO.StreamWriter w = new System.IO.StreamWriter(Path,false,System.Text.Encoding.Default);
                gen.GenerateCodeFromCompileUnit(ccu,w,new CodeGeneratorOptions());

                w.Close();
            }
            catch (Exception e)
            {
                throw new NAnt.Core.BuildException(e.Message,e);
            }
        }

        
    }
}
