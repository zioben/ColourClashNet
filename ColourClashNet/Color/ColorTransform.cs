using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color
{
    public interface ColorTransform
    {
        void Create(ColorItem[,] oSource, ColorItem oBackColor );
        void Create(Dictionary<ColorItem, int> oSourceHistogram, ColorItem oBackColor);

        ColorItem[,] Transform( ColorItem[,] oSource);
        ColorItem BackColor { get; }
        ColorItem BackColorTransform { get; }

        Dictionary<ColorItem, int> DictHistogram { get; }

        Dictionary<ColorItem, ColorItem> DictTransform { get; }

    }
}
