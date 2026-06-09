using ColourClashNet.Color;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashLib
{
    public static class ColorDefaults
    {
        

        /// <summary>
        /// Gets or sets the default background color.
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
        /// Default integer representation of an invalid color.
        /// </summary>
        public static int DefaultInvalidColorInt { get; set; } = ColorIntExt.FromRGB(255, 255, 255, ColorInfo.Invalid);

    }
}
