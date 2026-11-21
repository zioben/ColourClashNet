using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Commodore Amiga specific color reductions: HAM6, HAM8 and Extra Half-Brite.
    /// <para/>
    /// HAM (Hold And Modify) pixels either use a base palette color or copy the previous pixel changing only one channel:
    /// the image is quantized, a base palette is computed, then each line is encoded choosing the best HAM option per pixel.
    /// </summary>
    public class ColorTransformCbmAmiga : ColorTransformBase<AmigaOptions>
    {
        static string sC = nameof(ColorTransformCbmAmiga);


        /// <summary>
        /// Amiga video modes.
        /// </summary>
        public enum EnumAmigaVideoMode
        {
            /// <summary>
            /// Hold And Modify, 16 base colors, 4 bits per channel (OCS/ECS).
            /// </summary>
            Ham6,
            /// <summary>
            /// Hold And Modify, 64 base colors, 6 bits per channel (AGA).
            /// </summary>
            Ham8,
            /// <summary>
            /// 32 colors plus their half-bright copies (64 colors).
            /// </summary>
            ExtraHalfBright
        }

            

        /// <summary>
        /// Creates the Amiga transformation.
        /// </summary>
        public ColorTransformCbmAmiga()
        {
            Type = ColorTransformType.ColorReductionHam;
            Description = "Commodore Amiga specific color reduction";
        }








        /// <summary>
        /// Encodes the image in HAM mode, line by line (lines are independent and run in parallel).
        /// <para/>
        /// Each pixel is either the nearest base palette color or the previous pixel with one channel (R, G or B) set
        /// to the source value. Choosing only by the error of the current pixel causes fringes: a channel that is
        /// left behind takes several pixels to catch up. Every choice is therefore scored on the current pixel
        /// plus the next <paramref name="lookahead"/> pixels (see <see cref="SearchHam"/>).
        /// </summary>
        /// <param name="dataSource">Original image</param>
        /// <param name="dataPreProcessed">Image reduced to the base palette</param>
        /// <param name="quantization">Channel bit depth of the HAM mode</param>
        /// <param name="lookahead">Following pixels included in the error of each choice (0 = current pixel only)</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>The HAM image as displayed.</returns>
        ImageData ToHam(ImageData dataSource, ImageData dataPreProcessed, ColorQuantizationMode quantization, int lookahead, CancellationToken token = default)
        {
            int rows = dataSource.Rows;
            int columns = dataSource.Columns;
            var hamData = new int[rows, columns];
            var distanceMode = ColorDistanceEvaluationMode;
            Parallel.For(0, rows, new ParallelOptions { CancellationToken = token }, r =>
            {
                // Quantized target colors and nearest base colors of the line
                var targets = new int[columns];
                var baseColors = new int[columns];
                for (int c = 0; c < columns; c++)
                {
                    targets[c] = ColorTransformQuantization.QuantizeColor(quantization, dataSource.matrix[r, c]);
                    baseColors[c] = ColorTransformQuantization.QuantizeColor(quantization, dataPreProcessed.matrix[r, c]);
                }

                int rgbPrev = ColorDefaults.DefaultInvalidColorInt;
                for (int c = 0; c < columns; c++)
                {
                    if (targets[c] < 0)
                    {
                        // Non-color pixel: copied as is, the next pixel has nothing to modify
                        hamData[r, c] = targets[c];
                        rgbPrev = ColorDefaults.DefaultInvalidColorInt;
                        continue;
                    }
                    // First pixel of a line (or after a non-color pixel): only a base palette color can be used
                    int hamColor = rgbPrev < 0
                        ? baseColors[c]
                        : SearchHam(rgbPrev, targets, baseColors, c, lookahead, distanceMode, out _);
                    hamData[r, c] = hamColor;
                    rgbPrev = hamColor;
                }
            });
            return new ImageData().Create(hamData);
        }

        /// <summary>
        /// Depth limited search of the best HAM choice for pixel <paramref name="column"/>.
        /// <para/>
        /// Tries the 4 candidates (base color, modify R, modify G, modify B); each one is scored with its own error
        /// plus the best score of the following pixels, up to <paramref name="depth"/> more pixels or a non-color pixel.
        /// Branches whose partial error already exceeds the best score are skipped (errors are never negative).
        /// </summary>
        /// <param name="rgbPrev">Color displayed by the previous pixel</param>
        /// <param name="targets">Quantized source colors of the line</param>
        /// <param name="baseColors">Nearest base palette color of each pixel of the line</param>
        /// <param name="column">Pixel to encode</param>
        /// <param name="depth">Following pixels still to evaluate</param>
        /// <param name="distanceMode">Color distance metric</param>
        /// <param name="bestError">Total error of the returned choice</param>
        /// <returns>The color displayed by the best choice.</returns>
        internal static int SearchHam(int rgbPrev, int[] targets, int[] baseColors, int column, int depth, ColorDistanceEvaluationMode distanceMode, out double bestError)
        {
            int target = targets[column];
            int targetR = target.ToR();
            int targetG = target.ToG();
            int targetB = target.ToB();
            int prevR = rgbPrev.ToR();
            int prevG = rgbPrev.ToG();
            int prevB = rgbPrev.ToB();
            Span<int> candidates = stackalloc int[]
            {
                baseColors[column],
                ColorIntExt.FromRGB(targetR, prevG, prevB),
                ColorIntExt.FromRGB(prevR, targetG, prevB),
                ColorIntExt.FromRGB(prevR, prevG, targetB),
            };

            bool lookNext = depth > 0 && column + 1 < targets.Length && targets[column + 1] >= 0;
            int bestColor = candidates[0] >= 0 ? candidates[0] : candidates[1];
            bestError = double.MaxValue;
            for (int i = 0; i < candidates.Length; i++)
            {
                int candidate = candidates[i];
                if (candidate < 0)
                    continue;
                // Skip duplicates: the same color gives the same score
                bool duplicate = false;
                for (int k = 0; k < i && !duplicate; k++)
                    duplicate = candidates[k] == candidate;
                if (duplicate)
                    continue;

                double error = candidate.Distance(target, distanceMode);
                if (error >= bestError)
                    continue;
                if (lookNext)
                {
                    SearchHam(candidate, targets, baseColors, column + 1, depth - 1, distanceMode, out double nextError);
                    error += nextError;
                }
                if (error < bestError)
                {
                    bestError = error;
                    bestColor = candidate;
                }
            }
            return bestColor;
        }



        /// <summary>
        /// Reduces the image to 32 Extra Half-Brite clusters (each base color also provides its half-bright copy).
        /// </summary>
        /// <returns>A valid result with the EHB image, or an error result.</returns>
        ColorTransformResult ToEHB(ImageData dataSource, ImageData dataPreProcessed, CancellationToken token = default)
        {
            string sM = nameof(ToEHB);
            // Reuse the clustering settings of the base palette reduction, if any, with 32 base colors
            var clustering = Options.BasePaletteReduction as ClusteringOptions ?? new ClusteringOptions { Selection = ColorSelectionMode.UseColorPalette };
            var transform = ForwardEventsFrom(new ColorTransformClusteringEHB
            {
                Options = Options.InheritCommon(clustering) with { MaxColors = 32 },
            });
            var result = transform.CreateAndProcessColors(dataPreProcessed, token);
            if (result == null || !result.IsSuccess || result.DataOut == null)
            {
                LogMan.Error(sC, sM, $"{Type} : EHB clustering failed");
                return ColorTransformResult.CreateErrorResultFrom(sC, sM, dataSource, $"{Type} : EHB clustering failed", result);
            }
            return ColorTransformResult.CreateValidResult(dataSource, result.DataOut);
        }


        /// <summary>
        /// Runs the quantization and the mode specific encoding.
        /// </summary>
        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {
            string sM = nameof(ExecuteTransform);
            try
            {
                var quantization = ForwardEventsFrom(new ColorTransformQuantization
                {
                    Options = Options.InheritCommon(new QuantizationOptions { Mode = ColorQuantizationMode.RGB444 }),
                });

                int maxColors;
                ColorQuantizationMode hamQuantization;
                switch (Options.VideoMode)
                {
                    case EnumAmigaVideoMode.ExtraHalfBright:
                        {
                            var resultQuantized = quantization.CreateAndProcessColors(ImageSource, token);
                            if (resultQuantized == null || !resultQuantized.IsSuccess)
                            {
                                LogMan.Error(sC, sM, $"{Type} : RGB444 quantization failed");
                                return ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : RGB444 quantization failed", resultQuantized);
                            }
                            var ehbResult = ToEHB(ImageSource, resultQuantized.DataOut, token);
                            return ehbResult.IsSuccess
                                ? ColorTransformResult.CreateValidResult(ImageSource, ehbResult.DataOut)
                                : ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : EHB conversion failed", ehbResult);
                        }
                    case EnumAmigaVideoMode.Ham6:
                        maxColors = 16;
                        hamQuantization = ColorQuantizationMode.RGB444;
                        break;
                    case EnumAmigaVideoMode.Ham8:
                        maxColors = 64;
                        hamQuantization = ColorQuantizationMode.RGB666;
                        break;
                    default:
                        LogMan.Error(sC, sM, $"{Type} : unsupported video mode {Options.VideoMode}");
                        return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : unsupported video mode {Options.VideoMode}");
                }

                quantization.Options = quantization.Options with { Mode = hamQuantization };
                var quantizedResult = quantization.CreateAndProcessColors(ImageSource, token);
                if (quantizedResult == null || !quantizedResult.IsSuccess)
                {
                    LogMan.Error(sC, sM, $"{Type} : {hamQuantization} quantization failed");
                    return ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : {hamQuantization} quantization failed", quantizedResult);
                }

                // Compute the HAM base palette (16 or 64 colors) on the quantized image
                var cfg = Options.InheritCommon(Options.BasePaletteReduction) with { MaxColors = maxColors };
                var colorReduction = ForwardEventsFrom(ColorTransformInternal.Alloc(cfg));
                var dataPreprocessedResult = colorReduction.CreateAndProcessColors(quantizedResult.DataOut, token);
                if (dataPreprocessedResult == null || !dataPreprocessedResult.IsSuccess)
                {
                    LogMan.Error(sC, sM, $"{Type} : HAM base palette reduction failed");
                    return ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : HAM base palette reduction failed", dataPreprocessedResult);
                }
                BypassDithering = true;

                var amigaData = ToHam(ImageSource, dataPreprocessedResult.DataOut, hamQuantization, Options.HamLookahead, token);
                return ColorTransformResult.CreateValidResult(ImageSource, amigaData);
            }
            catch (OperationCanceledException ex)
            {
                LogMan.Exception(sC, sM, $"{Type} : operation cancelled", ex);
                return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : operation cancelled", ex);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, $"{Type} : {Options.VideoMode} conversion failed", ex);
                return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : {Options.VideoMode} conversion failed", ex);
            }
        }

    }
}
