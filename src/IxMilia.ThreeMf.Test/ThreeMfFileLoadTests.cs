// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Xunit;

namespace IxMilia.ThreeMf.Test
{
    public class ThreeMfFileLoadTests
    {
        [Fact]
        public void LoadFromDiskTest()
        {
            var path = Path.Combine(Path.GetDirectoryName(typeof(ThreeMfFileLoadTests).GetTypeInfo().Assembly.Location), "box.3mf");
            using (var fs = new FileStream(path, FileMode.Open))
            {
                var file = ThreeMfFile.Load(fs);
                var model = file.Models.Single();
                Assert.Equal(ThreeMfModelUnits.Millimeter, model.ModelUnits);
            }
        }

        [Fact]
        public void ReadFromArchiveTest()
        {
            using (var ms = new MemoryStream())
            {
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    {
                        var entry = archive.CreateEntry("_rels/.rels");
                        using (var stream = entry.Open())
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.WriteLine(@"<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">");
                            writer.WriteLine(@"  <Relationship Target=""/non/standard/path/to/model.model"" Id=""rel0"" Type=""http://schemas.microsoft.com/3dmanufacturing/2013/01/3dmodel"" />");
                            writer.WriteLine(@"</Relationships>");
                        }
                    }

                    {
                        var entry = archive.CreateEntry("non/standard/path/to/model.model");
                        using (var stream = entry.Open())
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.WriteLine($@"<model unit=""millimeter"" xml:lang=""en-US"" xmlns=""{ThreeMfModel.ModelNamespace}""></model>");
                        }
                    }
                }

                ms.Seek(0, SeekOrigin.Begin);
                var file = ThreeMfFile.Load(ms);
                var model = file.Models.Single();
                Assert.Equal(ThreeMfModelUnits.Millimeter, model.ModelUnits);
            }
        }
    }
}
