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
        /// <param name="matrix"></param>
        /// <param name="histogram"></param>
        static void CreateHistogramArray(int[,] matrix, Histogram histogram)
        {
            lock (oLocker)
            {
                Array.Clear(oColorArray, 0, oColorArray.Length);
                histogram.Reset();
                int R = matrix.GetLength(0);
                int C = matrix.GetLength(1);
                for (int r = 0; r < R; r++)
                {
                    for (int c = 0; c < C; c++)
                    {
                        var rgb = matrix[r, c];
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
                        histogram.AddToHistogram(rgb, oColorArray[rgb]);
                    }
                }
            }
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="matrix"></param>
       /// <param name="histogram"></param>
       /// <returns></returns>
        static bool CreateHistogramDirect(int[,] matrix, Histogram histogram)
        {
            if (matrix == null || histogram == null)
                return false;
            histogram.Reset();
            int R = matrix.GetLength(0);
            int C = matrix.GetLength(1);
            for (int r = 0; r < R; r++)
            {
                for (int c = 0; c < C; c++)
                {
                    var rgb = matrix[r, c];
                    histogram.AddToHistogram(rgb, 1);
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="histogram"></param>
        /// <returns></returns>
        static Histogram CreateHistogramStatic(int[,] matrix, Histogram histogram)
        {
            string sMethod = nameof(CreateHistogramStatic);
            try
            {
                if (matrix == null || histogram == null)
                {
                    LogMan.Error(sClass, sMethod, "Invalid data source");
                    return new Histogram();
                }
                histogram.Reset();
                int R = matrix.GetLength(0);
                int C = matrix.GetLength(1);
                if (R * C > SizeLimit)
                {
                    LogMan.Trace(sClass, sMethod, "Using array method for large image");
                    CreateHistogramArray(matrix, histogram);
                }
                else
                {
                    LogMan.Trace(sClass, sMethod, "Using direct method for small image");
                    CreateHistogramDirect(matrix, histogram);
                }
                return histogram;
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
        /// <param name="image"></param>
        /// <returns></returns>
        public static Histogram CreateHistogram(ImageData image) 
            => CreateHistogramStatic(image?.DataX, new Histogram());


        /// <summary>
        /// 
        /// </summary>
        /// <param name="histogram"></param>
        /// <returns></returns>
        public static Histogram CreateHistogram(Histogram histogram)
            => new Histogram().Create(histogram);
    }
}
