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
    /// RGB color histogram: maps each color to the number of pixels that use it.
    /// <para/>
    /// Only real colors are counted; tagged values (masks, tiles, invalid) are ignored.
    /// </summary>
    public partial class HistogramRGB
    {
        static string sClass = nameof(HistogramRGB);

        /// <summary>
        /// Dictionary that contains RGB color as key and its occurrences as value.
        /// </summary>
        public Dictionary<int, int> HistogramDictionary { get; private init; } = new Dictionary<int, int>();

        /// <summary>
        /// Gets the number of elements in the RGB histogram.
        /// </summary>
        public int Count => HistogramDictionary.Count;

        /// <summary>
        /// Clears the histogram.
        /// </summary>
        public void Reset()
        {
            HistogramDictionary.Clear();
        }


        /// <summary>
        /// Builds the histogram of an image.
        /// </summary>
        /// <param name="image">Source image</param>
        /// <returns>This instance.</returns>
        /// <exception cref="ArgumentNullException">The image is null or invalid.</exception>
        public HistogramRGB Create(ImageData image)
        {
            Reset();
            ImageData.AssertValid(image);   
            return HistogramRGB.CreateHistogramStatic(image.matrix, this);
        }

        /// <summary>
        /// Copies another histogram into this instance.
        /// </summary>
        /// <param name="histogram">Source histogram</param>
        /// <returns>This instance.</returns>
        public HistogramRGB Create(HistogramRGB histogram )
        {
            AssertValid(histogram);
            Reset();
            foreach( var kvp in histogram.HistogramDictionary )
            {
                this.HistogramDictionary[kvp.Key] = kvp.Value;
            }
            return this;
        }

        /// <summary>
        /// Adds <paramref name="count"/> occurrences of a color. Non-colors and non-positive counts are ignored.
        /// </summary>
        /// <param name="rgb">Color to add</param>
        /// <param name="count">Number of occurrences</param>
        public void AddToHistogram(int rgb, int count)
        {
            if (count <= 0)
                return;
            if (rgb.IsColor())
            {
                if( HistogramDictionary.ContainsKey(rgb))
                {
                    HistogramDictionary[rgb] += count;
                }
                else
                {
                    HistogramDictionary[rgb] = count;
                }
            }
        }

        /// <summary>
        /// Adds one occurrence of a color.
        /// </summary>
        /// <param name="rgb">Color to add</param>
        public void AddToHistogram(int rgb)
            => AddToHistogram(rgb, 1);

        /// <summary>
        /// Returns a palette with all the colors of the histogram.
        /// </summary>
        public Palette ToPalette()
        {
            var list = HistogramDictionary.Select(x => x.Key);
            return new Palette().Create(list);  
        }
        
        /// <summary>
        /// Returns a palette with the first <paramref name="maxColorWanted"/> colors of the histogram, in dictionary order.
        /// <para/>
        /// Sort the histogram first (e.g. <see cref="SortColorsDescending"/>) to get the most frequent colors.
        /// </summary>
        /// <param name="maxColorWanted">Maximum number of colors</param>
        public Palette ToPalette(int maxColorWanted)
        {
            var list = HistogramDictionary.Select(x => x.Key).ToList().Take(maxColorWanted);
            return new Palette().Create(list);
        }

        /// <summary>
        /// Returns a new histogram whose entries are inserted from the most to the least frequent color.
        /// </summary>
        public HistogramRGB SortColorsDescending()
        {
            var sorted = HistogramDictionary
                .OrderByDescending(kvp => kvp.Value)
                .ToList();
            var ret = new HistogramRGB();
            foreach (var kvp in sorted)
            {
                ret.AddToHistogram(kvp.Key, kvp.Value);
            }
            return ret;
        }

        /// <summary>
        /// Returns a new histogram whose entries are inserted from the least to the most frequent color.
        /// </summary>
        public HistogramRGB? SortColorsAscending()
        {
            var sorted = HistogramDictionary
                .OrderBy(kvp => kvp.Value)
                .ToList();
            var ret = new HistogramRGB();
            foreach (var kvp in sorted)
            {
                ret.AddToHistogram(kvp.Key, kvp.Value);
            }
            return ret;
        }

        /// <summary>
        /// Returns a short description with the number of colors.
        /// </summary>
        public override string ToString()
            => $"Histogram(Colors: {Count})";
    }
}
