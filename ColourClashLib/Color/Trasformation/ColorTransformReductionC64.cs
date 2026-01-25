using ColourClashNet.Color.Tile;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Common;
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
                    var HSVA = HSV.CreateFromIntRGB(iRGBA);
                    var HSVB = HSV.CreateFromIntRGB(iRGBB);                   
                    LogMan.Message(sC, sM, $"{i} : {j} -> {HSVA.V:f1} - {HSVB.V:f1}");
                    if (Math.Abs(HSVA.V-HSVB.V)<15.0)
                    {
                        int iRGBM = ColorIntExt.GetColorMean(iRGBA, iRGBB);
                        enhancedPalette.Add(iRGBM);
                    }
                }
            }
        }

        int TileBorderColor = 0x_00_00_FF_00;

        ColorTransformType processingType { get; } = ColorTransformType.ColorReductionGenericPalette;
        Dictionary<ColorTransformProperties, object> CreateProcessingParams(Palette palette, ColorDithering ditheringType, int maxColors)
        {
            var dict = new Dictionary<ColorTransformProperties, object>();
            dict[ColorTransformProperties.ColorDistanceEvaluationMode] = ColorDistanceEvaluationMode;
            dict[ColorTransformProperties.Fixed_Palette] = palette;
            dict[ColorTransformProperties.Forced_Palette] = palette;
            dict[ColorTransformProperties.Dithering_Type] = ditheringType;
            dict[ColorTransformProperties.Dithering_Strength] = DitheringStrength;
            dict[ColorTransformProperties.MaxColorsWanted] = maxColors;
            return dict;
        }

        internal protected override ColorTransformInterface SetProperty(ColorTransformProperties propertyName, object value)
        {
            base.SetProperty(propertyName, value);
            switch (propertyName)
            {
                case ColorTransformProperties.C64_VideoMode:
                        VideoMode = ToEnum< C64VideoMode>(value);
                    break;
                default:
                    break;
            }
            return this;
        }

     


        protected override ColorTransformResults CreateTransformationMap(CancellationToken oToken=default)
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

            return base.CreateTransformationMap(oToken);
        }

        // Not Needed
        // protected async override Task<ColorTransformResults> CreateTrasformationMapAsync(CancellationToken? oToken)
        ImageData? PreProcess(ImageData oDataSource, bool bHalveRes, CancellationToken oToken)
        {
            string sM= nameof(PreProcess);
            if ( !oDataSource?.IsValid ?? true )
            {
                LogMan.Error(sC, sM, "No data source provided");
                return null;
            }
            var oRealSource = oDataSource;
            if (bHalveRes)
            {
                oRealSource = new ImageData().Create(ColorTransformBase.HalveHorizontalRes(oDataSource.matrix));
            }
            // Reduce all to the base 16 C64 colors without restrictions
            var oProcessed = TransformationMap.Transform(oRealSource, oToken);
            var oDithered = oProcessed;
            if (DitheringType !=  ColorDithering.None )
            {
                var oDither = Dithering.DitherBase.CreateDitherInterface(DitheringType,DitheringStrength);             
                oDithered = oDither.Dither(oRealSource, oProcessed, ColorDistanceEvaluationMode, oToken);
            }
            // Raise pre processing event
            RaiseProcessPartialEvent(new ColorProcessingEventArgs()
            {
                ColorTransformInterface = this,
                CompletedPercent = 0,
                ProcessingResults = ColorTransformResults.CreateValidResult(SourceData, oDithered, "Dithered Base")
            });
            return oDithered;
        }

        // Only to debug purpose, this is the best image obtainable using C64 palette
        ImageData? ToBasePalette(ImageData oTmpDataSource, CancellationToken oToken) 
            => PreProcess(oTmpDataSource, false, oToken);

        // Cerate a Tile Map 8x8 2 indipendent colors
        ImageData ToHires(ImageData oDataSource, CancellationToken oToken)
        {
            var oTmpData = PreProcess(oDataSource, false, oToken);
            TileManager oManager = TileManager.CreateTileManager(8, 8, oDataSource, processingType, CreateProcessingParams(FixedPalette, DitheringType, 2), oToken);
            oManager.TileBorderShow = TileBorderShow;
            oManager.TileBorderColor = TileBorderColor;
            var tileResul = oManager.ProcessColors(oToken);
            if (tileResul)
            {
                var oImageData = oManager.CreateImageFromTiles();
                return oImageData;
            }
            else
            {
                return null;
            }
        }


        // Create a Tile Map 8x4 1 fixed color + 3 selectable colors per tile
        ImageData ToMultiColor(ImageData oDataSource, CancellationToken oToken)
        {
            var oTmpData = PreProcess(oDataSource, true, oToken);

            // Select the most used 
            var oTmpHistogram = Histogram.CreateHistogram(oTmpData);
            var oForcedPalette = oTmpHistogram.SortColorsDescending().ToPalette(1);

            TileManager oManager = TileManager.CreateTileManager(4, 8, oDataSource, processingType, CreateProcessingParams(FixedPalette, DitheringType, 4), oToken);
            oManager.TileBorderShow = TileBorderShow;
            oManager.TileBorderColor = TileBorderColor;
            var tileResul = oManager.ProcessColors(oToken);
            if (tileResul)
            {
                var oImageData = oManager.CreateImageFromTiles();
                var oTmpHalfProc = oManager.CreateImageFromTiles();
                var oResultData = ColorTransformBase.DoubleHorizontalRes(oTmpHalfProc.matrix);
                return new ImageData().Create(oResultData);
            }
            else
            {
                return null;
            }
        }

        // Create a Tile Map 1x4 2 selectable color per tile
        ImageData? ToFli(ImageData oDataSource, CancellationToken oToken=default)
        {
            var oTmpData = PreProcess(oDataSource, true, oToken);

            TileManager oManager = TileManager.CreateTileManager(4, 1, oDataSource, processingType, CreateProcessingParams(FixedPalette, DitheringType, 2), oToken);
            oManager.TileBorderShow = TileBorderShow;
            oManager.TileBorderColor = TileBorderColor;
            var tileResul = oManager.ProcessColors(oToken);
            if (tileResul)
            {
                var oImageData = oManager.CreateImageFromTiles();
                var oTmpHalfProc = oManager.CreateImageFromTiles();
                var oResultData = ColorTransformBase.DoubleHorizontalRes(oTmpHalfProc.matrix);
                return new ImageData().Create(oResultData);
            }
            else
            {
                return null;
            }
        }




        protected override ColorTransformResults ExecuteTransform(CancellationToken token = default)
        {
            ImageData? oPreprocessedData = null;
            BypassDithering = true;
            switch (VideoMode)
            {

                case C64VideoMode.DebugEnhancedPalette:
                case C64VideoMode.DebugBasePalette:
                    {
                        oPreprocessedData = ToBasePalette(SourceData, token);
                    }
                    break;
                case C64VideoMode.HiRes:
                    {                       
                        oPreprocessedData = ToHires(SourceData, token);
                    }
                break;
                case C64VideoMode.FLI:
                    {
                        oPreprocessedData = ToFli(SourceData, token);
                    }
                break;
                case C64VideoMode.Multicolor:
                    {
                        oPreprocessedData = ToMultiColor(SourceData, token);
                    }
                break;
                case C64VideoMode.MCI:
                    {
                        oPreprocessedData = ToMultiColor(SourceData, token);
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
                return ColorTransformResults.CreateErrorResult("Error during C64 transformation", null);
            }
        }
    }
}