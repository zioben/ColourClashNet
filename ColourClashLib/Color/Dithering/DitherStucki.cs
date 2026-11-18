using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColourClashNet.Color;

namespace ColourClashNet.Color.Dithering
{
    /// <summary>
    /// Stucki error diffusion dithering (12 neighbours, weights / 42).
    /// </summary>
    public class DitherStucki : DitherErrorDiffusion
    {
        /// <summary>
        /// Creates the ditherer; call <see cref="Create"/> to initialize the kernel.
        /// </summary>
        public DitherStucki()
        {
            Type = ColorDithering.Stucki;
            Description = "Stucki quantization error diffusion";
        }

        /// <summary>
        /// Loads and normalizes the error diffusion kernel.
        /// </summary>
        /// <returns>This instance.</returns>
        public override DitherInterface Create()
        {
            errorDiffusionMatrix = new double[,] {
                    { 0, 0, 0, 8, 4 },
                    { 2, 4, 8, 4, 2 },
                    { 1, 2, 4, 2, 1 },
                    };
            Normalize();
            return this;
        }
    }
}
