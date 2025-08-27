chcp 65001

REM !!! IMPORTANT : use x64 Native Tools Command Prompt !!!
REM !!! IMPORTANT : use x64 Native Tools Command Prompt !!!
REM !!! IMPORTANT : use x64 Native Tools Command Prompt !!!

cd compat
cl /MD /c shm.cpp
lib /OUT:shm.lib shm.obj


cd ..
cl /MD /EHsc /c freetrack20enhanced.cpp
lib /OUT:freetrack20enhanced.lib freetrack20enhanced.obj

cd ..
cl /EHsc test_freetrack.cpp compat\shm.lib

REM require (not loaded by star citizen)
REM reg add "HKCU\Software\Freetrack\FreetrackClient" /v Path /t REG_SZ /d "D:/Users/[--USERNAME--]/test_joystick/bin/" /f

REM require (loaded by star citizen : NPClient64.dll)
REM reg add "HKEY_CURRENT_USER\Software\NaturalPoint\NATURALPOINT\NPClient Location" /v Path /t REG_SZ /d "D:/Users/[--USERNAME--]/test_joystick/bin/" /f


cl /std:c++latest /EHsc test_joystick4.cpp freetrack20enhanced.lib compat\shm.lib
