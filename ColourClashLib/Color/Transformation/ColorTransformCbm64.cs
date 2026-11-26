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
    public class ColorTransformCbm64 : ColorTransformBase<C64Options>
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


        #endregion

        #region properties



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
        


        TileManager tileManager = new TileManager();

        #endregion

        /// <summary>
        /// Creates the C64 transformation and builds the enhanced palette.
        /// </summary>
        public ColorTransformCbm64()
        {
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
            // Tile colors must be picked from the (C64) image colors: the default tile reduction uses the palette selection
            var cfg = Options.InheritCommon(Options.TileReduction) with { MaxColors = maxColors, UserPalette = referencePalette.ToList() };
            tileManager = new TileManager()
                .WithTileBorder(Options.ShowTileBorders, Options.TileBorderColor);
            // Subscribe before Create: the tiles forward their transformation events through the manager
            ForwardEventsFrom(tileManager);
            tileManager.Create(tileWidth, tileHeight, image, 1.0, cfg, token);
            return tileManager;
        }

        /// <summary>
        /// Maps the image to <paramref name="palette"/> without tile constraints: the best result obtainable with that palette.
        /// </summary>
        /// <returns>A valid result with the mapped image, or an error result.</returns>
        ColorTransformResult ToDebugImage(ImageData image, Palette palette, CancellationToken token = default)
        {
            string sM = nameof(ToDebugImage);
            var paletteTransform = ForwardEventsFrom(new ColorTransformFixedPalette
            {
                Options = new FixedPaletteOptions { UserPalette = palette.ToList(), Dithering = DitheringOptions, DistanceMode = Options.DistanceMode },
            });
            var result = paletteTransform.Process(image, null, token);
            if (result == null || !result.IsSuccess || result.DataOut == null)
            {
                LogMan.Error(sC, sM, $"{Type} : C64 palette mapping failed : {result?.Message}");
                return ColorTransformResult.CreateErrorResultFrom(sC, sM, image, $"{Type} : C64 palette mapping failed", result);
            }
            return ColorTransformResult.CreateValidResult(image, result.DataOut);
        }

        /// <summary>
        /// Bitmap modes: maps the image to <paramref name="palette"/>, then color-reduces every tile independently.
        /// <list type="bullet">
        /// <item>Hires: 8x8 tiles, 2 colors per tile; Hires FLI: 8x1 tiles.</item>
        /// <item>Multicolor: 4x8 wide-pixel tiles, background color + 3 colors per tile; Multicolor FLI: 4x1 tiles.</item>
        /// </list>
        /// </summary>
        /// <param name="palette">Base or enhanced C64 palette</param>
        /// <param name="tileWidth">Tile width (in wide pixels for multicolor modes)</param>
        /// <param name="tileHeight">Tile height</param>
        /// <param name="tileColors">Colors per tile</param>
        /// <param name="multicolor">True for the wide-pixel modes, processed at half horizontal resolution</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>A valid result with the final image, or an error result describing the failed step.</returns>
        ColorTransformResult ToBitmap(Palette palette, int tileWidth, int tileHeight, int tileColors, bool multicolor, CancellationToken token = default)
        {
            string sM = nameof(ToBitmap);
            LogDebug(sC, sM, $"{Type} : {Options.VideoMode} : {tileWidth}x{tileHeight} tiles, {tileColors} colors per tile, {palette.Count} palette colors{(multicolor ? ", half horizontal resolution" : "")}");
            var workImage = multicolor ? ImageTools.HalveXResolution(ImageSource, HalveResolutionMode.MeanColor) : ImageSource;
            var mappedResult = ToDebugImage(workImage, palette, token);
            if (!mappedResult.IsSuccess)
            {
                return ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : {Options.VideoMode} palette mapping failed", mappedResult);
            }
            var mappedImage = mappedResult.DataOut;

            // Multicolor: the most frequent color becomes the global background color, shared by every tile
            var sharedColors = multicolor
                ? new HistogramRGB().Create(mappedImage).SortColorsDescending().ToPalette(1)
                : new Palette();
            if (multicolor)
                LogDebug(sC, sM, $"{Type} : shared background color {(sharedColors.Count > 0 ? sharedColors.ToList()[0].ToString("X6") : "none")}");
            TileManager manager = CreateTileManager(tileWidth, tileHeight, tileColors, mappedImage, sharedColors, token);
            var tileResult = manager.ProcessColors(token);
            if (tileResult == null || !tileResult.IsSuccess)
            {
                LogMan.Error(sC, sM, $"{Type} : {Options.VideoMode} tile processing failed : {tileResult?.Message}");
                return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, mappedImage, $"{Type} : {Options.VideoMode} tile processing failed : {tileResult?.Message}", tileResult?.Exception);
            }
            var tileImage = manager.CreateImageFromTiles();
            return ColorTransformResult.CreateValidResult(ImageSource, multicolor ? ImageTools.DoubleXResolution(tileImage, ImageSource.Columns) : tileImage);
        }

        /// <summary>
        /// Runs the conversion for the selected video mode. Dithering is handled internally.
        /// </summary>
        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {
            string sM = nameof(ExecuteTransform);
            BypassDithering = true;
            try
            {
                ColorTransformResult result;
                switch (Options.VideoMode)
                {
                    case C64VideoMode.DebugEnhancedPalette:
                        result = ToDebugImage(ImageSource, enhancedPalette, token);
                        break;
                    case C64VideoMode.DebugBasePalette:
                        result = ToDebugImage(ImageSource, basePalette, token);
                        break;
                    case C64VideoMode.Charset:
                    case C64VideoMode.HiResEnhanced:
                        result = ToBitmap(enhancedPalette, 8, 8, 2, false, token);
                        break;
                    case C64VideoMode.HiRes:
                        result = ToBitmap(basePalette, 8, 8, 2, false, token);
                        break;
                    case C64VideoMode.FlexibleLineInterpretation:
                        result = ToBitmap(basePalette, 4, 1, 4, true, token);
                        break;
                    case C64VideoMode.MulticolorEnhanced:
                        result = ToBitmap(enhancedPalette, 4, 8, 4, true, token);
                        break;
                    case C64VideoMode.Multicolor:
                        result = ToBitmap(basePalette, 4, 8, 4, true, token);
                        break;
                    case C64VideoMode.HiResFlexibleLineInterpretation:
                        result = ToBitmap(basePalette, 8, 1, 2, false, token);
                        break;
                    case C64VideoMode.CharsetMulticolor:
                    default:
                        LogMan.Error(sC, sM, $"{Type} : video mode {Options.VideoMode} not supported");
                        return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : video mode {Options.VideoMode} not supported");
                }
                if (!result.IsSuccess)
                {
                    return ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : video mode {Options.VideoMode} conversion failed", result);
                }
                return ColorTransformResult.CreateValidResult(ImageSource, result.DataOut);
            }
            catch (OperationCanceledException ex)
            {
                LogMan.Warning(sC, sM, $"{Type} : operation cancelled");
                return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : operation cancelled", ex);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, $"{Type} : video mode {Options.VideoMode} conversion failed", ex);
                return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : video mode {Options.VideoMode} conversion failed", ex);
            }
        }
    }
}