using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public abstract partial class ColorTransformBase
    {
        // Avoid to load 50 MB to process 16x16 tile image 
        // Use a global Histogram Matrix and lock in case of competition
        static int[,,] Histogram = new int[256, 256, 256];
        static object oHistLocker = new object();
        static void BuildColorHistHistogram(ColorItem[,] m, Dictionary<ColorItem, int> oDict)
        {
            oDict.Clear();
            lock (oHistLocker)
            {
                Array.Clear(Histogram);
                int R = m.GetLength(0);
                int C = m.GetLength(1);
                for (int r = 0; r < R; r++)
                {
                    for (int c = 0; c < C; c++)
                    {
                        var col = m[r, c];
                        if (col.Valid)
                        {
                            Histogram[col.R, col.G, col.B]++;
                        }
                    }
                }
                for (int r = 0; r < 256; r++)
                {
                    for (int g = 0; g < 256; g++)
                    {
                        for (int b = 0; b < 256; b++)
                        {
                            int val = Histogram[r, g, b];
                            if (val != 0)
                            {
                                oDict.Add(new ColorItem(r, g, b), val);
                            }
                        }
                    }
                }
            }
        }

        static void BuildColorHistDirect(ColorItem[,] m, Dictionary<ColorItem, int> oDict)
        {
            oDict.Clear();
            int R = m.GetLength(0);
            int C = m.GetLength(1);
            for (int r = 0; r < R; r++)
            {
                for (int c = 0; c < C; c++)
                {
                    var col = m[r, c];
                    if (col.Valid)
                    {
                        if (oDict.ContainsKey(col))
                            oDict[col]++;
                        else
                            oDict[col] = 1;
                    }
                }
            }
        }

        static void BuildColorHist(ColorItem[,] m, Dictionary<ColorItem, int> oDict)
        {
            if (m == null)
                return;
            if (oDict == null)
                return;
            if (m.Length < 64 * 64)
                BuildColorHistDirect(m, oDict);
            else
                BuildColorHistHistogram(m, oDict);
        }



    }
}
