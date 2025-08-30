using ColourClashNet.Color;
using ColourClashNet.Color.Dithering;
using ColourClashNet.Color.Transformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Tile
{
    /// <summary>
    /// Represents a tile of color data, including its position, associated color data, and processing capabilities.
    /// </summary>
    /// <remarks>This class is designed to handle color data for a specific tile, including processing the
    /// tile's color data  using a specified dithering algorithm. It also supports error evaluation based on the
    /// processed output.</remarks>
    public class TileData
    {
        internal int r { get; set; }
        internal int c { get; set; }
        internal int[,]? Tile { get; set; }
        internal double Error { get; private set; }

        internal ColorTransformReductionCluster oReduction = new ColorTransformReductionCluster()
        {
            ColorDistanceEvaluationMode = ColorDistanceEvaluationMode.RGB,
            ColorsMaxWanted = 2,
            UseClusterColorMean = false,
            TrainingLoop = 3,
        };

        internal int[,]? Process(DitherInterface oDither)
        {
            oReduction.Create(Tile,null);
            oReduction.Dithering = oDither;
            var oRet = oReduction.ProcessColors(Tile);
            Error = ColorTransformBase.EvaluateError(oRet.DataOut, Tile, oReduction.ColorDistanceEvaluationMode);
            return oRet.DataOut;
        }

    }
}
