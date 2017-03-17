// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace IxMilia.ThreeMf.Test
{
    public class ThreeMfModelLoadTests
    {
        internal static ThreeMfModel ParseXml(string contents, IEnumerable<string> additionalSupportedNamespaces = null)
        {
            var document = XDocument.Parse(contents);
            return ThreeMfModel.LoadXml(document.Root, additionalSupportedNamespaces);
        }

        private ThreeMfModel FromContent(string content)
        {
            return ParseXml($@"<model xmlns=""{ThreeMfModel.ModelNamespace}"">" + content + "</model>");
        }

        private void AssertUnits(string unitsString, ThreeMfModelUnits expectedUnits)
        {
            var model = ParseXml($@"<model unit=""{unitsString}"" xml:lang=""en-US"" xmlns=""{ThreeMfModel.ModelNamespace}""></model>");
            Assert.Equal(expectedUnits, model.ModelUnits);
        }

        [Fact]
        public void ReadModelUnitTest()
        {
            AssertUnits("micron", ThreeMfModelUnits.Micron);
            AssertUnits("millimeter", ThreeMfModelUnits.Millimeter);
            AssertUnits("centimeter", ThreeMfModelUnits.Centimeter);
            AssertUnits("inch", ThreeMfModelUnits.Inch);
            AssertUnits("foot", ThreeMfModelUnits.Foot);
            AssertUnits("meter", ThreeMfModelUnits.Meter);

            Assert.Throws<ThreeMfParseException>(() => ParseXml($@"<model unit=""mile"" xml:lang=""en-US"" xmlns=""{ThreeMfModel.ModelNamespace}""></model>"));
        }

        [Fact]
        public void MetadataReaderTest()
        {
            var model = FromContent(@"
<metadata name=""Title"">some title</metadata>
");
            Assert.Equal("some title", model.Title);
            Assert.Null(model.Description);
        }

        [Fact]
        public void ParseSimpleMeshTest()
        {
            var model = FromContent(@"
<resources>
  <object id=""3"" type=""support"">
    <mesh>
      <vertices>
        <vertex x=""0"" y=""0"" z=""0"" />
        <vertex x=""10"" y=""0"" z=""0"" />
        <vertex x=""5"" y=""10"" z=""0"" />
        <vertex x=""5"" y=""5"" z=""5"" />
      </vertices>
      <triangles>
        <triangle v1=""0"" v2=""1"" v3=""3"" />
        <triangle v1=""0"" v2=""2"" v3=""3"" />
        <triangle v1=""1"" v2=""2"" v3=""3"" />
        <triangle v1=""0"" v2=""1"" v3=""2"" />
      </triangles>
    </mesh>
  </object>
</resources>
");
            var obj = (ThreeMfObject)model.Resources.Single();
            Assert.Equal(3, obj.Id);
            Assert.Equal(ThreeMfObjectType.Support, obj.Type);

            var mesh = obj.Mesh;
            Assert.Equal(4, mesh.Triangles.Count);
            Assert.Equal(5.0, mesh.Triangles.First().V3.Z);
        }
    }
}
