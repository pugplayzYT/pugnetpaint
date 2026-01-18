@echo off
set "Root=%~dp0"
set "AppDir=%Root%PugNetPaint"
set "InstallerDir=%Root%PugNetPaintInstaller"
set "PublishDir=%AppDir%\bin\Publish"

echo [1/4] Cleaning previous builds...
if exist "%PublishDir%" rmdir /s /q "%PublishDir%"
if exist "%InstallerDir%\PugNetPaint.zip" del "%InstallerDir%\PugNetPaint.zip"

echo [2/4] Building PugNetPaint App...
dotnet publish "%AppDir%\PugNetPaint.csproj" -c Release -o "%PublishDir%"
if %errorlevel% neq 0 (
    echo [ERROR] Build failed!
    pause
    exit /b %errorlevel%
)

echo [3/4] Zipping Application for Installer...
powershell -Command "Compress-Archive -Path '%PublishDir%\*' -DestinationPath '%InstallerDir%\PugNetPaint.zip' -Force"
if %errorlevel% neq 0 (
    echo [ERROR] Zipping failed!
    pause
    exit /b %errorlevel%
)

echo [4/4] Building Installer...
dotnet build "%InstallerDir%\PugNetPaintInstaller.csproj" -c Release
if %errorlevel% neq 0 (
    echo [ERROR] Installer build failed!
    pause
    exit /b %errorlevel%
)

echo.
echo ==========================================
echo    SUCCESS! Installer ready.
echo ==========================================
echo.
pause
