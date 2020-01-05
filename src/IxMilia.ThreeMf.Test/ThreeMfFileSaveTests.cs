// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace IxMilia.ThreeMf.Test
{
    public class ThreeMfFileSaveTests : ThreeMfAbstractTestBase
    {
        private ZipArchive GetArchiveFromFile(ThreeMfFile file)
        {
            var ms = new MemoryStream();
            file.Save(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return GetArchiveFromStream(ms);
        }

        private ZipArchive GetArchiveFromStream(Stream stream)
        {
            return new ZipArchive(stream);
        }

        private ZipArchiveEntry GetEntryFromFile(ThreeMfFile file, string entryPath)
        {
            using (var archive = GetArchiveFromFile(file))
            {
                return archive.GetEntry(entryPath);
            }
        }

        private string GetEntryText(ThreeMfFile file, string entryPath)
        {
            using (var archive = GetArchiveFromFile(file))
            {
                return GetEntryText(archive, entryPath);
            }
        }

        private XElement GetXmlFromFile(ThreeMfFile file, string entryPath)
        {
            var actualText = GetEntryText(file, entryPath);
            return XDocument.Parse(actualText).Root;
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
        public void FileSystemAPITest()
        {
            var filePath = Path.GetTempFileName();
            var file = new ThreeMfFile();
            file.Save(filePath);
            var roundTripFile = ThreeMfFile.Load(filePath);

            try
            {
                File.Delete(filePath);
            }
            catch
            {
            }
        }

        [Fact]
        public void EnsureContentTypesTest()
        {
            // this file should be static
            var expected = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
  <Default Extension=""model"" ContentType=""application/vnd.ms-package.3dmanufacturing-3dmodel+xml"" />
  <Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml"" />
</Types>
".Trim();
            var file = new ThreeMfFile();
            file.Models.Add(new ThreeMfModel());
            var actual = GetEntryText(file, "[Content_Types].xml");

            // format contents
            expected = XDocument.Parse(expected).ToString();
            actual = XDocument.Parse(actual).ToString();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void EnsureZeroModelRelationshipsTest()
        {
            var file = new ThreeMfFile();
            var relsEntry = GetEntryFromFile(file, "_rels/.rels");
            Assert.Null(relsEntry);
        }

        [Fact]
        public void EnsureSingleModelRelationshipTest()
        {
            var file = new ThreeMfFile();
            file.Models.Add(new ThreeMfModel());
            var relsXml = GetXmlFromFile(file, "_rels/.rels");
            var relationship = relsXml.Elements().Single();
            Assert.Equal("http://schemas.microsoft.com/3dmanufacturing/2013/01/3dmodel", relationship.Attribute("Type").Value);
            Assert.Equal("/3D/3dmodel.model", relationship.Attribute("Target").Value);
        }

        [Fact]
        public void EnsureMultipleModelRelationshipTest()
        {
            var file = new ThreeMfFile();
            file.Models.Add(new ThreeMfModel());
            file.Models.Add(new ThreeMfModel());
            var relsXml = GetXmlFromFile(file, "_rels/.rels");
            Assert.Equal(2, relsXml.Elements().Count());
        }

        [Fact]
        public void EnsureModelXmlEncodingTest()
        {
            var file = new ThreeMfFile();
            file.Models.Add(new ThreeMfModel());
            var actual = GetEntryText(file, "3D/3dmodel.model");
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
                Assert.Equal(0, file.Models.Count);
            }

            file.Models.Add(new ThreeMfModel());
            using (var ms = new MemoryStream())
            {
                file.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);
                file = ThreeMfFile.Load(ms);
                Assert.Equal(1, file.Models.Count);
            }

            file.Models.Add(new ThreeMfModel());
            using (var ms = new MemoryStream())
            {
                file.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);
                file = ThreeMfFile.Load(ms);
                Assert.Equal(2, file.Models.Count);
            }
        }

        [Fact]
        public void EnsureTexturesAreWrittenTest()
        {
            var file = new ThreeMfFile();
            var model = new ThreeMfModel();
            model.Resources.Add(new ThreeMfTexture2D(StringToBytes("texture content"), ThreeMfImageContentType.Jpeg));
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
                Assert.StartsWith("/3D/Textures/", path);
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
        public void EnsureTextureRelationshipsArePresentTest()
        {
            var file = new ThreeMfFile();
            var model = new ThreeMfModel();
            model.Resources.Add(new ThreeMfTexture2D(new byte[0], ThreeMfImageContentType.Jpeg));
            model.Resources.Add(new ThreeMfTexture2D(new byte[0], ThreeMfImageContentType.Png));
            file.Models.Add(model);

            using (var archive = GetArchiveFromFile(file))
            {
                // get the actual texture paths
                var jpegPath = archive.Entries.Single(e => e.Name.EndsWith(".jpg")).FullName;
                var pngPath = archive.Entries.Single(e => e.Name.EndsWith(".png")).FullName;
                var expected = $@"
<?xml version=""1.0"" encoding=""utf-8""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Type=""http://schemas.microsoft.com/3dmanufacturing/2013/01/3dtexture"" Target=""/{jpegPath}"" Id=""rel1"" />
  <Relationship Type=""http://schemas.microsoft.com/3dmanufacturing/2013/01/3dtexture"" Target=""/{pngPath}"" Id=""rel2"" />
</Relationships>
".Trim();
                var actual = GetEntryText(archive, "3D/_rels/3dmodel.model.rels");

                // format contents
                expected = XDocument.Parse(expected).ToString();
                var actualXml = XDocument.Parse(actual);
                var relNumber = 1;
                foreach (var element in actualXml.Root.Elements())
                {
                    // normalize relationship ids
                    element.Attribute("Id").Value = $"rel{relNumber++}";
                }

                actual = actualXml.ToString();
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void EnsureThumbnailsAreWrittenTest()
        {
            var file = new ThreeMfFile();
            var obj = new ThreeMfObject();
            var model = new ThreeMfModel();
            model.Resources.Add(new ThreeMfObject() { ThumbnailData = StringToBytes("jpeg thumbnail"), ThumbnailContentType = ThreeMfImageContentType.Jpeg });
            file.Models.Add(model);
            using (var archive = GetArchiveFromFile(file))
            using (var modelStream = archive.GetEntry("3D/3dmodel.model").Open())
            using (var reader = new StreamReader(modelStream))
            {
                var modelXml = XDocument.Parse(reader.ReadToEnd());
                var objectElement = modelXml.Root.Element(ThreeMfModel.ResourcesName).Element(ThreeMfResource.ObjectName);

                // get the path to the thumbnail
                var path = objectElement.Attribute("thumbnail").Value;

                // ensure it looks correct and isn't an empty guid
                Assert.NotEqual($"{ThreeMfObject.ThumbnailPathPrefix}{new Guid().ToString("N")}.jpg", path);
                Assert.StartsWith(ThreeMfObject.ThumbnailPathPrefix, path);
                Assert.EndsWith(".jpg", path);
                path = path.Substring(1);

                // ensure that the item is present
                using (var thumbnailStream = archive.GetEntry(path).Open())
                using (var thumbnailReader = new StreamReader(thumbnailStream))
                {
                    // ensure that it's correct
                    Assert.Equal("jpeg thumbnail", thumbnailReader.ReadToEnd());
                }
            }
        }

        [Fact]
        public void EnsureThumbnailRelationshipsArePresentTest()
        {
            var file = new ThreeMfFile();
            var model = new ThreeMfModel();
            model.Resources.Add(new ThreeMfObject() { ThumbnailData = new byte[0], ThumbnailContentType = ThreeMfImageContentType.Jpeg });
            model.Resources.Add(new ThreeMfObject() { ThumbnailData = new byte[0], ThumbnailContentType = ThreeMfImageContentType.Png });
            file.Models.Add(model);

            using (var zipStream = new MemoryStream())
            using (var packageStream = new MemoryStream())
            {
                // save file to stream and duplicate it so it can be re-opened as both a simple zip file and a package to validate structure
                file.Save(zipStream);
                zipStream.Seek(0, SeekOrigin.Begin);
                zipStream.CopyTo(packageStream);
                zipStream.Seek(0, SeekOrigin.Begin);
                packageStream.Seek(0, SeekOrigin.Begin);
                using (var archive = GetArchiveFromStream(zipStream))
                using (var package = Package.Open(packageStream))
                {
                    // get the actual thumbnail paths
                    var jpegPath = "/" + archive.Entries.Single(e => e.Name.EndsWith(".jpg")).FullName;
                    var pngPath = "/" + archive.Entries.Single(e => e.Name.EndsWith(".png")).FullName;
                    var modelPart = package.GetPart(new Uri("/3D/3dmodel.model", UriKind.RelativeOrAbsolute));
                    var rels = modelPart.GetRelationships().ToList();
                    Assert.Equal(2, rels.Count);
                    Assert.Equal(jpegPath, rels[0].TargetUri.ToString());
                    Assert.Equal(pngPath, rels[1].TargetUri.ToString());
                }
            }
        }
    }
}
