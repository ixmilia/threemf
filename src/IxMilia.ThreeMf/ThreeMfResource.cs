// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Xml.Linq;

namespace IxMilia.ThreeMf
{
    public abstract class ThreeMfResource
    {
        protected const string IdAttributeName = "id";

        protected static XName ObjectName = XName.Get("object", ThreeMfModel.ModelNamespace);
        internal static XName BaseMaterialsName = XName.Get("basematerials", ThreeMfModel.ModelNamespace);

        public int Id { get; internal set; }

        abstract internal XElement ToXElement(Dictionary<ThreeMfResource, int> resourceMap);

        internal static ThreeMfResource ParseResource(XElement element, Dictionary<int, ThreeMfResource> resourceMap)
        {
            if (element.Name == ObjectName)
            {
                return ThreeMfObject.ParseObject(element, resourceMap);
            }
            else if (element.Name == BaseMaterialsName)
            {
                return ThreeMfBaseMaterials.ParseBaseMaterials(element);
            }
            else
            {
                return null;
            }
        }
    }
}
