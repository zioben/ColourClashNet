using ColourClashNet.Color;
using ColourClashNet.Defaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Imaging
{
    [Serializable]
    public class ImageData
    {
        public string Name { get; set; } = "";
        public int[,]? Image { get; set; }
        public Palette? Palette { get; set; } = new Palette();    
        public Histogram? Histogram { get; set; } = new Histogram();
        public Palette? BackgroundPalette { get; set; } = new Palette();

        public int BackgroundValue = ColorIntExt.FromDrawingColor(  ColorDefaults.DefaultBkgColor);

    }


}
