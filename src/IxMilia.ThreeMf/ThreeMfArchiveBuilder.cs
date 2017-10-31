// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace IxMilia.ThreeMf
{
    internal class ThreeMfArchiveBuilder : IDisposable
    {
        private const string ContentTypesNamespace = "http://schemas.openxmlformats.org/package/2006/content-types";
        private const string RelsExtension = "rels";
        private const string ModelExtension = "model";
        private const string ExtensionAttributeName = "Extension";
        private const string ContentTypeAttributeName = "ContentType";
        private const string PartNameAttributeName = "PartName";
        private const string DefaultRelationshipPrefix = "rel";
        private const string ContentTypesPath = "/[Content_Types].xml";
        private const string ModelRelationshipPath = "/3D/_rels/3dmodel.model.rels";

        private const string ModelContentType = "application/vnd.ms-package.3dmanufacturing-3dmodel+xml";
        private const string RelsContentType = "application/vnd.openxmlformats-package.relationships+xml";

        private static XName TypesName = XName.Get("Types", ContentTypesNamespace);
        private static XName DefaultName = XName.Get("Default", ContentTypesNamespace);
        internal static XName OverrideName = XName.Get("Override", ContentTypesNamespace);

        private ZipArchive _archive;
        private XElement _modelRelationships;
        private XElement _contentTypes;
        private HashSet<string> _seenContentTypes;
        private int _currentRelationshipId = 0;

        private static XmlWriterSettings WriterSettings = new XmlWriterSettings()
        {
            Encoding = Encoding.UTF8,
            Indent = true,
            IndentChars = "  "
        };

        public ThreeMfArchiveBuilder(ZipArchive archive)
        {
            _archive = archive;
            _contentTypes = new XElement(TypesName,
                GetDefaultContentType(RelsExtension, RelsContentType),
                GetDefaultContentType(ModelExtension, ModelContentType));
            _modelRelationships = new XElement(ThreeMfFile.RelationshipsName);
            _seenContentTypes = new HashSet<string>();
        }

        private static string GetNormalizedArchivePath(string fullPath)
        {
            if (fullPath == null)
            {
                throw new ArgumentNullException(nameof(fullPath));
            }

            return fullPath.Length > 0 && fullPath[0] == '/'
                ? fullPath.Substring(1)
                : fullPath;
        }

        public void WriteXmlToArchive(string fullPath, XElement xml)
        {
            if (xml == null)
            {
                throw new ArgumentNullException(nameof(xml));
            }

            var path = GetNormalizedArchivePath(fullPath);
            var entry = _archive.CreateEntry(path);
            using (var stream = entry.Open())
            using (var writer = XmlWriter.Create(stream, WriterSettings))
            {
                var document = new XDocument(xml);
                document.WriteTo(writer);
            }
        }

        public virtual void WriteBinaryDataToArchive(string fullPath, byte[] data, string relationshipType, string contentType, bool overrideContentType = false)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            // copy the content
            var path = GetNormalizedArchivePath(fullPath);
            var entry = _archive.CreateEntry(path);
            using (var archiveEntryStream = entry.Open())
            using (var writer = new BinaryWriter(archiveEntryStream))
            {
                writer.Write(data);
            }

            // ensure the content type is present
            var extension = Path.GetExtension(path).Substring(1); // trim off leading period
            if (overrideContentType)
            {
                // explicitly specify the content type
                var contentOverride = new XElement(OverrideName,
                    new XAttribute(PartNameAttributeName, fullPath),
                    new XAttribute(ContentTypeAttributeName, contentType));
                _contentTypes.Add(contentOverride);
            }
            else
            {
                // set the default content type
                if (_seenContentTypes.Add(extension))
                {
                    var contentTypeElement = GetDefaultContentType(extension, contentType);
                    _contentTypes.Add(contentTypeElement);
                }
            }

            _modelRelationships.Add(ThreeMfFile.GetRelationshipElement(fullPath, NextRelationshipId(), relationshipType));
        }

        private static XElement GetDefaultContentType(string extension, string contentType)
        {
            return new XElement(DefaultName,
                new XAttribute(ExtensionAttributeName, extension),
                new XAttribute(ContentTypeAttributeName, contentType));
        }

        public string NextRelationshipId()
        {
            var rel = DefaultRelationshipPrefix + _currentRelationshipId;
            _currentRelationshipId++;
            return rel;
        }

        #region IDisposable

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    WriteXmlToArchive(ContentTypesPath, _contentTypes);
                    if (_modelRelationships != null)
                    {
                        WriteXmlToArchive(ModelRelationshipPath, _modelRelationships);
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

    }
}
