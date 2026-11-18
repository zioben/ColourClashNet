using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Fast color reduction: keeps the <see cref="MaxColorWanted"/> most frequent colors (after the reference palette)
    /// and maps every other color to the nearest kept one.
    /// </summary>
    public class ColorTransformFast : ColorTransformBase
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
        /// Maximum number of output colors.
        /// </summary>
        public int MaxColorWanted 
        { 
            get => config.MaxColorsWanted;
            set => config.MaxColorsWanted = value;
        }
            
        /// <summary>
        /// Sets the maximum number of output colors (fluent).
        /// </summary>
        public ColorTransformFast WithProcessingParams(int maxColors)
        {
             MaxColorWanted = maxColors;
            return this;
        }
        /// <summary>
        /// Sets the maximum number of output colors from a configuration (fluent).
        /// </summary>
        public ColorTransformFast WithProcessingParams(ColorTransformConfig cfg) => WithProcessingParams(cfg.MaxColorsWanted);



        /// <summary>
        /// Builds the map from the color histogram.
        /// </summary>
        protected override ColorTransformResult CreateTransformationMap(CancellationToken token = default)
        {
            TransformationMap.Reset();
            var sourceHistogram = new HistogramRGB().Create(ImageSource);
            // Reference palette colors first, then the image colors from the most to the least frequent
            var sortedHistogram = sourceHistogram.SortColorsDescending();
            var candidatePalette = Palette.MergePalette(ReferencePalette, sortedHistogram.ToPalette());
            if (candidatePalette.Count <= MaxColorWanted)
            {
                foreach (var kvp in sourceHistogram.HistogramDictionary)
                {
                    TransformationMap.Add(kvp.Key, kvp.Key);
                }
            }
            else
            {
                var listAll = candidatePalette.ToList();
                var listMax = listAll.Take(MaxColorWanted).ToList();
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

        /// <summary>
        /// Applies the transformation map.
        /// </summary>
        protected override ColorTransformResult ExecuteTransform(CancellationToken token)
        {
            var outputImage = TransformationMap.Transform(ImageSource, token);
            return ColorTransformResult.CreateValidResult(ImageSource, outputImage);
        }
    }
}