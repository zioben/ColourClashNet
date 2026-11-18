using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColourClashNet.Color;

namespace ColourClashNet.Color.Dithering
{
    /// <summary>
    /// Atkinson error diffusion dithering.
    /// <para/>
    /// The kernel is normalized by 8 while its weights sum to 6: only 3/4 of the error is diffused,
    /// giving the high-contrast look of the original Apple Macintosh.
    /// </summary>
    public class DitherAtkinson : DitherErrorDiffusion
    {
        /// <summary>
        /// Creates the ditherer; call <see cref="Create"/> to initialize the kernel.
        /// </summary>
        public DitherAtkinson()
        {
            Type = ColorDithering.Atkinson;
            Description = "Atkinson quantization error diffusion";
        }

        /// <summary>
        /// Loads and normalizes the error diffusion kernel.
        /// </summary>
        /// <returns>This instance.</returns>
        public override DitherInterface Create()
        {
            errorDiffusionMatrix = new double[,] {
                    { 0, 0, 0, 1, 1 },
                    { 0, 1, 1, 1, 0 },
                    { 0, 0, 1, 0, 0 },
                    };
            Normalize(8);
            return this;
        }
    }
}
