// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Xunit;

namespace IxMilia.ThreeMf.Test
{
    public class ThreeMfModelTests
    {
        private string StripXmlns(string value)
        {
            return value.Replace(" xmlns=\"" + ThreeMfModel.ModelNamespace + "\"", "");
        }

        private void VerifyModelXml(string xml, ThreeMfModel model)
        {
            var actual = StripXmlns(model.ToXElement().ToString()).Replace(@" xml:lang=""en-US""", "");
            Assert.Equal(xml.Trim(), actual);
        }

        private void VerifyMeshXml(string xml, ThreeMfMesh mesh)
        {
            var actual = StripXmlns(mesh.ToXElement().ToString());
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
<model unit=""inch"">
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
<model unit=""millimeter"">
  <metadata name=""Title"">some title</metadata>
  <resources />
  <build />
</model>
", model);
        }

        [Fact]
        public void WriteSimpleMeshTest()
        {
            var mesh = new ThreeMfMesh();
            mesh.Triangles.Add(
                new ThreeMfTriangle(
                    new ThreeMfVertex(0.0, 0.0, 0.0),
                    new ThreeMfVertex(10.0, 0.0, 0.0),
                    new ThreeMfVertex(5.0, 10.0, 0.0)));
            VerifyMeshXml(@"
<mesh>
  <vertices>
    <vertex x=""0"" y=""0"" z=""0"" />
    <vertex x=""10"" y=""0"" z=""0"" />
    <vertex x=""5"" y=""10"" z=""0"" />
  </vertices>
  <triangles>
    <triangle v1=""0"" v2=""1"" v3=""2"" />
  </triangles>
</mesh>
", mesh);
        }
    }
}
