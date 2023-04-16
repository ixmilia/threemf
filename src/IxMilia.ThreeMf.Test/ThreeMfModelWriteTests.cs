using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using Xunit;

namespace IxMilia.ThreeMf.Test
{
    public class ThreeMfModelWriteTests : ThreeMfAbstractTestBase
    {
        private string StripXmlns(string value)
        {
            // don't want to specify this in every test
            return value.Replace(" xmlns=\"" + ThreeMfModel.ModelNamespace + "\"", "");
        }

        private string GetStrippedModelXml(ThreeMfModel model)
        {
            // don't want to specify the defaults in every test
            var dummyPackage = Package.Open(new MemoryStream(), FileMode.Create);
            return StripXmlns(model.ToXElement(dummyPackage).ToString())
                .Replace(@" xml:lang=""en-US""", "")
                .Replace(@" unit=""millimeter""", "");
        }

        private void VerifyModelXml(string xml, ThreeMfModel model)
        {
            var actual = GetStrippedModelXml(model).Replace("\r", "");
            Assert.Equal(xml.Replace("\r", "").Trim(), actual);
        }

        private void VerifyMeshXml(string xml, ThreeMfMesh mesh)
        {
            var actual = StripXmlns(mesh.ToXElement(new Dictionary<ThreeMfResource, int>()).ToString()).Replace("\r", "");
            Assert.Equal(xml.Replace("\r", "").Trim(), actual);
        }

        private ThreeMfModel ParseXml(string contents)
        {
            return ThreeMfModelLoadTests.ParseXml(contents);
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
  <build />
</model>
", model);
        }

        [Fact]
        public void WriteMetadataTest()
        {
            var model = new ThreeMfModel();
            model.Title = "some title";
            model.Description = "line 1\nline 2";
            VerifyModelXml(@"
<model>
  <metadata name=""Title"">some title</metadata>
  <metadata name=""Description"">line 1</metadata>
  <metadata name=""Description"">line 2</metadata>
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
        public void ReadSupportedRequiredExtensionsTest()
        {
            var model = ParseXml($@"<model requiredextensions=""m"" xmlns=""{ThreeMfModel.ModelNamespace}"" xmlns:m=""{ThreeMfModel.MaterialNamespace}"" />");
        }

        [Fact]
        public void ReadUnsupportedRequiredExtensionsTest()
        {
            Assert.Throws<ThreeMfParseException>(() => ParseXml($@"<model requiredextensions=""i"" xmlns=""{ThreeMfModel.ModelNamespace}"" xmlns:i=""http://www.ixmilia.com"" />"));
        }

        [Fact]
        public void WriteBuildItemTest()
        {
            var model = new ThreeMfModel();
            var obj = new ThreeMfObject();
            model.Resources.Add(obj);
            model.Items.Add(new ThreeMfModelItem(obj) { PartNumber = "part number" });
            VerifyModelXml(@"
<model>
  <resources>
    <object id=""1"" type=""model"">
      <mesh>
        <vertices />
        <triangles />
      </mesh>
    </object>
  </resources>
  <build>
    <item objectid=""1"" partnumber=""part number"" />
  </build>
</model>
", model);
        }

        [Fact]
        public void WriteBuildItemWithTransformMatrixTest()
        {
            var model = new ThreeMfModel();
            var obj = new ThreeMfObject();
            model.Resources.Add(obj);
            model.Items.Add(new ThreeMfModelItem(obj) { Transform = new ThreeMfMatrix(1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0, 12.0) });
            VerifyModelXml(@"
<model>
  <resources>
    <object id=""1"" type=""model"">
      <mesh>
        <vertices />
        <triangles />
      </mesh>
    </object>
  </resources>
  <build>
    <item objectid=""1"" transform=""1 2 3 4 5 6 7 8 9 10 11 12"" />
  </build>
</model>
", model);
        }

        [Fact]
        public void WriteComponentTest()
        {
            var model = new ThreeMfModel();
            var first = new ThreeMfObject();
            first.Name = "first";
            model.Resources.Add(first);
            var second = new ThreeMfObject();
            second.Name = "second";
            second.Components.Add(new ThreeMfComponent(first, new ThreeMfMatrix(1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0, 12.0)));
            model.Resources.Add(second);
            VerifyModelXml(@"
<model>
  <resources>
    <object id=""1"" type=""model"" name=""first"">
      <mesh>
        <vertices />
        <triangles />
      </mesh>
    </object>
    <object id=""2"" type=""model"" name=""second"">
      <mesh>
        <vertices />
        <triangles />
      </mesh>
      <components>
        <component objectid=""1"" transform=""1 2 3 4 5 6 7 8 9 10 11 12"" />
      </components>
    </object>
  </resources>
  <build />
</model>
", model);
        }

        [Fact]
        public void IncludeAllResourcesTest()
        {
            // ensure that improperly built models still write out all resources
            var model = new ThreeMfModel();
            var obj = new ThreeMfObject() { Name = "build item" };
            obj.Components.Add(new ThreeMfComponent(new ThreeMfObject() { Name = "component" }, ThreeMfMatrix.Identity));
            model.Items.Add(new ThreeMfModelItem(obj));

            // note that as of here, no objects have ever been added to `model.Resources`
            Assert.Equal(0, model.Resources.Count);

            // but calling `.ToXElement()` will force it to be populated appropriately
            VerifyModelXml(@"
<model>
  <resources>
    <object id=""1"" type=""model"" name=""component"">
      <mesh>
        <vertices />
        <triangles />
      </mesh>
    </object>
    <object id=""2"" type=""model"" name=""build item"">
      <mesh>
        <vertices />
        <triangles />
      </mesh>
      <components>
        <component objectid=""1"" />
      </components>
    </object>
  </resources>
  <build>
    <item objectid=""2"" />
  </build>
</model>
", model);
        }

        [Fact]
        public void WriteBaseMaterialsTest()
        {
            var model = new ThreeMfModel();
            var baseMaterials = new ThreeMfBaseMaterials();
            baseMaterials.Bases.Add(new ThreeMfBase("blue", new ThreeMfsRGBColor(0, 0, 255)));
            baseMaterials.Bases.Add(new ThreeMfBase("green no alpha", new ThreeMfsRGBColor(0, 255, 0, 0)));
            model.Resources.Add(baseMaterials);
            VerifyModelXml(@"
<model>
  <resources>
    <basematerials id=""1"">
      <base name=""blue"" displaycolor=""#0000FFFF"" />
      <base name=""green no alpha"" displaycolor=""#00FF0000"" />
    </basematerials>
  </resources>
  <build />
</model>
", model);
        }

        [Fact]
        public void WriteTriangleVertexPropertiesTest()
        {
            var model = new ThreeMfModel();
            var obj = new ThreeMfObject();
            var baseMaterials = new ThreeMfBaseMaterials();
            baseMaterials.Bases.Add(new ThreeMfBase("blue", new ThreeMfsRGBColor(0, 0, 255)));
            var triangle = new ThreeMfTriangle(new ThreeMfVertex(0, 0, 0), new ThreeMfVertex(1, 1, 1), new ThreeMfVertex(2, 2, 2));
            triangle.PropertyResource = baseMaterials;
            triangle.V1PropertyIndex = 0;
            triangle.V2PropertyIndex = null;
            triangle.V3PropertyIndex = null;
            obj.Mesh.Triangles.Add(triangle);
            model.Resources.Add(obj);

            // `baseMaterials` was never added to the model resources; ensure it is when writing
            VerifyModelXml(@"
<model>
  <resources>
    <basematerials id=""1"">
      <base name=""blue"" displaycolor=""#0000FFFF"" />
    </basematerials>
    <object id=""2"" type=""model"">
      <mesh>
        <vertices>
          <vertex x=""0"" y=""0"" z=""0"" />
          <vertex x=""1"" y=""1"" z=""1"" />
          <vertex x=""2"" y=""2"" z=""2"" />
        </vertices>
        <triangles>
          <triangle v1=""0"" v2=""1"" v3=""2"" pid=""1"" p1=""0"" />
        </triangles>
      </mesh>
    </object>
  </resources>
  <build />
</model>
", model);
        }

        [Fact]
        public void WriteObjectPropertiesTest()
        {
            var model = new ThreeMfModel();
            var obj = new ThreeMfObject();
            var baseMaterials = new ThreeMfBaseMaterials();
            baseMaterials.Bases.Add(new ThreeMfBase("blue", new ThreeMfsRGBColor(0, 0, 255)));
            obj.PropertyResource = baseMaterials;
            obj.PropertyIndex = 0;
            model.Resources.Add(obj);

            // `baseMaterials` was never added to the model resources; ensure it is when writing
            VerifyModelXml(@"
<model>
  <resources>
    <basematerials id=""1"">
      <base name=""blue"" displaycolor=""#0000FFFF"" />
    </basematerials>
    <object id=""2"" type=""model"" pid=""1"" pindex=""0"">
      <mesh>
        <vertices />
        <triangles />
      </mesh>
    </object>
  </resources>
  <build />
</model>
", model);
        }

        [Fact]
        public void WriteColorGroupTest()
        {
            var model = new ThreeMfModel();
            var colorGroup = new ThreeMfColorGroup();
            colorGroup.Colors.Add(new ThreeMfColor(new ThreeMfsRGBColor(0, 0, 255)));
            model.Resources.Add(colorGroup);
            VerifyModelXml($@"
<model xmlns:m=""{ThreeMfModel.MaterialNamespace}"">
  <resources>
    <m:colorgroup id=""1"">
      <m:color color=""#0000FFFF"" />
    </m:colorgroup>
  </resources>
  <build />
</model>
", model);
        }

        [Fact]
        public void WriteTexture2DTest()
        {
            var model = new ThreeMfModel();
            var texture = new ThreeMfTexture2D(new byte[0], ThreeMfImageContentType.Jpeg);
            texture.BoundingBox = new ThreeMfBoundingBox(0.0, 1.0, 2.0, 3.0);
            texture.TileStyleU = ThreeMfTileStyle.Mirror;
            model.Resources.Add(texture);
            var text = GetStrippedModelXml(model).Replace("\r", "");

            // texture path is randomly generated so we have to check before and after it
            Assert.StartsWith($@"
<model xmlns:m=""{ThreeMfModel.MaterialNamespace}"">
  <resources>
    <m:texture2d id=""1"" path=""/3D/Textures/
".Trim(), text);
            Assert.EndsWith(@"
.jpg"" contenttype=""image/jpeg"" box=""0 1 2 3"" tilestyleu=""mirror"" />
  </resources>
  <build />
</model>
".Replace("\r", "").Trim(), text);
        }

        [Fact]
        public void WriteTexture2DGroupTest()
        {
            var model = new ThreeMfModel();
            var textureGroup = new ThreeMfTexture2DGroup(new ThreeMfTexture2D(new byte[0], ThreeMfImageContentType.Jpeg));
            textureGroup.Coordinates.Add(new ThreeMfTexture2DCoordinate(1.0, 2.0));
            model.Resources.Add(textureGroup);

            // texture was never added to the texture group; ensure it is when writing
            var text = GetStrippedModelXml(model).Replace("\r", "");
            Assert.EndsWith(@"
.jpg"" contenttype=""image/jpeg"" />
    <m:texture2dgroup id=""2"" texid=""1"">
      <m:tex2coord u=""1"" v=""2"" />
    </m:texture2dgroup>
  </resources>
  <build />
</model>
".Replace("\r", "").Trim(), text);
        }
    }
}
