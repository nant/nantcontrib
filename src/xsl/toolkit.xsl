<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:param name="nant.filename" />
<xsl:param name="nant.version" />
<xsl:param name="nant.project.name" />
<xsl:param name="nant.project.buildfile" />
<xsl:param name="nant.project.basedir" />
<xsl:param name="nant.project.default" />
<xsl:param name="sys.os" />
<xsl:param name="sys.os.platform" />
<xsl:param name="sys.os.version" />
<xsl:param name="sys.clr.version" />

<!-- key used to select testcase classnames -->
<xsl:key name="classnameKey" match="testcase" use="@classname" />


<!--
   This XSL File is based on the toolkit.xsl
   template created by Erik Hatcher fot Ant's JUnitReport

   Modified by Tomas Restrepo (tomasr@mvps.org) for use
   with NUnitReport
-->

<!--
    format a number in to display its value in percent
    @param value the number to format
-->
<xsl:template name="display-time">
    <xsl:param name="value" />
    <xsl:value-of select="format-number($value,'0.000')" />
</xsl:template>

<!--
    format a number in to display its value in percent
    @param value the number to format
-->
<xsl:template name="display-percent">
    <xsl:param name="value"/>
    <xsl:value-of select="format-number($value,'0.00%')"/>
</xsl:template>

<!--
    transform string like a.b.c to ../../../
    @param path the path to transform into a descending directory path
-->
<xsl:template name="path">
    <xsl:param name="path"/>
    <xsl:if test="contains($path,'.')">
        <xsl:text>../</xsl:text>    
        <xsl:call-template name="path">
            <xsl:with-param name="path"><xsl:value-of select="substring-after($path,'.')"/></xsl:with-param>
        </xsl:call-template>    
    </xsl:if>
    <xsl:if test="not(contains($path,'.')) and not($path = '')">
        <xsl:text>../</xsl:text>    
    </xsl:if>   
</xsl:template>

<!--
    template that will convert a carriage return into a br tag
    @param word the text from which to convert CR to BR tag
-->
<xsl:template name="br-replace">
    <xsl:param name="word"/>
    <xsl:choose>
        <xsl:when test="contains($word,'&#xA;')">
            <xsl:value-of select="substring-before($word,'&#xA;')"/>
            <br/>
            <xsl:call-template name="br-replace">
                <xsl:with-param name="word" select="substring-after($word,'&#xA;')"/>
            </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
            <xsl:value-of select="$word"/>
        </xsl:otherwise>
    </xsl:choose>
</xsl:template>


<!-- 
        =====================================================================
        classes summary header
        =====================================================================
-->
<xsl:template name="header">
    <xsl:param name="path"/>
    <h1>Unit Tests Results - <xsl:value-of select="$nant.project.name"/></h1>
    <table width="100%">
    <tr>
       <td align="left">
          Generated: <xsl:value-of select="@created"/> -
          <a href="#envinfo">Environment Information</a>
       </td>
        <td align="right">Designed for use with 
           <a href='http://nunit.sourceforge.net/'>NUnit</a> and 
           <a href='http://nant.sourceforge.net/'>NAnt</a>.
        </td>
    </tr>
    </table>
    <hr size="1"/>
</xsl:template>

<xsl:template name="summaryHeader">
    <tr valign="top" class="TableHeader">
        <td><b>Tests</b></td>
        <td><b>Failures</b></td>
        <td><b>Errors</b></td>
        <td><b>Success Rate</b></td>
        <td nowrap="nowrap"><b>Time(s)</b></td>
    </tr>
</xsl:template>

<!-- 
        =====================================================================
        package summary header
        =====================================================================
-->
<xsl:template name="packageSummaryHeader">
    <tr class="TableHeader" valign="top">
        <td width="75%"><b>Name</b></td>
        <td width="5%"><b>Tests</b></td>
        <td width="5%"><b>Errors</b></td>
        <td width="5%"><b>Failures</b></td>
        <td width="10%" nowrap="nowrap"><b>Time</b></td>
    </tr>
</xsl:template>

<!-- 
        =====================================================================
        classes summary header
        =====================================================================
-->
<xsl:template name="classesSummaryHeader">
    <tr class="TableHeader" valign="top" style="height: 4px">
        <td width="85%"><b>Name</b></td>
        <td width="10%"><b>Status</b></td>
        <td width="5%" nowrap="nowrap"><b>Time</b></td>
    </tr>
</xsl:template>

<!-- 
        =====================================================================
        Write the summary report
        It creates a table with computed values from the document:
        User | Date | Environment | Tests | Failures | Errors | Rate | Time
        Note : this template must call at the testsuites level
        =====================================================================
-->
    <xsl:template name="summary">
        <h2>Summary</h2>
        <xsl:variable name="testCount" select="sum(./testsuite/@tests)"/>
        <xsl:variable name="errorCount" select="sum(./testsuite/@errors)"/>
        <xsl:variable name="failureCount" select="sum(./testsuite/@failures)"/>
        <xsl:variable name="timeCount" select="sum(./testsuite/@time)"/>
        <xsl:variable name="successRate" select="($testCount - $failureCount - $errorCount) div $testCount"/>
        
        <table border="0" class="DetailTable" width="95%">
        <xsl:call-template name="summaryHeader"/>
        <tr valign="top">
            <xsl:attribute name="class">
                <xsl:choose>
                    <xsl:when test="$failureCount &gt; 0">Failure</xsl:when>
                    <xsl:when test="$errorCount &gt; 0">Error</xsl:when>
                    <xsl:otherwise>Pass</xsl:otherwise>
                </xsl:choose>           
            </xsl:attribute>        
            <td><xsl:value-of select="$testCount"/></td>
            <td><xsl:value-of select="$failureCount"/></td>
            <td><xsl:value-of select="$errorCount"/></td>
            <td>
                <xsl:call-template name="display-percent">
                    <xsl:with-param name="value" select="$successRate"/>
                </xsl:call-template>
            </td>
            <td>
                <xsl:call-template name="display-time">
                    <xsl:with-param name="value" select="$timeCount"/>
                </xsl:call-template>
            </td>
        </tr>
        </table>
        Note: <i>failures</i> are anticipated and checked for with assertions while <i>errors</i> are unanticipated.
    </xsl:template>

<!-- 
        =====================================================================
        testcase report
        =====================================================================
-->
<xsl:template match="testcase">
   <xsl:variable name="result">
            <xsl:choose>
              <xsl:when test="./failure">Failure</xsl:when>
                <xsl:when test="./error">Error</xsl:when>
                <xsl:otherwise>Pass</xsl:otherwise>
            </xsl:choose>
   </xsl:variable>
   <xsl:variable name="newid" select="generate-id(@name)" />
    <TR valign="top">
        <xsl:attribute name="class"><xsl:value-of select="$result"/></xsl:attribute>
       <xsl:if test="$result != &quot;Pass&quot;">
          <xsl:attribute name="onclick">javascript:toggle(<xsl:value-of select="$newid"/>)</xsl:attribute>
          <xsl:attribute name="style">cursor: hand;</xsl:attribute>
       </xsl:if>
        
        <TD><xsl:value-of select="substring-before(./@name, '(')"/></TD>
        <td><xsl:value-of select="$result"/></td>
        <td>
            <xsl:call-template name="display-time">
                <xsl:with-param name="value" select="@time"/>
            </xsl:call-template>                
        </td>
    </TR>
    <xsl:if test="$result != &quot;Pass&quot;">
       <tr style="display: block;">
          <xsl:attribute name="id">
             <xsl:value-of select="$newid"/>
          </xsl:attribute>
          <td colspan="3" class="FailureDetail">
             <xsl:apply-templates select="./failure"/>
             <xsl:apply-templates select="./error"/>
         </td>        
      </tr>
    </xsl:if>
</xsl:template>

<!-- Note : the below template error and failure are the same style
            so just call the same style store in the toolkit template -->
<xsl:template match="failure">
    <xsl:call-template name="display-failures"/>
</xsl:template>

<xsl:template match="error">
    <xsl:call-template name="display-failures"/>
</xsl:template>

<!-- Style for the error and failure in the tescase template -->
<xsl:template name="display-failures">
    <xsl:choose>
        <xsl:when test="not(@message)">N/A</xsl:when>
        <xsl:otherwise>
            <xsl:value-of select="@message"/>
        </xsl:otherwise>
    </xsl:choose>
    <!-- display the stacktrace -->
    <code>
        <p/>
        <xsl:call-template name="br-replace">
            <xsl:with-param name="word" select="."/>
        </xsl:call-template>
    </code>
    <!-- the later is better but might be problematic for non-21" monitors... -->
    <!--pre><xsl:value-of select="."/></pre-->
</xsl:template>


<!-- 
        =====================================================================
        Environtment Info Report
        =====================================================================
-->
<xsl:template name="envinfo">
   <a name="envinfo"></a>
    <h2>Environment Information</h2>
    <table border="0" class="DetailTable" width="95%">
       <tr class="EnvInfoHeader">
          <td>Property</td>
          <td>Value</td>
       </tr>
       <tr class="EnvInfoRow">
          <td>NAnt Location</td>
          <td><xsl:value-of select="$nant.filename"/></td>
       </tr>
       <tr class="EnvInfoRow">
          <td>NAnt Version</td>
          <td><xsl:value-of select="$nant.version"/></td>
       </tr>
       <tr class="EnvInfoRow">
          <td>Buildfile</td>
          <td><xsl:value-of select="$nant.project.buildfile"/></td>
       </tr>
       <tr class="EnvInfoRow">
          <td>Base Directory</td>
          <td><xsl:value-of select="$nant.project.basedir"/></td>
       </tr>
       <tr class="EnvInfoRow">
          <td>Operating System</td>
          <td><xsl:value-of select="$sys.os"/></td>
<!--    
      If this doesn't look right, your version of NAnt
      has a broken sysinfo task...
          <td><xsl:value-of select="$sys.os.platform"/> - <xsl:value-of select="$sys.os.version"/></td>
      or
          <td><xsl:value-of select="$sys.os.version"/></td>
-->       
       </tr>
       <tr class="EnvInfoRow">
          <td>.NET CLR Version</td>
          <td><xsl:value-of select="$sys.clr.version"/></td>
       </tr>
   </table> 
    <a href="#top">Back to top</a>
</xsl:template>

<!-- I am sure that all nodes are called -->
<xsl:template match="*">
    <xsl:apply-templates/>
</xsl:template>

</xsl:stylesheet>
