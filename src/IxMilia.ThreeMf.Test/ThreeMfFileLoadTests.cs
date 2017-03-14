// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Xml.Linq;
using Xunit;

namespace IxMilia.ThreeMf.Test
{
    public class ThreeMfFileLoadTests
    {
        private ThreeMfFile ParseXml(string contents)
        {
            var document = XDocument.Parse(contents);
            return ThreeMfFile.LoadXml(document.Root);
        }

        private ThreeMfFile FromContent(string content)
        {
            return ParseXml($@"<model xmlns=""{ThreeMfFile.ModelNamespace}"">" + content + "</model>");
        }

        private void AssertUnits(string unitsString, ThreeMfModelUnits expectedUnits)
        {
            var file = ParseXml($@"<model unit=""{unitsString}"" xml:lang=""en-US"" xmlns=""{ThreeMfFile.ModelNamespace}""></model>");
            Assert.Equal(expectedUnits, file.ModelUnits);
        }

        [Fact]
        public void LoadFromDiskTest()
        {
            var path = Path.Combine(Path.GetDirectoryName(typeof(ThreeMfFileLoadTests).GetTypeInfo().Assembly.Location), "box.3mf");
            using (var fs = new FileStream(path, FileMode.Open))
            {
                var file = ThreeMfFile.Load(fs);
                Assert.Equal(ThreeMfModelUnits.Millimeter, file.ModelUnits);
            }
        }

        [Fact]
        public void ReadFromArchiveTest()
        {
            using (var ms = new MemoryStream())
            {
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    var entry = archive.CreateEntry("3D/3dmodel.model");
                    using (var stream = entry.Open())
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.WriteLine($@"<model unit=""millimeter"" xml:lang=""en-US"" xmlns=""{ThreeMfFile.ModelNamespace}""></model>");
                    }
                }

                ms.Seek(0, SeekOrigin.Begin);
                var file = ThreeMfFile.Load(ms);
                Assert.Equal(ThreeMfModelUnits.Millimeter, file.ModelUnits);
            }
        }

        [Fact]
        public void ReadModelUnitTest()
        {
            AssertUnits("micron", ThreeMfModelUnits.Micron);
            AssertUnits("millimeter", ThreeMfModelUnits.Millimeter);
            AssertUnits("centimeter", ThreeMfModelUnits.Centimeter);
            AssertUnits("inch", ThreeMfModelUnits.Inch);
            AssertUnits("foot", ThreeMfModelUnits.Foot);
            AssertUnits("meter", ThreeMfModelUnits.Meter);

            Assert.Throws<ThreeMfParseException>(() => ParseXml($@"<model unit=""mile"" xml:lang=""en-US"" xmlns=""{ThreeMfFile.ModelNamespace}""></model>"));
        }

        [Fact]
        public void MetadataReaderTest()
        {
            var file = FromContent(@"
<metadata name=""Title"">some title</metadata>
");
            Assert.Equal("some title", file.Title);
            Assert.Null(file.Description);
        }
    }
}
