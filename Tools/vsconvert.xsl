<?xml version="1.0" encoding="UTF-8" ?>
<!--
//
// NAntContrib
// Copyright (C) 2003 Gordon Weakliem
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

// Gordon Weakliem (gweakliem@yahoo.com)
-->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" version="1.0" indent="yes" />
  <xsl:template match="VisualStudioProject">
    <project default="test">
      <xsl:attribute name="name">
        <xsl:choose>
          <xsl:when test="@Name">
            <xsl:value-of select="@Name" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="*/Build/Settings/@AssemblyName" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <xsl:choose>
        <xsl:when test="@ProjectType='Visual C++'">
          <xsl:call-template name="Cxx" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates />
        </xsl:otherwise>
      </xsl:choose>
    </project>
  </xsl:template>
  <xsl:template match="CSHARP">
    <xsl:call-template name="prolog" />
    <xsl:apply-templates select="Build">
      <xsl:with-param name="compiler" select="'csc'" />
    </xsl:apply-templates>
    <xsl:call-template name="epilog" />
  </xsl:template>
  <xsl:template match="VisualBasic">
    <xsl:call-template name="prolog" />
    <xsl:apply-templates select="Build">
      <xsl:with-param name="compiler" select="'vbc'" />
    </xsl:apply-templates>
    <xsl:call-template name="epilog" />
  </xsl:template>
  <xsl:template match="Build">
    <xsl:param name="compiler" />
    <xsl:apply-templates select="Settings" />
    <xsl:apply-templates mode="tlbimp" select="References" />
    <target name="compile" description="Compile project">
      <xsl:attribute name="depends">
        <xsl:value-of select="'init'" />
        <xsl:for-each select="References/Reference[@WrapperTool='tlbimp']">,<xsl:value-of select="@Name" /></xsl:for-each>
      </xsl:attribute>
      <xsl:element name="{$compiler}">
        <xsl:attribute name="target">
          <xsl:value-of select="'${target.type}'" />
        </xsl:attribute>
        <xsl:attribute name="output">
          <xsl:value-of select="'${dir.output}\${project.output}'" />
        </xsl:attribute>
        <xsl:attribute name="debug">
          <xsl:value-of select="'${debug}'" />
        </xsl:attribute>
        <xsl:attribute name="define">
          <xsl:value-of select="'${define}'" />
        </xsl:attribute>
        <xsl:if test="$compiler != 'vbc'">
        <xsl:attribute name="doc">
          <xsl:value-of select="'${doc}'" />
        </xsl:attribute>
        </xsl:if>
        <xsl:if test="$compiler='vbc'">
        <xsl:attribute name="optioncompare">
          <xsl:value-of select="'${vbc.optionCompare}'" />
        </xsl:attribute>
        <xsl:attribute name="optionexplicit">
          <xsl:value-of select="'${vbc.optionExplicit}'" />
        </xsl:attribute>
        <xsl:attribute name="optionstrict">
          <xsl:value-of select="'${vbc.optionStrict}'" />
        </xsl:attribute>
        <xsl:attribute name="main">
			<xsl:value-of select="Settings/@StartupObject"/>
        </xsl:attribute>
        </xsl:if>
        <xsl:attribute name="removeintchecks">
          <xsl:value-of select="'${removeintchecks}'" />
        </xsl:attribute>
        <xsl:attribute name="rootnamespace">
          <xsl:value-of select="'${rootNamespace}'" />
        </xsl:attribute>
        <xsl:if test="string-length(Settings/@ApplicationIcon)>0">
          <xsl:attribute name="win32icon">
            <xsl:value-of select="Settings/@ApplicationIcon" />
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="$compiler = 'csc'">
        <xsl:element name="arg">
          <xsl:attribute name="value">
            <xsl:value-of select="'${unsafe}'" />
          </xsl:attribute>
        </xsl:element>
        </xsl:if>
        <xsl:apply-templates select="../Files" />
        <xsl:apply-templates select="References" />
		<xsl:apply-templates select="Imports" />        
      </xsl:element>
    </target>
  </xsl:template>
  <!-- boilerplate prolog code -->
  <!-- todo: could move this so that context is Build/Settings -->
  <xsl:template name="prolog">
    <property name="debug" value="false" />
    <property name="dir.output" value=".\bin" />
    <property name="dir.lib" value="..\lib" />
    <property name="dir.dist" value="..\dist" />
  </xsl:template>
  <!-- boilerplate epilog code -->
  <xsl:template name="epilog">
    <target name="build" description="Do an incremental build" depends="init,compile,test">
      <copy file="${{dir.output}}\${{project.output}}" todir="${{dir.lib}}" />
    </target>
    <target name="test" depends="init,compile"></target>
    <xsl:variable name="lib" select="Build/Settings/@OutputType='Library' or Build/Settings/@OutputType='Module'" />
    <target name="clean" depends="init" description="Delete output of a build">
      <delete file="${{dir.output}}\${{project.output}}" verbose="true" failonerror="false" />
      <delete file="${{dir.output}}\${{project.FormalName}}.pdb" verbose="true" failonerror="false" />
      <delete file="${{doc}}" verbose="true" failonerror="false" />
    </target>
    <!-- deploy and package targets only if it's an exe or web app -->
    <xsl:if test="@ProjectType='Web' or not($lib)">
      <target name="package" depends="init" description="Create a redistributable package">
        <delete failonerror="false">
          <fileset basedir="${{dist.name}}">
            <include name="**" />
          </fileset>
        </delete>
        <mkdir dir="${{dist.name}}" />
        <copy todir="${{dist.name}}">
          <fileset basedir="${{nant.project.basedir}}">
            <!-- scan the filset to see if there's content files -->
            <xsl:apply-templates select="Files/Include/File[@BuildAction='Content' or @DeploymentContent='TRUE']" />
          </fileset>
        </copy>
        <xsl:variable name="distdir">${dist.name}<xsl:if test="@ProjectType='Web'">/bin</xsl:if></xsl:variable>
        <mkdir dir="{$distdir}" />
        <copy todir="{$distdir}">
          <fileset basedir="${{dir.lib}}">
            <!-- include the output directory -->
            <include name="${{project.output}}" />
            <xsl:for-each select="References/Reference[@Project|@Guid]">
              <include name="{@Name}.dll" />
            </xsl:for-each>
          </fileset>
        </copy>
      </target>
    </xsl:if>
  </xsl:template>
  <xsl:template match="Settings">
    <property name="target.type">
      <xsl:attribute name="value">
        <xsl:choose>
          <xsl:when test="@OutputType='Library'">library</xsl:when>
          <xsl:when test="@OutputType='WinExe'">winexe</xsl:when>
          <xsl:when test="@OutputType='Module'">module</xsl:when>
          <xsl:otherwise>exe</xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
    </property>
    <property name="project.FormalName" value="{@AssemblyName}" />
    <xsl:variable name="is-library" select="@OutputType='Library' or @OutputType='Module'" />
    <xsl:variable name="output-file-name">
      <xsl:choose>
        <xsl:when test="$is-library">${project.FormalName}.dll</xsl:when>
        <xsl:otherwise>${project.FormalName}.exe</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <target name="init" description="Initialize properties for the build">
      <xsl:attribute name="depends">
        <xsl:for-each select="Config">
          <xsl:if test="position()>1">,</xsl:if>
          <xsl:call-template name="config-name" />
        </xsl:for-each>
      </xsl:attribute>
      <tstamp />
      <mkdir dir="${{dir.output}}" />
      <mkdir dir="${{dir.lib}}" />
      <mkdir dir="${{dir.dist}}" />
      <property name="project.output" value="{$output-file-name}" />
      <!-- don't like the name but dir.dist is already being used -->
      <property name="dist.name" value="${{dir.dist}}\${{project.FormalName}}" />
      <!-- VB Only settings.  Won't hurt C# stuff, but messy. -->
      <property name="vbc.optionCompare" value="{@OptionCompare}" />
      <property name="vbc.optionExplicit">
        <xsl:attribute name="value">
          <xsl:value-of select="string(boolean(@OptionExplicit='On'))" />
        </xsl:attribute>
      </property>
      <property name="vbc.optionStrict">
        <xsl:attribute name="value">
          <xsl:value-of select="string(@OptionStrict='On')" />
        </xsl:attribute>
      </property>
      <property name="rootNamespace" value="{@RootNamespace}" />
    </target>
    <xsl:apply-templates select="Config" />
  </xsl:template>
  <xsl:template name="config-name">init-<xsl:value-of select="@Name" /></xsl:template>
  <xsl:template match="Config">
    <!-- create targets for each configuration listed -->
    <target>
      <xsl:attribute name="name">
        <xsl:call-template name="config-name" />
      </xsl:attribute>
      <xsl:choose>
        <xsl:when test="@DebugSymbols='true'">
          <xsl:attribute name="if">${debug}</xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="unless">${debug}</xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
      <property name="define" value="{@DefineConstants}" />
      <property name="optimize" value="{@Optimize}" />
      <property name="incremental" value="{@IncrementalBuild}" />
      <property name="unsafe">
        <xsl:attribute name="value">
          <xsl:choose>
            <xsl:when test="@AllowUnsafeBlocks='true'">/unsafe+</xsl:when>
            <xsl:otherwise>/unsafe-</xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </property>
      <property name="debug" value="{@DebugSymbols}" />
      <xsl:choose>
        <xsl:when test="string-length(@DocumentationFile)>0">
          <property name="doc" value="${dir.lib}/{@DocumentationFile}" />
        </xsl:when>
        <xsl:otherwise>
          <property name="doc" value="" />
        </xsl:otherwise>
      </xsl:choose>
      <property name="removeintchecks" value="{@RemoveIntegerChecks}" />
    </target>
  </xsl:template>
  <xsl:template match="Files">
    <sources>
      <xsl:apply-templates select="Include/File[@BuildAction='Compile']" />
    </sources>
    <xsl:if test="count(Include/File[@BuildAction='EmbeddedResource']) &gt; 0">
      <resources>
        <xsl:apply-templates select="Include/File[@BuildAction='EmbeddedResource']" />
      </resources>
    </xsl:if>
  </xsl:template>
  <xsl:template match="File">
    <include name="{@RelPath}" />
  </xsl:template>
  <!-- rules to handle importing COM components via tlbimp -->
  <xsl:template match="References" mode="tlbimp">
    <xsl:if test="count(Reference[@WrapperTool='tlbimp'])>0">
      <xsl:apply-templates mode="tlbimp" select="Reference[@WrapperTool='tlbimp']" />
    </xsl:if>
  </xsl:template>
  <xsl:template match="Reference" mode="tlbimp">
    <target name="{@Name}" depends="init">
      <script language="C#">
      <code>
      [System.Runtime.InteropServices.DllImport( "oleaut32.dll", 
        CharSet = System.Runtime.InteropServices.CharSet.Auto, 
        PreserveSig = false,SetLastError=true )]
      private static extern void QueryPathOfRegTypeLib( 
        ref Guid guid,          
        Int16 wVerMajor, 
        Int16 wVerMinor, 
        Int32 lcid, 
        [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.VBByRefStr)] ref StringBuilder lpbstrPathName);
      public static void ScriptMain(Project project) 
      {
        try
        {
          StringBuilder pathResult = new StringBuilder(1024);
          Guid g = new Guid("<xsl:value-of select="@Guid"/>");
          QueryPathOfRegTypeLib(ref g,
            <xsl:value-of select="@VersionMajor"/>,
            <xsl:value-of select="@VersionMinor"/>,
            <xsl:value-of select="@Lcid"/>, 
            ref pathResult);

          project.Properties["<xsl:value-of select="@Name"/>.path"] = pathResult.ToString();
        }
        catch (Exception e)
        {
          throw new SourceForge.NAnt.BuildException( String.Format("Error {0} getting typelib path for guid <xsl:value-of select="@Guid"/>",e.Message),e);
        }
      }
      </code>
      </script>
      <tlbimp output="${{dir.lib}}/{@Name}.dll" typelib="${{{@Name}.path}}" />
    </target>
  </xsl:template>
  <xsl:template match="References">
    <references>
      <xsl:apply-templates />
    </references>
  </xsl:template>
  <xsl:template match="Reference">
    <xsl:choose>
      <!-- straight assembly reference -->
      <xsl:when test="@AssemblyName">
        <include name="{@AssemblyName}.dll" />
      </xsl:when>
      <!-- project references -->
      <xsl:when test="@Project">
        <!-- we assume that the project being referenced has been built
        already and the compiled dll is sitting in the lib directory. -->
        <include name="${{dir.lib}}/{@Name}.dll" />
      </xsl:when>
      <!-- COM object reference -->
      <xsl:when test="@Guid">
        <!-- the tlbimp task will put the interop lib into the output directory -->
        <include name="${{dir.lib}}/{@Name}.dll" />
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="Cxx">
    <property name="project.FormalName" value="{@Name}"/>
    <property name="dir.obj" value="obj"/>
    <xsl:call-template name="prolog" />
    <xsl:apply-templates select="Configurations" />
    <target name="compile" depends="init">
      <xsl:for-each select="Files/File[Tool/@CommandLine]">
        <echo message="{FileConfiguration/Tool/@Description}"/>
        <exec program="{FileConfiguration/Tool/@CommandLine}"/>
      </xsl:for-each>
      <xsl:for-each select="Files/Filter[@Name='Resource Files']/File" >
        <xsl:variable name="resname" select="substring-before(@RelativePath,'.')"/>
        <exec program="rc.exe" commandline="/fo ${{dir.obj}}/{$resname}.res {$resname}.rc" />
      </xsl:for-each>
      <cl outputdir="${{dir.obj}}" debug="${{debug}}" defines="${{define}}" subsystem="windows" verbose="true" options="${{cl.args}}" eh="true">
        <sources>
          <xsl:apply-templates select="Files/Filter[@Name='Source Files']" />
        </sources>
        <headers>
          <xsl:apply-templates select="Files/Filter[@Name='Header Files']" />
        </headers>
      </cl>
      <link output="${{dir.output}}${{project.output}}" options="${{link.opts}} ${{link.libs}}" verbose="true">
        <sources>
          <xsl:for-each select="Files/Filter[@Name='Resource Files']/File" >
            <include name="${{dir.obj}}/substring-before(@RelativePath,'.').res" />
          </xsl:for-each>
          <xsl:for-each select="Files/Filter[@Name='Source Files']/File" >
            <include name="${{dir.obj}}/{substring-before(@RelativePath,'.')}.obj"/>
          </xsl:for-each>
        </sources>
        <libdirs>
          <include name="${framework.lib}" />
        </libdirs>
      </link>
    </target>
    <xsl:call-template name="epilog" />
  </xsl:template>
  <xsl:template match="Configurations">
    <target name="init" description="Initialize properties for the build">
      <xsl:attribute name="depends">
        <xsl:for-each select="Configuration">
          <xsl:if test="position()>1">,</xsl:if>
          <xsl:value-of select="concat(@Name,'-init')" />
        </xsl:for-each>
      </xsl:attribute>
      <tstamp />
      <sysinfo/>
      <mkdir dir="${{dir.output}}" />
      <mkdir dir="${{dir.lib}}" />
      <mkdir dir="${{dir.obj}}" />
      <mkdir dir="${{dir.dist}}" />
      <!-- don't like the name but dir.dist is already being used -->
      <property name="dist.name" value="${{dir.dist}}\${{project.FormalName}}" />
    </target>
    <xsl:apply-templates select="Configuration"/>
  </xsl:template>
  <xsl:template match="Configuration">
    <target name="{@Name}-init">
      <xsl:choose>
        <xsl:when test="Tool[@Name='VCLinkerTool']/@ProgramDatabaseFile">
          <xsl:attribute name="if">${debug}</xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="unless">${debug}</xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
      <property name="cl.define" value="{Tool[@Name='VCCLCompilerTool']/@PreprocessorDefinitions}" />
      <property name="cl.warn" value="{Tool[@Name='VCCLCompilerTool']/@WarningLevel}" />
      <property name="cl.inc" value="{Tool[@Name='VCCLCompilerTool']/@MinimalRebuild}" />
      <property name="cl.args" value="/D={Tool[@Name='VCCLCompilerTool']/@PreProcessorDefinitions}" />
      <property name="link.dep" value="{Tool[@Name='VCLinkerTool']/@AdditionalDependencies}" />
      <property name="link.opts" value="" />
      <property name="project.output" value="{substring-after(Tool[@Name='VCLinkerTool']/@OutputFile,'$(OutDir)')}" />
      <property name="link.libdir" value="{Tool[@Name='VCLinkerTool']/@AdditionalLibraryDirectories}" />
      <property name="link.libs" value="{Tool[@Name='VCLinkerTool']/@AdditionalDependencies} kernel32.lib user32.lib advapi32.lib"/>
    </target>
  </xsl:template>
  <xsl:template match="Filter">
    <xsl:for-each select="File">
    <include name="{@RelativePath}" />
    </xsl:for-each>
  </xsl:template>
  <xsl:template match="Imports">
	<imports>
		<xsl:apply-templates />
	</imports>
  </xsl:template>
  <xsl:template match="Import">
	<import name="{@Namespace}"/>
  </xsl:template>
  <xsl:template name="expand-vsmacro">
    <xsl:param name="str"/>
    <xsl:param name="fileName"/>
    <xsl:value-of select="substring-before($str,'$(InputFileName)')"/>
    <xsl:value-of select="$fileName"/>
    <xsl:value-of select="substring-after($str,'$(InputFileName)')"/>
  </xsl:template>
</xsl:stylesheet>