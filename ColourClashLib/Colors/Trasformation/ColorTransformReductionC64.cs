using ColourClashLib.Color;
using ColourClashLib.Colors.Tile;
using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ColourClashNet.Colors.Transformation.ColorTransformReductionCPC;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformReductionC64 : ColorTransformReductionPalette
    {

        public enum C64VideoMode
        {
         //   Petscii,
            HiRes,
         //   HiResEnhancedPalette,
            Multicolor,
            MulticolorCaroline,
        }

        public C64VideoMode VideoMode { get; set; }= C64VideoMode.Multicolor;

        public override ColorTransformInterface SetProperty(ColorTransformProperties eProperty, object oValue)
        {
            if (base.SetProperty(eProperty, oValue) != null)
                return this;
            switch (eProperty)
            {
                case ColorTransformProperties.C64VideoMode:
                    if (Enum.TryParse<C64VideoMode>(oValue?.ToString(), out var evm))
                    {
                        VideoMode = evm;
                        return this;
                    }
                    break;
                default:
                    break;
            }
            return null;
        }

        public ColorTransformReductionC64()
        {
            Name = ColorTransformType.ColorReductionCBM64;
            Description = "Reduce color to C64 palette";
        }
        protected override void CreateTrasformationMap()
        {
            Palette = new ColorPalette(); 
            Palette.Add(0x00000000);
            Palette.Add(0x00FFFFFF);
            Palette.Add(0x00894036);
            Palette.Add(0x007ABFC7);
            Palette.Add(0x008A46AE);
            Palette.Add(0x0068A941);
            Palette.Add(0x003E31A2);
            Palette.Add(0x00D0DC71);
            Palette.Add(0x00905F25);
            Palette.Add(0x005C4700);
            Palette.Add(0x00BB776D);
            Palette.Add(0x00555555);
            Palette.Add(0x00808080);
            Palette.Add(0x00ACEA88);
            Palette.Add(0x00ABABAB);       
        }


        int[,]? PreProcess(int[,]? oDataSource, bool bHalveRes, CancellationToken oToken)
        {
            if (oDataSource == null)
                return null;
            var oTmp = oDataSource;
            if (bHalveRes)
            {
                oTmp = HalveHorizontalRes(oDataSource);
            }
            var oTmpData = base.ExecuteTransform(oTmp,oToken);
            if (Dithering != null)
            {
                oTmpData = Dithering.Dither(oTmp, oTmpData, Palette, ColorDistanceEvaluationMode, oToken);
            }
            BypassDithering = true;
            return oTmpData;
        }

        int[,]? TohiRes(int[,]? oTmpDataSource, CancellationToken oToken)
        {
            var oTmpData = PreProcess(oTmpDataSource, false, oToken);
            TileManager oManager = new TileManager();
            oManager.Init(oTmpData, 8, 8, 2, null, ColorDistanceEvaluationMode, TileBase.EnumColorReductionMode.Detailed);
            var oRet = oManager.TransformAndDither(oTmpData);
            BypassDithering = true;
            return oRet;
        }

        int[,]? To2X1(int[,]? oTmpDataSource, CancellationToken oToken)
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
                        var res = ColorIntExt.GetNearestColor(col, Palette, ColorDistanceEvaluationMode);
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


        int[,]? ToMultiColor(int[,]? oTmpDataSource, CancellationToken oToken)
{
            var oTmpData = PreProcess(oTmpDataSource, true, oToken);

            Histogram.Create(oTmpData);
            Histogram.SortColorsDescending();
            var oBGK = Histogram.rgbHistogram.First().Value;
            var oFixedColor = new ColorPalette();
            oFixedColor.Add(oBGK);
            TileManager oManager = new TileManager();

            oManager.Init(oTmpData, 4, 8, 4, oFixedColor, ColorDistanceEvaluationMode, TileBase.EnumColorReductionMode.Detailed);
            var oTmpHalfProc = oManager.TransformAndDither(oTmpData);
            var oRet = DoubleHorizontalRes(oTmpHalfProc);
            return oRet;
        }

        int[,]? ToMultiColorCaroline(int[,]? oTmpDataSource, CancellationToken oToken)
{
            var oTmpData = PreProcess(oTmpDataSource, true, oToken);

            Histogram.Create(oTmpData);
            Histogram.SortColorsDescending();
            var oBGK = Histogram.rgbHistogram.First().Value;
            var oFixedColor = new ColorPalette();
            oFixedColor.Add(oBGK);
            TileManager oManager = new TileManager();
            oManager.Init(oTmpData, 4, 1, 4, oFixedColor, ColorDistanceEvaluationMode, TileBase.EnumColorReductionMode.Detailed);
            var oTmpHalfProc = oManager.TransformAndDither(oTmpData);
            var oRet = DoubleHorizontalRes(oTmpHalfProc);
            return oRet;
        }




        protected override int[,]? ExecuteTransform(int[,]? oDataSource, CancellationToken oToken)
        {
            if (oDataSource == null)
                return null;

            switch (VideoMode)
            {
                case C64VideoMode.HiRes:
                    {                       
                        return TohiRes(oDataSource, oToken);
                    }
                case C64VideoMode.MulticolorCaroline:
                    {
                        return ToMultiColorCaroline(oDataSource, oToken);
                    }
                case C64VideoMode.Multicolor:
                    {
                        return ToMultiColor(oDataSource, oToken);
                    }
                default: return null;
            }

        }
    }
}