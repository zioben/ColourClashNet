using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// IBM CGA video modes: maps the image to the palette of the selected mode.
    /// <para/>
    /// Palette modes map every pixel to the nearest mode color. Composite modes emulate the NTSC artifact colors
    /// of 320x200 mode 4: each pair of adjacent pixels is seen as one of 16 colors, so the image is processed at
    /// half horizontal resolution and then doubled back (as the C64 multicolor and CPC wide-pixel modes).
    /// </summary>
    public class ColorTransformCGA : ColorTransformPaletteMapping<CgaOptions>
    {
        static readonly string sC = nameof(ColorTransformCGA);

        /// <summary>
        /// CGA video modes.
        /// </summary>
        public enum CGAVideoMode
        {
            /// <summary>
            /// 320x200, palette 0 (green, red, brown), low intensity.
            /// </summary>
            Mode4_0L,
            /// <summary>
            /// 320x200, palette 0, high intensity.
            /// </summary>
            Mode4_0H,
            /// <summary>
            /// 320x200, palette 1 (cyan, magenta, white), low intensity.
            /// </summary>
            Mode4_1L,
            /// <summary>
            /// 320x200, palette 1, high intensity.
            /// </summary>
            Mode4_1H,
            /// <summary>
            /// 320x200, mode 5 palette (cyan, red, white), low intensity.
            /// </summary>
            Mode5_1L,
            /// <summary>
            /// 320x200, mode 5 palette, high intensity.
            /// </summary>
            Mode5_1H,
            /// <summary>640x200, black and white.</summary>
            Mode6,
            /// <summary>Composite artifact colors of mode 4 palette 0: 16 colors at half horizontal resolution.</summary>
            Composite_0,
            /// <summary>Composite artifact colors of mode 4 palette 1: 16 colors at half horizontal resolution.</summary>
            Composite_1,
            /// <summary>Full 16 color RGBI palette.</summary>
            DebugPalette,
        }

        /// <summary>
        /// CGA output image type.
        /// </summary>
        public enum CGACompositeOutputMode
        {
            /// <summary>Image as displayed.</summary>
            ProcessedImage,
            /// <summary>
            /// Pixels as written in video memory. Palette modes are unchanged; in composite modes each
            /// artifact color is replaced by the pair of mode 4 pixels that generates it.
            /// </summary>
            RawImage,
        }


        // Palettes of each video mode; composite colors carry the mode 4 pixel pair that generates them
        EncodingPatternDictionary<List<int>> patternFull = new();
        EncodingPatternDictionary<List<int>> patternComposite0 = new();
        EncodingPatternDictionary<List<int>> patternComposite1 = new();
        EncodingPatternDictionary<List<int>> patternMode4_0L = new();
        EncodingPatternDictionary<List<int>> patternMode4_0H = new();
        EncodingPatternDictionary<List<int>> patternMode4_1L = new();
        EncodingPatternDictionary<List<int>> patternMode4_1H = new();
        EncodingPatternDictionary<List<int>> patternMode5_1L = new();
        EncodingPatternDictionary<List<int>> patternMode5_1H = new();
        EncodingPatternDictionary<List<int>> patternMode6 = new();



        /// <summary>
        /// Creates the CGA transformation and builds the mode palettes.
        /// </summary>
        public ColorTransformCGA()
        {
            Type = ColorTransformType.ColorReductionCGA;
            Description = "Reduce color to CGA palette";
            CreatePalette();
        }

        /// <summary>
        /// Builds the palettes of all video modes.
        /// </summary>
        void CreatePalette()
        {
            patternFull.Reset()
                .AddPattern(0x00_00_00_00)
                .AddPattern(0x00_00_00_AA)
                .AddPattern(0x00_00_AA_00)
                .AddPattern(0x00_00_AA_AA)
                //
                .AddPattern(0x00_AA_00_00)
                .AddPattern(0x00_AA_00_AA)
                .AddPattern(0x00_AA_55_00)
                .AddPattern(0x00_AA_AA_AA)
                //
                .AddPattern(0x00_55_55_55)
                .AddPattern(0x00_55_55_FF)
                .AddPattern(0x00_55_FF_55)
                .AddPattern(0x00_55_FF_FF)
                //
                .AddPattern(0x00_FF_55_55)
                .AddPattern(0x00_FF_55_FF)
                .AddPattern(0x00_FF_FF_55)
                .AddPattern(0x00_FF_FF_FF);

            // Mode 4/5 palettes are subsets of the full palette: background (black) + 3 fixed colors,
            // L/H = low/high intensity
            var pal = patternFull.GetPalette().ToList();

            patternMode4_0L.Reset()
                .AddPattern(pal[0])
                .AddPattern(pal[2])
                .AddPattern(pal[4])
                .AddPattern(pal[6]);

            patternMode4_0H.Reset()
                .AddPattern(pal[0])
                .AddPattern(pal[10])
                .AddPattern(pal[12])
                .AddPattern(pal[14]);

            patternMode4_1L.Reset()
                .AddPattern(pal[0])
                .AddPattern(pal[3])
                .AddPattern(pal[5])
                .AddPattern(pal[7]);

            patternMode4_1H.Reset()
                .AddPattern(pal[0])
                .AddPattern(pal[11])
                .AddPattern(pal[13])
                .AddPattern(pal[15]);

            patternMode5_1L.Reset()
                .AddPattern(pal[0])
                .AddPattern(pal[3])
                .AddPattern(pal[4])
                .AddPattern(pal[7]);

            patternMode5_1H.Reset()
                .AddPattern(pal[0])
                .AddPattern(pal[11])
                .AddPattern(pal[12])
                .AddPattern(pal[15]);

            patternMode6.Reset()
                .AddPattern(pal[0])
                .AddPattern(pal[15]);

            // Composite artifact colors (measured values). Color i is generated by the mode 4 pixel pair
            // (i >> 2, i & 3): left pixel in the high bits, as in the video memory byte.
            // The pixel indexes refer to the high intensity mode 4 palettes.
            int[] mode4Palette0H = { pal[0], pal[10], pal[12], pal[14] };
            int[] mode4Palette1H = { pal[0], pal[11], pal[13], pal[15] };
            patternComposite0.Reset()
                .AddPattern(0x00_00_00_00, Pair(mode4Palette0H, 0))
                .AddPattern(0x00_00_71_D1, Pair(mode4Palette0H, 1))
                .AddPattern(0x00_00_19_AC, Pair(mode4Palette0H, 2))
                .AddPattern(0x00_00_71_F1, Pair(mode4Palette0H, 3))
                //
                .AddPattern(0x00_95_4F_00, Pair(mode4Palette0H, 4))
                .AddPattern(0x00_6D_D4_41, Pair(mode4Palette0H, 5))
                .AddPattern(0x00_A2_7B_1C, Pair(mode4Palette0H, 6))
                .AddPattern(0x00_74_D4_61, Pair(mode4Palette0H, 7))
                //
                .AddPattern(0x00_B8_21_00, Pair(mode4Palette0H, 8))
                .AddPattern(0x00_90_A6_9C, Pair(mode4Palette0H, 9))
                .AddPattern(0x00_C5_4E_76, Pair(mode4Palette0H, 10))
                .AddPattern(0x00_97_A6_BB, Pair(mode4Palette0H, 11))
                //
                .AddPattern(0x00_F3_68_00, Pair(mode4Palette0H, 12))
                .AddPattern(0x00_CB_ED_26, Pair(mode4Palette0H, 13))
                .AddPattern(0x00_FF_95_01, Pair(mode4Palette0H, 14))
                .AddPattern(0x00_D2_ED_46, Pair(mode4Palette0H, 15));

            patternComposite1.Reset()
                .AddPattern(0x00_00_00_00, Pair(mode4Palette1H, 0))
                .AddPattern(0x00_00_9A_FF, Pair(mode4Palette1H, 1))
                .AddPattern(0x00_00_42_FF, Pair(mode4Palette1H, 2))
                .AddPattern(0x00_00_90_FF, Pair(mode4Palette1H, 3))
                //
                .AddPattern(0x00_AA_4C_00, Pair(mode4Palette1H, 4))
                .AddPattern(0x00_84_FA_D2, Pair(mode4Palette1H, 5))
                .AddPattern(0x00_B9_A2_AD, Pair(mode4Palette1H, 6))
                .AddPattern(0x00_96_F0_FF, Pair(mode4Palette1H, 7))
                //
                .AddPattern(0x00_CD_1F_00, Pair(mode4Palette1H, 8))
                .AddPattern(0x00_A7_CD_FF, Pair(mode4Palette1H, 9))
                .AddPattern(0x00_DC_75_FF, Pair(mode4Palette1H, 10))
                .AddPattern(0x00_B9_C3_FF, Pair(mode4Palette1H, 11))
                //
                .AddPattern(0x00_FF_5C_00, Pair(mode4Palette1H, 12))
                .AddPattern(0x00_ED_FF_CC, Pair(mode4Palette1H, 13))
                .AddPattern(0x00_FF_B2_A6, Pair(mode4Palette1H, 14))
                .AddPattern(0x00_FF_FF_FF, Pair(mode4Palette1H, 15));
        }

        /// <summary>
        /// Returns the two mode 4 pixels that generate the composite color <paramref name="index"/>.
        /// </summary>
        /// <param name="mode4Palette">High intensity mode 4 palette (background + 3 colors) the pixels are taken from</param>
        /// <param name="index">Composite color index [0, 15]</param>
        static List<int> Pair(int[] mode4Palette, int index)
            => new List<int> { mode4Palette[index >> 2], mode4Palette[index & 3] };

        /// <summary>
        /// True for the composite artifact modes.
        /// </summary>
        public static bool IsCompositeMode(CGAVideoMode videoMode)
            => videoMode is CGAVideoMode.Composite_0 or CGAVideoMode.Composite_1;

        /// <summary>
        /// Selects the palette of the current video mode.
        /// </summary>
        /// <exception cref="NotImplementedException">Unsupported video mode.</exception>
        protected override Palette GetTargetPalette()
        {
            Palette refPalette;
            switch (Options.VideoMode)
            {
                case CGAVideoMode.Mode4_0L:
                    refPalette = patternMode4_0L.GetPalette();
                    break;
                case CGAVideoMode.Mode4_0H:
                    refPalette = patternMode4_0H.GetPalette();
                    break;
                case CGAVideoMode.Mode4_1L:
                    refPalette = patternMode4_1L.GetPalette();
                    break;
                case CGAVideoMode.Mode4_1H:
                    refPalette = patternMode4_1H.GetPalette();
                    break;
                case CGAVideoMode.Mode5_1L:
                    refPalette = patternMode5_1L.GetPalette();
                    break;
                case CGAVideoMode.Mode5_1H:
                    refPalette = patternMode5_1H.GetPalette();
                    break;
                case CGAVideoMode.Mode6:
                    refPalette = patternMode6.GetPalette();
                    break;
                case CGAVideoMode.Composite_0:
                    refPalette = patternComposite0.GetPalette();
                    break;
                case CGAVideoMode.Composite_1:
                    refPalette = patternComposite1.GetPalette();
                    break;
                case CGAVideoMode.DebugPalette:
                    refPalette = patternFull.GetPalette();
                    break;
                default:
                    throw new NotImplementedException($"{Options.VideoMode} Unsupported video mode");

            }
            return refPalette;
        }


        /// <summary>
        /// Applies the palette of the video mode: palette modes use the transformation map,
        /// composite modes are processed at half horizontal resolution.
        /// </summary>
        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {
            string sM = nameof(ExecuteTransform);
            LogDebug(sC, sM, $"{Type} : {Options.VideoMode}, output {Options.OutputMode}, {GetTargetPalette().Count} palette colors");
            switch (Options.VideoMode)
            {
                case CGAVideoMode.Mode4_0L:
                case CGAVideoMode.Mode4_0H:
                case CGAVideoMode.Mode4_1L:
                case CGAVideoMode.Mode4_1H:
                case CGAVideoMode.Mode5_1L:
                case CGAVideoMode.Mode5_1H:
                case CGAVideoMode.Mode6:
                case CGAVideoMode.DebugPalette:
                    // The raw image of a palette mode is the processed image itself
                    return base.ExecuteTransform(token);
                case CGAVideoMode.Composite_0:
                    return ToComposite(patternComposite0, token);
                case CGAVideoMode.Composite_1:
                    return ToComposite(patternComposite1, token);
                default:
                    LogMan.Error(sC, sM, $"{Type} : unsupported video mode {Options.VideoMode}");
                    return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : unsupported video mode {Options.VideoMode}");
            }
        }

        /// <summary>
        /// Composite conversion: halves the horizontal resolution (each pixel pair is one artifact color),
        /// maps and dithers on the half resolution image, then doubles the resolution back.
        /// <para/>
        /// For <see cref="CGACompositeOutputMode.RawImage"/> each artifact color becomes the mode 4 pixel pair that generates it.
        /// </summary>
        /// <param name="compositePalette">Artifact colors of the mode, with their pixel pairs</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>A valid result with the final image, or an error result.</returns>
        ColorTransformResult ToComposite(EncodingPatternDictionary<List<int>> compositePalette, CancellationToken token)
        {
            string sM = nameof(ToComposite);
            // Dithering is applied by the nested mapping on the half resolution image
            BypassDithering = true;
            try
            {
                var halfImage = ImageTools.HalveXResolution(ImageSource, HalveResolutionMode.MeanColor);
                var mapping = ForwardEventsFrom(new ColorTransformFixedPalette
                {
                    Options = Options.InheritCommon(new FixedPaletteOptions()) with { UserPalette = compositePalette.GetPalette().ToList() },
                });
                var mappedResult = mapping.CreateAndProcessColors(halfImage, token);
                if (mappedResult == null || !mappedResult.IsSuccess || mappedResult.DataOut == null)
                {
                    LogMan.Error(sC, sM, $"{Type} : composite palette mapping failed : {mappedResult?.Message}");
                    return ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : composite palette mapping failed", mappedResult);
                }

                LogTrace(sC, sM, $"{Type} : composite image {mappedResult.DataOut.Columns}x{mappedResult.DataOut.Rows} mapped, building the {Options.OutputMode}");
                var outputImage = Options.OutputMode == CGACompositeOutputMode.RawImage
                    ? ToRawPixelPairs(mappedResult.DataOut, compositePalette, ImageSource.Columns)
                    : ImageTools.DoubleXResolution(mappedResult.DataOut, ImageSource.Columns);
                return ColorTransformResult.CreateValidResult(ImageSource, outputImage);
            }
            catch (OperationCanceledException ex)
            {
                LogMan.Warning(sC, sM, $"{Type} : operation cancelled");
                return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : operation cancelled", ex);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, $"{Type} : composite conversion failed", ex);
                return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : composite conversion failed", ex);
            }
        }

        /// <summary>
        /// Replaces each artifact color of the half resolution image with its two mode 4 pixels.
        /// <para/>
        /// Colors without a pair (e.g. non-color tags) are simply duplicated. On odd widths the last pair is cut to
        /// <paramref name="width"/> columns.
        /// </summary>
        static ImageData ToRawPixelPairs(ImageData compositeImage, EncodingPatternDictionary<List<int>> compositePalette, int width)
        {
            var source = compositeImage.matrix;
            int rows = compositeImage.Rows;
            int columns = compositeImage.Columns;
            int rawColumns = Math.Min(width, columns * 2);
            var raw = new int[rows, rawColumns];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    int rgb = source[r, c];
                    var pair = compositePalette.GetPattern(rgb);
                    raw[r, 2 * c] = pair?[0] ?? rgb;
                    if (2 * c + 1 < rawColumns)
                        raw[r, 2 * c + 1] = pair?[1] ?? rgb;
                }
            }
            return new ImageData().Create(raw);
        }
    }
}
