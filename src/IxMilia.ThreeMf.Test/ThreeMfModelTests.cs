// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Xunit;

namespace IxMilia.ThreeMf.Test
{
    public class ThreeMfModelTests
    {
        private void VerifyModelXml(string xml, ThreeMfModel model)
        {
            var actual = model.ToXElement().ToString();
            Assert.Equal(xml.Trim(), actual);
        }

        [Fact]
        public void DefaultUnitTest()
        {
            Assert.Equal(ThreeMfModelUnits.Millimeter, new ThreeMfModel().ModelUnits);
        }

        [Fact]
        public void WriteSimpleModelTest()
        {
            var model = new ThreeMfModel();
            model.ModelUnits = ThreeMfModelUnits.Inch;
            model.Resources.Add(new ThreeMfObject());
            VerifyModelXml(@"
<model unit=""inch"" xml:lang=""en-US"" xmlns=""http://schemas.microsoft.com/3dmanufacturing/core/2015/02"">
  <resources>
    <object id=""1"" type=""model"">
      <mesh>
        <vertices />
        <triangles />
      </mesh>
    </object>
  </resources>
  <build>
    <item objectid=""1"" />
  </build>
</model>
", model);
        }

        [Fact]
        public void WriteMetadataTest()
        {
            var model = new ThreeMfModel();
            model.Title = "some title";
            VerifyModelXml(@"
<model unit=""millimeter"" xml:lang=""en-US"" xmlns=""http://schemas.microsoft.com/3dmanufacturing/core/2015/02"">
  <metadata name=""Title"">some title</metadata>
  <resources />
  <build />
</model>
", model);
        }
    }
}
