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
    public class ColorTransformAmstradCPC : ColorTransformFixedPalette
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
        /// Target video mode.
        /// </summary>
        public CPCVideoMode VideoMode 
        { 
            get => config.CPCVideoMode;
            set => config.CPCVideoMode = value;
        }
        /// <summary>
        /// Creates the CPC transformation.
        /// </summary>
        public ColorTransformAmstradCPC()
        {
            Type = ColorTransformType.ColorReductionCPC;
            Description = "Reduce color to Amstrad CPC palette";
        }

        /// <summary>
        /// Loads the 27 color CPC hardware palette.
        /// </summary>
        protected override void RebuildReferencePalette()
        {
            WithReferencePalette(
                new List<int>
                {
                   0x00_00_00_00,
                   0x00_00_00_80,
                   0x00_00_00_FF,
                   //
                   0x00_80_00_00,
                   0x00_80_00_80,
                   0x00_80_00_FF,
                   //
                   0x00_FF_00_00,
                   0x00_FF_00_80,
                   0x00_FF_00_FF,
                   //
                   0x00_00_80_00,
                   0x00_00_80_80,
                   0x00_00_80_FF,
                   //
                   0x00_80_80_00,
                   0x00_80_80_80,
                   0x00_80_80_FF,
                   //
                   0x00_FF_80_00,
                   0x00_FF_80_80,
                   0x00_FF_80_FF,
                   //
                   0x00_00_FF_00,
                   0x00_00_FF_80,
                   0x00_00_FF_FF,
                   //
                   0x00_80_FF_00,
                   0x00_80_FF_80,
                   0x00_80_FF_FF,
                   //
                   0x00_FF_FF_00,
                   0x00_FF_FF_80,
                   0x00_FF_FF_FF,
                },
                // Force the overwrite: the reference palette may already be locked (previous run or SetProperties)
                true
            );
        }


        /// <summary>
        /// Sets the video mode (fluent).
        /// </summary>
        public ColorTransformAmstradCPC WithCpcVideoMode(CPCVideoMode videoMode)
        { 
            this.VideoMode = videoMode;
            return this;
        }

        /// <summary>
        /// Sets the video mode from a configuration (fluent).
        /// </summary>
        public ColorTransformAmstradCPC WithCpcVideoMode(ColorTransformConfig cfg) => WithCpcVideoMode(cfg.CPCVideoMode);

       


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
            var fastReduction = ForwardEventsFrom(new ColorTransformFast());
            fastReduction.WithProcessingParams(maxColors)
                .WithColorDistanceEvaluationMode(ColorDistanceEvaluationMode);
            var result = fastReduction.CreateAndProcessColors(image, token);
            if (result == null || !result.IsSuccess)
            {
                LogMan.Error(sC, sM, $"{Type} : {maxColors} color reduction failed");
                return ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : {maxColors} color reduction failed", result);
            }

            BypassDithering = true;

            var reducedImage = result.DataOut;
            if (DitheringConfig.DitheringType != ColorDithering.None)
            {
                var imageRef = doubleRes ? ImageTools.HalveXResolution(ImageSource,  HalveResolutionMode.OddPixel) : ImageSource;
                var dithering = DitherBase.CreateDitherInterface(DitheringConfig);
                var ditheredResult = dithering.Dither(imageRef, reducedImage, ColorDistanceEvaluationMode, token);
                if (ditheredResult == null || !ditheredResult.IsSuccess)
                {
                    LogMan.Error(sC, sM, $"{Type} : dithering failed");
                    return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, reducedImage, $"{Type} : dithering failed : {ditheredResult?.Message}", ditheredResult?.Exception);
                }
                reducedImage = ditheredResult.DataOut;
            }
            var outputImage = doubleRes ? ImageTools.DoubleXResolution(reducedImage) : reducedImage;
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
                switch (VideoMode)
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
                        LogMan.Error(sC, sM, $"{Type} : unsupported video mode {VideoMode}");
                        return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : unsupported video mode {VideoMode}");
                }
            }
            catch (OperationCanceledException ex)
            {
                LogMan.Exception(sC, sM, $"{Type} : operation cancelled", ex);
                return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : operation cancelled", ex);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, $"{Type} : video mode {VideoMode} conversion failed", ex);
                return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : video mode {VideoMode} conversion failed", ex);
            }
        }
    }
}