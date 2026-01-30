using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformReductionFast : ColorTransformBase
    {

        public ColorTransformReductionFast()
        {
            Type = ColorTransformType.ColorReductionFast;
            Description = "Quantitative color reduction";
        }

        public int ColorsMaxWanted { get; set; } = -1;

        internal protected override ColorTransformInterface SetProperty(ColorTransformProperties propertyName, object value)
        {
            base.SetProperty(propertyName, value);

            switch (propertyName)
            {
                case ColorTransformProperties.MaxColorsWanted:
                        ColorsMaxWanted = ToInt(value);
                    break;
                default:
                    break;
            }
            return this;
        }

        //        Palette OutputPalette = new Palette();

        protected override ColorTransformResult CreateTransformationMap(CancellationToken oToken = default)
        {
            //OutputPalette = new Palette();
            TransformationMap.Reset();
            var SourceHistogram = new Histogram().Create(SourceData);
            var oTempHist = SourceHistogram.SortColorsDescending();
            var oTempPalette = Palette.MergePalette(FixedPalette, oTempHist.ToPalette());
            if (oTempPalette.Count <= ColorsMaxWanted)
            {
                foreach (var kvp in SourceHistogram.rgbHistogram)
                {
                    //OutputPalette.Add(kvp.Key);
                    TransformationMap.Add(kvp.Key, kvp.Key);
                }
            }
            else
            {
                var listAll = oTempPalette.ToList();
                var listMax = listAll.Take(ColorsMaxWanted).ToList();
                var oPalette = new Palette().Create(listMax);
                listAll.ForEach(rgbItem =>
                {
                    oToken.ThrowIfCancellationRequested();
                    // From list of ColorsMaxWanted element get the best color approssimation
                    var rgbBest = ColorIntExt.GetNearestColor(rgbItem, oPalette, ColorDistanceEvaluationMode);
                    //var dErrorMin = listMax.Min(rgbMax => rgbMax.Distance(rgbItem, ColorDistanceEvaluationMode));
                    //var rgbBest = listMax.FirstOrDefault(rgbMax => rgbMax.Distance(rgbItem, ColorDistanceEvaluationMode) == dErrorMin);
                    //OutputPalette.Add(rgbBest);
                    TransformationMap.rgbTransformationMap[rgbItem] = rgbBest;
                });
            }
            return ColorTransformResult.CreateValidResult();
        }

        protected override ColorTransformResult ExecuteTransform(CancellationToken oToken)
        {
            var oRetData = TransformationMap.Transform(SourceData, oToken);
            return ColorTransformResult.CreateValidResult(SourceData, oRetData);
        }
    }
}