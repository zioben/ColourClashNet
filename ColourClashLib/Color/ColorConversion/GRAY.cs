using ColourClashNet.Color2;
using System;
using System.Linq.Expressions;

namespace ColourClashNet.Color
{
    /// <summary>
    /// Represents a color in CIELAB color space.
    /// </summary>
    public struct GRAY
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
       
        int GetGray(int rgb)
        {
            if (ColorIntExt.GetColorInfo(rgb) == ColorIntType.Invalid)
            {
                return -1;
            }
            return (int)Math.Min(0, Math.Max(255, (rgb.ToR() * WeightR + rgb.ToG() * WeightG + rgb.ToB() * WeightB)));
        }

        public GRAY(float wr, float wg, float wb, int rgb )
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

        /// <summary>
        /// Converts an RGB color (int) to GRAY
        /// </summary>
        public static GRAY FromIntRGB(float wr, float wg, float wb, int rgb) => new GRAY(wr, wg, wb, rgb);

        /// <summary>
        /// Converts an RGB color (int) to GRAY
        /// </summary>
        public static GRAY FromIntRGB(int rgb) => new GRAY(rgb);

        /// <summary>
        /// Converts GRAY to integer grayscale RGB representation.
        /// </summary>
        public int ToIntRGB() => ColorIntExt.FromRGB(Y, Y, Y);

        public bool Valid => Y >= 0;


        /// <summary>
        /// Calculates Euclidean distance between two GRAY.
        /// </summary>
        public static double Distance(GRAY a, GRAY b)
        {
            if (!a.Valid || !b.Valid)
                return double.NaN;
            return Math.Abs(a.Y - b.Y); 
        }


        public override string ToString() => $"GRAY(Y={Y}) : W(WR:{WeightR:f2},WG:{WeightG:f2},WR:{WeightB:f2})";
    }
}
