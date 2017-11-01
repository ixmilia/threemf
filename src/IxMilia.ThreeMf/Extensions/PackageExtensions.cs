// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.IO.Packaging;

namespace IxMilia.ThreeMf.Extensions
{
    internal static class PackageExtensions
    {
        public static byte[] GetPartBytes(this Package package, string uri)
        {
            var packagePart = package.GetPart(new Uri(uri, UriKind.RelativeOrAbsolute));
            using (var stream = packagePart.GetStream())
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                var data = new byte[memoryStream.Length];
                memoryStream.Read(data, 0, data.Length);
                return data;
            }
        }
    }
}
