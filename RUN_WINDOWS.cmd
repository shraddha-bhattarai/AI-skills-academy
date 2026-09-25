@echo off
setlocal
cd /d "%~dp0LearningManagementSystem.Web"
where dotnet >nul 2>nul
if errorlevel 1 (
  echo Install the .NET 8 SDK from https://dotnet.microsoft.com/en-us/download/dotnet/8.0
  pause
  exit /b 1
)
echo Starting AI Skills Academy...
echo Open http://localhost:5246 in your browser after the app starts.
dotnet run --launch-profile http
if errorlevel 1 (
  echo The app could not start. Check the error above.
  pause
  exit /b 1
)
