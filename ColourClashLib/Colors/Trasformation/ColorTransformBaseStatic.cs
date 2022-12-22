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

        public static int[,]? ExecuteStdTransform(int[,]? oSource, Dictionary<int, int>? oColorTransformationMap)
        {
            string sMethod = nameof(ExecuteStdTransform);
            if (oSource == null)
            {
                Trace.TraceError($"{sClass}.{sMethod} : Invalid data source");
                return null;
            }
            if (oColorTransformationMap == null || oColorTransformationMap.Count == 0)
            {
                Trace.TraceError($"{sClass}.{sMethod} : Invalid color transformation map");
                return null;
            }
            // var lListList = ToListList(oSource);
            Trace.TraceInformation($"{sClass}.{sMethod} : Apply default trasformatiion");
            var R = oSource.GetLength(0);
            var C = oSource.GetLength(1);
            var oRet = new int[R, C];
            Parallel.For(0, R, r =>
            {
                for (int c = 0; c < C; c++)
                {
                    var col = oSource[r, c];
                    if (col < 0 || !oColorTransformationMap.ContainsKey(col))
                        oRet[r, c] = -1;
                    else
                        oRet[r, c] = oColorTransformationMap[col];

                }
            });
            return oRet;
        }

        public static int[,]? ExecuteStdTransform(int[,]? oSource, ColorTransformInterface oI )
        {
            return ExecuteStdTransform(oSource, oI?.TransformationColorMap );
        }
    }
}
