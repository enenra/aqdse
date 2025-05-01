@echo off
set "logDir=C:\Users\enenra\AppData\Roaming\SpaceEngineers"
cd /d "%logDir%"

:: Find the newest .log file starting with "SpaceEngineers_"
for /f "delims=" %%A in ('dir "SpaceEngineers_*.log" /b /a-d /o-d') do (
    set "newestLog=%%A"
    goto :found
)

:found
if defined newestLog (
    echo Opening newest log file: %newestLog%
    start "" "%logDir%\%newestLog%"
) else (
    echo No log files found matching the pattern.
)