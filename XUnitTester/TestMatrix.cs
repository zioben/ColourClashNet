using ColourClashNet;
using ColourClashNet.Color;
using ColourClashNet.Color.Conversion;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace XUnitTester
{
    public class TestMatrix
    {

        [Theory]
        [InlineData(0, 0)]
        [InlineData(-5, 0)]      // clamp valori negativi a 0
        [InlineData(300, 255)]   // clamp valori oltre 255


        public void Clamp_TestCase(int input, int expected)
        {
            Assert.Equal(expected, ColourTools.Clamp(input, 0, 255));
        }

        //[Fact]
        //public void MatrixBlit_should_not_throw_exception()
        //{
        //    int ws = 32;
        //    int hs = 20;
        //    int wd = 64;
        //    int hd = 50;
        //    int off = 5;

        //    int[,] src = new int[hs, ws];
        //    int[,] dst = new int[hd, wd];
        //    for (int yd = -off; yd < hd+off; yd++)
        //    {
        //        for (int xd = -off; xd < wd+off; xd++)
        //        {
        //            for (int ys = -off; ys < hs+off; ys++)
        //            {
        //                for (int xs = -off; xs < ws+off; xs++)
        //                {
        //                    for (int w = -off; w < ws+10; w++)
        //                    {
        //                        for (int h = -off; h < hs+off; h++)
        //                        {
        //                            var ex = Record.Exception(() =>
        //                                MatrixTools.Blit(src, dst,xs,ys,xd,yd,w,h));
        //                            Assert.Null(ex);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
