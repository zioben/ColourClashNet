using ColourClashNet.Color.Tile;
using ColourClashNet.Imaging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// ZX Spectrum attribute mode: 8x8 cells with 2 colors each, from either the normal or the bright 8 color palette.
    /// <para/>
    /// Source colors are first classified on an "input seed" palette (the intensity at which ZX colors appear in the source),
    /// then remapped to the real ZX colors and reduced per cell. Normal and bright results are merged choosing,
    /// for each cell, the palette with the lower error. The autotune mode searches the best input seeds.
    /// </summary>
    public class ColorTransformZxSpectrum : ColorTransformBase
    {
        static string sC = nameof(ColorTransformZxSpectrum);

        /// <summary>
        /// ZX Spectrum palettes used.
        /// </summary>
        public enum ZxPaletteMode
        {
            /// <summary>
            /// Each cell uses the best of the normal and bright palettes.
            /// </summary>
            BothPalettes = 0,
            /// <summary>
            /// Normal palette only.
            /// </summary>
            PaletteLo = 1,
            /// <summary>
            /// Bright palette only.
            /// </summary>
            PaletteHi = 2,
            /// <summary>
            /// Maps the image to the full ZX palette without attribute constraints.
            /// </summary>
            DebugPalette = 3,
        }

        /// <summary>
        /// Automatic search of the input seed values.
        /// </summary>
        public enum ZxAutotuneMode
        {
            /// <summary>
            /// Use the configured seeds.
            /// </summary>
            None = 0,
            /// <summary>
            /// Coarse search.
            /// </summary>
            Fast = 1,
            /// <summary>
            /// Coarse search followed by a refinement around the best result.
            /// </summary>
            Detailed = 2,
        }

        /// <summary>
        /// Creates the ZX Spectrum transformation.
        /// </summary>
        public ColorTransformZxSpectrum()
        {
            Type = ColorTransformType.ColorReductionZxSpectrum;
            Description = "Reduce color to ZX Spectrum color map and apply ColourClash reduction";
        }

        /// <summary>
        /// Result of one ZX processing run with a given pair of input seeds.
        /// </summary>
        public class ZxProcessingResults
        {
            /// <summary>
            /// Input seed of the normal palette.
            /// </summary>
            public int ColorSeedInLow { get; internal set; }
            /// <summary>
            /// Input seed of the bright palette.
            /// </summary>
            public int ColorSeedInHigh { get; internal set; }
            /// <summary>
            /// Image processed with the normal palette.
            /// </summary>
            public ImageData? ProcessedImageLow { get; private set; }
            /// <summary>
            /// Image processed with the bright palette.
            /// </summary>
            public ImageData? ProcessedImageHigh { get; private set; }
            /// <summary>
            /// Final image: best cells of both palettes.
            /// </summary>
            public ImageData? MergeImage { get; internal set; }
            /// <summary>
            /// Mean color error of <see cref="MergeImage"/> against the source image.
            /// </summary>
            public double MergeError { get; internal set; }
            /// <summary>
            /// Tiles processed with the normal palette.
            /// </summary>
            public TileManager? TileManagerLow { get; private set; }
            /// <summary>
            /// Tiles processed with the bright palette.
            /// </summary>
            public TileManager? TileManagerHigh { get; private set; }

            /// <summary>
            /// Builds the processed images, merges them and evaluates the merge error.
            /// </summary>
            internal bool Create(TileManager tileManagerLow, TileManager tileManagerHigh, ImageData referenceImage, ColorDistanceEvaluationMode colorEvaluationMode, CancellationToken token = default)
            {
                TileManagerLow = tileManagerLow;
                TileManagerHigh = tileManagerHigh;
                ProcessedImageLow = tileManagerLow?.CreateImageFromTiles();
                ProcessedImageHigh = tileManagerHigh?.CreateImageFromTiles();
                MergeImage = TileManager.MergeToImage(new List<TileManager?> { tileManagerLow, tileManagerHigh });
                MergeError = ColorIntExt.EvaluateError(referenceImage, MergeImage, colorEvaluationMode, token);
                return true;
            }
        }


        /// <summary>
        /// Normal palette input seed (clamped to [8, high seed]).
        /// </summary>
        public int ZxPaletteInColorSeedLow
        {
            get => config.ZxPaletteInColorSeedLow;
            set
            {
                config.ZxPaletteInColorSeedLow = Math.Min(Math.Max(8, value), ZxPaletteInColorSeedHigh);
            }
        }

        /// <summary>
        /// Bright palette input seed (clamped to [low seed, 255]).
        /// </summary>
        public int ZxPaletteInColorSeedHigh
        {
            get => config.ZxPaletteInColorSeedHigh;
            set
            {
                config.ZxPaletteInColorSeedHigh = Math.Min(Math.Max(ZxPaletteInColorSeedLow, value), 0x00FF);
            }
        }

        /// <summary>
        /// Palettes used.
        /// </summary>
        public ZxPaletteMode PaletteMode 
        { 
            get => config.ZxPaletteMode;
            set => config.ZxPaletteMode = value;
        }
        /// <summary>
        /// Includes black in the bright palette.
        /// </summary>
        public bool IncludeBlackInHighColor 
        { 
            get => config.ZxIncludeBlackInHighColorImage;
            set => config.ZxIncludeBlackInHighColorImage = value;
        }
        /// <summary>
        /// Input seed search mode.
        /// </summary>
        public ZxAutotuneMode AutotuneMode 
        { 
            get => config.ZxAutotuneMode;
            set => config.ZxAutotuneMode = value;
        }
        /// <summary>
        /// Dithers the normal palette image.
        /// </summary>
        public bool DitherLowColorImage
        { 
            get => config.ZxDitherLowColorImage;
            set => config.ZxDitherLowColorImage = value;
        } 
        /// <summary>
        /// Dithers the bright palette image.
        /// </summary>
        public bool DitherHighColorImage 
        { 
            get => config.ZxDitherHighColorImage;
            set => config.ZxDitherHighColorImage = value;
        }
        /// <summary>
        /// Channel intensity of the real normal ZX colors.
        /// </summary>
        public int ZxPaletteOutColorSeedLow { get; init; } = 0x00D8;
        /// <summary>
        /// Channel intensity of the real bright ZX colors.
        /// </summary>
        public int ZxPaletteOutColorSeedHigh { get; init; } = 0x00FF;

        /// <summary>
        /// Reduction used for each attribute cell.
        /// </summary>
        ColorTransformType TransformType { get; set; } = ColorTransformType.ColorReductionClustering;
        /// <summary>
        /// Creates the cell configuration: 2 colors picked from the cell palette.
        /// </summary>
        ColorTransformConfig CreateConfig(Palette palette, ColorDithering ditheringType)
        {
            return config.Clone()
                .WithReferencePalette(palette)
                .WithDithering(DitheringConfig)
                .WithDitheringType(ditheringType)
                .WithClustering(2, 6, ColorSelectionMode.UseColorPalette);                
        }


        /// <summary>
        /// Sets the palette settings (fluent).
        /// </summary>
        public ColorTransformZxSpectrum WithZxScreenMode(ZxPaletteMode paletteMode, int lowColorInSeed, int highColorInSeed)
        {
            ZxPaletteInColorSeedLow = lowColorInSeed;
            ZxPaletteInColorSeedHigh = highColorInSeed;
            PaletteMode = paletteMode;
            return this;
        }
        /// <summary>
        /// Sets the palette settings from a configuration (fluent).
        /// </summary>
        public ColorTransformZxSpectrum WithZxScreenMode(ColorTransformConfig cfg)
            => WithZxScreenMode(cfg.ZxPaletteMode, cfg.ZxPaletteInColorSeedLow, cfg.ZxPaletteInColorSeedHigh);

        /// <summary>
        /// Sets the processing settings (fluent).
        /// </summary>
        public ColorTransformZxSpectrum WithProcessingParams(ZxAutotuneMode autotuneMode, bool ditherLowColorImage, bool ditherHighColorImage, bool includeBlackInHighColor)
        {
            DitherLowColorImage = ditherLowColorImage;
            DitherHighColorImage = ditherHighColorImage;
            IncludeBlackInHighColor = includeBlackInHighColor;
            AutotuneMode = autotuneMode;
            return this;
        }

        /// <summary>
        /// Sets the processing settings from a configuration (fluent).
        /// </summary>
        public ColorTransformZxSpectrum WithProcessingParams(ColorTransformConfig cfg)
            => WithProcessingParams(cfg.ZxAutotuneMode, cfg.ZxDitherLowColorImage, cfg.ZxDitherHighColorImage, cfg.ZxIncludeBlackInHighColorImage);


        /// <summary>
        /// Creates the map from the 8 input seed colors to the 8 real ZX colors of one palette.
        /// </summary>
        /// <param name="colorSeedIn">Channel intensity of the colors in the source image</param>
        /// <param name="colorSeedOut">Channel intensity of the real ZX colors</param>
        /// <param name="mapTheBlack">Includes black in the map</param>
        ColorTransformationMap CreateZxMap(int colorSeedIn, int colorSeedOut, bool mapTheBlack)
        {
            ColorTransformationMap map = new ColorTransformationMap();
            if (mapTheBlack)
            {
                map.Add(ColorIntExt.FromRGB(0, 0, 0), ColorIntExt.FromRGB(0, 0, 0));
            }
            map.Add(ColorIntExt.FromRGB(0, 0, colorSeedIn), ColorIntExt.FromRGB(0, 0, colorSeedOut));
            map.Add(ColorIntExt.FromRGB(colorSeedIn, 0, 0), ColorIntExt.FromRGB(colorSeedOut, 0, 0));
            map.Add(ColorIntExt.FromRGB(colorSeedIn, 0, colorSeedIn), ColorIntExt.FromRGB(colorSeedOut, 0, colorSeedOut));
            map.Add(ColorIntExt.FromRGB(0, colorSeedIn, 0), ColorIntExt.FromRGB(0, colorSeedOut, 0));
            map.Add(ColorIntExt.FromRGB(0, colorSeedIn, colorSeedIn), ColorIntExt.FromRGB(0, colorSeedOut, colorSeedOut));
            map.Add(ColorIntExt.FromRGB(colorSeedIn, colorSeedIn, 0), ColorIntExt.FromRGB(colorSeedOut, colorSeedOut, 0));
            map.Add(ColorIntExt.FromRGB(colorSeedIn, colorSeedIn, colorSeedIn), ColorIntExt.FromRGB(colorSeedOut, colorSeedOut, colorSeedOut));
            return map;
        }


        /// <summary>
        /// Maps the image to the full ZX palette (plus mixed colors) without attribute constraints: the reference for the error evaluation.
        /// </summary>
        ImageData CreateBestZxImage(CancellationToken token = default)
        {
            var zxPalette = new Palette();
            zxPalette.Add(ColorIntExt.FromRGB(0, 0, 0));
            // 8 normal colors (intensity 0xD8) + 7 bright colors (0xFF); bright black equals black
            var l = ZxPaletteOutColorSeedLow;
            zxPalette.Add(ColorIntExt.FromRGB(0, 0, l));
            zxPalette.Add(ColorIntExt.FromRGB(0, l, 0));
            zxPalette.Add(ColorIntExt.FromRGB(0, l, l));
            zxPalette.Add(ColorIntExt.FromRGB(l, 0, 0));
            zxPalette.Add(ColorIntExt.FromRGB(l, 0, l));
            zxPalette.Add(ColorIntExt.FromRGB(l, l, 0));
            zxPalette.Add(ColorIntExt.FromRGB(l, l, l));
            l = ZxPaletteOutColorSeedHigh;
            zxPalette.Add(ColorIntExt.FromRGB(0, 0, l));
            zxPalette.Add(ColorIntExt.FromRGB(0, l, 0));
            zxPalette.Add(ColorIntExt.FromRGB(0, l, l));
            zxPalette.Add(ColorIntExt.FromRGB(l, 0, 0));
            zxPalette.Add(ColorIntExt.FromRGB(l, 0, l));
            zxPalette.Add(ColorIntExt.FromRGB(l, l, 0));
            zxPalette.Add(ColorIntExt.FromRGB(l, l, l));

            var zxBaseTransform = new ColorTransformFixedPaletteEnhanced()
                .WithColorDistanceEvaluationMode(ColorDistanceEvaluationMode)
                .WithReferencePalette(zxPalette)
                .WithDithering(DitheringConfig)
               .Create(ImageSource);
            var zxBaseResult = zxBaseTransform.ProcessColors(token);
            var zxBaseImage = zxBaseResult.DataOut;
            return zxBaseImage;
        }

        /// <summary>
        /// Processes the image with one palette: seed classification, mapping to real ZX colors and per cell reduction.
        /// </summary>
        /// <param name="loPalette">True for the normal palette, false for the bright one</param>
        /// <param name="zxBestImage">Reference image for the error evaluation</param>
        /// <param name="transformationMap">Seed to ZX color map</param>
        /// <param name="useDithering">Enables dithering</param>
        /// <param name="token">Cancellation token</param>
        TileManager CreateAndProcessTiles(bool loPalette, ImageData zxBestImage, ColorTransformationMap transformationMap, bool useDithering, CancellationToken token = default)
        {
            var dithering = useDithering ? DitheringConfig.DitheringType : ColorDithering.None;

            // Create the best input image on the input seed palette
            var zxBaseTransform = new ColorTransformFixedPalette()
               .WithReferencePalette(transformationMap.GetInputPalette())
               .WithColorDistanceEvaluationMode(ColorDistanceEvaluationMode)
               .WithDithering(DitheringConfig)
               .Create(ImageSource,ImageReference);
            var zxBaseResult = zxBaseTransform.ProcessColors(token);
            var zxBaseImage = zxBaseResult.DataOut;

            // Transform to real ZX colors
            var zxRealImage = transformationMap.Transform(zxBaseImage, token);
            // Evaluate 8x8 attribute cells - 2 colors per cell.
            // The bright palette error is weighted half, so that it wins ties against the normal palette.
            TileManager tileManager = new TileManager().Create(8, 8, zxRealImage, loPalette ? 1.0 : 2.0, CreateConfig(new Palette(), dithering), token);
            var tileResult = tileManager.ProcessColors(token);
            var normalization = ColorIntExt.GetMaxColorDistance(transformationMap.GetOutputPalette(), ColorDistanceEvaluationMode, token);
            var error = tileManager.RecalcGlobalTransformationError(zxBestImage, token);

            return tileManager;
        }


        /// <summary>
        /// Runs the processing with the given input seeds, for the palettes selected by <see cref="PaletteMode"/>.
        /// </summary>
        protected ZxProcessingResults ExecuteTransformZx(ImageData zxBestImage, int colorSeedInLo, int colorSeedInHi, CancellationToken token = default)
        {
            string sM = nameof(ExecuteTransformZx);
            var zxTransformationMapLo = CreateZxMap(colorSeedInLo, ZxPaletteOutColorSeedLow, true);
            var zxTransformationMapHi = CreateZxMap(colorSeedInHi, ZxPaletteOutColorSeedHigh, IncludeBlackInHighColor);

            List<Task<TileManager>> taskList = new List<Task<TileManager>>();
            // Process the normal and the bright palette in parallel; a null slot means "palette not used"
            var processingResults = new ZxProcessingResults()
            {
                ColorSeedInLow = colorSeedInLo,
                ColorSeedInHigh = colorSeedInHi,
            };
            switch (PaletteMode)
            {
                case ZxPaletteMode.PaletteLo:
                    {
                        taskList.Add(Task<TileManager>.Run(() => CreateAndProcessTiles(true, zxBestImage, zxTransformationMapLo, DitherLowColorImage, token)));
                        taskList.Add(null);
                    }
                    break;
                case ZxPaletteMode.PaletteHi:
                    {
                        taskList.Add(null);
                        taskList.Add(Task<TileManager>.Run(() => CreateAndProcessTiles(false, zxBestImage, zxTransformationMapHi, DitherHighColorImage, token)));
                    }
                    break;
                case ZxPaletteMode.BothPalettes:
                    {
                        taskList.Add(Task<TileManager>.Run(() => CreateAndProcessTiles(true, zxBestImage, zxTransformationMapLo, DitherLowColorImage, token)));
                        taskList.Add(Task<TileManager>.Run(() => CreateAndProcessTiles(false, zxBestImage, zxTransformationMapHi, DitherHighColorImage, token)));
                    }
                    break;
                default:
                    throw new ArgumentException($"Unknown ZX Spectrum palette mode: {PaletteMode}");
            }

            // Wait for completion, then merge picking the best attribute cell from either palette
            var taskListValid = taskList.Where(task => task != null).ToList();
            Task.WaitAll(taskListValid.ToArray());
            processingResults.Create(taskList[0]?.Result, taskList[1]?.Result, ImageSource, ColorDistanceEvaluationMode, token);
            return processingResults;
        }




        /// <summary>
        /// Runs the processing with the configured seeds or, in autotune mode, searches the seeds with the lowest error.
        /// </summary>
        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {
            string sM = nameof(ExecuteTransform);

            BypassDithering = true;

            var bestZxImage = CreateBestZxImage(token);
            if (PaletteMode == ZxPaletteMode.DebugPalette)
            {
                return ColorTransformResult.CreateValidResult(ImageSource, bestZxImage);
            }

            if (AutotuneMode == ZxAutotuneMode.None)
            {
                var processingResult = ExecuteTransformZx(bestZxImage, ZxPaletteInColorSeedLow, ZxPaletteInColorSeedHigh, token);
                TransformationError = processingResult.MergeError;
                return ColorTransformResult.CreateValidResult(ImageSource, processingResult.MergeImage);
            }
            else
            {
                // Autotune: grid search of the (low, high) input seeds, coarse (step 32) then,
                // in detailed mode, refined (step 8) around the best result
                ZxProcessingResults bestResult = null;
                int step = 32;
                int cycle = 0;
                int start = ZxPaletteInColorSeedLow;
                int endL = 256 + (step / 2);
                int endH = 256 + (step / 2);
                int loop = AutotuneMode == ZxAutotuneMode.Fast ? 1 : 2;
                for (int i = 0; i < loop; i++)
                {
                    for (int low = start; low < endL; low += step)
                    {
                        int lowClamped = Math.Min(255, low);
                        for (int high = low + step - 1; high < endH; high += step, cycle++)
                        {
                            int highClamped = Math.Min(255, high);
                            RaiseProcessPartialEvent(new ColorProcessingEventArgs()
                            {
                                ProcessingResults = ColorTransformResult.CreateValidResult(null, null, $"step {cycle} : Range [{lowClamped} - {highClamped}]")
                            });
                            token.ThrowIfCancellationRequested();
                            var processingResult = ExecuteTransformZx(bestZxImage, lowClamped, highClamped, token);

                            if ((bestResult?.MergeError ?? double.PositiveInfinity) > processingResult.MergeError)
                            {
                                bestResult = processingResult;
                                RaiseProcessPartialEvent(new ColorProcessingEventArgs()
                                {
                                    ColorTransformInterface = this,
                                    CompletedPercent = ((cycle * step * step) / ((256.0 - ZxPaletteInColorSeedLow) * (256.0 - ZxPaletteInColorSeedLow))) * 100.0,
                                    ProcessingResults = ColorTransformResult.CreateValidResult(ImageSource, processingResult?.MergeImage, $"step {cycle} : Range [{lowClamped} - {highClamped}] : Error = {processingResult?.MergeError}"),
                                });
                            }
                        }
                    }
                    step = 8;
                    start = Math.Max(0, (int)(bestResult.ColorSeedInLow - 16));
                    endL = (int)(bestResult.ColorSeedInLow + 16);
                    endH = bestResult.ColorSeedInHigh + 9;
                    RaiseProcessPartialEvent(new ColorProcessingEventArgs()
                    {
                        ProcessingResults = ColorTransformResult.CreateValidResult(null, null, $"Refine Low palette : Range [{start} - {endL}]")
                    });
                    RaiseProcessPartialEvent(new ColorProcessingEventArgs()
                    {
                        ProcessingResults = ColorTransformResult.CreateValidResult(null, null, $"Refine High palette : until {endH}")
                    });
                }


                if (bestResult?.MergeImage != null)
                {
                    return ColorTransformResult.CreateValidResult(ImageSource, bestResult?.MergeImage);
                }
                else
                {
                    return ColorTransformResult.CreateErrorResult("Image process creation error");
                }
            }
        }
    }
}




     