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
        static readonly int[] oColorArray = new int[256 * 256 * 256];

        static readonly int SizeLimit = 1920 * 1080 * 4;

        static object oLocker = new object();

        /// <summary>
        /// Creates a histogram using a temporary local array for large images.
        /// This method is thread-safe and avoids using a static shared buffer.
        /// </summary>
        static void CreateColorHistArray(int[,] oDataSource, Histogram oHist, CancellationToken? oToken)
        {
            lock (oLocker)
            {
                Array.Clear(oColorArray, 0, oColorArray.Length);
                int R = oDataSource.GetLength(0);
                int C = oDataSource.GetLength(1);
                for (int r = 0; r < R; r++)
                {
                    oToken?.ThrowIfCancellationRequested();
                    for (int c = 0; c < C; c++)
                    {
                        var i = oDataSource[r, c];
                        if (i < 0)
                            continue;
                        oColorArray[i]++;
                    }
                }
                oToken?.ThrowIfCancellationRequested();
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
        /// Creates a histogram directly for small images.
        /// Adds 1 for each occurrence of a color.
        /// </summary>
        static bool CreateColorHistDirect(int[,]? oDataSource, Histogram? oHist, CancellationToken? oToken)
        { 
            if(oDataSource == null || oHist == null )
                return false;
            int R = oDataSource.GetLength(0);
            int C = oDataSource.GetLength(1);
            for (int r = 0; r < R; r++)
            {
                oToken?.ThrowIfCancellationRequested();
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
        static async Task<Histogram> CreateColorHistogramAsync(int[,]? oDataSource, Histogram oHist, CancellationToken? oToken)
        {
            string sMethod = nameof(CreateColorHistogramAsync);
            return await Task.Run(() =>
            {
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
                        CreateColorHistArray(oDataSource, oHist,oToken);
                    }
                    else
                    {
                        LogMan.Trace(sClass, sMethod, "Using direct method for small image");
                        CreateColorHistDirect(oDataSource, oHist, oToken);
                    }
                    return oHist;
                }
                catch (Exception ex)
                {
                    LogMan.Exception(sClass, sMethod, ex);
                    return new Histogram();
                }
            });
        }

        /// <summary>
        /// Simplified overload: creates a new histogram from a 2D int array asynchronously.
        /// </summary>
        public async static Task<Histogram> CreateColorHistogramAsync(int[,]? oDataSource, CancellationToken? oToken) => await CreateColorHistogramAsync(oDataSource, new Histogram(), oToken);

        ///// <summary>
        ///// Simplified overload: creates a new histogram from a 2D int array asynchronously.
        ///// </summary>
        //public static Histogram CreateColorHistogram(int[,]? oDataSource)
        //{
        //    var cts = new CancellationTokenSource();   
        //    return CreateColorHistogramAsync(oDataSource, cts.Token).GetAwaiter().GetResult();    
        //}

        /// <summary>
        /// Creates a histogram asynchronously, choosing  the method based on image size.
        /// Uses a temporary local array for large images.
        /// </summary>
        static async Task<Histogram> CreateColorHistogramAsync(Histogram? oHistSrc, Histogram? oHistDest, CancellationToken? oToken)
        {
            string sMethod = nameof(CreateColorHistogramAsync);
            return await Task.Run(() =>
            {
                try
                {
                    var oHistDest = new Histogram();
                    if (oHistSrc == null || oHistDest == null)
                    {
                        LogMan.Error(sClass, sMethod, "Invalid data source");
                        return new Histogram();
                    }
                    oHistDest.Reset();
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
            });
        }
    }
}
