// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace IxMilia.ThreeMf
{
    public abstract class ThreeMfResource
    {
        protected const string IdAttributeName = "id";

        protected static XName ObjectName = XName.Get("object", ThreeMfModel.ModelNamespace);
        internal static XName BaseMaterialsName = XName.Get("basematerials", ThreeMfModel.ModelNamespace);
        internal static XName ColorGroupName = XName.Get("colorgroup", ThreeMfModel.MaterialNamespace);
        internal static XName Texture2DName = XName.Get("texture2d", ThreeMfModel.MaterialNamespace);
        internal static XName Texture2DGroupName = XName.Get("texture2dgroup", ThreeMfModel.MaterialNamespace);

        public int Id { get; internal set; }

        abstract internal XElement ToXElement(Dictionary<ThreeMfResource, int> resourceMap, Action<string, Stream> addArchiveEntry);

        internal static ThreeMfResource ParseResource(XElement element, Dictionary<int, ThreeMfResource> resourceMap, Func<string, Stream> getArchiveEntry)
        {
            if (element.Name == ObjectName)
            {
                return ThreeMfObject.ParseObject(element, resourceMap);
            }
            else if (element.Name == BaseMaterialsName)
            {
                return ThreeMfBaseMaterials.ParseBaseMaterials(element);
            }
            else if (element.Name == ColorGroupName)
            {
                return ThreeMfColorGroup.ParseColorGroup(element);
            }
            else if (element.Name == Texture2DName)
            {
                return ThreeMfTexture2D.ParseTexture(element, getArchiveEntry);
            }
            else if (element.Name == Texture2DGroupName)
            {
                return ThreeMfTexture2DGroup.ParseTexture2DGroup(element, resourceMap);
            }
            else
            {
                return null;
            }
        }
    }
}
