using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformFast : ColorTransformBase
    {

        public ColorTransformFast()
        {
            Type = ColorTransformType.ColorReductionFast;
            Description = "Quantitative color reduction";
        }

        public int MaxColorWanted 
        { 
            get => config.MaxColorsWanted;
            set => config.MaxColorsWanted = value;
        }
            
        public ColorTransformFast WithProcessingParams(int maxColors)
        {
             MaxColorWanted = maxColors;
            return this;
        }
        public ColorTransformFast WithProcessingParams(ColorTransformConfig cfg) => WithProcessingParams(cfg.MaxColorsWanted);



        protected override ColorTransformResult CreateTransformationMap(CancellationToken token = default)
        {
            //outputPalette = new Palette();
            TransformationMap.Reset();
            var sourceHistogram = new HistogramRGB().Create(ImageSource);
            var tempHist = sourceHistogram.SortColorsDescending();
            var tempPalette = Palette.MergePalette(ReferencePalette, tempHist.ToPalette());
            if (tempPalette.Count <= MaxColorWanted)
            {
                foreach (var kvp in sourceHistogram.HistogramDictionary)
                {
                    //outputPalette.Add(kvp.Key);
                    TransformationMap.Add(kvp.Key, kvp.Key);
                }
            }
            else
            {
                var listAll = tempPalette.ToList();
                var listMax = listAll.Take(MaxColorWanted).ToList();
                var palette = new Palette().Create(listMax);
                listAll.ForEach(rgbItem =>
                {
                    token.ThrowIfCancellationRequested();
                    // From list of ColorsMaxWanted element get the best color approssimation
                    var rgbBest = ColorIntExt.GetNearestColor(rgbItem, palette, ColorDistanceEvaluationMode);
                    //var dErrorMin = listMax.Min(rgbMax => rgbMax.Distance(rgbItem, ColorDistanceEvaluationMode));
                    //var rgbBest = listMax.FirstOrDefault(rgbMax => rgbMax.Distance(rgbItem, ColorDistanceEvaluationMode) == dErrorMin);
                    //outputPalette.Add(rgbBest);
                    TransformationMap.RgbTransformationMap[rgbItem] = rgbBest;
                });
            }
            return ColorTransformResult.CreateValidResult();
        }

        protected override ColorTransformResult ExecuteTransform(CancellationToken token)
        {
            var retData = TransformationMap.Transform(ImageSource, token);
            return ColorTransformResult.CreateValidResult(ImageSource, retData);
        }
    }
}