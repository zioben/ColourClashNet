using ColourClashNet.Color;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using ColourClashNet;


namespace ColourClashNet.Defaults
{
    /// <summary>
    /// GDI+ (<see cref="System.Drawing.Color"/>) versions of the default colors defined in <see cref="ColorDefaults"/>.
    /// </summary>
    public static class ColorDefaultsGdi
    {
        

        /// <summary>
        /// Default background color.
        /// </summary>
        public static System.Drawing.Color DefaultBkgColor { get; set; } = System.Drawing.Color.FromArgb(ColorDefaults.DefaultBkgColorInt);

        /// <summary>
        /// Default mask color.
        /// </summary>
        public static System.Drawing.Color DefaultMaskColor { get; set; } = System.Drawing.Color.FromArgb(ColorDefaults.DefaultMaskColorInt);

        /// <summary>
        /// Default tile grid color.
        /// </summary>
        public static System.Drawing.Color DefaultTileColor { get; set; } = System.Drawing.Color.FromArgb(ColorDefaults.DefaultTileColorInt);

        /// <summary>
        /// Transparent color.
        /// </summary>
        public static System.Drawing.Color DefaultTransparentColor { get; set; } = System.Drawing.Color.Transparent;

        /// <summary>
        /// Color used to display invalid pixels.
        /// </summary>
        public static System.Drawing.Color DefaultInvalidColor { get; set; } = System.Drawing.Color.FromArgb(ColorDefaults.DefaultInvalidColorInt);

        /// <summary>
        /// Blob shape foreground color.
        /// </summary>
        public static System.Drawing.Color DefaultShapeForeColor { get; set; } = System.Drawing.Color.FromArgb(ColorDefaults.DefaultShapeForeColorInt);

        /// <summary>
        /// Blob shape background color.
        /// </summary>
        public static System.Drawing.Color DefaultShapeBkgColor { get; set; } = System.Drawing.Color.FromArgb(ColorDefaults.DefaultShapeBkgColorInt);

   
    }
}
