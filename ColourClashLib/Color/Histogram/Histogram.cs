using ColourClashNet.Color;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color
{
    /// <summary>
    /// Color Histogram evaluation.<BR/>
    /// Represent Histogram RGB Color with its occurrences.
    /// </summary>
    public partial class Histogram
    {
        static string sClass = nameof(Histogram);

        /// <summary>
        /// Dictionary that contains RGB color as key and its occurrences as value.
        /// </summary>
        public Dictionary<int, int> rgbHistogram { get; private init; } = new Dictionary<int, int>();

        /// <summary>
        /// Gets the number of elements in the RGB histogram.
        /// </summary>
        public int Count => rgbHistogram.Count;

        /// <summary>
        /// 
        /// </summary>
        public bool Valid => Count > 0;    

        /// <summary>
        /// Reset the histogram
        /// </summary>
        public void Reset()
        {
            rgbHistogram.Clear();
        }

        /// <summary>
        /// Create Histogram from a 2D int array of RGB values
        /// </summary>
        /// <param name="oDataSource">Image Data</param>
        /// <returns>this object</returns>
        public async Task<Histogram> CreateAsync(int[,] oDataSource, CancellationToken? oToken)
        {
            Reset();
            return await Histogram.CreateColorHistogramAsync(oDataSource, this, oToken);
        }

        /// <summary>
        /// Create Histogram from a 2D int array of RGB values
        /// </summary>
        /// <param name="oDataSource">Image Data</param>
        /// <returns>this object</returns>
        public async Task<Histogram> CreateAsync(Histogram? oHist, CancellationToken? oToken)
        {
            Reset();
            return await Histogram.CreateColorHistogramAsync(oHist, this, oToken);
        }

        /// <summary>
        /// Add occurrences to a RGB color in the histogram.<BR/>
        /// only 'ColorIntInfo.IsColor' will be accepted.
        /// </summary>
        /// <param name="rgb">color data</param>
        /// <param name="count">number of occourrences ot color data</param>
        public void AddToHistogram(int rgb, int value)
        {
            if (rgb < 0)
                return;
            if (rgb.GetColorInfo() == ColorIntType.IsColor)
            {
                if( rgbHistogram.ContainsKey(rgb))
                {
                    rgbHistogram[rgb] += value;
                }
                else
                {
                    rgbHistogram[rgb] = value;
                }
            }
        }

        /// <summary>
        /// Extract colors in the histogram as a Color Palette
        /// </summary>
        /// <returns></returns>
        public Palette ToColorPalette()
        {
            var oCP = new Palette();
            foreach (var rgb in rgbHistogram.Keys)
            {
                if (rgb >= 0)
                {
                    oCP.Add(rgb);
                }
            }
            return oCP; 
        }
        
        /// <summary>
        /// Extract colors in the histogram as a Color Palette
        /// </summary>
        /// <returns></returns>
        public Palette ToColorPalette(int iMaxColors)
        {
            var oCP = new Palette();
            foreach (var rgb in rgbHistogram.Keys)
            {
                if (rgb >= 0)
                {
                    oCP.Add(rgb);
                }
                if (oCP.Count >= iMaxColors)
                {
                    return oCP;
                }
            }
            return oCP;
        }

        /// <summary>
        /// Sort colors in the histogram by occurrences in descending order 
        /// </summary>
        /// <returns>new Histogram</returns>
        public Histogram SortColorsDescending()
        {
            var lSorted = rgbHistogram
                .OrderByDescending(kvp => kvp.Value)
                .ToList();
            var oRet = new Histogram();
            foreach (var kvp in lSorted)
            {
                oRet.AddToHistogram(kvp.Key, kvp.Value);
            }
            return oRet;
        }

        /// <summary>
        /// Sort colors in the histogram by occurrences in ascending order
        /// </summary>
        /// <returns>this object</returns>
        public Histogram? SortColorsAscending()
        {
            var lSorted = rgbHistogram
                .OrderBy(kvp => kvp.Value)
                .ToList();
            var oRet = new Histogram();
            foreach (var kvp in lSorted)
            {
                oRet.AddToHistogram(kvp.Key, kvp.Value);
            }
            return oRet;
        }
    }


}
