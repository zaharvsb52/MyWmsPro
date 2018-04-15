@echo off

xcopy \\10.0.0.19\DEV\DCL\wmsMLC.Tools.UpdateTool.exe  /Q /R /Y
start "" wmsMLC.Tools.UpdateTool \\10.0.0.19\DEV\DCL\UpdateInfo.xml
exit