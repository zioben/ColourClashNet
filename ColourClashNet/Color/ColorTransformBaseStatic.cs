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


    }
}
