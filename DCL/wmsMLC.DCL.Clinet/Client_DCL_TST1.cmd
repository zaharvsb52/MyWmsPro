@echo off

chcp 1252 > null
if not exist %wmsMLC.Tools.UpdateTool.exe goto UpdateCopy else goto Start

:Start
if not exist %wmsMLC.DCL.Client.exe goto UpdateCopy else goto ClientStart

:ClientStart
start wmsMLC.DCL.Client
exit

:UpdateCopy
xcopy \\10.0.0.91\TST1\DCL\wmsMLC.Tools.UpdateTool.exe
start "" wmsMLC.Tools.UpdateTool \\10.0.0.91\TST1\DCL\UpdateInfo.xml
exit