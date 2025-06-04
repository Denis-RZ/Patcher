@echo off
echo Building Universal Code Patcher...
dotnet build "UniversalCodePatcher.sln" --configuration Release
if %errorlevel% == 0 (
    echo Build successful!
    echo Starting application...
    start "UniversalCodePatcher\bin\Release\net6.0-windows\UniversalCodePatcher.exe"
) else (
    echo Build failed!
    pause
)
