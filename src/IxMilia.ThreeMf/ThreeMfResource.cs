// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace IxMilia.ThreeMf
{
    public abstract class ThreeMfResource
    {
        protected static XName ObjectName = XName.Get("object", ThreeMfModel.ModelNamespace);
        protected static XName BaseMaterialsName = XName.Get("basematerials", ThreeMfModel.ModelNamespace);

        public int Id { get; internal set; }

        abstract internal XElement ToXElement(Dictionary<ThreeMfResource, int> resourceMap);

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

        internal static ThreeMfResource ParseResource(XElement element, Dictionary<int, ThreeMfResource> resourceMap)
        {
            if (element.Name == ObjectName)
            {
                return ThreeMfObject.ParseObject(element, resourceMap);
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
