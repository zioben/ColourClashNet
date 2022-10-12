using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color
{


    public enum ColorQuantizationMode
    {
        Unknown=0,
        RGB888,
        RGB565,
        RGB555,
        RGB444,
        RGB333,
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
        All,
        RGB,
        HSV,
    }


    public struct ColorItem : IEquatable<ColorItem>
    {
        static double dHSVScale =360.0;

        public int R, G, B;
        public double H, S, V;

        public ColorItem(int r = -1, int g = -1, int b = -1)
        {
            R = r;
            G = g;
            B = b;
            H = 0;
            S = 0;
            V = 0;

            if (!Valid)
                return;

            double R1 = (double)R / 255;
            double G1 = (double)G / 255;
            double B1 = (double)B / 255;

            double Max = Math.Max(B1, Math.Max(R1, G1));
            if (Max <= 0)
                return;

            V = Max * 100;

            double Min = Math.Min(B1, Math.Min(R1, G1));

            double Delta = Max - Min;
            S = Delta / Max * 100;

            if (Delta <= 0)
                return;

            ////double Rc = (Max - R1) / Delta;
            ////double Gc = (Max - G1) / Delta; 
            ////double Bc = (Max - B1) / Delta;
            ////if (Max == R1)
            ////    H = 0.0 + Bc - Gc;
            ////else if (Max == G1)
            ////    H = 2.0 + Rc - Bc;
            ////else
            ////    H = 4.0 + Gc - Rc;
            ////H = (H / 6.0) % 1.0;
            ////H *= 360;

            // Non negative with Modulus
            if (Max == R1)
                H = 6.0 + (G1 - B1)/Delta;
            else if (Max == G1)
                H = 8.0 + (B1 - R1)/Delta;
            else
                H = 10.0 + (R1 - G1)/Delta;
            H = ((H / 6.0) % 1.0) * 180;
        }

        public bool Equals(ColorItem other)
        {
            return R == other.R && G == other.G && B == other.B;
        }

        public ColorItem Quantize(ColorQuantizationMode colorHistMode)
        {
            return FromColorItem(this, colorHistMode);
        }

        public override string ToString()
        {
            return $"{R};{G};{B}";
        }

        public static ColorItem FromColorItem( ColorItem c, ColorQuantizationMode colorHistMode)
        {
            if (!c.Valid)
                return c;
            switch (colorHistMode)
            {
                case ColorQuantizationMode.RGB888:
                    return new ColorItem
                    {
                        R = c.R,
                        G = c.G,
                        B = c.B,
                    };
                case ColorQuantizationMode.RGB333:
                    return new ColorItem
                    {
                        R = (c.R & 0xE0) | 0x1f, // | ((c.R & 0x20) > 0 ? 0x1f : 0),
                        G = (c.G & 0xE0) | 0x1f,// | ((c.G & 0x20) > 0 ? 0x1f : 0),
                        B = (c.B & 0xE0) | 0x1f,// | ((c.B & 0x20) > 0 ? 0x1f : 0),
                    };
                case ColorQuantizationMode.RGB444:
                    return new ColorItem
                    {
                        R = (c.R & 0xF0) | 0x0f,// | ((c.R & 0x10) > 0 ? 0x0f : 0),
                        G = (c.G & 0xF0) | 0x0f,// | ((c.G & 0x10) > 0 ? 0x0f : 0),
                        B = (c.B & 0xF0) | 0x0f,// | ((c.B & 0x10) > 0 ? 0x0f : 0),
                    };
                case ColorQuantizationMode.RGB555:
                    return new ColorItem
                    {
                        R = (c.R & 0xF8) | 0x07, // | ((c.R & 0x08) > 0 ? 0x07 : 0),
                        G = (c.G & 0xF8) | 0x07,//| ((c.G & 0x08) > 0 ? 0x07 : 0),
                        B = (c.B & 0xF8) | 0x07,//| ((c.B & 0x08) > 0 ? 0x07 : 0),
                    };
                case ColorQuantizationMode.RGB565:
                    return new ColorItem
                    {
                        R = (c.R & 0xF8) | 0x07, // | ((c.R & 0x08) > 0 ? 0x07 : 0),
                        G = (c.G & 0xFC) | 0x03,//| ((c.G & 0x08) > 0 ? 0x07 : 0),
                        B = (c.B & 0xF8) | 0x07,//| ((c.B & 0x08) > 0 ? 0x07 : 0),
                    };
                default:
                    return new ColorItem();
            }

        }

        public static ColorItem FromDrawingColor(System.Drawing.Color c, ColorQuantizationMode colorHistMode)
        {
            if (c == System.Drawing.Color.Transparent)
                return new ColorItem();
            ColorItem col = new ColorItem(c.R, c.G, c.B);
            return FromColorItem(col, colorHistMode);
        }

        public System.Drawing.Color ToDrawingColor()
        {
            if (R == -1 || B == -1 || G == -1)
                return System.Drawing.Color.Transparent;
            return System.Drawing.Color.FromArgb(R, G, B);
        }

        double DistanceRGB(ColorItem oColor)
        {
            double dR = R - oColor.R;
            double dG = G - oColor.G;
            double dB = B - oColor.B;
            return dR * dR + dG * dG + dB * dB;
        }

        double DistanceHSV(ColorItem oColor)
        {
            double dH = Math.Min( Math.Abs( H - oColor.H ), Math.Min( Math.Abs(H + dHSVScale - oColor.H), Math.Abs( H - oColor.H - dHSVScale) ));
            //Trace.WriteLine($"H1 = {H} : H2 = {oColor.H} : Delta = {dH}");
            double dS = S - oColor.S;
            double dV = V - oColor.V;
            return dH * dH + dS * dS + dV * dV;
        }

        double DistanceFull(ColorItem oColor)
        {
            var dRGB = DistanceRGB(oColor);
            var dHSV = DistanceHSV(oColor);
            return dRGB + dHSV;
        }

        public double Distance(ColorItem oColor, ColorDistanceEvaluationMode eEvaluation )
        {
            switch (eEvaluation)
            {
                case ColorDistanceEvaluationMode.All: return DistanceFull(oColor);
                case ColorDistanceEvaluationMode.RGB: return DistanceRGB(oColor);
                case ColorDistanceEvaluationMode.HSV: return DistanceHSV(oColor);
                default:return 0;
            }
        }

        public bool Valid => R >= 0 && G >= 0 && B >= 0;
    }
}
