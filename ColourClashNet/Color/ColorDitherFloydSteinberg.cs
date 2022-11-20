using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorDitherFloydSteinberg : ColorDitherErrorDiffusion
    {

        public ColorDitherFloydSteinberg()
        {
            Type = ColorDithering.FloydSteinberg;
            Description = "Floyd Steinberg quantization error diffusion";
        }


        public override bool Create()
        {
            matErrorDiffusion = new double[,] 
            { 
                { 0, 0, 7 }, 
                { 3, 5, 1 } 
            };
            Normalize();
            return true;
        }
    }
}
