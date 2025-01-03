using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ColourClashNet.Colors.Transformation.ColorTransformReductionZxSpectrum;

namespace ColourClashNet.Colors
{
    public enum ColorQuantizationMode
    {
        Unknown = 0,
        RGB888,
        RGB666,
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
        RGB,
        RGBalt,
        HSV,
    }

    public enum ColorTransformType
    {
        None = 0,
        ColorRemover = 1,
        ColorIdentity = 2,
        ColorReductionQuantization,
        ColorReductionFast,
        ColorReductionClustering,
        ColorReductionHam,
        ColorReductionScanline,
        ColorReductionGenericPalette,
        ColorReductionEga,
        ColorReductionZxSpectrum,
        ColorReductionCBM64,
        ColorReductionCPC,
        ColorReductionMedianCut,
        ColorReductionSaturation,
        ColorReductionTileBase,
    }

    public enum ColorDithering
    {
        None = 0,
        Ordered_2x2,
        Ordered_4x4,
        Ordered_6x6,
        Ordered_8x8,
        Ordered,
        FloydSteinberg,
        Atkinson,
        Burkes,
        JarvisJudiceNinke,
        Sierra,
        Stucki,
        ScanLine
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

    public enum ColorTransformProperties
    {
        Unknown = 0,
        ColorDistanceEvaluationMode,
        ColorBackgroundList,
        ColorBackgroundReplacement,
        MaxColorsWanted,
        HsbHueShift,
        HsvSaturationMultFactor,
        HsvBrightnessMultFactor,
        QuantizationMode,
        C64VideoMode,
        ClusterColorsMax,
        ClusterTrainingLoop,
        CPCVideoMode,
        AmigaVideoMode,
        AmigaHamColorReductionMode,
        UseColorMean,
        ScanlineUseClustering,
        ZxColL,
        ZxColH,
        ZxPaletteMode,
        ZxIncludeBlackInHighColor,
        ZxDitherHighColor
    }

}
