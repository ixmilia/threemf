// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
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

        private ThreeMfModel ParseXml(string contents, IEnumerable<string> additionalSupportedNamespaces = null)
        {
            return ThreeMfModelLoadTests.ParseXml(contents, additionalSupportedNamespaces);
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

        [Fact]
        public void ReadRequiredExtensionsTest()
        {
            var xml = $@"<model requiredextensions=""i"" xmlns=""{ThreeMfModel.ModelNamespace}"" xmlns:i=""http://www.ixmilia.com"" />";

            // unsupported required namespace
            Assert.Throws<ThreeMfParseException>(() => ParseXml(xml));

            // supported required namespace
            var model = ParseXml(xml, additionalSupportedNamespaces: new[] { "http://www.ixmilia.com" });
            Assert.Equal("http://www.ixmilia.com", model.RequiredExtensionNamespaces.Single());
        }

        [Fact]
        public void WriteRequiredExtensionsTest()
        {
            var model = new ThreeMfModel();
            model.RequiredExtensionNamespaces.Add("http://www.ixmilia.com");
            VerifyModelXml(@"
<model unit=""millimeter"" requiredextensions=""a"" xmlns:a=""http://www.ixmilia.com"">
  <resources />
  <build />
</model>
", model);
        }
    }
}
