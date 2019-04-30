IxMilia.ThreeMf
===============

A portable .NET library for reading and writing 3MF files.

[![Build Status](https://dev.azure.com/ixmilia/public/_apis/build/status/ThreeMf?branchName=master)](https://dev.azure.com/ixmilia/public/_build/latest?definitionId=23)

## Usage

Open a 3MF file:

``` C#
using System.IO;
using IxMilia.ThreeMf;
// ...

ThreeMfFile file = ThreeMfFile.Load(@"C:\Path\To\File.3mf");

// use `file` here
```

## Building locally

Requirements to build locally are:

- [Latest .NET Core SDK](https://github.com/dotnet/cli/releases)  As of this writing the following was also required on Ubuntu 14.04:

`sudo apt-get install dotnet-sharedframework-microsoft.netcore.app-1.0.3`

## 3MF Reference

[Full specification (from 3mf.io)](http://www.3mf.io/specification/)
