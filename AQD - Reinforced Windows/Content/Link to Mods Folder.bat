for %%I in (..) do set ParentDirName=%%~nxI
mklink /d /j "%APPDATA%\SpaceEngineers\Mods\%ParentDirName%" "%~dp0"
pause