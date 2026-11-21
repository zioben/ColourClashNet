using ColourClashNet.Color;
using ColourClashNet.Color.Dithering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Dithering
{
    public abstract partial class DitherBase
    {
        /// <summary>
        /// Creates and configures the ditherer that implements <paramref name="ditheringModel"/>.
        /// </summary>
        /// <param name="ditheringModel">Dithering algorithm</param>
        /// <param name="ditheringStrength">Strength multiplier</param>
        /// <param name="extraFxMode">Post-dithering effect</param>
        /// <returns>A ditherer instance; <see cref="DitherIdentity"/> for unknown models.</returns>
        static DitherInterface CreateDitherInterface(ColorDithering ditheringModel, double ditheringStrength, ColorDitheringFx extraFxMode)
        {
            switch (ditheringModel)
            {
                case ColorDithering.Atkinson:
                    return new DitherAtkinson() { DitheringStrenght = ditheringStrength, DitheringFx = extraFxMode };
                case ColorDithering.Burkes:
                    return new DitherBurkes() { DitheringStrenght = ditheringStrength, DitheringFx = extraFxMode };
                case ColorDithering.FloydSteinberg:
                    return new DitherFloydSteinberg() { DitheringStrenght = ditheringStrength, DitheringFx = extraFxMode };
                case ColorDithering.None:
                    return new DitherIdentity() { DitheringStrenght = ditheringStrength, DitheringFx = extraFxMode };
                case ColorDithering.JarvisJudiceNinke:
                    return new DitherJarvisJudiceNinke() { DitheringStrenght = ditheringStrength, DitheringFx = extraFxMode };
                case ColorDithering.Ordered_2x2:
                    return new DitherOrdered() { DitheringStrenght = ditheringStrength, Size = 2, DitheringFx = extraFxMode };
                case ColorDithering.Ordered_4x4:
                    return new DitherOrdered() { DitheringStrenght = ditheringStrength, Size = 4, DitheringFx = extraFxMode };
                case ColorDithering.Ordered_6x6:
                    return new DitherOrdered() { DitheringStrenght = ditheringStrength, Size = 6, DitheringFx = extraFxMode };
                case ColorDithering.Ordered_8x8:
                    return new DitherOrdered() { DitheringStrenght = ditheringStrength, Size = 8, DitheringFx = extraFxMode };
                case ColorDithering.ScanLine:
                    return new DitherScanLine() { DitheringStrenght = ditheringStrength, DitheringFx = extraFxMode };
                case ColorDithering.Sierra:
                    return new DitherSierra() { DitheringStrenght = ditheringStrength, DitheringFx = extraFxMode };
                case ColorDithering.Stucki:
                    return new DitherStucki() { DitheringStrenght = ditheringStrength, DitheringFx = extraFxMode };
                default:
                    return new DitherIdentity() { DitheringStrenght = ditheringStrength, DitheringFx = extraFxMode };
            }
        }

        /// <summary>
        /// Creates the ditherer described by <paramref name="ditherOptions"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="ditherOptions"/> is null.</exception>
        public static DitherInterface CreateDitherInterface(DitherOptions ditherOptions)
        {
            ArgumentNullException.ThrowIfNull(ditherOptions);
            return CreateDitherInterface(ditherOptions.DitheringType, ditherOptions.DitheringStrength, ditherOptions.DitheringFx);
        }
    }
}
