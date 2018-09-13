#!/bin/sh -e

_SCRIPT_DIR="$( cd -P -- "$(dirname -- "$(command -v -- "$0")")" && pwd -P )"
PROJECT=./IxMilia.ThreeMf/IxMilia.ThreeMf.csproj
dotnet restore $PROJECT
dotnet pack --configuration Release $PROJECT
