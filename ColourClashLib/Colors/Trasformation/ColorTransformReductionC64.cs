using ColourClashLib.Color;
using ColourClashLib.Colors.Tile;
using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformReductionC64 : ColorTransformReductionPalette
    {

        static int iArea = 8;
        static int iTile = 8;
        static int iOffS = iArea - iTile;

        public enum C64ScreenMode
        {
            HiRes,
            HiResEnhancedPalette,
            Multicolor,
            MulticolorCaroline,
        }

        public C64ScreenMode ScreenMode { get; set; }= C64ScreenMode.HiResEnhancedPalette;

        public ColorTransformReductionC64()
        {
            Type = ColorTransform.ColorReductionCBM64;
            Description = "Reduce color to C64 palette";
        }
        protected override void CreateTrasformationMap()
        {
            ColorPalette = new ColorPalette(); 
            ColorPalette.Add(0x00000000);
            ColorPalette.Add(0x00FFFFFF);
            ColorPalette.Add(0x00894036);
            ColorPalette.Add(0x007ABFC7);
            ColorPalette.Add(0x008A46AE);
            ColorPalette.Add(0x0068A941);
            ColorPalette.Add(0x003E31A2);
            ColorPalette.Add(0x00D0DC71);
            ColorPalette.Add(0x00905F25);
            ColorPalette.Add(0x005C4700);
            ColorPalette.Add(0x00BB776D);
            ColorPalette.Add(0x00555555);
            ColorPalette.Add(0x00808080);
            ColorPalette.Add(0x00ACEA88);
            ColorPalette.Add(0x00ABABAB);

            //if (ScreenMode == C64ScreenMode.HiResEnhancedPalette)
            //{
            //    var lRGB = ColorPalette.ToList();
            //    for (int i = 0; i < lRGB.Count-1; i++)
            //    {
            //        for (int j = i+1; j < lRGB.Count; j++)
            //        {
            //            var irgb = lRGB[i];
            //            var jrgb = lRGB[j];
            //            var iS = irgb.ToS();
            //            var jS = jrgb.ToS();
            //            if (iS == jS)
            //            {
            //                int ir = (irgb.ToR() + jrgb.ToR()) / 2;
            //                int ig = (irgb.ToG() + jrgb.ToG()) / 2;
            //                int ib = (irgb.ToB() + jrgb.ToB()) / 2;
            //                ColorPalette.Add(ColorIntExt.FromRGB(ir, ig, ib));
            //            }
            //        }
            //    }
            //}
        }

        int[,]? TohiRes(int[,]? oDataSource)
        {
            //ColorHistogram.Create(ColorPalette);
            //ColorPalette = ColorHistogram.ToColorPalette();
            //var oTmpData = base.ExecuteTransform(oDataSource);
            //if (Dithering != null)
            //{
            //    oTmpData = Dithering.Dither(oDataSource, oTmpData, ColorPalette, ColorDistanceEvaluationMode);
            //}
            //BypassDithering = true;

            ColorTransformReductionPalette oPreprocess = new ColorTransformReductionPalette()
            {
                ColorPalette = ColorPalette,
            };

            int R = oDataSource.GetLength(0);
            int C = oDataSource.GetLength(1);
            int[,] oRet = new int[R, C];

            List<ColorTile> lDataBlock = new List<ColorTile>();

            //Parallel.For(0, R / 8, r =>
            for (int r = 0; r < R / iTile; r++)
            {
                for (int c = 0; c < C / iTile; c++)
                //  Parallel.For(0, C / 8, c =>
                {
                    ColorTile oTile = new ColorTile()
                    {
                        r = r,
                        c = c,
                        TileData = new int[iArea, iArea],
                    };
                    for (int rr = 0; rr < iArea; rr++)
                    {
                        var rPos = Math.Min(R - 1, Math.Max(0, rr - iOffS + r * iTile));
                        for (int cc = 0; cc < iArea; cc++)
                        {
                            var cPos = Math.Min(C - 1, Math.Max(0, cc - iOffS + c * iTile));
                            var rgb = oDataSource[rPos, cPos];
                            oTile.TileData[rr, cc] = rgb;
                        }
                    }
                    lDataBlock.Add(oTile);
                }
            }


            Parallel.For(0, lDataBlock.Count, i =>
            {
                var oTile = lDataBlock[i];
                var TileDataProc = oTile.Process(null);
                for (int rr = 0; rr < iTile; rr++)
                {
                    for (int cc = 0; cc < iTile; cc++)
                    {
                        oRet[oTile.r * iTile + rr, oTile.c * iTile + cc] = TileDataProc[rr + iOffS, cc + iOffS];
                    }
                }
            });

            return oRet;
        }

        protected override int[,]? ExecuteTransform(int[,]? oDataSource)
        {
            if (oDataSource == null)
                return null;

            var oTmpData = base.ExecuteTransform(oDataSource);
            if (Dithering != null)
            {
                oTmpData = Dithering.Dither(oDataSource, oTmpData, ColorPalette, ColorDistanceEvaluationMode);
            }
            BypassDithering = true;

            switch (ScreenMode)
            {
                case C64ScreenMode.HiRes:
                    return TohiRes(oTmpData);
                case C64ScreenMode.HiResEnhancedPalette:
                    return TohiRes(oTmpData);
                default: return oTmpData;
            }
        }
    }
}