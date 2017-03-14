// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace IxMilia.ThreeMf
{
    public class ThreeMfFile
    {
        public const string ModelNamespace = "http://schemas.microsoft.com/3dmanufacturing/core/2015/02";
        private const string ModelPath = "3D/3dmodel.model";
        private const string Metadata_Title = "Title";
        private const string Metadata_Designer= "Designer";
        private const string Metadata_Description = "Description";
        private const string Metadata_Copyright = "Copyright";
        private const string Metadata_LicenseTerms = "LicenseTerms";
        private const string Metadata_Rating = "Rating";
        private const string Metadata_CreationDate = "CreationDate";
        private const string Metadata_ModificationDate = "ModificationDate";

        public ThreeMfModelUnits ModelUnits { get; set; }
        public string Title { get; set; }
        public string Designer { get; set; }
        public string Description { get; set; }
        public string Copyright { get; set; }
        public string LicenseTerms { get; set; }
        public string Rating { get; set; }
        public string CreationDate { get; set; }
        public string ModificationDate { get; set; }

        private void ParseModelUnits(string value)
        {
            switch (value)
            {
                case "micron":
                    ModelUnits = ThreeMfModelUnits.Micron;
                    break;
                case "millimeter":
                    ModelUnits = ThreeMfModelUnits.Millimeter;
                    break;
                case "centimeter":
                    ModelUnits = ThreeMfModelUnits.Centimeter;
                    break;
                case "inch":
                    ModelUnits = ThreeMfModelUnits.Inch;
                    break;
                case "foot":
                    ModelUnits = ThreeMfModelUnits.Foot;
                    break;
                case "meter":
                    ModelUnits = ThreeMfModelUnits.Meter;
                    break;
                case null:
                    ModelUnits = ThreeMfModelUnits.Millimeter;
                    break;
                default:
                    throw new ThreeMfParseException($"Unsupported model unit '{value}'");
            }
        }

        public static ThreeMfFile LoadXml(XElement root)
        {
            var file = new ThreeMfFile();
            file.ParseModelUnits(root.Attribute("unit")?.Value);
            file.Title = GetMetadataValue(root, Metadata_Title);
            file.Designer = GetMetadataValue(root, Metadata_Designer);
            file.Description = GetMetadataValue(root, Metadata_Description);
            file.Copyright = GetMetadataValue(root, Metadata_Copyright);
            file.LicenseTerms = GetMetadataValue(root, Metadata_LicenseTerms);
            file.Rating = GetMetadataValue(root, Metadata_Rating);
            file.CreationDate = GetMetadataValue(root, Metadata_CreationDate);
            file.ModificationDate = GetMetadataValue(root, Metadata_ModificationDate);
            return file;
        }

        public static ThreeMfFile Load(Stream stream)
        {
            using (var archive = new ZipArchive(stream))
            {
                var modelEntry = archive.GetEntry(ModelPath);
                using (var modelStream = modelEntry.Open())
                {
                    var document = XDocument.Load(modelStream);
                    var file = LoadXml(document.Root);
                    return file;
                }
            }
        }

        private static string GetMetadataValue(XElement root, string name)
        {
            return root.Elements(XName.Get("metadata", ModelNamespace))?.Where(e => e.Attribute("name")?.Value == name).SingleOrDefault()?.Value;
        }
    }
}
