// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Xml.Linq;

namespace IxMilia.ThreeMf
{
    public class ThreeMfObject : ThreeMfResource
    {
        // TODO:
        //   pid = reference to property group element with matching id attribute.  required if pindex is specified
        //   pindex = a zero-based index into the properties group specified by pid.  this is used to build the object
        //   thumbnail = path to a 3d texture of jpeg or png that represents a rendered image of the object
        //   components

        public int Id { get; private set; }
        public ThreeMfObjectType Type { get; set; }
        public string PartNumber { get; set; }
        public string Name { get; set; }
        public ThreeMfMesh Mesh { get; set; }

        public ThreeMfObject()
        {
            Type = ThreeMfObjectType.Model;
        }

        internal static ThreeMfObject ParseObject(XElement element)
        {
            var obj = new ThreeMfObject();
            obj.Id = ParseAttributeInt(element, "id", isRequired: true);
            obj.Type = ParseObjectType(element.Attribute("type")?.Value);
            obj.PartNumber = element.Attribute("partnumber")?.Value;
            obj.Name = element.Attribute("name")?.Value;
            obj.Mesh = ThreeMfMesh.ParseMesh(element.Element(XName.Get("mesh", ThreeMfModel.ModelNamespace)));
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
