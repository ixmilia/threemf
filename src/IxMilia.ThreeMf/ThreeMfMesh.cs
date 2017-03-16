// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Xml.Linq;

namespace IxMilia.ThreeMf
{
    public class ThreeMfMesh
    {
        public IList<ThreeMfTriangle> Triangles { get; } = new List<ThreeMfTriangle>();

        internal static ThreeMfMesh ParseMesh(XElement element)
        {
            if (element == null)
            {
                throw new ThreeMfParseException("Missing element <mesh>.");
            }

            var vertices = new List<ThreeMfVertex>();
            foreach (var vertexElement in element.Element(XName.Get("vertices", ThreeMfModel.ModelNamespace)).Elements(XName.Get("vertex", ThreeMfModel.ModelNamespace)))
            {
                var vertex = ThreeMfVertex.ParseVertex(vertexElement);
                vertices.Add(vertex);
            }

            var mesh = new ThreeMfMesh();
            foreach (var triangleElement in element.Element(XName.Get("triangles", ThreeMfModel.ModelNamespace)).Elements(XName.Get("triangle", ThreeMfModel.ModelNamespace)))
            {
                var triangle = ThreeMfTriangle.ParseTriangle(triangleElement, vertices);
                mesh.Triangles.Add(triangle);
            }

            return mesh;
        }
    }
}
