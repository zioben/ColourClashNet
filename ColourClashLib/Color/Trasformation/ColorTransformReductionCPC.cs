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

        protected override ColorTransformInterface SetProperty(ColorTransformProperties propertyName, object value)
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
            var oTmpData = SourceData;
            if (bHalveRes)
            {
                oTmpData = new ImageData().Create(ColorTransformBase.HalveHorizontalRes(SourceData.DataX));
            }
            var oTmpDataProc = TransformationMap.Transform(oTmpData, oToken);
            return oTmpDataProc;
        }

        ImageData? PostProcessAsync(ImageData oPreprocessData, int iMaxColors, bool bDoubleRes, CancellationToken oToken)
        {
            var oRes = new ColorTransformReductionFast()
                .SetProperty(ColorTransformProperties.MaxColorsWanted, iMaxColors)
                .SetProperty(ColorTransformProperties.ColorDistanceEvaluationMode, ColorDistanceEvaluationMode)
                .CreateAndProcessColors(oPreprocessData, oToken);

            BypassDithering = true;

            if (DitheringType != ColorDithering.None)
            {
                var oRealSource = SourceData;
                if (bDoubleRes)
                {
                    oRealSource = new ImageData().Create( ColorTransformBase.HalveHorizontalRes(SourceData.DataX));
                }
                var oDithering = DitherBase.CreateDitherInterface(DitheringType, DitheringStrength);
                var oDitheringOut = oDithering.Dither(oRealSource, oRes.DataOut, ColorDistanceEvaluationMode, oToken);
                if (bDoubleRes)
                {
                    return new ImageData().Create( DoubleHorizontalRes(oDitheringOut.DataX));
                }
                return oDitheringOut;
            }
            if (bDoubleRes)
            {
                return new ImageData().Create(DoubleHorizontalRes(oRes.DataOut.DataX));
            }
            return oRes.DataOut;
        }
        




        ImageData? ToMode0( CancellationToken oToken)
        {
            BypassDithering = true;
            var oTmp1 = PreProcess(true, oToken);
            var oTmp2 = PostProcessAsync(oTmp1, 16, true, oToken);
            return oTmp2;
        }
        ImageData? ToMode1( CancellationToken oToken)
        {
            BypassDithering = true;
            var oTmp1 = PreProcess(false, oToken);
            var oTmp2 = PostProcessAsync(oTmp1, 4, false, oToken);
            return oTmp2;
        }

        ImageData? ToMode2( CancellationToken oToken)
        {
            BypassDithering = true;
            var oTmp1 = PreProcess(false, oToken);
            var oTmp2 = PostProcessAsync(oTmp1, 2, false, oToken);
            return oTmp2;
        }

        ImageData? ToMode3( CancellationToken oToken)
        {
            BypassDithering = true;
            var oTmp1 = PreProcess(true, oToken);
            var oTmp2 = PostProcessAsync(oTmp1, 4, true, oToken);
            return oTmp2;
        }

        protected override ColorTransformResults ExecuteTransform(CancellationToken oToken)
        {
            ImageData? ret = null;
            switch (VideoMode)
            {
                case CPCVideoMode.Mode0:
                    {                       
                        ret= ToMode0(oToken );
                    }
                    break;
                case CPCVideoMode.Mode1:
                    {
                        ret = ToMode1(oToken);
                    }
                    break;
                case CPCVideoMode.Mode2:
                    {
                        ret = ToMode2(oToken);
                    }
                    break;
                case CPCVideoMode.Mode3:
                    {
                        ret = ToMode3(oToken);
                    }
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