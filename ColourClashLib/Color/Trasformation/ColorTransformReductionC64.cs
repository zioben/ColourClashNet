using ColourClashLib.Color.Trasformation;
using ColourClashNet.Color.Tile;
using ColourClashNet.Log;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
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
            Multicolor,
            HiRes,
            FLI,
            MCI,
            DebugBasePalette,
            DebugEnhancedPalette,
        }

        public C64VideoMode VideoMode { get; set; }= C64VideoMode.Multicolor;

        public bool TileBorderShow { get; set; } = false;

        public ColorTransformReductionC64()
        {
            Type = ColorTransformType.ColorReductionCBM64;
            Description = "Reduce color to C64 palette";
            CreatePalette();
        }

        List<int> basePalette = new List<int>
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
                };

        List<int> enhancedPalette = new List<int>();

        void CreatePalette()
        {
            var sM = nameof(CreatePalette);
            enhancedPalette = new List<int>();
            enhancedPalette.AddRange(basePalette);
            for (int i = 0; i < basePalette.Count-1; i++)
            {
                for (int j = i+1; j < basePalette.Count; j++)
                {
                    int iRGBA = basePalette[i];
                    int iRGBB = basePalette[j];   
                    var HSVA = HSV.FromIntRGB(iRGBA);
                    var HSVB = HSV.FromIntRGB(iRGBB);                   
                    LogMan.Message(sC, sM, $"{i} : {j} -> {HSVA.V:f1} - {HSVB.V:f1}");
                    if (Math.Abs(HSVA.V-HSVB.V)<15.0)
                    {
                        int iRGBM = ColorIntExt.GetColorMean(iRGBA, iRGBB);
                        enhancedPalette.Add(iRGBM);
                    }
                }
            }
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
            return this;
        }

        protected override Task<ColorTransformResults> CreateTrasformationMapAsync(CancellationToken? oToken)
        {
            switch (VideoMode)
            {
                case C64VideoMode.MCI:
                case C64VideoMode.DebugEnhancedPalette:
                    SetProperty(ColorTransformProperties.Fixed_Palette, enhancedPalette);
                    break;
                default:
                    SetProperty(ColorTransformProperties.Fixed_Palette, basePalette);
                    break;
            }

            return base.CreateTrasformationMapAsync(oToken);
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
            // Reduce all to the base 16 C64 colors without restrictions
            var oTmpData = await TransformationMap.TransformAsync(oSource, oToken);
            if (DitheringType !=  ColorDithering.None )
            {
                var oDither = Dithering.DitherBase.CreateDitherInterface(DitheringType,DitheringStrenght);             
                var oDitherResult = await oDither.DitherAsync(oSource, oTmpData, FixedPalette, ColorDistanceEvaluationMode, oToken);
                oTmpData = oDitherResult;
            }
            // Raise pre processing event
            RaiseProcessPartialEvent(new ColorTransformEventArgs()
            {
                ColorTransformInterface = this,
                CompletedPercent = 0,
                Message = "PreProcessing-Dithered",
                ProcessingResults = ColorTransformResults.CreateValidResult(SourceData, oTmpData)
            });
            return oTmpData;
        }

        // Only to debug purpose, this is the best image obtainable using C64 palette
        async Task<int[,]?> ToBasePalette(int[,]? oTmpDataSource, CancellationToken? oToken) => await PreProcessAsync(oTmpDataSource, false, oToken);

        // Cerate a Tile Map 8x8 2 indipendent colors
        async Task<int[,]?> ToHiresAsync(int[,]? oTmpDataSource, CancellationToken? oToken)
        {
            var oTmpData = await PreProcessAsync(oTmpDataSource, false, oToken);
            TileManager oManager = TileManager.Create(8, 8, 2)
                .SetProperty(ColorTransformProperties.ColorDistanceEvaluationMode, ColorDistanceEvaluationMode)
                .SetProperty(ColorTransformProperties.Fixed_Palette, FixedPalette)
                .SetProperty(ColorTransformProperties.Force_Palette, null);
//                .SetProperty(ColorTransformProperties.Dithering_Type, DitheringType)
//                .SetProperty(ColorTransformProperties.Dithering_Strength, DitheringStrenght);

            oManager.TileBorderShow = TileBorderShow;
            var res1 = await oManager.CreateAsync(oTmpData, oToken);
            var res2 = await oManager.ProcessAsync(oToken);
            if (res2)
            {
                var oRet = await oManager.CreateImageFromTilesAsync(oToken);
                return oRet;
            }
            else
            {
                return null;
            }
        }


        // Create a Tile Map 8x4 3 fixed colors + 1 selectable color per tile
        async Task<int[,]?> ToMultiColorAsync(int[,]? oTmpDataSource, CancellationToken? oToken)
        {
            var oTmpData = await PreProcessAsync(oTmpDataSource, true, oToken);
            DataContainer oTempContainer = new DataContainer();
            await oTempContainer.SetDataAsync(oTmpData,oToken);

            // Select the 3 most used 
            var oForcedPalette = oTempContainer.ColorHistogram.SortColorsDescending().ToColorPalette(1);

            TileManager oManager = TileManager.Create(4, 8, 4)
                .SetProperty(ColorTransformProperties.ColorDistanceEvaluationMode, ColorDistanceEvaluationMode)
                .SetProperty(ColorTransformProperties.Fixed_Palette, FixedPalette)
                .SetProperty(ColorTransformProperties.Force_Palette, oForcedPalette);
//                .SetProperty(ColorTransformProperties.Dithering_Type, DitheringType)
//                .SetProperty(ColorTransformProperties.Dithering_Strength, DitheringStrenght);

            oManager.TileBorderShow = TileBorderShow;
            // var res1 = await oManager.CreateAsync(oTmpData, oToken);
            var res1 = await oManager.CreateAsync(oTmpData, oToken);
            var res2 = await oManager.ProcessAsync(oToken);
            if (res2)
            {
                var oTmpHalfProc = await oManager.CreateImageFromTilesAsync(oToken);
                var oRet = ColorTransformBase.DoubleHorizontalRes(oTmpHalfProc);
                return oRet;
            }
            else
            {
                return null;
            }
        }

        // Create a Tile Map 1x4 2 selectable color per tile
        async Task<int[,]?> ToFliAsync(int[,]? oTmpDataSource, CancellationToken? oToken)
        {
            var oTmpData = await PreProcessAsync(oTmpDataSource, true, oToken);
            DataContainer oTempContainer = new DataContainer();
            await oTempContainer.SetDataAsync(oTmpData, oToken);

            TileManager oManager = TileManager.Create(4, 1, 2)
                .SetProperty(ColorTransformProperties.ColorDistanceEvaluationMode, ColorDistanceEvaluationMode)
                .SetProperty(ColorTransformProperties.Fixed_Palette, FixedPalette)
                .SetProperty(ColorTransformProperties.Force_Palette, null);
         //       .SetProperty(ColorTransformProperties.Dithering_Type, DitheringType)
         //       .SetProperty(ColorTransformProperties.Dithering_Strength, DitheringStrenght);

            oManager.TileBorderShow = TileBorderShow;
            var res1 = await oManager.CreateAsync(oTmpData, oToken);
            var res2 = await oManager.ProcessAsync(oToken);
            if (res2)
            {
                var oTmpHalfProc = await oManager.CreateImageFromTilesAsync(oToken);
                var oRet = ColorTransformBase.DoubleHorizontalRes(oTmpHalfProc);
                return oRet;
            }
            else
            {
                return null;
            }
        }




        protected async override Task<ColorTransformResults> ExecuteTransformAsync(CancellationToken? oToken)
        {
            int[,] oPreprocessedData = null;
            BypassDithering = true;
            switch (VideoMode)
            {

                case C64VideoMode.DebugEnhancedPalette:
                case C64VideoMode.DebugBasePalette:
                    {
                        oPreprocessedData = await ToBasePalette(SourceData, oToken);
                    }
                    break;
                case C64VideoMode.HiRes:
                    {                       
                        oPreprocessedData = await ToHiresAsync(SourceData, oToken);
                    }
                break;
                case C64VideoMode.FLI:
                    {
                        oPreprocessedData = await ToFliAsync(SourceData, oToken);
                    }
                break;
                case C64VideoMode.Multicolor:
                    {
                        oPreprocessedData = await ToMultiColorAsync(SourceData, oToken);
                    }
                break;
                case C64VideoMode.MCI:
                    {
                        oPreprocessedData = await ToMultiColorAsync(SourceData, oToken);
                    }
                    break;
                default:
                break;
            }
            if (oPreprocessedData != null)
            {
                return ColorTransformResults.CreateValidResult(SourceData, oPreprocessedData);
            }
            else
            {
                return new();
            }
        }
    }
}