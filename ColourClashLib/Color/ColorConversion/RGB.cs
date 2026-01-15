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
    public struct RGB
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
        public static RGB FromIntRGB(int rgb)
        {
            if (rgb.IsColor())
            {
                return new RGB(rgb.ToR(), rgb.ToG(), rgb.ToG());
            }
            return new RGB(-1, -1, -1);
        }

        /// <summary>
        /// Converts HSV to an integer RGB representation.
        /// </summary>
        public int ToIntRGB() => ColorIntExt.FromRGB(R, G, B);

        /// <summary>
        /// Calculates Euclidean distance between two RGB colors.
        /// </summary>
        public static double Distance(RGB a, RGB b)
        {
            if (!a.Valid || !b.Valid)
                return double.NaN;                
            double dL = a.R - b.R;
            double dA = a.G - b.G;
            double dB = a.B - b.B;
            return Math.Sqrt(dL * dL + dA * dA + dB * dB);
        }


        public override string ToString() => $"RGB(R={R}, G={G}, B={B})";
    }
}
