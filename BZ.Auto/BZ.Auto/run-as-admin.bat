@echo off
title Running Blazor Server as Admin
echo Requesting Administrator privileges to run 'dotnet run'...
powershell -Command "Start-Process dotnet -ArgumentList 'run' -WorkingDirectory 'D:\Code\bz-auto-click\BZ.Auto\BZ.Auto' -Verb RunAs"
exit