// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace IxMilia.ThreeMf.Test
{
    internal class ThreeMfTestArchiveBuilder : ThreeMfArchiveBuilder
    {
        public ThreeMfTestArchiveBuilder()
            : base(null)
        {
        }

        public override void WriteBinaryDataToArchive(string fullPath, byte[] data, string relationshipType, string contentType, bool overrideContentType = false)
        {
        }
    }
}
