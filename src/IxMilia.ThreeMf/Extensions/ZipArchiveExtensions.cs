// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.IO.Compression;

namespace IxMilia.ThreeMf.Extensions
{
    internal static class ZipArchiveExtensions
    {
        public static Stream GetEntryStream(this ZipArchive archive, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(nameof(path));
            }

            var entryPath = path;
            if (entryPath.StartsWith("/"))
            {
                entryPath = entryPath.Substring(1);
            }

            var entry = archive.GetEntry(entryPath);
            if (entry == null)
            {
                throw new ThreeMfParseException($"Package entry '{entryPath}' cannot be found.");
            }

            return entry.Open();
        }
    }
}
