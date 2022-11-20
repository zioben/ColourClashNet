using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
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
            SortColorsByHistogram();
            if (oColorHistogram.Count < ColorsMax)
            {
                foreach (var kvp in oColorHistogram)
                {
                    hashColorsPalette.Add(kvp.Key);
                    oColorTransformationMap[kvp.Key] = kvp.Key;
                }
                return;
            }
            var listAll = oColorHistogram.Select(X => X.Key).ToList();
            var listMax = listAll.Take(ColorsMax).ToList();
            listAll.ForEach(X =>
            {
                var dMin = listMax.Min(Y => Y.Distance(X,ColorDistanceEvaluationMode));
                var oItem = listMax.FirstOrDefault(Y => Y.Distance(X,ColorDistanceEvaluationMode) == dMin);
                hashColorsPalette.Add(oItem);
                oColorTransformationMap[X]= oItem; 
            });
        }

   
    }
}