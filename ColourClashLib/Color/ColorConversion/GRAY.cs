using ColourClashLib;
using ColourClashNet.Color;
using System;
using System.Linq.Expressions;

namespace ColourClashNet.Color.Conversion
{
    /// <summary>
    /// Represents a color as a single gray (luminance) value computed with configurable channel weights.
    /// </summary>
    public class GRAY : ColorConverterInterface
    {
        /// <summary>
        /// Red Weight [0..1]
        /// </summary>
        public float WeightR { get; set; } = 1.0f / 3;

        /// <summary>
        /// Green Weight [0..1]
        /// </summary>
        public float WeightG { get; set; } = 1.0f / 3;

        /// <summary>
        /// Blue Weight [0..1]
        /// </summary>
        public float WeightB { get; set; } = 1.0f / 3;

        /// <summary>
        /// Grayscale value [0..255]; negative when invalid.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Creates a gray value from an integer RGB color using custom channel weights.
        /// </summary>
        public GRAY(float weightR, float weightG, float weightB, int rgb)
        {
            WeightR = weightR;
            WeightG = weightG;
            WeightB = weightB;
            Y = GetGray(rgb);
        }

        /// <summary>
        /// Creates a gray value from RGB components using custom channel weights.
        /// </summary>
        public GRAY(float weightR, float weightG, float weightB, int r, int g, int b)
        {
            WeightR = weightR;
            WeightG = weightG;
            WeightB = weightB;
            Y = GetGray(ColorIntExt.FromRGB(r, g, b));
        }

        /// <summary>
        /// Creates a gray value from RGB components using equal channel weights.
        /// </summary>
        public GRAY(int r, int g, int b)
        {
            Y = GetGray(ColorIntExt.FromRGB(r, g, b));
        }

        /// <summary>
        /// Creates a gray value from an integer RGB color using equal channel weights.
        /// </summary>
        public GRAY(int rgb)
        {
            Y = GetGray(rgb);
        }

        /// <summary>
        /// Creates an invalid gray value.
        /// </summary>
        public GRAY()
        {
            Y = -1;
        }

        /// <summary>
        /// Weighted sum of the RGB channels, or an invalid value if <paramref name="rgb"/> is not a color.
        /// </summary>
        int GetGray(int rgb)
        {
            if (rgb.IsColor())
            {
                return (int)Math.Max(0, Math.Min(255, (rgb.ToR() * WeightR + rgb.ToG() * WeightG + rgb.ToB() * WeightB)));
            }
            return ColorDefaults.DefaultInvalidColorInt;
        }

        /// <inheritdoc/>
        public bool FromIntRGB(int rgb)
        {
            Y = GetGray(rgb);
            return IsValid;
        }


        /// <summary>
        /// Converts GRAY to integer grayscale RGB representation.
        /// </summary>
        public int ToIntRGB() => ColorIntExt.FromRGB(Y, Y, Y);

        /// <inheritdoc/>
        public bool IsValid => Y >= 0;

        /// <summary>
        /// Converts an RGB color (int) to GRAY using custom channel weights.
        /// </summary>
        public static GRAY CreateFromIntRGB(float weightR, float weightG, float weightB, int rgb) => new GRAY(weightR, weightG, weightB, rgb);

        /// <summary>
        /// Converts an RGB color (int) to GRAY
        /// </summary>
        public static GRAY CreateFromIntRGB(int rgb) 
            => new GRAY(rgb);

        /// <summary>
        /// Calculates the absolute difference between two gray values.
        /// </summary>
        public static double Distance(GRAY grayA, GRAY grayB)
        {
            if (!grayA.IsValid || !grayB.IsValid)
                return double.NaN;
            return Math.Abs(grayA.Y - grayB.Y); 
        }


        public override string ToString() => $"GRAY(Y={Y}) : W(WR:{WeightR:f2},WG:{WeightG:f2},WR:{WeightB:f2})";
    }
}
