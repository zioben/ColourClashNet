﻿using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorTransformReductionCluster : ColorTransformBase
    {
        public int MaxColors { get; set; } = -1;
        public int TrainingLoop { get; set; } = -1;
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
            // Init Set
            var lColors = oColorHistogram.Select(X=>X).ToList();

            var lMean = lColors.Take(MaxColors).ToList();
            List<List<int>> llList = new List<List<int>>();
            for (int i = 0; i < MaxColors; i++)
            {
                llList.Add(new List<int>());
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
                    var oCluster = lMean.FirstOrDefault(X => X.Value.Distance(item.Value, ColorDistanceEvaluationMode) == lMean.Min(Y => Y.Value.Distance(item.Value, ColorDistanceEvaluationMode)));
                    var iIndex = lMean.IndexOf(oCluster);
                    llList[iIndex].Add(item.Value);
                });
                // Evaluate cluster Mean
                {
                    lMean.Clear();
                    llList.ForEach(XX =>
                    {
                        //int Count = 0;
                        //double R = 0;
                        //double G = 0;
                        //double B = 0;
                        //XX.ForEach(Y =>
                        //{
                        //    var Elements = DictColorHistogram[Y];
                        //    Count += Elements;
                        //    R += Elements * Y.R;
                        //    G += Elements * Y.G;
                        //    B += Elements * Y.B;
                        //});
                        //R = Count > 0 ? R / Count : -1;
                        //G = Count > 0 ? G / Count : -1;
                        //B = Count > 0 ? B / Count : -1;
                        //lMean.Add(new ColorItem((int)R, (int)G, (int)B));
                        //lMean.Add(0);
                    });
                }
            }

            lColors.ForEach(X =>
            {                
                var dMin = lMean.Min(Y => Y.Value.Distance(X.Value, ColorDistanceEvaluationMode));
                var oItem = lMean.FirstOrDefault(Y => Y.Value.Distance(X.Value, ColorDistanceEvaluationMode) == dMin);
                oColorTransformation[X.Key] = oItem.Value;
            });
            ColorsUsed = oColorTransformation.Select(X => X.Value).ToList().Distinct().ToList().Count;
        }
    }
}
