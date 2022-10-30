using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorDitherIdentity : ColorDitherBase
    {

        public ColorDitherIdentity()
        {
            Name = "Identity dithering";
            Description = "Passthrought";
        }

        public override  bool Create()
        {
            return true;
        }

        public override int[,] Dither(int[,] oDataProcessed, List<int> oDataProcessedPalette, int[,] oDataOriginal, ColorDistanceEvaluationMode eDistanceMode)
        {
            return oDataProcessed?.Clone() as int[,] ?? null;
        }
    }
}
