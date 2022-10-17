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

        public int MaxColors { get; set; } = -1;
        protected override void BuildTrasformation()
        {
            SortColorsByHistogram();
            if (oColorHistogram.Count < MaxColors)
            {
                foreach (var kvp in oColorHistogram)
                {
                    oColorTransformation[kvp.Key] = kvp.Value;
                }
                return;
            }
            var listAll = oColorHistogram.Select(X => X.Key).ToList();
            var listMax = listAll.Take(MaxColors).ToList();
            listAll.ForEach(X =>
            {
                var dMin = listMax.Min(Y => Y.Distance(X,ColorDistanceEvaluationMode));
                var oItem = listMax.FirstOrDefault(Y => Y.Distance(X,ColorDistanceEvaluationMode) == dMin);
                oColorTransformation[X]= oItem; 
            });
            ColorsUsed = oColorTransformation.Select(X => X.Value).ToList().Distinct().ToList().Count;
        }
    }
}