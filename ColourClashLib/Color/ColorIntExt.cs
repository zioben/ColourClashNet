using ColourClashLib.Color;
using ColourClashNet.Color;
using ColourClashNet.Defaults;
using ColourClashNet.Imaging;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color
{


    /// <summary>
    /// Provides extension methods and default values for working with integer-based color representations.
    /// </summary>
    /// <remarks>This static class includes methods for converting integers to and from color representations,
    /// manipulating color components, and calculating color distances and means. It also defines default  colors for
    /// specific use cases, such as background, mask, and transparency.  The integer-based color representation assumes
    /// a 32-bit structure where the lower 24 bits represent  the RGB components, and the upper 8 bits may encode
    /// additional metadata (e.g., color type).</remarks>
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
        public static int SetColorInfo(this int rgb, ColorInfo colorInfo)
        {
            int newrgb = rgb & 0x00_FF_FF_FF;
            newrgb |= ((int)colorInfo) << 24;
            return newrgb;
        }

        /// <summary>
        /// Gets the color information type from the integer representation of a color.
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        static ColorInfo GetColorInfo(this int rgb)
        {
            int val = (rgb >> 24) & 0x00_00_00_FF;
            // Reserved old value for invalid color
            if (val == 0x_00_ff)
            {
                return ColorInfo.Invalid;
            }
            return (ColorInfo)val;
        }


        public static bool IsColor(this int rgb) => GetColorInfo(rgb) == ColorInfo.IsColor;
        //  public static bool IsTagged(this int rgb) => GetColorInfo(rgb) == ;
        public static bool IsInvalid(this int rgb) => GetColorInfo(rgb) == ColorInfo.Invalid;


        /// <summary>
        /// Converts an integer representation of a color to a <see cref="System.Drawing.Color"/>. 
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static System.Drawing.Color ToDrawingColor(this int rgb)
        {
            switch (GetColorInfo(rgb))
            {
                case ColorInfo.IsColor:
                    {
                        unchecked
                        {
                            return System.Drawing.Color.FromArgb(rgb | (int)0xFF_00_00_00);
                        }
                    }
                //case ColorIntType.IsBkg:
                //    {
                //        return DefaultBkgColor;
                //    }
                case ColorInfo.IsMask:
                    {
                        return ColorDefaults.DefaultMaskColor;
                    }
                case ColorInfo.IsTile:
                    {
                        return ColorDefaults.DefaultTileColor;
                    }
                default:
                    {
                        return System.Drawing.Color.Transparent;
                    }
            }
        }

        /// <summary>
        /// Extracts the Red component from an integer representation of a color.
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static int ToR(this int rgb)
        {
            return rgb >= 0 ? (rgb >> 16) & 0x00ff : -1;
        }

        /// <summary>
        /// Extracts the Green component from an integer representation of a color.
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static int ToG(this int rgb)
        {
            return rgb >= 0 ? (rgb >> 8) & 0x00ff : -1;
        }

        /// <summary>
        /// Extracts the Blue component from an integer representation of a color.
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static int ToB(this int rgb)
        {
            return rgb >= 0 ? (rgb >> 0) & 0x00ff : -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static HSV ToHSV(this int rgb) => HSV.CreateFromIntRGB(rgb);

        /// <summary>
        /// Converts an integer representation of a color to its LAB  representation.
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static LAB ToLAB(this int rgb) => LAB.CreateFromIntRGB(rgb);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static GRAY ToGray(this int rgb) => GRAY.CreateFromIntRGB(rgb);


        /// <summary>
        /// Calculates the distance between two colors represented as integers, based on the specified color distance evaluation mode.
        /// </summary>
        /// <param name="rgbA"></param>
        /// <param name="rgbB"></param>
        /// <param name="eMode"></param>
        /// <returns></returns>
        public static double Distance(this int rgbA, int rgbB, ColorDistanceEvaluationMode eMode)
        {
            if (rgbA.IsInvalid() || rgbB.IsInvalid())
                return double.NaN;
            switch (eMode)
            {
                case ColorDistanceEvaluationMode.RGB:
                    {
                        int R = rgbA.ToR() - rgbB.ToR();
                        int G = rgbA.ToG() - rgbB.ToG();
                        int B = rgbA.ToB() - rgbB.ToB();
                        return R * R + G * G + B * B; // + (R-G)*(R-G) + (R-B)*(R-B) + (G-B)*(G-B);
                    }
                case ColorDistanceEvaluationMode.RGBalt:
                    {
                        int R = rgbA.ToR() - rgbB.ToR();
                        int G = rgbA.ToG() - rgbB.ToG();
                        int B = rgbA.ToB() - rgbB.ToB();
                        return (R * R + G * G + B * B + (R - G) * (R - G) + (R - B) * (R - B) + (G - B) * (G - B));
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
        /// Creates an integer representation of a color from its Red, Green, and Blue components, along with an optional color type.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="eType"></param>
        /// <returns></returns>
        public static int FromRGB(int r, int g, int b, ColorInfo eType)
        {
            if (r < 0 || g < 0 || b < 0)
                return ((int)ColorInfo.Invalid << 24);
            return ((int)eType << 24) | (Math.Min(255, r) << 16) | (Math.Min(255, g) << 8) | (Math.Min(255, b));
        }


        /// <summary>
        /// Creates an integer representation of a color from its Red, Green, and Blue components.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int FromRGB(int r, int g, int b) => FromRGB(r, g, b, ColorInfo.IsColor);


        /// <summary>
        /// Creates an integer representation of a color from its Red, Green, and Blue components.
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="dg"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static int FromRGB(double dr, double dg, double db) => FromRGB((int)dr, (int)dg, (int)db);


        /// <summary>
        /// Converts a <see cref="System.Drawing.Color"/> to its integer representation.
        /// </summary>
        /// <param name="oColor"></param>
        /// <returns></returns>
        public static int FromDrawingColor(System.Drawing.Color oColor)
        {
            if (oColor == System.Drawing.Color.Transparent)
                return ColorDefaults.DefaultInvalidColorInt;

            return FromRGB(oColor.R, oColor.G, oColor.B);
        }

        /// <summary>
        /// Finds the nearest color in the provided color palette to the specified color, based on the given color distance evaluation mode.
        /// </summary>
        /// <param name="iColor"></param>
        /// <param name="oPalette"></param>
        /// <param name="eMode"></param>
        /// <returns></returns>
        public static int GetNearestColor(int iColor, Palette oPalette, ColorDistanceEvaluationMode eMode)
        {
            if (oPalette == null || oPalette.Count == 0)
            {
                return ColorDefaults.DefaultInvalidColorInt;
            }

            var rgbList = oPalette.ToList();
            var bestCol = rgbList
            .Select(c => (Color: c, Dist: c.Distance(iColor, eMode)))
            .OrderBy(x => x.Dist)
            .First();

            return bestCol.Color;
        }

        /// <summary>
        /// Calculates the mean color from a histogram of colors, based on the specified mean mode.
        /// </summary>
        /// <param name="lColorHistogram"></param>
        /// <param name="eMeanMode"></param>
        /// <returns></returns>
        public static int GetColorMean(Dictionary<int, int> lColorHistogram, ColorMeanMode eMeanMode)
        {
            if (lColorHistogram == null || lColorHistogram.Count == 0)
                return ColorDefaults.DefaultInvalidColorInt;
            switch (eMeanMode)
            {
                case ColorMeanMode.UseColorPalette:
                    {
                        var max = lColorHistogram.Max(X => X.Value);
                        var kvp = lColorHistogram.FirstOrDefault(X => X.Value == max);
                        return kvp.Key;
                    }
                case ColorMeanMode.UseMean:
                    {
                        int Count = 0;
                        double R = 0;
                        double G = 0;
                        double B = 0;
                        foreach (var kvp in lColorHistogram)
                        {
                            Count += kvp.Value;
                            R += kvp.Value * kvp.Key.ToR();
                            G += kvp.Value * kvp.Key.ToG();
                            B += kvp.Value * kvp.Key.ToB();
                        }
                        ;
                        if (Count <= 0)
                            return ColorDefaults.DefaultInvalidColorInt;
                        R /= Count;
                        G /= Count;
                        B /= Count;
                        return ColorIntExt.FromRGB(R, G, B);
                    }
                default:
                    return ColorDefaults.DefaultInvalidColorInt;
            }
        }

        /// <summary>
        /// Calculates the mean color from a color palette, based on the specified mean mode.
        /// </summary>
        /// <param name="oPalette"></param>
        /// <param name="eMeanMode"></param>
        /// <returns></returns>
        public static int GetColorMean(Palette oPalette, ColorMeanMode eMeanMode)
        {
            if (oPalette == null || oPalette.Count == 0)
                return ColorDefaults.DefaultInvalidColorInt;
            double R = 0;
            double G = 0;
            double B = 0;
            var rgbList = oPalette.ToList();
            foreach (var rgb in rgbList)
            {
                R += rgb.ToR();
                G += rgb.ToG();
                B += rgb.ToB();
            }
            ;
            R /= rgbList.Count;
            G /= rgbList.Count;
            B /= rgbList.Count;
            var iMean = ColorIntExt.FromRGB(R, G, B);
            switch (eMeanMode)
            {
                case ColorMeanMode.UseMean:
                    return iMean;
                case ColorMeanMode.UseColorPalette:
                    return GetNearestColor(iMean, oPalette, ColorDistanceEvaluationMode.RGB);
                default:
                    return ColorDefaults.DefaultInvalidColorInt;
            }
        }

        /// <summary>
        /// Calculates the mean color between two colors.
        /// </summary>
        /// <param name="rgbA"></param>
        /// <param name="rgbB"></param>
        /// <returns></returns>
        public static int GetColorMean(int rgbA, int rgbB)
        {
            if (rgbA < 0 || rgbB < 0)
            {
                return ColorDefaults.DefaultInvalidColorInt;
            }
            int R = (rgbA.ToR() + rgbB.ToR()) / 2;
            int G = (rgbA.ToG() + rgbB.ToG()) / 2;
            int B = (rgbA.ToB() + rgbB.ToB()) / 2;
            return FromRGB(R, G, B);
        }

        static public double EvaluateError(int rgbA, int rgbB, ColorDistanceEvaluationMode eMode) => Distance(rgbA, rgbB, eMode);

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

        static double GetMaxColorDistance(IEnumerable<int> rgbCollection, ColorDistanceEvaluationMode evalMode, CancellationToken token = default)
        {
            if (rgbCollection == null)
                throw new ArgumentNullException(nameof(rgbCollection));

            if( rgbCollection.Count()== 0)
                return double.NaN;

            double maxDistance = rgbCollection.Max(Y => rgbCollection.Max(X => Distance(Y, X, evalMode)));

            return maxDistance;
        }

        public static double GetMaxColorDistance(Palette colorPalette, ColorDistanceEvaluationMode evalMode, CancellationToken token = default)
        {
            if (colorPalette == null)
                throw new ArgumentNullException(nameof(colorPalette));
            return GetMaxColorDistance(colorPalette.ToList(), evalMode, CancellationToken.None);
        }

        public static double GetMaxColorDistance(HistogramRGB hist, ColorDistanceEvaluationMode evalMode, CancellationToken token = default)
        {
            if (hist == null)
                throw new ArgumentNullException(nameof(hist));
            return GetMaxColorDistance(hist.ToPalette(), evalMode, CancellationToken.None);
        }
        public static double GetMaxColorDistance(ImageData image, ColorDistanceEvaluationMode evalMode, CancellationToken token = default)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            return GetMaxColorDistance(image.ColorPalette, evalMode, CancellationToken.None);
        }
    }
}
