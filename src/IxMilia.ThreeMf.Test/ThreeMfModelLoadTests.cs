// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Xml.Linq;
using Xunit;

namespace IxMilia.ThreeMf.Test
{
    public class ThreeMfModelLoadTests
    {
        private ThreeMfModel ParseXml(string contents)
        {
            var document = XDocument.Parse(contents);
            return ThreeMfModel.LoadXml(document.Root);
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
    }
}
