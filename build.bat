@echo OFF
rem call "C:\Program Files\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86
echo "Building"
rem echo .
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" ^
DeckPredictor.sln /Property:Configuration=Debug
set BUILD_STATUS=%ERRORLEVEL%
if %BUILD_STATUS%==0 "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\Extensions\TestPlatform\VSTest.Console.exe" ^
"DeckPredictorTests/bin/x86/Debug/DeckPredictorTests.dll" /Platform:x86
