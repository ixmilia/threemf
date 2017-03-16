// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Xml.Linq;

namespace IxMilia.ThreeMf
{
    public abstract class ThreeMfResource
    {
        internal static int ParseAttributeInt(XElement element, string attributeName, bool isRequired = false)
        {
            var att = element.Attribute(attributeName);
            if (isRequired && att == null)
            {
                throw new ThreeMfParseException($"Missing required attribute '{attributeName}'.");
            }

            if (!int.TryParse(att.Value, out var value))
            {
                throw new ThreeMfParseException($"Unable to parse '{att.Value}' as an int.");
            }

            return value;
        }

        internal static ThreeMfResource ParseResource(XElement element)
        {
            switch (element.Name.LocalName)
            {
                case "object":
                    return ThreeMfObject.ParseObject(element);
                case "basematerials":
                default:
                    return null;
            }
        }
    }
}
