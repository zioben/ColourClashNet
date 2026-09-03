using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color
{
    /// <summary>
    /// Enums to select color quantization mode 
    /// </summary>
    [Serializable]
    public enum ColorQuantizationMode
    {
        RGB888,
        RGB666,
        RGB565,
        RGB555,
        RGB444,
        RGB333,
        RGB222,
        RGB111,
    }

    /// <summary>
    /// Enums to select color space for color distance evaluation
    /// </summary>
    [Serializable]
    public enum Colorspace
    {
        RGB,
        HSV,
        LAB,
        XYZ,
    }


    /// <summary>
    /// Enums to select color distance evaluation algorithm
    /// </summary>
    [Serializable]
    public enum ColorDistanceEvaluationMode
    {
        RGB,
        RGBalt,
        HSV,
        LAB,
        GRAY
    }

    /// <summary>
    /// Enums to select color transformation algorithm 
    /// </summary>
    [Serializable]
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
        ColorReductionEGA,
        ColorReductionZxSpectrum,
        ColorReductionCBM64,
        ColorReductionCPC,
        ColorReductionMedianCut,
        ColorReductionSaturation,
        ColorReductionEnhancedPalette,

        ColorReductionCGA,
    }

    /// <summary>
    /// Enums to select dithering algorithm 
    /// </summary>
    [Serializable]
    public enum ColorDithering
    {
        None = 0,
        Ordered_2x2,
        Ordered_4x4,
        Ordered_6x6,
        Ordered_8x8,
        FloydSteinberg,
        Atkinson,
        Burkes,
        JarvisJudiceNinke,
        Sierra,
        Stucki,
        ScanLine,
    }

    [Serializable]
    public enum ColorDitheringFx
    {
        None = 0,
        ScanlineOdd,
        ScanlineEven,
        ColumnOdd,
        ColumnEven
    }

    /// <summary>
    /// Enums to select how to calculate the mean color of a cluster 
    /// </summary>
    [Serializable]
    public enum ColorSelectionMode
    {
        EvaluateColorMean,
        UseColorPalette
    }

    /// <summary>
    /// Enums to decorate Parameter Class 
    /// </summary>
    [Serializable]
    public enum Parameters
    {
        Unknown = 0,
        ColorQuantizationMode,
        ColorDistanceMode,
        PaletteEntries,
        DitherMode,
    }

    /// <summary>
    /// Enums to decorate ColorTransformBase Class
    /// </summary>
    [Serializable]
    public enum ColorTransformProperties
    {
        Unknown = 0,
        ColorDistanceEvaluationMode,
        ColorBackgroundList,
        ColorBackgroundReplacement,
        MaxColorsWanted,
        MaxColorChangePerLine,
        HsvHueShift,
        HsvSaturationMultFactor,
        HsvBrightnessMultFactor,
        QuantizationMode,
        C64VideoMode,
        ClusterTrainingLoop,
        CPCVideoMode,
        AmigaVideoMode,
        UseColorMean,
        UseFixedPalette,
        UseSharedPalette,
        UseClustering,
        ZxColLSeed,
        ZxColHSeed,
        ZxPaletteMode,
        ZxIncludeBlackInHighColorImage,
        ZxDitherLowColorImage,
        ZxDitherHighColorImage,
        ZxAutotuneMode,
        PriorityPalette,
        DitheringType,
        DitheringStrength,
        DitheringFx,
    }


    /// <summary>
    /// Enums to decorate ColorIntExt Class
    /// <para>
    /// 24 bit space is neede to codigy RGB8 Color space data. Remaining 8 bit data can be used to identify a property of the color.<br/>
    /// Everything tha in nor "real" color is maked with 1 on MSB, so resulting in always a negative number. This Helps on filtering operations.
    /// </para>
    /// </summary>
    [Serializable]
    public enum ColorInfo
    {
        /// <summary>
        /// Real color Flag
        /// </summary>
        IsColor = 0,
        ///// <summary>
        ///// The color in part of the background, and should be processed apart
        ///// </summary>
        //IsBkg = 0b10000001,
        /// <summary>
        /// The color codifies a cookiecut image, and should be treated apart on image processing
        /// </summary>
        IsMask = 0b10000010,
        /// <summary>
        /// The color represents an alpha value
        /// </summary>
        IsAplha = 0b10000100,
        /// <summary>
        /// The color represents a tile grid, useful to align graphics
        /// </summary>
        IsTile = 0b10001000,
        /// <summary>
        /// The color should be treated as transparent
        /// </summary>
        IsTransparent = 0b10010000,
        /// <summary>
        /// The color should be considered invalid
        /// </summary>
        Invalid = 0b11111111,
    }

    public enum HalveResolutionMode
    {
        OddPixel = 0,
        EvenPixel,
        MeanColor,

    }
}
