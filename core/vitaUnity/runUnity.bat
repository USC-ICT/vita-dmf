
set PROJECTPATH=%CD%

pushd ..\Unity64
pushd Editor
start Unity.exe -projectPath %PROJECTPATH% %1 %2 %3 %4 %5 %6 %7
popd
popd
