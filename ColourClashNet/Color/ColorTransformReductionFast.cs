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
            if (DictColorHistogram.Count < MaxColors)
            {
                foreach (var kvp in DictColorHistogram)
                {
                    DictColorTransformation[kvp.Key] = kvp.Key;
                }
                return;
            }
            List<ColorItem> listAll = DictToList();
            List<ColorItem> listMax = new List<ColorItem>();
            listMax.AddRange(listAll.Take(MaxColors));
            listAll.ForEach(X =>
            {
                var dMin = listMax.Min(Y => Y.Distance(X,ColorDistanceEvaluationMode));
                var oItem = listMax.FirstOrDefault(Y => Y.Distance(X,ColorDistanceEvaluationMode) == dMin);
                DictColorTransformation[X] = oItem;
            });
            ColorsUsed = DictColorTransformation.Select(X => X.Value).ToList().Distinct().ToList().Count;
        }
    }
}