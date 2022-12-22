using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using ColourClashLib.Color;

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

        public static float ToH(this int i)
        {
            if (i < 0)
                return float.MaxValue;
            var Col = ToDrawingColor(i);
            return Col.GetHue();
        }

        public static float ToS(this int i)
        {
            if (i < 0)
                return float.MaxValue;
            var Col = ToDrawingColor(i);
            return Col.GetSaturation() * 100.0f;
        }

        public static float ToV(this int i)
        {
            if (i < 0)
                return float.MaxValue;
            var Col = ToDrawingColor(i);
            return Col.GetBrightness() * 100.0f;
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
                        return R * R + G * G + B * B + (R-G)*(R-G) + (R-B)*(R-B) + (G-B)*(G-B);
                    }
                case ColorDistanceEvaluationMode.HSV:
                    {
                        var H = i.ToH() - j.ToH();
                        var S = i.ToS() - j.ToS();
                        var V = i.ToV() - j.ToV();
                        return H * H + S * S + V * V;
                    }
                case ColorDistanceEvaluationMode.All:
                    {
                        int R = i.ToR() - j.ToR();
                        int G = i.ToG() - j.ToG();
                        int B = i.ToB() - j.ToB();
                        var H = i.ToH() - j.ToH();
                        var S = i.ToS() - j.ToS();
                        var V = i.ToV() - j.ToV();
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

        public static int GetColorMean(  Dictionary<int,int> lColorHistogram, ColorMeanMode eMeanMode )
        {
            if (lColorHistogram == null || lColorHistogram.Count == 0)
                return -1;
            switch (eMeanMode)
            {
                case ColorMeanMode.UseColorPalette:
                    {
                        var max = lColorHistogram.Max(X => X.Value);
                        var kvp = lColorHistogram.FirstOrDefault(X=>X.Value== max);   
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
            var iMean =  ColorIntExt.FromRGB(R, G, B);
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
    }
}
