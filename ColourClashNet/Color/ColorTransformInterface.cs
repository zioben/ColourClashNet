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
        void Create(HashSet<int> oColorPalette);
        ColorDitherInterface? Dithering { get; set; }
        int[,]? TransformAndDither(int[,]? oSource);
        Dictionary<int, int> ColorHistogram { get; }
        Dictionary<int, int> ColorTransformationMap { get; }
        HashSet<int> ColorTransformationPalette { get; }
        int ColorsUsed {get; }
    }
}
