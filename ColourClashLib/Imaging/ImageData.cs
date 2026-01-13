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
    public partial class ImageData
    {
        public string Name { get; set; } = "";
        public int[,]? Data { get; set; }
        public Palette Palette { get; set; } = new Palette();
        public Histogram Histogram { get; set; } = new Histogram();

        public int Width
        {
            get
            {
                if (Data == null) return 0;
                return Data.GetLength(0);
            }
        }

        public int Height
        {
            get
            {
                if (Data == null) return 0;
                return Data.GetLength(1);
            }
        }

        public int PixelCount
        {
            get
            {
                return Width * Height;
            }
        }   

        public int ColorCount
        {
            get
            {
                return Palette.Count;
            }
        }

    }
}
