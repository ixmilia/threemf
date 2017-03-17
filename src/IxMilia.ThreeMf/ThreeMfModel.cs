// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
        private const string UnitAttributeName = "unit";
        private const string NameAttributeName = "name";
        private const string DefaultLanguage = "en-US";

        private static XName ModelName = XName.Get("model", ModelNamespace);
        private static XName BuildName = XName.Get("build", ModelNamespace);
        private static XName ResourcesName = XName.Get("resources", ModelNamespace);
        private static XName MetadataName = XName.Get("metadata", ModelNamespace);
        private static XName XmlLanguageAttributeName = XNamespace.Xml + "lang";

        public ThreeMfModelUnits ModelUnits { get; set; }
        public string Title { get; set; }
        public string Designer { get; set; }
        public string Description { get; set; }
        public string Copyright { get; set; }
        public string LicenseTerms { get; set; }
        public string Rating { get; set; }
        public string CreationDate { get; set; }
        public string ModificationDate { get; set; }

        public IList<ThreeMfResource> Resources { get; } = new List<ThreeMfResource>();

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
            model.ParseModelUnits(root.Attribute(UnitAttributeName)?.Value);

            // metadata
            model.Title = GetMetadataValue(root, Metadata_Title);
            model.Designer = GetMetadataValue(root, Metadata_Designer);
            model.Description = GetMetadataValue(root, Metadata_Description);
            model.Copyright = GetMetadataValue(root, Metadata_Copyright);
            model.LicenseTerms = GetMetadataValue(root, Metadata_LicenseTerms);
            model.Rating = GetMetadataValue(root, Metadata_Rating);
            model.CreationDate = GetMetadataValue(root, Metadata_CreationDate);
            model.ModificationDate = GetMetadataValue(root, Metadata_ModificationDate);

            model.ParseResources(root.Element(ResourcesName));

            // TODO: <build>

            return model;
        }

        internal XElement ToXElement()
        {
            return new XElement(ModelName,
                new XAttribute(UnitAttributeName, ModelUnits.ToString().ToLowerInvariant()),
                new XAttribute(XmlLanguageAttributeName, DefaultLanguage),
                new XElement(ResourcesName,
                    Resources.Select(r => r.ToXElement())),
                new XElement(BuildName));
        }

        private void ParseResources(XElement resources)
        {
            if (resources == null)
            {
                return;
            }

            foreach (var element in resources.Elements())
            {
                var resource = ThreeMfResource.ParseResource(element);
                if (resource != null)
                {
                    Resources.Add(resource);
                }
            }
        }

        private static string GetMetadataValue(XElement root, string name)
        {
            return root.Elements(MetadataName)?.Where(e => e.Attribute(NameAttributeName)?.Value == name).SingleOrDefault()?.Value;
        }
    }
}
