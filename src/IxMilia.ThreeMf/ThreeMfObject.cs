// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace IxMilia.ThreeMf
{
    public class ThreeMfObject : ThreeMfResource
    {
        private const string IdAttributeName = "id";
        private const string NameAttributeName = "name";
        private const string PartNumberAttributeName = "partnumber";
        private const string TypeAttributeName = "type";

        internal static XName MeshName = XName.Get("mesh", ThreeMfModel.ModelNamespace);
        internal static XName ComponentsName = XName.Get("components", ThreeMfModel.ModelNamespace);

        // TODO:
        //   pid = reference to property group element with matching id attribute.  required if pindex is specified
        //   pindex = a zero-based index into the properties group specified by pid.  this is used to build the object
        //   thumbnail = path to a 3d texture of jpeg or png that represents a rendered image of the object

        public ThreeMfObjectType Type { get; set; }
        public string PartNumber { get; set; }
        public string Name { get; set; }
        public ThreeMfMesh Mesh
        {
            get => _mesh;
            set => _mesh = value ?? throw new ArgumentException(nameof(value));
        }

        public IList<ThreeMfComponent> Components { get; } = new List<ThreeMfComponent>();

        private ThreeMfMesh _mesh;

        public ThreeMfObject()
        {
            Type = ThreeMfObjectType.Model;
            Mesh = new ThreeMfMesh();
        }

        internal override XElement ToXElement(Dictionary<ThreeMfResource, int> resourceMap)
        {
            return new XElement(ObjectName,
                new XAttribute(IdAttributeName, Id),
                new XAttribute(TypeAttributeName, Type.ToString().ToLowerInvariant()),
                PartNumber == null ? null : new XAttribute(PartNumberAttributeName, PartNumber),
                Name == null ? null : new XAttribute(NameAttributeName, Name),
                Mesh.ToXElement(),
                Components.Count == 0 ? null : new XElement(ComponentsName, Components.Select(c => c.ToXElement(resourceMap))));
        }

        internal static ThreeMfObject ParseObject(XElement element, Dictionary<int, ThreeMfResource> resourceMap)
        {
            var obj = new ThreeMfObject();
            obj.Id = ParseAttributeInt(element, IdAttributeName, isRequired: true);
            obj.Type = ParseObjectType(element.Attribute(TypeAttributeName)?.Value);
            obj.PartNumber = element.Attribute(PartNumberAttributeName)?.Value;
            obj.Name = element.Attribute(NameAttributeName)?.Value;
            obj.Mesh = ThreeMfMesh.ParseMesh(element.Element(MeshName));
            var components = element.Element(ComponentsName);
            if (components != null)
            {
                foreach (var componentElement in components.Elements())
                {
                    var component = ThreeMfComponent.ParseComponent(componentElement, resourceMap);
                    obj.Components.Add(component);
                }
            }

            return obj;
        }

        internal static ThreeMfObjectType ParseObjectType(string value)
        {
            switch (value)
            {
                case "model":
                case null:
                    return ThreeMfObjectType.Model;
                case "support":
                    return ThreeMfObjectType.Support;
                case "other":
                    return ThreeMfObjectType.Other;
                default:
                    throw new ThreeMfParseException($"Invalid object type '{value}'.");
            }
        }
    }
}
