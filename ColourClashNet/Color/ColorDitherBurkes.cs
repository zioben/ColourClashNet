using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorDitherBurkes : ColorDitherErrorDiffusion
    {

        public ColorDitherBurkes()
        {
            Name = "Burkes dithering";
            Description = "Burkes quantization error diffusion";
        }


        public override bool Create()
        {
            matErrorDiffusion = new double[,] { 
                    { 0, 0, 0, 7, 5 },
                    { 3, 5, 7, 5, 3 },
                    { 1, 3, 5, 3, 1 },
                    };
            Normalize();
            return true;
        }
    }
}
