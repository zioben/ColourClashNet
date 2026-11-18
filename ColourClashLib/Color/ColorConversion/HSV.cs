using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
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
    public class HSV : ColorConverterInterface
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

        /// <inheritdoc/>
        public bool IsValid => H >= 0 && S >= 0 && V >= 0;

        /// <summary>
        /// Creates an HSV color from its components.
        /// </summary>
        public HSV(float h, float s, float v)
        {
            H = h;
            S = s;
            V = v;
        }

        /// <summary>
        /// Creates an HSV color from an integer RGB color.
        /// </summary>
        public HSV(int rgb)
        {
            FromIntRGB(rgb);
        }

        /// <summary>
        /// Converts an RGB color (int representation) to HSV.
        /// <para/>
        /// Non-color values produce an invalid HSV (all components set to -1).
        /// </summary>
        public bool FromIntRGB(int rgb)
        {
            if (rgb.IsColor())
            {
                float r = rgb.ToR() / 255f;
                float g = rgb.ToG() / 255f;
                float b = rgb.ToB() / 255f;

                float max = Math.Max(r, Math.Max(g, b));
                float min = Math.Min(r, Math.Min(g, b));
                float delta = max - min;

                // Hue depends on which channel is the maximum: each one covers a 120 degree sector
                float h = 0;
                if (delta > 0)
                {
                    if (r == max)
                        h = 60 * (((g - b) / delta) % 6);
                    else if (g == max)
                        h = 60 * (((b - r) / delta) + 2);
                    else
                        h = 60 * (((r - g) / delta) + 4);
                }

                if (h < 0) h += 360;

                H = h;
                S = max == 0 ? 0 : (delta / max) * 100f;
                V = max * 100f;
            }
            else
            {
                H = -1;
                S = -1;
                V = -1;
            }

            return IsValid;
        }

        /// <summary>
        /// Converts HSV to an integer RGB representation.
        /// </summary>
        public int ToIntRGB()
        {
            float h = H;
            float s = S / 100f;
            float v = V / 100f;

            if (s == 0)
            {
                int gray = (int)Math.Round(v * 255);
                return ColorIntExt.FromRGB(gray, gray, gray);
            }

            // Standard sector based conversion:
            // c = chroma, x = second largest component, m = offset added to match the value
            float c = v * s;
            float x = c * (1 - Math.Abs((h / 60f) % 2 - 1));
            float m = v - c;

            float r1 = 0, g1 = 0, b1 = 0;

            if (h >= 0 && h < 60) { r1 = c; g1 = x; b1 = 0; }
            else if (h < 120) { r1 = x; g1 = c; b1 = 0; }
            else if (h < 180) { r1 = 0; g1 = c; b1 = x; }
            else if (h < 240) { r1 = 0; g1 = x; b1 = c; }
            else if (h < 300) { r1 = x; g1 = 0; b1 = c; }
            else { r1 = c; g1 = 0; b1 = x; }

            int r = (int)Math.Round((r1 + m) * 255);
            int g = (int)Math.Round((g1 + m) * 255);
            int b = (int)Math.Round((b1 + m) * 255);

            r = Math.Clamp(r, 0, 255);
            g = Math.Clamp(g, 0, 255);
            b = Math.Clamp(b, 0, 255);

            return ColorIntExt.FromRGB(r, g, b);
        }

        /// <summary>
        /// Calculates the squared distance between two HSV colors.
        /// <para/>
        /// The hue difference is circular (the shortest arc on the color wheel).
        /// </summary>
        public static double Distance(HSV hsvA, HSV hsvB)
        {
            if (!hsvA.IsValid || !hsvB.IsValid)
                return double.NaN;
            double dh = Math.Min(Math.Abs(hsvA.H - hsvB.H), 360 - Math.Abs(hsvA.H - hsvB.H));
            double ds = hsvA.S - hsvB.S;
            double dv = hsvA.V - hsvB.V;
            return dh * dh + ds * ds + dv * dv;
        }

        /// <summary>
        /// Creates an HSV instance from an integer RGB representation.
        /// </summary>
        /// <param name="rgb">Packed RGB color</param>
        /// <returns>The HSV color; invalid if <paramref name="rgb"/> is not a color.</returns>
        public static HSV CreateFromIntRGB(int rgb)
            => new HSV(rgb);

        public override string ToString() => $"HSV(H={H:F2}, S={S:F2}, V={V:F2})";


       
    }
}


