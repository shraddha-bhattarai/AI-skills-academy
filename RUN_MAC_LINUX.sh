#!/usr/bin/env sh
set -eu
cd "$(dirname "$0")/LearningManagementSystem.Web"
if ! command -v dotnet >/dev/null 2>&1; then
  echo 'Install the .NET 8 SDK: https://dotnet.microsoft.com/en-us/download/dotnet/8.0'
  exit 1
fi
echo 'Starting AI Skills Academy. Open http://localhost:5246 in your browser.'
dotnet run --launch-profile http
