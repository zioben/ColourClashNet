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
            BitmapModeMulticolor,
            BitmapModeHiRes,
            CharsetMulticolor,
            CharsetHiRes,
            BitmapModeFLI,
            BitmapModeMCI,
            DebugBasePalette,
            DebugEnhancedPalette,
        }

        public C64VideoMode VideoMode { get; set; }= C64VideoMode.BitmapModeMulticolor;

        public bool TileBorderShow { get; set; } = false;
        int TileBorderColor = 0x_00_00_FF_00;
        
       
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

        ColorTransformType processingType { get; } = ColorTransformType.ColorReductionClustering;
        //ColorTransformType processingType { get; } = ColorTransformType.ColorReductionFast;

        TileManager tileManager = new TileManager();

        public ColorTransformReductionC64()
        {
            Type = ColorTransformType.ColorReductionCBM64;
            Description = "Reduce color to C64 palette";
            CreatePalette();
        }


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
                   // LogMan.Message(sC, sM, $"{i} : {j} -> {HSVA.V:f1} - {HSVB.V:f1}");
                    if (Math.Abs(HSVA.V-HSVB.V)<15.0)
                    {
                        int iRGBM = ColorIntExt.GetColorMean(iRGBA, iRGBB);
                        enhancedPalette.Add(iRGBM);
                    }
                }
            }
        }

     
        Dictionary<ColorTransformProperties, object> CreateProcessingParams(int maxColors, Palette fixedColorPalette )
        {
            var dict = new Dictionary<ColorTransformProperties, object>();
            dict[ColorTransformProperties.ColorDistanceEvaluationMode] = ColorDistanceEvaluationMode;
            // Passing pre-rendered image, keep the 
            dict[ColorTransformProperties.PriorityPalette] = fixedColorPalette;
            //dict[ColorTransformProperties.Forced_Palette] = fixedPalette;
            dict[ColorTransformProperties.DitheringType] = DitheringType ;
            dict[ColorTransformProperties.DitheringStrength] = DitheringStrength;
            dict[ColorTransformProperties.MaxColorsWanted] = maxColors;
            dict[ColorTransformProperties.UseColorMean] = false;
            dict[ColorTransformProperties.ClusterTrainingLoop] = 6;
            return dict;
        }

       

        internal protected override ColorTransformInterface SetProperty(ColorTransformProperties propertyName, object value)
        {
            base.SetProperty(propertyName, value);
            switch (propertyName)
            {
                case ColorTransformProperties.C64VideoMode:
                        VideoMode = ToEnum< C64VideoMode>(value);
                    break;
                default:
                    break;
            }
            return this;
        }

     


        protected override ColorTransformResult CreateTransformationMap(CancellationToken oToken=default)
        {
            switch (VideoMode)
            {
                case C64VideoMode.BitmapModeMCI:
                case C64VideoMode.DebugEnhancedPalette:
                    SetProperty(ColorTransformProperties.PriorityPalette, enhancedPalette);
                    break;
                default:
                    SetProperty(ColorTransformProperties.PriorityPalette, basePalette);
                    break;
            }

            return base.CreateTransformationMap(oToken);
        }

        TileManager CreateTileManager( int tileHeight, int tileWidth, int maxColors, ImageData image, Palette fixedColorPalette, CancellationToken token=default)
        {
            tileManager = new TileManager().Create(tileHeight, tileWidth, image, 1.0, processingType, CreateProcessingParams(maxColors, fixedColorPalette), token);
            tileManager.TileBorderShow = TileBorderShow;
            tileManager.TileBorderColor = TileBorderColor;
            return tileManager;
        }

        // Not Needed
        // protected async override Task<ColorTransformResults> CreateTrasformationMapAsync(CancellationToken? oToken)
        ImageData? PreProcess(bool bHalveRes, CancellationToken token=default)
        {
            string sM= nameof(PreProcess);
            var refImage = bHalveRes ? ImageTools.HalveXResolution(ImageSource) : ImageSource;
            // Reduce all to the base 16 C64 colors without restrictions
            var procImage = TransformationMap.Transform(refImage, token);
            var dithImage = procImage;
            if (DitheringType !=  ColorDithering.None )
            {
                var dithering = Dithering.DitherBase.CreateDitherInterface(DitheringType,DitheringStrength);             
                var dithRes = dithering.Dither(refImage, procImage, ColorDistanceEvaluationMode, token);
                dithImage = dithRes.DataOut;
            }
            // Raise pre processing event
            RaiseProcessPartialEvent(new ColorProcessingEventArgs()
            {
                ColorTransformInterface = this,
                CompletedPercent = 0,
                ProcessingResults = ColorTransformResult.CreateValidResult(ImageSource, dithImage, "Dithered Base")
            });
            return dithImage;
        }

        // Only to debug purpose, this is the best image obtainable using C64 palette
        ImageData? ToBasePalette(CancellationToken token=default) 
            => PreProcess(false, token);

        // Cerate a Tile Map 8x8 2 indipendent colors
        ImageData ToHires(CancellationToken token=default)
        {
            var oTmpData = PreProcess(false, token);
            var oManager = CreateTileManager(8, 8, 2, oTmpData, new Palette(), token);
            var tileResul = oManager.ProcessColors(token);
            if (tileResul)
            {
                return oManager.CreateImageFromTiles();
            }
            else
            {
                return null;
            }
        }


        // Create a Tile Map 8x4 1 fixed color + 3 selectable colors per tile
        ImageData ToBitmapMultiColor(CancellationToken token=default)
        {
            var preprocessImage = PreProcess(true, token);
            var paletteFixedColor = new HistogramRGB().Create(preprocessImage).SortColorsDescending().ToPalette(1);
            TileManager oManager = CreateTileManager(4, 8, 4, preprocessImage, paletteFixedColor, token);
            var tileResul = oManager.ProcessColors(token);
            if (tileResul)
            {
                var tileImage = oManager.CreateImageFromTiles();
                return ImageTools.DoubleXResolution(tileImage);
            }
            else
            {
                return null;
            }
        }


        // Create a Tile Map 8x4 3 fixed color + 1 selectable color per tile
        ImageData ToCharsetMulticolor(CancellationToken token = default)
        {
            var preprocessImage = PreProcess(true, token);
            var paletteFixedColor = new HistogramRGB().Create(preprocessImage).SortColorsDescending().ToPalette(3);
            TileManager oManager = CreateTileManager(4, 8, 4, preprocessImage, paletteFixedColor, token);
            var tileResul = oManager.ProcessColors(token);
            if (tileResul)
            {
                var tileImage = oManager.CreateImageFromTiles();
                return ImageTools.DoubleXResolution(tileImage);
            }
            else
            {
                return null;
            }
        }

        // Create a Tile Map 1x4 2 selectable color per tile
        ImageData? ToBitmapFli(CancellationToken token = default)
        {
            var preprocessImage = PreProcess(true, token);

            // Select the most used color
            var paletteFixedColor = new HistogramRGB().Create(preprocessImage).SortColorsDescending().ToPalette(1);
            TileManager oManager = CreateTileManager(4, 1, 2, preprocessImage, new Palette(), token);
            var tileResul = oManager.ProcessColors(token);
            if (tileResul)
            {
                var tileImage = oManager.CreateImageFromTiles();
                return ImageTools.DoubleXResolution(tileImage);
            }
            else
            {
                return null;
            }
        }

        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {
            ImageData? oPreprocessedData = null;
            BypassDithering = true;
            switch (VideoMode)
            {

                case C64VideoMode.DebugEnhancedPalette:
                case C64VideoMode.DebugBasePalette:
                    {
                        oPreprocessedData = ToBasePalette(token);
                    }
                    break;
                case C64VideoMode.CharsetHiRes:
                case C64VideoMode.BitmapModeHiRes:
                    {                       
                        oPreprocessedData = ToHires(token);
                    }
                break;
                case C64VideoMode.BitmapModeFLI:
                    {
                        oPreprocessedData = ToBitmapFli(token);
                    }
                break;
                case C64VideoMode.BitmapModeMulticolor:
                    {
                        oPreprocessedData = ToBitmapMultiColor(token);
                    }
                break;
                case C64VideoMode.BitmapModeMCI:
                    {
                        oPreprocessedData = ToBitmapMultiColor(token);
                    }
                    break;
                case C64VideoMode.CharsetMulticolor:
                    {
                        oPreprocessedData = ToCharsetMulticolor(token);
                    }
                    break;

                default:
                break;
            }
            if (oPreprocessedData != null)
            {
                return ColorTransformResult.CreateValidResult(ImageSource, oPreprocessedData);
            }
            else
            {
                return ColorTransformResult.CreateErrorResult("Error during C64 transformation", null);
            }
        }
    }
}