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
        public ColorTransformInterface CreateColorTransformInterface(ColorTransformType transformType, Dictionary<ColorTransformProperties, object> paramList)
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
                //case ColorTransformType.ColorReductionTileBase:
                //    trans = new TileBase();
                //    break;
                case ColorTransformType.ColorReductionZxSpectrum:
                    trans = new ColorTransformReductionZxSpectrum();
                    break;
                case ColorTransformType.ColorRemover:
                    trans = new ColorTransformBkgRemover();
                    break;
                default:
                    trans = new ColorTransformIdentity();
                    break;
            }
            foreach (var kvp in paramList)
                trans.SetProperty(kvp.Key, kvp.Value);
            return trans;
        }

        static public int[,] HalveHorizontalRes(int[,]? oTmpDataSource)
        {
            if (oTmpDataSource == null)
                return null;
            var R = oTmpDataSource.GetLength(0);
            var C = oTmpDataSource.GetLength(1);
            var oRet = new int[R, (C + 1) / 2];
            Parallel.For(0, R, r =>
            {
                for (int c = 0, co = 0; c < C; c += 2, co++)
                {
                    if (c < C - 1)
                    {
                        var a = oTmpDataSource[r, c];
                        var b = oTmpDataSource[r, c + 1];
                        oRet[r, co] = ColorIntExt.GetColorMean(a, b);
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
//    public abstract partial class ColorTransformBaseOLD : ColorTransformInterface
//    {
//        static string sClass = nameof(ColorTransformBase);

//        public static int[,]? ExecuteStdTransform(int[,]? oSource, ColorTransformationMap oColorTransformationMap, CancellationToken oToken)
//        {
//            string sMethod = nameof(ExecuteStdTransform);
//            if (oSource == null)
//            {
//                LogMan.Error(sClass, sMethod, "Invalid data source");   
//                return null;
//            }
//            if (oColorTransformationMap == null || oColorTransformationMap.Colors == 0)
//            {
//                LogMan.Error(sClass, sMethod, "Invalid color transformation map");
//                return null;
//            }
//            // var lListList = ToListList(oSource);
//            LogMan.Trace(sClass, sMethod, $"Apply default trasformation");
//            var R = oSource.GetLength(0);
//            var C = oSource.GetLength(1);
//            var oRet = new int[R, C];
//            Parallel.For(0, R, r =>
//            {
//                for (int c = 0; c < C; c++)
//                {
//                    var col = oSource[r, c];
//                    if (col < 0 || !oColorTransformationMap.rgbTransformationMap.ContainsKey(col))
//                        oRet[r, c] = col;
//                    else
//                        oRet[r, c] = oColorTransformationMap.rgbTransformationMap[col];

//                }
//                oToken.ThrowIfCancellationRequested();  
//            });
//            return oRet;
//        }

//        public static int[,]? ExecuteStdTransform(int[,]? oSource, ColorTransformInterface oI, CancellationToken oToken )
//        {
//            return ExecuteStdTransform(oSource, oI?.ColorTransformationMapper, oToken );
//        }

//        public ColorTransformInterface CreateColorTransformInterface(ColorTransformType transformType, Dictionary<ColorTransformProperties, object> paramList)
//        {
//            string sMethod = nameof(CreateColorTransformInterface);
//            ColorTransformInterface trans = null;
//            switch (transformType)
//            {
//                case ColorTransformType.None:
//                    trans = new ColorTransformIdentity();
//                    break;
//                case ColorTransformType.ColorReductionCBM64:
//                    trans = new ColorTransformIdentity();
//                    break;
//                case ColorTransformType.ColorReductionClustering:
//                    trans = new ColorTransformIdentity();
//                    break;
//                case ColorTransformType.ColorReductionCPC:
//                    trans = new ColorTransformIdentity();
//                    break;
//                case ColorTransformType.ColorReductionEga:
//                    trans = new ColorTransformIdentity();
//                    break;
//                case ColorTransformType.ColorReductionFast:
//                    trans = new ColorTransformIdentity();
//                    break;
//                case ColorTransformType.ColorReductionGenericPalette:
//                    trans = new ColorTransformIdentity();
//                    break;
//                case ColorTransformType.ColorReductionHam:
//                    trans = new ColorTransformIdentity();
//                    break;
//                case ColorTransformType.ColorReductionMedianCut:
//                    trans = new ColorTransformIdentity();
//                    break;
//                case ColorTransformType.ColorReductionQuantization:
//                    trans = new ColorTransformIdentity();
//                    break;
//                case ColorTransformType.ColorReductionSaturation:
//                    trans = new ColorTransformIdentity();
//                    break;
//                case ColorTransformType.ColorReductionScanline:
//                    trans = new ColorTransformIdentity();
//                    break;
//                case ColorTransformType.ColorReductionTileBase:
//                    trans = new ColorTransformIdentity();
//                    break;
//                case ColorTransformType.ColorReductionZxSpectrum:
//                    trans = new ColorTransformIdentity();
//                    break;
//                case ColorTransformType.ColorRemover:
//                    trans = new ColorTransformIdentity();
//                    break;
//                default:
//                    trans = new ColorTransformIdentity();
//                    break;
//            }
//            foreach (var kvp in paramList)
//                trans.SetProperty(kvp.Key, kvp.Value);
//            return trans;
//        }

//        static public int[,] HalveHorizontalRes(int[,]? oTmpDataSource)
//        {
//            if (oTmpDataSource == null)
//                return null;
//            var R = oTmpDataSource.GetLength(0);
//            var C = oTmpDataSource.GetLength(1);
//            var oRet = new int[R, (C + 1) / 2];
//            Parallel.For(0, R, r =>
//            {
//                for (int c = 0, co = 0; c < C; c += 2, co++)
//                {
//                    if (c < C - 1)
//                    {
//                        var a = oTmpDataSource[r, c];
//                        var b = oTmpDataSource[r, c + 1];
//                        oRet[r, co] = ColorIntExt.GetColorMean(a, b);
//                    }
//                }
//            });
//            return oRet;
//        }

//        public static int[,] DoubleHorizontalRes(int[,]? oTmpDataSource)
//        {
//            if (oTmpDataSource == null)
//                return null;
//            var R = oTmpDataSource.GetLength(0);
//            var C = oTmpDataSource.GetLength(1);
//            var oRet = new int[R, C * 2];

//            Parallel.For(0, R, r =>
//            {
//                for (int c = 0, co = 0; c < C; c++)
//                {
//                    var a = oTmpDataSource[r, c];
//                    oRet[r, co++] = a;
//                    oRet[r, co++] = a;
//                }
//            });
//            return oRet;
//        }
//    }
//}
