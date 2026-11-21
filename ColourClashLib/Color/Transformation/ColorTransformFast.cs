using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Fast color reduction: keeps the <see cref="ColorReductionOptions.MaxColors"/> most frequent colors (after the user palette)
    /// and maps every other color to the nearest kept one.
    /// </summary>
    public class ColorTransformFast : ColorTransformBase<FastOptions>
    {

        /// <summary>
        /// Creates the fast reduction.
        /// </summary>
        public ColorTransformFast()
        {
            Type = ColorTransformType.ColorReductionFast;
            Description = "Quantitative color reduction";
        }

            



        /// <summary>
        /// Builds the map from the color histogram.
        /// </summary>
        protected override ColorTransformResult CreateTransformationMap(CancellationToken token = default)
        {
            TransformationMap.Reset();
            var sourceHistogram = new HistogramRGB().Create(ImageSource);
            // Candidate colors in priority order: user palette first, then the image colors from the most to the least frequent
            var candidates = new List<int>();
            var added = new HashSet<int>();
            foreach (var rgb in UserPalette.Concat(sourceHistogram.HistogramDictionary.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key)))
            {
                if (added.Add(rgb))
                    candidates.Add(rgb);
            }
            if (candidates.Count <= Options.MaxColors)
            {
                foreach (var kvp in sourceHistogram.HistogramDictionary)
                {
                    TransformationMap.Add(kvp.Key, kvp.Key);
                }
            }
            else
            {
                var listAll = candidates;
                var listMax = listAll.Take(Options.MaxColors).ToList();
                var palette = new Palette().Create(listMax);
                listAll.ForEach(rgbItem =>
                {
                    token.ThrowIfCancellationRequested();
                    // Map every color to its nearest color among the most frequent ones
                    var rgbBest = ColorIntExt.GetNearestColor(rgbItem, palette, ColorDistanceEvaluationMode);
                    TransformationMap.RgbTransformationMap[rgbItem] = rgbBest;
                });
            }
            return ColorTransformResult.CreateValidResult();
        }

    }
}