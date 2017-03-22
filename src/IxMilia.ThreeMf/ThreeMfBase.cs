// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Xml.Linq;

namespace IxMilia.ThreeMf
{
    public class ThreeMfBase
    {
        private const string NameAttributeName = "name";
        private const string DisplayColorAttributeName = "displaycolor";

        internal static XName BaseName = XName.Get("base", ThreeMfModel.ModelNamespace);

        public string Name { get; set; }
        public ThreeMfsRGBColor Color { get; set; }

        public ThreeMfBase(string name, ThreeMfsRGBColor color)
        {
            Name = name;
            Color = color;
        }

        internal XElement ToXElement()
        {
            return new XElement(BaseName,
                new XAttribute(NameAttributeName, Name),
                new XAttribute(DisplayColorAttributeName, Color.ToString()));
        }

        internal static ThreeMfBase ParseBaseMaterial(XElement baseElement)
        {
            var nameAttribute = baseElement.Attribute(NameAttributeName);
            if (nameAttribute == null)
            {
                throw new ThreeMfParseException($"Expected attribute '{NameAttributeName}'.");
            }

            var colorAttribute = baseElement.Attribute(DisplayColorAttributeName);
            if (colorAttribute == null)
            {
                throw new ThreeMfParseException($"Expected attribute '{DisplayColorAttributeName}'.");
            }

            return new ThreeMfBase(nameAttribute.Value, ThreeMfsRGBColor.Parse(colorAttribute.Value));
        }
    }
}
