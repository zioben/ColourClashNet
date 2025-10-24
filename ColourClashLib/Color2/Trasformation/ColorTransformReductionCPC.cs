using ColourClashLib.Color.Trasformation;
using ColourClashNet.Color.Dithering;
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

        public override ColorTransformInterface SetProperty(ColorTransformProperties eProperty, object oValue)
        {
            base.SetProperty(eProperty, oValue);
            switch (eProperty)
            {
                case ColorTransformProperties.CPC_VideoMode:
                    if (Enum.TryParse<CPCVideoMode>(oValue?.ToString(), out var evm))
                    {
                        VideoMode = evm;
                        return this;
                    }
                    break;
                default:
                    break;
            }
            return null;
        }


        async Task<int[,]?> PreProcessAsync(bool bHalveRes, CancellationToken? oToken)
        {
            var oTmpData = SourceData;
            if (bHalveRes)
            {
                oTmpData = ColorTransformBase.HalveHorizontalRes(SourceData);
            }
            var oTmpDataProc = await TransformationMap.TransformAsync(oTmpData, oToken);
            return oTmpDataProc;
        }

        async Task<int[,]?> PostProcessAsync(int[,] oPreprocessData, int iMaxColors, bool bDoubleRes, CancellationToken? oToken)
        {
            //ColorTransformReductionCluster oTrasf = new();
            //oTrasf.SetProperty(ColorTransformProperties.MaxColorsWanted, iMaxColors)
            //     .SetProperty(ColorTransformProperties.ColorDistanceEvaluationMode, this.ColorDistanceEvaluationMode)
            //     .SetProperty(ColorTransformProperties.Dithering_Model, this.DitheringType)
            //     .SetProperty(ColorTransformProperties.ClusterTrainingLoop, 5)
            //     .SetProperty(ColorTransformProperties.UseColorMean, false);
            ColorTransformReductionFast oTrasf= new ColorTransformReductionFast();
            oTrasf.SetProperty(ColorTransformProperties.MaxColorsWanted, iMaxColors);
            oTrasf.SetProperty(ColorTransformProperties.ColorDistanceEvaluationMode, ColorDistanceEvaluationMode);
            await oTrasf.CreateAsync(oPreprocessData, oToken);
            var ret = await oTrasf.ProcessColorsAsync(oToken);
            DataContainer oContainer = new();
            await oContainer.SetDataAsync(ret.DataOut, oToken);
            BypassDithering = true;

            if (DitheringType != ColorDithering.None)
            {
                var oTmpData = SourceData;
                if (bDoubleRes)
                {
                    oTmpData = ColorTransformBase.HalveHorizontalRes(SourceData);
                }
                var oDithering = DitherBase.CreateDitherInterface(DitheringType, DitheringStrenght);
                var oDitheringOut = await oDithering.DitherAsync(oTmpData, oContainer.Data, oContainer.ColorPalette, ColorDistanceEvaluationMode, oToken);
                if (bDoubleRes)
                {
                    return DoubleHorizontalRes(oDitheringOut);
                }
                return oDitheringOut;
            }
            if (bDoubleRes)
            {
                return DoubleHorizontalRes(oContainer.Data);
            }
            return oContainer.Data;
        }
        




        async Task<int[,]?> ToMode0Async( CancellationToken? oToken)
        {
            BypassDithering = true;
            var oTmp1 = await PreProcessAsync( true, oToken);
            var oTmp2 = await PostProcessAsync(oTmp1, 16, true, oToken);
            return oTmp2;
        }
        async Task<int[,]?> ToMode1Async( CancellationToken? oToken)
        {
            BypassDithering = true;
            var oTmp1 = await PreProcessAsync(false, oToken);
            var oTmp2 = await PostProcessAsync(oTmp1, 4, false, oToken);
            return oTmp2;
        }

        async Task<int[,]?> ToMode2Async( CancellationToken? oToken)
        {
            BypassDithering = true;
            var oTmp1 = await PreProcessAsync(false, oToken);
            var oTmp2 = await PostProcessAsync(oTmp1, 2, false, oToken);
            return oTmp2;
        }

        async Task<int[,]?> ToMode3Async( CancellationToken? oToken)
        {
            BypassDithering = true;
            var oTmp1 = await PreProcessAsync(true, oToken);
            var oTmp2 = await PostProcessAsync(oTmp1, 4, true, oToken);
            return oTmp2;
        }

        protected async override Task<ColorTransformResults> ExecuteTransformAsync(CancellationToken? oToken)
        {
            int[,]? ret = null;
            switch (VideoMode)
            {
                case CPCVideoMode.Mode0:
                    {                       
                        ret= await ToMode0Async(oToken );
                    }
                    break;
                case CPCVideoMode.Mode1:
                    {
                        ret = await ToMode1Async(oToken);
                    }
                    break;
                case CPCVideoMode.Mode2:
                    {
                        ret = await ToMode2Async(oToken);
                    }
                    break;
                case CPCVideoMode.Mode3:
                    {
                        ret = await ToMode3Async(oToken);
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