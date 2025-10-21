﻿using System;
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
                    }
                    break;
                default:
                    break;
            }
            return this;
        }

//        Palette OutputPalette = new Palette();

        protected override async Task<bool> CreateTrasformationMapAsync(CancellationToken? oToken)
        {
            return await Task.Run(() =>
            {
                //OutputPalette = new Palette();
                TransformationMap.Reset();
                var oTempHist = SourceHistogram.SortColorsDescending();
                var oTempPalette = Palette.MergeColorPalette(FixedPalette, oTempHist.ToColorPalette());
                if (oTempPalette.Count < ColorsMaxWanted)
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
                    var oPalette = Palette.CreateColorPalette(listMax);
                    listAll.ForEach(rgbItem =>
                    {
                        oToken?.ThrowIfCancellationRequested();
                        // From list of ColorsMaxWanted element get the best color approssimation
                        var rgbBest = ColorIntExt.GetNearestColor(rgbItem, oPalette, ColorDistanceEvaluationMode);
                        //var dErrorMin = listMax.Min(rgbMax => rgbMax.Distance(rgbItem, ColorDistanceEvaluationMode));
                        //var rgbBest = listMax.FirstOrDefault(rgbMax => rgbMax.Distance(rgbItem, ColorDistanceEvaluationMode) == dErrorMin);
                        //OutputPalette.Add(rgbBest);
                        TransformationMap.rgbTransformationMap[rgbItem] = rgbBest;
                    });
                }
                return true;
            });
        }

        protected async override Task<ColorTransformResults> ExecuteTransformAsync(CancellationToken? oToken)
        {
            var oRetData = await TransformationMap.TransformAsync(SourceData, oToken);
            return ColorTransformResults.CreateValidResult(SourceData, oRetData);
        }
    }
}