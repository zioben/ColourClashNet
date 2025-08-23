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
        MaxColorChangePerLine,
        HsbHueShift,
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
        Output_Palette,
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
        /// <summary>
        /// The color in part of the background, and should be processed apart
        /// </summary>
        IsBkg = (0b10000001) << 24,
        /// <summary>
        /// The color codifies a cookiecut image, and should be treated apart on image processing
        /// </summary>
        IsMask = (0b10000010) << 24,
        /// <summary>
        /// The color represents an alpha value
        /// </summary>
        IsAplha = (0b10000100) << 24,
        /// <summary>
        /// The color represents a tile grid, useful to align graphics
        /// </summary>
        IsTile = (0b10001000) << 24,
        /// <summary>
        /// The color should be treated as transparent
        /// </summary>
        IsTransparent = (0b10010000) << 24,
        /// <summary>
        /// The color should be considered invalid
        /// </summary>
        Invalid = (0b11111111) << 24,
    }
}
