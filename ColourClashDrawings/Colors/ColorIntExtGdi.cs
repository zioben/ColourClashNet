using ColourClashNet.Color;
using ColourClashNet.Defaults;
using ColourClashNet.Imaging;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color
{


    /// <summary>
    /// Provides extension methods and default values for working with integer-based color representations.
    /// </summary>
    /// <remarks>This static class includes methods for converting integers to and from color representations,
    /// manipulating color components, and calculating color distances and means. It also defines default  colors for
    /// specific use cases, such as background, mask, and transparency.  The integer-based color representation assumes
    /// a 32-bit structure where the lower 24 bits represent  the RGB components, and the upper 8 bits may encode
    /// additional metadata (e.g., color type).</remarks>
    public static class ColorIntExtGdi
    {


        /// <summary>
        /// Converts an integer representation of a color to a <see cref="System.Drawing.Color"/>. 
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static System.Drawing.Color ToDrawingColor(this int rgb)
        {
            switch (ColorIntExt.GetColorInfo(rgb))
            {
                case ColorInfo.IsColor:
                    {
                        unchecked
                        {
                            return System.Drawing.Color.FromArgb(rgb | (int)0xFF_00_00_00);
                        }
                    }
                //case ColorIntType.IsBkg:
                //    {
                //        return DefaultBkgColor;
                //    }
                case ColorInfo.IsMask:
                    {
                        return ColorDefaults.DefaultMaskColorInt.ToDrawingColor();
                    }
                case ColorInfo.IsTile:
                    {
                        return ColorDefaults.DefaultTileColorInt.ToDrawingColor();
                    }
                default:
                    {
                        return System.Drawing.Color.Transparent;
                    }
            }
        }

      
        /// <summary>
        /// Converts a <see cref="System.Drawing.Color"/> to its integer representation.
        /// </summary>
        /// <param name="oColor"></param>
        /// <returns></returns>
        public static int FromDrawingColor(System.Drawing.Color oColor)
        {
            if (oColor == System.Drawing.Color.Transparent)
                return ColorDefaults.DefaultInvalidColorInt;

            return ColorIntExt.FromRGB(oColor.R, oColor.G, oColor.B);
        }

    }
}
