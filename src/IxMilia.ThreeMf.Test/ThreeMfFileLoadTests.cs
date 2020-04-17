using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Xunit;

namespace IxMilia.ThreeMf.Test
{
    public class ThreeMfFileLoadTests
    {
        private static ThreeMfFile FileFromParts(params Tuple<string, string>[] filesAndContents)
        {
            using (var ms = new MemoryStream())
            {
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    var contentTypes = Tuple.Create("[Content_Types].xml", @"
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
  <Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml"" />
  <Default Extension=""model"" ContentType=""application/vnd.ms-package.3dmanufacturing-3dmodel+xml"" />
</Types>
");
                    foreach (var pair in filesAndContents.Append(contentTypes))
                    {
                        var path = pair.Item1;
                        var contents = pair.Item2;
                        var entry = archive.CreateEntry(path);
                        using (var stream = entry.Open())
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.Write(contents);
                        }
                    }
                }

                ms.Seek(0, SeekOrigin.Begin);
                var file = ThreeMfFile.Load(ms);
                return file;
            }
        }

        [Fact]
        public void LoadFromDiskTest()
        {
            var samplesDir = Path.Combine(Path.GetDirectoryName(typeof(ThreeMfFileLoadTests).GetTypeInfo().Assembly.Location), "Samples");
            var loadedFiles = 0;
            foreach (var path in Directory.EnumerateFiles(samplesDir, "*.3mf", SearchOption.AllDirectories))
            {
                var fileName = Path.GetFileName(path);
                if (fileName == "multiprop-metallic.3mf" || fileName == "multiprop-translucent.3mf")
                {
                    // undefined namespace `ms`
                    continue;
                }

                var pathParts = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (pathParts.Contains("production") || pathParts.Contains("beam lattice"))
                {
                    // not yet implemented
                    continue;
                }
                if (pathParts.Contains("MUSTFAIL"))
                {
                    // expected to fail
                    continue;
                }
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    var file = ThreeMfFile.Load(fs);
                    var model = file.Models.Single();
                    loadedFiles++;
                }
            }

            Assert.True(loadedFiles > 0, "No sample files were loaded.  Ensure all submodules have been initialized.");
        }

        [Fact]
        public void ReadFromArchiveTest()
        {
            var file = FileFromParts(
                Tuple.Create("_rels/.rels", @"
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Target=""/non/standard/path/to/model.model"" Id=""rel0"" Type=""http://schemas.microsoft.com/3dmanufacturing/2013/01/3dmodel"" />
</Relationships>
"),
                Tuple.Create("non/standard/path/to/model.model", $@"<model unit=""millimeter"" xml:lang=""en-US"" xmlns=""{ThreeMfModel.ModelNamespace}""></model>")
            );

            var model = file.Models.Single();
            Assert.Equal(ThreeMfModelUnits.Millimeter, model.ModelUnits);
        }

        [Fact]
        public void ReadMultipleModelsTest()
        {
            var file = FileFromParts(
                Tuple.Create("_rels/.rels", @"
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Target=""/3D/3dmodel-1.model"" Id=""rel1"" Type=""http://schemas.microsoft.com/3dmanufacturing/2013/01/3dmodel"" />
  <Relationship Target=""/3D/3dmodel-2.model"" Id=""rel2"" Type=""http://schemas.microsoft.com/3dmanufacturing/2013/01/3dmodel"" />
</Relationships>
"),
                Tuple.Create("3D/3dmodel-1.model", $@"<model unit=""millimeter"" xml:lang=""en-US"" xmlns=""{ThreeMfModel.ModelNamespace}""></model>"),
                Tuple.Create("3D/3dmodel-2.model", $@"<model unit=""inch"" xml:lang=""en-US"" xmlns=""{ThreeMfModel.ModelNamespace}""></model>")
            );

            Assert.Equal(2, file.Models.Count);
            Assert.Equal(ThreeMfModelUnits.Millimeter, file.Models.First().ModelUnits);
            Assert.Equal(ThreeMfModelUnits.Inch, file.Models.Last().ModelUnits);
        }

        [Fact]
        public void ReadZeroModelsTest()
        {
            var file = FileFromParts(
                Tuple.Create("_rels/.rels", @"
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
</Relationships>
")
            );

            Assert.Equal(0, file.Models.Count);
        }
    }
}
