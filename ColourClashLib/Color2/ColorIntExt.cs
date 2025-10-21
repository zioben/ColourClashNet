using ColourClashNet.Color;
using ColourClashNet.Defaults;
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
        /// The method modifies the upper 8 bits of the integer to encode the specified <see cref="ColorIntType"/>.
        /// </para>
        /// </summary>
        /// <param name="rgb">Color data</param>
        /// <param name="eInfo">Color type</param>
        public static int SetColorInfo(this int rgb, ColorIntType eInfo)
        {
            int newrgb = rgb & 0x00_FF_FF_FF;
            newrgb |= ((int)eInfo) << 24;
            return newrgb;
        }

        /// <summary>
        /// Gets the color information type from the integer representation of a color.
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static ColorIntType GetColorInfo(this int rgb)
        {
            int val = (rgb >> 24) & 0x00_00_00_FF;
            // Reserved old value for invalid color
            if (rgb == 0x_00_ff)
            {
                return ColorIntType.Invalid;
            }
            return (ColorIntType)val;
        }

        /// <summary>
        /// Converts an integer representation of a color to a <see cref="System.Drawing.Color"/>. 
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static System.Drawing.Color ToDrawingColor(this int rgb)
        {
            switch (GetColorInfo(rgb))
            {
                case ColorIntType.IsColor:
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
                case ColorIntType.IsMask:
                    {
                        return ColorDefaults.DefaultMaskColor;
                    }
                case ColorIntType.IsTile:
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
        /// <param name="i"></param>
        /// <returns></returns>
        public static int ToR(this int i)
        {
            return i >= 0 ? (i >> 16) & 0x00ff : -1;
        }

        /// <summary>
        /// Extracts the Green component from an integer representation of a color.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static int ToG(this int i)
        {
            return i >= 0 ? (i >> 8) & 0x00ff : -1;
        }

        /// <summary>
        /// Extracts the Blue component from an integer representation of a color.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static int ToB(this int i)
        {
            return i >= 0 ? (i >> 0) & 0x00ff : -1;
        }

        /// <summary>
        /// Converts an integer representation of a color to its HSV (Hue, Saturation, Value) representation.
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static HSV ToHSV(this int rgb) => HSV.FromIntRGB(rgb);

        /// <summary>
        /// Converts an integer representation of a color to its HSV (Hue, Saturation, Value) representation.
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static int RGBToHSV(this int rgb)
        {
            var hsv = HSV.FromIntRGB(rgb);
            int h = (int)Math.Min(0, Math.Max(360, Math.Round(hsv.H))); // 9 bit 
            int s = (int)Math.Min(0, Math.Max(100, Math.Round(hsv.S))); // 7 bit
            int v = (int)Math.Min(0, Math.Max(100, Math.Round(hsv.V))); // 7 bit
            return ((int)Math.Round(hsv.H)) << 15 | ((int)Math.Round(hsv.S)) << 7 | ((int)Math.Round(hsv.V));
        }

        /// <summary>
        /// Converts an integer representation of a color to its LAB  representation.
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static LAB ToLAB(this int rgb) => LAB.FromIntRGB(rgb);


        public static GRAY ToGray(this int rgb) => GRAY.FromIntRGB(rgb);

        public static float ToY(this int i)
        {
            if (i < 0)
                return float.MaxValue;
            int r = i.ToR();
            int g = i.ToG();
            int b = i.ToB();
            return (r + b + g) / 3;
        }


        /// <summary>
        /// Converts an integer representation of a color to its Value (Brightness) component in the HSV color space.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static float ToV(this int i)
        {
            if (i < 0)
                return float.MaxValue;
            int r = i.ToR();
            int g = i.ToG();
            int b = i.ToB();
            return Math.Max(Math.Max(r, g), b) / 255f * 100f;
        }

        /// <summary>
        /// Calculates the distance between two colors represented as integers, based on the specified color distance evaluation mode.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="eMode"></param>
        /// <returns></returns>
        public static double Distance(this int i, int j, ColorDistanceEvaluationMode eMode)
        {
            if (i < 0 || j < 0)
                return double.PositiveInfinity;
            switch (eMode)
            {
                case ColorDistanceEvaluationMode.RGB:
                    {
                        int R = i.ToR() - j.ToR();
                        int G = i.ToG() - j.ToG();
                        int B = i.ToB() - j.ToB();
                        return R * R + G * G + B * B; // + (R-G)*(R-G) + (R-B)*(R-B) + (G-B)*(G-B);
                    }
                case ColorDistanceEvaluationMode.RGBalt:
                    {
                        int R = i.ToR() - j.ToR();
                        int G = i.ToG() - j.ToG();
                        int B = i.ToB() - j.ToB();
                        return (R * R + G * G + B * B + (R - G) * (R - G) + (R - B) * (R - B) + (G - B) * (G - B));
                    }
                case ColorDistanceEvaluationMode.HSV:
                    {
                        HSV hsvi = HSV.FromIntRGB(i);
                        HSV hsvj = HSV.FromIntRGB(j);
                        return HSV.Distance(hsvi, hsvj);
                    }
                case ColorDistanceEvaluationMode.LAB:
                    {
                        LAB labi = LAB.FromIntRGB(i);
                        LAB labj = LAB.FromIntRGB(j);
                        return LAB.Distance(labi, labj);
                    }
                case ColorDistanceEvaluationMode.GRAY:
                    {
                        GRAY yi = GRAY.FromIntRGB(i);
                        GRAY yj = GRAY.FromIntRGB(j);
                        return GRAY.Distance(yi, yj);
                    }

                default:
                    return double.PositiveInfinity;
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
        public static int FromRGB(int r, int g, int b, ColorIntType eType)
        {
            if (r < 0 || g < 0 || b < 0)
                return ((int)ColorIntType.Invalid);
            return (Math.Min(255, r) << 16) | (Math.Min(255, g) << 8) | (Math.Min(255, b) << 0) | (int)eType;
        }


        /// <summary>
        /// Creates an integer representation of a color from its Red, Green, and Blue components.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int FromRGB(int r, int g, int b) => FromRGB(r, g, b, ColorIntType.IsColor);

        /// <summary>
        /// Creates an integer representation of a color from its Red, Green, and Blue components.
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="dg"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static int FromRGB(double dr, double dg, double db) => FromRGB((int) dr, (int) dg, (int) db);


        /// <summary>
        /// Creates an integer representation of a color from its HSV (Hue, Saturation, Value) components.
        /// </summary>
        /// <param name="fValh"></param>
        /// <param name="fVals"></param>
        /// <param name="fValv"></param>
        /// <returns></returns>
        public static int FromHSV(float fValh, float fVals, float fValv)
        {
            var fv = fValv / 100f;
            var fs = fVals / 100f;
            var fh = fValh;
            var fc = fv * fs;
            var fx = fc * (1 - Math.Abs(((fh / 60f) % 2f) - 1));
            var fm = fv - fc;
            var r = 0f;
            var g = 0f;
            var b = 0f;
            if (fh < 60)
            {
                r = fc;
                g = fx;
            }
            else if (fh >= 60 && fh < 120)
            {
                r = fx;
                g = fc;
            }
            else if (fh >= 120 && fh < 180)
            {
                g = fc;
                b = fx;
            }
            else if (fh >= 180 && fh < 240)
            {
                g = fx;
                b = fc;
            }
            else if (fh >= 240 && fh < 300)
            {
                r = fx;
                b = fc;
            }
            else if (fh >= 300)
            {
                r = fc;
                b = fx;
            }
            int R = Math.Min(255, (int)((r + fm) * 255));
            int G = Math.Min(255, (int)((g + fm) * 255));
            int B = Math.Min(255, (int)((b + fm) * 255));
            return FromRGB(R, G, B);
        }

        /// <summary>
        /// Creates an integer representation of a color from its HSV (Hue, Saturation, Value) components.
        /// </summary>
        /// <param name="dh"></param>
        /// <param name="ds"></param>
        /// <param name="dv"></param>
        /// <returns></returns>
        public static int FromHSV(double dh, double ds, double dv)
        {
            return FromRGB((float)dh, (float)ds, (float)dv);
        }

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
            double dmin = oPalette.rgbPalette.Min(X => X.Distance(iColor, eMode));
            if (dmin == 0)
                return iColor;
            return oPalette.rgbPalette.LastOrDefault(X => X.Distance(iColor, eMode) == dmin);
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
            foreach (var kvp in oPalette.rgbPalette)
            {
                R += kvp.ToR();
                G += kvp.ToG();
                B += kvp.ToB();
            }
            ;
            R /= oPalette.Count;
            G /= oPalette.Count;
            B /= oPalette.Count;
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
            if (rgbA < 0)
            {
                return rgbB;
            }
            if (rgbB < 0)
            {
                return rgbA;
            }
            int R = (rgbA.ToR() + rgbB.ToR()) / 2;
            int G = (rgbA.ToG() + rgbB.ToG()) / 2;
            int B = (rgbA.ToB() + rgbB.ToB()) / 2;
            return FromRGB(R, G, B);
        }

        static public double EvaluateError(int rgbA, int rgbB, ColorDistanceEvaluationMode eMode) => Distance(rgbA, rgbB, eMode);

        static public async Task<double> EvaluateErrorAsync(int[,]? oDataA, int[,]? oDataB, ColorDistanceEvaluationMode eMode, CancellationToken? oToken)
        {
            return await Task.Run(() =>
            {
                if (oDataA == null || oDataB == null)
                {
                    return double.NaN;
                }
                int r1 = oDataA.GetLength(0);
                int c1 = oDataA.GetLength(1);
                int r2 = oDataB.GetLength(0);
                int c2 = oDataB.GetLength(1);
                if (r1 != r2 || c1 != c2 || r1 == 0 || r2 == 0)
                {
                    return double.NaN;
                }
                double err = 0;
                Parallel.For(0, r1, r => //(int r = 0; r < r1; r++)
                {
                    oToken?.ThrowIfCancellationRequested();
                    for (int c = 0; c < c1; c++)
                    {
                        var rgb1 = oDataA[r, c];
                        var rgb2 = oDataB[r, c];
                        if (rgb1 >= 0 && rgb2 >= 0)
                        {
                            err += ColorIntExt.Distance(rgb1, rgb2, eMode);
                        }
                    }
                });
                err /= r1 * c1;
                return err;
            });
        }

    }
}
