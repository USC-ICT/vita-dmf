
@rem Steps:
@rem - open 00_Master_Lines.xlsx
@rem - select first two columns
@rem - copy
@rem - open 00_MasterLines.txt
@rem - ctrl-a, paste
@rem - remove first line
@rem - save, exit
@rem - run 00_splitmasterlines.bat
@rem - run 00_runvisemeschedulerfacefx.bat


@echo off

setlocal enabledelayedexpansion

for /F "tokens=1*" %%i in (00_MasterLines.txt) do (
   set File=%%i
   set FileNoExt=!File:~0,-4!
   set Text=%%j
   set TextNoQuotes=!Text:~1,-1!
   @rem @echo !TextNoQuotes! > !FileNoExt!.txt
   @echo !Text! > !File!.txt
   )

endlocal
