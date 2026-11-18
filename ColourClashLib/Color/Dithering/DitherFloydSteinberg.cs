using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColourClashNet.Color;

namespace ColourClashNet.Color.Dithering
{
    /// <summary>
    /// Floyd-Steinberg error diffusion dithering (7/16, 3/16, 5/16, 1/16).
    /// </summary>
    public class DitherFloydSteinberg : DitherErrorDiffusion
    {
        /// <summary>
        /// Creates the ditherer; call <see cref="Create"/> to initialize the kernel.
        /// </summary>
        public DitherFloydSteinberg()
        {
            Type = ColorDithering.FloydSteinberg;
            Description = "Floyd Steinberg quantization error diffusion";
        }

        /// <summary>
        /// Loads and normalizes the error diffusion kernel.
        /// </summary>
        /// <returns>This instance.</returns>
        public override DitherInterface Create()
        {
            errorDiffusionMatrix = new double[,]
            {
                { 0, 0, 7 },
                { 3, 5, 1 }
            };
            Normalize();
            return this;
        }
    }
}
