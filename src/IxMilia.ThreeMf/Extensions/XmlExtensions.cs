// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Xml.Linq;

namespace IxMilia.ThreeMf.Extensions
{
    internal static class XmlExtensions
    {
        public static string AttributeValueOrThrow(this XElement element, string attributeName, string errorMessage)
        {
            var attribute = element.Attribute(attributeName);
            if (attribute == null)
            {
                throw new ThreeMfParseException(errorMessage);
            }

            return attribute.Value;
        }

        public static string AttributeValueOrThrow(this XElement element, string attributeName)
        {
            return element.AttributeValueOrThrow(attributeName, $"Expected attribute '{attributeName}'.");
        }

        public static int AttributeIntValueOrThrow(this XElement element, string attributeName)
        {
            if (!int.TryParse(element.AttributeValueOrThrow(attributeName), out var value))
            {
                throw new ThreeMfParseException($"Unable to parse attribute '{attributeName}' as an int.");
            }

            return value;
        }

        public static double AttributeDoubleValueOrThrow(this XElement element, string attributeName)
        {
            if (!double.TryParse(element.AttributeValueOrThrow(attributeName), out var value))
            {
                throw new ThreeMfParseException($"Unable to parse  attribute '{attributeName}' as a double.");
            }

            return value;
        }
    }
}
