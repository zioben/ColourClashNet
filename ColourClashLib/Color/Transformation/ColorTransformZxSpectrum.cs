using ColourClashNet.Color.Dithering;
using ColourClashNet.Color.Tile;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
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
    public class ColorTransformZxSpectrum : ColorTransformBase<ZxOptions>
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
        /// Creates the options of the attribute cell reduction (2 colors picked from the cell palette by default).
        /// </summary>
        ClusteringOptions CreateCellOptions(bool useDithering)
            => Options.InheritCommon(Options.CellReduction) with
            {
                UserPalette = Array.Empty<int>(),
                Dithering = useDithering ? DitheringOptions : DitherOptions.None,
            };

        /// <summary>
        /// Normal palette input seed actually used: <see cref="ZxOptions.SeedLow"/> clamped to [8, <see cref="SeedHighUsed"/>].
        /// </summary>
        public int SeedLowUsed => Math.Clamp(Options.SeedLow, 8, SeedHighUsed);

        /// <summary>
        /// Bright palette input seed actually used: <see cref="ZxOptions.SeedHigh"/> clamped to [8, 255].
        /// </summary>
        public int SeedHighUsed => Math.Clamp(Options.SeedHigh, 8, 255);

        /// <summary>
        /// Normal palette input seed of the last result (the best one found by the autotune, if enabled).
        /// </summary>
        public int BestSeedLow { get; private set; }

        /// <summary>
        /// Bright palette input seed of the last result (the best one found by the autotune, if enabled).
        /// </summary>
        public int BestSeedHigh { get; private set; }






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
        ColorTransformResult CreateBestZxImage(CancellationToken token = default)
        {
            string sM = nameof(CreateBestZxImage);
            var zxPalette = new Palette();
            zxPalette.Add(ColorIntExt.FromRGB(0, 0, 0));
            // 8 normal colors (intensity 0xD8) + 7 bright colors (0xFF); bright black equals black
            var l = Options.OutputLow;
            zxPalette.Add(ColorIntExt.FromRGB(0, 0, l));
            zxPalette.Add(ColorIntExt.FromRGB(0, l, 0));
            zxPalette.Add(ColorIntExt.FromRGB(0, l, l));
            zxPalette.Add(ColorIntExt.FromRGB(l, 0, 0));
            zxPalette.Add(ColorIntExt.FromRGB(l, 0, l));
            zxPalette.Add(ColorIntExt.FromRGB(l, l, 0));
            zxPalette.Add(ColorIntExt.FromRGB(l, l, l));
            l = Options.OutputHigh;
            zxPalette.Add(ColorIntExt.FromRGB(0, 0, l));
            zxPalette.Add(ColorIntExt.FromRGB(0, l, 0));
            zxPalette.Add(ColorIntExt.FromRGB(0, l, l));
            zxPalette.Add(ColorIntExt.FromRGB(l, 0, 0));
            zxPalette.Add(ColorIntExt.FromRGB(l, 0, l));
            zxPalette.Add(ColorIntExt.FromRGB(l, l, 0));
            zxPalette.Add(ColorIntExt.FromRGB(l, l, l));

            var zxBaseTransform = ForwardEventsFrom(new ColorTransformFixedPaletteEnhanced
            {
                Options = new EnhancedPaletteOptions { UserPalette = zxPalette.ToList(), DistanceMode = Options.DistanceMode, Dithering = DitheringOptions },
            });
            var zxBaseResult = zxBaseTransform.CreateAndProcessColors(ImageSource, token);
            if (zxBaseResult == null || !zxBaseResult.IsSuccess || zxBaseResult.DataOut == null)
            {
                LogMan.Error(sC, sM, $"{Type} : ZX palette mapping failed");
                return ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : ZX palette mapping failed", zxBaseResult);
            }
            return ColorTransformResult.CreateValidResult(ImageSource, zxBaseResult.DataOut);
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
            string sM = nameof(CreateAndProcessTiles);
            // Create the best input image on the input seed palette
            var zxBaseTransform = ForwardEventsFrom(new ColorTransformFixedPalette
            {
                Options = new FixedPaletteOptions { UserPalette = transformationMap.GetInputPalette().ToList(), DistanceMode = Options.DistanceMode, Dithering = DitheringOptions },
            });
            var zxBaseResult = zxBaseTransform.CreateAndProcessColors(ImageSource, ImageReference, token);
            if (zxBaseResult == null || !zxBaseResult.IsSuccess || zxBaseResult.DataOut == null)
            {
                // Runs inside a task: the error is reported to ExecuteTransform as an exception
                throw new InvalidOperationException($"{sC}.{sM} : {(loPalette ? "normal" : "bright")} palette seed mapping failed : {zxBaseResult?.Message}", zxBaseResult?.Exception);
            }
            var zxBaseImage = zxBaseResult.DataOut;

            // Transform to real ZX colors
            var zxRealImage = transformationMap.Transform(zxBaseImage, token);
            // Evaluate 8x8 attribute cells - 2 colors per cell.
            // The bright palette error is weighted half, so that it wins ties against the normal palette.
            TileManager tileManager = ForwardEventsFrom(new TileManager());
            tileManager.Create(8, 8, zxRealImage, loPalette ? 1.0 : 2.0, CreateCellOptions(useDithering), token);
            var tileResult = tileManager.ProcessColors(token);
            if (tileResult == null || !tileResult.IsSuccess)
            {
                throw new InvalidOperationException($"{sC}.{sM} : {(loPalette ? "normal" : "bright")} palette attribute cells failed : {tileResult?.Message}", tileResult?.Exception);
            }
            var normalization = ColorIntExt.GetMaxColorDistance(transformationMap.GetOutputPalette(), ColorDistanceEvaluationMode, token);
            var error = tileManager.RecalcGlobalTransformationError(zxBestImage, token);

            return tileManager;
        }


        /// <summary>
        /// Runs the processing with the given input seeds, for the palettes selected by <see cref="ZxOptions.PaletteMode"/>.
        /// </summary>
        protected ZxProcessingResults ExecuteTransformZx(ImageData zxBestImage, int colorSeedInLo, int colorSeedInHi, CancellationToken token = default)
        {
            string sM = nameof(ExecuteTransformZx);
            var zxTransformationMapLo = CreateZxMap(colorSeedInLo, Options.OutputLow, true);
            var zxTransformationMapHi = CreateZxMap(colorSeedInHi, Options.OutputHigh, Options.IncludeBlackInHighColor);

            List<Task<TileManager>> taskList = new List<Task<TileManager>>();
            // Process the normal and the bright palette in parallel; a null slot means "palette not used"
            var processingResults = new ZxProcessingResults()
            {
                ColorSeedInLow = colorSeedInLo,
                ColorSeedInHigh = colorSeedInHi,
            };
            switch (Options.PaletteMode)
            {
                case ZxPaletteMode.PaletteLo:
                    {
                        taskList.Add(Task<TileManager>.Run(() => CreateAndProcessTiles(true, zxBestImage, zxTransformationMapLo, Options.DitherLowColorImage, token)));
                        taskList.Add(null);
                    }
                    break;
                case ZxPaletteMode.PaletteHi:
                    {
                        taskList.Add(null);
                        taskList.Add(Task<TileManager>.Run(() => CreateAndProcessTiles(false, zxBestImage, zxTransformationMapHi, Options.DitherHighColorImage, token)));
                    }
                    break;
                case ZxPaletteMode.BothPalettes:
                    {
                        taskList.Add(Task<TileManager>.Run(() => CreateAndProcessTiles(true, zxBestImage, zxTransformationMapLo, Options.DitherLowColorImage, token)));
                        taskList.Add(Task<TileManager>.Run(() => CreateAndProcessTiles(false, zxBestImage, zxTransformationMapHi, Options.DitherHighColorImage, token)));
                    }
                    break;
                default:
                    throw new ArgumentException($"Unknown ZX Spectrum palette mode: {Options.PaletteMode}");
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
            try
            {
                var bestZxResult = CreateBestZxImage(token);
                if (!bestZxResult.IsSuccess)
                {
                    return ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : reference ZX image failed", bestZxResult);
                }
                var bestZxImage = bestZxResult.DataOut;
                if (Options.PaletteMode == ZxPaletteMode.DebugPalette)
                {
                    return ColorTransformResult.CreateValidResult(ImageSource, bestZxImage);
                }

                if (Options.Autotune == ZxAutotuneMode.None)
                {
                    var processingResult = ExecuteTransformZx(bestZxImage, SeedLowUsed, SeedHighUsed, token);
                    if (processingResult?.MergeImage == null)
                    {
                        LogMan.Error(sC, sM, $"{Type} : attribute cell merge failed");
                        return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, bestZxImage, $"{Type} : attribute cell merge failed");
                    }
                    TransformationError = processingResult.MergeError;
                    BestSeedLow = processingResult.ColorSeedInLow;
                    BestSeedHigh = processingResult.ColorSeedInHigh;
                    return ColorTransformResult.CreateValidResult(ImageSource, processingResult.MergeImage);
                }
                else
                {
                    // Autotune: grid search of the (low, high) input seeds, coarse (step 32) then,
                    // in detailed mode, refined (step 8) around the best result
                    ZxProcessingResults bestResult = null;
                    int step = 32;
                    int cycle = 0;
                    int start = SeedLowUsed;
                    int endL = 256 + (step / 2);
                    int endH = 256 + (step / 2);
                    int loop = Options.Autotune == ZxAutotuneMode.Fast ? 1 : 2;
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
                                    ColorTransformInterface = this,
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
                                        CompletedPercent = ((cycle * step * step) / ((256.0 - SeedLowUsed) * (256.0 - SeedLowUsed))) * 100.0,
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
                            ColorTransformInterface = this,
                            ProcessingResults = ColorTransformResult.CreateValidResult(null, null, $"Refine Low palette : Range [{start} - {endL}]")
                        });
                        RaiseProcessPartialEvent(new ColorProcessingEventArgs()
                        {
                            ColorTransformInterface = this,
                            ProcessingResults = ColorTransformResult.CreateValidResult(null, null, $"Refine High palette : until {endH}")
                        });
                    }


                    if (bestResult?.MergeImage != null)
                    {
                        BestSeedLow = bestResult.ColorSeedInLow;
                        BestSeedHigh = bestResult.ColorSeedInHigh;
                        return ColorTransformResult.CreateValidResult(ImageSource, bestResult?.MergeImage);
                    }
                    else
                    {
                        LogMan.Error(sC, sM, $"{Type} : autotune produced no image");
                        return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, bestZxImage, $"{Type} : autotune produced no image");
                    }
                }
            }
            catch (Exception ex)
            {
                // Tile processing runs in tasks: unwrap the AggregateException to report the real cause
                var cause = (ex as AggregateException)?.Flatten().InnerException ?? ex;
                if (cause is OperationCanceledException)
                {
                    LogMan.Exception(sC, sM, $"{Type} : operation cancelled", cause);
                    return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : operation cancelled", cause);
                }
                LogMan.Exception(sC, sM, $"{Type} : ZX Spectrum conversion failed", cause);
                return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : ZX Spectrum conversion failed : {cause.Message}", cause);
            }
        }
    }
}




     