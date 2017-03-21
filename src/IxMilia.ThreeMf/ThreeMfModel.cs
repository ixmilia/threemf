// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace IxMilia.ThreeMf
{
    public class ThreeMfModel
    {
        internal const string ModelNamespace = "http://schemas.microsoft.com/3dmanufacturing/core/2015/02";
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
        private const string RequiredExtensionsAttributeName = "requiredextensions";
        private const string DefaultLanguage = "en-US";

        private static XName ModelName = XName.Get("model", ModelNamespace);
        private static XName BuildName = XName.Get("build", ModelNamespace);
        private static XName ResourcesName = XName.Get("resources", ModelNamespace);
        private static XName MetadataName = XName.Get("metadata", ModelNamespace);
        
        private static XName XmlLanguageAttributeName = XNamespace.Xml + "lang";

        private static HashSet<string> KnownExtensionNamespaces = new HashSet<string>()
        {
        };

        public ThreeMfModelUnits ModelUnits { get; set; }
        public HashSet<string> RequiredExtensionNamespaces { get; private set; }
        public string Title { get; set; }
        public string Designer { get; set; }
        public string Description { get; set; }
        public string Copyright { get; set; }
        public string LicenseTerms { get; set; }
        public string Rating { get; set; }
        public string CreationDate { get; set; }
        public string ModificationDate { get; set; }

        public IList<ThreeMfResource> Resources { get; } = new List<ThreeMfResource>();
        public IList<ThreeMfModelItem> Items { get; } = new List<ThreeMfModelItem>();

        public ThreeMfModel()
        {
            ModelUnits = ThreeMfModelUnits.Millimeter;
            RequiredExtensionNamespaces = new HashSet<string>();
        }

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

        internal static ThreeMfModel LoadXml(XElement root, IEnumerable<string> additionalSupportedNamespaces = null)
        {
            var model = new ThreeMfModel();
            model.ParseModelUnits(root.Attribute(UnitAttributeName)?.Value);
            var requiredNamespaces = (root.Attribute(RequiredExtensionsAttributeName)?.Value ?? string.Empty)
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(prefix => root.GetNamespaceOfPrefix(prefix).NamespaceName);
            model.RequiredExtensionNamespaces = new HashSet<string>(requiredNamespaces);

            var additionalNamespaces = new HashSet<string>(additionalSupportedNamespaces ?? new string[0]);
            foreach (var rns in model.RequiredExtensionNamespaces)
            {
                if (!KnownExtensionNamespaces.Contains(rns) && !additionalNamespaces.Contains(rns))
                {
                    throw new ThreeMfParseException($"The required namespace '{rns}' is not supported.");
                }
            }

            // metadata
            model.Title = GetMetadataValue(root, Metadata_Title);
            model.Designer = GetMetadataValue(root, Metadata_Designer);
            model.Description = GetMetadataValue(root, Metadata_Description);
            model.Copyright = GetMetadataValue(root, Metadata_Copyright);
            model.LicenseTerms = GetMetadataValue(root, Metadata_LicenseTerms);
            model.Rating = GetMetadataValue(root, Metadata_Rating);
            model.CreationDate = GetMetadataValue(root, Metadata_CreationDate);
            model.ModificationDate = GetMetadataValue(root, Metadata_ModificationDate);

            var resourceMap = model.ParseResources(root.Element(ResourcesName));
            model.ParseBuild(root.Element(BuildName), resourceMap);

            return model;
        }

        internal XElement ToXElement()
        {
            // ensure build items are included
            var resourcesHash = new HashSet<ThreeMfResource>(Resources);
            foreach (var item in Items)
            {
                if (resourcesHash.Add(item.Object))
                {
                    Resources.Add(item.Object);
                }
            }

            // ensure components are included
            var objects = Resources.OfType<ThreeMfObject>().ToList();
            foreach (var resource in objects)
            {
                foreach (var component in resource.Components)
                {
                    if (resourcesHash.Add(component.Object))
                    {
                        // components must be defined ahead of their reference
                        Resources.Insert(0, component.Object);
                    }
                }
            }

            var resourceMap = new Dictionary<ThreeMfResource, int>();
            for (int i = 0; i < Resources.Count; i++)
            {
                Resources[i].Id = i + 1;
                resourceMap.Add(Resources[i], Resources[i].Id);
            }

            var modelXml = new XElement(ModelName);
            var requiredNamespaces = new List<Tuple<string, string>>();
            var currentNs = 'a';
            foreach (var ns in RequiredExtensionNamespaces.OrderBy(n => n))
            {
                requiredNamespaces.Add(Tuple.Create(ns, currentNs.ToString()));
                currentNs++;
            }

            modelXml.Add(
                new XAttribute(UnitAttributeName, ModelUnits.ToString().ToLowerInvariant()),
                requiredNamespaces.Count == 0
                    ? null
                    : new XAttribute(RequiredExtensionsAttributeName, string.Join(" ", requiredNamespaces.Select(rns => rns.Item2))),
                new XAttribute(XmlLanguageAttributeName, DefaultLanguage),
                requiredNamespaces.Select(rns => new XAttribute(XNamespace.Xmlns + rns.Item2, rns.Item1)),
                GetMetadataXElement(Metadata_Title, Title),
                GetMetadataXElement(Metadata_Designer, Designer),
                GetMetadataXElement(Metadata_Description, Description),
                GetMetadataXElement(Metadata_Copyright, Copyright),
                GetMetadataXElement(Metadata_LicenseTerms, LicenseTerms),
                GetMetadataXElement(Metadata_Rating, Rating),
                GetMetadataXElement(Metadata_CreationDate, CreationDate),
                GetMetadataXElement(Metadata_ModificationDate, ModificationDate),
                new XElement(ResourcesName,
                    Resources.Select(r => r.ToXElement(resourceMap))),
                new XElement(BuildName,
                    Items.Select(i => i.ToXElement(resourceMap))));
            return modelXml;
        }

        private XElement GetMetadataXElement(string metadataType, string value)
        {
            return string.IsNullOrEmpty(value)
                ? null
                : new XElement(MetadataName, new XAttribute(NameAttributeName, metadataType), value);
        }

        private Dictionary<int, ThreeMfResource> ParseResources(XElement resources)
        {
            var resourceMap = new Dictionary<int, ThreeMfResource>();
            if (resources == null)
            {
                return resourceMap;
            }

            foreach (var element in resources.Elements())
            {
                var resource = ThreeMfResource.ParseResource(element, resourceMap);
                if (resource != null)
                {
                    Resources.Add(resource);
                    resourceMap.Add(resource.Id, resource);
                }
            }

            return resourceMap;
        }

        private void ParseBuild(XElement build, Dictionary<int, ThreeMfResource> resourceMap)
        {
            if (build == null)
            {
                // no build items specified
                return;
            }

            foreach (var element in build.Elements())
            {
                var item = ThreeMfModelItem.ParseItem(element, resourceMap);
                if (item != null)
                {
                    Items.Add(item);
                }
            }
        }

        private static string GetMetadataValue(XElement root, string name)
        {
            return root.Elements(MetadataName)?.Where(e => e.Attribute(NameAttributeName)?.Value == name).SingleOrDefault()?.Value;
        }
    }
}
