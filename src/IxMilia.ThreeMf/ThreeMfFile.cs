// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace IxMilia.ThreeMf
{
    public class ThreeMfFile
    {
        private const string RelationshipNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";
        private const string ModelRelationshipType = "http://schemas.microsoft.com/3dmanufacturing/2013/01/3dmodel";

        public IList<ThreeMfModel> Models { get; } = new List<ThreeMfModel>();

        public static ThreeMfFile Load(Stream stream)
        {
            using (var archive = new ZipArchive(stream))
            {
                var modelFilePath = GetModelFilePath(archive);
                var modelEntry = archive.GetEntry(modelFilePath);
                if (modelEntry == null)
                {
                    throw new ThreeMfPackageException("Package does not contain a model.");
                }

                using (var modelStream = modelEntry.Open())
                {
                    var document = XDocument.Load(modelStream);
                    var model = ThreeMfModel.LoadXml(document.Root);
                    var file = new ThreeMfFile();
                    file.Models.Add(model); // assume one model for now
                    return file;
                }
            }
        }

        private static string GetModelFilePath(ZipArchive archive)
        {
            var relsEntry = archive.GetEntry("_rels/.rels");
            if (relsEntry == null)
            {
                throw new ThreeMfPackageException("Invalid package: missing relationship file.");
            }

            using (var relsStream = relsEntry.Open())
            {
                var document = XDocument.Load(relsStream);
                var firstRelationship = document.Root.Elements(XName.Get("Relationship", RelationshipNamespace)).FirstOrDefault(e => e.Attribute("Type")?.Value == ModelRelationshipType);
                if (firstRelationship == null)
                {
                    throw new ThreeMfPackageException("Package does not contain a root 3MF relation.");
                }

                var target = firstRelationship.Attribute("Target")?.Value;
                if (target == null)
                {
                    throw new ThreeMfPackageException("Relationship target not specified.");
                }

                if (target.StartsWith("/"))
                {
                    // ZipArchive doesn't like the leading slash
                    target = target.Substring(1);
                }

                return target;
            }
        }
    }
}
