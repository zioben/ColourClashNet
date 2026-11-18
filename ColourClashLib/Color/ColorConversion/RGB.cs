using ColourClashNet.Color;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Conversion
{
    /// <summary>
    /// Represents a color with separate red, green and blue components [0..255].
    /// </summary>
    public class RGB : ColorConverterInterface
    {
        /// <summary>Red component; negative when invalid.</summary>
        public int R { get; set; }
        /// <summary>Green component; negative when invalid.</summary>
        public int G { get; set; }
        /// <summary>Blue component; negative when invalid.</summary>
        public int B { get; set; }

        /// <inheritdoc/>
        public bool IsValid => R >= 0 && B >= 0 && G >= 0;

        /// <summary>
        /// Creates an RGB color from its components.
        /// </summary>
        public RGB(int r, int g, int b)
        {
            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// Splits an integer RGB color into its components; non-color values become invalid (-1).
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
        /// Creates an RGB instance from an integer RGB representation.
        /// </summary>
        public static RGB CreateFromIntRGB(int rgb)
        {
            var result = new RGB(-1, -1, -1);
            result.FromIntRGB(rgb);
            return result;
        }

        /// <summary>
        /// Packs the components into an integer RGB representation.
        /// </summary>
        public int ToIntRGB() => ColorIntExt.FromRGB(R, G, B);

        /// <summary>
        /// Calculates the Euclidean distance between two RGB colors.
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
