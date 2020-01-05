// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using Xunit;

namespace IxMilia.ThreeMf.Test
{
    public class ThreeMfMiscTests : ThreeMfAbstractTestBase
    {
        private static ThreeMfModel RoundTripModel(ThreeMfModel model)
        {
            var file1 = new ThreeMfFile();
            file1.Models.Add(model);
            var file2 = RoundTripFile(file1);
            return file2.Models.Single();
        }

        [Fact]
        public void MatrixTransformTest()
        {
            var matrix = new ThreeMfMatrix(
                2.0, 0.0, 0.0, // 0.0
                0.0, 2.0, 0.0, // 0.0
                0.0, 0.0, 2.0, // 0.0
                10.0, 10.0, 10.0); // 1.0
            var vertex = new ThreeMfVertex(1.0, 1.0, 1.0);
            var result = matrix.Transform(vertex);
            var expected = new ThreeMfVertex(12.0, 12.0, 12.0);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void TextureRoundTripTest()
        {
            var model1 = new ThreeMfModel();
            model1.Resources.Add(new ThreeMfTexture2D(StringToBytes("jpeg texture"), ThreeMfImageContentType.Jpeg));
            var model2 = RoundTripModel(model1);
            Assert.Equal("jpeg texture", BytesToString(model2.Resources.Cast<ThreeMfTexture2D>().Single().TextureBytes));
        }

        [Fact]
        public void ThumbnailRoundTripTest()
        {
            var model1 = new ThreeMfModel();
            model1.Resources.Add(new ThreeMfObject() { ThumbnailData = StringToBytes("jpeg thumbnail"), ThumbnailContentType = ThreeMfImageContentType.Jpeg });
            var model2 = RoundTripModel(model1);
            Assert.Equal("jpeg thumbnail", BytesToString(model2.Resources.Cast<ThreeMfObject>().Single().ThumbnailData));
        }
    }
}
