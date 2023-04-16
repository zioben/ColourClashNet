using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashLib.Color
{
    public partial class ColorHistogram
    {
        static int[] oColorArray = new int[256 * 256 * 256];

        static object oLocker = new object();

        static void CreateColorHistArray(int[,] oDataSource, ColorHistogram oHist)
        {
            lock (oLocker)
            {
                Array.Clear(oColorArray, 0, oColorArray.Length);
                int R = oDataSource.GetLength(0);
                int C = oDataSource.GetLength(1);
                for (int r = 0; r < R; r++)
                {
                    for (int c = 0; c < C; c++)
                    {
                        var i = oDataSource[r, c];
                        if (i >= 0)
                        {
                            oColorArray[i]++;
                        }
                    }
                }
                for (int rgb = 0; rgb < oColorArray.Length; rgb++)
                {
                    if (oColorArray[rgb] > 0)
                        oHist.Add(rgb, oColorArray[rgb]);
                }
            }
        }
        static bool CreateColorHistDirect(int[,] oDataSource, ColorHistogram oHist)
        {

            int R = oDataSource.GetLength(0);
            int C = oDataSource.GetLength(1);
            for (int r = 0; r < R; r++)
            {
                for (int c = 0; c < C; c++)
                {
                    var rgb = oDataSource[r, c];
                    if (rgb < 0)
                        continue;
                    oHist.Add(rgb, 1);
                }
            }
            return true;
        } 

        static bool CreateColorHist(int[,] oDataSource, ColorHistogram oHist)
        {
            string sMethod = nameof(CreateColorHist);
            try
            {
                if (oDataSource == null || oHist == null)
                {
                    Trace.TraceError($"{sClass}.{sMethod} : Invalid data source");
                    return false;
                }
                oHist.Reset();
                int R = oDataSource.GetLength(0);
                int C = oDataSource.GetLength(1);
                if (R * C > 64 * 64)
                {
                    if (ColorDefaults.Trace)
                        Trace.TraceInformation($"{sClass}.{sMethod} : call CreateColorHistArray");
                    CreateColorHistArray(oDataSource, oHist);
                }
                else
                {
                    if (ColorDefaults.Trace)
                        Trace.TraceInformation($"{sClass}.{sMethod} : creating HashColorHistogram");
                    CreateColorHistDirect(oDataSource, oHist);
                }
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"{sClass}.{sMethod} : {ex.Message}");
                return false;
            }
        }

    }
}
