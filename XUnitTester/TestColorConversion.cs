using ColourClashNet.Color;
using ColourClashNet.Color.Conversion;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace XUnitTester
{
    public class TestColorConversion
    {

        public static TheoryData<int> EdgeColors => new()
        {
            ColorIntExt.FromRGB(0, 0, 0),
            ColorIntExt.FromRGB(64, 64, 64),
            ColorIntExt.FromRGB(128, 128, 128),
            ColorIntExt.FromRGB(192, 192, 192),
            ColorIntExt.FromRGB(255, 255, 255),
            ColorIntExt.FromRGB(0,0,255),
            ColorIntExt.FromRGB(0,255,255),
            ColorIntExt.FromRGB(0,255,0),
            ColorIntExt.FromRGB(255,255,0),
            ColorIntExt.FromRGB(255,0,0),
            ColorIntExt.FromRGB(255,0,255),
        };
        public static TheoryData<ColorMode> GetColorModes()
        {
            var data = new TheoryData<ColorMode>();
            foreach (var colorMode in Enum.GetValues<ColorMode>())
            {
                data.Add(colorMode);
            }
            return data;
        }

        //[Theory]
        //[MemberData(nameof(EdgeColors))]
        //public void TestHSVConversionBase(int rgb)
        //{
        //    var ex = Record.Exception(() =>
        //    {
        //        var hsv = new HSV(rgb);
        //        var rgbConverted = hsv.ToIntRGB();
        //        Assert.Equal(rgbConverted, rgb);
        //    });
        //    Assert.Null(ex);
        //}

        [Theory]
        [MemberData(nameof(GetColorModes))]
        public void TestColorConversions(ColorMode colorMode)
        {
            const int tolerance = 0;
            var examples = new ConcurrentQueue<string>();
            int failCount = 0;

            Parallel.For(0, 0x00FFFFFF, rgb =>
            {
                var converter = ColorConversionTools.CreateConverter( colorMode,rgb);
                var rgbc = converter.ToIntRGB();
                switch (colorMode)
                {
                    case ColorMode.GRAY:
                        break;
                    case ColorMode.LAB:
                    case ColorMode.HSV:
                    case ColorMode.RGB:
                        if (ColorIntExt.Distance(rgb, rgbc, ColorDistanceEvaluationMode.RGB) > tolerance)
                        {
                            if (Interlocked.Increment(ref failCount) <= 10)
                            {
                                examples.Enqueue($"RGB={rgb.ToR()},{rgb.ToG()},{rgb.ToB()} -> HSV: {rgbc.ToR()},{rgbc.ToG()},{rgbc.ToB()}");
                            }
                        }
                        break;
                    default:
                        Assert.Fail($"Unknown color mode: {colorMode}");
                        break;
                }
            });
            Assert.True(failCount == 0, $"{colorMode} conversion error detected: first {failCount} colors over tolerance {tolerance}:{Environment.NewLine}" + string.Join(Environment.NewLine, examples));
        }
    }
}
