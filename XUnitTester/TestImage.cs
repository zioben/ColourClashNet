using ColourClashNet;
using ColourClashNet.Color;
using ColourClashNet.Color.Conversion;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Xunit;


namespace XUnitTester
{
    public class TestImage
    {
        [Theory]
        [MemberData(nameof(AllTransformTypes))]
        public void CreateColorTransformInterface_ShouldNotThrow(ColorTransformType type)
        {
            var ex = Record.Exception(() =>
                ColorTransformBase.CreateColorTransformInterface(type, new ColorTransformConfig()));
            Assert.Null(ex);
        }

        public static IEnumerable<object[]> AllTransformTypes() =>
            Enum.GetValues<ColorTransformType>().Select(t => new object[] { t });


       
    }
}
