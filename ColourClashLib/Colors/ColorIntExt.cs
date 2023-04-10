using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using ColourClashLib.Color;
using System.Runtime.InteropServices;

namespace ColourClashNet.Colors
{
    public static class ColorIntExt
    {
        public static System.Drawing.Color ToDrawingColor(this int i)
        {
            if (i < 0)
            {
                return System.Drawing.Color.Transparent;
            }
            unchecked
            {
                return System.Drawing.Color.FromArgb(i | (int)0xFF_00_00_00);
            }
        }

        public static int ToR(this int i)
        {
            return i >= 0 ? (i >> 16) & 0x00ff : -1;
        }
        public static int ToG(this int i)
        {
            return i >= 0 ? (i >> 8) & 0x00ff : -1;
        }
        public static int ToB(this int i)
        {
            return i >= 0 ? (i >> 0) & 0x00ff : -1;
        }

        public static int ToHSV(this int i)
        {
            float r = i.ToR() / 255f;
            float g = i.ToG() / 255f;
            float b = i.ToB() / 255f;
            float max = Math.Max(Math.Max(r, g), b);
            float min = Math.Min(Math.Min(r, g), b);
            float delta = max - min;
            float v = max * 100f;
            float s = 0;
            float h = 0;
            if (delta > 0)
            {
                s = (int)Math.Min(100, delta / max * 100);
                if (r >= g && r >= b)
                {
                    h = (g - b) / delta;
                    if (h < 0)
                        h += 6;
                }
                else if (g >= r && g >= b)
                {
                    h = ((b - r) / delta) + 2;
                }
                else
                {
                    h = ((r - g) / delta) + 4;
                }
                h *= 60;
            }
            return ((int)Math.Round(h)) << 24 | ((int)Math.Round(s)) << 8 | ((int)Math.Round(v));
        }


        public static float ToH(this int i)
        {
            if (i < 0)
                return float.MaxValue;
            float r = i.ToR() / 255f;
            float g = i.ToG() / 255f;
            float b = i.ToB() / 255f;
            float max = Math.Max(Math.Max(r, g), b);
            float min = Math.Min(Math.Min(r, g), b);
            if (max == min)
                return 0;
            float delta = max - min;
            float h = 0;
            if (r >= g && r >= b)
            {
                h = (g - b) / delta;
                if (h < 0)
                    h += 6;
            }
            else if (g >= r && g >= b)
            {
                h = ((b - r) / delta) + 2;
            }
            else
            {
                h = ((r - g) / delta) + 4;
            }
            return h * 60;
        }

        public static float DistanceH(float hA, float hB)
        {
            // hA -> [0-360] + 2k*Pi
            // hB -> [0-360] + 2k*Pi
            return Math.Min(Math.Abs(hA - hB), Math.Abs(hA + 360 - hB));
        }

        public static float ToS(this int i)
        {
            if (i < 0)
                return float.MaxValue;
            int r = i.ToR();
            int g = i.ToG();
            int b = i.ToB();
            float max = Math.Max(Math.Max(r, g), b);
            float min = Math.Min(Math.Min(r, g), b);
            if (max == min)
                return 0;
            return (1f - min / max) * 100;
        }

        public static float ToY(this int i)
        {
            if (i < 0)
                return float.MaxValue;
            int r = i.ToR();
            int g = i.ToG();
            int b = i.ToB();
            return (r + b + g) / 3;
        }

        public static float ToV(this int i)
        {
            if (i < 0)
                return float.MaxValue;
            int r = i.ToR();
            int g = i.ToG();
            int b = i.ToB();
            return Math.Max(Math.Max(r, g), b) / 255f * 100f;
        }


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
                        return R * R + G * G + B * B + (R - G) * (R - G) + (R - B) * (R - B) + (G - B) * (G - B);
                    }
                case ColorDistanceEvaluationMode.HSV:
                    {
                        var H = DistanceH(i.ToH(), j.ToH());
                        var S = i.ToS() - j.ToS();
                        var V = i.ToV() - j.ToV();
                        return H * H + S * S + V * V;
                    }
                case ColorDistanceEvaluationMode.All:
                    {
                        int R = i.ToR() - j.ToR();
                        int G = i.ToG() - j.ToG();
                        int B = i.ToB() - j.ToB();
                        var H = (DistanceH(i.ToH(), j.ToH())) / 360f * 255f;
                        var S = (i.ToS() - j.ToS()) / 100f * 255f;
                        var V = (i.ToV() - j.ToV()) / 100f * 255f;
                        return R * R + G * G + B * B + H * H + S * S + V * V;
                    }
                default:
                    return double.PositiveInfinity;
            }
        }

        public static int FromRGB(int r, int g, int b)
        {
            if (r < 0 || g < 0 || b < 0)
                return -1;
            return (Math.Min(255, r) << 16) | (Math.Min(255, g) << 8) | (Math.Min(255, b) << 0);
        }

        public static int FromRGB(double dr, double dg, double db)
        {
            return FromRGB((int)dr, (int)dg, (int)db);
        }

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

        public static int FromHSV(double dh, double ds, double dv)
        {
            return FromRGB((float)dh, (float)ds, (float)dv);
        }

        public static int FromDrawingColor(System.Drawing.Color oColor)
        {
            if (oColor == System.Drawing.Color.Transparent)
                return -1;

            return FromRGB(oColor.R, oColor.G, oColor.B);
        }

        public static int GetNearestColor(int iColor, ColorPalette oPalette, ColorDistanceEvaluationMode eMode)
        {
            double dmin = oPalette.rgbPalette.Min(X => X.Distance(iColor, eMode));
            if (dmin == 0)
                return iColor;
            return oPalette.rgbPalette.LastOrDefault(X => X.Distance(iColor, eMode) == dmin);
        }

        public static int GetColorMean(Dictionary<int, int> lColorHistogram, ColorMeanMode eMeanMode)
        {
            if (lColorHistogram == null || lColorHistogram.Count == 0)
                return -1;
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
                        };
                        if (Count <= 0)
                            return -1;
                        R /= Count;
                        G /= Count;
                        B /= Count;
                        return ColorIntExt.FromRGB(R, G, B);
                    }
                default:
                    return -1;
            }
        }


        public static int GetColorMean(ColorPalette oPalette, ColorMeanMode eMeanMode)
        {
            if (oPalette == null || oPalette.Colors == 0)
                return -1;
            double R = 0;
            double G = 0;
            double B = 0;
            foreach (var kvp in oPalette.rgbPalette)
            {
                R += kvp.ToR();
                G += kvp.ToG();
                B += kvp.ToB();
            };
            R /= oPalette.Colors;
            G /= oPalette.Colors;
            B /= oPalette.Colors;
            var iMean = ColorIntExt.FromRGB(R, G, B);
            switch (eMeanMode)
            {
                case ColorMeanMode.UseMean:
                    return iMean;
                case ColorMeanMode.UseColorPalette:
                    return GetNearestColor(iMean, oPalette, ColorDistanceEvaluationMode.RGB);
                default:
                    return -1;
            }
        }

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

    }
}
