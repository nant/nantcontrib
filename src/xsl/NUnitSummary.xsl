<?xml version="1.0" encoding="ISO-8859-1"?>

<!--
   This XSL File is based on the summary_overview.xsl
   template created by Erik Hatcher fot Ant's JUnitReport.
   
   Modified by Tomas Restrepo (tomasr@mvps.org) for use
   with NUnitReport
-->

<xsl:stylesheet version="1.0" 
      xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
      xmlns:html="http://www.w3.org/Profiles/XHTML-transitional"
   >

   <xsl:output method="html" indent="yes"/>
   <xsl:include href="toolkit.xsl"/>

<!--
    ====================================================
        Create the page structure
    ====================================================
-->
<xsl:template match="testsummary">
    <HTML>
        <HEAD>
        <!-- put the style in the html so that we can mail it w/o problem -->
        <style type="text/css">
            BODY {
               font: normal 10px verdana, arial, helvetica;
               color:#000000;
            }
            TD {
               FONT-SIZE: 10px
            }
            P {
               line-height:1.5em;
               margin-top:0.5em; margin-bottom:1.0em;
            }
            H1 {
            MARGIN: 0px 0px 5px; 
            FONT: bold arial, verdana, helvetica;
            FONT-SIZE: 16px;
            }
            H2 {
            MARGIN-TOP: 1em; MARGIN-BOTTOM: 0.5em; 
            FONT: bold 14px verdana,arial,helvetica
            }
            H3 {
            MARGIN-BOTTOM: 0.5em; FONT: bold 13px verdana,arial,helvetica
            }
            H4 {
               MARGIN-BOTTOM: 0.5em; FONT: bold 100% verdana,arial,helvetica
            }
            H5 {
            MARGIN-BOTTOM: 0.5em; FONT: bold 100% verdana,arial,helvetica
            }
            H6 {
            MARGIN-BOTTOM: 0.5em; FONT: bold 100% verdana,arial,helvetica
            }   
         .Error {
            font-weight:bold; background:#EEEEE0; color:purple;
         }
         .Failure {
            font-weight:bold; background:#EEEEE0; color:red;
         }
         .ClassName {
            font-weight:bold; 
            padding-left: 18px;
            cursor: hand;
            color: #777;
         }
         .TestClassDetails {
            width: 95%;
            margin-bottom: 10px; 
            border-bottom: 1px dotted #999;
         }
         .FailureDetail {
            font-size: -1;
            padding-left: 2.0em;
            border: 1px solid #999;
         }
         .Pass {
            background:#EEEEE0; 
         }
         .DetailTable TD {
            padding-top: 1px;
            padding-bottom: 1px;
            padding-left: 3px;
            padding-right: 3px;
         }
         .TableHeader {
            background: #6699cc;
            color: white;
            font-weight: bold;
            horizontal-align: center;
         }
         .EnvInfoHeader {
            background: #ff0000;
            color: white;
            font-weight: bold;
            horizontal-align: center;
         }
         .EnvInfoRow {
            background:#EEEEE0
         }
         
         A:visited {
            color: #0000ff;
         }
         A {
            color: #0000ff;
         }
         A:active {
            color: #800000;
         }
            </style>
      <script language="JavaScript"><![CDATA[   
        function toggle (field)
        {
          field.style.display = (field.style.display == "block") ? "none" : "block";
        }  ]]> 
      </script>
        </HEAD>
        <body text="#000000" bgColor="#ffffff">
            <a name="#top"></a>
            <xsl:call-template name="header"/>
            
            <!-- Summary part -->
            <xsl:call-template name="summary"/>
            <hr size="1" width="95%" align="left"/>
            
            <!-- Package List part -->
            <xsl:call-template name="packagelist"/>
            <hr size="1" width="95%" align="left"/>
            
            <!-- For each testsuite create the part -->
            <xsl:apply-templates select="testsuite">
                <xsl:sort select="@name"/>
         </xsl:apply-templates>
            
            <!-- Environment info part -->
            <xsl:call-template name="envinfo"/>

        </body>
    </HTML>
</xsl:template>
    
    
    
    <!-- ================================================================== -->
    <!-- Write a list of all packages with an hyperlink to the anchor of    -->
    <!-- of the package name.                                               -->
    <!-- ================================================================== -->
    <xsl:template name="packagelist">   
        <h2>TestSuite Summary</h2>
        <table border="0" class="DetailTable" width="95%">
            <xsl:call-template name="packageSummaryHeader"/>
            <!-- list all packages recursively -->
            <xsl:for-each select="./testsuite[not(./@name = preceding-sibling::testsuite/@name)]">
                <xsl:sort select="@name"/>
                <xsl:variable name="testCount" select="sum(../testsuite[./@name = current()/@name]/@tests)"/>
                <xsl:variable name="errorCount" select="sum(../testsuite[./@name = current()/@name]/@errors)"/>
                <xsl:variable name="failureCount" select="sum(../testsuite[./@name = current()/@name]/@failures)"/>
                <xsl:variable name="timeCount" select="sum(../testsuite[./@name = current()/@name]/@time)"/>
                
                <!-- write a summary for the package -->
                <tr valign="top">
                    <!-- set a nice color depending if there is an error/failure -->
                    <xsl:attribute name="class">
                        <xsl:choose>
                            <xsl:when test="$failureCount &gt; 0">Failure</xsl:when>
                            <xsl:when test="$errorCount &gt; 0">Error</xsl:when>
                            <xsl:otherwise>Pass</xsl:otherwise>
                        </xsl:choose>
                    </xsl:attribute>                
                    <td><a href="#{generate-id(@name)}"><xsl:value-of select="@name"/></a></td>
                    <td><xsl:value-of select="$testCount"/></td>
                    <td><xsl:value-of select="$errorCount"/></td>
                    <td><xsl:value-of select="$failureCount"/></td>
                    <td>
                  <xsl:call-template name="display-time">
                     <xsl:with-param name="value" select="$timeCount"/>
                  </xsl:call-template>                  
                    </td>                   
                </tr>
            </xsl:for-each>
        </table>        
    </xsl:template>


    <!-- ================================================================== -->
    <!-- Write a list of all classes used in a testsuite, alongside with    -->
    <!-- the results for each one.                                          -->
    <!-- ================================================================== -->
    <xsl:template match="testsuite">
       
        <!-- create an anchor to this class name -->
        <a name="#{generate-id(@name)}"></a>
        <h3>TestSuite <xsl:value-of select="@name"/></h3>

      <xsl:for-each select="testcase[generate-id(.)=generate-id(key('classnameKey',@classname))]">
         <xsl:variable name="thisClass"><xsl:value-of select="@classname"/></xsl:variable>
         <xsl:variable name="details"><xsl:value-of select="generate-id()"/></xsl:variable>
         <div class="TestClassDetails">
            <div class="ClassName" onclick="toggle({$details})">
               <xsl:value-of select="@classname"/>
            </div>
              <table border="0" width="80%" id="{$details}" style="display: block; margin-left: 35px" class="DetailTable">
                  <xsl:call-template name="classesSummaryHeader"/>
                  <xsl:apply-templates select="../testcase[@classname=$thisClass]">
                      <xsl:sort select="@name" />
                  </xsl:apply-templates>
              </table>
           </div>
      </xsl:for-each>
        <a href="#top">Back to top</a>
        <hr size="1" width="95%" align="left"/>
    </xsl:template>
    

  <xsl:template name="dot-replace">
      <xsl:param name="package"/>
      <xsl:choose>
          <xsl:when test="contains($package,'.')"><xsl:value-of select="substring-before($package,'.')"/>_<xsl:call-template name="dot-replace"><xsl:with-param name="package" select="substring-after($package,'.')"/></xsl:call-template></xsl:when>
          <xsl:otherwise><xsl:value-of select="$package"/></xsl:otherwise>
      </xsl:choose>
  </xsl:template>

</xsl:stylesheet>
