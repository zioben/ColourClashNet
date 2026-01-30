using ColourClashNet.Color;
using ColourClashNet.Imaging;
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
        public bool IsValid => Count > 0;    

        /// <summary>
        /// Reset the histogram
        /// </summary>
        public void Reset()
        {
            rgbHistogram.Clear();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public Histogram Create(ImageData image)
        {
            Reset();
            if( image == null) 
                throw new ArgumentNullException(nameof(image));
            return Histogram.CreateHistogramStatic(image.matrix, this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="histogram"></param>
        /// <returns></returns>
        public Histogram Create(Histogram histogram )
        {
            Reset();
            if( histogram == null)
                throw new ArgumentNullException( nameof(histogram));
            foreach( var kvp in histogram.rgbHistogram )
            {
                rgbHistogram[kvp.Key] = kvp.Value;
            }
            return this;
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="rgb"></param>
       /// <param name="count"></param>
        public void AddToHistogram(int rgb, int count)
        {
            if (count <= 0)
                return;
            if (rgb.IsColor())
            {
                if( rgbHistogram.ContainsKey(rgb))
                {
                    rgbHistogram[rgb] += count;
                }
                else
                {
                    rgbHistogram[rgb] = count;
                }
            }
        }

       /// <summary>
       /// Increments the count for the specified RGB color value in the histogram by one.
       /// </summary>
       /// <param name="rgb">The RGB color value to add to the histogram. The value should be a 32-bit integer representing an ARGB or RGB
       /// color, depending on the histogram's expected format.</param>
        public void AddToHistogram(int rgb)
            => AddToHistogram(rgb, 1);

       /// <summary>
       /// Creates a new Palette containing all valid RGB values present in the histogram.
       /// </summary>
       /// <returns>A Palette instance populated with all non-negative RGB values from the histogram. The Palette will be empty
       /// if no valid RGB values are found.</returns>
        public Palette ToPalette()
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
       /// Creates a new palette containing up to the specified maximum number of unique colors from the current
       /// histogram.
       /// </summary>
       /// <param name="maxColorWanted">The maximum number of colors to include in the resulting palette. Must be greater than or equal to zero.</param>
       /// <returns>A new Palette instance containing up to maxColorWanted unique colors. If fewer colors are available, the
       /// palette will contain all available colors.</returns>
        public Palette ToPalette(int maxColorWanted)
        {
            var oCP = new Palette();
            foreach (var rgb in rgbHistogram.Keys)
            {
                if (rgb >= 0)
                {
                    oCP.Add(rgb);
                }
                if (oCP.Count >= maxColorWanted)
                {
                    return oCP;
                }
            }
            return oCP;
        }

       /// <summary>
       /// Returns a new histogram with colors sorted in descending order by their frequency.
       /// </summary>
       /// <remarks>The returned histogram is independent of the original and modifications to it do not
       /// affect the source histogram.</remarks>
       /// <returns>A new <see cref="Histogram"/> instance containing the same color-frequency pairs as the original, ordered
       /// from most to least frequent.</returns>
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
       /// Returns a new histogram with its colors sorted in ascending order by their frequency.
       /// </summary>
       /// <returns>A new <see cref="Histogram"/> instance containing the same color-frequency pairs as the original, ordered
       /// from least to most frequent. Returns <see langword="null"/> if the histogram is empty.</returns>
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

        /// <summary>
        /// Returns a string that represents the current histogram, including the count of items.
        /// </summary>
        /// <returns>A string representation of the histogram in the format "Histogram(Count={Count})".</returns>
        public override string ToString()
            => $"Histogram(Colors: {Count})";
    }
}
