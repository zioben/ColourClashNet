using ColourClashNet.Colors;
using ColourClashSupport.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ColourClashNet.Colors
{
    public static class ColorDefaults
    {
        
        public static int DefaultDitherKernelSize { get; set; } = 4;

        /// <summary>
        /// Gets or sets the default background color.
        /// </summary>
        public static Color DefaultBkgColor { get; set; } = Color.FromArgb(255, 255, 0, 255);

        /// <summary>
        /// Gets or sets the default mask color used for rendering operations.
        /// </summary>
        public static Color DefaultMaskColor { get; set; } = Color.FromArgb(255, 255, 255, 255);

        /// <summary>
        /// Gets or sets the default color used for tile layers.
        /// </summary>
        public static Color DefaultTileLayerColor { get; set; } = Color.FromArgb(255, 255, 255, 255);

        /// <summary>
        /// Gets or sets the default transparent color.
        /// </summary>
        public static Color DefaultTransparentColor { get; set; } = Color.Transparent;

        /// <summary>
        /// Default integer representation of an invalid color.
        /// </summary>
        public readonly static int DefaultInvalidColor = ColorIntExt.FromRGB(255, 255, 255, ColorIntType.Invalid);

    }
}
