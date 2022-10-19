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
                    oColorsPalette.Add(kvp.Key);
                    oColorTransformation[kvp.Key] = kvp.Key;
                }
                return;
            }
            var listAll = oColorHistogram.Select(X => X.Key).ToList();
            var listMax = listAll.Take(MaxColors).ToList();
            listAll.ForEach(X =>
            {
                var dMin = listMax.Min(Y => Y.Distance(X,ColorDistanceEvaluationMode));
                var oItem = listMax.FirstOrDefault(Y => Y.Distance(X,ColorDistanceEvaluationMode) == dMin);
                oColorsPalette.Add(oItem);
                oColorTransformation[X]= oItem; 
            });
        }

        public override int[,] Transform(int[,] oDataSource)
        {
            return base.TransformBase(oDataSource);
        }
    }
}