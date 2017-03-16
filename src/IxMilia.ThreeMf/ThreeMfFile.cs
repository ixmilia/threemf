// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;

namespace IxMilia.ThreeMf
{
    public class ThreeMfFile
    {
        private const string ModelPath = "3D/3dmodel.model";

        public IList<ThreeMfModel> Models { get; } = new List<ThreeMfModel>();

        public static ThreeMfFile Load(Stream stream)
        {
            using (var archive = new ZipArchive(stream))
            {
                var modelEntry = archive.GetEntry(ModelPath);
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
    }
}
