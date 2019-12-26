for /r %%I in (*.sbc) do (
	if "%%~nI" == "bp" exit
	mkdir "%CD%\%%~nI"

	ren "%%~nI.sbc" "bp.sbc"
	move "%CD%\bp.sbc" "%CD%\%%~nI"
	)
pause