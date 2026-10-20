using ColourClashNet.Color.Transformation;
using ColourClashNet.Color.Tile;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public abstract partial class ColorTransformBase : ColorTransformInterface
    {
        public static void AssertValid(ColorTransformInterface transform)
        {
            if (transform == null)
                throw new ArgumentNullException($"{nameof(transform)}");
        }

        public static ColorTransformInterface CreateColorTransformInterface(ColorTransformType transformType, ColorTransformConfig paramList)
        {
            string sMethod = nameof(CreateColorTransformInterface);
            ColorTransformInterface trans = null;
            switch (transformType)
            {
                case ColorTransformType.ColorIdentity:
                case ColorTransformType.None:
                    trans = new ColorTransformIdentity();
                    break;
                case ColorTransformType.ColorReductionCBM64:
                    trans = new ColorTransformCbm64();
                    break;
                case ColorTransformType.ColorReductionClustering:
                    trans = new ColorTransformClustering();
                    break;
                case ColorTransformType.ColorReductionCPC:
                    trans = new ColorTransformAmstradCPC();
                    break;
                case ColorTransformType.ColorReductionEGA:
                    trans = new ColorTransformEGA();
                    break;
                case ColorTransformType.ColorReductionFast:
                    trans = new ColorTransformFast();
                    break;
                case ColorTransformType.ColorReductionGenericPalette:
                    trans = new ColorTransformFixedPalette();
                    break;
                case ColorTransformType.ColorReductionHam:
                    trans = new ColorTransformCbmAmiga();
                    break;
                case ColorTransformType.ColorReductionMedianCut:
                    trans = new ColorTransformMedianCut();
                    break;
                case ColorTransformType.ColorReductionQuantization:
                    trans = new ColorTransformQuantization();
                    break;
                case ColorTransformType.ColorReductionSaturation:
                    trans = new ColorTransformLumSat();
                    break;
                case ColorTransformType.ColorReductionScanline:
                    trans = new ColorTransformScanLine();
                    break;
                case ColorTransformType.ColorReductionZxSpectrum:
                    trans = new ColorTransformZxSpectrum();                    
                    break;
                case ColorTransformType.ColorRemover:
                    trans = new ColorTransformBkgRemover();
                    break;
                case ColorTransformType.ColorReductionEnhancedPalette:
                    trans = new ColorTransformFixedPaletteEnhanced();
                    break;
                case ColorTransformType.ColorReductionCGA:
                    trans = new ColorTransformCGA();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(sMethod, $"Transform type {transformType} not yet supported");
            }

            return trans.SetProperties(paramList);
        }
    }
}
