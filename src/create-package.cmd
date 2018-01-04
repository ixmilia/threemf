@echo off

set PROJECT=%~dp0IxMilia.ThreeMf\IxMilia.ThreeMf.csproj
dotnet restore %PROJECT%
if errorlevel 1 exit /b 1
dotnet pack --include-symbols --include-source --configuration Release %PROJECT% /p:OfficialBuild=true
