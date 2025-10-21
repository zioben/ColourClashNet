using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ColourClashNet.Color.Transformation.ColorTransformReductionZxSpectrum;

namespace ColourClashNet.Color
{
    /// <summary>
    /// Enums to select color quantization mode 
    /// </summary>
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

    /// <summary>
    /// Enums to select color space for color distance evaluation
    /// </summary>
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

    /// <summary>
    /// Enums to select dithering algorithm 
    /// </summary>
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

    /// <summary>
    /// Enums to select how to calculate the mean color of a cluster 
    /// </summary>
    public enum ColorMeanMode
    {
        UseMean,
        UseColorPalette
    }

    /// <summary>
    /// Enums to decorate Parameter Class 
    /// </summary>
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
        C64_VideoMode,
        //ClusterColorsMax,
        ClusterTrainingLoop,
        CPC_VideoMode,
        Amiga_VideoMode,
        Amiga_HamColorReductionMode,
        UseColorMean,
        UseFixedPalette,
        UseClustering,
        Zx_ColL,
        Zx_ColH,
        Zx_PaletteMode,
        Zx_IncludeBlackInHighColor,
        Zx_DitherHighColor,
        Fixed_Palette,
        Dithering_Model
    }

    /// <summary>
    /// Enums to decorate ColorIntExt Class
    /// <para>
    /// 24 bit space is neede to codigy RGB8 colour space data. Remaining 8 bit data can be used to identify a property of the color.<br/>
    /// Everything tha in nor "real" color is maked with 1 on MSB, so resulting in always a negative number. This Helps on filtering operations.
    /// </para>
    /// </summary>
    public enum ColorIntType
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
}
