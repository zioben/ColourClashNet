using ColourClashNet.Color;
using ColourClashNet.Defaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashLib.Color 
{
    /// <summary>
    /// Represents a color in HSV (Hue, Saturation, Value) color space.
    /// </summary>
    public class RGB : ColorConverterInterface
    {
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public bool Valid => R >= 0 && B >= 0 && G >= 0;
        public RGB(int r, int g, int b)
        {
            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// Converts an RGB color (int representation) to HSV.
        /// </summary>
        public bool FromIntRGB(int rgb)
        {
            if (rgb.IsColor())
            {
                R = rgb.ToR();
                G = rgb.ToG();
                B = rgb.ToB();
            }
            else
            {
                R = G = B = -1;
            }
            return Valid;
        }

        /// <summary>
        /// Converts an RGB color (int representation) to HSV.
        /// </summary>
        public static RGB CreateFromIntRGB(int rgb)
        {
            var RGB = new RGB(-1, -1, -1);
            RGB.FromIntRGB(rgb);
            return RGB;
        }

        /// <summary>
        /// Converts HSV to an integer RGB representation.
        /// </summary>
        public int ToIntRGB() => ColorIntExt.FromRGB(R, G, B);

        /// <summary>
        /// Calculates Euclidean distance between two RGB colors.
        /// </summary>
        public static double Distance(RGB rgbA, RGB rgbB)
        {
            if (!rgbA.Valid || !rgbB.Valid)
                return double.NaN;                
            double dL = rgbA.R - rgbB.R;
            double dA = rgbA.G - rgbB.G;
            double dB = rgbA.B - rgbB.B;
            return Math.Sqrt(dL * dL + dA * dA + dB * dB);
        }


        public override string ToString() => $"RGB(R={R}, G={G}, B={B})";
    }
}
