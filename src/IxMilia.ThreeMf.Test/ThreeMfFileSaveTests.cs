// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Xunit;

namespace IxMilia.ThreeMf.Test
{
    public class ThreeMfFileSaveTests
    {
        private byte[] StringToBytes(string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        private ZipArchive GetArchiveFromFile(ThreeMfFile file)
        {
            var ms = new MemoryStream();
            file.Save(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return new ZipArchive(ms);
        }

        private string GetEntryText(ThreeMfFile file, string entryPath)
        {
            using (var archive = GetArchiveFromFile(file))
            {
                return GetEntryText(archive, entryPath);
            }
        }

        private string GetEntryText(ZipArchive archive, string entryPath)
        {
            var entry = archive.GetEntry(entryPath);
            using (var stream = entry.Open())
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        [Fact]
        public void EnsureContentTypesTest()
        {
            // this file should be static
            var expected = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
  <Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml"" />
  <Default Extension=""model"" ContentType=""application/vnd.ms-package.3dmanufacturing-3dmodel+xml"" />
</Types>
".Trim();
            var actual = GetEntryText(new ThreeMfFile(), "[Content_Types].xml");
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void EnsureRelationshipsTest()
        {
            // this file should be static
            var expected = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Target=""/3D/3dmodel.model"" Id=""rel0"" Type=""http://schemas.microsoft.com/3dmanufacturing/2013/01/3dmodel"" />
</Relationships>
".Trim();
            var actual = GetEntryText(new ThreeMfFile(), "_rels/.rels");
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void EnsureModelXmlEncodingTest()
        {
            var actual = GetEntryText(new ThreeMfFile(), "3D/3dmodel.model");
            Assert.StartsWith(@"<?xml version=""1.0"" encoding=""utf-8""?>", actual);
        }

        [Fact]
        public void EnsureModelRelationshipEntryInSavedFileTest()
        {
            var file = new ThreeMfFile();
            using (var ms = new MemoryStream())
            {
                file.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);
                file = ThreeMfFile.Load(ms);
                Assert.Equal(1, file.Models.Count);
            }
        }

        [Fact]
        public void EnsureTexturesAreWrittenTest()
        {
            var file = new ThreeMfFile();
            var model = new ThreeMfModel();
            model.Resources.Add(new ThreeMfTexture2D(StringToBytes("texture content"), ThreeMfTextureContentType.Jpeg));
            file.Models.Add(model);
            using (var archive = GetArchiveFromFile(file))
            using (var modelStream = archive.GetEntry("3D/3dmodel.model").Open())
            using (var reader = new StreamReader(modelStream))
            {
                var modelXml = XDocument.Parse(reader.ReadToEnd());
                var textureElement = modelXml.Root.Element(ThreeMfModel.ResourcesName).Element(ThreeMfResource.Texture2DName);

                // get the path to the texture
                var path = textureElement.Attribute("path").Value;

                // ensure it looks correct and isn't an empty guid
                Assert.NotEqual($"/3D/Textures/{new Guid().ToString("N")}.jpg", path);
                Assert.StartsWith("/3D/Textures", path);
                Assert.EndsWith(".jpg", path);
                path = path.Substring(1);

                // ensure that the item is present
                using (var textureStream = archive.GetEntry(path).Open())
                using (var textureReader = new StreamReader(textureStream))
                {
                    // ensure that it's correct
                    Assert.Equal("texture content", textureReader.ReadToEnd());
                }
            }
        }

        [Fact]
        public void EnsureTextureContentTypesArePresentTest()
        {
            var file = new ThreeMfFile();
            var model = new ThreeMfModel();
            model.Resources.Add(new ThreeMfTexture2D(new byte[0], ThreeMfTextureContentType.Jpeg));
            file.Models.Add(model);

            var expected = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
  <Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml"" />
  <Default Extension=""model"" ContentType=""application/vnd.ms-package.3dmanufacturing-3dmodel+xml"" />
  <Default Extension=""jpg"" ContentType=""application/vnd.ms-package.3dmanufacturing-3dmodeltexture"" />
</Types>
".Trim();
            var actual = GetEntryText(file, "[Content_Types].xml");
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void EnsureTextureRelationshipsArePresentTest()
        {
            var file = new ThreeMfFile();
            var model = new ThreeMfModel();
            model.Resources.Add(new ThreeMfTexture2D(new byte[0], ThreeMfTextureContentType.Jpeg));
            model.Resources.Add(new ThreeMfTexture2D(new byte[0], ThreeMfTextureContentType.Png));
            file.Models.Add(model);

            using (var archive = GetArchiveFromFile(file))
            {
                // get the actual texture paths
                var jpegPath = archive.Entries.Single(e => e.Name.EndsWith(".jpg")).FullName;
                var pngPath = archive.Entries.Single(e => e.Name.EndsWith(".png")).FullName;
                var expected = $@"
<?xml version=""1.0"" encoding=""utf-8""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Target=""/{jpegPath}"" Id=""rel1"" Type=""http://schemas.microsoft.com/3dmanufacturing/2013/01/3dtexture"" />
  <Relationship Target=""/{pngPath}"" Id=""rel2"" Type=""http://schemas.microsoft.com/3dmanufacturing/2013/01/3dtexture"" />
</Relationships>
".Trim();
                var actual = GetEntryText(archive, "3D/_rels/3dmodel.model.rels");
                Assert.Equal(expected, actual);
            }
        }
    }
}
