for %%I in (..) do set ParentDirName=%%~nxI
rmdir "%APPDATA%\SpaceEngineers\Mods\%ParentDirName%"
pause