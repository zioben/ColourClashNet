using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColourClashNet.Color;

namespace ColourClashNet.Color.Dithering
{
    public class DitherAtkinson : DitherErrorDiffusion
    {

        public DitherAtkinson()
        {
            Type = ColorDithering.Atkinson;
            Description = "Atkinson quantization error diffusion";
        }

        public override bool Create()
        {
            matErrorDiffusion = new double[,] {
                    { 0, 0, 0, 1, 1 },
                    { 0, 1, 1, 1, 0 },
                    { 0, 0, 1, 0, 0 },
                    };
            Normalize(8);
            return true;
        }
    }
}
