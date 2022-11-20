using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public interface ColorTransformInterface
    {
        ColorTransform Type { get; }
        string Description { get; }
        void Create(int[,]? oDataSource);
        void Create(Dictionary<int, int>? oHistogramSource);
        void Create(List<int> oColorPalette);
        ColorDitherInterface? Dithering { get; set; }
        int[,]? TransformAndDither(int[,]? oSource);
        Dictionary<int, int> oColorHistogram { get; }
        Dictionary<int, int> oColorTransformationMap { get; }
        List<int> oColorTransformationPalette { get; }

        int ColorsUsed {get; }
    }
}
