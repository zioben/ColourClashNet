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
        public static DitherInterface CreateDitherInterface(ColorDithering ditheringModel, double ditheringStrength, ColorDitheringFx extraFxMode)
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

        public static DitherInterface CreateDitherInterface( Dictionary<ColorTransformProperties,object> properties )
        {
            if( properties == null ) 
                throw new ArgumentNullException(nameof(properties));
            if (properties.ContainsKey(ColorTransformProperties.DitheringType))
            {
                var ditheringType = (ColorDithering)properties[ColorTransformProperties.DitheringType];
                var ditheringStrength = properties.ContainsKey(ColorTransformProperties.DitheringStrength) ? (double)properties[ColorTransformProperties.DitheringStrength] : 100;
                var ditheringFx = properties.ContainsKey(ColorTransformProperties.DitheringFx) ? (ColorDitheringFx)properties[ColorTransformProperties.DitheringFx] : ColorDitheringFx.Full;
                return CreateDitherInterface(ditheringType, ditheringStrength, ditheringFx);
            }
            else
            {   
                return CreateDitherInterface(ColorDithering.None, 100, ColorDitheringFx.Full);
            }
        }
    }
}
