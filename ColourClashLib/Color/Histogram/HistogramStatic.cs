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
        /// Creates a histogram using a temporary local array for large images.
        /// This method is thread-safe and avoids using a static shared buffer.
        /// </summary>
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
                        var i = oDataSource[r, c];
                        if (i < 0)
                            continue;
                        oColorArray[i]++;
                    }
                }
                for (int rgb = 0; rgb < oColorArray.Length; rgb++)
                {
                    if (oColorArray[rgb] >= 0)
                    {
                        oHist.AddToHistogram(rgb, oColorArray[rgb]);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a histogram directly for small images.
        /// Adds 1 for each occurrence of a color.
        /// </summary>
        static bool CreateHistogramDirect(int[,] oDataSource, Histogram oHist)
        { 
            if(oDataSource == null || oHist == null )
                return false;
            oHist.Reset();
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

        /// <summary>
        /// Creates a histogram asynchronously, choosing the method based on image size.
        /// Uses a temporary local array for large images.
        /// </summary>
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

      
        public static Histogram CreateHistogram(int[,] oDataSource) => CreateHistogramStatic(oDataSource, new Histogram());

        public static Histogram CreateHistogram(ImageData oImageData) => CreateHistogramStatic(oImageData?.Data, new Histogram());

       

        /// <summary>
        /// Creates a histogram asynchronously, choosing  the method based on image size.
        /// Uses a temporary local array for large images.
        /// </summary>
        public static Histogram CreateHistogram(Histogram oHistSrc)
        {
            string sMethod = nameof(CreateHistogram);

            try
            {
                if (oHistSrc == null)
                {
                    LogMan.Error(sClass, sMethod, "Invalid data source");
                    return new Histogram();
                }
                var oHistDest = new Histogram();
                foreach (var kvp in oHistSrc.rgbHistogram)
                {
                    oHistDest.rgbHistogram.Add(kvp.Key, kvp.Value);
                }
                return oHistDest;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sClass, sMethod, ex);
                return new Histogram();
            }
        }
    }
}
