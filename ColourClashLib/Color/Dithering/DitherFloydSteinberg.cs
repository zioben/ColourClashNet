using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColourClashNet.Color;

namespace ColourClashNet.Color.Dithering
{
    public class DitherFloydSteinberg : DitherErrorDiffusion
    {

        public DitherFloydSteinberg()
        {
            Type = ColorDithering.FloydSteinberg;
            Description = "Floyd Steinberg quantization error diffusion";
        }


        public override DitherInterface Create()
        {
            matErrorDiffusion = new double[,]
            {
                { 0, 0, 7 },
                { 3, 5, 1 }
            };
            Normalize();
            return this;
        }
    }
}
