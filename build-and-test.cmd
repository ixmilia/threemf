set TEST_PROJECT=.\src\IxMilia.ThreeMf.Test\IxMilia.ThreeMf.Test.csproj
dotnet restore %TEST_PROJECT%
dotnet test %TEST_PROJECT%
