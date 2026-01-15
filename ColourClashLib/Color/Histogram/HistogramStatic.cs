using ColourClashNet.Color;
using ColourClashNet.Imaging;
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
        static readonly int[] oColorArray = new int[256 * 256 * 256];

        static readonly int SizeLimit = 1920 * 1080;

        static object oLocker = new object();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oDataSource"></param>
        /// <param name="oHist"></param>
        static void CreateHistogramArray(int[,] oDataSource, Histogram oHist)
        {
            lock (oLocker)
            {
                Array.Clear(oColorArray, 0, oColorArray.Length);
                oHist.Reset();
                int R = oDataSource.GetLength(0);
                int C = oDataSource.GetLength(1);
                for (int r = 0; r < R; r++)
                {
                    for (int c = 0; c < C; c++)
                    {
                        var rgb = oDataSource[r, c];
                        if (rgb.IsColor())
                        {
                            oColorArray[rgb]++;
                        }
                    }
                }
                for (int rgb = 0; rgb < oColorArray.Length; rgb++)
                {
                    if (oColorArray[rgb] > 0)
                    {
                        oHist.AddToHistogram(rgb, oColorArray[rgb]);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oDataSource"></param>
        /// <param name="oHist"></param>
        /// <returns></returns>
        static bool CreateHistogramDirect(int[,] oDataSource, Histogram oHist)
        {
            if (oDataSource == null || oHist == null)
                return false;
            oHist.Reset();
            int R = oDataSource.GetLength(0);
            int C = oDataSource.GetLength(1);
            for (int r = 0; r < R; r++)
            {
                for (int c = 0; c < C; c++)
                {
                    var rgb = oDataSource[r, c];
                    oHist.AddToHistogram(rgb, 1);
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oDataSource"></param>
        /// <param name="oHist"></param>
        /// <returns></returns>
        static Histogram CreateHistogramStatic(int[,] oDataSource, Histogram oHist)
        {
            string sMethod = nameof(CreateHistogramStatic);
            try
            {
                if (oDataSource == null || oHist == null)
                {
                    LogMan.Error(sClass, sMethod, "Invalid data source");
                    return new Histogram();
                }
                oHist.Reset();
                int R = oDataSource.GetLength(0);
                int C = oDataSource.GetLength(1);
                if (R * C > SizeLimit)
                {
                    LogMan.Trace(sClass, sMethod, "Using array method for large image");
                    CreateHistogramArray(oDataSource, oHist);
                }
                else
                {
                    LogMan.Trace(sClass, sMethod, "Using direct method for small image");
                    CreateHistogramDirect(oDataSource, oHist);
                }
                return oHist;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sClass, sMethod, ex);
                return new Histogram();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oDataSource"></param>
        /// <returns></returns>
        public static Histogram CreateHistogram(int[,] oDataSource)
          => CreateHistogramStatic(oDataSource, new Histogram());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oImageData"></param>
        /// <returns></returns>
        public static Histogram CreateHistogram(ImageData oImageData) 
            => CreateHistogramStatic(oImageData?.Data, new Histogram());


        /// <summary>
        /// 
        /// </summary>
        /// <param name="oHistSrc"></param>
        /// <returns></returns>
        public static Histogram CreateHistogram(Histogram oHistSrc)
            => new Histogram().Create(oHistSrc);
    }
}
