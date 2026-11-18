using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColourClashNet.Color;

namespace ColourClashNet.Color.Dithering
{
    /// <summary>
    /// Burkes error diffusion dithering (2 rows, weights / 32).
    /// </summary>
    public class DitherBurkes : DitherErrorDiffusion
    {
        /// <summary>
        /// Creates the ditherer; call <see cref="Create"/> to initialize the kernel.
        /// </summary>
        public DitherBurkes()
        {
            Type = ColorDithering.Burkes;
            Description = "Burkes quantization error diffusion";
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
                    };
            Normalize();
            return this;
        }
    }
}
