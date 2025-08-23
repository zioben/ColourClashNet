using ColourClashLib.Color;
using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ColourClashNet.Colors.Transformation.ColorTransformReductionAmiga;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformReductionFast : ColorTransformBase
    {

        public ColorTransformReductionFast()
        {
            Type = ColorTransformType.ColorReductionFast;
            Description = "Quantitative color reduction";
        }

        public int ColorsMaxWanted { get; set; } = -1;

        public override ColorTransformInterface SetProperty(ColorTransformProperties eProperty, object oValue)
        {
            if (base.SetProperty(eProperty, oValue) != null)
                return this;
            switch (eProperty)
            {
                case ColorTransformProperties.MaxColorsWanted:
                    if (int.TryParse(oValue?.ToString(), out var c))
                    {
                        ColorsMaxWanted = c;
                        return this;
                    }
                    break;
                default:
                    break;
            }
            return null;
        }


        protected override void CreateTrasformationMap()
        {
            OutputPalette.Reset();
            OutputHistogram.SortColorsDescending();
            var oTempPalette = ColorPalette.MergeColorPalette(InputFixedColorPalette, OutputHistogram.ToColorPalette());
            if (oTempPalette.Count < ColorsMaxWanted)
            {
                foreach (var kvp in OutputHistogram.rgbHistogram )
                {
                    OutputPalette.Add(kvp.Key);
                    ColorTransformationMapper.rgbTransformationMap[kvp.Key] = kvp.Key;
                }
                return;
            }
            //var listAll = colorHistogram.ToColorPalette().ToList();
            //var listMax = oTempPalette.Take(ColorsMax).ToList();
            var listAll = oTempPalette.ToList();
            var listMax = listAll.Take(ColorsMaxWanted).ToList();
            listAll.ForEach(X =>
            {
                var dMin = listMax.Min(Y => Y.Distance(X, ColorDistanceEvaluationMode));
                var oItem = listMax.FirstOrDefault(Y => Y.Distance(X, ColorDistanceEvaluationMode) == dMin);
                OutputPalette.Add(oItem);
                ColorTransformationMapper.rgbTransformationMap[X] = oItem;
            });
        }


    }
}