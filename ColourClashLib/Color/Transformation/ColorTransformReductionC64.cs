using ColourClashNet.Color.Conversion;
using ColourClashNet.Color.Tile;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformReductionC64 : ColorTransformBase
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

        public C64VideoMode VideoMode 
        { 
            get => config.C64VideoMode;
            set => config.C64VideoMode = value; 
        }
        public C64DitheringMode VideoDithering 
        { 
            get => config.C64DitheringMode;
            private set => config.C64DitheringMode = value; 
        }

        public bool TileBorderShow 
        { 
            get => config.ShowTileBorders;
            set => config.ShowTileBorders = value;
        }
        public int TileBorderColor
        {
            get => config.TileBorderColor;
            set => config.TileBorderColor = value;
        }

        Palette basePalette = new Palette().Create(
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

        Palette enhancedPalette = new();
        
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

        void CreateEnhancedPalette()
        {
            var sM = nameof(CreateEnhancedPalette);
            var baseList = basePalette.ToList();
            var enhancedList = basePalette.ToList();
            for (int i = 0; i < basePalette.Count - 1; i++)
            {
                for (int j = i + 1; j < basePalette.Count; j++)
                {
                    int iRGBA = baseList[i];
                    int iRGBB = baseList[j];
                    var HSVA = HSV.CreateFromIntRGB(iRGBA);
                    var HSVB = HSV.CreateFromIntRGB(iRGBB);
                    // LogMan.Message(sC, sM, $"{i} : {j} -> {HSVA.V:f1} - {HSVB.V:f1}");
                    if (Math.Abs(HSVA.V - HSVB.V) < 15.0)
                    {
                        int iRGBM = ColorIntExt.GetColorMean(iRGBA, iRGBB);
                        enhancedList.Add(iRGBM);
                    }
                }
            }
            enhancedPalette = new Palette().Create(enhancedList);
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


        #endregion

        TileManager CreateTileManager(int tileWidth, int tileHeight, int maxColors, ImageData image, Palette referencePalette, CancellationToken token = default)
        {
            var cfg = config.Clone().WithReferencePalette(referencePalette).WithDithering(DitheringConfig).WithMaxColorWanted(maxColors);
            tileManager = new TileManager()
                .WithTileBorder(TileBorderShow, TileBorderColor)
                .Create(tileWidth, tileHeight, image, 1.0, cfg, token);
            return tileManager;
        }

        // Only to debug purpose, this is the best image obtainable using C64 palette
        ImageData? ToDebugImage(ImageData image, Palette palette, CancellationToken token = default)
        {
            var tempResult = new ColorTransformReductionPalette()
                .WithReferencePalette(palette)
                .WithDithering(DitheringConfig)
                .WithColorDistanceEvaluationMode(ColorDistanceEvaluationMode)
                .CreateAndProcessColors(image, token);
            return tempResult.DataOut;
        }


        // Tile 8x8 - 2 selectable color per tile
        ImageData ToBitmapHires(Palette palette, CancellationToken token = default)
        {
            var tempImage = ToDebugImage(ImageSource,palette,token);
            ImageData.AssertValid(tempImage);
            TileManager oManager = CreateTileManager(8, 8, 2, tempImage, new Palette(), token);
            var tileResul = oManager.ProcessColors(token);
            if (tileResul.IsSuccess)
            {
                var tileImage = oManager.CreateImageFromTiles();
                return tileImage;
            }
            else
            {
                return null;
            }
        }

        // Tile 4x8 - 1 fixed color (bkg) + 3 selectable color per tile
        ImageData ToBitmapMultiColor(Palette palette, CancellationToken token = default)
        {
            var halveImage = ImageTools.HalveXResolution(ImageSource, HalveResolutionMode.MeanColor);
            var tempImage = ToDebugImage(halveImage, palette, token);

            ImageData.AssertValid(tempImage);
            var backgroundColor = new HistogramRGB().Create(tempImage).SortColorsDescending().ToPalette(1);
            TileManager oManager = CreateTileManager(4, 8, 4, tempImage, backgroundColor, token);
            var tileResul = oManager.ProcessColors(token);
            if (tileResul.IsSuccess)
            {
                var tileImage = oManager.CreateImageFromTiles();
                return ImageTools.DoubleXResolution(tileImage);
            }
            else
            {
                return null;
            }
        }

        // Tile 4x1 - 1 fixed color (bkg) + 3 selectable color per tile
        ImageData ToBitmapMulticolorFli( Palette palette, CancellationToken token = default)
        {
            var halveImage = ImageTools.HalveXResolution(ImageSource, HalveResolutionMode.MeanColor);
            var tempImage = ToDebugImage(halveImage, palette, token);
            ImageData.AssertValid(tempImage);
            var backgroundColor = new HistogramRGB().Create(tempImage).SortColorsDescending().ToPalette(1);
            TileManager oManager = CreateTileManager(4, 1, 4, tempImage, backgroundColor, token);
            var tileResul = oManager.ProcessColors(token);
            if (tileResul.IsSuccess)
            {
                var tileImage = oManager.CreateImageFromTiles();
                return ImageTools.DoubleXResolution(tileImage);
            }
            else
            {
                return null;
            }
        }

        // Tile 8x1 - 2 selectable color per tile
        ImageData ToBitmapHiresFli(Palette palette, CancellationToken token = default)
        {
            var tempImage = ToDebugImage(ImageSource, palette, token);
            ImageData.AssertValid(tempImage);
            TileManager oManager = CreateTileManager(8, 1, 2, tempImage, new Palette(), token);
            var tileResul = oManager.ProcessColors(token);
            if (tileResul.IsSuccess)
            {
                var tileImage = oManager.CreateImageFromTiles();
                return tileImage;
            }
            else
            {
                return null;
            }
        }



        // Create a Tile Map 8x4 3 fixed color + 1 selectable color per tile
        ImageData ToCharsetMulticolor(CancellationToken token = default)
        {   
            return null;
        }

      
        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {
            ImageData? oPreprocessedData = null;
            BypassDithering = true;
            switch (VideoMode)
            {

                case C64VideoMode.DebugEnhancedPalette:
                    {
                        oPreprocessedData = ToDebugImage(ImageSource, enhancedPalette, token);
                    }
                    break;
                case C64VideoMode.DebugBasePalette:
                    {
                        oPreprocessedData = ToDebugImage(ImageSource, basePalette, token);
                    }
                    break;
                case C64VideoMode.Charset:
                case C64VideoMode.HiResEnhanced:
                    {
                        oPreprocessedData = ToBitmapHires(enhancedPalette, token);
                    }
                    break;
                case C64VideoMode.HiRes:
                    {                       
                        oPreprocessedData = ToBitmapHires(basePalette, token);
                    }
                break;
                case C64VideoMode.FlexibleLineInterpretation:
                    {
                        oPreprocessedData = ToBitmapMulticolorFli(basePalette, token);
                    }
                break;
                case C64VideoMode.MulticolorEnhanced:
                    {
                        oPreprocessedData = ToBitmapMultiColor(enhancedPalette, token);
                    }
                    break;
                case C64VideoMode.Multicolor:
                    {
                        oPreprocessedData = ToBitmapMultiColor(basePalette, token);
                    }
                break;
                case C64VideoMode.HiResFlexibleLineInterpretation:
                    {
                        oPreprocessedData = ToBitmapHiresFli(basePalette, token);
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