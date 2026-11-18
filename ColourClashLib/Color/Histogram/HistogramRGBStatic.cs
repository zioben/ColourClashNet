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
    public partial class HistogramRGB
    {
        static readonly int[] colorArray = new int[256 * 256 * 256];

        static readonly int sizeLimit = 1920 * 1080;

        static object locker = new object();

        public static void AssertValid(HistogramRGB hitogram)
        {
            if (hitogram == null)
                throw new ArgumentNullException($"{nameof(hitogram)} is null");
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="histogram"></param>
        static void CreateHistogramArray(int[,] matrix, HistogramRGB histogram)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));
            if (histogram == null)
                throw new ArgumentNullException(nameof(histogram));
            lock (locker)
            {
                Array.Clear(colorArray, 0, colorArray.Length);
                histogram.Reset();
                int rows = matrix.GetLength(0);
                int columns = matrix.GetLength(1);
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < columns; c++)
                    {
                        var rgb = matrix[r, c];
                        if (rgb.IsColor())
                        {
                            colorArray[rgb]++;
                        }
                    }
                }
                for (int rgb = 0; rgb < colorArray.Length; rgb++)
                {
                    if (colorArray[rgb] > 0)
                    {
                        histogram.AddToHistogram(rgb, colorArray[rgb]);
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
        static bool CreateHistogramDirect(int[,] matrix, HistogramRGB histogram)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));
            if (histogram == null)
                throw new ArgumentNullException(nameof(histogram));
            histogram.Reset();
            int rows = matrix.GetLength(0);
            int columns = matrix.GetLength(1);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
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
        static HistogramRGB CreateHistogramStatic(int[,] matrix, HistogramRGB histogram)
        {
            string sMethod = nameof(CreateHistogramStatic);
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));
            if (histogram == null)
                throw new ArgumentNullException(nameof(histogram));
            histogram.Reset();
            int rows = matrix.GetLength(0);
            int columns = matrix.GetLength(1);
            if (rows * columns > sizeLimit)
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
    }
}
