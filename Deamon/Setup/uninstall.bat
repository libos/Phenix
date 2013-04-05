net stop "PhenixDeamon" 
"%cd%\InstallUtil.exe" "%cd%\PhenixDeamon.exe"  -u
taskkill /f /im PhenixDeamon.exe
taskkill /f /im Phenix.exe
pause