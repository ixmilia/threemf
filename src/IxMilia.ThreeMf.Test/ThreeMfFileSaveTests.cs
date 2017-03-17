// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;
using System.IO.Compression;
using Xunit;

namespace IxMilia.ThreeMf.Test
{
    public class ThreeMfFileSaveTests
    {
        private string GetEntryText(ThreeMfFile file, string entryPath)
        {
            using (var ms = new MemoryStream())
            {
                file.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);
                using (var archive = new ZipArchive(ms))
                {
                    var entry = archive.GetEntry(entryPath);
                    using (var stream = entry.Open())
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
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
    }
}
