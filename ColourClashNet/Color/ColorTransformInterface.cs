using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public interface ColorTransformInterface
    {
        void Create(int[,] oDataSource);
        void Create(Dictionary<int, int> oHistogramSource);
        void Create(ColorTransformInterface oTrasformationSource);
        int[,] Transform(int[,] oSource);
        Dictionary<int, int> oColorHistogram { get; }
        Dictionary<int, int> oColorTransformation { get; }
        List<int> oColorTransformationPalette { get; }

        int ColorsUsed {get; }
    }
}
