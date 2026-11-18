using ColourClashNet.Color;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet
{
    /// <summary>
    /// Global default colors (packed RGB integers) shared by processing and rendering code.
    /// </summary>
    public static class ColorDefaults
    {
        /// <summary>
        /// Gets or sets the default background color (magenta).
        /// </summary>
        public static int DefaultBkgColorInt { get; set; } = ColorIntExt.FromRGB(255,0,255);


        /// <summary>
        /// Gets or sets the default mask color used for rendering operations.
        /// </summary>
        public static int DefaultMaskColorInt { get; set; } = ColorIntExt.FromRGB(255, 255, 255);
        

        /// <summary>
        /// Gets or sets the default color used for tile layers.
        /// </summary>
        public static int DefaultTileColorInt { get; set; } = ColorIntExt.FromRGB(255, 0, 0);


        /// <summary>
        /// Gets or sets the integer representation of an invalid color (flagged with <see cref="ColorInfo.Invalid"/>).
        /// </summary>
        public static int DefaultInvalidColorInt { get; set; } = ColorIntExt.FromRGB(255, 255, 255, ColorInfo.Invalid);

        /// <summary>
        /// Gets or sets the foreground color used to draw blob shapes.
        /// </summary>
        public static int DefaultShapeForeColorInt { get; set; } = ColorIntExt.FromRGB(255, 255, 255);

        /// <summary>
        /// Gets or sets the background color used to draw blob shapes.
        /// </summary>
        public static int DefaultShapeBkgColorInt { get; set; } = ColorIntExt.FromRGB(0,0,0);

    }
}
