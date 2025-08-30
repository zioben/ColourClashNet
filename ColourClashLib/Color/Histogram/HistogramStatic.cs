using ColourClashNet.Color;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color
{
    public partial class Histogram
    {
        static int[] oColorArray = new int[256 * 256 * 256];

        static object oLocker = new object();

        static void CreateColorHistArray(int[,] oDataSource, Histogram oHist)
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
                        if (i < 0)
                            continue;
                        oColorArray[i]++;
                    }
                }
                for (int rgb = 0; rgb < oColorArray.Length; rgb++)
                {
                    if (oColorArray[rgb] > 0)
                        oHist.AddToHistogram(rgb, oColorArray[rgb]);
                }
            }
        }
        static bool CreateColorHistDirect(int[,] oDataSource, Histogram oHist)
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
                    oHist.AddToHistogram(rgb, 1);
                }
            }
            return true;
        }

        static Histogram? CreateColorHist(int[,] oDataSource, Histogram oHist)
        {
            string sMethod = nameof(CreateColorHist);
            try
            {
                if (oDataSource == null || oHist == null)
                {
                    LogMan.Error(sClass, sMethod, "Invalid data source");
                    return null;
                }
                oHist.Reset();
                int R = oDataSource.GetLength(0);
                int C = oDataSource.GetLength(1);
                if (R * C > 1920 * 1080)
                {
                    LogMan.Trace(sClass, sMethod, "Using array method for large image");
                    CreateColorHistArray(oDataSource, oHist);
                }
                else
                {
                    LogMan.Trace(sClass, sMethod, "Using direct method for small image");
                    CreateColorHistDirect(oDataSource, oHist);
                }
                return oHist;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sClass, sMethod, ex);
                return null;
            }
        }

        /// <summary>
        /// Create Histogram from a 2D int array of RGB values
        /// </summary>
        /// <param name="oDataSource">Image Array</param>
        /// <returns>ColorHistogram or null on error</returns>
        public static Histogram? CreateColorHistogram(int[,] oDataSource)
        {
            Histogram colorHistogram = new Histogram();
            return CreateColorHist(oDataSource, colorHistogram);    
        }
    }
}
