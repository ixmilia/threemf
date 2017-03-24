// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using IxMilia.ThreeMf.Extensions;

namespace IxMilia.ThreeMf
{
    public class ThreeMfTexture2D : ThreeMfResource
    {
        private const string PathAttributeName = "path";
        private const string ContentTypeAttributeName = "contenttype";
        private const string BoxAttributeName = "box";
        private const string TileStyleUAttributeName = "tilestyleu";
        private const string TileStyleVAttributeName = "tilestylev";

        private Stream _textureStream;

        public ThreeMfTextureContentType ContentType { get; set; }
        public ThreeMfBoundingBox BoundingBox { get; set; }
        public ThreeMfTileStyle TileStyleU { get; set; }
        public ThreeMfTileStyle TileStyleV { get; set; }

        public Stream TextureStream
        {
            get => _textureStream;
            set => _textureStream = value ?? throw new ArgumentNullException(nameof(value));
        }

        public ThreeMfTexture2D(Stream textureStream, ThreeMfTextureContentType contentType)
        {
            BoundingBox = ThreeMfBoundingBox.Default;
            TextureStream = textureStream;
            ContentType = contentType;
        }

        internal override XElement ToXElement(Dictionary<ThreeMfResource, int> resourceMap, Action<string, Stream> addArchiveEntry)
        {
            var path = $"/3D/Textures/{Guid.NewGuid().ToString("N")}{ContentType.ToExtensionString()}";
            addArchiveEntry(path, TextureStream);
            return new XElement(Texture2DName,
                new XAttribute(IdAttributeName, Id),
                new XAttribute(PathAttributeName, path),
                new XAttribute(ContentTypeAttributeName, ContentType.ToContentTypeString()),
                BoundingBox.ToXAttribute(),
                TileStyleU == ThreeMfTileStyle.Wrap ? null : new XAttribute(TileStyleUAttributeName, TileStyleU.ToTileStyleString()),
                TileStyleV == ThreeMfTileStyle.Wrap ? null : new XAttribute(TileStyleVAttributeName, TileStyleV.ToTileStyleString()));
        }

        internal static ThreeMfTexture2D ParseTexture(XElement element, Func<string, Stream> getArchiveEntry)
        {
            var path = element.AttributeValueOrThrow(PathAttributeName);
            var id = element.AttributeIntValueOrThrow(IdAttributeName);
            var textureStream = new MemoryStream();
            getArchiveEntry(path).CopyTo(textureStream);
            textureStream.Seek(0, SeekOrigin.Begin);
            var contentType = ThreeMfTextureContentTypeExtensions.ParseContentType(element.AttributeValueOrThrow(ContentTypeAttributeName));
            var texture = new ThreeMfTexture2D(textureStream, contentType)
            {
                BoundingBox = ThreeMfBoundingBox.ParseBoundingBox(element.Attribute(ThreeMfBoundingBox.BoundingBoxAttributeName)?.Value),
                TileStyleU = ThreeMfTileStyleExtensions.ParseTileStyle(element.Attribute(TileStyleUAttributeName)?.Value),
                TileStyleV = ThreeMfTileStyleExtensions.ParseTileStyle(element.Attribute(TileStyleVAttributeName)?.Value)
            };

            texture.Id = id;
            return texture;
        }
    }
}
