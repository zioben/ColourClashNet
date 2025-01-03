﻿using ColourClashLib;
using ColourClashLib.Color;
using ColourClashNet.Colors.Dithering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors.Transformation
{
    public abstract partial class ColorTransformBase : ColorTransformInterface
    {
        static string sClass = nameof(ColorTransformBase);

        public static int[,]? ExecuteStdTransform(int[,]? oSource, ColorTransformationMap oColorTransformationMap, CancellationToken oToken)
        {
            string sMethod = nameof(ExecuteStdTransform);
            if (oSource == null)
            {
                Trace.TraceError($"{sClass}.{sMethod} : Invalid data source");
                return null;
            }
            if (oColorTransformationMap == null || oColorTransformationMap.Colors == 0)
            {
                Trace.TraceError($"{sClass}.{sMethod} : Invalid color transformation map");
                return null;
            }
            // var lListList = ToListList(oSource);
            if (ColorDefaults.Trace)
                Trace.TraceInformation($"{sClass}.{sMethod} : Apply default trasformatiion");
            var R = oSource.GetLength(0);
            var C = oSource.GetLength(1);
            var oRet = new int[R, C];
            Parallel.For(0, R, r =>
            {
                for (int c = 0; c < C; c++)
                {
                    var col = oSource[r, c];
                    if (col < 0 || !oColorTransformationMap.rgbTransformationMap.ContainsKey(col))
                        oRet[r, c] = -1;
                    else
                        oRet[r, c] = oColorTransformationMap.rgbTransformationMap[col];

                }
                oToken.ThrowIfCancellationRequested();  
            });
            return oRet;
        }

        public static int[,]? ExecuteStdTransform(int[,]? oSource, ColorTransformInterface oI, CancellationToken oToken )
        {
            return ExecuteStdTransform(oSource, oI?.ColorTransformationMapper, oToken );
        }

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
                    trans = new ColorTransformIdentity();
                    break;
                case ColorTransformType.ColorReductionClustering:
                    trans = new ColorTransformIdentity();
                    break;
                case ColorTransformType.ColorReductionCPC:
                    trans = new ColorTransformIdentity();
                    break;
                case ColorTransformType.ColorReductionEga:
                    trans = new ColorTransformIdentity();
                    break;
                case ColorTransformType.ColorReductionFast:
                    trans = new ColorTransformIdentity();
                    break;
                case ColorTransformType.ColorReductionGenericPalette:
                    trans = new ColorTransformIdentity();
                    break;
                case ColorTransformType.ColorReductionHam:
                    trans = new ColorTransformIdentity();
                    break;
                case ColorTransformType.ColorReductionMedianCut:
                    trans = new ColorTransformIdentity();
                    break;
                case ColorTransformType.ColorReductionQuantization:
                    trans = new ColorTransformIdentity();
                    break;
                case ColorTransformType.ColorReductionSaturation:
                    trans = new ColorTransformIdentity();
                    break;
                case ColorTransformType.ColorReductionScanline:
                    trans = new ColorTransformIdentity();
                    break;
                case ColorTransformType.ColorReductionTileBase:
                    trans = new ColorTransformIdentity();
                    break;
                case ColorTransformType.ColorReductionZxSpectrum:
                    trans = new ColorTransformIdentity();
                    break;
                case ColorTransformType.ColorRemover:
                    trans = new ColorTransformIdentity();
                    break;
                default:
                    trans = new ColorTransformIdentity();
                    break;
            }
            foreach (var kvp in paramList)
                trans.SetProperty(kvp.Key, kvp.Value);
            return trans;
        }

    }
}
