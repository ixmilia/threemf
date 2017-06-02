// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using IxMilia.ThreeMf.Collections;
using IxMilia.ThreeMf.Extensions;

namespace IxMilia.ThreeMf
{
    public class ThreeMfFile
    {
        private const string ContentTypesNamespace = "http://schemas.openxmlformats.org/package/2006/content-types";
        private const string RelationshipNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";
        private const string ModelRelationshipType = "http://schemas.microsoft.com/3dmanufacturing/2013/01/3dmodel";
        private const string TextureRelationshipType = "http://schemas.microsoft.com/3dmanufacturing/2013/01/3dtexture";
        private const string ExtensionAttributeName = "Extension";
        private const string ContentTypeAttributeName = "ContentType";
        private const string RelsExtension = "rels";
        private const string ModelExtension = "model";
        private const string JpegExtension = "jpg";
        private const string PngExtension = "png";
        private const string RelsContentType = "application/vnd.openxmlformats-package.relationships+xml";
        private const string ModelContentType = "application/vnd.ms-package.3dmanufacturing-3dmodel+xml";
        private const string TextureContentType = "application/vnd.ms-package.3dmanufacturing-3dmodeltexture";
        private const string ContentTypesPath = "[Content_Types].xml";
        private const string DefaultModelEntryName = "/3D/3dmodel";
        private const string ModelPathExtension = ".model";
        private const string RelsEntryPath = "_rels/.rels";
        private const string ModelRelationshipPath = "3D/_rels/3dmodel.model.rels";
        private const string DefaultRelationshipPrefix = "rel";
        private const string TargetAttributeName = "Target";
        private const string IdAttributeName = "Id";
        private const string TypeAttributeName = "Type";

        private static XName TypesName = XName.Get("Types", ContentTypesNamespace);
        private static XName DefaultName = XName.Get("Default", ContentTypesNamespace);
        private static XName RelationshipsName = XName.Get("Relationships", RelationshipNamespace);
        private static XName RelationshipName = XName.Get("Relationship", RelationshipNamespace);

        private static XmlWriterSettings WriterSettings = new XmlWriterSettings()
        {
            Encoding = Encoding.UTF8,
            Indent = true,
            IndentChars = "  "
        };

        public IList<ThreeMfModel> Models { get; } = new ListNonNull<ThreeMfModel>();

        public void Save(Stream stream)
        {
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                var currentRelationshipId = 0;
                var nextRelationshipId = new Func<string>(() =>
                {
                    var rel = DefaultRelationshipPrefix + currentRelationshipId;
                    currentRelationshipId++;
                    return rel;
                });
                var currentModelSuffix = 0;
                var nextModelFileName = new Func<string>(() =>
                {
                    var suffix = currentModelSuffix++ == 0 ? string.Empty : $"-{currentModelSuffix}";
                    return string.Concat(DefaultModelEntryName, suffix, ModelPathExtension);
                });
                var seenContentTypes = new HashSet<string>() { "rels", "model" };
                var contentTypes = new XElement(TypesName,
                    GetDefaultContentType(RelsExtension, RelsContentType),
                    GetDefaultContentType(ModelExtension, ModelContentType));

                var modelPaths = Enumerable.Range(0, Models.Count).Select(_ => nextModelFileName()).ToList();
                var rootRels = new XElement(RelationshipsName,
                    modelPaths.Select(path => GetRelationshipElement(path, nextRelationshipId(), ModelRelationshipType)));
                WriteXmlToArchive(archive, rootRels, RelsEntryPath);

                XElement modelRels = null;
                var addArchiveEntry = new Action<string, byte[]>((fullPath, data) =>
                {
                    if (fullPath == null || fullPath[0] != '/')
                    {
                        throw new InvalidOperationException($"Invalid archive entry path '{fullPath}'.");
                    }

                    if (data == null)
                    {
                        throw new ArgumentNullException(nameof(data));
                    }

                    // copy the content
                    var path = fullPath.Substring(1);
                    var entry = archive.CreateEntry(path);
                    using (var archiveEntryStream = entry.Open())
                    using (var writer = new BinaryWriter(archiveEntryStream))
                    {
                        writer.Write(data);
                    }

                    // ensure the content type is present
                    var extension = Path.GetExtension(path).Substring(1); // trim off leading period
                    if (seenContentTypes.Add(extension))
                    {
                        contentTypes.Add(GetContentTypeFromExtension(extension));
                    }

                    // ensure the relationships are present
                    if (modelRels == null)
                    {
                        modelRels = new XElement(RelationshipsName);
                    }

                    modelRels.Add(GetRelationshipElement(fullPath, nextRelationshipId(), TextureRelationshipType));
                });

                for (int i = 0; i < Models.Count; i++)
                {
                    var model = Models[i];
                    var modelXml = model.ToXElement(addArchiveEntry);
                    var modelPath = modelPaths[i];
                    var modelArchivePath = modelPath.Substring(1); // trim the leading slash for ZipArchive
                    WriteXmlToArchive(archive, modelXml, modelArchivePath);
                    WriteXmlToArchive(archive, contentTypes, ContentTypesPath);
                }

                if (modelRels != null)
                {
                    WriteXmlToArchive(archive, modelRels, ModelRelationshipPath);
                }
            }
        }

        private static XElement GetDefaultContentType(string extension, string contentType)
        {
            return new XElement(DefaultName,
                new XAttribute(ExtensionAttributeName, extension),
                new XAttribute(ContentTypeAttributeName, contentType));
        }

        private static XElement GetContentTypeFromExtension(string extension)
        {
            switch (extension)
            {
                case JpegExtension:
                case PngExtension:
                    return GetDefaultContentType(extension, TextureContentType);
                default:
                    throw new InvalidOperationException($"Unsupported content type extension '{extension}'.");
            }
        }

        private static XElement GetRelationshipElement(string target, string id, string type)
        {
            return new XElement(RelationshipName,
                new XAttribute(TargetAttributeName, target),
                new XAttribute(IdAttributeName, id),
                new XAttribute(TypeAttributeName, type));
        }

        private static void WriteXmlToArchive(ZipArchive archive, XElement xml, string path)
        {
            var entry = archive.CreateEntry(path);
            using (var stream = entry.Open())
            using (var writer = XmlWriter.Create(stream, WriterSettings))
            {
                var document = new XDocument(xml);
                document.WriteTo(writer);
            }
        }

        public static ThreeMfFile Load(Stream stream)
        {
            var file = new ThreeMfFile();
            using (var archive = new ZipArchive(stream))
            {
                var relationshipDocument = GetRootRelationshipFile(archive);
                foreach (var modelPath in GetModelFilePaths(relationshipDocument))
                {
                    using (var modelStream = archive.GetEntryStream(modelPath))
                    {
                        var document = XDocument.Load(modelStream);
                        var model = ThreeMfModel.LoadXml(document.Root, entryPath => archive.GetEntryBytes(entryPath));
                        file.Models.Add(model);
                    }
                }
            }

            return file;
        }

        private static XDocument GetRootRelationshipFile(ZipArchive archive)
        {
            using (var relsStream = archive.GetEntryStream(RelsEntryPath))
            {
                var document = XDocument.Load(relsStream);
                return document;
            }
        }

        private static IEnumerable<XElement> GetRelationshipsOfType(XDocument relationshipDocument, string relationshipType)
        {
            return relationshipDocument.Root.Elements(RelationshipName).Where(e => e.Attribute(TypeAttributeName)?.Value == relationshipType);
        }

        private static IEnumerable<string> GetModelFilePaths(XDocument relationshipDocument)
        {
            return GetRelationshipsOfType(relationshipDocument, ModelRelationshipType)
                .Select(rel => rel.AttributeValueOrThrow(TargetAttributeName, "Relationship target not specified."))
                .Select(path => path.StartsWith("/") ? path.Substring(1) : path); // ZipArchive doesn't like the leading slash
        }
    }
}
