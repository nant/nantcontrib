// NAnt - A .NET build tool
// Copyright (C) 2001-2003 Gerry Shaw
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
// Clayton Harbour (claytonharbour@sporadicism.com)
//
//  Special thanks to the AStyle team for such a great tool!
//
//  http://asytle.sourceforge.net

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;
using NAnt.Core.Tasks;
using NAnt.Core.Util;

namespace NAnt.Contrib.Tasks {
    /// <summary>
    /// Formats source code in a given directory to a specified code format.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Most examples inline have been produced by Tal Davidson and team and 
    /// are part of the astyle documentation.  They have been included in
    /// the task documentation as an easy reference.
    /// </para>
    /// NOTE: This task relies on the astyle.exe file being in your path variable.
    /// Please download the astyle.exe from http://astyle.sourceforge.net.
    /// </remarks>
    /// <example>
    ///   <code>
    ///     <![CDATA[
    /// <astyle style="NAnt" cleanup="true">
    ///     <sources>
    ///         <include name="**/**.cs" />
    ///     </sources>
    /// </astyle>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("astyle")]
    public class Astyle : ExternalProgramBase {
        #region Private Instance Fields

        /// <summary>
        /// The default style seems to be the closest to C# standards.
        /// </summary>
        private const String DEFAULT_STYLE = "kr";
        private const String DEFAULT_EXECUTABLE_NAME = "astyle.exe";
        private const String ASTYLE_OPTION_ENV_VAR = "ARTISTIC_STYLE_OPTIONS";
        private FileSet _sources = new FileSet();
        private bool _cleanUp = false;
        private Hashtable _commandOptions = new Hashtable();
        private string _commandLineArguments;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// A collection of command line option switches.
        /// </summary>
        public Hashtable CommandOptions {
            get { return _commandOptions;}
            set { this._commandOptions = value; }
        }

        /// <summary>
        /// Used to select the files to copy. 
        /// </summary>
        [BuildElement("fileset")]
        public virtual FileSet Sources {
            get { return _sources; }
            set { _sources = value; }
        }

        /// <summary>
        /// The command-line arguments for the program.
        /// </summary>
        [TaskAttribute("commandline")]
        public string CommandLineArguments {
            get { return this._commandLineArguments; }
            set { this._commandLineArguments = StringUtils.ConvertEmptyToNull(value); }
        }

        /// <summary>
        /// Indicate the preset style to use.
        ///     <list type="table">
        ///         <item>ansi
        ///             <code>
        ///                namespace foospace
        ///                {
        ///                    int Foo()
        ///                    {
        ///                        if (isBar)
        ///                        {
        ///                            bar();
        ///                            return 1;
        ///                        }
        ///                        else
        ///                            return 0;
        ///                    }
        ///                }
        ///             </code>
        ///         </item>
        ///         <item>kr ( Kernighan&amp;Ritchie )
        ///             <code>
        ///                namespace foospace {
        ///                    int Foo() {
        ///                        if (isBar) {
        ///                            bar();
        ///                            return 1;
        ///                        } else
        ///                            return 0;
        ///                    }
        ///                }
        ///             </code>
        ///         </item>
        ///         <item>linux
        ///             <code>
        ///                namespace foospace
        ///                {
        ///                        int Foo()
        ///                        {
        ///                                if (isBar) {
        ///                                        bar();
        ///                                        return 1;
        ///                                } else
        ///                                        return 0;
        ///                        }
        ///                }
        ///             </code>
        ///         </item>
        ///         <item>gnu
        ///             <code>
        ///                namespace foospace
        ///                {
        ///                    int Foo()
        ///                    {
        ///                        if (isBar)
        ///                        {
        ///                            bar();
        ///                            return 1;
        ///                        }
        ///                        else
        ///                        return 0;
        ///                    }
        ///                }
        ///             </code>
        ///         </item>
        ///         <item>java
        ///             <code>
        ///                class foospace {
        ///                    int Foo() {
        ///                        if (isBar) {
        ///                            bar();
        ///                            return 1;
        ///                        } else
        ///                            return 0;
        ///                    }
        ///                }
        ///             </code>
        ///         </item>
        ///         <item>NAnt
        ///             <code>
        ///                namespace foospace {
        ///                    class foo() {
        ///                #region Protected Static Fields
        ///                        private int Foo() {
        ///                            if (isBar) {
        ///                                bar();
        ///                                return 1;
        ///                            } else {
        ///                                return 0;
        ///                            }
        ///                        }
        ///                #endregion
        ///                }
        ///             </code>
        ///         </item>
        ///     </list>
        /// </summary>
        [TaskAttribute("style", Required=false)]
        public string Style {
            get { return ((Option) this.CommandOptions["style"]).Value; }
            set {
                if (value == "ansi" ||
                    value == "kr" ||
                    value == "linux" ||
                    value == "gnu" ||
                    value == "java") {
                    this.SetCommandOption("style", String.Format("style={0}", value), true);
                } else if (value == "NAnt") {
                    this.IndentNumSpaces = 4;
                    this.BracketsAttach = true;
                    this.PadOperators = true;
                    this.ConvertTabs = true;
                    this.IndentNamespaces = true;
                } else {
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                        "Unknown style '{0}'.", value), Location);
                }
            }
        }

        /// <summary>
        /// Astyle leaves the original files around, renamed with a different
        ///     suffix.  Setting this to <code>true</code>
        ///     will remove these files.
        /// </summary>
        [TaskAttribute("cleanup", Required=false)]
        public bool CleanUp {
            get {return this._cleanUp;}
            set {this._cleanUp = value;}
        }

        /// <summary>
        /// The suffix to append to original files, defaults to <c>.orig</c> 
        /// if not specified.
        /// </summary>
        [TaskAttribute("suffix", Required=false)]
        public string Suffix {
            get {
                if (this.CommandOptions.Contains("suffix")) {
                    return ((Option) this.CommandOptions["suffix"]).Value;
                } else {
                    return ".orig";
                }
            }
            set { 
                this.SetCommandOption("suffix", string.Format(CultureInfo.InvariantCulture,
                    "suffix=.{0}", value), true);
            }
        }

        /// <summary>
        /// Indicate the maximum number of spaces to indent relative to a 
        /// previous line.
        /// </summary>
        [TaskAttribute("indent-num-spaces", Required=false)]
        public int IndentNumSpaces {
            get { 
                return Convert.ToInt32(((Option) this.CommandOptions["indent-num-spaces"]).Value,
                    CultureInfo.InvariantCulture);
            }
            set {
                this.SetCommandOption("indent-num-spaces", string.Format(CultureInfo.InvariantCulture,
                    "indent=spaces={0}", value), true);
            }
        }

        /// <summary>
        /// Indicate that tabs should be used to indent sources.  The number 
        /// specified indicates the maximum number of spaces the tab character
        /// will represent.
        /// </summary>
        [TaskAttribute("indent-num-tabs", Required=false)]
        public int IndentNumTabs {
            get {
                return Convert.ToInt32(((Option) this.CommandOptions["indent-num-tabs"]).Value, 
                    CultureInfo.InvariantCulture);
            }
            set {
                this.SetCommandOption("indent-num-tabs", string.Format(CultureInfo.InvariantCulture,
                    "indent=tabs={0}", value), true);
            }
        }

        /// <summary>
        /// Indent using tab characters. Treat each tab as # spaces. Uses tabs as 
        /// indents in areas '--indent=tab' prefers to use spaces, such as 
        /// inside multi-line statements.
        /// </summary>
        [TaskAttribute("indent-num-tabs-force", Required=false)]
        public int IndentNumTabsForce {
            get {
                return Convert.ToInt32(((Option) this.CommandOptions["indent-num-tabs-force"]).Value,
                    CultureInfo.InvariantCulture);
            }
            set {
                this.SetCommandOption("indent-num-tabs-force", string.Format(CultureInfo.InvariantCulture,
                    "force-indent=tab={0}", value), true);
            }
        }

        /// <summary>
        /// <see langword="true" /> to convert tabs to spaces.
        /// </summary>
        [TaskAttribute("convert-tabs", Required=false)]
        [BooleanValidator()]
        public bool ConvertTabs {
            get { return ((Option) this.CommandOptions["convert-tabs"]).IfDefined; }
            set { this.SetCommandOption("convert-tabs", "convert-tabs", value); }
        }

        /// <summary>
        /// <see langword="true" /> if class statements should be indented.
        /// <code>
        /// 
        ///    The default:
        ///
        ///    class Foo
        ///    {
        ///    public:
        ///        Foo();
        ///        virtual ~Foo();
        ///    };
        ///
        ///    becomes:
        ///
        ///    class Foo
        ///    {
        ///        public:
        ///            Foo();
        ///            virtual ~Foo();
        ///    };
        ///    
        /// </code>
        /// </summary>
        [TaskAttribute("indent-classes", Required=false)]
        [BooleanValidator()]
        public bool IndentClass {
            get { return ((Option) this.CommandOptions["indent-classes"]).IfDefined; }
            set { this.SetCommandOption("indent-classes", "indent-classes", value); }
        }

        /// <summary>
        /// <see langword="true" /> if switch statements should be indented.
        /// <code>
        /// 
        ///        The default:
        ///
        ///        switch (foo)
        ///        {
        ///        case 1:
        ///            a += 2;
        ///            break;
        ///
        ///        default:
        ///            a += 2;
        ///            break;
        ///        }
        ///
        ///        becomes:
        ///
        ///        switch (foo)
        ///        {
        ///            case 1:
        ///                a += 2;
        ///                break;
        ///
        ///            default:
        ///                a += 2;
        ///                break;
        ///        }
        ///        
        /// </code>
        /// </summary>
        [TaskAttribute("indent-switches", Required=false)]
        [BooleanValidator()]
        public bool IndentSwitch {
            get { return ((Option) this.CommandOptions["indent-switches"]).IfDefined; }
            set { this.SetCommandOption("indent-switches", "indent-switches", value); }
        }

        /// <summary>
        /// <see langword="true" /> if case statements should be indented.
        /// <code>
        /// 
        ///    The default:
        ///
        ///    switch (foo)
        ///    {
        ///    case 1:
        ///        {
        ///            a += 2;
        ///            break;
        ///        }
        ///
        ///    default:
        ///        {
        ///            a += 2;
        ///            break;
        ///        }
        ///    }
        ///
        ///    becomes:
        ///
        ///    switch (foo)
        ///    {
        ///        case 1:
        ///        {
        ///            a += 2;
        ///            break;
        ///        }
        ///
        ///        default:
        ///        {
        ///            a += 2;
        ///            break;
        ///        }
        ///    }
        ///    
        /// </code>
        /// </summary>
        [TaskAttribute("indent-cases", Required=false)]
        [BooleanValidator()]
        public bool IndentCase {
            get { return ((Option) this.CommandOptions["indent-cases"]).IfDefined; }
            set { this.SetCommandOption("indent-cases", "indent-cases", value); }
        }

        /// <summary>
        /// <code>true</code> if bracket statements should be indented.
        /// <code>
        /// 
        ///    The default:
        ///
        ///    if (isFoo)
        ///    {
        ///        bar();
        ///    }
        ///    else
        ///    {
        ///        anotherBar();
        ///    }
        ///
        ///    becomes:
        ///
        ///    if (isFoo)
        ///        {
        ///        bar();
        ///        }
        ///    else
        ///        {
        ///        anotherBar();
        ///        }
        ///        
        /// </code>
        /// </summary>
        [TaskAttribute("indent-brackets", Required=false)]
        [BooleanValidator()]
        public bool IndentBracket {
            get { return ((Option) this.CommandOptions["indent-brackets"]).IfDefined; }
            set { this.SetCommandOption("indent-brackets", "indent-brackets", value); }
        }

        /// <summary>
        /// <see langword="true" /> if block statements should be indented.
        ///    The default:
        ///
        ///    if (isFoo)
        ///    {
        ///        bar();
        ///    }
        ///    else
        ///        anotherBar();
        ///
        ///    becomes:
        ///
        ///    if (isFoo)
        ///        {
        ///            bar();
        ///        }
        ///    else
        ///        anotherBar();
        /// </summary>
        [TaskAttribute("indent-blocks", Required=false)]
        [BooleanValidator()]
        public bool IndentBlock {
            get { return ((Option) this.CommandOptions["indent-blocks"]).IfDefined; }
            set { this.SetCommandOption("indent-blocks", "indent-blocks", value); }
        }

        /// <summary>
        /// <see langword="true" /> if namespace statements should be indented.
        /// <code>
        ///
        ///    The default:
        ///
        ///    namespace foospace
        ///    {
        ///    class Foo
        ///    {
        ///        public:
        ///            Foo();
        ///            virtual ~Foo();
        ///    };
        ///    }
        ///
        ///    becomes:
        ///
        ///    namespace foospace
        ///    {
        ///        class Foo
        ///        {
        ///            public:
        ///                Foo();
        ///                virtual ~Foo();
        ///        };
        ///    }
        ///
        /// </code>
        /// </summary>
        [TaskAttribute("indent-namespaces", Required=false)]
        [BooleanValidator()]
        public bool IndentNamespaces {
            get { return ((Option) this.CommandOptions["indent-namespaces"]).IfDefined; }
            set { this.SetCommandOption("indent-namespaces", "indent-namespaces", value); }
        }

        /// <summary>
        /// <see langword="true" /> if label statements should be indented.
        /// <code>
        /// 
        ///    The default:
        ///
        ///    int foospace()
        ///    {
        ///        while (isFoo)
        ///        {
        ///            ...
        ///            goto error;
        ///
        ///    error:
        ///            ...
        ///        }
        ///    }
        ///
        ///    becomes:
        ///
        ///    int foospace()
        ///    {
        ///        while (isFoo)
        ///        {
        ///            ...
        ///            goto error;
        ///
        ///        error:
        ///            ...
        ///        }
        ///    }
        ///
        /// </code>
        /// </summary>
        [TaskAttribute("indent-labels", Required=false)]
        [BooleanValidator()]
        public bool IndentLabels {
            get { return ((Option) this.CommandOptions["indent-labels"]).IfDefined; }
            set { this.SetCommandOption("indent-labels", "indent-labels", value); }
        }

        /// <summary>
        /// Indicate the maximum number of spaces to indent relative to a 
        /// previous line.
        /// </summary>
        [TaskAttribute("indent-max", Required=false)]
        public int IndentMax {
            get { 
                return Convert.ToInt32(((Option) this.CommandOptions["indent-max"]).Value,
                    CultureInfo.InvariantCulture);
            }
            set {
                this.SetCommandOption("indent-max", string.Format(CultureInfo.InvariantCulture,
                    "max-instatement-indent={0}", value), true);
            }
        }

        /// <summary>
        /// Indicate the maximum number of spaces to indent relative to a 
        /// previous line.
        /// </summary>
        [TaskAttribute("indent-min", Required=false)]
        public int IndentMin {
            get {
                return Convert.ToInt32(((Option) this.CommandOptions["indent-min"]).Value,
                    CultureInfo.InvariantCulture);
            }
            set {
                this.SetCommandOption("indent-min", string.Format(CultureInfo.InvariantCulture,
                    "min-conditional-indent={0}", value), true);
            }
        }

        /// <summary>
        /// <see langword="true" /> if empty lines should be filled with the 
        /// whitespace of the previous line.
        /// </summary>
        [TaskAttribute("fill-empty-lines", Required=false)]
        [BooleanValidator()]
        public bool FillEmptyLines {
            get { return ((Option) this.CommandOptions["fill-empty-lines"]).IfDefined; }
            set { this.SetCommandOption("fill-empty-lines", "fill-empty-lines", value); }
        }

        /// <summary>
        /// <see langword="true" /> if brackets should be put on a new line.
        /// <code>
        ///
        ///    if (isFoo)
        ///    {
        ///        bar();
        ///    }
        ///    else
        ///    {
        ///        anotherBar();
        ///    }
        ///    
        /// </code>
        /// </summary>
        [TaskAttribute("brackets-newline", Required=false)]
        [BooleanValidator()]
        public bool BracketsNewLine {
            get { return ((Option) this.CommandOptions["bracket-newline"]).IfDefined; }
            set { this.SetCommandOption("bracket-newline", "brackets=break", value); }
        }

        /// <summary>
        /// <see langword="true" /> if brackets should be attached.
        /// <code>
        /// 
        ///    if (isFoo){
        ///        bar();
        ///    } else {
        ///        anotherBar();
        ///    }
        /// 
        /// </code>
        /// </summary>
        [TaskAttribute("brackets-attach", Required=false)]
        [BooleanValidator()]
        public bool BracketsAttach {
            get { return ((Option) this.CommandOptions["brackets-attach"]).IfDefined; }
            set { this.SetCommandOption("brackets-attach", "brackets=attach", value); }
        }

        /// <summary>
        /// <see langword="true" /> if brackets should be put on a new line and 
        /// indented.
        /// <code>
        ///
        ///    namespace foospace
        ///    {
        ///        int Foo()
        ///        {
        ///            if (isBar) {
        ///                bar();
        ///                return 1;
        ///            } else
        ///                return 0;
        ///        }
        ///    }
        ///
        /// </code>
        /// </summary>
        [TaskAttribute("brackets-linux", Required=false)]
        [BooleanValidator()]
        public bool BracketsLinux {
            get { return ((Option) this.CommandOptions["brackets-linux"]).IfDefined; }
            set { this.SetCommandOption("brackets-linux", "brackets=linux", value); }
        }

        /// <summary>
        /// <see langword="true" /> if the line after a bracket (i.e. an else 
        /// statement after the closing if) should be placed on the next line.
        /// <code>
        /// 
        ///    if (isFoo){
        ///        bar();
        ///    }else {
        ///        anotherBar();
        ///    }
        ///
        ///    becomes:
        ///
        ///    if (isFoo) {
        ///        bar();
        ///    }
        ///    else {
        ///        anotherBar();
        ///    }
        ///    
        /// </code>
        /// </summary>
        [TaskAttribute("break-closing", Required=false)]
        [BooleanValidator()]
        public bool BreakClosing {
            get { return ((Option) this.CommandOptions["break-closing"]).IfDefined; }
            set { this.SetCommandOption("break-closing", "brackets=break-closing-headers", value); }
        }

        /// <summary>
        /// <see langword="true" /> to break block statements with an empty line.
        /// <code>
        ///
        ///    isFoo = true;
        ///    if (isFoo) {
        ///        bar();
        ///    } else {
        ///        anotherBar();
        ///    }
        ///    isBar = false;
        ///
        ///    becomes:
        ///
        ///    isFoo = true;
        ///
        ///    if (isFoo) {
        ///        bar();
        ///    } else {
        ///        anotherBar();
        ///    }
        ///
        ///    isBar = false;
        ///
        /// </code>
        /// </summary>
        [TaskAttribute("break-blocks", Required=false)]
        [BooleanValidator()]
        public bool BreakBlocks {
            get { return ((Option) this.CommandOptions["break-blocks"]).IfDefined; }
            set { this.SetCommandOption("break-blocks", "break-blocks", value); }
        }

        /// <summary>
        /// <see langword="true" /> to break all block statements, even on 
        /// nested ifs with an empty line.
        /// <code>
        ///
        ///    isFoo = true;
        ///    if (isFoo) {
        ///        bar();
        ///    } else {
        ///        anotherBar();
        ///    }
        ///    isBar = false;
        ///
        ///    becomes:
        ///
        ///    isFoo = true;
        ///
        ///    if (isFoo) {
        ///        bar();
        ///
        ///    } else {
        ///        anotherBar();
        ///    }
        ///
        ///    isBar = false;
        ///
        /// </code>
        /// </summary>
        [TaskAttribute("break-blocks-all", Required=false)]
        [BooleanValidator()]
        public bool BreakBlocksAll {
            get { return ((Option) this.CommandOptions["break-blocks-all"]).IfDefined; }
            set { this.SetCommandOption("break-blocks-all", "break-blocks=all", value); }
        }

        /// <summary>
        /// <see langword="true" /> to put the if component of an else if on a 
        /// new line.
        /// <code>
        ///
        ///    if (isFoo) {
        ///        bar();
        ///    } else if (isBar()){
        ///        anotherBar();
        ///    }
        ///
        ///    becomes:
        ///
        ///    if (isFoo) {
        ///        bar();
        ///    } else
        ///        if (isBar()){
        ///            anotherBar();
        ///        } 
        ///
        /// </code>
        /// 
        /// </summary>
        [TaskAttribute("break-elseif", Required=false)]
        [BooleanValidator()]
        public bool BreakElseif {
            get { return ((Option) this.CommandOptions["break-elseif"]).IfDefined; }
            set { this.SetCommandOption("break-elseif", "break-elseifs", value); }
        }

        /// <summary>
        /// <see langword="true" /> to pad operators with a space.
        /// <code>
        /// 
        ///    if (isFoo)
        ///        a = bar((b-c)*a,*d--);
        ///
        ///    becomes:
        ///
        ///    if (isFoo)
        ///        a = bar((b - c) * a, *d--);
        ///
        /// </code>
        /// </summary>
        [TaskAttribute("pad-operators", Required=false)]
        [BooleanValidator()]
        public bool PadOperators {
            get { return ((Option) this.CommandOptions["pad-operators"]).IfDefined; }
            set { this.SetCommandOption("pad-operators", "pad=oper", value); }
        }

        /// <summary>
        /// <see langword="true" /> to pad parenthesis with a space.
        /// <code>
        ///
        ///    if (isFoo)
        ///        a = bar((b-c)*a,*d--);
        ///
        ///    becomes:
        ///
        ///    if ( isFoo )
        ///        a = bar( ( b-c )*a, *d-- );
        ///
        /// </code>
        /// </summary>
        [TaskAttribute("pad-parenthesis", Required=false)]
        [BooleanValidator()]
        public bool PadParenthesis {
            get { return ((Option) this.CommandOptions["pad-parenthesis"]).IfDefined; }
            set { this.SetCommandOption("pad-parenthesis", "pad=paren", value); }
        }

        /// <summary>
        /// <see langword="true" /> to pad operators and parenthesis.
        /// <code>
        /// 
        ///    if (isFoo)
        ///        a = bar((b-c)*a,*d--);
        ///
        ///    becomes:
        ///
        ///    if ( isFoo )
        ///        a = bar( ( b - c ) * a, *d-- );
        ///
        /// </code>
        /// </summary>
        [TaskAttribute("pad-all", Required=false)]
        [BooleanValidator()]
        public bool PadAll {
            get { return ((Option) this.CommandOptions["pad-all"]).IfDefined; }
            set { this.SetCommandOption("pad-all", "pad=all", value); }
        }

        /// <summary>
        /// <see langword="true" /> to keep complex statements on the same line.
        /// <code>
        /// 
        ///    if (isFoo)
        ///    {  
        ///        isFoo = false; cout &lt;&lt; isFoo &lt;&lt; endl;
        ///    }
        ///
        ///    remains as is.
        ///
        ///    if (isFoo) DoBar();
        ///
        ///    remains as is.
        ///
        /// </code>
        /// </summary>
        [TaskAttribute("nobreak-complex", Required=false)]
        [BooleanValidator()]
        public bool NoBreakComplex {
            get { return ((Option) this.CommandOptions["nobreak-complex"]).IfDefined; }
            set { this.SetCommandOption("nobreak-complex", "one-line=keep-statements", value); }
        }

        /// <summary>
        /// <see langword="true" /> to keep single line statements on the same line.
        /// <code>
        ///
        ///    if (isFoo)
        ///    { isFoo = false; cout &lt;&lt; isFoo &lt;&lt; endl; }
        ///
        ///    remains as is.
        ///
        /// </code>
        /// </summary>
        [TaskAttribute("nobreak-singlelineblocks", Required=false)]
        [BooleanValidator()]
        public bool NoBreakSingleLineBlocks {
            get { return ((Option) this.CommandOptions["nobreak-singlelineblocks"]).IfDefined; }
            set { this.SetCommandOption("nobreak-singlelineblocks", "one-line=keep-blocks", value); }
        }

        #endregion Public Instance Properties

        #region Override implementation of ExternalProgramBase

        /// <summary>
        /// Gets the command-line arguments for the external program.
        /// </summary>
        /// <value>
        /// The command-line arguments for the external program.
        /// </value>
        public override string ProgramArguments {
            get { return this.CommandLineArguments; }
        }

        /// <summary>
        /// Build up the command line arguments, determine which executable is 
        /// being used and find the path to that executable and set the working 
        /// directory.
        /// </summary>
        /// <param name="process">The process to prepare.</param>
        protected override void PrepareProcess (Process process) {
            // Although a global property can be set, take the property closest
            // to the task execution, which is the attribute on the task itself.
            this.AppendCommandOptions();
            this.AppendFiles();

            base.PrepareProcess(process);
            process.StartInfo.FileName = DEFAULT_EXECUTABLE_NAME;

            Log(Level.Verbose, "Working directory: {0}", 
                process.StartInfo.WorkingDirectory);
            Log(Level.Verbose, "Executable: {0}", process.StartInfo.FileName);
            Log(Level.Verbose, "Arguments: {0}", process.StartInfo.Arguments);
        }

        protected override void ExecuteTask() {
            base.ExecuteTask();

            if (this.CleanUp) {
                foreach (string pathname in this.Sources.FileNames) {
                    String originalFile = pathname + this.Suffix;
                    if (File.Exists(originalFile)) {
                        File.Delete(originalFile);
                    }
                }
            }
        }

        #endregion Override implementation of ExternalProgramBase

        #region Protected Instance Methods

        /// <summary>
        /// Adds a new command option if none exists.  If one does exist then
        /// the use switch is toggled on or of.
        /// </summary>
        /// <param name="name">The common name of the option.</param>
        /// <param name="value">The option value or command line switch of the option.</param>
        /// <param name="on"><see langword="true" /> if the option should be appended to the commandline, otherwise <see langword="false" />.</param>
        protected void SetCommandOption(string name, String value, bool on) {
            Option option;
            if (this.CommandOptions.Contains(name)) {
                option = (Option) this.CommandOptions[name];
            } else {
                option = new Option();
                option.OptionName = name;
                option.Value = value;
                this.CommandOptions.Add(name, option);
            } 
            option.IfDefined = on;
        }

        #endregion Protected Instance Methods

        #region Private Instance Methods

        /// <summary>
        /// Append the command line options or commen names for the options
        /// to the generic options collection.  This is then piped to the
        /// command line as a switch.
        /// </summary>
        private void AppendCommandOptions () {
            foreach (Option option in this.CommandOptions.Values) {
                if (!option.IfDefined || option.UnlessDefined) {
                    // skip option
                    continue;
                }
                this.AddArg(option.Value);
            }
        }

        private void AddArg (String arg) {
            if (arg.IndexOf("-") != 0) {
                Arguments.Add(new Argument(string.Format("--{0}",
                    arg)));
            } else {
                Arguments.Add(new Argument(string.Format("{0}",
                    arg)));
            }
        }

        /// <summary>
        /// Append the files specified in the fileset to the command line argument.
        /// </summary>
        private void AppendFiles () {
            foreach (string pathname in this.Sources.FileNames) {
                Arguments.Add(new Argument('\"' + pathname + '\"'));
            }
        }

        #endregion Private Instance Methods
    }
}
