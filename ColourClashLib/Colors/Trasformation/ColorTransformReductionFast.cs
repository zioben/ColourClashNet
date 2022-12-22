using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformReductionFast : ColorTransformBase
    {

        public ColorTransformReductionFast()
        {
            Type = ColorTransform.ColorReductionFast;
            Description = "Quantitative color reduction";
        }

        public int ColorsMax { get; set; } = -1;

        protected override void CreateTrasformationMap()
        {
            ColorPalette.Reset();
            ColorHistogram.SortColorsDescending();
            if (ColorHistogram.rgbHistogram.Count < ColorsMax)
            {
                foreach (var kvp in ColorHistogram.rgbHistogram )
                {
                    ColorPalette.Add(kvp.Key);
                    ColorTransformationMap.rgbTransformationMap[kvp.Key] = kvp.Key;
                }
                return;
            }
            var listAll = ColorHistogram.rgbHistogram.Select(X => X.Key).ToList();
            var listMax = listAll.Take(ColorsMax).ToList();
            listAll.ForEach(X =>
            {
                var dMin = listMax.Min(Y => Y.Distance(X, ColorDistanceEvaluationMode));
                var oItem = listMax.FirstOrDefault(Y => Y.Distance(X, ColorDistanceEvaluationMode) == dMin);
                ColorPalette.Add(oItem);
                ColorTransformationMap.rgbTransformationMap[X] = oItem;
            });
        }


    }
}