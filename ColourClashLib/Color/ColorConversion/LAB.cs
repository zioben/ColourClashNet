using ColourClashNet.Color;
using System;

namespace ColourClashNet.Color.Conversion
{
    /// <summary>
    /// Represents a color in CIELAB color space.
    /// </summary>
    public class LAB : ColorConverterInterface
    {
        /// <summary>
        /// Lightness [0,100]
        /// </summary>
        public float L { get; set; }

        /// <summary>
        /// Green-Red component [-128,127]
        /// </summary>
        public float A { get; set; }

        /// <summary>
        /// Blue-Yellow component [-128,127]
        /// </summary>
        public float B { get; set; }

        /// <inheritdoc/>
        public bool IsValid => 
            L >= 0f && L <= 100f &&
            A >= -128f && A <= 127f &&
            B >= -128f && B <= 127f;

        /// <summary>
        /// Creates a LAB color from its components.
        /// </summary>
        public LAB(float l, float a, float b)
        {
            L = l;
            A = a;
            B = b;
        }

        /// <summary>
        /// Creates a LAB color from an integer RGB color.
        /// </summary>
        public LAB(int rgb)
        {
            FromIntRGB(rgb);
        }

        /// <summary>
        /// Converts an RGB color (int) to LAB (sRGB, D65 reference white).
        /// <para/>
        /// Non-color values produce an invalid LAB (L = -1).
        /// </summary>
        public bool FromIntRGB(int rgb)
        {
            if (rgb.IsColor())
            {
                // Step 1: RGB -> linear RGB (inverse sRGB gamma) -> XYZ
                float r = rgb.ToR() / 255f;
                float g = rgb.ToG() / 255f;
                float b = rgb.ToB() / 255f;

                r = (r > 0.04045f) ? (float)Math.Pow((r + 0.055f) / 1.055f, 2.4) : r / 12.92f;
                g = (g > 0.04045f) ? (float)Math.Pow((g + 0.055f) / 1.055f, 2.4) : g / 12.92f;
                b = (b > 0.04045f) ? (float)Math.Pow((b + 0.055f) / 1.055f, 2.4) : b / 12.92f;

                float x = r * 0.4124f + g * 0.3576f + b * 0.1805f;
                float y = r * 0.2126f + g * 0.7152f + b * 0.0722f;
                float z = r * 0.0193f + g * 0.1192f + b * 0.9505f;

                // Step 2: Normalize for D65 reference white
                x /= 0.95047f;
                y /= 1.00000f;
                z /= 1.08883f;

                // Step 3: XYZ -> LAB
                x = (x > 0.008856f) ? (float)Math.Pow(x, 1.0 / 3) : (7.787f * x) + 16f / 116f;
                y = (y > 0.008856f) ? (float)Math.Pow(y, 1.0 / 3) : (7.787f * y) + 16f / 116f;
                z = (z > 0.008856f) ? (float)Math.Pow(z, 1.0 / 3) : (7.787f * z) + 16f / 116f;

                L = 116f * y - 16f;
                A = 500f * (x - y);
                B = 200f * (y - z);
            }
            else
            {
                L = -1;
                A = B = 0;
            }
            return IsValid;
        }

        /// <summary>
        /// Converts LAB to integer RGB representation.
        /// </summary>
        public int ToIntRGB()
        {
            // Inverse of FromIntRGB: LAB -> XYZ (D65) -> linear RGB -> sRGB gamma -> 8 bit
            float y = (L + 16f) / 116f;
            float x = A / 500f + y;
            float z = y - B / 200f;

            x = (float)Math.Pow(x, 3) > 0.008856f ? (float)Math.Pow(x, 3) : (x - 16f / 116f) / 7.787f;
            y = (float)Math.Pow(y, 3) > 0.008856f ? (float)Math.Pow(y, 3) : (y - 16f / 116f) / 7.787f;
            z = (float)Math.Pow(z, 3) > 0.008856f ? (float)Math.Pow(z, 3) : (z - 16f / 116f) / 7.787f;

            x *= 0.95047f;
            y *= 1.00000f;
            z *= 1.08883f;

            float r = x * 3.2406f + y * -1.5372f + z * -0.4986f;
            float g = x * -0.9689f + y * 1.8758f + z * 0.0415f;
            float b = x * 0.0557f + y * -0.2040f + z * 1.0570f;

            r = (r > 0.0031308f) ? 1.055f * (float)Math.Pow(r, 1 / 2.4) - 0.055f : 12.92f * r;
            g = (g > 0.0031308f) ? 1.055f * (float)Math.Pow(g, 1 / 2.4) - 0.055f : 12.92f * g;
            b = (b > 0.0031308f) ? 1.055f * (float)Math.Pow(b, 1 / 2.4) - 0.055f : 12.92f * b;

            int red = Math.Min(255, Math.Max(0, (int)Math.Round(r * 255)));
            int green = Math.Min(255, Math.Max(0, (int)Math.Round(g * 255)));
            int blue = Math.Min(255, Math.Max(0, (int)Math.Round(b * 255)));

            return ColorIntExt.FromRGB(red, green, blue);
        }

        /// <summary>
        /// Calculates the Euclidean distance between two LAB colors (CIE76 ΔE).
        /// </summary>
        public static double Distance(LAB labA, LAB labB)
        {
            if (!labA.IsValid || !labB.IsValid)
                return double.NaN;

            double deltaL = labA.L - labB.L;
            double deltaA = labA.A - labB.A;
            double deltaB = labA.B - labB.B;
            return Math.Sqrt(deltaL * deltaL + deltaA * deltaA + deltaB * deltaB);
        }

        /// <summary>
        /// Creates a LAB instance from an integer RGB representation.
        /// </summary>
        public static LAB CreateFromIntRGB(int rgb)
            => new LAB(rgb);

        public override string ToString() => $"LAB(L={L:F2}, A={A:F2}, B={B:F2})";
    }
}
