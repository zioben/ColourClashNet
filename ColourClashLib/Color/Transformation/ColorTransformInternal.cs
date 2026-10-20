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


        public static ColorTransformInterface Alloc(ColorTransformConfig cfg, ColorTransformType type)
        {
            string sM = nameof(ColorTransformInternal);
            if(cfg == null) 
            { 
                throw new ArgumentNullException($"{sC}.{sM} : {nameof(cfg)}");
            }
            ColorTransformInterface trans;
            switch (type)
            {
                case ColorTransformType.ColorReductionClustering:
                    {
                        trans = new ColorTransformClustering();
                    }
                    break;
                case ColorTransformType.ColorReductionFast:
                    {
                        trans = new ColorTransformFast();
                    }
                    break;
                case ColorTransformType.ColorReductionMedianCut:
                    {
                        trans = new ColorTransformMedianCut();
                    }
                    break;
                case ColorTransformType.ColorReductionGenericPalette:
                    {
                        trans = new ColorTransformFixedPalette();
                    }
                    break;
                default:
                    throw new ArgumentException($"{sC}.{sM} : {type} not supported");
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
