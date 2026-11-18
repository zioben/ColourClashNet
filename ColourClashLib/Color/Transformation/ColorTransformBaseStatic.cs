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
        /// <summary>
        /// Throws if <paramref name="transform"/> is null.
        /// </summary>
        public static void AssertValid(ColorTransformInterface transform)
        {
            if (transform == null)
                throw new ArgumentNullException($"{nameof(transform)}");
        }

        /// <summary>
        /// Factory: creates the transformation that implements <paramref name="transformType"/> and applies the configuration.
        /// </summary>
        /// <param name="transformType">Algorithm to create</param>
        /// <param name="paramList">Configuration copied into the new instance</param>
        /// <returns>The configured transformation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Unsupported transformation type.</exception>
        public static ColorTransformInterface CreateColorTransformInterface(ColorTransformType transformType, ColorTransformConfig paramList)
        {
            string sMethod = nameof(CreateColorTransformInterface);
            ColorTransformInterface transform = null;
            switch (transformType)
            {
                case ColorTransformType.ColorIdentity:
                case ColorTransformType.None:
                    transform = new ColorTransformIdentity();
                    break;
                case ColorTransformType.ColorReductionCBM64:
                    transform = new ColorTransformCbm64();
                    break;
                case ColorTransformType.ColorReductionClustering:
                    transform = new ColorTransformClustering();
                    break;
                case ColorTransformType.ColorReductionCPC:
                    transform = new ColorTransformAmstradCPC();
                    break;
                case ColorTransformType.ColorReductionHercules:
                    transform = new ColorTransformHercules();
                    break;
                case ColorTransformType.ColorReductionCGA:
                    transform = new ColorTransformCGA();
                    break;
                case ColorTransformType.ColorReductionEGA:
                    transform = new ColorTransformEGA();
                    break;
                case ColorTransformType.ColorReductionVGA:
                    transform = new ColorTransformVGA();
                    break;
                case ColorTransformType.ColorReductionFast:
                    transform = new ColorTransformFast();
                    break;
                case ColorTransformType.ColorReductionGenericPalette:
                    transform = new ColorTransformFixedPalette();
                    break;
                case ColorTransformType.ColorReductionHam:
                    transform = new ColorTransformCbmAmiga();
                    break;
                case ColorTransformType.ColorReductionMedianCut:
                    transform = new ColorTransformMedianCut();
                    break;
                case ColorTransformType.ColorReductionQuantization:
                    transform = new ColorTransformQuantization();
                    break;
                case ColorTransformType.ColorReductionSaturation:
                    transform = new ColorTransformLumSat();
                    break;
                case ColorTransformType.ColorReductionScanline:
                    transform = new ColorTransformScanLine();
                    break;
                case ColorTransformType.ColorReductionZxSpectrum:
                    transform = new ColorTransformZxSpectrum();                    
                    break;
                case ColorTransformType.ColorRemover:
                    transform = new ColorTransformBkgRemover();
                    break;
                case ColorTransformType.ColorReductionEnhancedPalette:
                    transform = new ColorTransformFixedPaletteEnhanced();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(sMethod, $"Transform type {transformType} not yet supported");
            }

            return transform.SetProperties(paramList);
        }
    }
}
