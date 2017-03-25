// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace IxMilia.ThreeMf
{
    public enum ThreeMfTextureContentType
    {
        Jpeg,
        Png
    }

    internal static class ThreeMfTextureContentTypeExtensions
    {
        private const string JpegContentType = "image/jpeg";
        private const string PngContentType = "image/png";

        public static string ToContentTypeString(this ThreeMfTextureContentType contentType)
        {
            switch (contentType)
            {
                case ThreeMfTextureContentType.Jpeg:
                    return JpegContentType;
                case ThreeMfTextureContentType.Png:
                    return PngContentType;
                default:
                    throw new InvalidOperationException();
            }
        }

        public static string ToExtensionString(this ThreeMfTextureContentType contentType)
        {
            switch (contentType)
            {
                case ThreeMfTextureContentType.Jpeg:
                    return ".jpg";
                case ThreeMfTextureContentType.Png:
                    return ".png";
                default:
                    throw new InvalidOperationException();
            }
        }

        public static ThreeMfTextureContentType ParseContentType(string contentType)
        {
            switch (contentType)
            {
                case JpegContentType:
                    return ThreeMfTextureContentType.Jpeg;
                case PngContentType:
                    return ThreeMfTextureContentType.Png;
                default:
                    throw new ThreeMfParseException($"Invalid texture content type '{contentType}'.");
            }
        }
    }
}
