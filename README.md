IxMilia.ThreeMf
===============

A portable .NET library for reading and writing 3MF files.

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

To build locally, install the [latest .NET Core 3.0 SDK](https://dotnet.microsoft.com/download).

## 3MF Reference

[Full specification (from 3mf.io)](http://www.3mf.io/specification/)
