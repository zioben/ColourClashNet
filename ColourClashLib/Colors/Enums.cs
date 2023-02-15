using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public enum ColorQuantizationMode
    {
        Unknown = 0,
        RGB888,
        RGB565,
        RGB555,
        RGB444,
        RGB333,
        RGB222,
        RGB111,
    }

    public enum Colorspace
    {
        RGB,
        HSV,
        LAB,
        XYZ,
    }

    public enum ColorDistanceEvaluationMode
    {
        All,
        RGB,
        HSV,
    }

    public enum ColorTransform
    {
        None = 0,
        ColorRemover = 1,
        ColorIdentity = 2,
        ColorReductionQuantization,
        ColorReductionFast,
        ColorReductionClustering,
        ColorReductionScanline,
        ColorReductionEga,
        ColorReductionZxSpectrum,
        ColorReductionCBM64,
        ColorReductionMedianCut,
        ColorReductionSaturation,
    }

    public enum ColorDithering
    {
        None=0,
        Ordered_2x2,
        Ordered_4x4,
        Ordered_8x8,
        FloydSteinberg,
        Atkinson,
        Burkes,
        JarvisJudiceNinke,
        Sierra,
        Stucki,
        Linear
    }
    public enum ColorMeanMode
    {
        UseMean,
        UseColorPalette
    }

    public enum Parameters
    {
        Unknown = 0,
        ColorQuantizationMode,
        ColorDistanceMode,
        PaletteEntries,
        DitherMode,
    }

}
