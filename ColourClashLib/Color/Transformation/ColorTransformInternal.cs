using ColourClashNet.Color.Dithering;
using ColourClashNet.Color.Transformation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColourClashNet.Color.Transformation
{
    public static class ColorTransformInternal
    {
        static string sC = nameof(ColorTransformInternal);

        public static ColorTransformInterface Create(ColorTrasformInternalModel internalTransformModel, DitherConfig ditherConfig,int maxColors,bool useColorMean)
        {
            string sM = nameof(ColorTransformInternal);
            ColorTransformInterface trans;
            switch (internalTransformModel)
            {
                case ColourClashNet.Color.ColorTrasformInternalModel.ColorReductionClustering:
                    {
                        trans = new ColorTransformReductionCluster().WithProcessingParams(maxColors,10,useColorMean).WithDithering(ditherConfig);
                        
                    }
                    break;
                case ColourClashNet.Color.ColorTrasformInternalModel.ColorReductionFast:
                    {
                        trans = new ColorTransformReductionFast().WithProcessingParams(maxColors).WithDithering(ditherConfig);
                    }
                    break;
                case ColourClashNet.Color.ColorTrasformInternalModel.ColorReductionMedianCut:
                    {
                        trans = new ColorTransformReductionMedianCut().WithProcessingParams(maxColors,useColorMean).WithDithering(ditherConfig);
                    }
                    break;
                default:
                    throw new ArgumentException($"{sC}.{sM} : {internalTransformModel} not supported");
            }
            return trans;
        }

    

        public static ColorTransformInterface Create(ColorTransformConfig cfg)
        {
            string sM = nameof(ColorTransformInternal);
            if (cfg == null)
                throw new ArgumentNullException($"{sC}.{sM} : {nameof(cfg)} null");
            ColorTransformInterface trans = Create(cfg.InternalTransformationModel, cfg.DitheringCfg, cfg.MaxColorsWanted, cfg.UseColorMean);
            //trans.SetProperties(cfg);
            return trans;
        }

    }
}
