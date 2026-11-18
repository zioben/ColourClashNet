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
    public class ColorTransformAmstradCPC : ColorTransformFixedPalette
    {

        public enum CPCVideoMode
        {
            Mode0,
            Mode1,
            Mode2,
            Mode3,
            DebugPalette
        }

        public CPCVideoMode VideoMode 
        { 
            get => config.CPCVideoMode;
            set => config.CPCVideoMode = value;
        }
        public ColorTransformAmstradCPC()
        {
            Type = ColorTransformType.ColorReductionCPC;
            Description = "Reduce color to Amstrad CPC palette";
        }

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


        public ColorTransformAmstradCPC WithCpcVideoMode(CPCVideoMode videoMode)
        { 
            this.VideoMode = videoMode;
            return this;
        }

        public ColorTransformAmstradCPC WithCpcVideoMode(ColorTransformConfig cfg) => WithCpcVideoMode(cfg.CPCVideoMode);

       


        ImageData? PreProcess(bool halveRes, CancellationToken token=default)
        {           
            var tmpData = halveRes ? ImageTools.HalveXResolution(ImageSource, HalveResolutionMode.OddPixel) : ImageSource;
            var tmpDataProcessed = TransformationMap.Transform(tmpData, token);
            return tmpDataProcessed;
        }

        ImageData? PostProcess(ImageData image, int maxColors, bool doubleRes, CancellationToken token)
        {
            var res = new ColorTransformFast()
                .WithProcessingParams(maxColors)
                .WithColorDistanceEvaluationMode(ColorDistanceEvaluationMode)
                .CreateAndProcessColors(image, token);

            BypassDithering = true;

            if (DitheringConfig.DitheringType != ColorDithering.None)
            {
                var imageRef = doubleRes ? ImageTools.HalveXResolution(ImageSource,  HalveResolutionMode.OddPixel) : ImageSource;
                var dithering = DitherBase.CreateDitherInterface(DitheringConfig);
                var ditherRes = dithering.Dither(imageRef, res.DataOut, ColorDistanceEvaluationMode, token);
                return doubleRes ? ImageTools.DoubleXResolution(ditherRes.DataOut) : ditherRes.DataOut;
            }
            else
            {
                return doubleRes ? ImageTools.DoubleXResolution(res.DataOut) : res.DataOut;
            }
        }
        

        ImageData? ToMode0( CancellationToken token)
        {
            var imagePre = PreProcess(true, token);
            var imagePost = PostProcess(imagePre, 16, true, token);
            return imagePost;
        }
        ImageData? ToMode1( CancellationToken token)
        {
            var imagePre = PreProcess(false, token);
            var imagePost = PostProcess(imagePre, 4, false, token);
            return imagePost;
        }

        ImageData? ToMode2( CancellationToken token)
        {
            var imagePre = PreProcess(false, token);
            var imagePost = PostProcess(imagePre, 2, false, token);
            return imagePost;
        }

        ImageData? ToMode3( CancellationToken token)
        {
            var imagePre = PreProcess(true, token);
            var imagePost = PostProcess(imagePre, 4, true, token);
            return imagePost;
        }

        protected override ColorTransformResult ExecuteTransform(CancellationToken token)
        {
            ImageData? ret = null;
            BypassDithering = true;

            switch (VideoMode)
            {
                case CPCVideoMode.Mode0:
                        ret= ToMode0(token );
                    break;
                case CPCVideoMode.Mode1:
                        ret = ToMode1(token);
                    break;
                case CPCVideoMode.Mode2:
                        ret = ToMode2(token);
                    break;
                case CPCVideoMode.Mode3:
                        ret = ToMode3(token);
                    break;
                case CPCVideoMode.DebugPalette:
                        ret = PreProcess(false, token);
                    break;
                default:
                    break;
            }
            if (ret != null)
            { 
                return ColorTransformResult.CreateValidResult(ImageSource, ret);
            }
            return new ColorTransformResult();
        }
    }
}