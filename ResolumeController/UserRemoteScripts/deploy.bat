@echo off
REM You may include this script into the build process (as a build event), or you may run it manually
echo Copying user remote script
copy UserConfiguration.txt "c:\Users\geri\AppData\Roaming\Ableton\Live 9.7.1\Preferences\User Remote Scripts\ResolumeController\"
EXIT /B 0