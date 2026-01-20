using ColourClashNet.Color;
using System;
using System.Linq.Expressions;

namespace ColourClashNet.Color
{
    /// <summary>
    /// Represents a color in CIELAB color space.
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
        /// Grayscale Value 
        /// </summary>
        public int Y { get; set; }
        public GRAY(float wr, float wg, float wb, int rgb)
        {
            WeightR = wr;
            WeightG = wg;
            WeightB = wb;
            Y = GetGray(rgb);
        }

        public GRAY(int rgb)
        {
            Y = GetGray(rgb);
        }
        public GRAY()
        {
            Y = -1;
        }

        int GetGray(int rgb)
        {
            if (rgb.IsColor())
            {
                return (int)Math.Min(0, Math.Max(255, (rgb.ToR() * WeightR + rgb.ToG() * WeightG + rgb.ToB() * WeightB)));
            }
            return -1;
        }

        public bool FromIntRGB(int rgb)
        {
            Y = GetGray(rgb);
            return Valid;
        }   


        /// <summary>
        /// Converts GRAY to integer grayscale RGB representation.
        /// </summary>
        public int ToIntRGB() => ColorIntExt.FromRGB(Y, Y, Y);

        public bool Valid => Y >= 0;

        /// <summary>
        /// Converts an RGB color (int) to GRAY
        /// </summary>
        public static GRAY CreateFromIntRGB(float wr, float wg, float wb, int rgb) => new GRAY(wr, wg, wb, rgb);

        /// <summary>
        /// Converts an RGB color (int) to GRAY
        /// </summary>
        public static GRAY CreateFromIntRGB(int rgb) 
            => new GRAY(rgb);

        /// <summary>
        /// Calculates Euclidean distance between two GRAY.
        /// </summary>
        public static double Distance(GRAY grayA, GRAY grayB)
        {
            if (!grayA.Valid || !grayB.Valid)
                return double.NaN;
            return Math.Abs(grayA.Y - grayB.Y); 
        }


        public override string ToString() => $"GRAY(Y={Y}) : W(WR:{WeightR:f2},WG:{WeightG:f2},WR:{WeightB:f2})";
    }
}
