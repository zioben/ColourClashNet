using ColourClashNet.Color;
using ColourClashNet.Color.Dithering;
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
    /// Amstrad CPC video modes on the 27 color hardware palette (3 levels per channel).
    /// <para/>
    /// The image is mapped to the CPC palette, reduced to the number of colors of the mode and, for the
    /// wide-pixel modes, processed at half horizontal resolution and then doubled back.
    /// </summary>
    public class ColorTransformAmstradCPC : ColorTransformPaletteMapping<CpcOptions>
    {
        static readonly string sC = nameof(ColorTransformAmstradCPC);

        /// <summary>
        /// Amstrad CPC video modes.
        /// </summary>
        public enum CPCVideoMode
        {
            /// <summary>
            /// 160x200, 16 colors, wide pixels.
            /// </summary>
            Mode0,
            /// <summary>
            /// 320x200, 4 colors.
            /// </summary>
            Mode1,
            /// <summary>
            /// 640x200, 2 colors.
            /// </summary>
            Mode2,
            /// <summary>
            /// 160x200, 4 colors, wide pixels (undocumented mode).
            /// </summary>
            Mode3,
            /// <summary>
            /// Only maps the image to the full 27 color palette.
            /// </summary>
            DebugPalette
        }

        /// <summary>
        /// Creates the CPC transformation.
        /// </summary>
        public ColorTransformAmstradCPC()
        {
            Description = "Reduce color to Amstrad CPC palette";
        }

        /// <summary>
        /// 27 color CPC hardware palette: every combination of the three channel levels 0x00, 0x80, 0xFF.
        /// </summary>
        static readonly Palette cpcPalette = CreateCpcPalette();

        static Palette CreateCpcPalette()
        {
            int[] levels = { 0x00, 0x80, 0xFF };
            var palette = new Palette();
            foreach (var g in levels)
                foreach (var r in levels)
                    foreach (var b in levels)
                        palette.Add(ColorIntExt.FromRGB(r, g, b));
            return palette;
        }

        /// <summary>
        /// Returns the 27 color CPC hardware palette.
        /// </summary>
        protected override Palette GetTargetPalette() => cpcPalette;




       


        /// <summary>
        /// Maps the image (optionally at half horizontal resolution) to the CPC palette.
        /// </summary>
        ImageData? PreProcess(bool halveRes, CancellationToken token=default)
        {           
            var workImage = halveRes ? ImageTools.HalveXResolution(ImageSource, HalveResolutionMode.OddPixel) : ImageSource;
            var mappedImage = TransformationMap.Transform(workImage, token);
            return mappedImage;
        }

        /// <summary>
        /// Reduces the mapped image to <paramref name="maxColors"/>, applies dithering and restores the horizontal resolution.
        /// </summary>
        /// <returns>A valid result with the final image, or an error result describing the failed step.</returns>
        ColorTransformResult PostProcess(ImageData image, int maxColors, bool doubleRes, CancellationToken token)
        {
            string sM = nameof(PostProcess);

            // Keep only the colors allowed by the video mode among the mapped CPC colors
            var fastReduction = ForwardEventsFrom(new ColorTransformFast
            {
                Options = new FastOptions { MaxColors = maxColors, DistanceMode = Options.DistanceMode },
            });
            var result = fastReduction.Process(image, null, token);
            if (result == null || !result.IsSuccess)
            {
                LogMan.Error(sC, sM, $"{Type} : {maxColors} color reduction failed : {result?.Message}");
                return ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : {maxColors} color reduction failed", result);
            }

            BypassDithering = true;

            var reducedImage = result.DataOut;
            if (DitheringOptions.DitheringType != ColorDithering.None)
            {
                LogTrace(sC, sM, $"{Type} : dithering {DitheringOptions.DitheringType} on the {(doubleRes ? "half" : "full")} resolution image");
                var imageRef = doubleRes ? ImageTools.HalveXResolution(ImageSource,  HalveResolutionMode.OddPixel) : ImageSource;
                var dithering = DitherBase.CreateDitherInterface(DitheringOptions);
                var ditheredResult = dithering.Dither(imageRef, reducedImage, ColorDistanceEvaluationMode, token);
                if (ditheredResult == null || !ditheredResult.IsSuccess)
                {
                    LogMan.Error(sC, sM, $"{Type} : dithering failed");
                    return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, reducedImage, $"{Type} : dithering failed : {ditheredResult?.Message}", ditheredResult?.Exception);
                }
                reducedImage = ditheredResult.DataOut;
            }
            var outputImage = doubleRes ? ImageTools.DoubleXResolution(reducedImage, ImageSource.Columns) : reducedImage;
            return ColorTransformResult.CreateValidResult(ImageSource, outputImage);
        }

        /// <summary>
        /// Maps the image to the CPC palette and reduces it to the colors of a video mode.
        /// </summary>
        /// <param name="maxColors">Colors of the video mode</param>
        /// <param name="widePixels">True for the wide-pixel modes (half horizontal resolution)</param>
        /// <param name="token">Cancellation token</param>
        ColorTransformResult ToMode(int maxColors, bool widePixels, CancellationToken token)
        {
            string sM = nameof(ToMode);
            LogDebug(sC, sM, $"{Type} : {Options.VideoMode} : {maxColors} colors{(widePixels ? ", wide pixels (half horizontal resolution)" : "")}");
            var imagePre = PreProcess(widePixels, token);
            return PostProcess(imagePre, maxColors, widePixels, token);
        }

        /// <summary>
        /// Runs the conversion for the selected video mode.
        /// </summary>
        protected override ColorTransformResult ExecuteTransform(CancellationToken token)
        {
            string sM = nameof(ExecuteTransform);
            BypassDithering = true;
            try
            {
                // Map every source color to the nearest CPC color: used by every mode
                var mapResult = CreateTransformationMap(token);
                if (mapResult == null || !mapResult.IsSuccess)
                {
                    LogMan.Error(sC, sM, $"{Type} : CPC palette map failed : {mapResult?.Message}");
                    return ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : CPC palette map failed", mapResult);
                }
                switch (Options.VideoMode)
                {
                    case CPCVideoMode.Mode0:
                        // 160x200, 16 colors, wide pixels
                        return ToMode(16, true, token);
                    case CPCVideoMode.Mode1:
                        // 320x200, 4 colors
                        return ToMode(4, false, token);
                    case CPCVideoMode.Mode2:
                        // 640x200, 2 colors
                        return ToMode(2, false, token);
                    case CPCVideoMode.Mode3:
                        // 160x200, 4 colors, wide pixels
                        return ToMode(4, true, token);
                    case CPCVideoMode.DebugPalette:
                        return ColorTransformResult.CreateValidResult(ImageSource, PreProcess(false, token));
                    default:
                        LogMan.Error(sC, sM, $"{Type} : unsupported video mode {Options.VideoMode}");
                        return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : unsupported video mode {Options.VideoMode}");
                }
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