using ColourClashNet.Color.Tile;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

        public static ColorTransformInterface CreateColorTransformInterface(ColorTransformType transformType, Dictionary<ColorTransformProperties, object> paramList)
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
                    trans = new ColorTransformReductionC64();
                    break;
                case ColorTransformType.ColorReductionClustering:
                    trans = new ColorTransformReductionCluster();
                    break;
                case ColorTransformType.ColorReductionCPC:
                    trans = new ColorTransformReductionCPC();
                    break;
                case ColorTransformType.ColorReductionEga:
                    trans = new ColorTransformReductionEGA();
                    break;
                case ColorTransformType.ColorReductionFast:
                    trans = new ColorTransformReductionFast();
                    break;
                case ColorTransformType.ColorReductionGenericPalette:
                    trans = new ColorTransformReductionPalette();
                    break;
                case ColorTransformType.ColorReductionHam:
                    trans = new ColorTransformReductionAmiga();
                    break;
                case ColorTransformType.ColorReductionMedianCut:
                    trans = new ColorTransformReductionMedianCut();
                    break;
                case ColorTransformType.ColorReductionQuantization:
                    trans = new ColorTransformQuantization();
                    break;
                case ColorTransformType.ColorReductionSaturation:
                    trans = new ColorTransformLumSat();
                    break;
                case ColorTransformType.ColorReductionScanline:
                    trans = new ColorTransformReductionScanLine();
                    break;
                case ColorTransformType.ColorReductionZxSpectrum:
                    trans = new ColorTransformReductionZxSpectrum();                    
                    break;
                case ColorTransformType.ColorRemover:
                    trans = new ColorTransformBkgRemover();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(sMethod, $"Transform type {transformType} not recognised");
            }

            if (trans is ColorTransformBase transBase)
            {
                foreach (var kvp in paramList)
                    transBase.SetProperty(kvp.Key, kvp.Value);
            }
            return trans;
        }
    }
}
