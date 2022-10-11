using ColourClashNet.Color;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color
{
    public class ColorTransformReductionFast : ColorTransformBase
    {

        public int MaxColors { get; set; } = -1;
        protected override void BuildTrasformation()
        {
            SortColors();
            if (DictHistogram.Count < MaxColors)
            {
                foreach (var kvp in DictHistogram)
                {
                    DictTransform[kvp.Key] = kvp.Key;
                }
                return;
            }
            List<ColorItem> listAll = DictToList();
            List<ColorItem> listMax = new List<ColorItem>();
            listMax.AddRange(listAll.Take(MaxColors));
            listAll.ForEach(X =>
            {
                var dMin = listMax.Min(Y => Y.DistanceRGB(X));
                var oItem = listMax.FirstOrDefault(Y => Y.DistanceRGB(X) == dMin);
                DictTransform[X] = oItem;
            });
        }
    }
}