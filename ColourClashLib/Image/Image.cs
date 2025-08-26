using ColourClashLib.Color;
using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashLib.Image
{
    [Serializable]
    public class Image
    {
        public string Name { get; set; } = "";
        public int[,]? ImageData { get; set; }
        public ColorPalette? Palette { get; set; } = new ColorPalette();    
        public ColorHistogram? Histogram { get; set; } = new ColorHistogram();
        public ColorPalette? BackgroundPalette { get; set; } = new ColorPalette();

        public int BackgroundValue = ColorIntExt.FromDrawingColor(  ColorDefaults.DefaultBkgColor);

    }


}
