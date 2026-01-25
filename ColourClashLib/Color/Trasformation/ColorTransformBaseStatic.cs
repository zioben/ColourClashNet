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
        public static ColorTransformInterface CreateColorTransformInterface(ColorTransformType transformType, Dictionary<ColorTransformProperties, object> paramList)
        {
            string sMethod = nameof(CreateColorTransformInterface);
            ColorTransformInterface trans = null;
            switch (transformType)
            {
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
                    trans = new ColorTransformReductionZxSpectrumV2();                    
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

        static public int[,] HalveHorizontalRes(int[,]? oTmpDataSource)
        {
            if (oTmpDataSource == null)
                return null;
            var R = oTmpDataSource.GetLength(0);
            var C = oTmpDataSource.GetLength(1);
            var CO = (C + 1) / 2;
            var oRet = new int[R, CO];
            Parallel.For(0, R, r =>
            {
                for (int c = 0, co = 0; c < C; c += 2, co++)
                {
                    if (c < C - 1)
                    {
                        var a = oTmpDataSource[r, c];
                        var b = oTmpDataSource[r, c + 1];
                        oRet[r, co] = ColorIntExt.GetColorMean(a, a);
                    }
                }
            });
            return oRet;
        }

        public static int[,] DoubleHorizontalRes(int[,]? oTmpDataSource)
        {
            if (oTmpDataSource == null)
                return null;
            var R = oTmpDataSource.GetLength(0);
            var C = oTmpDataSource.GetLength(1);
            var oRet = new int[R, C * 2];

            Parallel.For(0, R, r =>
            {
                for (int c = 0, co = 0; c < C; c++)
                {
                    var a = oTmpDataSource[r, c];
                    oRet[r, co++] = a;
                    oRet[r, co++] = a;
                }
            });
            return oRet;
        }
    }
}
