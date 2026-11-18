using ColourClashNet.Color.Conversion;
using ColourClashNet.Imaging;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color
{


    /// <summary>
    /// Extension methods for packed integer colors.
    /// <para/>
    /// A color is a 32 bit integer: the lower 24 bits hold the RGB888 components (0xRRGGBB),
    /// the upper 8 bits hold a <see cref="ColorInfo"/> flag. Non-color values are negative.
    /// </summary>
    public static class ColorIntExt
    {
        /// <summary>
        /// Sets the color information type in the integer representation of a color.
        /// <para>
        /// The method modifies the upper 8 bits of the integer to encode the specified <see cref="ColorInfo"/>.
        /// </para>
        /// </summary>
        /// <param name="rgb">Color data</param>
        /// <param name="colorInfo">Color type</param>
        /// <returns>The color with the new type flag.</returns>
        public static int SetColorInfo(this int rgb, ColorInfo colorInfo)
        {
            int newrgb = rgb & 0x00_FF_FF_FF;
            newrgb |= ((int)colorInfo) << 24;
            return newrgb;
        }

        /// <summary>
        /// Gets the color information type from the integer representation of a color.
        /// </summary>
        /// <param name="rgb">Color data</param>
        /// <returns>The <see cref="ColorInfo"/> stored in the upper 8 bits.</returns>
        public static ColorInfo GetColorInfo(this int rgb)
        {
            int val = (rgb >> 24) & 0x00_00_00_FF;
            // Legacy encoding: 0xFF in the upper byte always means invalid
            if (val == 0x_00_ff)
            {
                return ColorInfo.Invalid;
            }
            return (ColorInfo)val;
        }


        /// <summary>
        /// Returns true when the value is a real color (<see cref="ColorInfo.IsColor"/>).
        /// </summary>
        public static bool IsColor(this int rgb) => GetColorInfo(rgb) == ColorInfo.IsColor;

        /// <summary>
        /// Returns true when the value is flagged as <see cref="ColorInfo.Invalid"/>.
        /// </summary>
        public static bool IsInvalid(this int rgb) => GetColorInfo(rgb) == ColorInfo.Invalid;


        /// <summary>
        /// Extracts the red component from an integer representation of a color.
        /// </summary>
        /// <param name="rgb">Color data</param>
        /// <returns>Red value in [0,255], or -1 when <paramref name="rgb"/> is not a color.</returns>
        public static int ToR(this int rgb)
        {
            return rgb >= 0 ? (rgb >> 16) & 0x00ff : -1;
        }

        /// <summary>
        /// Extracts the green component from an integer representation of a color.
        /// </summary>
        /// <param name="rgb">Color data</param>
        /// <returns>Green value in [0,255], or -1 when <paramref name="rgb"/> is not a color.</returns>
        public static int ToG(this int rgb)
        {
            return rgb >= 0 ? (rgb >> 8) & 0x00ff : -1;
        }

        /// <summary>
        /// Extracts the blue component from an integer representation of a color.
        /// </summary>
        /// <param name="rgb">Color data</param>
        /// <returns>Blue value in [0,255], or -1 when <paramref name="rgb"/> is not a color.</returns>
        public static int ToB(this int rgb)
        {
            return rgb >= 0 ? (rgb >> 0) & 0x00ff : -1;
        }

        /// <summary>
        /// Converts an integer color to its HSV representation.
        /// </summary>
        public static HSV ToHSV(this int rgb) => HSV.CreateFromIntRGB(rgb);

        /// <summary>
        /// Converts an integer color to its CIE L*a*b* representation.
        /// </summary>
        public static LAB ToLAB(this int rgb) => LAB.CreateFromIntRGB(rgb);

        /// <summary>
        /// Converts an integer color to its gray (luminance) representation.
        /// </summary>
        public static GRAY ToGray(this int rgb) => GRAY.CreateFromIntRGB(rgb);


        /// <summary>
        /// Calculates the distance between two colors using the given metric.
        /// <para/>
        /// RGB modes return squared distances (no square root) to keep the evaluation fast:
        /// the values can be compared but are not on the same scale as the other modes.
        /// </summary>
        /// <param name="rgbA">First color</param>
        /// <param name="rgbB">Second color</param>
        /// <param name="mode">Distance metric</param>
        /// <returns>The distance, or <see cref="double.NaN"/> if a color is invalid or the mode is unknown.</returns>
        public static double Distance(this int rgbA, int rgbB, ColorDistanceEvaluationMode mode)
        {
            if (rgbA.IsInvalid() || rgbB.IsInvalid())
                return double.NaN;
            switch (mode)
            {
                case ColorDistanceEvaluationMode.RGB:
                    {
                        int r = rgbA.ToR() - rgbB.ToR();
                        int g = rgbA.ToG() - rgbB.ToG();
                        int b = rgbA.ToB() - rgbB.ToB();
                        return r * r + g * g + b * b; // + (r-g)*(r-g) + (r-b)*(r-b) + (g-b)*(g-b);
                    }
                case ColorDistanceEvaluationMode.RGBalt:
                    {
                        int r = rgbA.ToR() - rgbB.ToR();
                        int g = rgbA.ToG() - rgbB.ToG();
                        int b = rgbA.ToB() - rgbB.ToB();
                        return (r * r + g * g + b * b + (r - g) * (r - g) + (r - b) * (r - b) + (g - b) * (g - b));
                    }
                case ColorDistanceEvaluationMode.HSV:
                    {
                        HSV hsvi = HSV.CreateFromIntRGB(rgbA);
                        HSV hsvj = HSV.CreateFromIntRGB(rgbB);
                        return HSV.Distance(hsvi, hsvj);
                    }
                case ColorDistanceEvaluationMode.LAB:
                    {
                        LAB labi = LAB.CreateFromIntRGB(rgbA);
                        LAB labj = LAB.CreateFromIntRGB(rgbB);
                        return LAB.Distance(labi, labj);
                    }
                case ColorDistanceEvaluationMode.GRAY:
                    {
                        GRAY yi = GRAY.CreateFromIntRGB(rgbA);
                        GRAY yj = GRAY.CreateFromIntRGB(rgbB);
                        return GRAY.Distance(yi, yj);
                    }

                default:
                    return double.NaN;
            }
        }


        /// <summary>
        /// Packs red, green and blue components and a color type flag into an integer color.
        /// <para/>
        /// Components are clamped to 255; any negative component produces an invalid color.
        /// </summary>
        /// <param name="r">Red component</param>
        /// <param name="g">Green component</param>
        /// <param name="b">Blue component</param>
        /// <param name="type">Color type flag</param>
        /// <returns>The packed color.</returns>
        public static int FromRGB(int r, int g, int b, ColorInfo type)
        {
            if (r < 0 || g < 0 || b < 0)
                return ((int)ColorInfo.Invalid << 24);
            return ((int)type << 24) | (Math.Min(255, r) << 16) | (Math.Min(255, g) << 8) | (Math.Min(255, b));
        }


        /// <summary>
        /// Packs red, green and blue components into a real (<see cref="ColorInfo.IsColor"/>) integer color.
        /// </summary>
        /// <param name="r">Red component</param>
        /// <param name="g">Green component</param>
        /// <param name="b">Blue component</param>
        /// <returns>The packed color.</returns>
        public static int FromRGB(int r, int g, int b) => FromRGB(r, g, b, ColorInfo.IsColor);


        /// <summary>
        /// Packs floating point red, green and blue components (truncated to int) into an integer color.
        /// </summary>
        /// <param name="r">Red component</param>
        /// <param name="g">Green component</param>
        /// <param name="b">Blue component</param>
        /// <returns>The packed color.</returns>
        public static int FromRGB(double r, double g, double b) => FromRGB((int)r, (int)g, (int)b);


        /// <summary>
        /// Finds the palette color nearest to <paramref name="color"/>.
        /// </summary>
        /// <param name="color">Color to match</param>
        /// <param name="palette">Candidate colors</param>
        /// <param name="mode">Distance metric</param>
        /// <returns>The nearest palette color, or <see cref="ColorDefaults.DefaultInvalidColorInt"/> when the palette is empty.</returns>
        public static int GetNearestColor(int color, Palette palette, ColorDistanceEvaluationMode mode)
        {
            if (palette == null || palette.Count == 0)
            {
                return ColorDefaults.DefaultInvalidColorInt;
            }

            var rgbList = palette.ToList();
            var bestCol = rgbList
            .Select(c => (Color: c, Dist: c.Distance(color, mode)))
            .OrderBy(x => x.Dist)
            .First();

            return bestCol.Color;
        }

        /// <summary>
        /// Calculates the representative color of a color histogram.
        /// </summary>
        /// <param name="colorHistogram">Color to pixel count map</param>
        /// <param name="meanMode">
        /// <see cref="ColorSelectionMode.EvaluateColorMean"/>: pixel-count weighted mean;
        /// <see cref="ColorSelectionMode.UseColorPalette"/>: the most frequent color.
        /// </param>
        /// <returns>The representative color, or an invalid color when the histogram is empty.</returns>
        public static int GetColorMean(Dictionary<int, int> colorHistogram, ColorSelectionMode meanMode)
        {
            if (colorHistogram == null || colorHistogram.Count == 0)
                return ColorDefaults.DefaultInvalidColorInt;
            switch (meanMode)
            {
                case ColorSelectionMode.UseColorPalette:
                    {
                        var max = colorHistogram.Max(x => x.Value);
                        var kvp = colorHistogram.FirstOrDefault(x => x.Value == max);
                        return kvp.Key;
                    }
                case ColorSelectionMode.EvaluateColorMean:
                    {
                        int count = 0;
                        double r = 0;
                        double g = 0;
                        double b = 0;
                        foreach (var kvp in colorHistogram)
                        {
                            count += kvp.Value;
                            r += kvp.Value * kvp.Key.ToR();
                            g += kvp.Value * kvp.Key.ToG();
                            b += kvp.Value * kvp.Key.ToB();
                        }
                        ;
                        if (count <= 0)
                            return ColorDefaults.DefaultInvalidColorInt;
                        r /= count;
                        g /= count;
                        b /= count;
                        return ColorIntExt.FromRGB(r, g, b);
                    }
                default:
                    return ColorDefaults.DefaultInvalidColorInt;
            }
        }

        /// <summary>
        /// Calculates the representative color of a palette.
        /// </summary>
        /// <param name="palette">Source palette</param>
        /// <param name="meanMode">
        /// <see cref="ColorSelectionMode.EvaluateColorMean"/>: unweighted mean of the palette colors;
        /// <see cref="ColorSelectionMode.UseColorPalette"/>: the palette color nearest to that mean.
        /// </param>
        /// <returns>The representative color, or an invalid color when the palette is empty.</returns>
        public static int GetColorMean(Palette palette, ColorSelectionMode meanMode)
        {
            if (palette == null || palette.Count == 0)
                return ColorDefaults.DefaultInvalidColorInt;
            double r = 0;
            double g = 0;
            double b = 0;
            var rgbList = palette.ToList();
            foreach (var rgb in rgbList)
            {
                r += rgb.ToR();
                g += rgb.ToG();
                b += rgb.ToB();
            }
            ;
            r /= rgbList.Count;
            g /= rgbList.Count;
            b /= rgbList.Count;
            var mean = ColorIntExt.FromRGB(r, g, b);
            switch (meanMode)
            {
                case ColorSelectionMode.EvaluateColorMean:
                    return mean;
                case ColorSelectionMode.UseColorPalette:
                    return GetNearestColor(mean, palette, ColorDistanceEvaluationMode.RGB);
                default:
                    return ColorDefaults.DefaultInvalidColorInt;
            }
        }

        /// <summary>
        /// Calculates the mean color between two colors.
        /// </summary>
        /// <param name="rgbA">First color</param>
        /// <param name="rgbB">Second color</param>
        /// <returns>The mean color, or an invalid color if any input is not a color.</returns>
        public static int GetColorMean(int rgbA, int rgbB)
        {
            if (rgbA < 0 || rgbB < 0)
            {
                return ColorDefaults.DefaultInvalidColorInt;
            }
            int r = (rgbA.ToR() + rgbB.ToR()) / 2;
            int g = (rgbA.ToG() + rgbB.ToG()) / 2;
            int b = (rgbA.ToB() + rgbB.ToB()) / 2;
            return FromRGB(r, g, b);
        }

        /// <summary>
        /// Evaluates the approximation error between two colors (alias of <see cref="Distance"/>).
        /// </summary>
        static public double EvaluateError(int rgbA, int rgbB, ColorDistanceEvaluationMode mode) => Distance(rgbA, rgbB, mode);

        /// <summary>
        /// Evaluates the mean per-pixel color error between two images of the same size.
        /// <para/>
        /// Pixels that are not real colors (masks, tiles, invalid) in either image are ignored.
        /// </summary>
        /// <param name="imageA">First image</param>
        /// <param name="imageB">Second image</param>
        /// <param name="evalMode">Distance metric</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>The mean error, or <see cref="double.NaN"/> when no comparable pixel exists.</returns>
        static public double EvaluateError(ImageData imageA, ImageData imageB, ColorDistanceEvaluationMode evalMode, CancellationToken token = default)
        {
            ImageData.AssertValidAndDimension(imageA,imageB);
            double err = 0;
            int count = 0;
            for (int r = 0; r < imageA.Rows; r++)
            //Parallel.For(0, imageA.Rows, r => //(int r = 0; r < r1; r++)
            {
                token.ThrowIfCancellationRequested();
                for (int c = 0; c < imageA.Columns; c++)
                {
                    var rgb1 = imageA.matrix[r, c];
                    var rgb2 = imageB.matrix[r, c];
                    if (rgb1.IsColor() && rgb2.IsColor())
                    {
                        err += ColorIntExt.EvaluateError(rgb1, rgb2, evalMode);
                        count++;
                    }
                }
            }//);
            if (count > 0)
                err /= count;
            else
                err = double.NaN;
            return err;
        }

        /// <summary>
        /// Returns the maximum distance between any two colors of the collection (O(n²)).
        /// </summary>
        static double GetMaxColorDistance(IEnumerable<int> rgbCollection, ColorDistanceEvaluationMode evalMode, CancellationToken token = default)
        {
            if (rgbCollection == null)
                throw new ArgumentNullException(nameof(rgbCollection));

            if( rgbCollection.Count()== 0)
                return double.NaN;

            double maxDistance = rgbCollection.Max(y => rgbCollection.Max(x => Distance(y, x, evalMode)));

            return maxDistance;
        }

        /// <summary>
        /// Returns the maximum distance between any two colors of the palette.
        /// </summary>
        public static double GetMaxColorDistance(Palette colorPalette, ColorDistanceEvaluationMode evalMode, CancellationToken token = default)
        {
            if (colorPalette == null)
                throw new ArgumentNullException(nameof(colorPalette));
            return GetMaxColorDistance(colorPalette.ToList(), evalMode, CancellationToken.None);
        }

        /// <summary>
        /// Returns the maximum distance between any two colors of the histogram.
        /// </summary>
        public static double GetMaxColorDistance(HistogramRGB hist, ColorDistanceEvaluationMode evalMode, CancellationToken token = default)
        {
            if (hist == null)
                throw new ArgumentNullException(nameof(hist));
            return GetMaxColorDistance(hist.ToPalette(), evalMode, CancellationToken.None);
        }

        /// <summary>
        /// Returns the maximum distance between any two colors of the image palette.
        /// </summary>
        public static double GetMaxColorDistance(ImageData image, ColorDistanceEvaluationMode evalMode, CancellationToken token = default)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            return GetMaxColorDistance(image.ColorPalette, evalMode, CancellationToken.None);
        }
    }
}
