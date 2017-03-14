set PROJECT=.\IxMilia.ThreeMf\IxMilia.ThreeMf.csproj
dotnet restore %PROJECT%
dotnet pack --include-symbols --include-source --configuration Release %PROJECT%

