using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color
{
    /// <summary>
    /// Color quantization mode: number of bits kept for each red, green and blue channel.
    /// <para/>
    /// Lower bit depths emulate the color space (selectable colors) of retro hardware,
    /// e.g. <see cref="RGB444"/> for Amiga OCS/ECS or <see cref="RGB333"/> for Atari ST.
    /// </summary>
    [Serializable]
    public enum ColorQuantizationMode
    {
        /// <summary>8-8-8 bits: 16.7M colors (no quantization).</summary>
        RGB888,
        /// <summary>6-6-6 bits: 262,144 colors (VGA, Amiga HAM8).</summary>
        RGB666,
        /// <summary>5-6-5 bits: 65,536 colors (16 bit high color).</summary>
        RGB565,
        /// <summary>5-5-5 bits: 32,768 colors (15 bit high color).</summary>
        RGB555,
        /// <summary>4-4-4 bits: 4,096 colors (Amiga OCS/ECS, Atari STE).</summary>
        RGB444,
        /// <summary>3-3-3 bits: 512 colors (Atari ST, Sega Genesis).</summary>
        RGB333,
        /// <summary>2-2-2 bits: 64 colors (EGA).</summary>
        RGB222,
        /// <summary>1-1-1 bits: 8 colors.</summary>
        RGB111,
    }

    /// <summary>
    /// Color space used to represent a color.
    /// </summary>
    [Serializable]
    public enum ColorMode
    {
        RGB,
        HSV,
        LAB,
        GRAY,
    }


    /// <summary>
    /// Metric used to evaluate the distance between two colors.
    /// </summary>
    [Serializable]
    public enum ColorDistanceEvaluationMode
    {
        /// <summary>Squared Euclidean distance in RGB space.</summary>
        RGB,
        /// <summary>Squared RGB distance plus cross-channel terms that penalize hue shifts.</summary>
        RGBalt,
        /// <summary>Distance in HSV space.</summary>
        HSV,
        /// <summary>Euclidean distance in CIE L*a*b* space.</summary>
        LAB,
        /// <summary>Difference of the gray (luminance) values.</summary>
        GRAY
    }

    /// <summary>
    /// Color transformation (color reduction) algorithm.
    /// </summary>
    [Serializable]
    public enum ColorTransformType
    {
        /// <summary>No transformation.</summary>
        None = 0,
        /// <summary>Background color removal / replacement.</summary>
        ColorRemover = 1,
        /// <summary>Output equals input.</summary>
        ColorIdentity = 2,
        /// <summary>Channel bit-depth reduction.</summary>
        ColorReductionQuantization,
        /// <summary>Keeps the most frequent colors.</summary>
        ColorReductionFast,
        /// <summary>K-means like clustering.</summary>
        ColorReductionClustering,
        /// <summary>Amiga Hold-And-Modify modes.</summary>
        ColorReductionHam,
        /// <summary>Palette recomputed per scanline (Amiga copper style).</summary>
        ColorReductionScanline,
        /// <summary>Mapping to a user supplied fixed palette.</summary>
        ColorReductionGenericPalette,
        /// <summary>ZX Spectrum attribute mode.</summary>
        ColorReductionZxSpectrum,
        /// <summary>Commodore 64 video modes.</summary>
        ColorReductionCBM64,
        /// <summary>Amstrad CPC video modes.</summary>
        ColorReductionCPC,
        /// <summary>Median cut palette reduction.</summary>
        ColorReductionMedianCut,
        /// <summary>Hue / saturation / brightness adjustment.</summary>
        ColorReductionSaturation,
        /// <summary>Fixed palette refined by clustering.</summary>
        ColorReductionEnhancedPalette,

        /// <summary>IBM CGA video modes.</summary>
        ColorReductionCGA,
        /// <summary>Hercules monochrome mode.</summary>
        ColorReductionHercules,
        /// <summary>IBM EGA palette.</summary>
        ColorReductionEGA,
        /// <summary>IBM VGA palette.</summary>
        ColorReductionVGA,

    }

    /// <summary>
    /// Dithering algorithm.
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

    /// <summary>
    /// Post-dithering effect: restores every other row or column from the undithered image,
    /// producing a scanline-like blend between the dithered and the non-dithered result.
    /// </summary>
    [Serializable]
    public enum ColorDitheringFx
    {
        /// <summary>No effect.</summary>
        None = 0,
        /// <summary>Restore even rows (0, 2, ...) from the undithered image.</summary>
        ScanlineOdd,
        /// <summary>Restore odd rows (1, 3, ...) from the undithered image.</summary>
        ScanlineEven,
        /// <summary>Restore even columns from the undithered image.</summary>
        ColumnOdd,
        /// <summary>Restore odd columns from the undithered image.</summary>
        ColumnEven
    }

    /// <summary>
    /// Selects how the representative color of a cluster is calculated.
    /// </summary>
    [Serializable]
    public enum ColorSelectionMode
    {
        /// <summary>Mean of the colors in the cluster (may produce a new color).</summary>
        EvaluateColorMean,
        /// <summary>Most representative color already present in the cluster palette.</summary>
        UseColorPalette
    }


    /// <summary>
    /// Flags stored in the most significant byte of a packed color integer.
    /// <para/>
    /// The lower 24 bits hold the RGB888 color; the upper 8 bits describe the nature of the value.
    /// Every value that is not a "real" color has the MSB set, so it is always a negative number:
    /// a simple <c>value &lt; 0</c> test filters it out during processing.
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
        /// The color encodes a cookie-cut mask and must be handled separately during processing
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

    /// <summary>
    /// Strategy used to merge two adjacent pixels when halving the horizontal resolution.
    /// </summary>
    public enum HalveResolutionMode
    {
        /// <summary>Keep the first pixel of each pair (columns 0, 2, ...).</summary>
        OddPixel = 0,
        /// <summary>Keep the second pixel of each pair (columns 1, 3, ...).</summary>
        EvenPixel,
        /// <summary>Use the mean color of the pair.</summary>
        MeanColor,

    }
}
