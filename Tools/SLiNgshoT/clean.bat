@echo off

if exist SLiNgshoT.suo del /AH SLiNgshoT.suo

if exist SLiNgshoT\SLiNgshoT.csproj.user del SLiNgshoT\SLiNgshoT.csproj.user
if exist SLiNgshoT.Core\bin rd /s /q SLiNgshoT\bin
if exist SLiNgshoT\obj rd /s /q SLiNgshoT\obj

if exist SLiNgshoT.Core\SLiNgshoT.Core.csproj.user del SLiNgshoT.Core\SLiNgshoT.Core.csproj.user
if exist SLiNgshoT.Core\bin rd /s /q SLiNgshoT.Core\bin
if exist SLiNgshoT.Core\obj rd /s /q SLiNgshoT.Core\obj

if exist build rd /s /q build
