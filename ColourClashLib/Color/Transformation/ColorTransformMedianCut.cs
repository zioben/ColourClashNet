using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformMedianCut : ColorTransformBase
    {

        public ColorTransformMedianCut()
        {
            Type = ColorTransformType.ColorReductionMedianCut;
            Description = "Median partition color reduction";
        }

        public int MaxColorsWanted 
        { 
            get => config.MaxColorsWanted;
            set => config.MaxColorsWanted = value;
        }

        public ColorSelectionMode ColorSelectionMode
        {
            get => config.MedianCutColorSelectionMode;
            set => config.MedianCutColorSelectionMode = value;
        }


        public ColorTransformMedianCut WithProcessingParams(int maxColors, ColorSelectionMode colorSelectionMode )
        {
            MaxColorsWanted = maxColors;
            ColorSelectionMode = colorSelectionMode;
            return this;
        }

        public ColorTransformMedianCut WithProcessingParams(ColorTransformConfig cfg) => WithProcessingParams(cfg.MaxColorsWanted, cfg.MedianCutColorSelectionMode );





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


        int GetRange(List<int> list)
        {
            int first = list.FindIndex(x => x != 0);
            if (first < 0) return 0; // lista tutta a zero
            int last = list.FindLastIndex(x => x != 0);
            return last - first;
        }

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
                int ird = GetRange(histR);
                int igd = GetRange(histG);
                int ibd = GetRange(histB);
                Palette lowerHalf = new Palette();
                Palette upperHalf = new Palette();
                if (ird > igd && ird > ibd)
                {
                    var irm = GetMedian(histR);
                    foreach (var rgb in rgbList)
                    {
                        if (rgb.ToR() <= irm)
                            lowerHalf.Add(rgb);
                        else
                            upperHalf.Add(rgb);
                    }
                }
                else if (igd > ibd)
                {
                    var igm = GetMedian(histG);
                    foreach (var rgb in rgbList)
                    {
                        if (rgb.ToG() <= igm)
                            lowerHalf.Add(rgb);
                        else
                            upperHalf.Add(rgb);
                    }
                }
                else
                {
                    var ibm = GetMedian(histB);
                    foreach (var rgb in rgbList)
                    {
                        if (rgb.ToB() <= ibm)
                            lowerHalf.Add(rgb);
                        else
                            upperHalf.Add(rgb);
                    }
                }
                // With ties the median can be the highest value on the channel, leaving one side empty:
                // in that case split the colors sorted on the selected channel by index.
                if ((lowerHalf.Count == 0 || upperHalf.Count == 0) && rgbList.Count > 1)
                {
                    Func<int, int> channel = (ird > igd && ird > ibd) ? (rgb => rgb.ToR()) : (igd > ibd) ? (rgb => rgb.ToG()) : (rgb => rgb.ToB());
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
                int colorsMax = Math.Min(256, Math.Max(2, MaxColorsWanted));
                Partition(sourceHistogram.ToPalette(), colorsMax / 2);
                outputPalette = TransformationMap.GetOutputPalette();
            }
            return ColorTransformResult.CreateValidResult();
        }

        protected override ColorTransformResult ExecuteTransform(CancellationToken token)
        {
            var ret = TransformationMap.Transform(ImageSource, token);
            if (ret != null)
            {
                return ColorTransformResult.CreateValidResult(ImageSource, ret);
            }
            return new();
        }
    }
}