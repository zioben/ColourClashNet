using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Median cut color reduction (Heckbert).
    /// <para/>
    /// The color set is recursively split in two along the channel with the widest range, at the median value,
    /// until the requested number of boxes is reached; each box is then mapped to its representative color.
    /// </summary>
    public class ColorTransformMedianCut : ColorTransformBase
    {

        /// <summary>
        /// Creates the median cut reduction.
        /// </summary>
        public ColorTransformMedianCut()
        {
            Type = ColorTransformType.ColorReductionMedianCut;
            Description = "Median partition color reduction";
        }

        /// <summary>
        /// Maximum number of output colors (rounded to a power of two, max 256).
        /// </summary>
        public int MaxColorsWanted 
        { 
            get => config.MaxColorsWanted;
            set => config.MaxColorsWanted = value;
        }

        /// <summary>
        /// How the representative color of each box is selected.
        /// </summary>
        public ColorSelectionMode ColorSelectionMode
        {
            get => config.MedianCutColorSelectionMode;
            set => config.MedianCutColorSelectionMode = value;
        }


        /// <summary>
        /// Sets the processing parameters (fluent).
        /// </summary>
        public ColorTransformMedianCut WithProcessingParams(int maxColors, ColorSelectionMode colorSelectionMode )
        {
            MaxColorsWanted = maxColors;
            ColorSelectionMode = colorSelectionMode;
            return this;
        }

        /// <summary>
        /// Sets the processing parameters from a configuration (fluent).
        /// </summary>
        public ColorTransformMedianCut WithProcessingParams(ColorTransformConfig cfg) => WithProcessingParams(cfg.MaxColorsWanted, cfg.MedianCutColorSelectionMode );





        /// <summary>
        /// Returns the index of the histogram median (first bin where the cumulative count exceeds half of the total).
        /// </summary>
        int GetMedian(List<int> list)
        {
            float limit = list.Sum() / 2f;
            if (limit <= 0) return 0;

            float sum = 0;
            for (int i = 0; i < list.Count; i++)
            {
                sum += list[i];
                if (sum > limit) return i;
            }
            return list.Count - 1;
        }


        /// <summary>
        /// Returns the distance between the first and the last non-empty bin of a histogram.
        /// </summary>
        int GetRange(List<int> list)
        {
            int first = list.FindIndex(x => x != 0);
            if (first < 0) return 0; // all zero histogram
            int last = list.FindLastIndex(x => x != 0);
            return last - first;
        }

        /// <summary>
        /// Recursively splits <paramref name="palette"/>; when <paramref name="maxColor"/> reaches zero the box is a leaf
        /// and its colors are added to the transformation map.
        /// </summary>
        void Partition(Palette palette, int maxColor)
        {
            var rgbList = palette.ToList();
            if (maxColor > 0)
            {

                var histR = new int[256].ToList();
                var histG = new int[256].ToList();
                var histB = new int[256].ToList();
                foreach (var rgb in rgbList)
                {
                    histR[rgb.ToR()]++;
                    histG[rgb.ToG()]++;
                    histB[rgb.ToB()]++;
                }
                // Split along the channel with the widest range, at its median
                int rangeR = GetRange(histR);
                int rangeG = GetRange(histG);
                int rangeB = GetRange(histB);
                Palette lowerHalf = new Palette();
                Palette upperHalf = new Palette();
                if (rangeR > rangeG && rangeR > rangeB)
                {
                    var medianR = GetMedian(histR);
                    foreach (var rgb in rgbList)
                    {
                        if (rgb.ToR() <= medianR)
                            lowerHalf.Add(rgb);
                        else
                            upperHalf.Add(rgb);
                    }
                }
                else if (rangeG > rangeB)
                {
                    var medianG = GetMedian(histG);
                    foreach (var rgb in rgbList)
                    {
                        if (rgb.ToG() <= medianG)
                            lowerHalf.Add(rgb);
                        else
                            upperHalf.Add(rgb);
                    }
                }
                else
                {
                    var medianB = GetMedian(histB);
                    foreach (var rgb in rgbList)
                    {
                        if (rgb.ToB() <= medianB)
                            lowerHalf.Add(rgb);
                        else
                            upperHalf.Add(rgb);
                    }
                }
                // With ties the median can be the highest value on the channel, leaving one side empty:
                // in that case split the colors sorted on the selected channel by index.
                if ((lowerHalf.Count == 0 || upperHalf.Count == 0) && rgbList.Count > 1)
                {
                    Func<int, int> channel = (rangeR > rangeG && rangeR > rangeB) ? (rgb => rgb.ToR()) : (rangeG > rangeB) ? (rgb => rgb.ToG()) : (rgb => rgb.ToB());
                    var sorted = rgbList.OrderBy(channel).ToList();
                    int half = sorted.Count / 2;
                    lowerHalf = new Palette().Create(sorted.Take(half));
                    upperHalf = new Palette().Create(sorted.Skip(half));
                }
                Partition(lowerHalf, maxColor / 2);
                Partition(upperHalf, maxColor / 2);
                return;
            }
            else
            {
                // Leaf box: all its colors map to the representative color
                if (palette.Count == 0)
                    return;
                var meanColor = ColorIntExt.GetColorMean(palette, ColorSelectionMode);
                foreach (var rgb in rgbList)
                {
                    if (!TransformationMap.RgbTransformationMap.ContainsKey(rgb))
                    {
                        TransformationMap.Add(rgb, meanColor);
                    }
                }
            }
        }

        Palette outputPalette = new Palette();

        /// <summary>
        /// Builds the map: identity when the image already fits, median cut partition otherwise.
        /// </summary>
        protected override ColorTransformResult CreateTransformationMap(CancellationToken token=default)
        {
            outputPalette = new Palette();
            var sourceHistogram = new HistogramRGB().Create(ImageSource);
            if (sourceHistogram.ToPalette().Count <= MaxColorsWanted)
            {
                foreach (var kvp in sourceHistogram.HistogramDictionary)
                {
                    TransformationMap.RgbTransformationMap[kvp.Key] = kvp.Key;
                }
                outputPalette = TransformationMap.GetOutputPalette();
            }
            else
            {
                // Partition(maxColor) produces 2 * maxColor leaf boxes
                int colorsMax = Math.Min(256, Math.Max(2, MaxColorsWanted));
                Partition(sourceHistogram.ToPalette(), colorsMax / 2);
                outputPalette = TransformationMap.GetOutputPalette();
            }
            return ColorTransformResult.CreateValidResult();
        }

        /// <summary>
        /// Applies the transformation map.
        /// </summary>
        protected override ColorTransformResult ExecuteTransform(CancellationToken token)
        {
            var outputImage = TransformationMap.Transform(ImageSource, token);
            if (outputImage != null)
            {
                return ColorTransformResult.CreateValidResult(ImageSource, outputImage);
            }
            return new();
        }
    }
}