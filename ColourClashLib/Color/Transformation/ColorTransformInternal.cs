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


        public static ColorTransformInterface Alloc(ColorTransformConfig cfg)
        {
            string sM = nameof(ColorTransformInternal);
            if(cfg == null) 
            { 
                throw new ArgumentNullException($"{sC}.{sM} : {nameof(cfg)}");
            }
            ColorTransformInterface trans;
            switch (cfg.InternalTransformationModel)
            {
                case ColorTransformType.ColorReductionClustering:
                    {
                        trans = new ColorTransformReductionCluster();
                    }
                    break;
                case ColorTransformType.ColorReductionFast:
                    {
                        trans = new ColorTransformReductionFast();
                    }
                    break;
                case ColorTransformType.ColorReductionMedianCut:
                    {
                        trans = new ColorTransformReductionMedianCut();
                    }
                    break;
                case ColorTransformType.ColorReductionGenericPalette:
                    {
                        trans = new ColorTransformReductionPalette();
                    }
                    break;
                default:
                    throw new ArgumentException($"{sC}.{sM} : {cfg.InternalTransformationModel} not supported");
            }
            return trans.SetProperties(cfg);
        }

        public static List<string> GetInternalTransformationList()
        {
            return new List<string>()
            {
                ColorTransformType.ColorReductionClustering.ToString(),
                ColorTransformType.ColorReductionFast.ToString(),
                ColorTransformType.ColorReductionMedianCut.ToString(),
                ColorTransformType.ColorReductionGenericPalette.ToString()
            };
        }
    }
}
