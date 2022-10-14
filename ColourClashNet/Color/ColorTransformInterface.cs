using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public interface ColorTransformInterface
    {
        void Create(ColorItem[,] oDataSource);
        void Create(Dictionary<ColorItem, int> oDictHistogramSource);
        void Create(ColorTransformInterface oTrasformationSource);
        ColorItem[,] Transform(ColorItem[,] oSource);
        Dictionary<ColorItem, int> DictColorHistogram { get; }
        Dictionary<ColorItem, ColorItem> DictColorTransformation { get; }

        int ColorsUsed{get; }
    }
}
