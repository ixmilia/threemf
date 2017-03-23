// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using IxMilia.ThreeMf.Extensions;

namespace IxMilia.ThreeMf
{
    public struct ThreeMfTriangle
    {
        private const string V1AttributeName = "v1";
        private const string V2AttributeName = "v2";
        private const string V3AttributeName = "v3";
        private const string PropertyIndexAttributeName = "pid";
        private const string V1PropertyAttributeName = "p1";
        private const string V2PropertyAttributeName = "p2";
        private const string V3PropertyAttributeName = "p3";

        internal static XName TriangleName = XName.Get("triangle", ThreeMfModel.ModelNamespace);

        public ThreeMfVertex V1 { get; set; }
        public ThreeMfVertex V2 { get; set; }
        public ThreeMfVertex V3 { get; set; }

        public IThreeMfPropertyResource PropertyResource { get; set; }
        public int V1PropertyIndex { get; set; }
        public int V2PropertyIndex { get; set; }
        public int V3PropertyIndex { get; set; }

        public ThreeMfTriangle(ThreeMfVertex v1, ThreeMfVertex v2, ThreeMfVertex v3)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
            PropertyResource = null;
            V1PropertyIndex = 0;
            V2PropertyIndex = 0;
            V3PropertyIndex = 0;
        }

        internal XElement ToXElement(List<ThreeMfVertex> vertices, Dictionary<ThreeMfResource, int> resourceMap)
        {
            return new XElement(TriangleName,
                new XAttribute(V1AttributeName, GetVertexIndex(vertices, V1)),
                new XAttribute(V2AttributeName, GetVertexIndex(vertices, V2)),
                new XAttribute(V3AttributeName, GetVertexIndex(vertices, V3)),
                PropertyResource == null
                    ? null
                    : new[]
                    {
                        new XAttribute(PropertyIndexAttributeName, resourceMap[(ThreeMfResource)PropertyResource]),
                        new XAttribute(V1PropertyAttributeName, V1PropertyIndex),
                        new XAttribute(V2PropertyAttributeName, V2PropertyIndex),
                        new XAttribute(V3PropertyAttributeName, V3PropertyIndex)
                    });
        }

        private static int GetVertexIndex(List<ThreeMfVertex> vertices, ThreeMfVertex item)
        {
            return vertices.IndexOf(item);
        }

        internal static ThreeMfTriangle ParseTriangle(XElement triangleElement, IList<ThreeMfVertex> vertices, Dictionary<int, ThreeMfResource> resourceMap)
        {
            var v1Index = triangleElement.AttributeIntValueOrThrow(V1AttributeName);
            var v2Index = triangleElement.AttributeIntValueOrThrow(V2AttributeName);
            var v3Index = triangleElement.AttributeIntValueOrThrow(V3AttributeName);

            if (v1Index == v2Index || v1Index == v3Index || v2Index == v3Index)
            {
                throw new ThreeMfParseException("Triangle must specify distinct indices.");
            }

            if (v1Index < 0 || v1Index >= vertices.Count ||
                v2Index < 0 || v2Index >= vertices.Count ||
                v3Index < 0 || v3Index >= vertices.Count)
            {
                throw new ThreeMfParseException("Triangle vertex index does not exist.");
            }

            var triangle = new ThreeMfTriangle(vertices[v1Index], vertices[v2Index], vertices[v3Index]);

            var propertyIndexAttribute = triangleElement.Attribute(PropertyIndexAttributeName);
            if (propertyIndexAttribute != null)
            {
                if (!int.TryParse(propertyIndexAttribute.Value, out var propertyIndex))
                {
                    throw new ThreeMfParseException($"Property index '{propertyIndexAttribute.Value}' is not an int.");
                }

                if (resourceMap.ContainsKey(propertyIndex))
                {
                    var propertyResource = resourceMap[propertyIndex] as IThreeMfPropertyResource;
                    if (propertyResource == null)
                    {
                        throw new ThreeMfParseException($"{nameof(PropertyResource)} was expected to be of type {nameof(IThreeMfPropertyResource)}.");
                    }

                    var propertyCount = propertyResource.PropertyItems.Count();
                    var v1PropertyIndex = TryParseVertexPropertyIndex(triangleElement, V1PropertyAttributeName);
                    var v2PropertyIndex = TryParseVertexPropertyIndex(triangleElement, V2PropertyAttributeName);
                    var v3PropertyIndex = TryParseVertexPropertyIndex(triangleElement, V3PropertyAttributeName);
                    if (v1PropertyIndex < 0 || v1PropertyIndex >= propertyCount ||
                        v2PropertyIndex < 0 || v2PropertyIndex >= propertyCount ||
                        v3PropertyIndex < 0 || v3PropertyIndex >= propertyCount)
                    {
                        throw new ThreeMfParseException($"Property index is out of range.  Value must be [0, {propertyCount}).");
                    }

                    triangle.PropertyResource = propertyResource;
                    triangle.V1PropertyIndex = v1PropertyIndex;
                    triangle.V2PropertyIndex = v2PropertyIndex;
                    triangle.V3PropertyIndex = v3PropertyIndex;
                }
                else
                {
                    // could have been an unsupported resource type
                }
            }

            return triangle;
        }

        private static int TryParseVertexPropertyIndex(XElement element, string attributeName)
        {
            var attribute = element.Attribute(attributeName);
            if (attribute == null)
            {
                return 0;
            }

            if (!int.TryParse(attribute.Value, out var index))
            {
                throw new ThreeMfParseException($"Property index '{attribute.Value}' is not an int.");
            }

            return index;
        }
    }
}
