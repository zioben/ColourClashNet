using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColourClashNet.Color;

namespace ColourClashNet.Color.Dithering
{
    public class DitherJarvisJudiceNinke : DitherErrorDiffusion
    {

        public DitherJarvisJudiceNinke()
        {
            Type = ColorDithering.JarvisJudiceNinke;
            Description = "Jarvis, Judice and Ninke quantization error diffusion";
        }


        public override DitherInterface Create()
        {
            matErrorDiffusion = new double[,] {
                    { 0, 0, 0, 7, 5 },
                    { 3, 5, 7, 5, 3 },
                    { 1, 3, 5, 3, 1 },
                    };
            Normalize();
            return this;
        }
    }
}
