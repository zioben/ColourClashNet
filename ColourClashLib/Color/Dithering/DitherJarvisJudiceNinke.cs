using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColourClashNet.Color;

namespace ColourClashNet.Color.Dithering
{
    /// <summary>
    /// Jarvis, Judice and Ninke error diffusion dithering (12 neighbours, weights / 48).
    /// </summary>
    public class DitherJarvisJudiceNinke : DitherErrorDiffusion
    {
        /// <summary>
        /// Creates the ditherer; call <see cref="Create"/> to initialize the kernel.
        /// </summary>
        public DitherJarvisJudiceNinke()
        {
            Type = ColorDithering.JarvisJudiceNinke;
            Description = "Jarvis, Judice and Ninke quantization error diffusion";
        }

        /// <summary>
        /// Loads and normalizes the error diffusion kernel.
        /// </summary>
        /// <returns>This instance.</returns>
        public override DitherInterface Create()
        {
            errorDiffusionMatrix = new double[,] {
                    { 0, 0, 0, 7, 5 },
                    { 3, 5, 7, 5, 3 },
                    { 1, 3, 5, 3, 1 },
                    };
            Normalize();
            return this;
        }
    }
}
