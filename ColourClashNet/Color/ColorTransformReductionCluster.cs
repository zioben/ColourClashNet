using ColourClashNet.Color;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color
{
    public class ColorTransformReductionCluster : ColorTransformBase
    {

        public int MaxColors { get; set; } = -1;
        public int TrainingLoop { get; set; } = -1;
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
            // Init Set
            var lColors = DictToList();

            var lMean = lColors.Take(MaxColors).ToList();
            List<List<ColorItem>> llList = new List<List<ColorItem>>();
            for (int i = 0; i < MaxColors; i++)
            {
                llList.Add(new List<ColorItem>());
            }
            // Init Clustering
            // Dictionary<ColorItem,List<ColorItem>> lKMeans = new Dictionary<ColorItem, List<ColorItem>>();

            for (int train = 0; train < TrainingLoop; train++)
            {
                // Reset Set
                llList.ForEach(X => X.Clear());
                // Aggregate
                lColors.ForEach(item =>
                {
                    var oCluster = lMean.FirstOrDefault(X => X.DistanceHSV(item) == lMean.Min(Y => Y.DistanceHSV(item)));
                    var iIndex = lMean.IndexOf(oCluster);
                    llList[iIndex].Add(item);
                });
                // Evaluate cluster Mean
                {
                    lMean.Clear();
                    llList.ForEach(XX =>
                    {
                        int Count = 0;
                        double R = 0;
                        double G = 0;
                        double B = 0;
                        XX.ForEach(Y =>
                        {
                            var Elements = DictHistogram[Y];
                            Count += Elements;
                            R += Elements * Y.R;
                            G += Elements * Y.G;
                            B += Elements * Y.B;
                        });
                        R = Count > 0 ? R / Count : -1;
                        G = Count > 0 ? G / Count : -1;
                        B = Count > 0 ? B / Count : -1;
                        lMean.Add(new ColorItem((int)R, (int)G, (int)B));
                    });
                }
            }

            lColors.ForEach(X =>
            {
                var dMin = lMean.Min(Y => Y.DistanceHSV(X));
                var oItem = lMean.FirstOrDefault(Y => Y.DistanceHSV(X) == dMin);
                DictTransform[X] = oItem;
            });
        }
    }
}
