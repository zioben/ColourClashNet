using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public abstract partial class ColorTransformBase : ColorTransformInterface
    {

        static int[] oColorArray = new int[256 * 256 * 256];

        static object oLocker = new object();

        static Dictionary<int, int> CreateColorHistArray(int[,] oDataSource)
        {
            var ret = new Dictionary<int, int>();
            if (oDataSource == null)
                return ret;

            lock (oLocker)
            {
                Array.Clear(oColorArray, 0, oColorArray.Length);
                int R = oDataSource.GetLength(0);
                int C = oDataSource.GetLength(1);
                var HashColorHistogram = new HashSet<int>();
                for (int r = 0; r < R; r++)
                {
                    for (int c = 0; c < C; c++)
                    {
                        var i = oDataSource[r, c];
                        if (i >= 0)
                        {
                            HashColorHistogram.Add(oDataSource[r, c]);
                            oColorArray[i]++;
                        }
                    }
                }

                foreach (var item in HashColorHistogram)
                {
                    ret.Add(item, oColorArray[item]);
                }
                return ret;
            }
        }

        static Dictionary<int, int> CreateColorHist(int[,] oDataSource)
        {
            var ret = new Dictionary<int, int>();
            if (oDataSource == null)
                return ret;

            int R = oDataSource.GetLength(0);
            int C = oDataSource.GetLength(1);
            if (R * C > 64 * 64)
            {
                return CreateColorHistArray(oDataSource);
            }
            else
            {
                var ListPixels = new List<int>(R * C);
                var HashColorHistogram = new HashSet<int>();
                for (int r = 0; r < R; r++)
                {
                    for (int c = 0; c < C; c++)
                    {
                        HashColorHistogram.Add(oDataSource[r, c]);
                        ListPixels.Add(oDataSource[r, c]);
                    }
                }
                var lColors = HashColorHistogram.ToList();
                foreach (var item in HashColorHistogram)
                {
                    ret.Add(item, ListPixels.Count(X => X == item));
                }
                return ret;
            }
        }

        public static List<int> CreatePalette(int[,] oData)
        {
            if (oData == null)
                return new List<int>();
            HashSet<int> palette = new HashSet<int>();
            foreach (var k in oData)
            {
                palette.Add(k);
            }
            return palette.ToList();
        }

        public static List<int> CreatePalette(Dictionary<int, int> oDataHistogram)
        {
            if (oDataHistogram == null || oDataHistogram.Count == 0)
                return new List<int>();

            HashSet<int> palette = new HashSet<int>();
            foreach (var k in oDataHistogram)
            {
                palette.Add(k.Key);
            }
            return palette.ToList();
        }

        public static int GetNearestColor(int iColor, List<int> oPalette, ColorDistanceEvaluationMode eMode)
        {
            //if (oPalette == null || oPalette.Count == -1)
            //    return -1;
            //var col = oPalette[0];
            //var distMin = iColor.Distance(oPalette[0], eMode);
            //for (int i = 1; i < oPalette.Count; i++)
            //{ 
            //    var dist = col.Distance(oPalette[i], eMode);
            //    if (distMin > dist)
            //    {
            //        col = oPalette[i];
            //        distMin = dist;
            //    }
            //}
            //return col;
            //
            // SLOOOOW
            double dmin = oPalette.Min(X => X.Distance(iColor, eMode));
            if (dmin == 0)
                return iColor;
            return oPalette.FirstOrDefault(X => X.Distance(iColor, eMode) == dmin);
        }

        public static int[,]? ApplyTransform(int[,]? oSource, Dictionary<int, int>? oColorTransformationMap)
        {
            if (oSource == null)
                return null;
            if (oColorTransformationMap == null || oColorTransformationMap.Count == 0)
                return null;
            // var lListList = ToListList(oSource);
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

        public static int[,]? ApplyTransform(int[,]? oSource, ColorTransformInterface oI )
        {
            return ApplyTransform(oSource, oI?.oColorTransformationMap );
        }
    }
}
