﻿// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using IxMilia.ThreeMf.Collections;

namespace IxMilia.ThreeMf
{
    public class ThreeMfBaseMaterials : ThreeMfResource
    {
        public IList<ThreeMfBase> Bases { get; } = new ListNonNull<ThreeMfBase>();

        internal override XElement ToXElement(Dictionary<ThreeMfResource, int> resourceMap)
        {
            return new XElement(BaseMaterialsName,
                new XAttribute(IdAttributeName, Id),
                Bases.Select(b => b.ToXElement()));
        }

        internal static ThreeMfBaseMaterials ParseBaseMaterials(XElement element)
        {
            var baseMaterials = new ThreeMfBaseMaterials();
            baseMaterials.Id = ParseAttributeInt(element, IdAttributeName, isRequired: true);
            foreach (var baseElement in element.Elements(ThreeMfBase.BaseName))
            {
                var baseMaterial = ThreeMfBase.ParseBaseMaterial(baseElement);
                baseMaterials.Bases.Add(baseMaterial);
            }

            return baseMaterials;
        }
    }
}
