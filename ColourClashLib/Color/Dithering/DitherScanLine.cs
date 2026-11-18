using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColourClashNet.Color;

namespace ColourClashNet.Color.Dithering
{
    /// <summary>
    /// Error diffusion along the current scanline only (no error is pushed to the next rows).
    /// </summary>
    public class DitherScanLine : DitherErrorDiffusion
    {
        /// <summary>
        /// Creates the ditherer; call <see cref="Create"/> to initialize the kernel.
        /// </summary>
        public DitherScanLine()
        {
            Type = ColorDithering.ScanLine;
            Description = "Scanline quantization error diffusion";
        }

        /// <summary>
        /// Loads and normalizes the error diffusion kernel.
        /// </summary>
        /// <returns>This instance.</returns>
        public override DitherInterface Create()
        {
            errorDiffusionMatrix = new double[,] {
                    { 0, 0, 0, 3, 1 }
                    };
            Normalize();
            return this;
        }
    }
}
