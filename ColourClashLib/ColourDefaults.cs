using ColourClashNet.Color;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet
{
    public static class ColorDefaults
    {
        

        /// <summary>
        /// Gets or sets the default background Color.
        /// </summary>
        public static int DefaultBkgColorInt { get; set; } = ColorIntExt.FromRGB(255,0,255);


        /// <summary>
        /// Gets or sets the default mask Color used for rendering operations.
        /// </summary>
        public static int DefaultMaskColorInt { get; set; } = ColorIntExt.FromRGB(255, 255, 255);
        

        /// <summary>
        /// Gets or sets the default Color used for tile layers.
        /// </summary>
        public static int DefaultTileColorInt { get; set; } = ColorIntExt.FromRGB(255, 0, 0);


        /// <summary>
        /// Default integer representation of an invalid Color.
        /// </summary>
        public static int DefaultInvalidColorInt { get; set; } = ColorIntExt.FromRGB(255, 255, 255, ColorInfo.Invalid);

        public static int DefaultShapeForeColorInt { get; set; } = ColorIntExt.FromRGB(255, 255, 255);
        public static int DefaultShapeBkgColorInt { get; set; } = ColorIntExt.FromRGB(0,0,0);

        public static int DefaultBlobForeColorInt { get; set; } = ColorIntExt.FromRGB(0, 0, 255);
        public static int DefaultBlobBkgColorInt { get; set; } = ColorIntExt.FromRGB(0, 0, 0);

    }
}
