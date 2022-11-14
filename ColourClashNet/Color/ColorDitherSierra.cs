using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorDitherSierra : ColorDitherErrorDiffusion
    {

        public ColorDitherSierra()
        {
            Name = "Sierra dithering";
            Description = "Sierra quantization error diffusion";
        }


        public override bool Create()
        {
            matErrorDiffusion = new double[,] {
                    { 0, 0, 0, 5, 3 },
                    { 2, 4, 5, 4, 2 },
                    { 0, 2, 3, 2, 0 },
                    };
            Normalize();
            return true;
        }
    }
}
