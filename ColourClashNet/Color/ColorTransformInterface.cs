using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color
{
    public interface ColorTransformInterface
    {
        void Create(ColorItem[,] oSource );
        void Create(Dictionary<ColorItem, int> oSourceHistogram );
        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; set; }
        ColorItem[,] Transform( ColorItem[,] oSource );
        Dictionary<ColorItem, int> DictHistogram { get; }
        Dictionary<ColorItem, ColorItem> DictTransform { get; }

    }
}
