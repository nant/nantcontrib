@echo off
cd ..
if not exist SLiNgshoT.zip goto zip
echo SLiNgshoT.zip exists. rename it or delete it.
goto end
:zip
"C:\Program Files\WinZip\WZZIP.EXE" -rP SLiNgshoT.zip SLiNgshoT
:end
cd SLiNgshoT
