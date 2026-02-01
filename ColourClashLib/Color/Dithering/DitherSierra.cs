using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColourClashNet.Color;

namespace ColourClashNet.Color.Dithering
{
    public class DitherSierra : DitherErrorDiffusion
    {

        public DitherSierra()
        {
            Type = ColorDithering.Sierra;
            Description = "Sierra quantization error diffusion";
        }


        public override DitherInterface Create()
        {
            matErrorDiffusion = new double[,] {
                    { 0, 0, 0, 5, 3 },
                    { 2, 4, 5, 4, 2 },
                    { 0, 2, 3, 2, 0 },
                    };
            Normalize();
            return this;
        }
    }
}
