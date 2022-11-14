using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorDitherStucki : ColorDitherErrorDiffusion
    {

        public ColorDitherStucki()
        {
            Name = "Stucki dithering";
            Description = "Stucki quantization error diffusion";
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
