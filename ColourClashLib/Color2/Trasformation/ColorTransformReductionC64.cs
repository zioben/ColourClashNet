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
                    0x00_00_00_00,
                    0x00_FF_FF_FF,
                    0x00_89_40_36,
                    0x00_7A_BF_C7,
                    0x00_8A_46_AE,
                    0x00_68_A9_41,
                    0x00_3E_31_A2,
                    0x00_D0_DC_71,
                    0x00_90_5F_25,
                    0x00_5C_47_00,
                    0x00_BB_77_6D,
                    0x00_55_55_55,
                    0x00_80_80_80,
                    0x00_AC_EA_88,
                    0x00_AB_AB_AB,
                });
        }

        public override ColorTransformInterface SetProperty(ColorTransformProperties eProperty, object oValue)
        {
            base.SetProperty(eProperty, oValue);
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

        // Not Needed
        // protected async override Task<ColorTransformResults> CreateTrasformationMapAsync(CancellationToken? oToken)
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
                var oDitherResult = await oDither.DitherAsync(oSource, oTmpData, FixedPalette, ColorDistanceEvaluationMode, oToken);

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