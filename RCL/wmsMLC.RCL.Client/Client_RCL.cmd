@echo off
set VersionClient=1.0.39.0
set ServerIp=10.0.0.67
set Env=TST

chcp 1252 > null
FOR /F "usebackq" %%i IN (`hostname`) DO (SET MYVAR=%%i)
if not exist \\10.0.0.67\TST1\Share\ClientLogs\%MYVAR% mkdir \\10.0.0.67\TST1\Share\ClientLogs\%MYVAR%\
if not exist \\10.0.0.67\TST1\Share\ClientLogs\%MYVAR%\%MYVAR%_Archive\ mkdir \\10.0.0.67\TST1\Share\ClientLogs\%MYVAR%\%MYVAR%_Archive\
if not exist \\10.0.0.67\TST1\RCL\1.0.39.0.txt (xcopy /D/E/Y wmsMLC.RCL.Client.exe_LogArchive \\10.0.0.67\TST1\Share\ClientLogs\%MYVAR%\%MYVAR%_Archive\ 
copy *.log \\10.0.0.67\TST1\Share\ClientLogs\%MYVAR%\%MYVAR%.%DATE%_%TIME:~1,1%_%TIME:~3,2%.log)
if not exist wmsMLC.RCL.Client.exe goto ClientCopy else goto ClientStart
if not exist \\10.0.0.67\TST1\RCL\1.0.39.0.txt goto ClientCopy else goto ClientStart

:ClientStart
start "" wmsMLC.RCL.Client 1.0.39.0
exit

:ClientCopy
rmdir /s /q  Activities
rmdir /s /q  External
rmdir /s /q  General
rmdir /s /q  Modules
rmdir /s /q  ResultReport
del *.dll /q
del *.chm /q
del *.exe /q
xcopy /D/Y/E \\10.0.0.67\TST1\RCL
copy \\10.0.0.67\TST1\RCL\Client_RCL.cmd Client_RCL.cmd
start "" wmsMLC.RCL.Client 1.0.39.0
