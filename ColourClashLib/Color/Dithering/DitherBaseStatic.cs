using ColourClashNet.Color;
using ColourClashNet.Color.Dithering;
using ColourClashNet.Defaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Dithering
{
    public abstract partial class DitherBase 
    {
        public static DitherInterface CreateDitherInterface(ColorDithering ditheringModel, double ditheringStrength, int kernelSize)
        {
            switch (ditheringModel)
            {
                case ColorDithering.Atkinson:
                    return new DitherAtkinson() { DitheringStrenght = ditheringStrength };
                case ColorDithering.Burkes:
                    return new DitherBurkes() { DitheringStrenght = ditheringStrength };
                case ColorDithering.FloydSteinberg:
                    return new DitherFloydSteinberg() { DitheringStrenght = ditheringStrength };
                case ColorDithering.None:
                    return new DitherIdentity() { DitheringStrenght = ditheringStrength };
                case ColorDithering.JarvisJudiceNinke:
                    return new DitherJarvisJudiceNinke() { DitheringStrenght = ditheringStrength };
                case ColorDithering.Ordered:
                    return new DitherOrdered() { DitheringStrenght = ditheringStrength, Size = Math.Min(2,kernelSize) };
                case ColorDithering.Ordered_2x2:
                    return new DitherOrdered() { DitheringStrenght = ditheringStrength, Size = 2 };
                case ColorDithering.Ordered_4x4:
                    return new DitherOrdered() { DitheringStrenght = ditheringStrength, Size = 4 };
                case ColorDithering.Ordered_6x6:
                    return new DitherOrdered() { DitheringStrenght = ditheringStrength, Size = 6 };
                case ColorDithering.Ordered_8x8:
                    return new DitherOrdered() { DitheringStrenght = ditheringStrength, Size = 5 };
                case ColorDithering.ScanLine:
                    return new DitherScanLine() { DitheringStrenght = ditheringStrength };
                case ColorDithering.Sierra:
                    return new DitherSierra() { DitheringStrenght = ditheringStrength };
                case ColorDithering.Stucki:
                    return new DitherStucki() { DitheringStrenght = ditheringStrength };
                default:
                    return new DitherIdentity() { DitheringStrenght = ditheringStrength };
            }
        }

        public static DitherInterface CreateDitherInterface(ColorDithering ditheringModel, int kernelSize) => CreateDitherInterface(ditheringModel, 100, kernelSize);
        public static DitherInterface CreateDitherInterface(ColorDithering ditheringModel, double ditheringStrength) => CreateDitherInterface(ditheringModel, ditheringStrength, ColorDefaults.DefaultDitherKernelSize);      
        public static DitherInterface CreateDitherInterface(ColorDithering ditheringModel) => CreateDitherInterface(ditheringModel, 100, ColorDefaults.DefaultDitherKernelSize);
    }
}
