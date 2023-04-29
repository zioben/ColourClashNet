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

        public enum C64ScreenMode
        {
         //   Petscii,
            HiRes,
         //   HiResEnhancedPalette,
            Multicolor,
            MulticolorCaroline,
        }

        public C64ScreenMode ScreenMode { get; set; }= C64ScreenMode.Multicolor;

        public ColorTransformReductionC64()
        {
            type = ColorTransform.ColorReductionCBM64;
            description = "Reduce color to C64 palette";
        }
        protected override void CreateTrasformationMap()
        {
            colorPalette = new ColorPalette(); 
            colorPalette.Add(0x00000000);
            colorPalette.Add(0x00FFFFFF);
            colorPalette.Add(0x00894036);
            colorPalette.Add(0x007ABFC7);
            colorPalette.Add(0x008A46AE);
            colorPalette.Add(0x0068A941);
            colorPalette.Add(0x003E31A2);
            colorPalette.Add(0x00D0DC71);
            colorPalette.Add(0x00905F25);
            colorPalette.Add(0x005C4700);
            colorPalette.Add(0x00BB776D);
            colorPalette.Add(0x00555555);
            colorPalette.Add(0x00808080);
            colorPalette.Add(0x00ACEA88);
            colorPalette.Add(0x00ABABAB);       
        }


        int[,]? PreProcess(int[,]? oDataSource, bool bHalveRes)
        {
            if (oDataSource == null)
                return null;
            var oTmp = oDataSource;
            if (bHalveRes)
            {
                oTmp = HalveHorizontalRes(oDataSource);
            }
            var oTmpData = base.ExecuteTransform(oTmp);
            if (dithering != null)
            {
                oTmpData = dithering.Dither(oTmp, oTmpData, colorPalette, ColorDistanceEvaluationMode);
            }
            BypassDithering = true;
            return oTmpData;
        }

        int[,]? TohiRes(int[,]? oTmpDataSource)
        {
            var oTmpData = PreProcess(oTmpDataSource, false);
            TileManager oManager = new TileManager();
            oManager.Init(oTmpData, 8, 8, 2, null, ColorDistanceEvaluationMode, TileBase.EnumColorReductionMode.Detailed);
            var oRet = oManager.TransformAndDither(oTmpData);
            BypassDithering = true;
            return oRet;
        }

        int[,]? To2X1(int[,]? oTmpDataSource)
        {
            if (oTmpDataSource == null)
                return null;
            var R = oTmpDataSource.GetLength(0);
            var C = oTmpDataSource.GetLength(1);
            var oRet = new int[R, C];   
            Parallel.For( 0, R, r=>  
            {
                for (int c = 0; c < C; c+=2)
                {
                    if (c == C - 1)
                    {
                        oRet[r, c] = oTmpDataSource[r, c];
                    }
                    else
                    {
                        var a = oTmpDataSource[r, c];
                        var b = oTmpDataSource[r, c+1];
                        var cr = (a.ToR() + b.ToR()) / 2;
                        var cg = (a.ToG() + b.ToG()) / 2;
                        var cb = (a.ToB() + b.ToB()) / 2;
                        var col = ColorIntExt.FromRGB(cr, cg, cb);
                        var res = ColorIntExt.GetNearestColor(col, colorPalette, ColorDistanceEvaluationMode);
                        if (ColorIntExt.Distance(res, a, ColorDistanceEvaluationMode) < ColorIntExt.Distance(res, b, ColorDistanceEvaluationMode))
                        {
                            oRet[r, c] = a;
                            oRet[r, c + 1] = a;
                        }
                        else
                        {
                            oRet[r, c] = b;
                            oRet[r, c + 1] = b;
                        }
                    }
                }
            });
            return oRet;
        }


        int[,]? ToMultiColor(int[,]? oTmpDataSource)
        {
            var oTmpData = PreProcess(oTmpDataSource, true);

            colorHistogram.Create(oTmpData);
            colorHistogram.SortColorsDescending();
            var oBGK = colorHistogram.rgbHistogram.First().Value;
            var oFixedColor = new ColorPalette();
            oFixedColor.Add(oBGK);
            TileManager oManager = new TileManager();

            oManager.Init(oTmpData, 4, 8, 4, oFixedColor, ColorDistanceEvaluationMode, TileBase.EnumColorReductionMode.Detailed);
            var oTmpHalfProc = oManager.TransformAndDither(oTmpData);
            var oRet = DoubleHorizontalRes(oTmpHalfProc);
            return oRet;
        }

        int[,]? ToMultiColorCaroline(int[,]? oTmpDataSource)
        {
            var oTmpData = PreProcess(oTmpDataSource, true);

            colorHistogram.Create(oTmpData);
            colorHistogram.SortColorsDescending();
            var oBGK = colorHistogram.rgbHistogram.First().Value;
            var oFixedColor = new ColorPalette();
            oFixedColor.Add(oBGK);
            TileManager oManager = new TileManager();
            oManager.Init(oTmpData, 4, 1, 4, oFixedColor, ColorDistanceEvaluationMode, TileBase.EnumColorReductionMode.Detailed);
            var oTmpHalfProc = oManager.TransformAndDither(oTmpData);
            var oRet = DoubleHorizontalRes(oTmpHalfProc);
            return oRet;
        }




        protected override int[,]? ExecuteTransform(int[,]? oDataSource)
        {
            if (oDataSource == null)
                return null;

            switch (ScreenMode)
            {
                case C64ScreenMode.HiRes:
                    {                       
                        return TohiRes(oDataSource);
                    }
                case C64ScreenMode.MulticolorCaroline:
                    {
                        return ToMultiColorCaroline(oDataSource);
                    }
                case C64ScreenMode.Multicolor:
                    {
                        return ToMultiColor(oDataSource);
                    }
                default: return null;
            }

        }
    }
}