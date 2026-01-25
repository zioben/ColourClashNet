using ColourClashLib.Color;
using ColourClashNet.Color;
using System;

namespace ColourClashNet.Color 
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

        public bool IsValid => L >=0 && A >=0 && B >=0;   

        public LAB(float l, float a, float b)
        {
            L = l;
            A = a;
            B = b;
        }

        public LAB(int rgb)
        {
            FromIntRGB(rgb);
        }

        /// <summary>
        /// Converts an RGB color (int) to LAB.
        /// </summary>
        public bool FromIntRGB(int rgb)
        {
            if (rgb.IsColor())
            {
                // Step 1: RGB -> XYZ
                float r = rgb.ToR() / 255f;
                float g = rgb.ToG() / 255f;
                float b = rgb.ToB() / 255f;

                r = (r > 0.04045f) ? (float)Math.Pow((r + 0.055f) / 1.055f, 2.4) : r / 12.92f;
                g = (g > 0.04045f) ? (float)Math.Pow((g + 0.055f) / 1.055f, 2.4) : g / 12.92f;
                b = (b > 0.04045f) ? (float)Math.Pow((b + 0.055f) / 1.055f, 2.4) : b / 12.92f;

                float X = r * 0.4124f + g * 0.3576f + b * 0.1805f;
                float Y = r * 0.2126f + g * 0.7152f + b * 0.0722f;
                float Z = r * 0.0193f + g * 0.1192f + b * 0.9505f;

                // Step 2: Normalize for D65 reference white
                X /= 0.95047f;
                Y /= 1.00000f;
                Z /= 1.08883f;

                // Step 3: XYZ -> LAB
                X = (X > 0.008856f) ? (float)Math.Pow(X, 1.0 / 3) : (7.787f * X) + 16f / 116f;
                Y = (Y > 0.008856f) ? (float)Math.Pow(Y, 1.0 / 3) : (7.787f * Y) + 16f / 116f;
                Z = (Z > 0.008856f) ? (float)Math.Pow(Z, 1.0 / 3) : (7.787f * Z) + 16f / 116f;

                L = 116f * Y - 16f;
                A = 500f * (X - Y);
                B = 200f * (Y - Z);
            }
            else
            {
                L = A = B = -1;
            }
            return IsValid;
        }

        /// <summary>
        /// Converts LAB to integer RGB representation.
        /// </summary>
        public int ToIntRGB()
        {
            float Y = (L + 16f) / 116f;
            float X = A / 500f + Y;
            float Z = Y - B / 200f;

            X = (float)Math.Pow(X, 3) > 0.008856f ? (float)Math.Pow(X, 3) : (X - 16f / 116f) / 7.787f;
            Y = (float)Math.Pow(Y, 3) > 0.008856f ? (float)Math.Pow(Y, 3) : (Y - 16f / 116f) / 7.787f;
            Z = (float)Math.Pow(Z, 3) > 0.008856f ? (float)Math.Pow(Z, 3) : (Z - 16f / 116f) / 7.787f;

            X *= 0.95047f;
            Y *= 1.00000f;
            Z *= 1.08883f;

            float r = X * 3.2406f + Y * -1.5372f + Z * -0.4986f;
            float g = X * -0.9689f + Y * 1.8758f + Z * 0.0415f;
            float b = X * 0.0557f + Y * -0.2040f + Z * 1.0570f;

            r = (r > 0.0031308f) ? 1.055f * (float)Math.Pow(r, 1 / 2.4) - 0.055f : 12.92f * r;
            g = (g > 0.0031308f) ? 1.055f * (float)Math.Pow(g, 1 / 2.4) - 0.055f : 12.92f * g;
            b = (b > 0.0031308f) ? 1.055f * (float)Math.Pow(b, 1 / 2.4) - 0.055f : 12.92f * b;

            int R = Math.Min(255, Math.Max(0, (int)Math.Round(r * 255)));
            int G = Math.Min(255, Math.Max(0, (int)Math.Round(g * 255)));
            int Bc = Math.Min(255, Math.Max(0, (int)Math.Round(b * 255)));

            return ColorIntExt.FromRGB(R, G, Bc);
        }

        

       

        /// <summary>
        /// Calculates Euclidean distance between two LAB colors (ΔE).
        /// </summary>
        public static double Distance(LAB labA, LAB labB)
        {
            if (!labA.IsValid || !labB.IsValid)
                return double.NaN;

            double dL = labA.L - labB.L;
            double dA = labA.A - labB.A;
            double dB = labA.B - labB.B;
            return Math.Sqrt(dL * dL + dA * dA + dB * dB);
        }

        public static LAB CreateFromIntRGB(int rgb)
            => new LAB(rgb);

        public override string ToString() => $"LAB(L={L:F2}, A={A:F2}, B={B:F2})";
    }
}
