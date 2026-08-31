using ColourClashNet.Color.Dithering;
using ColourClashNet.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformReductionCPC : ColorTransformReductionPalette
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
        public ColorTransformReductionCPC()
        {
            Type = ColorTransformType.ColorReductionCBM64;
            Description = "Reduce color to Amstrad CPC palette";
            CreatePalette();
        }
        void CreatePalette()
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
                }
            );
        }


        public ColorTransformReductionCPC WithCpcVideoMode(CPCVideoMode videoMode)
        { 
            this.VideoMode = videoMode;
            return this;
        }

        public ColorTransformReductionCPC WithCpcVideoMode(ColorTransformConfig cfg) => WithCpcVideoMode(cfg.CPCVideoMode);

       


            ImageData? PreProcess(bool bHalveRes, CancellationToken oToken=default)
        {           
            var oTmpData = bHalveRes ? ImageTools.HalveXResolution(ImageSource, HalveResolutionMode.OddPixel) : ImageSource;
            var oTmpDataProc = TransformationMap.Transform(oTmpData, oToken);
            return oTmpDataProc;
        }

        ImageData? PostProcess(ImageData image, int iMaxColors, bool bDoubleRes, CancellationToken oToken)
        {
            var oRes = new ColorTransformReductionFast()
                .WithProcessingParams(iMaxColors)
                .WithColorDistanceEvaluationMode(ColorDistanceEvaluationMode)
                .CreateAndProcessColors(image, oToken);

            BypassDithering = true;

            if (DitheringConfig.DitheringType != ColorDithering.None)
            {
                var imageRef = bDoubleRes ? ImageTools.HalveXResolution(ImageSource,  HalveResolutionMode.OddPixel) : ImageSource;
                var dithering = DitherBase.CreateDitherInterface(DitheringConfig);
                var ditherRes = dithering.Dither(imageRef, oRes.DataOut, ColorDistanceEvaluationMode, oToken);
                return bDoubleRes ? ImageTools.DoubleXResolution(ditherRes.DataOut) : ditherRes.DataOut;
            }
            else
            {
                return bDoubleRes ? ImageTools.DoubleXResolution(oRes.DataOut) : oRes.DataOut;
            }
        }
        

        ImageData? ToMode0( CancellationToken oToken)
        {
            var imagePre = PreProcess(true, oToken);
            var imagePost = PostProcess(imagePre, 16, true, oToken);
            return imagePost;
        }
        ImageData? ToMode1( CancellationToken oToken)
        {
            var imagePre = PreProcess(false, oToken);
            var imagePost = PostProcess(imagePre, 4, false, oToken);
            return imagePost;
        }

        ImageData? ToMode2( CancellationToken oToken)
        {
            var imagePre = PreProcess(false, oToken);
            var imagePost = PostProcess(imagePre, 2, false, oToken);
            return imagePost;
        }

        ImageData? ToMode3( CancellationToken oToken)
        {
            var imagePre = PreProcess(true, oToken);
            var imagePost = PostProcess(imagePre, 4, true, oToken);
            return imagePost;
        }

        protected override ColorTransformResult ExecuteTransform(CancellationToken oToken)
        {
            ImageData? ret = null;
            BypassDithering = true;

            switch (VideoMode)
            {
                case CPCVideoMode.Mode0:
                        ret= ToMode0(oToken );
                    break;
                case CPCVideoMode.Mode1:
                        ret = ToMode1(oToken);
                    break;
                case CPCVideoMode.Mode2:
                        ret = ToMode2(oToken);
                    break;
                case CPCVideoMode.Mode3:
                        ret = ToMode3(oToken);
                    break;
                case CPCVideoMode.DebugPalette:
                        ret = PreProcess(false, oToken);
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