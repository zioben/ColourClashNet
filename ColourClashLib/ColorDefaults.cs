using ColourClashNet.Color;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ColourClashNet.Color
{
    public static class ColorDefaults
    {
        
        public static int DefaultDitherKernelSize { get; set; } = 4;

        /// <summary>
        /// Gets or sets the default background color.
        /// </summary>
        public static int DefaultBkgColorRGB { get; set; } = ColorIntExt.FromRGB(255,0,255);

        public static System.Drawing.Color DefaultBkgColor => System.Drawing.Color.FromArgb(DefaultBkgColorRGB);

        /// <summary>
        /// Gets or sets the default mask color used for rendering operations.
        /// </summary>
        public static int DefaultMaskColorRGB { get; set; } = ColorIntExt.FromRGB(255, 255, 255);
        public static System.Drawing.Color DefaultMaskColor => System.Drawing.Color.FromArgb(DefaultMaskColorRGB);

        /// <summary>
        /// Gets or sets the default color used for tile layers.
        /// </summary>
        public static int DefaultTileColorRGB { get; set; } = ColorIntExt.FromRGB(255, 0, 0);
        public static System.Drawing.Color DefaultTileColor => System.Drawing.Color.FromArgb(DefaultTileColorRGB);

        /// <summary>
        /// Gets or sets the default transparent color.
        /// </summary>
        public static System.Drawing.Color DefaultTransparentColor { get; set; } = System.Drawing.Color.Transparent;

        /// <summary>
        /// Default integer representation of an invalid color.
        /// </summary>
        public static int DefaultInvalidColorRGB { get; set; } = ColorIntExt.FromRGB(255, 255, 255);
        public static System.Drawing.Color DefaultInvalidColor => System.Drawing.Color.FromArgb(DefaultInvalidColorRGB);

    }
}
