using ColourClashNet.Color;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ColourClashNet.Defaults
{
    public static class ColorDefaultsGdi
    {
        

        public static System.Drawing.Color DefaultBkgColor => System.Drawing.Color.FromArgb(ColorDefaults.DefaultBkgColorInt);

        public static System.Drawing.Color DefaultMaskColor => System.Drawing.Color.FromArgb(ColorDefaults.DefaultMaskColorInt);

        public static System.Drawing.Color DefaultTileColor => System.Drawing.Color.FromArgb(ColorDefaults.DefaultTileColorInt);

        public static System.Drawing.Color DefaultTransparentColor { get; set; } = System.Drawing.Color.Transparent;

        public static System.Drawing.Color DefaultInvalidColor => System.Drawing.Color.FromArgb(ColorDefaults.DefaultInvalidColorInt);

    }
}
