﻿// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Xml.Linq;

namespace IxMilia.ThreeMf
{
    public class ThreeMfModelItem
    {
        private const string ObjectIdAttributeName = "objectid";
        private const string PartNumberAttributeName = "partnumber";

        private static XName ItemName = XName.Get("item", ThreeMfModel.ModelNamespace);

        public ThreeMfResource Object { get; set; }
        public ThreeMfMatrix Transform { get; set; }
        public string PartNumber { get; set; }

        public ThreeMfModelItem(ThreeMfResource obj)
        {
            Object = obj;
            Transform = ThreeMfMatrix.Identity;
        }

        internal XElement ToXElement(Dictionary<ThreeMfResource, int> resourceMap)
        {
            var objectId = resourceMap[Object];
            return new XElement(ItemName,
                new XAttribute(ObjectIdAttributeName, objectId),
                Transform.ToXAttribute(),
                string.IsNullOrEmpty(PartNumber) ? null : new XAttribute(PartNumberAttributeName, PartNumber));
        }

        internal static ThreeMfModelItem ParseItem(XElement element, Dictionary<int, ThreeMfResource> resourceMap)
        {
            var objectIdAttribute = element.Attribute(ObjectIdAttributeName);
            if (objectIdAttribute == null)
            {
                throw new ThreeMfParseException("Expected object id.");
            }

            var objectId = int.Parse(objectIdAttribute.Value);
            if (!resourceMap.ContainsKey(objectId))
            {
                throw new ThreeMfParseException($"Invalid object id {objectId}");
            }

            var modelItem = new ThreeMfModelItem(resourceMap[objectId]);
            modelItem.Transform = ThreeMfMatrix.ParseMatrix(element.Attribute(ThreeMfMatrix.TransformAttributeName));
            modelItem.PartNumber = element.Attribute(PartNumberAttributeName)?.Value;
            return modelItem;
        }
    }
}
