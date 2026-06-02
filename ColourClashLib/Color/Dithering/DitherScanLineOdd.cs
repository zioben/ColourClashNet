using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColourClashNet.Color;

namespace ColourClashNet.Color.Dithering
{
    public class DitherScanLineOdd : DitherErrorDiffusion
    {

        public DitherScanLineOdd()
        {
            Type = ColorDithering.ScanLine;
            Description = "Scanline quantization error diffusion";
        }


        public override DitherInterface Create()
        {
            matErrorDiffusion = new double[,] {
                    { 0, 0, 0, 0, 0 },
                    { 2, 0, 1, 0, 1 },
                    };
            Normalize();
            return this;
        }
    }
}
