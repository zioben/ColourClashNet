using ColourClashNet.Colors;
using ColourClashNet.Colors.Dithering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashLib.Colors.Dithering
{
    public abstract partial class DitherBase 
    {
        static DitherInterface CreateDitherInterface(ColorDithering eModelType, double ditheringStrength, int iKernelSize)
        {
            switch (eModelType)
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
                    return new DitherOrdered() { DitheringStrenght = ditheringStrength, Size = Math.Min(2,iKernelSize) };
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

        static DitherInterface CreateDitherInterface(ColorDithering eModelType, int iKernelSize) => CreateDitherInterface(eModelType, 100, iKernelSize);
        static DitherInterface CreateDitherInterface(ColorDithering eModelType, double dDitherStrength) => CreateDitherInterface(eModelType, dDitherStrength, ColorDefaults.DefaultDitherKernelSize);      
        static DitherInterface CreateDitherInterface(ColorDithering eModelType) => CreateDitherInterface(eModelType, 100, ColorDefaults.DefaultDitherKernelSize);
    }
}
