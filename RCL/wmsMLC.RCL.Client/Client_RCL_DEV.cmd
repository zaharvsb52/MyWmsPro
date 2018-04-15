@echo off
chcp 1252 > null

xcopy /D/Y/E \\10.0.0.19\DEV\RCL
del Client_RCL.cmd /q
start wmsMLC.RCL.Client 
