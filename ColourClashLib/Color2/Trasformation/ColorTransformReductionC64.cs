using ColourClashNet.Color.Tile;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformReductionC64 : ColorTransformReductionPalette
    {
        static readonly string sC = nameof(ColorTransformReductionC64);
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
                case ColorTransformProperties.C64_VideoMode:
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
            Type = ColorTransformType.ColorReductionCBM64;
            Description = "Reduce color to C64 palette";
            CreatePalette();
        }

        void CreatePalette()
        {
            SetProperty(
                ColorTransformProperties.Fixed_Palette,
                new List<int>
                {
                    0x00000000,
                    0x00FFFFFF,
                    0x00894036,
                    0x007ABFC7,
                    0x008A46AE,
                    0x0068A941,
                    0x003E31A2,
                    0x00D0DC71,
                    0x00905F25,
                    0x005C4700,
                    0x00BB776D,
                    0x00555555,
                    0x00808080,
                    0x00ACEA88,
                    0x00ABABAB,
                });
        }


        async Task<int[,]?> PreProcessAsync(int[,]? oDataSource, bool bHalveRes, CancellationToken? oToken)
        {
            if (oDataSource == null)
                return null;
            var oSource = oDataSource;
            if (bHalveRes)
            {
                oSource = ColorTransformBase.HalveHorizontalRes(oDataSource);
            }
            var oTmpData = await TransformationMap.TransformAsync(oSource, oToken);
            if (DitheringType !=  ColorDithering.None )
            {
                var oDither = Dithering.DitherBase.CreateDitherInterface(DitheringType);             
                oTmpData = await oDither.DitherAsync(oSource, oTmpData, FixedPalette, ColorDistanceEvaluationMode, oToken);
            }
            BypassDithering = true;
            return oTmpData;
        }

        async Task<int[,]?> TohiResAsync(int[,]? oTmpDataSource, CancellationToken? oToken)
        {
            var oTmpData = await PreProcessAsync(oTmpDataSource, false, oToken);
            TileManager oManager = new TileManager();
            oManager.Init(oTmpData, 8, 8, 2, null, ColorDistanceEvaluationMode, TileBase.EnumColorReductionMode.Detailed);
            //            oManager.Init(oTmpData, 8, 8, 2, Palette, ColorDistanceEvaluationMode, TileBase.EnumColorReductionMode.Detailed);
            var oRet = await oManager.TransformAndDitherAsync(oTmpData,oToken);
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
                        var res = ColorIntExt.GetNearestColor(col, FixedPalette, ColorDistanceEvaluationMode);
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


        async Task<int[,]?> ToMultiColorAsync(int[,]? oTmpDataSource, CancellationToken? oToken)
        {
            var oTmpData = await PreProcessAsync(oTmpDataSource, true, oToken);
            var OutputHistogram = new Histogram();
            await OutputHistogram.CreateAsync(oTmpData,oToken);
            OutputHistogram.SortColorsDescending();
            var oBGK = OutputHistogram.rgbHistogram.First().Value;
            var oFixedColor = new Palette();
            oFixedColor.Add(oBGK);
            TileManager oManager = new TileManager();

            oManager.Init(oTmpData, 4, 8, 4, oFixedColor, ColorDistanceEvaluationMode, TileBase.EnumColorReductionMode.Detailed);
            var oTmpHalfProc = await oManager.TransformAndDitherAsync(oTmpData,oToken);
            var oRet = ColorTransformBase.DoubleHorizontalRes(oTmpHalfProc);
            return oRet;
        }

        async Task<int[,]?> ToMultiColorCarolineAsync(int[,]? oTmpDataSource, CancellationToken? oToken)
        {
            var oTmpData = await PreProcessAsync(oTmpDataSource, true, oToken);
            var OutputHistogram = new Histogram();
            await OutputHistogram.CreateAsync(oTmpData, oToken);
            OutputHistogram.SortColorsDescending();
            var oBGK = OutputHistogram.rgbHistogram.First().Value;
            var oFixedColor = new Palette();
            oFixedColor.Add(oBGK);
            TileManager oManager = new TileManager();
            oManager.Init(oTmpData, 4, 1, 4, oFixedColor, ColorDistanceEvaluationMode, TileBase.EnumColorReductionMode.Detailed);
            var oTmpHalfProc = await oManager.TransformAndDitherAsync(oTmpData,oToken);
            var oRet = ColorTransformBase.DoubleHorizontalRes(oTmpHalfProc);
            return oRet;
        }




        protected async override Task<ColorTransformResults> ExecuteTransformAsync(CancellationToken? oToken)
        {
            int[,] oRet = null;
            switch (VideoMode)
            {
                case C64VideoMode.HiRes:
                    {                       
                        oRet = await TohiResAsync(SourceData, oToken);
                    }
                break;
                case C64VideoMode.MulticolorCaroline:
                    {
                        oRet = await ToMultiColorCarolineAsync(SourceData, oToken);
                    }
                break;
                case C64VideoMode.Multicolor:
                    {
                          oRet = await ToMultiColorAsync(SourceData, oToken);
                    }
                break;
                default:
                break;
            }
            if (oRet != null)
            {
                return ColorTransformResults.CreateValidResult(SourceData, oRet);
            }
            else
            {
                return new();
            }
        }
    }
}