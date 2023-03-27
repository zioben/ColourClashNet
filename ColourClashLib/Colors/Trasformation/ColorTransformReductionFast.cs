using ColourClashLib.Color;
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
            type = ColorTransform.ColorReductionFast;
            description = "Quantitative color reduction";
        }

        public int ColorsMax { get; set; } = -1;

        protected override void CreateTrasformationMap()
        {
            colorPalette.Reset();
            colorHistogram.SortColorsDescending();
            var oTempPalette = ColorPalette.MergeColorPalette(FixedColorPalette, colorHistogram.ToColorPalette());
            if (oTempPalette.Colors < ColorsMax)
            {
                foreach (var kvp in colorHistogram.rgbHistogram )
                {
                    colorPalette.Add(kvp.Key);
                    colorTransformationMap.rgbTransformationMap[kvp.Key] = kvp.Key;
                }
                return;
            }
            //var listAll = colorHistogram.ToColorPalette().ToList();
            //var listMax = oTempPalette.Take(ColorsMax).ToList();
            var listAll = oTempPalette.ToList();
            var listMax = listAll.Take(ColorsMax).ToList();
            listAll.ForEach(X =>
            {
                var dMin = listMax.Min(Y => Y.Distance(X, ColorDistanceEvaluationMode));
                var oItem = listMax.FirstOrDefault(Y => Y.Distance(X, ColorDistanceEvaluationMode) == dMin);
                colorPalette.Add(oItem);
                colorTransformationMap.rgbTransformationMap[X] = oItem;
            });
        }


    }
}