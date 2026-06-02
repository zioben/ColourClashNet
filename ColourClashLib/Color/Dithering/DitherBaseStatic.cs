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
        public static DitherInterface CreateDitherInterface(ColorDithering ditheringModel, double ditheringStrength, int kernelSize, ColorDitheringFx extraFxMode )
        {
            switch (ditheringModel)
            {
                case ColorDithering.Atkinson:
                    return new DitherAtkinson() { DitheringStrenght = ditheringStrength, DitheringFx= extraFxMode };
                case ColorDithering.Burkes:
                    return new DitherBurkes() { DitheringStrenght = ditheringStrength, DitheringFx = extraFxMode };
                case ColorDithering.FloydSteinberg:
                    return new DitherFloydSteinberg() { DitheringStrenght = ditheringStrength, DitheringFx = extraFxMode };
                case ColorDithering.None:
                    return new DitherIdentity() { DitheringStrenght = ditheringStrength, DitheringFx = extraFxMode };
                case ColorDithering.JarvisJudiceNinke:
                    return new DitherJarvisJudiceNinke() { DitheringStrenght = ditheringStrength, DitheringFx = extraFxMode };
                case ColorDithering.Ordered:
                    return new DitherOrdered() { DitheringStrenght = ditheringStrength, Size = Math.Min(2,kernelSize), DitheringFx = extraFxMode };
                case ColorDithering.Ordered_2x2:
                    return new DitherOrdered() { DitheringStrenght = ditheringStrength, Size = 2, DitheringFx = extraFxMode };
                case ColorDithering.Ordered_4x4:
                    return new DitherOrdered() { DitheringStrenght = ditheringStrength, Size = 4, DitheringFx = extraFxMode };
                case ColorDithering.Ordered_6x6:
                    return new DitherOrdered() { DitheringStrenght = ditheringStrength, Size = 6, DitheringFx = extraFxMode };
                case ColorDithering.Ordered_8x8:
                    return new DitherOrdered() { DitheringStrenght = ditheringStrength, Size = 5, DitheringFx = extraFxMode };
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

        //public static DitherInterface CreateDitherInterface(ColorDithering ditheringModel, int kernelSize) => CreateDitherInterface(ditheringModel, 100, kernelSize, ColorDitheringFx.Full);
        //public static DitherInterface CreateDitherInterface(ColorDithering ditheringModel, double ditheringStrength) => CreateDitherInterface(ditheringModel, ditheringStrength, ColorDefaults.DefaultDitherKernelSize, ColorDitheringFx.Full);      
        //public static DitherInterface CreateDitherInterface(ColorDithering ditheringModel) => CreateDitherInterface(ditheringModel, 100, ColorDefaults.DefaultDitherKernelSize, ColorDitheringFx.Full);
        public static DitherInterface CreateDitherInterface(ColorDithering ditheringModel, double ditheringStrength, ColorDitheringFx ditheringFx) => CreateDitherInterface(ditheringModel, ditheringStrength, ColorDefaults.DefaultDitherKernelSize, ditheringFx);

    }
}
