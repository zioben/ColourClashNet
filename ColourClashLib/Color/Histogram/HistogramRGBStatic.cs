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
        // Shared counting buffer with one slot per RGB888 color (64 MB): allocated once, guarded by locker
        static readonly int[] colorArray = new int[256 * 256 * 256];

        // Pixel count above which the array based counting is faster than the dictionary based one
        static readonly int sizeLimit = 1920 * 1080;

        static object locker = new object();

        /// <summary>
        /// Throws if <paramref name="histogram"/> is null.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AssertValid(HistogramRGB histogram)
        {
            if (histogram == null)
                throw new ArgumentNullException($"{nameof(histogram)} is null");
        }

        /// <summary>
        /// Counts colors using the shared 16M entries array, then copies non-zero entries into the histogram.
        /// <para/>
        /// O(pixels + 16M): efficient for large images, serialized by a lock.
        /// </summary>
        /// <param name="matrix">Image matrix</param>
        /// <param name="histogram">Destination histogram</param>
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
        /// Counts colors directly into the histogram dictionary: efficient for small images.
        /// </summary>
        /// <param name="matrix">Image matrix</param>
        /// <param name="histogram">Destination histogram</param>
        /// <returns>Always true.</returns>
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
        /// Builds the histogram of a matrix, choosing the counting strategy by image size.
        /// </summary>
        /// <param name="matrix">Image matrix</param>
        /// <param name="histogram">Destination histogram (reset first)</param>
        /// <returns><paramref name="histogram"/>.</returns>
        static HistogramRGB CreateHistogramStatic(int[,] matrix, HistogramRGB histogram)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));
            if (histogram == null)
                throw new ArgumentNullException(nameof(histogram));
            histogram.Reset();
            int rows = matrix.GetLength(0);
            int columns = matrix.GetLength(1);
            if (rows * columns > sizeLimit)
            {
                CreateHistogramArray(matrix, histogram);
            }
            else
            {
                CreateHistogramDirect(matrix, histogram);
            }
            return histogram;
        }
    }
}
