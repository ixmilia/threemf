// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using System.Xml.Linq;

namespace IxMilia.ThreeMf
{
    public class ThreeMfModel
    {
        public const string ModelNamespace = "http://schemas.microsoft.com/3dmanufacturing/core/2015/02";
        private const string Metadata_Title = "Title";
        private const string Metadata_Designer = "Designer";
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

        public static ThreeMfModel LoadXml(XElement root)
        {
            var model = new ThreeMfModel();
            model.ParseModelUnits(root.Attribute("unit")?.Value);
            model.Title = GetMetadataValue(root, Metadata_Title);
            model.Designer = GetMetadataValue(root, Metadata_Designer);
            model.Description = GetMetadataValue(root, Metadata_Description);
            model.Copyright = GetMetadataValue(root, Metadata_Copyright);
            model.LicenseTerms = GetMetadataValue(root, Metadata_LicenseTerms);
            model.Rating = GetMetadataValue(root, Metadata_Rating);
            model.CreationDate = GetMetadataValue(root, Metadata_CreationDate);
            model.ModificationDate = GetMetadataValue(root, Metadata_ModificationDate);
            return model;
        }

        private static string GetMetadataValue(XElement root, string name)
        {
            return root.Elements(XName.Get("metadata", ModelNamespace))?.Where(e => e.Attribute("name")?.Value == name).SingleOrDefault()?.Value;
        }
    }
}
