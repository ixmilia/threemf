// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;
using System.IO.Compression;
using System.Xml.Linq;

namespace IxMilia.ThreeMf
{
    public class ThreeMfFile
    {
        public const string ModelNamespace = "http://schemas.microsoft.com/3dmanufacturing/core/2015/02";
        private const string ModelPath = "3D/3dmodel.model";

        public ThreeMfModelUnits ModelUnits { get; set; }

        private void ParseModelUnits(string value)
        {
            switch (value)
            {
                case "micron":
                    ModelUnits = ThreeMfModelUnits.Micron;
                    break;
                case "millimeter":
                    ModelUnits = ThreeMfModelUnits.Millimeter;
                    break;
                case "centimeter":
                    ModelUnits = ThreeMfModelUnits.Centimeter;
                    break;
                case "inch":
                    ModelUnits = ThreeMfModelUnits.Inch;
                    break;
                case "foot":
                    ModelUnits = ThreeMfModelUnits.Foot;
                    break;
                case "meter":
                    ModelUnits = ThreeMfModelUnits.Meter;
                    break;
                default:
                    throw new ThreeMfParseException($"Unsupported model unit '{value}'");
            }
        }

        public static ThreeMfFile LoadXml(XElement root)
        {
            var file = new ThreeMfFile();
            file.ParseModelUnits(root.Attribute("unit").Value);
            return file;
        }

        public static ThreeMfFile Load(Stream stream)
        {
            using (var archive = new ZipArchive(stream))
            {
                var modelEntry = archive.GetEntry(ModelPath);
                using (var modelStream = modelEntry.Open())
                {
                    var document = XDocument.Load(modelStream);
                    var file = LoadXml(document.Root);
                    return file;
                }
            }
        }
    }
}
