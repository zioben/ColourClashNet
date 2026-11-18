using ColourClashNet.Color;
using ColourClashNet.Color.Dithering;
using ColourClashNet.Imaging;
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
        ImageData? PostProcess(ImageData image, int maxColors, bool doubleRes, CancellationToken token)
        {
            // Keep only the colors allowed by the video mode among the mapped CPC colors
            var result = new ColorTransformFast()
                .WithProcessingParams(maxColors)
                .WithColorDistanceEvaluationMode(ColorDistanceEvaluationMode)
                .CreateAndProcessColors(image, token);

            BypassDithering = true;

            if (DitheringConfig.DitheringType != ColorDithering.None)
            {
                var imageRef = doubleRes ? ImageTools.HalveXResolution(ImageSource,  HalveResolutionMode.OddPixel) : ImageSource;
                var dithering = DitherBase.CreateDitherInterface(DitheringConfig);
                var ditheredResult = dithering.Dither(imageRef, result.DataOut, ColorDistanceEvaluationMode, token);
                return doubleRes ? ImageTools.DoubleXResolution(ditheredResult.DataOut) : ditheredResult.DataOut;
            }
            else
            {
                return doubleRes ? ImageTools.DoubleXResolution(result.DataOut) : result.DataOut;
            }
        }
        

        /// <summary>
        /// Mode 0: 16 colors, wide pixels.
        /// </summary>
        ImageData? ToMode0( CancellationToken token)
        {
            var imagePre = PreProcess(true, token);
            var imagePost = PostProcess(imagePre, 16, true, token);
            return imagePost;
        }
        /// <summary>
        /// Mode 1: 4 colors.
        /// </summary>
        ImageData? ToMode1( CancellationToken token)
        {
            var imagePre = PreProcess(false, token);
            var imagePost = PostProcess(imagePre, 4, false, token);
            return imagePost;
        }

        /// <summary>
        /// Mode 2: 2 colors.
        /// </summary>
        ImageData? ToMode2( CancellationToken token)
        {
            var imagePre = PreProcess(false, token);
            var imagePost = PostProcess(imagePre, 2, false, token);
            return imagePost;
        }

        /// <summary>
        /// Mode 3: 4 colors, wide pixels.
        /// </summary>
        ImageData? ToMode3( CancellationToken token)
        {
            var imagePre = PreProcess(true, token);
            var imagePost = PostProcess(imagePre, 4, true, token);
            return imagePost;
        }

        /// <summary>
        /// Runs the conversion for the selected video mode.
        /// </summary>
        protected override ColorTransformResult ExecuteTransform(CancellationToken token)
        {
            ImageData? outputImage = null;
            BypassDithering = true;

            switch (VideoMode)
            {
                case CPCVideoMode.Mode0:
                        outputImage= ToMode0(token );
                    break;
                case CPCVideoMode.Mode1:
                        outputImage = ToMode1(token);
                    break;
                case CPCVideoMode.Mode2:
                        outputImage = ToMode2(token);
                    break;
                case CPCVideoMode.Mode3:
                        outputImage = ToMode3(token);
                    break;
                case CPCVideoMode.DebugPalette:
                        outputImage = PreProcess(false, token);
                    break;
                default:
                    break;
            }
            if (outputImage != null)
            { 
                return ColorTransformResult.CreateValidResult(ImageSource, outputImage);
            }
            return new ColorTransformResult();
        }
    }
}