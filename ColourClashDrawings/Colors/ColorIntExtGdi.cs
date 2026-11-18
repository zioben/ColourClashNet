using ColourClashLib;
using ColourClashNet.Color;
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
    /// Conversions between packed integer colors and <see cref="System.Drawing.Color"/>.
    /// </summary>
    public static class ColorIntExtGdi
    {
        /// <summary>
        /// Converts a packed integer color to an opaque <see cref="System.Drawing.Color"/>.
        /// <para/>
        /// Mask and tile values are shown with their default colors; other non-color values become transparent.
        /// </summary>
        /// <param name="rgb">Packed color</param>
        /// <returns>The GDI+ color.</returns>
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
        /// Converts a <see cref="System.Drawing.Color"/> to a packed integer color (alpha is ignored).
        /// </summary>
        /// <param name="color">GDI+ color</param>
        /// <returns>The packed color, or an invalid color for <see cref="System.Drawing.Color.Transparent"/>.</returns>
        public static int FromDrawingColor(System.Drawing.Color color)
        {
            if (color == System.Drawing.Color.Transparent)
                return ColorDefaults.DefaultInvalidColorInt;

            return ColorIntExt.FromRGB(color.R, color.G, color.B);
        }

    }
}
