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
    /// <summary>
    /// Commodore 64 video modes.
    /// <para/>
    /// The image is mapped to the C64 palette (base or enhanced), then split into tiles that are color-reduced
    /// independently, emulating the per-cell color limits of the VIC-II. Multicolor modes work at half horizontal resolution.
    /// </summary>
    public class ColorTransformCbm64 : ColorTransformBase
    {
        static readonly string sC = nameof(ColorTransformCbm64);

        #region enums
        /// <summary>
        /// Commodore 64 video modes.
        /// </summary>
        public enum C64VideoMode
        {
            /// <summary>
            /// Multicolor bitmap: 4x8 wide-pixel tiles, background color + 3 colors per tile
            /// </summary>
            Multicolor,
            /// <summary>
            /// Multicolor bitmap on the enhanced (mixed colors) palette
            /// </summary>
            MulticolorEnhanced,
            /// <summary>
            /// Hires bitmap: 8x8 tiles with 2 independent colors
            /// </summary>
            HiRes,
            /// <summary>
            /// Hires bitmap on the enhanced (mixed colors) palette
            /// </summary>
            HiResEnhanced,
            /// <summary>
            /// 8x8 redefined charset with 1 background color, 2 fixed colors and 1 independent color (not implemented)
            /// </summary>
            CharsetMulticolor,
            /// <summary>
            /// 8x8 redefined charset with 1 background color and 1 independent color
            /// </summary>
            Charset,
            /// <summary>
            /// Multicolor FLI: 4x1 wide-pixel tiles, background color + 3 colors per tile line
            /// </summary>
            FlexibleLineInterpretation,
            /// <summary>
            /// Hires FLI: 8x1 tiles with 2 independent colors per tile line
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

        /// <summary>
        /// When dithering is applied in the C64 pipeline.
        /// </summary>
        public enum C64DitheringMode
        {
            /// <summary>Dither each tile after its color reduction.</summary>
            PostDitherTile,
            /// <summary>Dither the whole image on the C64 palette before the tile reduction.</summary>
            PreDitherImage,
        }

        #endregion

        #region properties

        /// <summary>
        /// Target video mode.
        /// </summary>
        public C64VideoMode VideoMode 
        { 
            get => config.C64VideoMode;
            set => config.C64VideoMode = value; 
        }
        /// <summary>
        /// When dithering is applied.
        /// </summary>
        public C64DitheringMode VideoDithering 
        { 
            get => config.C64DitheringMode;
            private set => config.C64DitheringMode = value; 
        }

        /// <summary>
        /// Marks the tile grid on the output image.
        /// </summary>
        public bool TileBorderShow 
        { 
            get => config.ShowTileBorders;
            set => config.ShowTileBorders = value;
        }
        /// <summary>
        /// Color used to mark the tile grid.
        /// </summary>
        public int TileBorderColor
        {
            get => config.TileBorderColor;
            set => config.TileBorderColor = value;
        }

        // C64 hardware palette (VIC-II, 16 colors)
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
                    0x00_7C_70_DA,
                    0x00_AB_AB_AB,
                });

        // Base palette plus the mean of color pairs with similar brightness (simulated by dithering/interlace)
        Palette enhancedPalette = new();
        
        /// <summary>
        /// Reduction used for each tile.
        /// </summary>
        ColorTransformType ColorTransformationModel { get; } = ColorTransformType.ColorReductionClustering;

        /// <summary>
        /// Dithering stage used internally.
        /// </summary>
        C64DitheringMode DitheringProcessing { get; set; } = C64DitheringMode.PreDitherImage;

        TileManager tileManager = new TileManager();

        #endregion

        /// <summary>
        /// Creates the C64 transformation and builds the enhanced palette.
        /// </summary>
        public ColorTransformCbm64()
        {
            Type = ColorTransformType.ColorReductionCBM64;
            Description = "Reduce color to C64 palette";
            CreateEnhancedPalette();
        }

        /// <summary>
        /// Builds the enhanced palette: base colors plus the mean of every pair with brightness difference below 15%.
        /// </summary>
        void CreateEnhancedPalette()
        {
            var sM = nameof(CreateEnhancedPalette);
            var baseList = basePalette.ToList();
            var enhancedList = basePalette.ToList();
            for (int i = 0; i < basePalette.Count - 1; i++)
            {
                for (int j = i + 1; j < basePalette.Count; j++)
                {
                    int rgbA = baseList[i];
                    int rgbB = baseList[j];
                    var hsvA = HSV.CreateFromIntRGB(rgbA);
                    var hsvB = HSV.CreateFromIntRGB(rgbB);
                    // LogMan.Message(sC, sM, $"{i} : {j} -> {hsvA.V:f1} - {hsvB.V:f1}");
                    // Only colors with similar brightness blend well when mixed on screen
                    if (Math.Abs(hsvA.V - hsvB.V) < 15.0)
                    {
                        int rgbM = ColorIntExt.GetColorMean(rgbA, rgbB);
                        enhancedList.Add(rgbM);
                    }
                }
            }
            enhancedPalette = new Palette().Create(enhancedList);
        }

        #region fluent with - set

        /// <summary>
        /// Sets the C64 settings (fluent).
        /// </summary>
        public ColorTransformCbm64 WithC64ScreenMode(C64VideoMode mode, C64DitheringMode ditheringMode,bool showTileBorders)
        {
            TileBorderShow = showTileBorders;
            VideoMode = mode;
            VideoDithering = ditheringMode;
            return this;
        }

        /// <summary>
        /// Sets the C64 settings from a configuration (fluent).
        /// </summary>
        public ColorTransformCbm64 WithC64ScreenMode(ColorTransformConfig cfg) => WithC64ScreenMode(cfg.C64VideoMode, cfg.C64DitheringMode, cfg.ShowTileBorders);


        #endregion

        /// <summary>
        /// Creates the tile manager for a video mode.
        /// </summary>
        /// <param name="tileWidth">Tile width</param>
        /// <param name="tileHeight">Tile height</param>
        /// <param name="maxColors">Colors per tile</param>
        /// <param name="image">Image already mapped to the C64 palette</param>
        /// <param name="referencePalette">Colors shared by all tiles (e.g. background color)</param>
        /// <param name="token">Cancellation token</param>
        TileManager CreateTileManager(int tileWidth, int tileHeight, int maxColors, ImageData image, Palette referencePalette, CancellationToken token = default)
        {
            var cfg = config.Clone().WithReferencePalette(referencePalette).WithDithering(DitheringConfig).WithMaxColorWanted(maxColors);
            // Tile colors must be picked from the (C64) image colors, not evaluated as cluster means
            cfg.ClusterColorSelectionMode = ColorSelectionMode.UseColorPalette;
            tileManager = new TileManager()
                .WithTileBorder(TileBorderShow, TileBorderColor)
                .Create(tileWidth, tileHeight, image, 1.0, cfg, token);
            return tileManager;
        }

        /// <summary>
        /// Maps the image to <paramref name="palette"/> without tile constraints: the best result obtainable with that palette.
        /// </summary>
        ImageData? ToDebugImage(ImageData image, Palette palette, CancellationToken token = default)
        {
            var tempResult = new ColorTransformFixedPalette()
                .WithReferencePalette(palette)
                .WithDithering(DitheringConfig)
                .WithColorDistanceEvaluationMode(ColorDistanceEvaluationMode)
                .CreateAndProcessColors(image, token);
            return tempResult.DataOut;
        }


        /// <summary>
        /// Hires bitmap: 8x8 tiles, 2 colors per tile.
        /// </summary>
        ImageData ToBitmapHires(Palette palette, CancellationToken token = default)
        {
            var tempImage = ToDebugImage(ImageSource,palette,token);
            ImageData.AssertValid(tempImage);
            TileManager manager = CreateTileManager(8, 8, 2, tempImage, new Palette(), token);
            var tileResult = manager.ProcessColors(token);
            if (tileResult.IsSuccess)
            {
                var tileImage = manager.CreateImageFromTiles();
                return tileImage;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Multicolor bitmap: 4x8 wide-pixel tiles, background color + 3 colors per tile.
        /// </summary>
        ImageData ToBitmapMultiColor(Palette palette, CancellationToken token = default)
        {
            var halveImage = ImageTools.HalveXResolution(ImageSource, HalveResolutionMode.MeanColor);
            var tempImage = ToDebugImage(halveImage, palette, token);

            ImageData.AssertValid(tempImage);
            // The most frequent color becomes the global background color, shared by every tile
            var backgroundColor = new HistogramRGB().Create(tempImage).SortColorsDescending().ToPalette(1);
            TileManager manager = CreateTileManager(4, 8, 4, tempImage, backgroundColor, token);
            var tileResult = manager.ProcessColors(token);
            if (tileResult.IsSuccess)
            {
                var tileImage = manager.CreateImageFromTiles();
                return ImageTools.DoubleXResolution(tileImage);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Multicolor FLI: 4x1 wide-pixel tiles, background color + 3 colors per tile.
        /// </summary>
        ImageData ToBitmapMulticolorFli( Palette palette, CancellationToken token = default)
        {
            var halveImage = ImageTools.HalveXResolution(ImageSource, HalveResolutionMode.MeanColor);
            var tempImage = ToDebugImage(halveImage, palette, token);
            ImageData.AssertValid(tempImage);
            var backgroundColor = new HistogramRGB().Create(tempImage).SortColorsDescending().ToPalette(1);
            TileManager manager = CreateTileManager(4, 1, 4, tempImage, backgroundColor, token);
            var tileResult = manager.ProcessColors(token);
            if (tileResult.IsSuccess)
            {
                var tileImage = manager.CreateImageFromTiles();
                return ImageTools.DoubleXResolution(tileImage);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Hires FLI: 8x1 tiles, 2 colors per tile.
        /// </summary>
        ImageData ToBitmapHiresFli(Palette palette, CancellationToken token = default)
        {
            var tempImage = ToDebugImage(ImageSource, palette, token);
            ImageData.AssertValid(tempImage);
            TileManager manager = CreateTileManager(8, 1, 2, tempImage, new Palette(), token);
            var tileResult = manager.ProcessColors(token);
            if (tileResult.IsSuccess)
            {
                var tileImage = manager.CreateImageFromTiles();
                return tileImage;
            }
            else
            {
                return null;
            }
        }



        /// <summary>
        /// Multicolor charset mode (not implemented yet).
        /// </summary>
        /// <returns>Always null.</returns>
        ImageData ToCharsetMulticolor(CancellationToken token = default)
        {   
            return null;
        }

      
        /// <summary>
        /// Runs the conversion for the selected video mode. Dithering is handled internally.
        /// </summary>
        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {
            ImageData? preprocessedData = null;
            BypassDithering = true;
            switch (VideoMode)
            {

                case C64VideoMode.DebugEnhancedPalette:
                    {
                        preprocessedData = ToDebugImage(ImageSource, enhancedPalette, token);
                    }
                    break;
                case C64VideoMode.DebugBasePalette:
                    {
                        preprocessedData = ToDebugImage(ImageSource, basePalette, token);
                    }
                    break;
                case C64VideoMode.Charset:
                case C64VideoMode.HiResEnhanced:
                    {
                        preprocessedData = ToBitmapHires(enhancedPalette, token);
                    }
                    break;
                case C64VideoMode.HiRes:
                    {                       
                        preprocessedData = ToBitmapHires(basePalette, token);
                    }
                break;
                case C64VideoMode.FlexibleLineInterpretation:
                    {
                        preprocessedData = ToBitmapMulticolorFli(basePalette, token);
                    }
                break;
                case C64VideoMode.MulticolorEnhanced:
                    {
                        preprocessedData = ToBitmapMultiColor(enhancedPalette, token);
                    }
                    break;
                case C64VideoMode.Multicolor:
                    {
                        preprocessedData = ToBitmapMultiColor(basePalette, token);
                    }
                break;
                case C64VideoMode.HiResFlexibleLineInterpretation:
                    {
                        preprocessedData = ToBitmapHiresFli(basePalette, token);
                    }
                    break;
                case C64VideoMode.CharsetMulticolor:
                    {
                        preprocessedData = ToCharsetMulticolor(token);
                    }
                    break;

                default:
                break;
            }
            if (preprocessedData != null)
            {
                return ColorTransformResult.CreateValidResult(ImageSource, preprocessedData);
            }
            else
            {
                return ColorTransformResult.CreateErrorResult("Error during C64 transformation", null);
            }
        }
    }
}