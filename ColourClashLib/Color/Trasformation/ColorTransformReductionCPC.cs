using ColourClashNet.Color.Dithering;
using ColourClashNet.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ColourClashNet.Color.Transformation.ColorTransformReductionC64;

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

        public CPCVideoMode VideoMode { get; set; } =  CPCVideoMode.Mode0;
        public ColorTransformReductionCPC()
        {
            Type = ColorTransformType.ColorReductionCBM64;
            Description = "Reduce color to Amstrad CPC palette";
            CreatePalette();
        }
        void CreatePalette()
        {
            SetProperty(
                ColorTransformProperties.Fixed_Palette,
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

        internal protected override ColorTransformInterface SetProperty(ColorTransformProperties propertyName, object value)
        {
            base.SetProperty(propertyName, value);
            switch (propertyName)
            {
                case ColorTransformProperties.CPC_VideoMode:
                        VideoMode = ToEnum<CPCVideoMode>(value);
                    break;
                default:
                    break;
            }
            return this;
        }

      
        ImageData? PreProcess(bool bHalveRes, CancellationToken oToken=default)
        {           
            var oTmpData = bHalveRes ? ImageTools.HalveXResolution(SourceData) : SourceData;
            var oTmpDataProc = TransformationMap.Transform(oTmpData, oToken);
            return oTmpDataProc;
        }

        ImageData? PostProcess(ImageData image, int iMaxColors, bool bDoubleRes, CancellationToken oToken)
        {
            var oRes = new ColorTransformReductionFast()
                .SetProperty(ColorTransformProperties.MaxColorsWanted, iMaxColors)
                .SetProperty(ColorTransformProperties.ColorDistanceEvaluationMode, ColorDistanceEvaluationMode)
                .CreateAndProcessColors(image, oToken);

            BypassDithering = true;

            if (DitheringType != ColorDithering.None)
            {
                var imageRef = bDoubleRes ? ImageTools.HalveXResolution(SourceData) : SourceData;
                var oDithering = DitherBase.CreateDitherInterface(DitheringType, DitheringStrength);
                var imageDither = oDithering.Dither(imageRef, oRes.DataOut, ColorDistanceEvaluationMode, oToken);
                return bDoubleRes ? ImageTools.DoubleXResolution(imageDither) : imageDither;
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

        protected override ColorTransformResults ExecuteTransform(CancellationToken oToken)
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
                return ColorTransformResults.CreateValidResult(SourceData, ret);
            }
            return new ColorTransformResults();
        }
    }
}