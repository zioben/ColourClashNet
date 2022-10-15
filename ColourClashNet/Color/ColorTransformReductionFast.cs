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
            if (ListColorHistogram.Count < MaxColors)
            {
                foreach (var kvp in ListColorHistogram)
                {
                    ListColorTransformation[kvp.Key] = kvp.Key;
                }
                return;
            }
            List<int> listMax = new List<int>();
/*            listMax.AddRange(this.Lis.Take(MaxColors));
            listAll.ForEach(X =>
            {
                var dMin = listMax.Min(Y => Y.Distance(X,ColorDistanceEvaluationMode));
                var oItem = listMax.FirstOrDefault(Y => Y.Distance(X,ColorDistanceEvaluationMode) == dMin);
                DictColorTransformation[X] = oItem;
            });
            ColorsUsed = DictColorTransformation.Select(X => X.Value).ToList().Distinct().ToList().Count;*/
        }
    }
}