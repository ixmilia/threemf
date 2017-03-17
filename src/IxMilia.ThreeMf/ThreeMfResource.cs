// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Xml.Linq;

namespace IxMilia.ThreeMf
{
    public abstract class ThreeMfResource
    {
        protected static XName ObjectName = XName.Get("object", ThreeMfModel.ModelNamespace);
        protected static XName BaseMaterialsName = XName.Get("basematerials", ThreeMfModel.ModelNamespace);

        abstract internal XElement ToXElement();

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
            if (element.Name == ObjectName)
            {
                return ThreeMfObject.ParseObject(element);
            }
            else if (element.Name == BaseMaterialsName)
            {
                // NYI
                return null;
            }
            else
            {
                return null;
            }
        }
    }
}
