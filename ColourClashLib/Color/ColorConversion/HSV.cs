﻿using ColourClashNet.Color;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ColourClashNet.Color
{
    /// <summary>
    /// Represents a color in HSV (Hue, Saturation, Value) color space.
    /// </summary>
    public struct HSV
    {
        /// <summary>
        /// Hue component, in degrees [0, 360)
        /// </summary>
        public float H { get; set; }

        /// <summary>
        /// Saturation component, in percentage [0, 100]
        /// </summary>
        public float S { get; set; }

        /// <summary>
        /// Value (Brightness) component, in percentage [0, 100]
        /// </summary>
        public float V { get; set; }

        public bool Valid => H >= 0 && S >= 0 && V >= 0;


        public HSV(float h, float s, float v)
        {
            H = h;
            S = s;
            V = v;
        }

        /// <summary>
        /// Converts an RGB color (int representation) to HSV.
        /// </summary>
        public static HSV FromIntRGB(int rgb)
        {
            float r = rgb.ToR() / 255f;
            float g = rgb.ToG() / 255f;
            float b = rgb.ToB() / 255f;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;

            float h = 0;
            if (delta > 0)
            {
                if (r == max)
                {
                    h = 60 * (((g - b) / delta) % 6);
                }
                else if (g == max)
                {
                    h = 60 * (((b - r) / delta) + 2);
                }
                else
                {
                    h = 60 * (((r - g) / delta) + 4);
                }
            }

            if (h < 0) h += 360;

            float s = max == 0 ? 0 : (delta / max) * 100f;
            float v = max * 100f;

            return new HSV(h, s, v);
        }

        /// <summary>
        /// Converts HSV to an integer RGB representation.
        /// </summary>
        public int ToIntRGB()
        {
            float h = H;
            float s = S / 100f;
            float v = V / 100f;

            float c = v * s;
            float x = c * (1 - Math.Abs((h / 60f) % 2 - 1));
            float m = v - c;

            float r1 = 0, g1 = 0, b1 = 0;

            if (h >= 0 && h < 60) { r1 = c; g1 = x; b1 = 0; }
            else if (h >= 60 && h < 120) { r1 = x; g1 = c; b1 = 0; }
            else if (h >= 120 && h < 180) { r1 = 0; g1 = c; b1 = x; }
            else if (h >= 180 && h < 240) { r1 = 0; g1 = x; b1 = c; }
            else if (h >= 240 && h < 300) { r1 = x; g1 = 0; b1 = c; }
            else { r1 = c; g1 = 0; b1 = x; }

            int r = (int)Math.Round((r1 + m) * 255);
            int g = (int)Math.Round((g1 + m) * 255);
            int b = (int)Math.Round((b1 + m) * 255);

            return ColorIntExt.FromRGB(r, g, b);
        }

        ///// <summary>
        ///// Converts LAB to integer RGB representation.
        ///// </summary>
        //public int ToInt32()
        //{
        //    int h = (int)Math.Min(0, Math.Max(360, Math.Round(H))); // 9 bit 
        //    int s = (int)Math.Min(0, Math.Max(100, Math.Round(S))); // 7 bit
        //    int v = (int)Math.Min(0, Math.Max(100, Math.Round(V))); // 7 bit
        //    return (h << 16 | s << 8 | v);
        //}

        ///// <summary>
        ///// Converts LAB to integer RGB representation.
        ///// </summary>
        //public static HSV FromInt32(int hsvValue)
        //{
        //    if (ColorIntExt.GetColorInfo(hsvValue) == ColorIntType.Invalid)
        //    {
        //        return new HSV(0,0,0);
        //    }
        //    int v = (hsvValue >> 0) & 0x007F;
        //    int s = (hsvValue >> 8) & 0x007F;
        //    int h = (hsvValue >> 16) & 0x01FF;
        //    return new HSV(h,s,v);
        //}

        /// <summary>
        /// Calculates the distance between two HSV colors.
        /// </summary>
        public static double Distance(HSV a, HSV b)
        {
            if (!a.Valid || !b.Valid)
                return double.NaN;
            double dh = Math.Min(Math.Abs(a.H - b.H), 360 - Math.Abs(a.H - b.H));
            double ds = a.S - b.S;
            double dv = a.V - b.V;
            return dh * dh + ds * ds + dv * dv;
        }

        public override string ToString() => $"HSV(H={H:F2}, S={S:F2}, V={V:F2})";
    }
}


