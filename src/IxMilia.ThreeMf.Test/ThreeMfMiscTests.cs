// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Xunit;

namespace IxMilia.ThreeMf.Test
{
    public class ThreeMfMiscTests
    {
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
    }
}
