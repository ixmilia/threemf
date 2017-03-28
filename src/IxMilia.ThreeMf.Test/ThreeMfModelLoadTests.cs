// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace IxMilia.ThreeMf.Test
{
    public class ThreeMfModelLoadTests
    {
        internal static ThreeMfModel ParseXml(string contents)
        {
            var document = XDocument.Parse(contents);
            var getArchiveEntry = new Func<string, byte[]>(_ => new byte[0]);
            return ThreeMfModel.LoadXml(document.Root, getArchiveEntry);
        }

        private ThreeMfModel FromContent(string content)
        {
            return ParseXml($@"<model xmlns=""{ThreeMfModel.ModelNamespace}"" xmlns:m=""{ThreeMfModel.MaterialNamespace}"">" +
                content +
                "</model>");
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
            Assert.Null(mesh.Triangles.First().PropertyResource);
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

        [Fact]
        public void ReadBaseMaterialsTest()
        {
            var model = FromContent(@"
<resources>
  <basematerials id=""1"">
    <base name=""blue"" displaycolor=""#0000FF00"" />
    <base name=""green no alpha"" displaycolor=""#00FF00"" />
  </basematerials>
</resources>
");

            var baseMaterials = (ThreeMfBaseMaterials)model.Resources.Single();
            Assert.Equal(2, baseMaterials.Bases.Count);

            Assert.Equal("blue", baseMaterials.Bases.First().Name);
            Assert.Equal(new ThreeMfsRGBColor(0, 0, 255, 0), baseMaterials.Bases.First().Color);

            Assert.Equal("green no alpha", baseMaterials.Bases.Last().Name);
            Assert.Equal(new ThreeMfsRGBColor(0, 255, 0), baseMaterials.Bases.Last().Color);
        }

        [Fact]
        public void ReadTriangleVertexPropertiesTest()
        {
            var model = FromContent(@"
<resources>
  <basematerials id=""1"">
    <base name=""white"" displaycolor=""#FFFFFF"" />
    <base name=""black"" displaycolor=""#000000"" />
  </basematerials>
  <object id=""2"">
    <mesh>
      <vertices>
        <vertex x=""0"" y=""0"" z=""0"" />
        <vertex x=""0"" y=""0"" z=""0"" />
        <vertex x=""0"" y=""0"" z=""0"" />
      </vertices>
      <triangles>
        <triangle v1=""0"" v2=""1"" v3=""2"" pid=""1"" p1=""0"" p3=""1"" />
      </triangles>
    </mesh>
  </object>
</resources>
");

            var triangle = ((ThreeMfObject)model.Resources.Last()).Mesh.Triangles.Single();
            var propertyResource = triangle.PropertyResource;
            Assert.Equal("white", ((ThreeMfBase)propertyResource.PropertyItems.First()).Name);
            Assert.Equal(0, triangle.V1PropertyIndex);
            Assert.Null(triangle.V2PropertyIndex);
            Assert.Equal(1, triangle.V3PropertyIndex);
        }

        [Fact]
        public void ReadTriangleVertexPropertiesFromUnsupportedResourceTest()
        {
            var model = ParseXml($@"
<model xmlns=""{ThreeMfModel.ModelNamespace}"" xmlns:x=""http://www.ixmilia.com"">
  <resources>
    <x:unsupported id=""1"" />
    <object id=""2"">
      <mesh>
        <vertices>
          <vertex x=""0"" y=""0"" z=""0"" />
          <vertex x=""0"" y=""0"" z=""0"" />
          <vertex x=""0"" y=""0"" z=""0"" />
        </vertices>
        <triangles>
          <triangle v1=""0"" v2=""1"" v3=""2"" pid=""1"" p1=""0"" p2=""0"" p3=""1"" />
        </triangles>
      </mesh>
    </object>
  </resources>
</model>
");

            var triangle = ((ThreeMfObject)model.Resources.Last()).Mesh.Triangles.Single();
            Assert.Null(triangle.PropertyResource);
        }

        [Fact]
        public void ReadObjectPropertiesTest()
        {
            var model = FromContent(@"
<resources>
  <basematerials id=""1"">
    <base name=""white"" displaycolor=""#FFFFFF"" />
  </basematerials>
  <object id=""2"" pid=""1"" pindex=""0"">
    <mesh>
      <vertices />
      <triangles />
    </mesh>
  </object>
</resources>
");

            var obj = (ThreeMfObject)model.Resources.Last();
            var propertyResource = obj.PropertyResource;
            Assert.Equal("white", ((ThreeMfBase)propertyResource.PropertyItems.First()).Name);
            Assert.Equal(0, obj.PropertyIndex);
        }

        [Fact]
        public void ReadObjectPropertiesFromUnsupportedResourceTest()
        {
            var model = ParseXml($@"
<model xmlns=""{ThreeMfModel.ModelNamespace}"" xmlns:x=""http://www.ixmilia.com"">
  <resources>
    <x:unsupported id=""1"" />
    <object id=""2"" pid=""1"" pindex=""0"">
      <mesh>
      <vertices />
      <triangles />
    </mesh>
    </object>
  </resources>
</model>
");

            var obj = (ThreeMfObject)model.Resources.Last();
            Assert.Null(obj.PropertyResource);
        }

        [Fact]
        public void ReadColorGroupTest()
        {
            var model = FromContent(@"
<resources>
  <m:colorgroup id=""1"">
    <m:color color=""#0000FF00"" />
  </m:colorgroup>
</resources>
");

            var colorGroup = (ThreeMfColorGroup)model.Resources.Single();
            Assert.Equal(new ThreeMfsRGBColor(0, 0, 255, 0), colorGroup.Colors.Single().Color);
        }

        [Fact]
        public void ReadTexture2DTest()
        {
            var model = FromContent(@"
<resources>
  <m:texture2d id=""1"" path=""/3D/Textures/texture.png"" contenttype=""image/png"" box=""0 1 2 3"" tilestyleu=""mirror"" />
</resources>
");

            var texture = (ThreeMfTexture2D)model.Resources.Single();
            Assert.Equal(ThreeMfTextureContentType.Png, texture.ContentType);
            Assert.Equal(0.0, texture.BoundingBox.U);
            Assert.Equal(1.0, texture.BoundingBox.V);
            Assert.Equal(2.0, texture.BoundingBox.Width);
            Assert.Equal(3.0, texture.BoundingBox.Height);
            Assert.Equal(ThreeMfTileStyle.Mirror, texture.TileStyleU);
            Assert.Equal(ThreeMfTileStyle.Wrap, texture.TileStyleV);
        }

        [Fact]
        public void ReadTexture2DGroupTest()
        {
            var model = FromContent(@"
<resources>
  <m:texture2d id=""1"" path=""/3D/Textures/texture.png"" contenttype=""image/png"" />
  <m:texture2dgroup id=""2"" texid=""1"">
    <m:tex2coord u=""1"" v=""2"" />
  </m:texture2dgroup>
</resources>
");

            var texture = (ThreeMfTexture2D)model.Resources.First();
            var textureGroup = (ThreeMfTexture2DGroup)model.Resources.Last();
            Assert.True(ReferenceEquals(texture, textureGroup.Texture));
            Assert.Equal(1.0, textureGroup.Coordinates.Single().U);
            Assert.Equal(2.0, textureGroup.Coordinates.Single().V);
        }
    }
}
