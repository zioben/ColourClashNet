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
    public static class ColorDefaultsGdi
    {
        

        public static System.Drawing.Color DefaultBkgColor { get; set; } = System.Drawing.Color.FromArgb(ColorDefaults.DefaultBkgColorInt);

        public static System.Drawing.Color DefaultMaskColor { get; set; } = System.Drawing.Color.FromArgb(ColorDefaults.DefaultMaskColorInt);

        public static System.Drawing.Color DefaultTileColor { get; set; } = System.Drawing.Color.FromArgb(ColorDefaults.DefaultTileColorInt);

        public static System.Drawing.Color DefaultTransparentColor { get; set; } = System.Drawing.Color.Transparent;

        public static System.Drawing.Color DefaultInvalidColor { get; set; } = System.Drawing.Color.FromArgb(ColorDefaults.DefaultInvalidColorInt);

        public static System.Drawing.Color DefaultShapeForeColor { get; set; } = System.Drawing.Color.FromArgb(ColorDefaults.DefaultShapeForeColorInt);

        public static System.Drawing.Color DefaultShapeBkgColor { get; set; } = System.Drawing.Color.FromArgb(ColorDefaults.DefaultShapeBkgColorInt);

        public static System.Drawing.Color DefaultBlobForeColor { get; set; } = System.Drawing.Color.FromArgb(ColorDefaults.DefaultBlobForeColorInt);

        public static System.Drawing.Color DefaultBlobBkgColor { get; set; } = System.Drawing.Color.FromArgb(ColorDefaults.DefaultBlobBkgColorInt);


    }
}
