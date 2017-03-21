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
<metadata name=""Description"">line 1</metadata>
<metadata name=""Description"">line 2</metadata>
");
            Assert.Equal("some title", model.Title);
            Assert.Equal("line 1\r\nline 2", model.Description);
            Assert.Null(model.Copyright);
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

        [Fact]
        public void ReadModelItemTest()
        {
            var model = FromContent(@"
<resources>
  <object id=""1"" partnumber=""object part number"">
    <mesh>
      <vertices />
      <triangles />
    </mesh>
  </object>
</resources>
<build>
  <item objectid=""1"" partnumber=""some part number"" />
</build>
");

            Assert.Equal("object part number", ((ThreeMfObject)model.Resources.Single()).PartNumber);
            Assert.Equal("some part number", model.Items.Single().PartNumber);
            Assert.True(ReferenceEquals(model.Resources.Single(), model.Items.Single().Object));
        }

        [Fact]
        public void ReadModelItemTransformMatrixTest()
        {
            var model = FromContent(@"
<resources>
  <object id=""1"">
    <mesh>
      <vertices />
      <triangles />
    </mesh>
  </object>
</resources>
<build>
  <item objectid=""1"" transform=""1 2 3 4 5 6 7 8 9 10 11 12"" />
</build>
");

            var expected = new ThreeMfMatrix(1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0, 12.0);
            Assert.Equal(expected, model.Items.Single().Transform);
        }

        [Fact]
        public void ReadModelComponentsTest()
        {
            var model = FromContent(@"
<resources>
  <object id=""1"" name=""first"">
    <mesh>
      <vertices />
      <triangles />
    </mesh>
  </object>
  <object id=""2"" name=""second"">
    <mesh>
      <vertices />
      <triangles />
    </mesh>
    <components>
      <component objectid=""1"" transform=""1 2 3 4 5 6 7 8 9 10 11 12"" />
    </components>
  </object>
</resources>
");

            Assert.Equal(2, model.Resources.Count);
            var first = (ThreeMfObject)(model.Resources.First());
            var second = (ThreeMfObject)(model.Resources.Last());
            Assert.Equal("first", first.Name);
            Assert.Equal("second", second.Name);

            var expected = new ThreeMfMatrix(1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0, 12.0);
            Assert.True(ReferenceEquals(first, second.Components.Single().Object));
            Assert.Equal(expected, second.Components.Single().Transform);
        }
    }
}
