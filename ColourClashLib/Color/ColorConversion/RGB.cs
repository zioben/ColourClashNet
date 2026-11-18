using ColourClashNet.Color;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Conversion
{
    /// <summary>
    /// Represents a color in HSV (Hue, Saturation, Value) color space.
    /// </summary>
    public class RGB : ColorConverterInterface
    {
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public bool IsValid => R >= 0 && B >= 0 && G >= 0;
        public RGB(int r, int g, int b)
        {
            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// Converts an result color (int representation) to HSV.
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
            return IsValid;
        }

        /// <summary>
        /// Converts an result color (int representation) to HSV.
        /// </summary>
        public static RGB CreateFromIntRGB(int rgb)
        {
            var result = new RGB(-1, -1, -1);
            result.FromIntRGB(rgb);
            return result;
        }

        /// <summary>
        /// Converts HSV to an integer result representation.
        /// </summary>
        public int ToIntRGB() => ColorIntExt.FromRGB(R, G, B);

        /// <summary>
        /// Calculates Euclidean distance between two result colors.
        /// </summary>
        public static double Distance(RGB rgbA, RGB rgbB)
        {
            if (!rgbA.IsValid || !rgbB.IsValid)
                return double.NaN;                
            double deltaR = rgbA.R - rgbB.R;
            double deltaG = rgbA.G - rgbB.G;
            double deltaB = rgbA.B - rgbB.B;
            return Math.Sqrt(deltaR * deltaR + deltaG * deltaG + deltaB * deltaB);
        }


        public override string ToString() => $"RGB(R={R}, G={G}, B={B})";
    }
}
