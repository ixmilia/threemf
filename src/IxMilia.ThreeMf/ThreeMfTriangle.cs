// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Xml.Linq;

namespace IxMilia.ThreeMf
{
    public struct ThreeMfTriangle
    {
        private const string V1AttributeName = "v1";
        private const string V2AttributeName = "v2";
        private const string V3AttributeName = "v3";

        internal static XName TriangleName = XName.Get("triangle", ThreeMfModel.ModelNamespace);

        // TODO:
        //   p1 = overrides object level pindex for the first vertex
        //   p2
        //   p3
        //   pid = overrides object-level pid for the triangle

        public ThreeMfVertex V1 { get; set; }
        public ThreeMfVertex V2 { get; set; }
        public ThreeMfVertex V3 { get; set; }

        public ThreeMfTriangle(ThreeMfVertex v1, ThreeMfVertex v2, ThreeMfVertex v3)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
        }

        internal XElement ToXElement(List<ThreeMfVertex> vertices)
        {
            return new XElement(TriangleName,
                new XAttribute(V1AttributeName, GetVertexIndex(vertices, V1)),
                new XAttribute(V2AttributeName, GetVertexIndex(vertices, V2)),
                new XAttribute(V3AttributeName, GetVertexIndex(vertices, V3)));
        }

        private static int GetVertexIndex(List<ThreeMfVertex> vertices, ThreeMfVertex item)
        {
            return vertices.IndexOf(item);
        }

        internal static ThreeMfTriangle ParseTriangle(XElement triangleElement, IList<ThreeMfVertex> vertices)
        {
            var v1Index = ThreeMfResource.ParseAttributeInt(triangleElement, V1AttributeName, isRequired: true);
            var v2Index = ThreeMfResource.ParseAttributeInt(triangleElement, V2AttributeName, isRequired: true);
            var v3Index = ThreeMfResource.ParseAttributeInt(triangleElement, V3AttributeName, isRequired: true);

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

            return new ThreeMfTriangle(vertices[v1Index], vertices[v2Index], vertices[v3Index]);
        }
    }
}
