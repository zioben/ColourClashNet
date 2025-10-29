using ColourClashLib.Color.Trasformation;
using ColourClashNet.Color.Tile;
using ColourClashNet.Log;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
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
            Multicolor,
            HiRes,
         //   HiResEnhancedPalette,
            MulticolorCaroline,
            DebugBasePalette,
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
            return this;
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
            RaiseProcessPartialEvent(new ColorTransformEventArgs()
            {
                ColorTransformInterface = this,
                CompletedPercent = 10,
                Message = "PreProcessing",
                ProcessingResults = ColorTransformResults.CreateValidResult(SourceData, oTmpData)
            });
            if (DitheringType !=  ColorDithering.None )
            {
                var oDither = Dithering.DitherBase.CreateDitherInterface(DitheringType,DitheringStrenght);             
                var oDitherResult = await oDither.DitherAsync(oSource, oTmpData, FixedPalette, ColorDistanceEvaluationMode, oToken);
                oTmpData = oDitherResult;
                RaiseProcessPartialEvent(new ColorTransformEventArgs()
                {
                    ColorTransformInterface = this,
                    CompletedPercent = 20,
                    Message = "PreProcessing-Dithered",
                    ProcessingResults = ColorTransformResults.CreateValidResult(SourceData, oTmpData)
                });
            }
            BypassDithering = true;
            return oTmpData;
        }

        async Task<int[,]?> ToHiresAsync(int[,]? oTmpDataSource, CancellationToken? oToken)
        {
            var oTmpData = await PreProcessAsync(oTmpDataSource, false, oToken);
            // Tile calculus: 8x8 with 2 color for tile
            TileManager oManager = new();
            var res1 = await oManager.CreateAsync(oTmpData, 8, 8, 2, FixedPalette, null, ColorDistanceEvaluationMode, TileBase.EnumColorReductionMode.Detailed,oToken);
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

        async Task<int[,]?> ToBasePalette(int[,]? oTmpDataSource, CancellationToken? oToken)
        {
            var oTmpData = await PreProcessAsync(oTmpDataSource, false, oToken);
            return oTmpData;
        }

        async Task<int[,]?> ToMultiColorAsync(int[,]? oTmpDataSource, CancellationToken? oToken)
        {
            // Tile calculus: 8x8 with 3 fixed folor for tile - plus 1 selectable per tile 
            var oTmpData = await PreProcessAsync(oTmpDataSource, true, oToken);
            DataContainer oTempContainer = new DataContainer();
            await oTempContainer.SetDataAsync(oTmpData,oToken);

            // Select the 3 most used 
            var oForcedColor = oTempContainer.ColorHistogram.SortColorsDescending().ToColorPalette(3);

            TileManager oManager = new TileManager();
            var res1 = await oManager.CreateAsync(oTmpData, 4, 8, 4, FixedPalette, oForcedColor, ColorDistanceEvaluationMode, TileBase.EnumColorReductionMode.Detailed, oToken);
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

        async Task<int[,]?> ToMultiColorCarolineAsync(int[,]? oTmpDataSource, CancellationToken? oToken)
        {
            // Tile calculus: 8x1 with 3 fixed folor for tile - plus 1 selectable per tile 
            var oTmpData = await PreProcessAsync(oTmpDataSource, true, oToken);
            DataContainer oTempContainer = new DataContainer();
            await oTempContainer.SetDataAsync(oTmpData, oToken);

            // Select the 3 most used 
            var oForcedColor = oTempContainer.ColorHistogram.SortColorsDescending().ToColorPalette(3);

            TileManager oManager = new TileManager();
            var res1 = await oManager.CreateAsync(oTmpData, 4, 1, 4, FixedPalette, oForcedColor, ColorDistanceEvaluationMode, TileBase.EnumColorReductionMode.Detailed, oToken);
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
            switch (VideoMode)
            {
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
                case C64VideoMode.MulticolorCaroline:
                    {
                        oPreprocessedData = await ToMultiColorCarolineAsync(SourceData, oToken);
                    }
                break;
                case C64VideoMode.Multicolor:
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