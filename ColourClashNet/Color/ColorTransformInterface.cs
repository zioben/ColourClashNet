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
        void Create(SortedList<int, int> oListHistogramSource);
        void Create(ColorTransformInterface oTrasformationSource);
        int[,] Transform(int[,] oSource);
        SortedList<int, int> ListColorHistogram { get; }
        SortedList<int, int> ListColorTransformation { get; }

        int ColorsUsed{get; }
    }
}
