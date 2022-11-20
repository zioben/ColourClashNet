using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorDitherScanLine : ColorDitherErrorDiffusion
    {

        public ColorDitherScanLine()
        {
            Type = ColorDithering.Linear;
            Description = "Scanline quantization error diffusion";
        }


        public override bool Create()
        {
            matErrorDiffusion = new double[,] { 
                    { 0, 0, 0, 3, 1 }
                    };
            Normalize();
            return true;
        }
    }
}
