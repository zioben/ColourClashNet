using ColourClashNet.Colors.Dithering;
using ColourClashNet.Colors.Transformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorTile
    {
        internal int r { get; set; }
        internal int c { get; set; }
        internal int[,] TileData { get; set; }
        //internal int[,] TileDataProc { get; set; }

        internal ColorTransformReductionCluster oReduction = new ColorTransformReductionCluster()
        {
            ColorDistanceEvaluationMode = ColorDistanceEvaluationMode.RGB,
            ColorsMax = 2,
            UseClusterColorMean = false,
            TrainingLoop = 6,
        };

        internal int[,] Process(DitherInterface oDither)
        {
            oReduction.Create(TileData);
            oReduction.Dithering = oDither;
            return oReduction.TransformAndDither(TileData);
        }

    }
}
