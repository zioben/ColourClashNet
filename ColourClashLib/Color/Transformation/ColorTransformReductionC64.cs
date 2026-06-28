using ColourClashNet.Color.Conversion;
using ColourClashNet.Color.Tile;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformReductionC64 : ColorTransformReductionPalette
    {
        static readonly string sC = nameof(ColorTransformReductionC64);

        #region enums
        public enum C64VideoMode
        {
            //   Petscii,
            /// <summary>
            /// 4x8 tile with 4 indipendent colors
            /// </summary>
            Multicolor,
            /// <summary>
            /// 4x8 tile with 4 indipendent colors - enachanced palette
            /// </summary>
            MulticolorEnhanced,
            /// <summary>
            /// 8x8 tile with 2 indipendent colors
            /// </summary>
            HiRes,
            /// <summary>
            /// 8x8 tile with 2 indipendent colors - enachanced palette
            /// </summary>
            HiResEnhanced,
            /// <summary>
            /// 8x8 refedined-charset with 1 bkg color, 2 fixed colors, and 1 indipendent color
            /// </summary>
            CharsetMulticolor,
            /// <summary>
            /// 8x8 refedined-charset with 1 bkg color and 1 indipendent color
            /// </summary>
            Charset,
            /// <summary>
            /// 4x8 tile with 4 indipendent colors per line
            /// </summary>
            FlexibleLineInterpretation,
            /// <summary>
            /// 8x8 tile with 2 indipendent colors per line
            /// </summary>
            HiResFlexibleLineInterpretation,
            /// <summary>
            /// Debug with c64 palette colors
            /// </summary>
            DebugBasePalette,
            /// <summary>
            /// Debug with c64 enhanced palette colors
            /// </summary>
            DebugEnhancedPalette,
        }

        public enum C64DitheringMode
        {
            PostDitherTile,
            PreDitherImage,
        }

        #endregion

        #region properties

        public C64VideoMode VideoMode { get; set; }= C64VideoMode.Multicolor;
        public C64DitheringMode VideoDithering { get; private set; }
        public bool EnableColorSwitching { get; set; } = false;

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
        ColorTransformType ColorTransformationModel { get; } = ColorTransformType.ColorReductionClustering;

        C64DitheringMode DitheringProcessing { get; set; } = C64DitheringMode.PreDitherImage;

        TileManager tileManager = new TileManager();

        #endregion

        public ColorTransformReductionC64()
        {
            Type = ColorTransformType.ColorReductionCBM64;
            Description = "Reduce color to C64 palette";
            CreateEnhancedPalette();
        }

        #region fluent with - set

        public ColorTransformReductionC64 WithC64ScreenMode(C64VideoMode mode, C64DitheringMode ditheringMode,bool showTileBorders)
        {
            TileBorderShow = showTileBorders;
            VideoMode = mode;
            VideoDithering = ditheringMode;
            return this;
        }

        public ColorTransformReductionC64 WithC64ScreenMode(ColorTransformConfig cfg) => WithC64ScreenMode(cfg.C64VideoMode, cfg.C64DitheringMode, cfg.ShowTileBorders);

        public override ColorTransformInterface SetProperties(ColorTransformConfig cfg)
        {
            base.SetProperties(cfg);
            return WithC64ScreenMode(cfg);
        }

        #endregion

        void CreateEnhancedPalette()
        {
            var sM = nameof(CreateEnhancedPalette);
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

        ColorTransformConfig CreateConfig(int maxColors, Palette referencePalette)
        {
            return new ColorTransformConfig()
                .WithReferencePalette(referencePalette)
                .WithDithering(DitheringType, DitheringStrength, DitheringFx)
                .WithClustering(maxColors, 6, false);
        }

        TileManager CreateTileManager(int tileHeight, int tileWidth, int maxColors, ImageData image, Palette referencePalette, CancellationToken token = default)
        {
            tileManager = new TileManager().Create(tileHeight, tileWidth, image, 1.0, ColorTransformationModel, CreateConfig(maxColors, referencePalette), token);
            tileManager.TileBorderShow = TileBorderShow;
            tileManager.TileBorderColor = TileBorderColor;
            return tileManager;
        }

        Palette GetPalette()
        {
            if (!EnableColorSwitching)
            {
                
            }
            var palB = new Palette().Create(basePalette);
            var palE = new Palette().Create(enhancedPalette);
            switch (VideoMode)
            {
                case C64VideoMode.FlexibleLineInterpretation: return palB;
                case C64VideoMode.HiRes: return palB;
                case C64VideoMode.HiResEnhanced: return palE;
                case C64VideoMode.HiResFlexibleLineInterpretation: return palB;
                case C64VideoMode.Multicolor: return palB;
                case C64VideoMode.MulticolorEnhanced: return palE;
                case C64VideoMode.Charset: return palB;
                case C64VideoMode.CharsetMulticolor: return palB;
                case C64VideoMode.DebugBasePalette: return palB;
                case C64VideoMode.DebugEnhancedPalette: return palE;

                default:
                    return palB;
            }
        }

ImageData? PreProcess(bool bHalveRes, CancellationToken token=default)
        {
            string sM= nameof(PreProcess);
            var refImage = bHalveRes ? ImageTools.HalveXResolution(ImageSource,true) : ImageSource;
            // Reduce all to the base 16 C64 colors without restrictions
            var colorTrans = new ColorTransformReductionPalette()
                .WithReferencePalette(new Palette().Create(GetPalette()))
                .WithDithering(DitheringType, DitheringStrength, DitheringFx)
                .WithColorDistanceEvaluationMode(ColorDistanceEvaluationMode);
            var res = colorTrans.CreateAndProcessColors(refImage, token);
            // Raise pre processing event
            RaiseProcessPartialEvent(new ColorProcessingEventArgs()
            {
                ColorTransformInterface = this,
                CompletedPercent = 0,
                ProcessingResults = ColorTransformResult.CreateValidResult(ImageSource, res.DataOut, "Dithered Base")
            });
            return res.DataOut;
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
                case C64VideoMode.Charset:
                case C64VideoMode.HiResEnhanced:
                case C64VideoMode.HiRes:
                    {                       
                        oPreprocessedData = ToHires(token);
                    }
                break;
                case C64VideoMode.FlexibleLineInterpretation:
                    {
                        oPreprocessedData = ToBitmapFli(token);
                    }
                break;
                case C64VideoMode.MulticolorEnhanced:
                case C64VideoMode.Multicolor:
                    {
                        oPreprocessedData = ToBitmapMultiColor(token);
                    }
                break;
                case C64VideoMode.HiResFlexibleLineInterpretation:
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